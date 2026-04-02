// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemClean;

/// <summary>
/// Cleans browser caches, cookies, and history data.
/// Supports Chrome, Edge, Firefox, Brave, Opera.
/// </summary>
public sealed class BrowserCleaner
{
    private readonly IZeroTraceLogger _logger;

    public BrowserCleaner(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<CleanResult>> CleanAllBrowsersAsync(CancellationToken ct = default)
    {
        _logger.Info("Starte Browser-Reinigung...");
        var results = new List<CleanResult>();

        var local   = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Chromium-based browsers
        var chromiumBrowsers = new Dictionary<string, string>
        {
            ["Chrome"]  = Path.Combine(local, @"Google\Chrome\User Data"),
            ["Edge"]    = Path.Combine(local, @"Microsoft\Edge\User Data"),
            ["Brave"]   = Path.Combine(local, @"BraveSoftware\Brave-Browser\User Data"),
            ["Opera"]   = Path.Combine(roaming, @"Opera Software\Opera Stable"),
        };

        foreach (var (name, basePath) in chromiumBrowsers)
        {
            ct.ThrowIfCancellationRequested();
            if (!Directory.Exists(basePath)) continue;

            var cachePaths = new[]
            {
                Path.Combine(basePath, "Default", "Cache"),
                Path.Combine(basePath, "Default", "Code Cache"),
                Path.Combine(basePath, "Default", "GPUCache"),
                Path.Combine(basePath, "Default", "Service Worker", "CacheStorage"),
            };

            var result = await CleanPathsAsync(name, cachePaths, ct);
            results.Add(result);
        }

        // Firefox
        var firefoxBase = Path.Combine(roaming, @"Mozilla\Firefox\Profiles");
        if (Directory.Exists(firefoxBase))
        {
            var cachePaths = Directory.GetDirectories(firefoxBase)
                .SelectMany(profile => new[]
                {
                    Path.Combine(profile, "cache2"),
                    Path.Combine(profile, "startupCache"),
                })
                .ToArray();

            var result = await CleanPathsAsync("Firefox", cachePaths, ct);
            results.Add(result);
        }

        int totalFreed = results.Sum(r => (int)(r.BytesFreed / 1024));
        _logger.Info($"Browser-Reinigung abgeschlossen: {totalFreed} KB frei");
        return results;
    }

    private async Task<CleanResult> CleanPathsAsync(
        string browserName, string[] paths, CancellationToken ct)
    {
        int deleted = 0, failed = 0;
        long freed = 0;

        foreach (var path in paths.Where(Directory.Exists))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        var fi = new FileInfo(file);
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
                _logger.Debug($"  {browserName}: {path} - {ex.Message}");
            }
        }

        if (deleted > 0)
            _logger.Info($"  {browserName}: {deleted} Dateien geloescht ({freed / 1024} KB)");

        return new CleanResult
        {
            Category = browserName,
            FilesDeleted = deleted,
            FilesFailed = failed,
            BytesFreed = freed
        };
    }
}
