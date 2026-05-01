using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Extracts records of type <typeparamref name="TRecord"/> from a CSV stream
/// using <see href="https://joshclose.github.io/CsvHelper/">CsvHelper</see>.
/// </summary>
/// <typeparam name="TRecord">The type of records to extract. Must be <c>notnull</c>.</typeparam>
/// <example>
/// <code>
/// using var reader = new StreamReader("people.csv");
/// var extractor = new CsvExtractor&lt;Person&gt;(reader);
/// await foreach (var person in extractor.ExtractAsync(cancellationToken))
/// {
///     Console.WriteLine(person.Name);
/// }
/// </code>
/// </example>
public sealed class CsvExtractor<TRecord> : ExtractorBase<TRecord, CsvExtractorProgress>
    where TRecord : notnull
{
    private static readonly string OperationName = $"CSV extraction of {typeof(TRecord).Name}";

    private readonly StreamReader _reader;
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private int _progressTimerWired;

    private long _byteCount;
    private int _currentRowIndex;
    private int _currentRawRowIndex;



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractor{TRecord}"/> class.
    /// </summary>
    /// <param name="streamReader">The <see cref="StreamReader"/> to read CSV data from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="streamReader"/> is <c>null</c>.</exception>
    public CsvExtractor
    (
        StreamReader streamReader
    )
    {
        _reader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
        _logger = NullLogger.Instance;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractor{TRecord}"/> class with diagnostic logging.
    /// </summary>
    /// <param name="streamReader">The <see cref="StreamReader"/> to read CSV data from.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="streamReader"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public CsvExtractor
    (
        StreamReader streamReader,
        ILogger<CsvExtractor<TRecord>> logger
    )
    {
        _reader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractor{TRecord}"/> class with an
    /// injected progress timer for testing.
    /// </summary>
    /// <param name="streamReader">The <see cref="StreamReader"/> to read CSV data from.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal CsvExtractor
    (
        StreamReader streamReader,
        ILogger? logger,
        IProgressTimer timer
    )
    {
        _reader = streamReader ?? throw new ArgumentNullException(nameof(streamReader));
        _logger = logger ?? (ILogger)NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>Gets or sets a value indicating whether comment lines are allowed.</summary>
    public bool AllowComments { get; set; }



    /// <summary>
    /// Gets or sets a callback invoked when the underlying parser detects bad data.
    /// Return <c>true</c> to continue processing; <c>false</c> to stop.
    /// When <c>null</c>, bad data is logged and processing continues.
    /// </summary>
    public Func<CsvBadDataInfo, bool>? BadDataFound { get; set; }



    /// <summary>Gets or sets the character used to mark a comment line.</summary>
    public char Comment { get; set; } = '#';



    /// <summary>Gets or sets the field delimiter. Default is <c>","</c>.</summary>
    public string Delimiter { get; set; } = ",";



    /// <summary>Gets or sets the character used to escape the quote character within a field.</summary>
    public char Escape { get; set; } = '"';



    /// <summary>Gets or sets the encoding used by the underlying parser.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;



    /// <summary>Gets or sets a value indicating whether the CSV has a header record.</summary>
    public bool HasHeaderRecord { get; set; } = true;



    /// <summary>Gets or sets a value indicating whether blank lines are skipped.</summary>
    public bool IgnoreBlankLines { get; set; } = true;



    /// <summary>
    /// Gets or sets the 1-based index of the first line that contains data.
    /// Lines before this index are read and discarded.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">value is less than 1.</exception>
    public int InitialRecordIndex
    {
        get => _initialRecordIndex;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "InitialRecordIndex must be 1 or greater.");
            }

            _initialRecordIndex = value;
        }
    }
    private int _initialRecordIndex = 1;



    /// <summary>
    /// Gets or sets a value indicating whether the underlying stream should be left open
    /// after the parser is disposed.
    /// </summary>
    public bool LeaveOpen { get; set; }



    /// <summary>Gets or sets the quote character used to wrap fields.</summary>
    public char Quote { get; set; } = '"';



    /// <summary>Gets or sets the trimming options applied while reading.</summary>
    public CsvTrimOptions TrimOptions { get; set; } = CsvTrimOptions.None;



    /// <summary>
    /// Gets or sets the number of records to skip before yielding results.
    /// This is an alias for <see cref="ExtractorBase{TSource,TProgress}.SkipItemCount"/>.
    /// </summary>
    public int SkipRecordCount
    {
        get => SkipItemCount;
        set => SkipItemCount = value;
    }



    /// <summary>
    /// Gets or sets the maximum number of records to extract.
    /// This is an alias for <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/>.
    /// </summary>
    public int MaxRecordCount
    {
        get => MaximumItemCount;
        set => MaximumItemCount = value;
    }



    private CsvConfiguration BuildConfiguration()
    {
        var callerBadDataFound = BadDataFound;

        return new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            AllowComments = AllowComments,
            BadDataFound = callerBadDataFound is not null
                ? args => callerBadDataFound(ToCsvBadDataInfo(args))
                : args => CsvLogMessages.BadDataFound(_logger, args.Context.Parser?.RawRow ?? -1, args.Field, args.RawRecord, null),
            Comment = Comment,
            CountBytes = true,
            Delimiter = Delimiter,
            Escape = Escape,
            Encoding = Encoding,
            HasHeaderRecord = HasHeaderRecord,
            IgnoreBlankLines = IgnoreBlankLines,
            Quote = Quote,
            ReadingExceptionOccurred = OnReadingExceptionOccurred,
            TrimOptions = (TrimOptions)(int)TrimOptions,
        };
    }



    private static CsvBadDataInfo ToCsvBadDataInfo(BadDataFoundArgs args) =>
        new
        (
            args.Context.Parser?.RawRow ?? -1,
            args.Field,
            args.RawRecord
        );



    private bool OnReadingExceptionOccurred(ReadingExceptionOccurredArgs args)
    {
        var ctx = args.Exception.Context;
        var columnIndex = ctx?.Reader?.CurrentIndex ?? -1;
        var headerRecord = ctx?.Reader?.HeaderRecord;
        var columnName = headerRecord is not null && columnIndex >= 0 && columnIndex < headerRecord.Length
            ? headerRecord[columnIndex]
            : null;
        var columnValue = ctx?.Reader is not null && columnIndex >= 0
            ? ctx.Reader[columnIndex]
            : null;
        CsvLogMessages.ReadingExceptionOccurred
        (
            _logger,
            ctx?.Parser?.Row ?? -1,
            ctx?.Parser?.RawRow ?? -1,
            columnIndex,
            columnName,
            columnValue,
            ctx?.Parser?.RawRecord ?? string.Empty,
            args.Exception
        );
        return true;
    }



    /// <inheritdoc />
    protected override async IAsyncEnumerable<TRecord> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        CsvLogMessages.StartingOperation(_logger, OperationName, null);

        var configuration = BuildConfiguration();

