using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV extraction operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with CSV-specific progress information,
/// including byte position, the current parsed row index, the current raw row index,
/// and the count of skipped items.
/// </remarks>
public record CsvExtractorProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvExtractorProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items extracted so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    /// <param name="byteCount">The number of bytes consumed so far.</param>
    /// <param name="currentRowIndex">The current parsed (logical) row index reported by CsvHelper.</param>
    /// <param name="currentRawRowIndex">The current raw (file) row index reported by CsvHelper.</param>
    public CsvExtractorProgress
    (
        int currentItemCount,
        int currentSkippedItemCount,
        long byteCount,
        int currentRowIndex,
        int currentRawRowIndex
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
        ByteCount = byteCount;
        CurrentRowIndex = currentRowIndex;
        CurrentRawRowIndex = currentRawRowIndex;
    }



    /// <summary>
    /// Gets the number of items skipped so far during extraction.
    /// </summary>
    public int CurrentSkippedItemCount { get; }



    /// <summary>
    /// Gets the number of bytes consumed from the underlying stream so far.
    /// </summary>
    public long ByteCount { get; }



    /// <summary>
    /// Gets the current parsed (logical) row index reported by the underlying CSV parser.
    /// </summary>
    public int CurrentRowIndex { get; }



    /// <summary>
    /// Gets the current raw (file) row index reported by the underlying CSV parser.
    /// </summary>
    public int CurrentRawRowIndex { get; }
}
