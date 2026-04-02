// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemInfo;

/// <summary>
/// Analyzes disk space usage. Finds the largest directories
/// to help users identify what is consuming storage.
/// </summary>
public sealed class DiskSpaceAnalyzer
{
    private readonly IZeroTraceLogger _logger;

    public event EventHandler<string>? ScanProgress;

    public DiskSpaceAnalyzer(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Find the top N largest directories under the given root path.
    /// Scans only the first level of subdirectories for performance.
    /// </summary>
    public async Task<List<DirectorySizeInfo>> GetLargestDirectoriesAsync(
        string rootPath,
        int topN = 20,
        CancellationToken ct = default)
    {
        _logger.Info($"Analysiere Speicherplatz: {rootPath}");

        if (!Directory.Exists(rootPath))
            return [];

        var results = new List<DirectorySizeInfo>();

        var dirs = Directory.GetDirectories(rootPath);
        for (int i = 0; i < dirs.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            var dir = dirs[i];
            ScanProgress?.Invoke(this, $"Scanne: {Path.GetFileName(dir)} ({i + 1}/{dirs.Length})");

            try
            {
                long size = await Task.Run(() => GetDirectorySize(dir), ct);
                if (size > 0)
                {
                    results.Add(new DirectorySizeInfo
                    {
                        Path = dir,
                        Name = Path.GetFileName(dir),
                        SizeBytes = size,
                        FileCount = CountFiles(dir)
                    });
                }
            }
            catch { /* skip inaccessible dirs */ }
        }

        var sorted = results
            .OrderByDescending(d => d.SizeBytes)
            .Take(topN)
            .ToList();

        _logger.Info($"Speicheranalyse: {sorted.Count} Ordner analysiert, " +
                     $"groesster: {sorted.FirstOrDefault()?.FormattedSize ?? "n/a"}");
        return sorted;
    }

    /// <summary>Get total disk usage summary for all fixed drives.</summary>
    public List<DriveSummary> GetDriveSummaries()
    {
        return DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(d => new DriveSummary
            {
                DriveLetter = d.Name,
                TotalBytes = d.TotalSize,
                FreeBytes = d.AvailableFreeSpace,
                UsedBytes = d.TotalSize - d.AvailableFreeSpace,
                Format = d.DriveFormat,
                Label = d.VolumeLabel
            })
            .ToList();
    }

    private static long GetDirectorySize(string path)
    {
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0L; } });
        }
        catch { return 0; }
    }

    private static int CountFiles(string path)
    {
        try { return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length; }
        catch { return 0; }
    }
}

public sealed class DirectorySizeInfo
{
    public required string Path      { get; init; }
    public required string Name      { get; init; }
    public required long   SizeBytes { get; init; }
    public required int    FileCount { get; init; }

    public string FormattedSize => SizeBytes switch
    {
        < 1024              => $"{SizeBytes} B",
        < 1024 * 1024       => $"{SizeBytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{SizeBytes / (1024.0 * 1024):F1} MB",
        _                   => $"{SizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

public sealed class DriveSummary
{
    public required string DriveLetter { get; init; }
    public required long   TotalBytes  { get; init; }
    public required long   FreeBytes   { get; init; }
    public required long   UsedBytes   { get; init; }
    public required string Format      { get; init; }
    public required string Label       { get; init; }

    public double UsagePercent =>
        TotalBytes > 0 ? (double)UsedBytes / TotalBytes * 100 : 0;
}
