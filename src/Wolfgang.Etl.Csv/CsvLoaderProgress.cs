using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Progress report for CSV loading operations.
/// </summary>
/// <remarks>
/// Extends <see cref="Report"/> with CSV-specific progress information,
/// including byte position, the current row index, and the count of skipped items.
/// </remarks>
public record CsvLoaderProgress : Report
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLoaderProgress"/> class.
    /// </summary>
    /// <param name="currentItemCount">The number of items loaded so far.</param>
    /// <param name="currentSkippedItemCount">The number of items skipped so far.</param>
    /// <param name="byteCount">The number of bytes written to the underlying stream so far.</param>
    /// <param name="currentRowIndex">The current row index reported by the underlying CSV writer.</param>
    public CsvLoaderProgress
    (
        int currentItemCount,
        int currentSkippedItemCount,
        long byteCount,
        int currentRowIndex
    )
        : base(currentItemCount)
    {
        CurrentSkippedItemCount = currentSkippedItemCount;
        ByteCount = byteCount;
        CurrentRowIndex = currentRowIndex;
    }



    /// <summary>
    /// Gets the number of items skipped so far during loading.
    /// </summary>
    public int CurrentSkippedItemCount { get; }



    /// <summary>
    /// Gets the number of bytes written to the underlying stream so far.
    /// </summary>
    public long ByteCount { get; }



    /// <summary>
    /// Gets the current row index reported by the underlying CSV writer.
    /// </summary>
    public int CurrentRowIndex { get; }
}
