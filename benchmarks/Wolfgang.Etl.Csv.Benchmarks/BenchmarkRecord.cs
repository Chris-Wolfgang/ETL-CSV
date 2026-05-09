namespace Wolfgang.Etl.Csv.Benchmarks;

public class BenchmarkRecord
{
    [CsvColumn(Name = "first_name")]
    public string FirstName { get; set; } = string.Empty;



    [CsvColumn(Name = "last_name")]
    public string LastName { get; set; } = string.Empty;



    [CsvColumn(Name = "city")]
    public string City { get; set; } = string.Empty;



    [CsvColumn(Name = "zip_code")]
    public int ZipCode { get; set; }



    [CsvColumn(Name = "age")]
    public int Age { get; set; }
}
