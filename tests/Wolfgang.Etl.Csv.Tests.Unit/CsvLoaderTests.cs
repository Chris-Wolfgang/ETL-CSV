using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.Csv.Tests.Unit.TestModels;
using Wolfgang.Etl.TestKit.Xunit;
using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvLoaderTests
    : LoaderBaseContractTests
    <
        CsvLoader<PersonRecord>,
        PersonRecord,
        CsvLoaderProgress
    >
{
    private static readonly IReadOnlyList<PersonRecord> SourceItems = new List<PersonRecord>
    {
        new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        new() { FirstName = "Bob", LastName = "Jones", Age = 25 },
        new() { FirstName = "Carol", LastName = "White", Age = 35 },
        new() { FirstName = "Dave", LastName = "Brown", Age = 40 },
        new() { FirstName = "Eve", LastName = "Davis", Age = 28 },
    };



    private static (CsvLoader<PersonRecord> sut, MemoryStream stream, StreamWriter writer) CreateLoader()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<PersonRecord>(writer)
        {
            LeaveOpen = true,
        };
        return (sut, stream, writer);
    }



    protected override CsvLoader<PersonRecord> CreateSut(int itemCount)
    {
        var (sut, _, _) = CreateLoader();
        return sut;
    }



    protected override IReadOnlyList<PersonRecord> CreateSourceItems() => SourceItems;



    protected override CsvLoader<PersonRecord> CreateSutWithTimer
    (
        IProgressTimer timer
    )
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        return new CsvLoader<PersonRecord>
        (
            writer,
            NullLogger<CsvLoader<PersonRecord>>.Instance,
            timer
        )
        {
            LeaveOpen = true,
        };
    }



    [Fact]
    public void Constructor_when_streamWriter_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvLoader<PersonRecord>(null!)
        );
    }



    [Fact]
    public void Constructor_with_logger_when_streamWriter_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new CsvLoader<PersonRecord>
            (
                null!,
                NullLogger<CsvLoader<PersonRecord>>.Instance
            )
        );
    }



    [Fact]
    public void Constructor_with_logger_when_logger_is_null_throws_ArgumentNullException()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);

        Assert.Throws<ArgumentNullException>
        (
            () => new CsvLoader<PersonRecord>
            (
                writer,
                logger: null!
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_timer_is_null_throws_ArgumentNullException()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);

        Assert.Throws<ArgumentNullException>
        (
            () => new CsvLoader<PersonRecord>
            (
                writer,
                NullLogger<CsvLoader<PersonRecord>>.Instance,
                null!
            )
        );
    }



    [Fact]
    public void Internal_constructor_when_logger_is_null_does_not_throw()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);

        var sut = new CsvLoader<PersonRecord>
        (
            writer,
            logger: null,
            new ManualProgressTimer()
        );

        Assert.NotNull(sut);
    }



    [Fact]
    public async Task LoadAsync_writes_header_and_records()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<PersonRecord>(writer)
        {
            LeaveOpen = true,
        };

        await sut.LoadAsync(SourceItems.Take(2).ToAsyncEnumerable());

        await writer.FlushAsync();
        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.Contains("FirstName,LastName,Age", text);
        Assert.Contains("Alice,Smith,30", text);
        Assert.Contains("Bob,Jones,25", text);
    }



    [Fact]
    public async Task LoadAsync_when_HasHeaderRecord_is_false_omits_header()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<PersonRecord>(writer)
        {
            HasHeaderRecord = false,
            LeaveOpen = true,
        };

        await sut.LoadAsync(SourceItems.Take(1).ToAsyncEnumerable());

        await writer.FlushAsync();
        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.DoesNotContain("FirstName,LastName,Age", text);
        Assert.Contains("Alice,Smith,30", text);
    }



    [Fact]
    public async Task LoadAsync_when_Delimiter_is_pipe_writes_pipe_delimited()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<PersonRecord>(writer)
        {
            Delimiter = "|",
            LeaveOpen = true,
        };

        await sut.LoadAsync(SourceItems.Take(1).ToAsyncEnumerable());

        await writer.FlushAsync();
        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.Contains("FirstName|LastName|Age", text);
        Assert.Contains("Alice|Smith|30", text);
    }



    [Fact]
    public async Task LoadAsync_when_ShouldQuote_callback_is_set_invokes_it()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<PersonRecord>(writer)
        {
            ShouldQuote = _ => true,
            LeaveOpen = true,
        };

        await sut.LoadAsync(SourceItems.Take(1).ToAsyncEnumerable());

        await writer.FlushAsync();
        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.Contains("\"Alice\"", text);
        Assert.Contains("\"Smith\"", text);
    }



}
