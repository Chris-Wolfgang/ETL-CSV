using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Wolfgang.Etl.Csv.Benchmarks;

/// <summary>
/// Measures the GC.GetTotalMemory delta between just before extraction starts and
/// just after the final record is yielded. This is a "memory pressure" / steady-state
/// retention proxy, not a true peak — peak transient allocations are reported by
/// the BenchmarkDotNet <c>MemoryDiagnoser</c> attribute (see "Allocated" column).
/// </summary>
[MemoryDiagnoser]
public class MemoryDeltaBenchmarks
{
    private byte[][] _dataBySize = Array.Empty<byte[]>();



    [Params(0, 1, 1_000, 10_000, 100_000, 1_000_000)]
    public int RecordCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _dataBySize = new byte[1][];
        _dataBySize[0] = BuildData(RecordCount);
    }



    private static byte[] BuildData(int count)
    {
        var sb = new StringBuilder(Math.Max(64, count * 50));
        sb.AppendLine("first_name,last_name,city,zip_code,age");
        for (var i = 0; i < count; i++)
        {
            sb.AppendLine("John,Smith,Seattle,98101,42");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }



    [Benchmark]
    public async Task<long> Extract_MemoryDelta()
    {
        var before = GC.GetTotalMemory(forceFullCollection: true);

        using var reader = new StreamReader(new MemoryStream(_dataBySize[0]), Encoding.UTF8);
        var extractor = new CsvExtractor<BenchmarkRecord>(reader);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        var after = GC.GetTotalMemory(forceFullCollection: true);
        return after - before;
    }
}
