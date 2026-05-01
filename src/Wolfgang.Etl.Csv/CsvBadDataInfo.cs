namespace Wolfgang.Etl.Csv;

/// <summary>
/// Describes a bad-data event encountered while reading a CSV stream.
/// </summary>
/// <param name="LineNumber">
/// The 1-based file line on which the bad data was encountered, or <c>-1</c> if unknown.
/// Counts every physical line in the source, including comments and blank lines.
/// </param>
/// <param name="ColumnNumber">
/// The 1-based column position of the bad field within the line, or <c>-1</c> if unknown.
/// </param>
/// <param name="Field">The raw field value that was flagged as bad data, or <c>null</c> if unavailable.</param>
/// <param name="RawRecord">The full raw record (line) that contained the bad data.</param>
public sealed record CsvBadDataInfo
(
    int LineNumber,
    int ColumnNumber,
    string? Field,
    string RawRecord
);
