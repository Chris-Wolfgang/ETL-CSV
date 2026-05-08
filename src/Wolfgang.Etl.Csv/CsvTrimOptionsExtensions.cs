using CsvHelperTrimOptions = CsvHelper.Configuration.TrimOptions;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Translation helpers between the public <see cref="CsvTrimOptions"/> and the
/// underlying CsvHelper flag set.
/// </summary>
internal static class CsvTrimOptionsExtensions
{
    /// <summary>
    /// Translates a <see cref="CsvTrimOptions"/> value into the underlying CsvHelper
    /// flag set. Done by explicit flag-by-flag mapping so the public enum is not
    /// coupled to CsvHelper's numeric values; either enum can change independently
    /// without silently corrupting the other.
    /// </summary>
    public static CsvHelperTrimOptions ToCsvHelper(this CsvTrimOptions options)
    {
        var result = CsvHelperTrimOptions.None;

        if ((options & CsvTrimOptions.Trim) == CsvTrimOptions.Trim)
        {
            result |= CsvHelperTrimOptions.Trim;
        }

        if ((options & CsvTrimOptions.InsideQuotes) == CsvTrimOptions.InsideQuotes)
        {
            result |= CsvHelperTrimOptions.InsideQuotes;
        }

        return result;
    }
}
