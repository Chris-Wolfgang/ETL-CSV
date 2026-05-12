using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV extraction operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with the count of items skipped during extraction,
/// the 1-based line number currently being read, and the count of bad-data events
/// observed so far.
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
    /// <param name="currentBadDataCount">
    /// The number of bad-data events observed so far. Counts every invocation of the
    /// underlying parser's bad-data callback, regardless of whether the caller supplied
    /// a custom <c>BadDataFound</c> handler.
    /// </param>
    public CsvExtractorProgress
    (
        int currentItemCount,
        int currentSkippedItemCount,
        int currentLineNumber,
        int currentBadDataCount
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
        CurrentLineNumber = currentLineNumber;
        CurrentBadDataCount = currentBadDataCount;
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



    /// <summary>
    /// Gets the number of bad-data events observed so far.
    /// </summary>
    public int CurrentBadDataCount { get; }
}
