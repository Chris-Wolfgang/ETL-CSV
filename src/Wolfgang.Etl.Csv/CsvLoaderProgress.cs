using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV loading operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with the count of items skipped during loading.
/// </remarks>
public record CsvLoaderProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoaderProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items loaded so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    public CsvLoaderProgress
    (
        int currentItemCount,
        int currentSkippedItemCount
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
    }



    /// <summary>
    /// Gets the number of items skipped so far during loading.
    /// </summary>
    public int CurrentSkippedItemCount { get; }
}
