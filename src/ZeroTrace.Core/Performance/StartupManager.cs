// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Performance;

/// <summary>
/// Manages Windows autostart entries from Registry Run/RunOnce keys.
/// Can list, disable, enable, and remove startup programs.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class StartupManager
{
    private readonly IZeroTraceLogger _logger;

    private static readonly (RegistryHive Hive, string Path, string Label)[] StartupLocations =
    [
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM Run"),
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "HKLM RunOnce"),
        (RegistryHive.CurrentUser,  @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU Run"),
        (RegistryHive.CurrentUser,  @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "HKCU RunOnce"),
    ];

    public StartupManager(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Returns all autostart entries from Registry.</summary>
    public List<StartupEntry> GetStartupEntries()
    {
        _logger.Info("Scanne Autostart-Eintraege...");
        var entries = new List<StartupEntry>();

        foreach (var (hive, path, label) in StartupLocations)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(path);
                if (key is null) continue;

                foreach (var name in key.GetValueNames())
                {
                    var cmd = key.GetValue(name)?.ToString() ?? "";
                    entries.Add(new StartupEntry
                    {
                        Name = name,
                        Command = cmd,
                        RegistryPath = $"{(hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU")}\\{path}",
                        Location = label,
                        IsEnabled = true,
                        Hive = hive,
                        SubPath = path
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"  Autostart-Scan: {label} - {ex.Message}");
            }
        }

        _logger.Info($"Autostart: {entries.Count} Eintraege gefunden");
        return entries;
    }

    /// <summary>Remove an autostart entry from the Registry.</summary>
    public bool RemoveEntry(StartupEntry entry)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(entry.Hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(entry.SubPath, writable: true);
            if (key is null) return false;

            key.DeleteValue(entry.Name, throwOnMissingValue: false);
            _logger.Info($"Autostart entfernt: {entry.Name}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning($"Autostart-Entfernung fehlgeschlagen: {entry.Name} - {ex.Message}");
            return false;
        }
    }
}

public sealed class StartupEntry
{
    public required string       Name         { get; init; }
    public required string       Command      { get; init; }
    public required string       RegistryPath { get; init; }
    public required string       Location     { get; init; }
    public required bool         IsEnabled    { get; init; }
    public required RegistryHive Hive         { get; init; }
    public required string       SubPath      { get; init; }
}
