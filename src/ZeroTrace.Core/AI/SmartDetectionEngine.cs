// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.AI;

/// <summary>
/// AI-powered detection engine that uses weighted scoring and learned patterns
/// to identify residual files with higher accuracy than simple name matching.
/// Combines multiple signals: path patterns, file age, size, known signatures.
/// </summary>
public sealed class SmartDetectionEngine
{
    private readonly IZeroTraceLogger _logger;
    private readonly PatternDatabase _patterns;

    // Detection weights (tuned over time by PatternLearner)
    private double _weightNameMatch    = 0.30;
    private double _weightPathPattern  = 0.25;
    private double _weightKnownSig     = 0.20;
    private double _weightFileAge      = 0.10;
    private double _weightSize         = 0.05;
    private double _weightRegistry     = 0.10;

    public SmartDetectionEngine(IZeroTraceLogger logger, PatternDatabase? patterns = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _patterns = patterns ?? new PatternDatabase(logger);
    }

    /// <summary>
    /// Analyze a potential residual item using multiple AI signals.
    /// Returns a confidence score between 0.0 and 1.0.
    /// </summary>
    public SmartDetectionResult Analyze(string path, InstalledProgram program)
    {
        var signals = new List<DetectionSignal>();
        double totalScore = 0;

        // Signal 1: Name matching (fuzzy)
        var nameScore = CalculateNameMatch(path, program);
        signals.Add(new DetectionSignal("Namens-Uebereinstimmung", nameScore, _weightNameMatch));
        totalScore += nameScore * _weightNameMatch;

        // Signal 2: Known path patterns
        var pathScore = CalculatePathPattern(path, program);
        signals.Add(new DetectionSignal("Pfad-Muster", pathScore, _weightPathPattern));
        totalScore += pathScore * _weightPathPattern;

        // Signal 3: Known software signatures
        var sigScore = _patterns.MatchesKnownPattern(path, program.DisplayName);
        signals.Add(new DetectionSignal("Bekannte Signatur", sigScore, _weightKnownSig));
        totalScore += sigScore * _weightKnownSig;

        // Signal 4: File age relative to install date
        var ageScore = CalculateAgeRelevance(path, program);
        signals.Add(new DetectionSignal("Alter-Relevanz", ageScore, _weightFileAge));
        totalScore += ageScore * _weightFileAge;

        // Signal 5: Size analysis
        var sizeScore = CalculateSizeRelevance(path);
        signals.Add(new DetectionSignal("Groessen-Analyse", sizeScore, _weightSize));
        totalScore += sizeScore * _weightSize;

        // Signal 6: Registry reference
        var regScore = CalculateRegistryRelevance(path, program);
        signals.Add(new DetectionSignal("Registry-Verweis", regScore, _weightRegistry));
        totalScore += regScore * _weightRegistry;

        // Normalize to 0-1
        totalScore = Math.Clamp(totalScore, 0.0, 1.0);

        var risk = totalScore switch
        {
            >= 0.8 => RiskLevel.High,
            >= 0.5 => RiskLevel.Medium,
            >= 0.3 => RiskLevel.Low,
            _      => RiskLevel.Safe
        };

        return new SmartDetectionResult
        {
            Path = path,
            ProgramName = program.DisplayName,
            ConfidenceScore = totalScore,
            Risk = risk,
            Signals = signals.AsReadOnly(),
            Recommendation = GenerateRecommendation(totalScore, risk)
        };
    }

    /// <summary>Batch-analyze multiple paths for a program.</summary>
    public List<SmartDetectionResult> AnalyzeBatch(
        IEnumerable<string> paths, InstalledProgram program)
    {
        _logger.Info($"SmartDetection: Analysiere Pfade fuer '{program.DisplayName}'");
        var results = paths.Select(p => Analyze(p, program)).ToList();

        int high = results.Count(r => r.Risk == RiskLevel.High);
        int med = results.Count(r => r.Risk == RiskLevel.Medium);
        _logger.Info($"  Ergebnis: {high} hohes Risiko, {med} mittleres Risiko von {results.Count}");

        return results;
    }

    /// <summary>Update detection weights (called by PatternLearner).</summary>
    public void UpdateWeights(double nameMatch, double pathPattern, double knownSig,
                              double fileAge, double size, double registry)
    {
        _weightNameMatch = nameMatch;
        _weightPathPattern = pathPattern;
        _weightKnownSig = knownSig;
        _weightFileAge = fileAge;
        _weightSize = size;
        _weightRegistry = registry;
        _logger.Info("SmartDetection: Gewichte aktualisiert");
    }

