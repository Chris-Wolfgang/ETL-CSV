using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvShouldQuoteContextTests
{
    [Fact]
    public void Constructor_when_called_assigns_all_properties()
    {
        var ctx = new CsvShouldQuoteContext
        (
            Field: "value",
            FieldType: typeof(string)
        );

        Assert.Equal("value", ctx.Field);
        Assert.Equal(typeof(string), ctx.FieldType);
    }



    [Fact]
    public void Constructor_when_field_and_type_are_null_assigns_null()
    {
        var ctx = new CsvShouldQuoteContext(Field: null, FieldType: null);

        Assert.Null(ctx.Field);
        Assert.Null(ctx.FieldType);
    }



    [Fact]
    public void Equals_when_all_properties_match_returns_true()
    {
        var a = new CsvShouldQuoteContext("v", typeof(int));
        var b = new CsvShouldQuoteContext("v", typeof(int));

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }



    [Fact]
    public void Equals_when_a_property_differs_returns_false()
    {
        var baseline = new CsvShouldQuoteContext("v", typeof(int));

        Assert.NotEqual(baseline, baseline with { Field = "other" });
        Assert.NotEqual(baseline, baseline with { FieldType = typeof(string) });
    }



    [Fact]
    public void ToString_when_called_returns_non_empty_value()
    {
        var ctx = new CsvShouldQuoteContext("v", typeof(int));

        Assert.False(string.IsNullOrEmpty(ctx.ToString()));
    }
}
