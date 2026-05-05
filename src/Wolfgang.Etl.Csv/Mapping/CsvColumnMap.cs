namespace Wolfgang.Etl.Csv;

/// <summary>
/// Runtime descriptor for a single property-to-column mapping. The parser-agnostic
/// equivalent of <see cref="CsvColumnAttribute"/> for cases where the layout isn't
/// known until runtime (for example, layout selected from configuration or a database).
/// </summary>
/// <param name="PropertyName">
/// The case-sensitive name of the property on the record type to bind. Must match
/// a public property declared on <c>TRecord</c>.
/// </param>
public sealed record CsvColumnMap(string PropertyName)
{
    /// <summary>
    /// The CSV column name to bind to. Ignored when <see cref="Index"/> is non-negative.
    /// </summary>
    public string? Name { get; init; }



    /// <summary>
    /// The 0-based column index to bind to. Use <c>-1</c> (the default) to bind by
    /// <see cref="Name"/> only.
    /// </summary>
    public int Index { get; init; } = -1;



    /// <summary>
    /// When <c>true</c>, missing columns do not cause read failures.
    /// </summary>
    public bool Optional { get; init; }



    /// <summary>
    /// Parse/format string applied when converting between CSV text and the property's
    /// type (e.g. <c>"yyyy-MM-dd"</c> for dates).
    /// </summary>
    public string? Format { get; init; }



    /// <summary>
    /// Default value used when the column is absent or empty during reading. The
    /// string is converted to the property's type.
    /// </summary>
    public string? Default { get; init; }
}
