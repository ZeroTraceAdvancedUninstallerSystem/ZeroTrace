// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Analysis;

public sealed class ScanProgressEventArgs : EventArgs
{
    public required string CurrentAreaName  { get; init; }
    public required string CurrentPath      { get; init; }
    public required int    TotalAreas       { get; init; }
    public required int    CompletedAreas   { get; init; }
    public required int    ItemsFoundSoFar  { get; init; }
    public int PercentComplete =>
        TotalAreas == 0 ? 0 : CompletedAreas * 100 / TotalAreas;
}

[SupportedOSPlatform("windows")]
public sealed partial class ResidualAnalysisEngine
{
    private readonly IZeroTraceLogger _logger;
    public event EventHandler<ScanProgressEventArgs>? ProgressChanged;

    private static readonly HashSet<string> ProtectedPaths =
        new(StringComparer.OrdinalIgnoreCase)
    {
        @"C:\Windows", @"C:\Windows\System32", @"C:\Windows\SysWOW64",
        @"C:\Windows\WinSxS", @"C:\Windows\Fonts", @"C:\Windows\Installer",
        @"C:\Program Files\Windows Defender", @"C:\Program Files\Windows NT",
        @"C:\Program Files\WindowsApps",
        @"C:\ProgramData\Microsoft", @"C:\ProgramData\Windows",
    };

    private static readonly string[] ProtectedRegistryPrefixes =
    [
        @"HKEY_LOCAL_MACHINE\SYSTEM",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID",
    ];

    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
    {
        "The", "Inc", "Ltd", "LLC", "Corp", "Corporation", "Software",
        "Microsoft", "Windows", "System", "System32", "Update", "Setup",
        "Install", "Uninstall", "Common", "Shared", "Data", "Files",
        "Program", "App", "Application", "Tools", "Utility",
        "x86", "x64", "amd64", "arm64",
    };

    public ResidualAnalysisEngine(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<ResidualItem>> ScanAsync(
        InstalledProgram program, CancellationToken ct = default)
    {
        _logger.Info($"Residual-Scan: {program.DisplayName}");
        var terms = BuildSearchTerms(program);
        _logger.Info($"  Suchbegriffe: [{string.Join(", ", terms)}]");
        if (terms.Count == 0)
        {
            _logger.Warning("  Keine Suchbegriffe - Scan abgebrochen.");
            return Array.Empty<ResidualItem>();
        }
        var areas = BuildScanAreas(program);
        var results = new List<ResidualItem>();
        int done = 0;
        foreach (var area in areas)
        {
            ct.ThrowIfCancellationRequested();
            ReportProgress(area.Name, area.BasePath, areas.Count, done, results.Count);
            try
            {
                var found = await Task.Run(() =>
                    area.IsRegistry ? ScanRegistry(area, terms) : ScanFileSystem(area, terms), ct);
                if (found.Count > 0)
                {
                    _logger.Info($"  {area.Name}: {found.Count} Rest(e)");
                    results.AddRange(found);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { _logger.Warning($"  {area.Name}: {ex.Message}"); }
            done++;
        }
        var sorted = results
            .OrderByDescending(r => r.ConfidenceScore)
            .ThenBy(r => r.ItemType)
            .ThenBy(r => r.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var item in sorted)
            item.IsSelectedForDeletion = item.ConfidenceScore >= 0.5;
        _logger.Info($"Scan fertig: {sorted.Count} Reste gefunden");
        return sorted.AsReadOnly();
    }

    private static List<string> BuildSearchTerms(InstalledProgram program)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(program.DisplayName))
        {
            var clean = CleanName(program.DisplayName);
            if (clean.Length >= 3) terms.Add(clean);
            foreach (var w in clean.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries))
            {
                if (w.Length >= 3 && !StopWords.Contains(w) && !w.All(c => char.IsDigit(c) || c == '.'))
                    terms.Add(w);
            }
        }
        if (!string.IsNullOrEmpty(program.Publisher))
        {
            foreach (var w in program.Publisher.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
                if (w.Length >= 3 && !StopWords.Contains(w)) terms.Add(w);
        }
        if (!string.IsNullOrEmpty(program.MsiProductCode))
            terms.Add(program.MsiProductCode);
        return terms.ToList();
    }

    private static string CleanName(string name)
    {
        int p = name.IndexOf('(');
        var s = p > 0 ? name[..p] : name;
        return VersionSuffix().Replace(s, "").Trim();
    }

    [GeneratedRegex(@"\s+v?\d+[\.\d]*\s*$")]
    private static partial Regex VersionSuffix();

    private static List<ScanArea> BuildScanAreas(InstalledProgram program)
    {
        var list = new List<ScanArea>();
        if (!string.IsNullOrEmpty(program.InstallLocation) && Directory.Exists(program.InstallLocation))
            list.Add(new("Installationsordner", program.InstallLocation, false, ResidualDetectionSource.InstallPath));
        list.Add(new("Program Files",
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            false, ResidualDetectionSource.NameMatch));
        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (pf86 != Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))
            list.Add(new("Program Files (x86)", pf86, false, ResidualDetectionSource.NameMatch));
        list.Add(new("AppData Roaming",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            false, ResidualDetectionSource.AppDataScan));
        list.Add(new("AppData Local",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            false, ResidualDetectionSource.AppDataScan));
        list.Add(new("ProgramData",
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            false, ResidualDetectionSource.ProgramDataScan));
        list.Add(new("Temp", Path.GetTempPath(), false, ResidualDetectionSource.TempScan));
        list.Add(new("Registry HKCU", @"HKEY_CURRENT_USER\SOFTWARE", true, ResidualDetectionSource.RegistryScan));
        list.Add(new("Registry HKLM", @"HKEY_LOCAL_MACHINE\SOFTWARE", true, ResidualDetectionSource.RegistryScan));
        return list;
    }

