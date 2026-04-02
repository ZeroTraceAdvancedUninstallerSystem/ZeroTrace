// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.RestorePoints;

/// <summary>
/// Creates and manages Windows System Restore Points.
/// A restore point should be created before major cleanup operations
/// as an additional safety net beyond the ZeroTrace Vault.
/// Requires Administrator privileges.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RestorePointManager
{
    private readonly IZeroTraceLogger _logger;

    public RestorePointManager(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Create a Windows System Restore Point.
    /// Uses PowerShell's Checkpoint-Computer cmdlet internally.
    /// Returns true if the restore point was created successfully.
    /// </summary>
    public async Task<RestorePointResult> CreateRestorePointAsync(
        string description = "ZeroTrace Sicherungspunkt",
        CancellationToken ct = default)
    {
        _logger.Info($"Erstelle Wiederherstellungspunkt: {description}");
        var sw = Stopwatch.StartNew();

        try
        {
            // Use PowerShell to create restore point
            var script = $"Checkpoint-Computer -Description '{description.Replace("'", "''")}' -RestorePointType 'MODIFY_SETTINGS'";

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return new RestorePointResult
                {
                    Success = false, Duration = sw.Elapsed,
                    ErrorMessage = "PowerShell konnte nicht gestartet werden"
                };
            }

            var output = await proc.StandardOutput.ReadToEndAsync(ct);
            var error = await proc.StandardError.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            sw.Stop();
            bool success = proc.ExitCode == 0;

            if (success)
                _logger.Info($"Wiederherstellungspunkt erstellt ({sw.Elapsed.TotalSeconds:F1}s)");
            else
                _logger.Warning($"Wiederherstellungspunkt fehlgeschlagen: {error.Trim()}");

            return new RestorePointResult
            {
                Success = success,
                Duration = sw.Elapsed,
                Description = description,
                ErrorMessage = success ? null : error.Trim()
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.Error("Fehler beim Erstellen des Wiederherstellungspunkts", ex);
            return new RestorePointResult
            {
                Success = false, Duration = sw.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// List existing restore points via PowerShell.
    /// </summary>
    public async Task<List<RestorePointInfo>> GetRestorePointsAsync(
        CancellationToken ct = default)
    {
        var points = new List<RestorePointInfo>();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -Command \"Get-ComputerRestorePoint | Select-Object SequenceNumber, Description, CreationTime | ConvertTo-Json\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var proc = Process.Start(psi);
            if (proc is null) return points;

            var json = await proc.StandardOutput.ReadToEndAsync(ct);
            await proc.WaitForExitAsync(ct);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var items = System.Text.Json.JsonSerializer.Deserialize<List<RestorePointRaw>>(json);
                if (items is not null)
                {
                    foreach (var item in items)
                    {
                        points.Add(new RestorePointInfo
                        {
                            SequenceNumber = item.SequenceNumber,
                            Description = item.Description ?? "Unbekannt",
                            CreationTime = item.CreationTime
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Wiederherstellungspunkte auslesen fehlgeschlagen: {ex.Message}");
        }

        _logger.Info($"Wiederherstellungspunkte: {points.Count} gefunden");
        return points;
    }

    /// <summary>Check if System Restore is enabled on the system drive.</summary>
    public bool IsSystemRestoreEnabled()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -Command \"(Get-ComputerRestorePoint -ErrorAction SilentlyContinue) -ne $null\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            using var proc = Process.Start(psi);
            if (proc is null) return false;

            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit(5000);
            return output.Equals("True", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

public sealed class RestorePointResult
{
    public required bool     Success      { get; init; }
    public required TimeSpan Duration     { get; init; }
    public          string?  Description  { get; init; }
    public          string?  ErrorMessage { get; init; }
}

public sealed class RestorePointInfo
{
    public required int      SequenceNumber { get; init; }
    public required string   Description    { get; init; }
    public required DateTime CreationTime   { get; init; }

    public string FormattedDate =>
        CreationTime.ToString("dd.MM.yyyy HH:mm");
}

// Internal DTO for JSON deserialization from PowerShell
internal sealed class RestorePointRaw
{
    public int      SequenceNumber { get; set; }
    public string?  Description    { get; set; }
    public DateTime CreationTime   { get; set; }
}
