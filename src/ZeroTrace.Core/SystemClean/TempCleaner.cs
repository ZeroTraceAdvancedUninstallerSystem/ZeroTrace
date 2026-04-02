// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemClean;

/// <summary>
/// Cleans temporary files from system and user temp directories.
/// Reports how much space was freed.
/// </summary>
public sealed class TempCleaner
{
    private readonly IZeroTraceLogger _logger;

    public TempCleaner(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CleanResult> CleanSystemTempAsync(CancellationToken ct = default)
    {
        _logger.Info("Starte Temp-Bereinigung...");

        var paths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            @"C:\Windows\Temp",
            Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
        };

        int deleted = 0, failed = 0;
        long freed = 0;

        foreach (var path in paths.Where(Directory.Exists).Distinct())
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        // Skip files modified in the last hour (might be in use)
                        if (fi.LastWriteTimeUtc > DateTime.UtcNow.AddHours(-1)) continue;

                        long size = fi.Length;
                        fi.Delete();
                        freed += size;
                        deleted++;
                    }
                    catch { failed++; }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Temp-Scan uebersprungen: {path} - {ex.Message}");
            }
        }

        _logger.Info($"Temp-Bereinigung: {deleted} geloescht, {failed} uebersprungen, {FormatSize(freed)} frei");

        return new CleanResult
        {
            Category = "Temp-Dateien",
            FilesDeleted = deleted,
            FilesFailed = failed,
            BytesFreed = freed
        };
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024              => $"{bytes} B",
        < 1024 * 1024       => $"{bytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _                   => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

/// <summary>Result of any cleaning operation.</summary>
public sealed class CleanResult
{
    public required string Category     { get; init; }
    public required int    FilesDeleted { get; init; }
    public required int    FilesFailed  { get; init; }
    public required long   BytesFreed   { get; init; }

    public string FormattedFreed => BytesFreed switch
    {
        < 1024              => $"{BytesFreed} B",
        < 1024 * 1024       => $"{BytesFreed / 1024.0:F1} KB",
        _                   => $"{BytesFreed / (1024.0 * 1024):F1} MB"
    };
}
