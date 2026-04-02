// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Reflection;
using System.Text.Json;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Update;

/// <summary>
/// Checks for updates via a JSON manifest on GitHub/server.
/// Downloads and applies updates when available.
/// </summary>
public sealed class UpdateService
{
    private readonly IZeroTraceLogger _logger;
    private readonly HttpClient _http;

    // HIER DEINE URL EINTRAGEN wenn du das Projekt auf GitHub hostest:
    private const string UpdateManifestUrl =
        "https://raw.githubusercontent.com/MarioB/ZeroTrace/main/update-manifest.json";

    public UpdateService(IZeroTraceLogger logger, HttpClient? httpClient = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _http = httpClient ?? new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    /// <summary>Current installed version.</summary>
    public Version CurrentVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 0, 0);

    /// <summary>Check if a newer version is available.</summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        _logger.Info($"Pruefe auf Updates... (aktuelle Version: {CurrentVersion})");
        try
        {
            var json = await _http.GetStringAsync(UpdateManifestUrl, ct);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (manifest is null) return null;

            var latest = new Version(manifest.LatestVersion);
            if (latest > CurrentVersion)
            {
                _logger.Info($"Update verfuegbar: {manifest.LatestVersion}");
                return new UpdateInfo
                {
                    CurrentVersion = CurrentVersion.ToString(),
                    LatestVersion = manifest.LatestVersion,
                    DownloadUrl = manifest.DownloadUrl,
                    ReleaseNotes = manifest.ReleaseNotes,
                    IsUpdateAvailable = true
                };
            }

            _logger.Info("Software ist aktuell.");
            return new UpdateInfo
            {
                CurrentVersion = CurrentVersion.ToString(),
                LatestVersion = manifest.LatestVersion,
                IsUpdateAvailable = false
            };
        }
        catch (Exception ex)
        {
            _logger.Warning($"Update-Check fehlgeschlagen: {ex.Message}");
            return null;
        }
    }
}

public sealed class UpdateManifest
{
    public string LatestVersion { get; set; } = "1.0.0";
    public string DownloadUrl   { get; set; } = "";
    public string ReleaseNotes  { get; set; } = "";
}

public sealed class UpdateInfo
{
    public required string CurrentVersion   { get; init; }
    public required string LatestVersion    { get; init; }
    public          string? DownloadUrl     { get; init; }
    public          string? ReleaseNotes    { get; init; }
    public required bool   IsUpdateAvailable { get; init; }
}
