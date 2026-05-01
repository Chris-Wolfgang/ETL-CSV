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

    internal static readonly Action<ILogger, int, string?, string, Exception?> BadDataFound =
        LoggerMessage.Define<int, string?, string>(LogLevel.Error, new EventId(13, nameof(BadDataFound)), "Bad data found on raw row {RawRowIndex} with field '{Field}'. Raw record: {RawRecord}.");

    internal static readonly Action<ILogger, int, int, int, string?, string?, string, Exception?> ReadingExceptionOccurred =
        LoggerMessage.Define<int, int, int, string?, string?, string>(LogLevel.Error, new EventId(14, nameof(ReadingExceptionOccurred)), "Error parsing row {RowIndex} (raw {RawRowIndex}), column index {ColumnIndex} ('{ColumnName}'), value '{ColumnValue}'. Raw record: {RawRecord}.");

    internal static readonly Action<ILogger, int, Exception?> LoadedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(20, nameof(LoadedItem)), "Loaded item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> LoadingCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(21, nameof(LoadingCompleted)), "CSV loading completed. Loaded: {ItemCount}, skipped: {SkippedCount}.");
}
