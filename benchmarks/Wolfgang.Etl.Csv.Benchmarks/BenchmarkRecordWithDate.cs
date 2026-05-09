using System;

namespace Wolfgang.Etl.Csv.Benchmarks;

public class BenchmarkRecordWithDate
{
    [CsvColumn(Name = "first_name")]
    public string FirstName { get; set; } = string.Empty;



    [CsvColumn(Name = "last_name")]
    public string LastName { get; set; } = string.Empty;



    [CsvColumn(Name = "birth_date", Format = "yyyy-MM-dd")]
    public DateTime BirthDate { get; set; }



    [CsvColumn(Name = "zip_code")]
    public int ZipCode { get; set; }
}
