using System;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Provides per-field context to a <c>ShouldQuote</c> callback while writing a CSV stream.
/// </summary>
/// <param name="Field">The field value being written.</param>
/// <param name="FieldType">The declared type of the field, when known.</param>
public sealed record CsvShouldQuoteContext
(
    string? Field,
    Type? FieldType
);
