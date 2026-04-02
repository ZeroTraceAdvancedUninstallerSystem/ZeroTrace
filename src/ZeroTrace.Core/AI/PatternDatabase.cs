// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.AI;

/// <summary>
/// Database of known software residual patterns.
/// Contains common paths and signatures left by popular programs.
/// Used by SmartDetectionEngine for pattern matching.
/// </summary>
public sealed class PatternDatabase
{
    private readonly IZeroTraceLogger _logger;

    // Known residual patterns: program name -> typical leftover paths
    private static readonly Dictionary<string, string[]> KnownPatterns =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["Chrome"] = [@"\Google\Chrome\", @"\Google\Update\", @"\Google\CrashReports\"],
        ["Firefox"] = [@"\Mozilla\Firefox\", @"\Mozilla\Extensions\", @"\Mozilla\MaintenanceService\"],
        ["Edge"] = [@"\Microsoft\Edge\", @"\Microsoft\EdgeUpdate\"],
        ["Spotify"] = [@"\Spotify\", @"\Spotify\Data\", @"\Spotify\Storage\"],
        ["Discord"] = [@"\discord\", @"\Discord\Cache\", @"\Discord\blob_storage\"],
        ["Steam"] = [@"\Steam\", @"\steamapps\", @"\Steam\config\"],
        ["Adobe"] = [@"\Adobe\", @"\Adobe\ARM\", @"\Adobe\AcroCef\"],
        ["VLC"] = [@"\vlc\", @"\VideoLAN\"],
        ["7-Zip"] = [@"\7-Zip\"],
        ["WinRAR"] = [@"\WinRAR\"],
        ["Notepad++"] = [@"\Notepad++\"],
        ["Visual Studio Code"] = [@"\Code\", @"\Code - Insiders\", @"\.vscode\"],
        ["Visual Studio"] = [@"\Microsoft\VisualStudio\", @"\.vs\"],
        ["Node.js"] = [@"\npm\", @"\npm-cache\", @"\node_modules\"],
        ["Python"] = [@"\Python\", @"\pip\", @"\.python\"],
        ["Java"] = [@"\Java\", @"\.java\", @"\Oracle\Java\"],
        ["Zoom"] = [@"\Zoom\", @"\ZoomVideo\"],
        ["Teams"] = [@"\Microsoft\Teams\", @"\Teams\"],
        ["Slack"] = [@"\Slack\", @"\slack\"],
        ["Skype"] = [@"\Skype\", @"\Microsoft\Skype for Desktop\"],
        ["Dropbox"] = [@"\Dropbox\", @"\.dropbox\"],
        ["OneDrive"] = [@"\OneDrive\", @"\Microsoft\OneDrive\"],
        ["Git"] = [@"\Git\", @"\.git\", @"\GitHubDesktop\"],
        ["NVIDIA"] = [@"\NVIDIA\", @"\NVIDIA Corporation\"],
        ["AMD"] = [@"\AMD\", @"\ATI\"],
        ["Intel"] = [@"\Intel\"],
        ["CCleaner"] = [@"\CCleaner\", @"\Piriform\"],
        ["Avast"] = [@"\Avast Software\", @"\AVAST Software\"],
        ["Kaspersky"] = [@"\Kaspersky Lab\"],
        ["McAfee"] = [@"\McAfee\"],
    };

    public PatternDatabase(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if a path matches a known software residual pattern.
    /// Returns a confidence score (0.0 to 1.0).
    /// </summary>
    public double MatchesKnownPattern(string path, string programName)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(programName))
            return 0;

        var lowerPath = path.ToLowerInvariant();

        // Direct match: check if any known pattern for this program matches
        foreach (var (knownName, patterns) in KnownPatterns)
        {
            if (!programName.Contains(knownName, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var pattern in patterns)
            {
                if (lowerPath.Contains(pattern.ToLowerInvariant()))
                    return 0.9; // High confidence for known pattern
            }
        }

        // Fuzzy match: check all patterns regardless of program name
        foreach (var (_, patterns) in KnownPatterns)
        {
            foreach (var pattern in patterns)
            {
                if (lowerPath.Contains(pattern.ToLowerInvariant()))
                    return 0.5; // Medium confidence (might be different program)
            }
        }

        return 0; // No match
    }

    /// <summary>Check if a program name is in the known database.</summary>
    public bool IsKnownProgram(string programName) =>
        KnownPatterns.Keys.Any(k =>
            programName.Contains(k, StringComparison.OrdinalIgnoreCase));

    /// <summary>Get all known pattern entries for a program.</summary>
    public string[] GetPatternsForProgram(string programName)
    {
        foreach (var (name, patterns) in KnownPatterns)
        {
            if (programName.Contains(name, StringComparison.OrdinalIgnoreCase))
                return patterns;
        }
        return [];
    }

    /// <summary>Total known program patterns in the database.</summary>
    public int TotalKnownPrograms => KnownPatterns.Count;

    /// <summary>Total known path patterns across all programs.</summary>
    public int TotalKnownPatterns => KnownPatterns.Values.Sum(v => v.Length);
}
