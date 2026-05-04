using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvBadDataInfoTests
{
    [Fact]
    public void Constructor_when_called_assigns_all_properties()
    {
        var info = new CsvBadDataInfo
        (
            LineNumber: 7,
            ColumnNumber: 3,
            Field: "bad",
            RawRecord: "a,bad,c"
        );

        Assert.Equal(7, info.LineNumber);
        Assert.Equal(3, info.ColumnNumber);
        Assert.Equal("bad", info.Field);
        Assert.Equal("a,bad,c", info.RawRecord);
    }



    [Fact]
    public void Constructor_when_field_is_null_assigns_null()
    {
        var info = new CsvBadDataInfo
        (
            LineNumber: -1,
            ColumnNumber: -1,
            Field: null,
            RawRecord: "raw"
        );

        Assert.Null(info.Field);
        Assert.Equal(-1, info.LineNumber);
        Assert.Equal(-1, info.ColumnNumber);
    }



    [Fact]
    public void Equals_when_all_properties_match_returns_true()
    {
        var a = new CsvBadDataInfo(1, 2, "x", "raw");
        var b = new CsvBadDataInfo(1, 2, "x", "raw");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }



    [Fact]
    public void Equals_when_a_property_differs_returns_false()
    {
        var baseline = new CsvBadDataInfo(1, 2, "x", "raw");

        Assert.NotEqual(baseline, baseline with { LineNumber = 99 });
        Assert.NotEqual(baseline, baseline with { ColumnNumber = 99 });
        Assert.NotEqual(baseline, baseline with { Field = "different" });
        Assert.NotEqual(baseline, baseline with { RawRecord = "different" });
    }



    [Fact]
    public void ToString_when_called_returns_non_empty_value()
    {
        var info = new CsvBadDataInfo(5, 2, "bad", "row");

        Assert.False(string.IsNullOrEmpty(info.ToString()));
    }
}