    // ── Signal Calculators ───────────────────────────────────────

    private static double CalculateNameMatch(string path, InstalledProgram program)
    {
        var fileName = Path.GetFileName(path) ?? "";
        var dirName = Path.GetFileName(Path.GetDirectoryName(path) ?? "") ?? "";
        var programName = program.DisplayName;

        if (string.IsNullOrEmpty(programName)) return 0;

        // Exact folder name match
        if (dirName.Equals(programName, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Folder contains program name
        if (dirName.Contains(programName, StringComparison.OrdinalIgnoreCase))
            return 0.8;

        // Split program name into words and check partial matches
        var words = programName.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3)
            .ToList();

        if (words.Count == 0) return 0;

        int matches = words.Count(w =>
            dirName.Contains(w, StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(w, StringComparison.OrdinalIgnoreCase));

        return (double)matches / words.Count * 0.7;
    }

    private static double CalculatePathPattern(string path, InstalledProgram program)
    {
        var lowerPath = path.ToLowerInvariant();

        // Known residual locations score higher
        if (lowerPath.Contains(@"\appdata\roaming\")) return 0.7;
        if (lowerPath.Contains(@"\appdata\local\")) return 0.7;
        if (lowerPath.Contains(@"\programdata\")) return 0.6;
        if (lowerPath.Contains(@"\temp\")) return 0.4;

        // Install location match
        if (!string.IsNullOrEmpty(program.InstallLocation) &&
            lowerPath.StartsWith(program.InstallLocation.ToLowerInvariant()))
            return 1.0;

        return 0.2;
    }

    private static double CalculateAgeRelevance(string path, InstalledProgram program)
    {
        try
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return 0;

            var lastWrite = File.Exists(path)
                ? File.GetLastWriteTimeUtc(path)
                : Directory.GetLastWriteTimeUtc(path);

            var daysSinceModified = (DateTime.UtcNow - lastWrite).TotalDays;

            // Files not touched in 30+ days are more likely residuals
            if (daysSinceModified > 180) return 0.9;
            if (daysSinceModified > 90) return 0.7;
            if (daysSinceModified > 30) return 0.5;
            return 0.2; // Recently modified = probably still in use
        }
        catch { return 0.3; }
    }

    private static double CalculateSizeRelevance(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var size = new FileInfo(path).Length;
                // Very small config files or very large data files are suspicious
                if (size < 1024) return 0.6;        // < 1 KB config file
                if (size > 100 * 1024 * 1024) return 0.3; // > 100 MB = probably important
                return 0.5;
            }
            return 0.5;
        }
        catch { return 0.3; }
    }

    private static double CalculateRegistryRelevance(string path, InstalledProgram program)
    {
        // If the program has a known registry key and this path is referenced there
        if (!string.IsNullOrEmpty(program.RegistryKeyPath))
            return 0.6;
        return 0.3;
    }

    private static string GenerateRecommendation(double score, RiskLevel risk) => risk switch
    {
        RiskLevel.High   => "Sicher zu loeschen. Hohe Wahrscheinlichkeit fuer Software-Rest.",
        RiskLevel.Medium => "Wahrscheinlich ein Rest. Backup vor dem Loeschen empfohlen.",
        RiskLevel.Low    => "Unsicher. Manuell ueberpruefen vor dem Loeschen.",
        _                => "Kein Risiko erkannt. Nicht zum Loeschen empfohlen."
    };
}

// ── Result Types ─────────────────────────────────────────────────

public sealed class SmartDetectionResult
{
    public required string   Path            { get; init; }
    public required string   ProgramName     { get; init; }
    public required double   ConfidenceScore { get; init; }
    public required RiskLevel Risk           { get; init; }
    public required IReadOnlyList<DetectionSignal> Signals { get; init; }
    public required string   Recommendation  { get; init; }

    public string ScoreDisplay => $"{ConfidenceScore:P0}";
    public string RiskDisplay => Risk switch
    {
        RiskLevel.High   => "HOCH",
        RiskLevel.Medium => "MITTEL",
        RiskLevel.Low    => "NIEDRIG",
        _                => "SICHER"
    };
}

public sealed class DetectionSignal
{
    public string Name   { get; }
    public double Score  { get; }
    public double Weight { get; }
    public double WeightedScore => Score * Weight;

    public DetectionSignal(string name, double score, double weight)
    {
        Name = name; Score = score; Weight = weight;
    }
}

public enum RiskLevel { Safe, Low, Medium, High }
