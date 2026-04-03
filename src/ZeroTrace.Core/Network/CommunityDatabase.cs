// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Network;

/// <summary>
/// Community-shared residual patterns database.
/// Users can download known residual patterns for popular software
/// and contribute their own findings (opt-in only).
/// Patterns are stored locally and synced periodically.
/// </summary>
public sealed class CommunityDatabase
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _localDbPath;
    private CommunityData _data;

    // URL where community patterns are hosted (GitHub raw or own server)
    private const string CommunityPatternsUrl =
        "https://raw.githubusercontent.com/ZeroTraceAdvancedUninstallerSystem/ZeroTrace/main/community-patterns.json";

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public CommunityDatabase(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _localDbPath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "community-patterns.json");
        _data = Load();
    }

    /// <summary>Total number of community patterns available locally.</summary>
    public int TotalPatterns => _data.Patterns.Count;

    /// <summary>When the database was last synced.</summary>
    public DateTime? LastSyncUtc => _data.LastSyncUtc;

    /// <summary>
    /// Download latest community patterns from the server.
    /// </summary>
    public async Task<SyncResult> SyncAsync(HttpClient? httpClient = null, CancellationToken ct = default)
    {
        _logger.Info("Community-Datenbank: Starte Synchronisation...");

        try
        {
            var client = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var json = await client.GetStringAsync(CommunityPatternsUrl, ct);
            var remote = JsonSerializer.Deserialize<CommunityData>(json, Json);

            if (remote is null)
                return new SyncResult { Success = false, ErrorMessage = "Ungueltige Server-Antwort" };

            int added = 0;
            foreach (var pattern in remote.Patterns)
            {
                var key = $"{pattern.ProgramName}|{pattern.PathPattern}".ToLowerInvariant();
                if (!_data.Patterns.Any(p =>
                    $"{p.ProgramName}|{p.PathPattern}".Equals(key, StringComparison.OrdinalIgnoreCase)))
                {
                    _data.Patterns.Add(pattern);
                    added++;
                }
            }

            _data.LastSyncUtc = DateTime.UtcNow;
            _data.ServerVersion = remote.ServerVersion;
            Save();

            _logger.Info($"Community-Sync: {added} neue Muster, {_data.Patterns.Count} gesamt");
            return new SyncResult { Success = true, NewPatternsAdded = added, TotalPatterns = _data.Patterns.Count };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Community-Sync fehlgeschlagen: {ex.Message}");
            return new SyncResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>Search community patterns for a specific program.</summary>
    public List<CommunityPattern> GetPatternsForProgram(string programName)
    {
        return _data.Patterns
            .Where(p => programName.Contains(p.ProgramName, StringComparison.OrdinalIgnoreCase)
                     || p.ProgramName.Contains(programName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.ConfidenceScore)
            .ToList();
    }

    /// <summary>Check if a path matches any community pattern.</summary>
    public CommunityPattern? FindMatchingPattern(string path, string programName)
    {
        var lowerPath = path.ToLowerInvariant();
        return _data.Patterns
            .Where(p => programName.Contains(p.ProgramName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(p => lowerPath.Contains(p.PathPattern.ToLowerInvariant()));
    }

    /// <summary>Add a locally discovered pattern to the database.</summary>
    public void AddLocalPattern(string programName, string pathPattern, double confidence)
    {
        _data.Patterns.Add(new CommunityPattern
        {
            ProgramName = programName,
            PathPattern = pathPattern,
            ConfidenceScore = confidence,
            Source = "local",
            AddedUtc = DateTime.UtcNow,
            ReportCount = 1
        });
        Save();
    }

    /// <summary>Export local patterns for community contribution.</summary>
    public string ExportLocalPatterns()
    {
        var local = _data.Patterns.Where(p => p.Source == "local").ToList();
        return JsonSerializer.Serialize(new { patterns = local }, Json);
    }

    // ── Persistence ──────────────────────────────────────────────

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_localDbPath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_localDbPath, JsonSerializer.Serialize(_data, Json));
        }
        catch (Exception ex)
        { _logger.Warning($"CommunityDB speichern fehlgeschlagen: {ex.Message}"); }
    }

    private CommunityData Load()
    {
        try
        {
            if (File.Exists(_localDbPath))
            {
                var json = File.ReadAllText(_localDbPath);
                return JsonSerializer.Deserialize<CommunityData>(json, Json) ?? new();
            }
        }
        catch (Exception ex)
        { _logger.Warning($"CommunityDB laden fehlgeschlagen: {ex.Message}"); }
        return new();
    }
}

// ── Data Models ──────────────────────────────────────────────────

internal sealed class CommunityData
{
    public string ServerVersion { get; set; } = "1.0";
    public DateTime? LastSyncUtc { get; set; }
    public List<CommunityPattern> Patterns { get; set; } = [];
}

public sealed class CommunityPattern
{
    public required string   ProgramName    { get; init; }
    public required string   PathPattern    { get; init; }
    public required double   ConfidenceScore { get; init; }
    public required string   Source         { get; init; }  // "community" or "local"
    public required DateTime AddedUtc       { get; init; }
    public          int      ReportCount    { get; init; }
}

public sealed class SyncResult
{
    public required bool   Success          { get; init; }
    public          int    NewPatternsAdded { get; init; }
    public          int    TotalPatterns    { get; init; }
    public          string? ErrorMessage    { get; init; }
}
