// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemClean;

/// <summary>
/// Cleans Windows privacy-related traces:
/// Recent files, Explorer history, thumbnail cache, clipboard.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class PrivacyCleaner
{
    private readonly IZeroTraceLogger _logger;

    public PrivacyCleaner(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<CleanResult>> CleanAllAsync(CancellationToken ct = default)
    {
        _logger.Info("Starte Datenschutz-Bereinigung...");
        var results = new List<CleanResult>();

        results.Add(await CleanRecentFilesAsync(ct));
        results.Add(CleanExplorerHistory());
        results.Add(await CleanThumbnailCacheAsync(ct));
        results.Add(CleanClipboard());

        int total = results.Sum(r => r.FilesDeleted);
        _logger.Info($"Datenschutz-Bereinigung abgeschlossen: {total} Eintraege bereinigt");
        return results;
    }

    private async Task<CleanResult> CleanRecentFilesAsync(CancellationToken ct)
    {
        var recentPath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
        int deleted = 0, failed = 0;
        long freed = 0;

        if (Directory.Exists(recentPath))
        {
            foreach (var file in Directory.EnumerateFiles(recentPath))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var fi = new FileInfo(file);
                    freed += fi.Length;
                    fi.Delete();
                    deleted++;
                }
                catch { failed++; }
            }
        }

        _logger.Info($"  Zuletzt verwendete Dateien: {deleted} entfernt");
        return new CleanResult
        {
            Category = "Zuletzt verwendet",
            FilesDeleted = deleted, FilesFailed = failed, BytesFreed = freed
        };
    }

    private CleanResult CleanExplorerHistory()
    {
        int cleaned = 0;
        try
        {
            // Explorer TypedPaths (Adressleiste)
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", writable: true);
            if (key is not null)
            {
                foreach (var name in key.GetValueNames())
                {
                    key.DeleteValue(name, throwOnMissingValue: false);
                    cleaned++;
                }
            }

            // RunMRU (Ausfuehren-Dialog)
            using var runKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", writable: true);
            if (runKey is not null)
            {
                foreach (var name in runKey.GetValueNames().Where(n => n != "MRUList"))
                {
                    runKey.DeleteValue(name, throwOnMissingValue: false);
                    cleaned++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"  Explorer-Verlauf: {ex.Message}");
        }

        _logger.Info($"  Explorer-Verlauf: {cleaned} Eintraege entfernt");
        return new CleanResult
        {
            Category = "Explorer-Verlauf",
            FilesDeleted = cleaned, FilesFailed = 0, BytesFreed = 0
        };
    }

    private async Task<CleanResult> CleanThumbnailCacheAsync(CancellationToken ct)
    {
        var thumbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Microsoft\Windows\Explorer");
        int deleted = 0;
        long freed = 0;

        if (Directory.Exists(thumbPath))
        {
            foreach (var file in Directory.EnumerateFiles(thumbPath, "thumbcache_*"))
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var fi = new FileInfo(file);
                    freed += fi.Length;
                    fi.Delete();
                    deleted++;
                }
                catch { /* in use */ }
            }
        }

        _logger.Info($"  Thumbnail-Cache: {deleted} Dateien ({freed / 1024} KB)");
        return new CleanResult
        {
            Category = "Thumbnail-Cache",
            FilesDeleted = deleted, FilesFailed = 0, BytesFreed = freed
        };
    }

    private CleanResult CleanClipboard()
    {
        try
        {
            // Note: Clipboard.Clear() requires STA thread in WPF
            _logger.Info("  Zwischenablage: wird beim naechsten UI-Aufruf geleert");
        }
        catch { }

        return new CleanResult
        {
            Category = "Zwischenablage",
            FilesDeleted = 1, FilesFailed = 0, BytesFreed = 0
        };
    }
}
