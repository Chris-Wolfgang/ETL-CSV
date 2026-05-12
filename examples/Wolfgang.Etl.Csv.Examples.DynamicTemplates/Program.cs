using System.Text.Json;
using Wolfgang.Etl.Csv;
using Wolfgang.Etl.Csv.Examples.DynamicTemplates;

// CA1849 / S6966 (call async methods in async context) and MA0004 (ConfigureAwait)
// are overly strict for a demo entry point that's chiefly Console.WriteLine output.
// Async console I/O and ConfigureAwait noise would obscure what the example is teaching.
#pragma warning disable CA1849, S6966, MA0004

if (args.Length == 0)
{
    PrintUsage();
    return 1;
}

var templateName = args[0];

// In production, this would be a SQL lookup. For the example we read JSON.
var templates = await LoadTemplatesAsync("templates.json");
var template = templates.FirstOrDefault
(
    t => string.Equals(t.Name, templateName, StringComparison.OrdinalIgnoreCase)
);

if (template is null)
{
    Console.Error.WriteLine($"No template named '{templateName}'. Known templates:");
    foreach (var t in templates)
    {
        Console.Error.WriteLine($"  - {t.Name}");
    }
    return 2;
}

Console.WriteLine($"Template:    {template.Name}");
if (!string.IsNullOrWhiteSpace(template.Description))
{
    Console.WriteLine($"Description: {template.Description}");
}
Console.WriteLine($"CSV file:    {template.CsvFile}");
Console.WriteLine($"Start row:   {template.StartRow}");
Console.WriteLine
(
    $"Columns:     ProductNumber=col {template.ProductNumberColumn}, " +
    $"RetailPrice=col {template.RetailPriceColumn}, " +
    $"MSRP=col {template.MsrpColumn}"
);
Console.WriteLine();

await ReadAndPrintAsync(template);

return 0;



static async Task<IReadOnlyList<CsvTemplate>> LoadTemplatesAsync(string path)
{
    await using var stream = File.OpenRead(path);
    var list = await JsonSerializer.DeserializeAsync<List<CsvTemplate>>
    (
        stream,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
    );
    return list ?? new List<CsvTemplate>();
}



static async Task ReadAndPrintAsync(CsvTemplate template)
{
    using var reader = new StreamReader(template.CsvFile);

    var extractor = new CsvExtractor<ProductRecord>(reader)
    {
        // Start reading on the first data row; everything above is skipped.
        InitialRecordIndex = template.StartRow,

        // We bind by column position, so headers (if any) are not used.
        HasHeaderRecord = false,

        // The runtime mapping descriptor — built fresh from the template each run.
        // Convert the human-friendly 1-based column positions to the library's 0-based Index.
        ColumnMaps = new[]
        {
            new CsvColumnMap(nameof(ProductRecord.ProductNumber)) { Index = template.ProductNumberColumn - 1 },
            new CsvColumnMap(nameof(ProductRecord.RetailPrice))   { Index = template.RetailPriceColumn   - 1 },
            new CsvColumnMap(nameof(ProductRecord.MSRP))          { Index = template.MsrpColumn          - 1 },
        },
    };

    Console.WriteLine($"{"ProductNumber",-15} {"RetailPrice",12} {"MSRP",12}");
    Console.WriteLine(new string('-', 41));

    var rowCount = 0;
    await foreach (var row in extractor.ExtractAsync())
    {
        Console.WriteLine
        (
            $"{row.ProductNumber,-15} {row.RetailPrice,12:C} {row.MSRP,12:C}"
        );
        rowCount++;
    }

    Console.WriteLine(new string('-', 41));
    Console.WriteLine($"{rowCount} row(s) extracted.");
}



static void PrintUsage()
{
    Console.Error.WriteLine("Usage: dynamictemplates <template-name>");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Reads a CSV file using a runtime-selected template (loaded from templates.json).");
    Console.Error.WriteLine("The template specifies the start row and the 1-based column positions for");
    Console.Error.WriteLine("ProductNumber, RetailPrice, and MSRP. Other columns in the file are ignored.");
    Console.Error.WriteLine();
    Console.Error.WriteLine("Example: dotnet run -- supplier-a");
}
