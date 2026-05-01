using System;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Specifies how whitespace should be trimmed when reading or writing CSV fields.
/// </summary>
[Flags]
public enum CsvTrimOptions
{
    /// <summary>No trimming is performed.</summary>
    None = 0,

    /// <summary>Trims the whitespace surrounding a field.</summary>
    Trim = 1,

    /// <summary>Trims the whitespace inside of quotes around a field.</summary>
    InsideQuotes = 2,
}
