using System;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Configures how a property is mapped to a CSV column when read or written.
/// </summary>
/// <remarks>
/// This attribute lets consumers describe their CSV layout without depending on
/// any specific CSV parser. The CSV library translates these settings to the
/// underlying parser at runtime.
/// </remarks>
/// <example>
/// <code>
/// public record Person
/// {
///     [CsvColumn(Name = "first_name")]
///     public string FirstName { get; init; }
///
///     [CsvColumn(Name = "last_name")]
///     public string LastName { get; init; }
///
///     [CsvColumn(Name = "dob", Format = "yyyy-MM-dd", Optional = true, Default = "1970-01-01")]
///     public DateTime DateOfBirth { get; init; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CsvColumnAttribute : Attribute
{
    /// <summary>Initializes a new instance with no positional name.</summary>
    public CsvColumnAttribute()
    {
    }



    /// <summary>Initializes a new instance with the specified column name.</summary>
    /// <param name="name">The CSV column name to bind to.</param>
    public CsvColumnAttribute(string name)
    {
        Name = name;
    }



    /// <summary>
    /// Gets or sets the CSV column name to bind this property to.
    /// When <c>null</c>, the property name is used.
    /// </summary>
    public string? Name { get; init; }



    /// <summary>
    /// Gets or sets the 0-based column index to bind this property to.
    /// Use <c>-1</c> (the default) when binding by name only.
    /// </summary>
    public int Index { get; init; } = -1;



    /// <summary>
    /// Gets or sets a value indicating whether this column is optional.
    /// When <c>true</c>, missing columns do not cause read failures.
    /// </summary>
    public bool Optional { get; init; }



    /// <summary>
    /// Gets or sets a parse/format string applied when converting between the
    /// CSV text and this property's type (e.g. <c>"yyyy-MM-dd"</c> for dates).
    /// </summary>
    public string? Format { get; init; }



    /// <summary>
    /// Gets or sets a default value used when the column is absent or empty
    /// during reading. The string is converted to the property's type.
    /// </summary>
    public string? Default { get; init; }
}
