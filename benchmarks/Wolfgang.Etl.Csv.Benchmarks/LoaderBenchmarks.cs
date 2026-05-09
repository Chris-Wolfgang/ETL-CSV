using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Csv.Benchmarks;

[MemoryDiagnoser]
public class LoaderBenchmarks
{
    private BenchmarkRecord[] _records = Array.Empty<BenchmarkRecord>();
    private string _filePath = string.Empty;



    [Params(1_000, 10_000, 100_000)]
    public int RecordCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _records = new BenchmarkRecord[RecordCount];
        for (var i = 0; i < RecordCount; i++)
        {
            _records[i] = new BenchmarkRecord
            {
                FirstName = "John",
                LastName = "Smith",
                City = "Seattle",
                ZipCode = 98101,
                Age = 42,
            };
        }

        _filePath = Path.Combine(Path.GetTempPath(), $"csv_bench_load_{RecordCount}.csv");
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
    // In-memory (MemoryStream) — isolates formatting cost from I/O
    // ------------------------------------------------------------------

    [Benchmark(Baseline = true)]
    public async Task Memory_TextWriter()
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        var loader = new CsvLoader<BenchmarkRecord>(writer);

        await loader.LoadAsync(ToAsyncEnumerable(_records));
        await writer.FlushAsync();
    }



    // ------------------------------------------------------------------
    // File-backed — shows real I/O effect of buffer sizing
    // ------------------------------------------------------------------

    [Benchmark]
    public async Task File_TextWriter_1KB()
    {
        using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096);
        using var writer = new StreamWriter(stream, bufferSize: 1024);
        var loader = new CsvLoader<BenchmarkRecord>(writer);

        await loader.LoadAsync(ToAsyncEnumerable(_records));
        await writer.FlushAsync();
    }



    [Benchmark]
    public async Task File_TextWriter_64KB()
    {
        using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536);
        using var writer = new StreamWriter(stream, bufferSize: 65536);
        var loader = new CsvLoader<BenchmarkRecord>(writer);

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
