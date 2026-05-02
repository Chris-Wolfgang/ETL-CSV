namespace Wolfgang.Etl.Csv.Examples.DynamicTemplates;

/// <summary>
/// The fixed DTO every CSV layout binds into. The shape of this type does not
/// change between templates — only the column positions on disk do.
/// </summary>
public sealed record ProductRecord
{
    public string ProductNumber { get; set; } = string.Empty;

    public decimal RetailPrice { get; set; }

    public decimal MSRP { get; set; }
}
