using System;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Describes a recoverable parse exception encountered while reading a CSV stream.
/// Surfaced to the caller via <see cref="CsvExtractor{TRecord}.ReadingExceptionOccurred"/>
/// so logging, metrics, or quarantine logic can be plugged in without the library
/// making logging decisions on the caller's behalf.
/// </summary>
/// <remarks>
/// The underlying exception always propagates out of the async iterator after the
/// callback returns — this record is purely for observation. If the caller wants
/// extraction to continue past parse errors they must catch the exception around
/// their <c>await foreach</c>.
/// </remarks>
/// <param name="LineNumber">The 1-based file line where the exception was encountered, or <c>-1</c> if unknown.</param>
/// <param name="ColumnNumber">The 1-based column position of the offending field, or <c>-1</c> if unknown.</param>
/// <param name="ColumnName">The header name of the offending column, or <c>null</c> when unavailable (no header, or column index out of range).</param>
/// <param name="ColumnValue">The raw field value that failed to parse, or <c>null</c> when unavailable.</param>
/// <param name="Exception">The exception thrown by the underlying parser.</param>
public sealed record CsvReadingExceptionInfo
(
    int LineNumber,
    int ColumnNumber,
    string? ColumnName,
    string? ColumnValue,
    Exception Exception
);
