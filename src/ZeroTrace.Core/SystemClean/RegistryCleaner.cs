// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemClean;

/// <summary>
/// Finds and removes orphaned registry entries that point to
/// non-existent files, folders, or programs.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryCleaner
{
    private readonly IZeroTraceLogger _logger;

    public RegistryCleaner(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Scan for orphaned uninstall entries.</summary>
    public List<OrphanedRegistryEntry> ScanForOrphans()
    {
        _logger.Info("Scanne Registry nach verwaisten Eintraegen...");
        var orphans = new List<OrphanedRegistryEntry>();

        var uninstallPaths = new[]
        {
            (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
            (RegistryHive.CurrentUser,  @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        };

        foreach (var (hive, path) in uninstallPaths)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var subKey = baseKey.OpenSubKey(path);
                if (subKey is null) continue;

                foreach (var name in subKey.GetSubKeyNames())
                {
                    try
                    {
                        using var entry = subKey.OpenSubKey(name);
                        if (entry is null) continue;

                        var displayName = entry.GetValue("DisplayName") as string;
                        var installLoc = entry.GetValue("InstallLocation") as string;
                        var uninstallStr = entry.GetValue("UninstallString") as string;

                        // Check if install location exists
                        bool isOrphan = false;
                        string reason = "";

                        if (!string.IsNullOrEmpty(installLoc)
                            && !Directory.Exists(installLoc.Trim('"')))
                        {
                            isOrphan = true;
                            reason = $"Installpfad nicht gefunden: {installLoc}";
                        }
                        else if (string.IsNullOrWhiteSpace(displayName)
                            && string.IsNullOrWhiteSpace(uninstallStr))
                        {
                            isOrphan = true;
                            reason = "Kein Name und kein Deinstaller";
                        }

                        if (isOrphan)
                        {
                            orphans.Add(new OrphanedRegistryEntry
                            {
                                FullPath = $"{(hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU")}\\{path}\\{name}",
                                DisplayName = displayName ?? name,
                                Reason = reason,
                                Hive = hive,
                                SubPath = $"{path}\\{name}"
                            });
                        }
                    }
                    catch { /* skip inaccessible keys */ }
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Registry-Scan Fehler: {path} - {ex.Message}");
            }
        }

        _logger.Info($"Registry-Scan: {orphans.Count} verwaiste Eintraege gefunden");
        return orphans;
    }

    /// <summary>Remove a specific orphaned registry entry.</summary>
    public bool RemoveOrphan(OrphanedRegistryEntry orphan)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(orphan.Hive, RegistryView.Registry64);
            int lastSlash = orphan.SubPath.LastIndexOf('\\');
            if (lastSlash < 0) return false;

            string parentPath = orphan.SubPath[..lastSlash];
            string keyName = orphan.SubPath[(lastSlash + 1)..];

            using var parent = baseKey.OpenSubKey(parentPath, writable: true);
            if (parent is null) return false;

            parent.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
            _logger.Info($"  Entfernt: {orphan.DisplayName} ({orphan.FullPath})");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning($"  Fehler beim Entfernen: {orphan.FullPath} - {ex.Message}");
            return false;
        }
    }
}

public sealed class OrphanedRegistryEntry
{
    public required string       FullPath    { get; init; }
    public required string       DisplayName { get; init; }
    public required string       Reason      { get; init; }
    public required RegistryHive Hive        { get; init; }
    public required string       SubPath     { get; init; }
}
