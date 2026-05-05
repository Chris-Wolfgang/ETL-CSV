# Wolfgang.Etl.Csv

A `CsvExtractor<T>` and `CsvLoader<T>` for streaming CSV files into and out of strongly-typed records, built on [`Wolfgang.Etl.Abstractions`](https://github.com/Chris-Wolfgang/ETL-Abstractions) and powered internally by [CsvHelper](https://joshclose.github.io/CsvHelper/). Supports compile-time and runtime column mapping, progress reporting with line numbers and bad-data counts, and an async-first I/O model.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-CSV)

---

## 📦 Installation

```bash
dotnet add package Wolfgang.Etl.Csv
```

**NuGet Package:** Coming soon to NuGet.org

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/ETL-CSV](https://github.com/Chris-Wolfgang/ETL-CSV)
- **API Documentation:** https://Chris-Wolfgang.github.io/ETL-CSV/
- **Formatting Guide:** [README-FORMATTING.md](README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 🚀 Quick Start

Define a record type, then read or write CSV with a single `await foreach` / `await LoadAsync(...)`.

### Reading a CSV

```csharp
using System.IO;
using Wolfgang.Etl.Csv;

public sealed record Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;
    public int    Age       { get; set; }
}

using var reader = new StreamReader("people.csv");
var extractor = new CsvExtractor<Person>(reader);

await foreach (var person in extractor.ExtractAsync(cancellationToken))
{
    Console.WriteLine($"{person.FirstName} {person.LastName} ({person.Age})");
}
```

### Writing a CSV

```csharp
using var writer = new StreamWriter("people.csv");
var loader = new CsvLoader<Person>(writer);

var people = new[]
{
    new Person { FirstName = "Alice", LastName = "Smith", Age = 30 },
    new Person { FirstName = "Bob",   LastName = "Jones", Age = 25 },
}.ToAsyncEnumerable();

await loader.LoadAsync(people, cancellationToken);
```

That's the full surface for the simplest case. The library auto-maps record properties to CSV columns by name; everything below is opt-in.

---

## ✨ Features

| Feature | Where it lives |
|---|---|
| Async-first read/write of CSV streams | `CsvExtractor<T>`, `CsvLoader<T>` |
| Multi-targeted: net462 → net10.0 | csproj `<TargetFrameworks>` |
| Progress reporting with item count, skip count, line number, bad-data count | `CsvExtractorProgress`, `CsvLoaderProgress` |
| Cancellation support throughout | `CancellationToken` on every async API |
| Skip metadata rows / start at a specific line | `InitialRecordIndex` |
| Limit how many records get processed | `MaxRecordCount`, `SkipRecordCount` |
| Compile-time column mapping (declarative) | `[CsvColumn]`, `[CsvIgnore]` |
| Runtime column mapping (configuration / DB-driven layouts) | `CsvColumnMap`, `ColumnMaps` property |
| Parser-agnostic public surface (no `CsvHelper` types leaked) | `CsvTrimOptions`, `CsvBadDataInfo`, `CsvShouldQuoteContext` |
| Custom delimiter, quote, escape, comment character | `Delimiter`, `Quote`, `Escape`, `Comment`, `AllowComments` |
| Encoding control | `Encoding` |
| `LeaveOpen` semantics matching .NET conventions | `LeaveOpen` |
| Trim whitespace inside or outside quotes | `TrimOptions` |
| Custom bad-data and quoting callbacks | `BadDataFound`, `ShouldQuote` |

### Example: Compile-time column mapping with `[CsvColumn]`

When the CSV's column names don't match your record's property names, decorate properties:

```csharp
public sealed record Person
{
    [CsvColumn(Name = "first_name")]
    public string FirstName { get; set; } = string.Empty;

    [CsvColumn(Name = "last_name")]
    public string LastName { get; set; } = string.Empty;

    [CsvColumn(Name = "dob", Format = "yyyy-MM-dd", Optional = true, Default = "1970-01-01")]
    public DateTime DateOfBirth { get; set; }

    [CsvIgnore]
    public string ComputedDisplayName { get; set; } = string.Empty;
}
```

`CsvColumnAttribute` accepts `Name`, `Index` (0-based), `Optional`, `Format`, and `Default`. `[CsvIgnore]` excludes a property entirely. Records without any of these attributes fall back to CsvHelper's default by-name mapping.

### Example: Runtime column mapping (when the layout isn't known at compile time)

When column positions come from configuration or a database — for example, several supplier files with different column orders all binding into the same DTO — set `ColumnMaps` instead of (or in addition to) attributes:

```csharp
var template = LoadTemplateFromDb("supplier-a"); // 1-based positions per the user's domain

var extractor = new CsvExtractor<ProductRecord>(reader)
{
    InitialRecordIndex = template.StartRow,
    HasHeaderRecord = false,
    ColumnMaps = new[]
    {
        new CsvColumnMap(nameof(ProductRecord.ProductNumber)) { Index = template.ProductNumberColumn - 1 },
        new CsvColumnMap(nameof(ProductRecord.RetailPrice))   { Index = template.RetailPriceColumn   - 1 },
        new CsvColumnMap(nameof(ProductRecord.MSRP))          { Index = template.MsrpColumn          - 1 },
    },
};
```

When `ColumnMaps` is non-null and non-empty, runtime maps override any attribute-based mapping for that instance. See the runnable example under [`examples/Wolfgang.Etl.Csv.Examples.DynamicTemplates/`](examples/Wolfgang.Etl.Csv.Examples.DynamicTemplates/).

### Example: Progress reporting

```csharp
var progress = new Progress<CsvExtractorProgress>(p =>
    Console.WriteLine($"Item {p.CurrentItemCount} (line {p.CurrentLineNumber}); skipped {p.CurrentSkippedItemCount}; bad data {p.CurrentBadDataCount}"));

await foreach (var person in extractor.ExtractAsync(progress, cancellationToken))
{
    // ...
}
```

`ReportingInterval` (inherited from `ExtractorBase`) controls how often progress is sampled.

---

## 🎯 Target Frameworks

| Framework | Versions |
|-----------|----------|
| .NET Framework | .NET 4.6.2, .NET 4.7.0, .NET 4.7.1, .NET 4.7.2, .NET 4.8, .NET 4.8.1 |
| .NET Core | .NET Core 3.1 |
| .NET | .NET 5.0, .NET 6.0, .NET 7.0, .NET 8.0, .NET 9.0, .NET 10.0 |

---

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers** and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This library uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- ❌ `Task.Wait()`, `Task.Result` - Use `await` instead
- ❌ `Thread.Sleep()` - Use `await Task.Delay()` instead
- ❌ Synchronous file I/O (`File.ReadAllText`) - Use async versions
- ❌ Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- ❌ `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- ❌ Obsolete APIs (`WebClient`, `BinaryFormatter`)

**Why?** To ensure all code is **truly async** and **non-blocking** for optimal performance in async contexts.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- Optional: [PowerShell Core](https://github.com/PowerShell/PowerShell) for formatting scripts

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-CSV.git
cd ETL-CSV

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting (PowerShell Core)
pwsh ./format.ps1
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting (as CI does)
dotnet format --verify-no-changes
```

See [README-FORMATTING.md](README-FORMATTING.md) for detailed formatting guidelines.

### Building Documentation

This project uses [DocFX](https://dotnet.github.io/docfx/) to generate API documentation:

```bash
# Install DocFX (one-time setup)
dotnet tool install -g docfx

# Generate API metadata and build documentation
cd docfx_project
docfx metadata  # Extract API metadata from source code
docfx build     # Build HTML documentation

# Documentation is generated in the docs/ folder at the repository root
```

The documentation is automatically built and deployed to GitHub Pages when changes are pushed to the `main` branch.

**Local Preview:**
```bash
# Serve documentation locally (with live reload)
cd docfx_project
docfx build --serve

# Open http://localhost:8080 in your browser
```

**Documentation Structure:**
- `docfx_project/` - DocFX configuration and source files
- `docs/` - Generated HTML documentation (published to GitHub Pages)
- `docfx_project/index.md` - Main landing page content
- `docfx_project/docs/` - Additional documentation articles
- `docfx_project/api/` - Auto-generated API reference YAML files

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details

---


## 🙏 Acknowledgments

- [**CsvHelper**](https://joshclose.github.io/CsvHelper/) by Josh Close — the underlying CSV parser and writer. Dual-licensed under MS-PL and Apache 2.0; both are MIT-compatible.
- [**Wolfgang.Etl.Abstractions**](https://github.com/Chris-Wolfgang/ETL-Abstractions) — the `ExtractorBase<T, TProgress>` / `LoaderBase<T, TProgress>` contracts this library implements.
- [**Wolfgang.Etl.TestKit.Xunit**](https://github.com/Chris-Wolfgang/ETL-Test-Kit) — provides the `ExtractorBaseContractTests` / `LoaderBaseContractTests` used to verify behavioural conformance.

