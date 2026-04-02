// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Security;

/// <summary>
/// Protects critical system paths and registry keys from accidental deletion.
/// Every cleanup operation MUST pass through SystemGuard.IsPathSafe() first.
/// This is the last line of defense before any destructive operation.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SystemGuard
{
    private readonly IZeroTraceLogger _logger;

    // Paths that must NEVER be deleted
    private static readonly HashSet<string> CriticalPaths =
        new(StringComparer.OrdinalIgnoreCase)
    {
        // Windows Core
        @"C:\Windows",
        @"C:\Windows\System32",
        @"C:\Windows\SysWOW64",
        @"C:\Windows\WinSxS",
        @"C:\Windows\Fonts",
        @"C:\Windows\Installer",
        @"C:\Windows\Logs",
        @"C:\Windows\Boot",
        @"C:\Windows\Cursors",
        @"C:\Windows\Globalization",
        @"C:\Windows\INF",
        @"C:\Windows\Media",
        @"C:\Windows\Microsoft.NET",
        @"C:\Windows\PolicyDefinitions",
        @"C:\Windows\Provisioning",
        @"C:\Windows\Resources",
        @"C:\Windows\SchCache",
        @"C:\Windows\Security",
        @"C:\Windows\servicing",
        @"C:\Windows\Setup",
        @"C:\Windows\ShellExperiences",
        @"C:\Windows\SystemApps",
        @"C:\Windows\SystemResources",
        @"C:\Windows\Web",

        // System Programs
        @"C:\Program Files\Windows Defender",
        @"C:\Program Files\Windows NT",
        @"C:\Program Files\WindowsApps",
        @"C:\Program Files\Windows Mail",
        @"C:\Program Files\Windows Sidebar",

        // System Data
        @"C:\ProgramData\Microsoft",
        @"C:\ProgramData\Windows",

        // User Core (never delete user profile root!)
        @"C:\Users\Default",
        @"C:\Users\Public",

        // Boot
        @"C:\Boot",
        @"C:\Recovery",
    };

    // Registry keys that must NEVER be deleted
    private static readonly string[] CriticalRegistryPrefixes =
    [
        @"HKEY_LOCAL_MACHINE\SYSTEM",
        @"HKEY_LOCAL_MACHINE\SAM",
        @"HKEY_LOCAL_MACHINE\SECURITY",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer",
        @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
    ];

    // File extensions that should never be shredded
    private static readonly HashSet<string> CriticalExtensions =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ".sys", ".drv", ".dll", // Only in System32
        ".cat", ".mum", ".manifest",
    };

    public SystemGuard(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Check if a file/directory path is safe to delete.
    /// Returns false for any system-critical location.
    /// </summary>
    public bool IsPathSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        var normalized = Path.GetFullPath(path).TrimEnd('\\');

        // Rule 1: Never delete critical paths or anything directly inside them
        foreach (var critical in CriticalPaths)
        {
            if (normalized.Equals(critical, StringComparison.OrdinalIgnoreCase))
            {
                LogBlocked(path, "Kritischer Systempfad");
                return false;
            }
        }

        // Rule 2: Never delete from Windows directory
        var winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (normalized.StartsWith(winDir, StringComparison.OrdinalIgnoreCase))
        {
            LogBlocked(path, "Windows-Verzeichnis");
            return false;
        }

        // Rule 3: Never delete system drive root items
        var sysRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? @"C:\";
        if (normalized.Equals(sysRoot.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
        {
            LogBlocked(path, "Laufwerk-Root");
            return false;
        }

        // Rule 4: Check critical file extensions in system paths
        var ext = Path.GetExtension(normalized);
        if (CriticalExtensions.Contains(ext))
        {
            var sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (normalized.StartsWith(sys32, StringComparison.OrdinalIgnoreCase))
            {
                LogBlocked(path, $"Kritische Systemdatei ({ext})");
                return false;
            }
        }

        // Rule 5: Never delete the user profile root
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (normalized.Equals(userProfile, StringComparison.OrdinalIgnoreCase))
        {
            LogBlocked(path, "Benutzer-Profilordner");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if a registry path is safe to delete.
    /// Returns false for any system-critical registry key.
    /// </summary>
    public bool IsRegistryPathSafe(string registryPath)
    {
        if (string.IsNullOrWhiteSpace(registryPath)) return false;

        foreach (var prefix in CriticalRegistryPrefixes)
        {
            if (registryPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                LogBlocked(registryPath, "Kritischer Registry-Schluessel");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Validate a list of paths and return only the safe ones.
    /// Logs all blocked paths.
    /// </summary>
    public List<string> FilterSafePaths(IEnumerable<string> paths)
    {
        var safe = new List<string>();
        int blocked = 0;

        foreach (var path in paths)
        {
            if (IsPathSafe(path))
                safe.Add(path);
            else
                blocked++;
        }

        if (blocked > 0)
            _logger.Warning($"SystemGuard: {blocked} Pfad(e) blockiert (Systemschutz)");

        return safe;
    }

    private void LogBlocked(string path, string reason) =>
        _logger.Warning($"[SYSTEMGUARD BLOCKIERT] {reason}: {path}");
}