#pragma warning disable CA2007, MA0004
        using var csvReader = new CsvReader(_reader, configuration, LeaveOpen);
#pragma warning restore CA2007, MA0004

        await PrepareReaderAsync(csvReader).ConfigureAwait(false);

        await foreach (var record in csvReader.GetRecordsAsync<TRecord>(token).WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();
            UpdateRowSnapshot(csvReader);

            if (CurrentItemCount >= MaximumItemCount)
            {
                CsvLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                yield break;
            }

            IncrementCurrentItemCount();
            CsvLogMessages.ExtractedItem(_logger, CurrentItemCount, null);

            yield return record;
        }

        CsvLogMessages.ExtractionCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
    }



    private async Task PrepareReaderAsync(CsvReader csvReader)
    {
        // Skip lines before InitialRecordIndex (1-based).
        while (csvReader.Parser.RawRow < InitialRecordIndex - 1 && await csvReader.ReadAsync().ConfigureAwait(false))
        {
            UpdateRowSnapshot(csvReader);
            CsvLogMessages.IgnoredRow(_logger, csvReader.Parser.RawRow, null);
        }

        // Read the header record if present.
        if (HasHeaderRecord && await csvReader.ReadAsync().ConfigureAwait(false))
        {
            UpdateRowSnapshot(csvReader);
            csvReader.ReadHeader();
            csvReader.ValidateHeader<TRecord>();
        }

        // Honour SkipItemCount.
        while (CurrentSkippedItemCount < SkipItemCount && await csvReader.ReadAsync().ConfigureAwait(false))
        {
            UpdateRowSnapshot(csvReader);
            IncrementCurrentSkippedItemCount();
            CsvLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
        }
    }



    private void UpdateRowSnapshot(CsvReader csvReader)
    {
        _byteCount = csvReader.Context.Parser?.ByteCount ?? 0;
        _currentRowIndex = csvReader.Context.Parser?.Row ?? 0;
        _currentRawRowIndex = csvReader.Context.Parser?.RawRow ?? 0;
    }



    /// <inheritdoc />
    protected override CsvExtractorProgress CreateProgressReport() =>
        new
        (
            CurrentItemCount,
            CurrentSkippedItemCount,
            Volatile.Read(ref _byteCount),
            Volatile.Read(ref _currentRowIndex),
            Volatile.Read(ref _currentRawRowIndex)
        );



    /// <inheritdoc />
    protected override IProgressTimer CreateProgressTimer(IProgress<CsvExtractorProgress> progress)
    {
        if (_progressTimer is not null)
        {
            if (Interlocked.CompareExchange(ref _progressTimerWired, 1, 0) == 0)
            {
                _progressTimer.Elapsed += () => progress.Report(CreateProgressReport());
            }

            return _progressTimer;
        }

        return base.CreateProgressTimer(progress);
    }
}
