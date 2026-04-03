// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Network;

/// <summary>
/// Fetches remote configuration and feature flags from the server.
/// Allows enabling/disabling features without releasing a new version.
/// Falls back to local defaults if the server is unreachable.
/// </summary>
public sealed class RemoteConfigService
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _localCachePath;
    private RemoteConfig _config;

    private const string ConfigUrl =
        "https://raw.githubusercontent.com/ZeroTraceAdvancedUninstallerSystem/ZeroTrace/main/remote-config.json";

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public RemoteConfigService(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _localCachePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "remote-config-cache.json");
        _config = LoadCache();
    }

    /// <summary>Fetch latest config from server. Falls back to cache on failure.</summary>
    public async Task<bool> RefreshAsync(HttpClient? httpClient = null, CancellationToken ct = default)
    {
        _logger.Info("RemoteConfig: Lade Konfiguration...");
        try
        {
            var client = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var json = await client.GetStringAsync(ConfigUrl, ct);
            var remote = JsonSerializer.Deserialize<RemoteConfig>(json, Json);

            if (remote is not null)
            {
                _config = remote;
                _config.LastFetchedUtc = DateTime.UtcNow;
                SaveCache();
                _logger.Info($"RemoteConfig: {_config.FeatureFlags.Count} Feature-Flags geladen");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"RemoteConfig: Server nicht erreichbar, nutze Cache ({ex.Message})");
        }
        return false;
    }

    // ── Feature Flags ────────────────────────────────────────────

    /// <summary>Check if a feature is enabled.</summary>
    public bool IsFeatureEnabled(string featureName)
    {
        if (_config.FeatureFlags.TryGetValue(featureName, out bool enabled))
            return enabled;
        return true; // Default: enabled
    }

    /// <summary>Get a config value as string.</summary>
    public string GetValue(string key, string defaultValue = "")
    {
        if (_config.Settings.TryGetValue(key, out string? value))
            return value;
        return defaultValue;
    }

    /// <summary>Get a config value as integer.</summary>
    public int GetInt(string key, int defaultValue = 0)
    {
        var str = GetValue(key);
        return int.TryParse(str, out int result) ? result : defaultValue;
    }

    // ── Well-known Feature Flags ─────────────────────────────────

    /// <summary>Is the AI SmartDetection module enabled?</summary>
    public bool IsAIEnabled => IsFeatureEnabled("ai_smart_detection");

    /// <summary>Is community pattern sharing enabled?</summary>
    public bool IsCommunityEnabled => IsFeatureEnabled("community_patterns");

    /// <summary>Is telemetry collection enabled?</summary>
    public bool IsTelemetryEnabled => IsFeatureEnabled("telemetry");

    /// <summary>Is the file shredder enabled?</summary>
    public bool IsShredderEnabled => IsFeatureEnabled("shredder");

    /// <summary>Minimum app version required (for forced updates).</summary>
    public string MinimumVersion => GetValue("minimum_version", "1.0.0");

    /// <summary>Message of the day shown on dashboard.</summary>
    public string? MessageOfTheDay => GetValue("motd");

    /// <summary>When was the config last fetched from server?</summary>
    public DateTime? LastFetchedUtc => _config.LastFetchedUtc;

    // ── Persistence ──────────────────────────────────────────────

    private void SaveCache()
    {
        try
        {
            var dir = Path.GetDirectoryName(_localCachePath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_localCachePath, JsonSerializer.Serialize(_config, Json));
        }
        catch { /* cache is optional */ }
    }

    private RemoteConfig LoadCache()
    {
        try
        {
            if (File.Exists(_localCachePath))
            {
                var json = File.ReadAllText(_localCachePath);
                return JsonSerializer.Deserialize<RemoteConfig>(json, Json) ?? GetDefaults();
            }
        }
        catch { }
        return GetDefaults();
    }

    private static RemoteConfig GetDefaults() => new()
    {
        FeatureFlags = new Dictionary<string, bool>
        {
            ["ai_smart_detection"] = true,
            ["community_patterns"] = true,
            ["telemetry"] = false,
            ["shredder"] = true,
            ["cloud_backup"] = false,
        },
        Settings = new Dictionary<string, string>
        {
            ["minimum_version"] = "1.0.0",
            ["motd"] = "",
            ["max_vault_age_days"] = "30",
            ["max_log_age_days"] = "14",
        }
    };
}

internal sealed class RemoteConfig
{
    public Dictionary<string, bool> FeatureFlags { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];
    public DateTime? LastFetchedUtc { get; set; }
}
