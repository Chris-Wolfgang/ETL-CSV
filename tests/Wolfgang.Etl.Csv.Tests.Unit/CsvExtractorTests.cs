using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Csv.Tests.Unit.TestModels;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvExtractorTests
    : ExtractorBaseContractTests
    <
        CsvExtractor<PersonRecord>,
        PersonRecord,
        CsvExtractorProgress
    >
{
    private static readonly IReadOnlyList<PersonRecord> ExpectedItems = new List<PersonRecord>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
        new() { FirstName = "Dave", LastName = "Brown", Age = 40 },
        new() { FirstName = "Eve", LastName = "Davis", Age = 28 },
    };



    private static StreamReader CreateCsvStream(IEnumerable<PersonRecord> items, bool includeHeader = true)
    {
        var sb = new StringBuilder();
        if (includeHeader)
        {
            sb.AppendLine("FirstName,LastName,Age");
        }
        foreach (var p in items)
        {
            sb.Append(p.FirstName).Append(',').Append(p.LastName).Append(',').Append(p.Age).Append('\n');
        }
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        return new StreamReader(stream, Encoding.UTF8);
    }



    protected override CsvExtractor<PersonRecord> CreateSut(int itemCount)
    {
        var items = ExpectedItems.Take(itemCount).ToList();
        return new CsvExtractor<PersonRecord>(CreateCsvStream(items));
    }



    protected override IReadOnlyList<PersonRecord> CreateExpectedItems() => ExpectedItems;



    protected override CsvExtractor<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    )
    {
        return new CsvExtractor<PersonRecord>
        (
            CreateCsvStream(ExpectedItems),
            NullLogger<CsvExtractor<PersonRecord>>.Instance,
            timer
        );
    }



    [Fact]
    public void Constructor_when_streamReader_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvExtractor<PersonRecord>(null!)
        );
    }



    [Fact]
    public void Constructor_with_logger_when_streamReader_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvExtractor<PersonRecord>
            (
                null!,
                NullLogger<CsvExtractor<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Constructor_with_logger_when_logger_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvExtractor<PersonRecord>
            (
                CreateCsvStream(ExpectedItems),
                logger: null!
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_timer_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvExtractor<PersonRecord>
            (
                CreateCsvStream(ExpectedItems),
                NullLogger<CsvExtractor<PersonRecord>>.Instance,
                null!
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_streamReader_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvExtractor<PersonRecord>
            (
                null!,
                NullLogger<CsvExtractor<PersonRecord>>.Instance,
                new ManualProgressTimer()
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_logger_is_null_does_not_throw()
    {
        var sut = new CsvExtractor<PersonRecord>
        (
            CreateCsvStream(ExpectedItems),
            logger: null,
            new ManualProgressTimer()
        );

        Assert.NotNull(sut);
    }



    [Fact]
    public async Task ExtractAsync_keeps_caller_StreamReader_and_underlying_stream_usable()
    {
        // Tightened from a stream-only assertion: also verify the caller's
        // StreamReader itself wasn't disposed. The original `stream.CanRead`-only
        // check could not detect a regression where the extractor disposed the
        // StreamReader, because the StreamReader was constructed with leaveOpen:true
        // which prevents disposal from cascading to the underlying stream.
        // `reader.Peek()` throws ObjectDisposedException if the StreamReader was
        // disposed, so calling it without expecting a throw asserts liveness.
        var csv = "FirstName,LastName,Age\r\nAlice,Smith,30\r\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var sut = new CsvExtractor<PersonRecord>(reader);

        await foreach (var _ in sut.ExtractAsync())
        {
            // drain
        }

        // StreamReader liveness — implicit assertion via Peek not throwing.
        var peeked = reader.Peek();
        Assert.True(peeked == -1 || peeked >= 0);

        // Underlying stream is also still readable.
        Assert.True(stream.CanRead);
    }



    [Fact]
    public void InitialRecordIndex_when_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = new CsvExtractor<PersonRecord>(CreateCsvStream(ExpectedItems));

        Assert.Throws<ArgumentOutOfRangeException>
        (
            () => sut.InitialRecordIndex = 0
        );
    }



    [Fact]
    public async Task ExtractAsync_when_HasHeaderRecord_is_false_reads_all_rows_as_data()
    {
        var sut = new CsvExtractor<PersonRecord>(CreateCsvStream(ExpectedItems.Take(2), includeHeader: false))
        {
            HasHeaderRecord = false,
        };

        // Without a header record, CsvHelper relies on positional binding via auto-mapping
        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Bob", results[1].FirstName);
    }



    [Fact]
    public async Task ExtractAsync_when_AllowComments_skips_comment_lines()
    {
        var csv = "FirstName,LastName,Age\r\n# this is a comment\r\nAlice,Smith,30\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            AllowComments = true,
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
    }



    [Fact]
    public async Task ExtractAsync_when_Delimiter_is_pipe_parses_pipe_delimited_data()
    {
        var csv = "FirstName|LastName|Age\r\nAlice|Smith|30\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            Delimiter = "|",
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Smith", results[0].LastName);
        Assert.Equal(30, results[0].Age);
    }



    [Fact]
    public async Task ExtractAsync_when_InitialRecordIndex_is_three_skips_first_two_lines()
    {
        var csv = "Title row to ignore\r\nMore garbage\r\nFirstName,LastName,Age\r\nAlice,Smith,30\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            InitialRecordIndex = 3,
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
    }



    [Fact]
    public async Task ExtractAsync_when_MaximumItemCount_reached_does_not_invoke_BadDataFound_for_subsequent_rows()
    {
        // Bad data on row 2 (after the header). With MaximumItemCount = 1, we should
        // yield exactly one record and the BadDataFound callback should NOT fire for
        // any row beyond the limit. Regression test for the prior implementation that
        // used GetRecordsAsync, which materialized one extra row before the limit
        // check could stop us.
        var csv = "FirstName,LastName,Age\r\nAlice,Smith,30\r\nBo\"b,Jones,25\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var badDataFires = 0;
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            MaximumItemCount = 1,
            BadDataFound = _ => { badDataFires++; },
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal(0, badDataFires);   // never read past the limit, so the bad row never fires
    }



    [Fact]
    public async Task ExtractAsync_increments_CurrentBadDataCount_for_each_bad_data_event()
    {
        var csv = "FirstName,LastName,Age\r\nAl\"ice,Smith,30\r\nBo\"b,Jones,25\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8));

        var progress = new SyncProgress<CsvExtractorProgress>();

        await foreach (var _ in sut.ExtractAsync(progress))
        {
            // drain
        }

        Assert.NotNull(progress.LastValue);
        Assert.True(progress.LastValue!.CurrentBadDataCount >= 1);
    }



    [Fact]
    public async Task ExtractAsync_when_BadDataFound_is_set_invokes_callback_and_continues()
    {
        var csv = "FirstName,LastName,Age\r\nAl\"ice,Smith,30\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var observedFields = new List<string?>();
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            BadDataFound = info => observedFields.Add(info.Field),
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        // The callback fires when bad data is encountered; extraction still completes
        // and yields the row (CsvHelper recovers and parses what it can).
        Assert.NotEmpty(observedFields);
        Assert.Single(results);
    }



    [Fact]
    public async Task ExtractAsync_when_TrimOptions_is_Trim_trims_whitespace()
    {
        var csv = "FirstName,LastName,Age\r\n  Alice  , Smith , 30 \r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            TrimOptions = CsvTrimOptions.Trim,
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Smith", results[0].LastName);
    }



    [Fact]
    public async Task ExtractAsync_when_IgnoreBlankLines_is_true_skips_blank_lines()
    {
        var csv = "FirstName,LastName,Age\r\nAlice,Smith,30\r\n\r\nBob,Jones,25\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8))
        {
            IgnoreBlankLines = true,
        };

        var results = new List<PersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("Bob", results[1].FirstName);
    }



    [Fact]
    public void Constructor_with_logger_when_args_valid_returns_instance()
    {
        var sut = new CsvExtractor<PersonRecord>
        (
            CreateCsvStream(ExpectedItems),
            NullLogger<CsvExtractor<PersonRecord>>.Instance
        );

        Assert.NotNull(sut);
    }



    [Fact]
    public void SkipRecordCount_when_set_updates_SkipItemCount_alias()
    {
        var sut = new CsvExtractor<PersonRecord>(CreateCsvStream(ExpectedItems))
        {
            SkipRecordCount = 7,
        };

        Assert.Equal(7, sut.SkipRecordCount);
        Assert.Equal(7, sut.SkipItemCount);
    }



    [Fact]
    public void MaxRecordCount_when_set_updates_MaximumItemCount_alias()
    {
        var sut = new CsvExtractor<PersonRecord>(CreateCsvStream(ExpectedItems))
        {
            MaxRecordCount = 11,
        };

        Assert.Equal(11, sut.MaxRecordCount);
        Assert.Equal(11, sut.MaximumItemCount);
    }



    [Fact]
    public Task ExtractAsync_when_type_conversion_fails_propagates_TypeConverterException()
    {
        // "not-a-number" cannot be converted to int for the Age column.
        // CsvHelper raises this through OnReadingExceptionOccurred; our handler
        // returns true so CsvHelper still propagates the exception via the async
        // iterator. This test asserts the propagation path; the companion test
        // below asserts the public ReadingExceptionOccurred callback fires.
        var csv = "FirstName,LastName,Age\r\nBob,Jones,not-a-number\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8));

        return Assert.ThrowsAsync<CsvHelper.TypeConversion.TypeConverterException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync().ConfigureAwait(false))
                {
                    // drain
                }
            }
        );
    }



    [Fact]
    public async Task ExtractAsync_when_type_conversion_fails_invokes_ReadingExceptionOccurred_callback()
    {
        // The public ReadingExceptionOccurred callback should fire with a populated
        // CsvReadingExceptionInfo whenever CsvHelper raises a parse exception. The
        // exception still propagates after the callback returns — the callback is
        // purely for observation/logging.
        var csv = "FirstName,LastName,Age\r\nBob,Jones,not-a-number\r\n";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var sut = new CsvExtractor<PersonRecord>(new StreamReader(stream, Encoding.UTF8));

        CsvReadingExceptionInfo? captured = null;
        sut.ReadingExceptionOccurred = info => captured = info;

        await Assert.ThrowsAsync<CsvHelper.TypeConversion.TypeConverterException>
        (
            async () =>
            {
                await foreach (var _ in sut.ExtractAsync().ConfigureAwait(false))
                {
                    // drain
                }
            }
        );

        Assert.NotNull(captured);
        Assert.NotNull(captured!.Exception);
        Assert.Equal(2, captured.LineNumber);
        Assert.Equal(3, captured.ColumnNumber);
        Assert.Equal("Age", captured.ColumnName);
        Assert.Equal("not-a-number", captured.ColumnValue);
    }
}
