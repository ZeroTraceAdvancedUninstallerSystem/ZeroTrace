// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.AI;

/// <summary>
/// Learns from user decisions (delete/keep) to improve future detection accuracy.
/// Stores learned patterns locally and adjusts SmartDetectionEngine weights.
/// All learning happens on-device - no data leaves the computer.
/// </summary>
public sealed class PatternLearner
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _dataFilePath;
    private LearningData _data;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public PatternLearner(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataFilePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "ai-learning-data.json");
        _data = Load();
    }

    /// <summary>
    /// Record a user decision: did they delete or keep this residual?
    /// This feedback trains the pattern recognition over time.
    /// </summary>
    public void RecordDecision(string path, string programName, bool wasDeleted, double aiScore)
    {
        var pattern = ExtractPattern(path);

        if (!_data.PathPatterns.TryGetValue(pattern, out var stats))
        {
            stats = new PatternStats();
            _data.PathPatterns[pattern] = stats;
        }

        stats.TotalSeen++;
        if (wasDeleted) stats.TimesDeleted++;
        else stats.TimesKept++;

        // Track program-specific patterns
        var progKey = programName.ToLowerInvariant();
        if (!_data.ProgramPatterns.TryGetValue(progKey, out var progStats))
        {
            progStats = new ProgramLearning();
            _data.ProgramPatterns[progKey] = progStats;
        }
        progStats.TotalScanned++;
        if (wasDeleted) progStats.TotalCleaned++;

        // Track AI accuracy
        bool aiWasRight = (aiScore >= 0.5 && wasDeleted) || (aiScore < 0.5 && !wasDeleted);
        _data.TotalPredictions++;
        if (aiWasRight) _data.CorrectPredictions++;

        _data.LastLearnedUtc = DateTime.UtcNow;
        Save();

        _logger.Debug($"PatternLearner: {(wasDeleted ? "GELOESCHT" : "BEHALTEN")} " +
                      $"Muster='{pattern}' (AI={aiScore:P0}, Korrekt={aiWasRight})");
    }

    /// <summary>
    /// Get the learned deletion probability for a path pattern.
    /// Returns 0.5 (neutral) if no data is available.
    /// </summary>
    public double GetLearnedScore(string path)
    {
        var pattern = ExtractPattern(path);

        if (_data.PathPatterns.TryGetValue(pattern, out var stats) && stats.TotalSeen >= 3)
            return (double)stats.TimesDeleted / stats.TotalSeen;

        return 0.5; // No data = neutral
    }

    /// <summary>Current AI accuracy percentage.</summary>
    public double AccuracyPercent =>
        _data.TotalPredictions > 0
            ? (double)_data.CorrectPredictions / _data.TotalPredictions * 100
            : 0;

    /// <summary>Total patterns learned.</summary>
    public int TotalPatternsLearned => _data.PathPatterns.Count;

    /// <summary>Total user decisions recorded.</summary>
    public int TotalDecisions => _data.TotalPredictions;

    /// <summary>Get a summary of learning progress.</summary>
    public LearningSummary GetSummary() => new()
    {
        TotalPatterns = _data.PathPatterns.Count,
        TotalPrograms = _data.ProgramPatterns.Count,
        TotalDecisions = _data.TotalPredictions,
        CorrectPredictions = _data.CorrectPredictions,
        AccuracyPercent = AccuracyPercent,
        LastLearnedUtc = _data.LastLearnedUtc,
        TopDeletedPatterns = _data.PathPatterns
            .Where(kv => kv.Value.TotalSeen >= 3)
            .OrderByDescending(kv => (double)kv.Value.TimesDeleted / kv.Value.TotalSeen)
            .Take(10)
            .Select(kv => new PatternInfo
            {
                Pattern = kv.Key,
                DeleteRate = (double)kv.Value.TimesDeleted / kv.Value.TotalSeen,
                TimesSeen = kv.Value.TotalSeen
            })
            .ToList()
            .AsReadOnly()
    };

    /// <summary>Reset all learned data.</summary>
    public void Reset()
    {
        _data = new LearningData();
        Save();
        _logger.Info("PatternLearner: Alle Lerndaten zurueckgesetzt");
    }

    // ── Pattern Extraction ───────────────────────────────────────

    /// <summary>
    /// Extract a generalized pattern from a file path.
    /// Example: C:\Users\Mario\AppData\Local\Spotify\Data\cache -> appdata_local\{app}\data
    /// </summary>
    private static string ExtractPattern(string path)
    {
        var lower = path.ToLowerInvariant().Replace('/', '\\');

        // Generalize known base paths
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLowerInvariant();
        if (lower.StartsWith(userProfile))
            lower = "{user}" + lower[userProfile.Length..];

        lower = lower
            .Replace(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToLowerInvariant(), "{appdata_roaming}")
            .Replace(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).ToLowerInvariant(), "{appdata_local}")
            .Replace(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToLowerInvariant(), "{programdata}")
            .Replace(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).ToLowerInvariant(), "{progfiles}")
            .Replace(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86).ToLowerInvariant(), "{progfiles_x86}");

        // Keep only the last 3 path segments for the pattern
        var parts = lower.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 3)
            return string.Join("\\", parts[^3..]);

        return lower;
    }

    // ── Persistence ──────────────────────────────────────────────

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_dataFilePath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_dataFilePath, JsonSerializer.Serialize(_data, Json));
        }
        catch (Exception ex)
        { _logger.Warning($"PatternLearner: Speichern fehlgeschlagen: {ex.Message}"); }
    }

    private LearningData Load()
    {
        try
        {
            if (File.Exists(_dataFilePath))
            {
                var json = File.ReadAllText(_dataFilePath);
                return JsonSerializer.Deserialize<LearningData>(json, Json) ?? new();
            }
        }
        catch (Exception ex)
        { _logger.Warning($"PatternLearner: Laden fehlgeschlagen: {ex.Message}"); }
        return new();
    }
}

// ── Data Models ──────────────────────────────────────────────────

internal sealed class LearningData
{
    public Dictionary<string, PatternStats> PathPatterns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, ProgramLearning> ProgramPatterns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int TotalPredictions { get; set; }
    public int CorrectPredictions { get; set; }
    public DateTime LastLearnedUtc { get; set; }
}

internal sealed class PatternStats
{
    public int TotalSeen     { get; set; }
    public int TimesDeleted  { get; set; }
    public int TimesKept     { get; set; }
}

internal sealed class ProgramLearning
{
    public int TotalScanned { get; set; }
    public int TotalCleaned { get; set; }
}

public sealed class LearningSummary
{
    public required int      TotalPatterns       { get; init; }
    public required int      TotalPrograms       { get; init; }
    public required int      TotalDecisions      { get; init; }
    public required int      CorrectPredictions  { get; init; }
    public required double   AccuracyPercent     { get; init; }
    public required DateTime LastLearnedUtc      { get; init; }
    public required IReadOnlyList<PatternInfo> TopDeletedPatterns { get; init; }

    public string AccuracyDisplay => $"{AccuracyPercent:F1}%";
}

public sealed class PatternInfo
{
    public required string Pattern    { get; init; }
    public required double DeleteRate { get; init; }
    public required int    TimesSeen  { get; init; }
}
