// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Statistics;

namespace ZeroTrace.Core.AI;

/// <summary>
/// Generates personalized recommendations for the user based on
/// system state, usage history, and scan results.
/// </summary>
public sealed class RecommendationEngine
{
    private readonly IZeroTraceLogger _logger;

    public RecommendationEngine(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Generate recommendations based on current system state.
    /// </summary>
    public List<Recommendation> GenerateRecommendations(
        IReadOnlyList<InstalledProgram>? programs = null,
        UsageStatistics? stats = null)
    {
        _logger.Info("Generiere Empfehlungen...");
        var recommendations = new List<Recommendation>();

        // Recommendation 1: Disk space
        CheckDiskSpace(recommendations);

        // Recommendation 2: Temp files
        CheckTempFiles(recommendations);

        // Recommendation 3: Large unused programs
        if (programs is not null)
            CheckUnusedPrograms(recommendations, programs);

        // Recommendation 4: Usage-based
        if (stats is not null)
            CheckUsagePatterns(recommendations, stats);

        // Recommendation 5: System health
        CheckSystemHealth(recommendations);

        _logger.Info($"Empfehlungen: {recommendations.Count} generiert");
        return recommendations.OrderByDescending(r => r.Priority).ToList();
    }

    private static void CheckDiskSpace(List<Recommendation> list)
    {
        try
        {
            var sysDrive = new DriveInfo(Path.GetPathRoot(
                Environment.GetFolderPath(Environment.SpecialFolder.System)) ?? "C:");

            double freePercent = (double)sysDrive.AvailableFreeSpace / sysDrive.TotalSize * 100;

            if (freePercent < 10)
            {
                list.Add(new Recommendation
                {
                    Title = "Speicherplatz kritisch niedrig!",
                    Description = $"Nur noch {freePercent:F1}% frei auf {sysDrive.Name}. " +
                                  "Fuehre eine Tiefenreinigung durch.",
                    Category = RecommendationCategory.DiskSpace,
                    Priority = RecommendationPriority.Critical,
                    ActionLabel = "System bereinigen",
                    ActionCommand = "ShowCleanup"
                });
            }
            else if (freePercent < 20)
            {
                list.Add(new Recommendation
                {
                    Title = "Speicherplatz wird knapp",
                    Description = $"{freePercent:F1}% frei. Browser-Cache und Temp-Dateien bereinigen.",
                    Category = RecommendationCategory.DiskSpace,
                    Priority = RecommendationPriority.High,
                    ActionLabel = "Cache bereinigen",
                    ActionCommand = "CleanBrowserCache"
                });
            }
        }
        catch { /* skip */ }
    }

    private static void CheckTempFiles(List<Recommendation> list)
    {
        try
        {
            var tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath)) return;

            var tempFiles = Directory.GetFiles(tempPath);
            long tempSize = tempFiles.Sum(f =>
            { try { return new FileInfo(f).Length; } catch { return 0L; } });

            if (tempSize > 500 * 1024 * 1024) // > 500 MB
            {
                list.Add(new Recommendation
                {
                    Title = $"Temp-Dateien: {tempSize / (1024 * 1024)} MB",
                    Description = $"{tempFiles.Length} temporaere Dateien belegen Speicherplatz.",
                    Category = RecommendationCategory.Cleanup,
                    Priority = RecommendationPriority.Medium,
                    ActionLabel = "Temp bereinigen",
                    ActionCommand = "CleanTemp"
                });
            }
        }
        catch { /* skip */ }
    }

    private static void CheckUnusedPrograms(
        List<Recommendation> list, IReadOnlyList<InstalledProgram> programs)
    {
        // Programs without uninstall string might be leftovers
        var noUninstall = programs.Count(p =>
            string.IsNullOrEmpty(p.UninstallString) &&
            string.IsNullOrEmpty(p.QuietUninstallString) &&
            string.IsNullOrEmpty(p.MsiProductCode));

        if (noUninstall > 5)
        {
            list.Add(new Recommendation
            {
                Title = $"{noUninstall} Programme ohne Deinstaller",
                Description = "Diese Programme koennen nicht normal entfernt werden. " +
                              "ZeroTrace kann deren Reste finden und bereinigen.",
                Category = RecommendationCategory.Programs,
                Priority = RecommendationPriority.Low,
                ActionLabel = "Programme pruefen",
                ActionCommand = "ShowPrograms"
            });
        }
    }

    private static void CheckUsagePatterns(
        List<Recommendation> list, UsageStatistics stats)
    {
        if (stats.TotalScans == 0)
        {
            list.Add(new Recommendation
            {
                Title = "Erster Scan empfohlen",
                Description = "Du hast noch keinen System-Scan durchgefuehrt. " +
                              "Starte jetzt um Programme und Reste zu finden.",
                Category = RecommendationCategory.GettingStarted,
                Priority = RecommendationPriority.High,
                ActionLabel = "Jetzt scannen",
                ActionCommand = "Scan"
            });
        }
        else if (stats.DaysSinceFirstUse > 7 && stats.TotalCleanups == 0)
        {
            list.Add(new Recommendation
            {
                Title = "Erste Bereinigung durchfuehren",
                Description = "Du nutzt ZeroTrace seit einer Woche aber hast noch nie bereinigt.",
                Category = RecommendationCategory.Cleanup,
                Priority = RecommendationPriority.Medium,
                ActionLabel = "Bereinigung starten",
                ActionCommand = "ShowCleanup"
            });
        }
    }

    private static void CheckSystemHealth(List<Recommendation> list)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        if (uptime.TotalDays > 14)
        {
            list.Add(new Recommendation
            {
                Title = "Neustart empfohlen",
                Description = $"Dein System laeuft seit {uptime.Days} Tagen. " +
                              "Ein Neustart kann die Leistung verbessern.",
                Category = RecommendationCategory.SystemHealth,
                Priority = RecommendationPriority.Low,
                ActionLabel = "Spaeter erinnern",
                ActionCommand = "Dismiss"
            });
        }
    }
}

// ── Data Models ──────────────────────────────────────────────────

public sealed class Recommendation
{
    public required string Title                   { get; init; }
    public required string Description             { get; init; }
    public required RecommendationCategory Category { get; init; }
    public required RecommendationPriority Priority { get; init; }
    public required string ActionLabel             { get; init; }
    public required string ActionCommand           { get; init; }

    public string PriorityDisplay => Priority switch
    {
        RecommendationPriority.Critical => "KRITISCH",
        RecommendationPriority.High     => "HOCH",
        RecommendationPriority.Medium   => "MITTEL",
        _                               => "NIEDRIG"
    };

    public string CategoryDisplay => Category switch
    {
        RecommendationCategory.DiskSpace      => "Speicherplatz",
        RecommendationCategory.Cleanup        => "Bereinigung",
        RecommendationCategory.Programs       => "Programme",
        RecommendationCategory.SystemHealth   => "System-Gesundheit",
        RecommendationCategory.GettingStarted => "Erste Schritte",
        _                                     => "Allgemein"
    };
}

public enum RecommendationCategory
{
    DiskSpace, Cleanup, Programs, SystemHealth, GettingStarted
}

public enum RecommendationPriority
{
    Low = 0, Medium = 1, High = 2, Critical = 3
}
