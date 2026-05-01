using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV extraction operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with the count of items skipped during extraction.
/// </remarks>
public record CsvExtractorProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractorProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items extracted so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    public CsvExtractorProgress
    (
        int currentItemCount,
        int currentSkippedItemCount
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
    }



    /// <summary>
    /// Gets the number of items skipped so far during extraction.
    /// </summary>
    public int CurrentSkippedItemCount { get; }
}
