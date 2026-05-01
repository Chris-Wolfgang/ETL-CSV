using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wolfgang.Etl.Csv.Tests.Unit.TestModels;
using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvAttributeMappingTests
{
    [ExcludeFromCodeCoverage]
    public sealed record IndexedRecord
    {
        [CsvColumn(Index = 0)]
        public string A { get; set; } = string.Empty;

        [CsvColumn(Index = 1)]
        public string B { get; set; } = string.Empty;
    }



    [ExcludeFromCodeCoverage]
    public sealed record OptionalRecord
    {
        [CsvColumn(Name = "first_name")]
        public string FirstName { get; set; } = string.Empty;

        [CsvColumn(Name = "middle_name", Optional = true, Default = "(none)")]
        public string MiddleName { get; set; } = string.Empty;
    }



    [ExcludeFromCodeCoverage]
    public sealed record DateRecord
    {
        [CsvColumn(Name = "name")]
        public string Name { get; set; } = string.Empty;

        [CsvColumn(Name = "dob", Format = "yyyy-MM-dd")]
        public DateTime DateOfBirth { get; set; }
    }



    private static StreamReader Reader(string csv) =>
        new(new MemoryStream(Encoding.UTF8.GetBytes(csv)), Encoding.UTF8);



    [Fact]
    public async Task ExtractAsync_when_CsvColumn_Name_is_used_maps_renamed_columns()
    {
        var csv = "first_name,last_name,age\r\nAlice,Smith,30\r\n";
        var sut = new CsvExtractor<AttributedPersonRecord>(Reader(csv));

        var results = new List<AttributedPersonRecord>();
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
    public async Task ExtractAsync_when_CsvIgnore_is_present_skips_property()
    {
        // The CSV has no ComputedDisplayName column; with [CsvIgnore] this must succeed.
        var csv = "first_name,last_name,age\r\nAlice,Smith,30\r\n";
        var sut = new CsvExtractor<AttributedPersonRecord>(Reader(csv));

        var results = new List<AttributedPersonRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal(string.Empty, results[0].ComputedDisplayName);
    }



    [Fact]
    public async Task ExtractAsync_when_CsvColumn_Index_is_used_binds_by_position()
    {
        // No header row — bind by index.
        var csv = "Alice,Smith\r\nBob,Jones\r\n";
        var sut = new CsvExtractor<IndexedRecord>(Reader(csv))
        {
            HasHeaderRecord = false,
        };

        var results = new List<IndexedRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("Alice", results[0].A);
        Assert.Equal("Smith", results[0].B);
    }



    [Fact]
    public async Task ExtractAsync_when_Optional_column_is_missing_uses_Default()
    {
        // middle_name column is absent.
        var csv = "first_name\r\nAlice\r\n";
        var sut = new CsvExtractor<OptionalRecord>(Reader(csv));

        var results = new List<OptionalRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal("Alice", results[0].FirstName);
        Assert.Equal("(none)", results[0].MiddleName);
    }



    [Fact]
    public async Task ExtractAsync_when_Format_is_specified_parses_using_format()
    {
        var csv = "name,dob\r\nAlice,1995-04-12\r\n";
        var sut = new CsvExtractor<DateRecord>(Reader(csv));

        var results = new List<DateRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Single(results);
        Assert.Equal(new DateTime(1995, 4, 12), results[0].DateOfBirth);
    }



    [Fact]
    public async Task LoadAsync_when_CsvColumn_Name_is_used_writes_renamed_columns()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<AttributedPersonRecord>(writer)
        {
            LeaveOpen = true,
        };

        var items = new List<AttributedPersonRecord>
        {
            new() { FirstName = "Alice", LastName = "Smith", Age = 30 },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());
        await writer.FlushAsync();

        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.Contains("first_name,last_name,age", text);
        Assert.Contains("Alice,Smith,30", text);
        Assert.DoesNotContain("ComputedDisplayName", text);
    }



    [Fact]
    public async Task LoadAsync_when_Format_is_specified_writes_using_format()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 1024, leaveOpen: true);
        var sut = new CsvLoader<DateRecord>(writer)
        {
            LeaveOpen = true,
        };

        var items = new List<DateRecord>
        {
            new() { Name = "Alice", DateOfBirth = new DateTime(1995, 4, 12) },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());
        await writer.FlushAsync();

        stream.Position = 0;
        var text = new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        Assert.Contains("1995-04-12", text);
    }
}
