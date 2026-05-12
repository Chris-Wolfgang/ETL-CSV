using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Csv.Benchmarks;

/// <summary>
/// Exercises the reader/writer hot paths with a record that contains a
/// <see cref="DateTime"/> field, isolating CsvHelper's date parse/format path.
/// </summary>
[MemoryDiagnoser]
public class DateTimeBenchmarks
{
    private byte[] _data = Array.Empty<byte>();
    private BenchmarkRecordWithDate[] _records = Array.Empty<BenchmarkRecordWithDate>();



    [Params(10_000)]
    public int RecordCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _records = new BenchmarkRecordWithDate[RecordCount];
        var sb = new StringBuilder(RecordCount * 50);
        sb.AppendLine("first_name,last_name,birth_date,zip_code");
        for (var i = 0; i < RecordCount; i++)
        {
            var dob = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).AddDays(i % 10000);
            _records[i] = new BenchmarkRecordWithDate
            {
                FirstName = "John",
                LastName = "Smith",
                BirthDate = dob,
                ZipCode = 98101,
            };

            sb.Append("John,Smith,");
            sb.Append(dob.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            sb.AppendLine(",98101");
        }

        _data = Encoding.UTF8.GetBytes(sb.ToString());
    }



    [Benchmark]
    public async Task<int> Extract_Memory()
    {
        using var reader = new StreamReader(new MemoryStream(_data), Encoding.UTF8);
        var extractor = new CsvExtractor<BenchmarkRecordWithDate>(reader);
        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }
        return count;
    }



    [Benchmark]
    public async Task Load_Memory()
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        var loader = new CsvLoader<BenchmarkRecordWithDate>(writer);
        await loader.LoadAsync(ToAsyncEnumerable(_records));
        await writer.FlushAsync();
    }



    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return item;
        }
        await Task.CompletedTask;
    }
}
