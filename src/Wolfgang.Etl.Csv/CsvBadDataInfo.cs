namespace Wolfgang.Etl.Csv;

/// <summary>
/// Describes a bad-data event encountered while reading a CSV stream.
/// </summary>
/// <param name="RawRow">The 1-based raw line number where the bad data was found, or <c>-1</c> if unknown.</param>
/// <param name="Field">The raw field value that was flagged as bad data, or <c>null</c> if unavailable.</param>
/// <param name="RawRecord">The full raw record (line) that contained the bad data.</param>
public sealed record CsvBadDataInfo
(
    int RawRow,
    string? Field,
    string RawRecord
);
