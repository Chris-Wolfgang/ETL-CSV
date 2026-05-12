using CsvHelperTrimOptions = CsvHelper.Configuration.TrimOptions;
using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvTrimOptionsExtensionsTests
{
    [Fact]
    public void ToCsvHelper_when_None_returns_None()
    {
        Assert.Equal(CsvHelperTrimOptions.None, CsvTrimOptions.None.ToCsvHelper());
    }



    [Fact]
    public void ToCsvHelper_when_Trim_returns_Trim()
    {
        Assert.Equal(CsvHelperTrimOptions.Trim, CsvTrimOptions.Trim.ToCsvHelper());
    }



    [Fact]
    public void ToCsvHelper_when_InsideQuotes_returns_InsideQuotes()
    {
        Assert.Equal(CsvHelperTrimOptions.InsideQuotes, CsvTrimOptions.InsideQuotes.ToCsvHelper());
    }



    [Fact]
    public void ToCsvHelper_when_Trim_and_InsideQuotes_returns_combined_flags()
    {
        Assert.Equal
        (
            CsvHelperTrimOptions.Trim | CsvHelperTrimOptions.InsideQuotes,
            (CsvTrimOptions.Trim | CsvTrimOptions.InsideQuotes).ToCsvHelper()
        );
    }
}
