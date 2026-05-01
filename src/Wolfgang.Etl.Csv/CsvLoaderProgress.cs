using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV loading operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with the count of items skipped during loading
/// and the 1-based line number most recently written.
/// </remarks>
public record CsvLoaderProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoaderProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items loaded so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    /// <param name="currentLineNumber">
    /// The 1-based line number most recently written to the destination, or <c>0</c>
    /// before any line has been written.
    /// </param>
    public CsvLoaderProgress
    (
        int currentItemCount,
        int currentSkippedItemCount,
        int currentLineNumber
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
        CurrentLineNumber = currentLineNumber;
    }



    /// <summary>
    /// Gets the number of items skipped so far during loading.
    /// </summary>
    public int CurrentSkippedItemCount { get; }



    /// <summary>
    /// Gets the 1-based line number most recently written to the destination, or <c>0</c>
    /// before any line has been written.
    /// </summary>
    public int CurrentLineNumber { get; }
}
