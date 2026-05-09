using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Csv.Benchmarks;

[MemoryDiagnoser]
public class ExtractorBenchmarks
{
    private byte[] _data = Array.Empty<byte>();
    private string _filePath = string.Empty;



    [Params(1_000, 10_000, 100_000)]
    public int RecordCount { get; set; }



    [GlobalSetup]
    public async Task Setup()
    {
        var sb = new StringBuilder(RecordCount * 50);
        sb.AppendLine("first_name,last_name,city,zip_code,age");
        for (var i = 0; i < RecordCount; i++)
        {
            sb.AppendLine("John,Smith,Seattle,98101,42");
        }

        _data = Encoding.UTF8.GetBytes(sb.ToString());

        _filePath = Path.Combine(Path.GetTempPath(), $"csv_bench_extract_{RecordCount}.csv");
        await File.WriteAllBytesAsync(_filePath, _data);
    }



    [GlobalCleanup]
    public void Cleanup()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }



    // ------------------------------------------------------------------
    // In-memory (MemoryStream) — isolates parsing cost from I/O
    // ------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public async Task<int> Memory_TextReader()
    {
        using var reader = new StreamReader
        (
            new MemoryStream(_data),
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: false
        );
        var extractor = new CsvExtractor<BenchmarkRecord>(reader);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }



    // ------------------------------------------------------------------
    // File-backed — shows real I/O effect of buffer sizing
    // ------------------------------------------------------------------

    [Benchmark]
    public async Task<int> File_TextReader_1KB()
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024);
        var extractor = new CsvExtractor<BenchmarkRecord>(reader);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }



    [Benchmark]
    public async Task<int> File_TextReader_64KB()
    {
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 65536);
        var extractor = new CsvExtractor<BenchmarkRecord>(reader);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }
}
