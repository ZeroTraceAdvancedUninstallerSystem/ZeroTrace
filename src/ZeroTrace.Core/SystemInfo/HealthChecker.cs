// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemInfo;

/// <summary>
/// Performs system health checks and generates a report.
/// Checks disk space, temp file buildup, startup load, and more.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class HealthChecker
{
    private readonly IZeroTraceLogger _logger;

    public HealthChecker(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Run all health checks and return a report.</summary>
    public HealthReport RunFullCheck()
    {
        _logger.Info("Starte System-Gesundheitspruefung...");
        var issues = new List<HealthIssue>();

        CheckDiskSpace(issues);
        CheckTempFiles(issues);
        CheckMemory(issues);
        CheckUptime(issues);

        var score = CalculateScore(issues);

        _logger.Info($"Gesundheitspruefung: Score {score}/100, {issues.Count} Probleme");

        return new HealthReport
        {
            TimestampUtc = DateTime.UtcNow,
            OverallScore = score,
            Rating = score switch
            {
                >= 90 => HealthRating.Excellent,
                >= 70 => HealthRating.Good,
                >= 50 => HealthRating.Fair,
                _     => HealthRating.Poor
            },
            Issues = issues.AsReadOnly()
        };
    }

    private void CheckDiskSpace(List<HealthIssue> issues)
    {
        foreach (var drive in DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed))
        {
            double freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;

            if (freePercent < 5)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Speicherplatz",
                    Severity = IssueSeverity.Critical,
                    Message = $"Laufwerk {drive.Name} hat nur noch {freePercent:F1}% frei!",
                    Recommendation = "Sofort Speicherplatz freigeben oder Dateien verschieben."
                });
            }
            else if (freePercent < 15)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Speicherplatz",
                    Severity = IssueSeverity.Warning,
                    Message = $"Laufwerk {drive.Name} hat nur noch {freePercent:F1}% frei.",
                    Recommendation = "Temp-Dateien und Browser-Cache bereinigen."
                });
            }
        }
    }

    private void CheckTempFiles(List<HealthIssue> issues)
    {
        try
        {
            var tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath)) return;

            var files = Directory.GetFiles(tempPath);
            if (files.Length > 500)
            {
                issues.Add(new HealthIssue
                {
                    Category = "Temp-Dateien",
                    Severity = IssueSeverity.Warning,
                    Message = $"{files.Length} temporaere Dateien gefunden.",
                    Recommendation = "Fuehre die Temp-Bereinigung in ZeroTrace aus."
                });
            }
        }
        catch { /* skip */ }
    }

    private void CheckMemory(List<HealthIssue> issues)
    {
        var gcInfo = GC.GetGCMemoryInfo();
        long total = gcInfo.TotalAvailableMemoryBytes;
        long used = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;

        // Check if system memory is very constrained
        if (total > 0 && total < 4L * 1024 * 1024 * 1024)
        {
            issues.Add(new HealthIssue
            {
                Category = "Arbeitsspeicher",
                Severity = IssueSeverity.Info,
                Message = $"System hat nur {total / (1024 * 1024 * 1024.0):F1} GB RAM.",
                Recommendation = "Fuer optimale Leistung werden 8 GB oder mehr empfohlen."
            });
        }
    }

    private void CheckUptime(List<HealthIssue> issues)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        if (uptime.TotalDays > 14)
        {
            issues.Add(new HealthIssue
            {
                Category = "System-Neustart",
                Severity = IssueSeverity.Info,
                Message = $"System laeuft seit {uptime.Days} Tagen ohne Neustart.",
                Recommendation = "Ein gelegentlicher Neustart kann die Leistung verbessern."
            });
        }
    }

    private static int CalculateScore(List<HealthIssue> issues)
    {
        int score = 100;
        foreach (var issue in issues)
        {
            score -= issue.Severity switch
            {
                IssueSeverity.Critical => 25,
                IssueSeverity.Warning  => 10,
                IssueSeverity.Info     => 3,
                _                      => 0
            };
        }
        return Math.Max(0, Math.Min(100, score));
    }
}

public sealed class HealthReport
{
    public required DateTime TimestampUtc                 { get; init; }
    public required int      OverallScore                 { get; init; }
    public required HealthRating Rating                   { get; init; }
    public required IReadOnlyList<HealthIssue> Issues     { get; init; }

    public string RatingDisplay => Rating switch
    {
        HealthRating.Excellent => "Ausgezeichnet",
        HealthRating.Good      => "Gut",
        HealthRating.Fair      => "Befriedigend",
        HealthRating.Poor      => "Schlecht",
        _                      => "Unbekannt"
    };
}

public sealed class HealthIssue
{
    public required string        Category       { get; init; }
    public required IssueSeverity Severity       { get; init; }
    public required string        Message        { get; init; }
    public required string        Recommendation { get; init; }
}

public enum HealthRating { Excellent, Good, Fair, Poor }
public enum IssueSeverity { Info, Warning, Critical }