    private List<ResidualItem> ScanFileSystem(ScanArea area, List<string> terms)
    {
        var results = new List<ResidualItem>();
        if (!Directory.Exists(area.BasePath) || IsProtected(area.BasePath)) return results;
        if (area.Source == ResidualDetectionSource.InstallPath)
        {
            try { results.Add(MakeDirItem(area.BasePath, 1.0, "Installationsordner existiert noch", area.Source)); } catch { }
            return results;
        }
        try
        {
            foreach (var dir in Directory.GetDirectories(area.BasePath))
            {
                if (IsProtected(dir)) continue;
                var score = Score(Path.GetFileName(dir), terms);
                if (score >= 0.5)
                {
                    try { results.Add(MakeDirItem(dir, score, $"Ordnername passt (Score: {score:P0})", area.Source)); } catch { }
                }
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (Exception ex) { _logger.Debug($"Scan-Fehler: {area.BasePath} - {ex.Message}"); }
        return results;
    }

    private static ResidualItem MakeDirItem(string path, double score, string reason, ResidualDetectionSource src)
    {
        long size = 0;
        try
        {
            size = new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0L; } });
        }
        catch { }
        return new ResidualItem
        {
            Id = Guid.NewGuid().ToString("N"), FullPath = path,
            ItemType = ResidualItemType.Directory, SizeInBytes = size,
            ConfidenceScore = score, DetectionReason = reason,
            DetectionSource = src, IsSelectedForDeletion = score >= 0.5
        };
    }

    private List<ResidualItem> ScanRegistry(ScanArea area, List<string> terms)
    {
        var results = new List<ResidualItem>();
        if (IsProtectedReg(area.BasePath)) return results;
        try
        {
            var (hive, sub) = ParseRegPath(area.BasePath);
            if (hive is null) return results;
            using var baseKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Default);
            using var key = baseKey.OpenSubKey(sub);
            if (key is null) return results;
            foreach (var name in key.GetSubKeyNames())
            {
                var full = $@"{area.BasePath}\{name}";
                if (IsProtectedReg(full)) continue;
                var score = Score(name, terms);
                if (score >= 0.6)
                {
                    results.Add(new ResidualItem
                    {
                        Id = Guid.NewGuid().ToString("N"), FullPath = full,
                        ItemType = ResidualItemType.RegistryKey, ConfidenceScore = score,
                        DetectionReason = $"Registry-Key passt (Score: {score:P0})",
                        DetectionSource = ResidualDetectionSource.RegistryScan,
                        IsSelectedForDeletion = score >= 0.6
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }
        catch (Exception ex) { _logger.Debug($"Registry-Fehler: {area.BasePath} - {ex.Message}"); }
        return results;
    }

    private static double Score(string name, List<string> terms)
    {
        if (string.IsNullOrWhiteSpace(name) || terms.Count == 0) return 0.0;
        double best = 0.0;
        foreach (var t in terms)
        {
            if (t.Length < 3) continue;
            if (name.Equals(t, StringComparison.OrdinalIgnoreCase)) return 1.0;
            if (name.Contains(t, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 0.5 + (double)t.Length / name.Length * 0.5);
        }
        return best;
    }

    private static bool IsProtected(string p) =>
        ProtectedPaths.Any(pp => p.StartsWith(pp, StringComparison.OrdinalIgnoreCase));
    private static bool IsProtectedReg(string p) =>
        ProtectedRegistryPrefixes.Any(pp => p.StartsWith(pp, StringComparison.OrdinalIgnoreCase));

    private static (RegistryHive? Hive, string Sub) ParseRegPath(string path) => path switch
    {
        _ when path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.CurrentUser, path[18..]),
        _ when path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.LocalMachine, path[19..]),
        _ => (null, path)
    };

    private void ReportProgress(string area, string path, int total, int done, int found)
    {
        ProgressChanged?.Invoke(this, new ScanProgressEventArgs
        {
            CurrentAreaName = area, CurrentPath = path,
            TotalAreas = total, CompletedAreas = done, ItemsFoundSoFar = found
        });
    }
}

internal sealed record ScanArea(
    string Name, string BasePath, bool IsRegistry, ResidualDetectionSource Source);
