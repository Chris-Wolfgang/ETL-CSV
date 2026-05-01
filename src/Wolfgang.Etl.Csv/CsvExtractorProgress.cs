using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV extraction operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with the count of items skipped during extraction
/// and the 1-based line number currently being read.
/// </remarks>
public record CsvExtractorProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractorProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items extracted so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    /// <param name="currentLineNumber">
    /// The 1-based line number most recently read from the source, or <c>0</c> before reading begins.
    /// Counts every physical line in the source, including comments and blank lines.
    /// </param>
    public CsvExtractorProgress
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
    /// Gets the number of items skipped so far during extraction.
    /// </summary>
    public int CurrentSkippedItemCount { get; }



    /// <summary>
    /// Gets the 1-based line number most recently read from the source, or <c>0</c>
    /// before reading begins.
    /// </summary>
    public int CurrentLineNumber { get; }
}
