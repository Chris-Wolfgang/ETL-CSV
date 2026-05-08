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
        Assert.Equal(new DateTime(1995, 4, 12, 0, 0, 0, DateTimeKind.Utc), results[0].DateOfBirth);
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
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();

        Assert.Contains("first_name,last_name,age", text, StringComparison.Ordinal);
        Assert.Contains("Alice,Smith,30", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputedDisplayName", text, StringComparison.Ordinal);
    }



    [ExcludeFromCodeCoverage]
    public sealed record PriceRecord
    {
        public string ProductNumber { get; set; } = string.Empty;
        public decimal RetailPrice { get; set; }
        public decimal MSRP { get; set; }
    }



    [Fact]
    public async Task ExtractAsync_when_runtime_ColumnMaps_set_binds_by_index_and_overrides_attributes()
    {
        // Layout: garbage,garbage,ProductNumber,garbage,RetailPrice,garbage,MSRP
        var csv = "x,y,P-12345,z,9.99,q,14.50\r\nx,y,P-67890,z,4.50,q,7.75\r\n";
        var sut = new CsvExtractor<PriceRecord>(Reader(csv))
        {
            HasHeaderRecord = false,
            ColumnMaps = new[]
            {
                new CsvColumnMap(nameof(PriceRecord.ProductNumber)) { Index = 2 },
                new CsvColumnMap(nameof(PriceRecord.RetailPrice))   { Index = 4 },
                new CsvColumnMap(nameof(PriceRecord.MSRP))          { Index = 6 },
            },
        };

        var results = new List<PriceRecord>();
        await foreach (var item in sut.ExtractAsync())
        {
            results.Add(item);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal("P-12345", results[0].ProductNumber);
        Assert.Equal(9.99m, results[0].RetailPrice);
        Assert.Equal(14.50m, results[0].MSRP);
        Assert.Equal("P-67890", results[1].ProductNumber);
    }



    [Fact]
    public void CsvClassMapFactory_BuildFromColumnMaps_when_columnMaps_is_empty_throws()
    {
        Assert.Throws<ArgumentException>
        (
            () => CsvClassMapFactory.BuildFromColumnMaps<PriceRecord>(Array.Empty<CsvColumnMap>())
        );
    }



    [Fact]
    public void CsvClassMapFactory_BuildFromColumnMaps_when_columnMaps_is_null_throws()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => CsvClassMapFactory.BuildFromColumnMaps<PriceRecord>(null!)
        );
    }



    [Fact]
    public Task ExtractAsync_when_runtime_ColumnMaps_names_unknown_property_throws()
    {
        var csv = "x\r\n";
        var sut = new CsvExtractor<PriceRecord>(Reader(csv))
        {
            HasHeaderRecord = false,
            ColumnMaps = new[]
            {
                new CsvColumnMap("DoesNotExist") { Index = 0 },
            },
        };

        return Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await foreach (var _ in sut.ExtractAsync().ConfigureAwait(false))
            {
                // drain — exception fires before the first record yields
            }
        });
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
            new() { Name = "Alice", DateOfBirth = new DateTime(1995, 4, 12, 0, 0, 0, DateTimeKind.Utc) },
        };

        await sut.LoadAsync(items.ToAsyncEnumerable());
        await writer.FlushAsync();

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = await reader.ReadToEndAsync();

        Assert.Contains("1995-04-12", text, StringComparison.Ordinal);
    }
}
