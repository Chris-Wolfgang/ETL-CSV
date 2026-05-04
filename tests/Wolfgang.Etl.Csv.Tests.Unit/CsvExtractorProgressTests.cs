using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvExtractorProgressTests
{
    [Fact]
    public void Constructor_when_called_assigns_all_properties()
    {
        var progress = new CsvExtractorProgress
        (
            currentItemCount: 10,
            currentSkippedItemCount: 2,
            currentLineNumber: 13,
            currentBadDataCount: 1
        );

        Assert.Equal(10, progress.CurrentItemCount);
        Assert.Equal(2, progress.CurrentSkippedItemCount);
        Assert.Equal(13, progress.CurrentLineNumber);
        Assert.Equal(1, progress.CurrentBadDataCount);
    }



    [Fact]
    public void Constructor_when_all_zero_returns_zeroed_progress()
    {
        var progress = new CsvExtractorProgress(0, 0, 0, 0);

        Assert.Equal(0, progress.CurrentItemCount);
        Assert.Equal(0, progress.CurrentSkippedItemCount);
        Assert.Equal(0, progress.CurrentLineNumber);
        Assert.Equal(0, progress.CurrentBadDataCount);
    }



    [Fact]
    public void Equals_when_all_properties_match_returns_true()
    {
        var a = new CsvExtractorProgress(5, 1, 6, 0);
        var b = new CsvExtractorProgress(5, 1, 6, 0);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }



    [Fact]
    public void Equals_when_a_property_differs_returns_false()
    {
        var a = new CsvExtractorProgress(5, 1, 6, 0);
        var b = new CsvExtractorProgress(5, 1, 6, 1);

        Assert.NotEqual(a, b);
    }
}
