using Xunit;

namespace Wolfgang.Etl.Csv.Tests.Unit;

public class CsvLoaderProgressTests
{
    [Fact]
    public void Constructor_when_called_assigns_all_properties()
    {
        var progress = new CsvLoaderProgress
        (
            currentItemCount: 8,
            currentSkippedItemCount: 1,
            currentLineNumber: 9
        );

        Assert.Equal(8, progress.CurrentItemCount);
        Assert.Equal(1, progress.CurrentSkippedItemCount);
        Assert.Equal(9, progress.CurrentLineNumber);
    }



    [Fact]
    public void Constructor_when_all_zero_returns_zeroed_progress()
    {
        var progress = new CsvLoaderProgress(0, 0, 0);

        Assert.Equal(0, progress.CurrentItemCount);
        Assert.Equal(0, progress.CurrentSkippedItemCount);
        Assert.Equal(0, progress.CurrentLineNumber);
    }



    [Fact]
    public void Equals_when_all_properties_match_returns_true()
    {
        var a = new CsvLoaderProgress(3, 0, 4);
        var b = new CsvLoaderProgress(3, 0, 4);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }



    [Fact]
    public void Equals_when_a_property_differs_returns_false()
    {
        var a = new CsvLoaderProgress(3, 0, 4);
        var b = new CsvLoaderProgress(3, 1, 4);

        Assert.NotEqual(a, b);
    }
}
