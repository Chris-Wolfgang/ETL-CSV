using System;
using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvColumnAttributeTests
{
    [Fact]
    public void Default_constructor_leaves_Name_null()
    {
        var sut = new CsvColumnAttribute();

        Assert.Null(sut.Name);
    }



    [Fact]
    public void Default_constructor_sets_Index_to_negative_one()
    {
        var sut = new CsvColumnAttribute();

        Assert.Equal(-1, sut.Index);
    }



    [Fact]
    public void Default_constructor_sets_Optional_to_false()
    {
        var sut = new CsvColumnAttribute();

        Assert.False(sut.Optional);
    }



    [Fact]
    public void Default_constructor_leaves_Format_null()
    {
        var sut = new CsvColumnAttribute();

        Assert.Null(sut.Format);
    }



    [Fact]
    public void Default_constructor_leaves_Default_null()
    {
        var sut = new CsvColumnAttribute();

        Assert.Null(sut.Default);
    }



    [Fact]
    public void Name_constructor_sets_Name_property()
    {
        var sut = new CsvColumnAttribute("first_name");

        Assert.Equal("first_name", sut.Name);
    }



    [Fact]
    public void Name_constructor_keeps_Index_default_of_negative_one()
    {
        var sut = new CsvColumnAttribute("first_name");

        Assert.Equal(-1, sut.Index);
    }



    [Fact]
    public void Name_constructor_accepts_null()
    {
        var sut = new CsvColumnAttribute(null!);

        Assert.Null(sut.Name);
    }



    [Fact]
    public void Name_constructor_accepts_empty_string()
    {
        var sut = new CsvColumnAttribute(string.Empty);

        Assert.Equal(string.Empty, sut.Name);
    }



    [Fact]
    public void Name_property_initializer_round_trips()
    {
        var sut = new CsvColumnAttribute { Name = "last_name" };

        Assert.Equal("last_name", sut.Name);
    }



    [Fact]
    public void Index_property_initializer_round_trips()
    {
        var sut = new CsvColumnAttribute { Index = 7 };

        Assert.Equal(7, sut.Index);
    }



    [Fact]
    public void Optional_property_initializer_round_trips()
    {
        var sut = new CsvColumnAttribute { Optional = true };

        Assert.True(sut.Optional);
    }



    [Fact]
    public void Format_property_initializer_round_trips()
    {
        var sut = new CsvColumnAttribute { Format = "yyyy-MM-dd" };

        Assert.Equal("yyyy-MM-dd", sut.Format);
    }



    [Fact]
    public void Default_property_initializer_round_trips()
    {
        var sut = new CsvColumnAttribute { Default = "1970-01-01" };

        Assert.Equal("1970-01-01", sut.Default);
    }



    [Fact]
    public void All_properties_can_be_set_together()
    {
        var sut = new CsvColumnAttribute("dob")
        {
            Index = 3,
            Optional = true,
            Format = "yyyy-MM-dd",
            Default = "1970-01-01"
        };

        Assert.Equal("dob", sut.Name);
        Assert.Equal(3, sut.Index);
        Assert.True(sut.Optional);
        Assert.Equal("yyyy-MM-dd", sut.Format);
        Assert.Equal("1970-01-01", sut.Default);
    }



    [Fact]
    public void AttributeUsage_targets_property_only()
    {
        var usage = (AttributeUsageAttribute[])typeof(CsvColumnAttribute)
            .GetCustomAttributes(attributeType: typeof(AttributeUsageAttribute), inherit: false);

        Assert.Single(usage);
        Assert.Equal(AttributeTargets.Property, usage[0].ValidOn);
    }



    [Fact]
    public void AttributeUsage_does_not_allow_multiple()
    {
        var usage = (AttributeUsageAttribute[])typeof(CsvColumnAttribute)
            .GetCustomAttributes(attributeType: typeof(AttributeUsageAttribute), inherit: false);

        Assert.False(usage[0].AllowMultiple);
    }



    [Fact]
    public void AttributeUsage_is_inherited()
    {
        var usage = (AttributeUsageAttribute[])typeof(CsvColumnAttribute)
            .GetCustomAttributes(attributeType: typeof(AttributeUsageAttribute), inherit: false);

        Assert.True(usage[0].Inherited);
    }
}
