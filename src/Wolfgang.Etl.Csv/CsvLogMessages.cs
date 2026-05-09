using System;
using Microsoft.Extensions.Logging;

namespace Wolfgang.Etl.Csv;

/// <summary>
/// Cached <see cref="LoggerMessage"/> delegates for high-performance structured logging
/// across the CSV extractor and loader.
/// </summary>
internal static class CsvLogMessages
{
    internal static readonly Action<ILogger, string, Exception?> StartingOperation =
        LoggerMessage.Define<string>(LogLevel.Debug, new EventId(1, nameof(StartingOperation)), "Starting {Operation}.");

    internal static readonly Action<ILogger, int, int, Exception?> SkippedItem =
        LoggerMessage.Define<int, int>(LogLevel.Debug, new EventId(2, nameof(SkippedItem)), "Skipped item {SkippedCount} of {SkipTotal}.");

    internal static readonly Action<ILogger, int, Exception?> ReachedMaximumItemCount =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(3, nameof(ReachedMaximumItemCount)), "Reached MaximumItemCount of {MaximumItemCount}. Stopping.");

    internal static readonly Action<ILogger, int, Exception?> ExtractedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(10, nameof(ExtractedItem)), "Extracted item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> ExtractionCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(11, nameof(ExtractionCompleted)), "CSV extraction completed. Extracted: {ItemCount}, skipped: {SkippedCount}.");

    internal static readonly Action<ILogger, int, Exception?> IgnoredRow =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(12, nameof(IgnoredRow)), "Ignored row {RawRowIndex} before InitialRecordIndex.");

    // Note: bad-data and reading-exception events are surfaced to callers via the
    // CsvExtractor.BadDataFound and CsvExtractor.ReadingExceptionOccurred Action<>
    // properties rather than being logged by the library. This keeps potentially-
    // sensitive CSV record contents out of application logs unless the caller
    // explicitly opts in by wiring those callbacks to their own logger.

    internal static readonly Action<ILogger, int, Exception?> LoadedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(20, nameof(LoadedItem)), "Loaded item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> LoadingCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(21, nameof(LoadingCompleted)), "CSV loading completed. Loaded: {ItemCount}, skipped: {SkippedCount}.");
}
