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

    // Privacy note: the default no-callback log paths could otherwise emit PII from
    // CSV records into application logs. We mitigate two ways:
    //   * BadDataFound logs at Debug — production loggers won't capture it unless
    //     the operator explicitly turns up verbosity. Devs/troubleshooters who
    //     enable Debug see the raw record for diagnosis. Callers who want louder
    //     behaviour can set CsvExtractor.BadDataFound and route the events themselves.
    //   * ReadingExceptionOccurred logs at Warning (it's a recoverable parse error)
    //     but the format omits {RawRecord} — only row/column metadata and the
    //     specific bad value are emitted. The raw record can still be inspected
    //     by setting CsvExtractor.BadDataFound (the bad-data case) or by examining
    //     the inner exception's CsvHelperException.Context in the caller's catch.

    internal static readonly Action<ILogger, int, string?, string, Exception?> BadDataFound =
        LoggerMessage.Define<int, string?, string>(LogLevel.Debug, new EventId(13, nameof(BadDataFound)), "Bad data found on raw row {RawRowIndex} with field '{Field}'. Raw record: {RawRecord}.");

    internal static readonly Action<ILogger, int, int, int, string?, string?, Exception?> ReadingExceptionOccurred =
        LoggerMessage.Define<int, int, int, string?, string?>(LogLevel.Warning, new EventId(14, nameof(ReadingExceptionOccurred)), "Error parsing row {RowIndex} (raw {RawRowIndex}), column index {ColumnIndex} ('{ColumnName}'), value '{ColumnValue}'.");

    internal static readonly Action<ILogger, int, Exception?> LoadedItem =
        LoggerMessage.Define<int>(LogLevel.Debug, new EventId(20, nameof(LoadedItem)), "Loaded item {CurrentItemCount}.");

    internal static readonly Action<ILogger, int, int, Exception?> LoadingCompleted =
        LoggerMessage.Define<int, int>(LogLevel.Information, new EventId(21, nameof(LoadingCompleted)), "CSV loading completed. Loaded: {ItemCount}, skipped: {SkippedCount}.");
}
