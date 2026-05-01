using System;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Marks a property to be excluded from CSV mapping.
/// </summary>
/// <remarks>
/// Properties decorated with this attribute are neither read from nor written
/// to the CSV stream. Useful for computed, transient, or sensitive properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class CsvIgnoreAttribute : Attribute
{
}
