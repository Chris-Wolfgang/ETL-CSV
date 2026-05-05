namespace Wolfgang.Etl.Csv.Examples.DynamicTemplates;

/// <summary>
/// Describes a CSV layout. Loaded from JSON in this example, but in production
/// would typically be loaded from a database table keyed by template name.
/// </summary>
/// <remarks>
/// All column positions are <b>1-based</b> to match how a non-developer would
/// describe a spreadsheet ("the product number is in column 5"). The example
/// converts to 0-based when handing the values to the library.
/// </remarks>
public sealed record CsvTemplate
{
    /// <summary>The name used to look up this template at runtime.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>Path (relative to the example) of the CSV file to read.</summary>
    public string CsvFile { get; init; } = string.Empty;

    /// <summary>The 1-based row number on which data begins (header/metadata rows above are skipped).</summary>
    public int StartRow { get; init; } = 1;

    /// <summary>The 1-based column position of the product number.</summary>
    public int ProductNumberColumn { get; init; }

    /// <summary>The 1-based column position of the retail price.</summary>
    public int RetailPriceColumn { get; init; }

    /// <summary>The 1-based column position of the MSRP.</summary>
    public int MsrpColumn { get; init; }
}
