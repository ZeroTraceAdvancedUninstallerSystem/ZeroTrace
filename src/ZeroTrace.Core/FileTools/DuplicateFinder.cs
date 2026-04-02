// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Security.Cryptography;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.FileTools;

/// <summary>
/// Finds duplicate files by comparing SHA-256 hashes.
/// Groups files by size first for performance, then hashes only same-size files.
/// </summary>
public sealed class DuplicateFinder
{
    private readonly IZeroTraceLogger _logger;

    public event EventHandler<DuplicateScanProgress>? ProgressChanged;

    public DuplicateFinder(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Find all duplicate files in the given directories.</summary>
    public async Task<List<DuplicateGroup>> FindDuplicatesAsync(
        IEnumerable<string> directories,
        long minSizeBytes = 1024, // ignore files < 1 KB
        CancellationToken ct = default)
    {
        _logger.Info("Starte Duplikat-Suche...");

        // Step 1: Collect all files grouped by size
        var bySize = new Dictionary<long, List<string>>();
        int totalFiles = 0;

        foreach (var dir in directories.Where(Directory.Exists))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        var fi = new FileInfo(file);
                        if (fi.Length < minSizeBytes) continue;

                        if (!bySize.TryGetValue(fi.Length, out var list))
                        {
                            list = [];
                            bySize[fi.Length] = list;
                        }
                        list.Add(file);
                        totalFiles++;
                    }
                    catch { /* skip inaccessible */ }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Verzeichnis uebersprungen: {dir} - {ex.Message}");
            }
        }

        // Step 2: Only hash files that share the same size
        var candidates = bySize.Where(kv => kv.Value.Count > 1).ToList();
        _logger.Info($"  {totalFiles} Dateien gescannt, {candidates.Sum(c => c.Value.Count)} Kandidaten");

        var duplicates = new List<DuplicateGroup>();
        int processed = 0;

        foreach (var (size, files) in candidates)
        {
            ct.ThrowIfCancellationRequested();
            var hashGroups = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    var hash = await ComputeHashAsync(file, ct);
                    if (!hashGroups.TryGetValue(hash, out var group))
                    {
                        group = [];
                        hashGroups[hash] = group;
                    }
                    group.Add(file);
                }
                catch { /* skip */ }

                processed++;
                Report($"Prüfe: {Path.GetFileName(file)}", candidates.Sum(c => c.Value.Count), processed);
            }

            foreach (var (hash, group) in hashGroups.Where(g => g.Value.Count > 1))
            {
                duplicates.Add(new DuplicateGroup
                {
                    Hash = hash,
                    FileSize = size,
                    Files = group.AsReadOnly(),
                    WastedBytes = size * (group.Count - 1)
                });
            }
        }

        long totalWasted = duplicates.Sum(d => d.WastedBytes);
        _logger.Info($"Duplikat-Suche abgeschlossen: {duplicates.Count} Gruppen, {totalWasted / (1024 * 1024)} MB verschwendet");
        return duplicates;
    }

    private static async Task<string> ComputeHashAsync(string path, CancellationToken ct)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash);
    }

    private void Report(string msg, int total, int current) =>
        ProgressChanged?.Invoke(this, new DuplicateScanProgress
        { Message = msg, TotalFiles = total, ProcessedFiles = current });
}

public sealed class DuplicateGroup
{
    public required string              Hash        { get; init; }
    public required long                FileSize    { get; init; }
    public required IReadOnlyList<string> Files      { get; init; }
    public required long                WastedBytes { get; init; }

    public string FormattedSize => FileSize switch
    {
        < 1024              => $"{FileSize} B",
        < 1024 * 1024       => $"{FileSize / 1024.0:F1} KB",
        _                   => $"{FileSize / (1024.0 * 1024):F1} MB"
    };
}

public sealed class DuplicateScanProgress : EventArgs
{
    public required string Message        { get; init; }
    public required int    TotalFiles     { get; init; }
    public required int    ProcessedFiles { get; init; }
}
