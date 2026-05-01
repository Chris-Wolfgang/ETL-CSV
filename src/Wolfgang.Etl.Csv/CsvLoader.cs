using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
/// Loads records of type <typeparamref name="TRecord"/> into a CSV stream
/// using <see href="https://joshclose.github.io/CsvHelper/">CsvHelper</see>.
/// </summary>
/// <typeparam name="TRecord">The type of records to load. Must be <c>notnull</c>.</typeparam>
/// <example>
/// <code>
/// using var writer = new StreamWriter("people.csv");
/// var loader = new CsvLoader&lt;Person&gt;(writer);
/// await loader.LoadAsync(items, cancellationToken);
/// </code>
/// </example>
public sealed class CsvLoader<TRecord> : LoaderBase<TRecord, CsvLoaderProgress>
    where TRecord : notnull
{
    private static readonly string OperationName = $"CSV loading of {typeof(TRecord).Name}";

    private readonly StreamWriter _writer;
    private readonly ILogger _logger;
    private readonly IProgressTimer? _progressTimer;
    private int _progressTimerWired;

    private long _byteCount;
    private int _currentRowIndex;



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoader{TRecord}"/> class.
    /// </summary>
    /// <param name="streamWriter">The <see cref="StreamWriter"/> to write CSV data to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="streamWriter"/> is <c>null</c>.</exception>
    public CsvLoader
    (
        StreamWriter streamWriter
    )
    {
        _writer = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        _logger = NullLogger.Instance;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoader{TRecord}"/> class with diagnostic logging.
    /// </summary>
    /// <param name="streamWriter">The <see cref="StreamWriter"/> to write CSV data to.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="streamWriter"/> or <paramref name="logger"/> is <c>null</c>.
    /// </exception>
    public CsvLoader
    (
        StreamWriter streamWriter,
        ILogger<CsvLoader<TRecord>> logger
    )
    {
        _writer = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoader{TRecord}"/> class with an
    /// injected progress timer for testing.
    /// </summary>
    /// <param name="streamWriter">The <see cref="StreamWriter"/> to write CSV data to.</param>
    /// <param name="logger">An optional logger instance for diagnostic output.</param>
    /// <param name="timer">The progress timer to inject.</param>
    internal CsvLoader
    (
        StreamWriter streamWriter,
        ILogger? logger,
        IProgressTimer timer
    )
    {
        _writer = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));
        _logger = logger ?? (ILogger)NullLogger.Instance;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>Gets or sets the field delimiter. Default is <c>","</c>.</summary>
    public string Delimiter { get; set; } = ",";



    /// <summary>Gets or sets the character used to escape the quote character within a field.</summary>
    public char Escape { get; set; } = '"';



    /// <summary>Gets or sets the encoding to be passed to CsvHelper's writer configuration.</summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;



    /// <summary>Gets or sets a value indicating whether a header record should be written.</summary>
    public bool HasHeaderRecord { get; set; } = true;



    /// <summary>
    /// Gets or sets a value indicating whether the underlying stream should be left open
    /// after the writer is disposed.
    /// </summary>
    public bool LeaveOpen { get; set; }



    /// <summary>Gets or sets the line terminator written between records.</summary>
    public string NewLine { get; set; } = "\r\n";



    /// <summary>Gets or sets the quote character used to wrap fields when needed.</summary>
    public char Quote { get; set; } = '"';



    /// <summary>
    /// Gets or sets a callback that decides whether a field should be quoted.
    /// When <c>null</c>, the underlying parser's default policy is used.
    /// </summary>
    public Func<CsvShouldQuoteContext, bool>? ShouldQuote { get; set; }



    /// <summary>Gets or sets the trimming options applied while writing.</summary>
    public CsvTrimOptions TrimOptions { get; set; } = CsvTrimOptions.None;



    /// <summary>
    /// Gets or sets the number of records to skip before writing.
    /// This is an alias for <see cref="LoaderBase{TDestination,TProgress}.SkipItemCount"/>.
    /// </summary>
    public int SkipRecordCount
    {
        get => SkipItemCount;
        set => SkipItemCount = value;
    }



    /// <summary>
    /// Gets or sets the maximum number of records to write.
    /// This is an alias for <see cref="LoaderBase{TDestination,TProgress}.MaximumItemCount"/>.
    /// </summary>
    public int MaxRecordCount
    {
        get => MaximumItemCount;
        set => MaximumItemCount = value;
    }



    private CsvConfiguration BuildConfiguration()
    {
        var configuration = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            Delimiter = Delimiter,
            Escape = Escape,
            Encoding = Encoding,
            NewLine = NewLine,
            Quote = Quote,
            TrimOptions = (TrimOptions)(int)TrimOptions,
        };

        var callerShouldQuote = ShouldQuote;
        if (callerShouldQuote is not null)
        {
            configuration.ShouldQuote = args => callerShouldQuote
            (
                new CsvShouldQuoteContext(args.Field, args.FieldType)
            );
        }

        return configuration;
    }



    /// <inheritdoc />
    protected override async Task LoadWorkerAsync
    (
        IAsyncEnumerable<TRecord> items,
        CancellationToken token
    )
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        CsvLogMessages.StartingOperation(_logger, OperationName, null);

#pragma warning disable CA2007, MA0004
        await using var csvWriter = new CsvWriter(_writer, BuildConfiguration(), LeaveOpen);
#pragma warning restore CA2007, MA0004

        if (HasHeaderRecord)
        {
            csvWriter.WriteHeader<TRecord>();
            await csvWriter.NextRecordAsync().ConfigureAwait(false);
            UpdateRowSnapshot(csvWriter);
        }

        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            token.ThrowIfCancellationRequested();

            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                CsvLogMessages.SkippedItem(_logger, CurrentSkippedItemCount, SkipItemCount, null);
                continue;
            }

            if (CurrentItemCount >= MaximumItemCount)
            {
                CsvLogMessages.ReachedMaximumItemCount(_logger, MaximumItemCount, null);
                break;
            }

            csvWriter.WriteRecord(item);
            await csvWriter.NextRecordAsync().ConfigureAwait(false);

            IncrementCurrentItemCount();
            UpdateRowSnapshot(csvWriter);

            CsvLogMessages.LoadedItem(_logger, CurrentItemCount, null);
        }

        await csvWriter.FlushAsync().ConfigureAwait(false);

        CsvLogMessages.LoadingCompleted(_logger, CurrentItemCount, CurrentSkippedItemCount, null);
    }



    private void UpdateRowSnapshot(CsvWriter csvWriter)
    {
        _currentRowIndex = csvWriter.Row;

        var baseStream = _writer.BaseStream;
        if (baseStream is not null && baseStream.CanSeek)
        {
            _byteCount = baseStream.Position;
        }
    }



    /// <inheritdoc />
    protected override CsvLoaderProgress CreateProgressReport() =>
        new
        (
            CurrentItemCount,
            CurrentSkippedItemCount,
            Volatile.Read(ref _byteCount),
            Volatile.Read(ref _currentRowIndex)
        );



    /// <inheritdoc />
    protected override IProgressTimer CreateProgressTimer(IProgress<CsvLoaderProgress> progress)
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
