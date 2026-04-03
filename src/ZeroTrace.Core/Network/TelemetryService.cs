// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Network;

/// <summary>
/// Collects anonymous usage telemetry for improving ZeroTrace.
/// ALL data stays LOCAL by default. Nothing is sent without explicit user consent.
/// The user can opt-in to share anonymized data with the community database.
/// </summary>
public sealed class TelemetryService
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _telemetryFilePath;
    private TelemetryData _data;
    private bool _isOptedIn;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public TelemetryService(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryFilePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "telemetry.json");
        _data = Load();
    }

    /// <summary>Whether the user has opted in to share anonymized data.</summary>
    public bool IsOptedIn
    {
        get => _isOptedIn;
        set
        {
            _isOptedIn = value;
            _data.OptedIn = value;
            Save();
            _logger.Info($"Telemetrie Opt-{(value ? "In" : "Out")}");
        }
    }

    // ── Track Events ─────────────────────────────────────────────

    public void TrackScan(int programsFound)
    {
        _data.Events.Add(new TelemetryEvent
        {
            Type = "scan",
            TimestampUtc = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["programs_found"] = programsFound.ToString()
            }
        });
        TrimOldEvents();
        Save();
    }

    public void TrackCleanup(int itemsDeleted, long bytesFreed)
    {
        _data.Events.Add(new TelemetryEvent
        {
            Type = "cleanup",
            TimestampUtc = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["items_deleted"] = itemsDeleted.ToString(),
                ["bytes_freed"] = bytesFreed.ToString()
            }
        });
        TrimOldEvents();
        Save();
    }

    public void TrackUninstall(string programName, bool success)
    {
        _data.Events.Add(new TelemetryEvent
        {
            Type = "uninstall",
            TimestampUtc = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                // Anonymize: only first 3 chars of program name
                ["program_hint"] = programName.Length > 3 ? programName[..3] + "***" : "***",
                ["success"] = success.ToString()
            }
        });
        TrimOldEvents();
        Save();
    }

    public void TrackFeatureUsed(string featureName)
    {
        _data.Events.Add(new TelemetryEvent
        {
            Type = "feature",
            TimestampUtc = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["name"] = featureName
            }
        });
        TrimOldEvents();
        Save();
    }

    // ── Reports ──────────────────────────────────────────────────

    /// <summary>Get a summary of collected telemetry.</summary>
    public TelemetrySummary GetSummary() => new()
    {
        TotalEvents = _data.Events.Count,
        OldestEventUtc = _data.Events.MinBy(e => e.TimestampUtc)?.TimestampUtc,
        NewestEventUtc = _data.Events.MaxBy(e => e.TimestampUtc)?.TimestampUtc,
        EventsByType = _data.Events
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.Count()),
        IsOptedIn = _isOptedIn
    };

    /// <summary>Export anonymized telemetry for sharing (only if opted in).</summary>
    public string? ExportAnonymized()
    {
        if (!_isOptedIn)
        {
            _logger.Warning("Telemetrie-Export verweigert: Nutzer hat nicht zugestimmt");
            return null;
        }

        var anonymized = new
        {
            version = "1.0",
            exportedUtc = DateTime.UtcNow,
            osVersion = Environment.OSVersion.Version.ToString(),
            events = _data.Events.Select(e => new
            {
                e.Type,
                e.TimestampUtc,
                e.Data
            })
        };

        return JsonSerializer.Serialize(anonymized, Json);
    }

    /// <summary>Clear all collected telemetry data.</summary>
    public void ClearAll()
    {
        _data = new TelemetryData();
        Save();
        _logger.Info("Telemetrie-Daten geloescht");
    }

    // ── Persistence ──────────────────────────────────────────────

    private void TrimOldEvents()
    {
        // Keep only last 500 events
        if (_data.Events.Count > 500)
            _data.Events = _data.Events.OrderByDescending(e => e.TimestampUtc).Take(500).ToList();
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_telemetryFilePath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_telemetryFilePath, JsonSerializer.Serialize(_data, Json));
        }
        catch (Exception ex)
        { _logger.Warning($"Telemetrie speichern fehlgeschlagen: {ex.Message}"); }
    }

    private TelemetryData Load()
    {
        try
        {
            if (File.Exists(_telemetryFilePath))
            {
                var json = File.ReadAllText(_telemetryFilePath);
                var data = JsonSerializer.Deserialize<TelemetryData>(json, Json);
                if (data is not null)
                {
                    _isOptedIn = data.OptedIn;
                    return data;
                }
            }
        }
        catch (Exception ex)
        { _logger.Warning($"Telemetrie laden fehlgeschlagen: {ex.Message}"); }
        return new TelemetryData();
    }
}

// ── Data Models ──────────────────────────────────────────────────

internal sealed class TelemetryData
{
    public bool OptedIn { get; set; }
    public List<TelemetryEvent> Events { get; set; } = [];
}

internal sealed class TelemetryEvent
{
    public required string Type { get; init; }
    public required DateTime TimestampUtc { get; init; }
    public Dictionary<string, string> Data { get; init; } = [];
}

public sealed class TelemetrySummary
{
    public required int      TotalEvents    { get; init; }
    public          DateTime? OldestEventUtc { get; init; }
    public          DateTime? NewestEventUtc { get; init; }
    public required Dictionary<string, int> EventsByType { get; init; }
    public required bool     IsOptedIn      { get; init; }
}
