// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.FileTools;

/// <summary>
/// Finds broken shortcuts (.lnk files) that point to non-existent targets.
/// Scans Desktop, Start Menu, and Quick Launch folders.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class BrokenLinkFinder
{
    private readonly IZeroTraceLogger _logger;

    public BrokenLinkFinder(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Find all broken shortcuts on the system.</summary>
    public List<BrokenLink> FindBrokenLinks()
    {
        _logger.Info("Suche nach defekten Verknuepfungen...");
        var broken = new List<BrokenLink>();

        var searchPaths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
        };

        foreach (var dir in searchPaths.Where(Directory.Exists).Distinct())
        {
            try
            {
                foreach (var lnk in Directory.EnumerateFiles(dir, "*.lnk", SearchOption.AllDirectories))
                {
                    try
                    {
                        var target = ResolveShortcutTarget(lnk);
                        if (target is null) continue;

                        bool exists = File.Exists(target) || Directory.Exists(target);
                        if (!exists)
                        {
                            broken.Add(new BrokenLink
                            {
                                ShortcutPath = lnk,
                                TargetPath = target,
                                ShortcutName = Path.GetFileNameWithoutExtension(lnk),
                                Location = Path.GetDirectoryName(lnk) ?? ""
                            });
                        }
                    }
                    catch { /* skip inaccessible shortcuts */ }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Verknuepfungs-Scan: {dir} - {ex.Message}");
            }
        }

        _logger.Info($"Verknuepfungs-Scan: {broken.Count} defekte gefunden");
        return broken;
    }

    /// <summary>Delete broken shortcuts.</summary>
    public int RemoveBrokenLinks(IEnumerable<BrokenLink> links)
    {
        int removed = 0;
        foreach (var link in links)
        {
            try
            {
                if (File.Exists(link.ShortcutPath))
                {
                    File.Delete(link.ShortcutPath);
                    removed++;
                    _logger.Debug($"  Entfernt: {link.ShortcutName}");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"  Fehler: {link.ShortcutName} - {ex.Message}");
            }
        }

        _logger.Info($"Defekte Verknuepfungen entfernt: {removed}");
        return removed;
    }

    /// <summary>
    /// Resolves a .lnk shortcut to its target path using COM Shell.
    /// Simplified approach using binary reading of .lnk file header.
    /// </summary>
    private static string? ResolveShortcutTarget(string lnkPath)
    {
        try
        {
            // Read the .lnk binary format (simplified)
            // The target path is typically stored after the shell link header
            var bytes = File.ReadAllBytes(lnkPath);
            if (bytes.Length < 76) return null;

            // Check magic number (0x4C = shell link)
            if (bytes[0] != 0x4C) return null;

            // Try to find a path string in the file
            var content = System.Text.Encoding.Unicode.GetString(bytes);
            var pathChars = new[] { ":\\", ":\\" };

            foreach (var marker in pathChars)
            {
                int idx = content.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                {
                    // Extract path starting one char before ':'
                    int start = idx - 1;
                    int end = content.IndexOf('\0', idx);
                    if (end > start && end - start < 500)
                    {
                        var path = content[start..end].Trim();
                        if (path.Length > 3 && path[1] == ':')
                            return path;
                    }
                }
            }
        }
        catch { /* not a valid shortcut */ }

        return null;
    }
}

public sealed class BrokenLink
{
    public required string ShortcutPath { get; init; }
    public required string TargetPath   { get; init; }
    public required string ShortcutName { get; init; }
    public required string Location     { get; init; }
}
