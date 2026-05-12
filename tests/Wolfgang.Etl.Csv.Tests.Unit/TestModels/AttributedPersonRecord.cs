using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.Etl.Csv.Tests.Unit.TestModels;

[ExcludeFromCodeCoverage]
public record AttributedPersonRecord
{
    [CsvColumn(Name = "first_name")]
    public string FirstName { get; set; } = string.Empty;



    [CsvColumn(Name = "last_name")]
    public string LastName { get; set; } = string.Empty;



    [CsvColumn(Name = "age")]
    public int Age { get; set; }



    [CsvIgnore]
    public string ComputedDisplayName { get; set; } = string.Empty;
}
