// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Statistics;

/// <summary>
/// Tracks and persists usage statistics for ZeroTrace.
/// Shows the user how much cleanup ZeroTrace has done over time.
/// All data stays local - nothing is sent anywhere.
/// </summary>
public sealed class UsageStatistics
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _statsFilePath;
    private StatsData _data;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public UsageStatistics(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _statsFilePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "usage-stats.json");
        _data = Load();
    }

    // ── Read Stats ───────────────────────────────────────────────

    public int    TotalScans         => _data.TotalScans;
    public int    TotalUninstalls    => _data.TotalUninstalls;
    public int    TotalCleanups      => _data.TotalCleanups;
    public long   TotalBytesFreed    => _data.TotalBytesFreed;
    public int    TotalFilesDeleted  => _data.TotalFilesDeleted;
    public int    TotalRegistryKeys  => _data.TotalRegistryKeysRemoved;
    public int    TotalBackups       => _data.TotalBackupsCreated;
    public int    TotalRestores      => _data.TotalRestores;
    public DateTime FirstUsedUtc     => _data.FirstUsedUtc;
    public DateTime LastUsedUtc      => _data.LastUsedUtc;

    public string FormattedBytesFreed => _data.TotalBytesFreed switch
    {
        < 1024              => $"{_data.TotalBytesFreed} B",
        < 1024 * 1024       => $"{_data.TotalBytesFreed / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{_data.TotalBytesFreed / (1024.0 * 1024):F1} MB",
        _                   => $"{_data.TotalBytesFreed / (1024.0 * 1024 * 1024):F2} GB"
    };

    public int DaysSinceFirstUse =>
        (int)(DateTime.UtcNow - _data.FirstUsedUtc).TotalDays;

    // ── Record Events ────────────────────────────────────────────

    public void RecordScan(int programsFound)
    {
        _data.TotalScans++;
        _data.LastScanProgramCount = programsFound;
        Touch();
    }

    public void RecordUninstall(string programName)
    {
        _data.TotalUninstalls++;
        _data.LastUninstalledProgram = programName;
        Touch();
    }

    public void RecordCleanup(int filesDeleted, int registryKeys, long bytesFreed)
    {
        _data.TotalCleanups++;
        _data.TotalFilesDeleted += filesDeleted;
        _data.TotalRegistryKeysRemoved += registryKeys;
        _data.TotalBytesFreed += bytesFreed;
        Touch();
    }

    public void RecordBackup()
    {
        _data.TotalBackupsCreated++;
        Touch();
    }

    public void RecordRestore()
    {
        _data.TotalRestores++;
        Touch();
    }

    // ── Persistence ──────────────────────────────────────────────

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_statsFilePath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_statsFilePath, JsonSerializer.Serialize(_data, Json));
        }
        catch (Exception ex)
        {
            _logger.Warning($"Statistik speichern fehlgeschlagen: {ex.Message}");
        }
    }

    public void Reset()
    {
        _data = new StatsData { FirstUsedUtc = DateTime.UtcNow };
        Save();
        _logger.Info("Statistiken zurueckgesetzt");
    }

    private StatsData Load()
    {
        try
        {
            if (File.Exists(_statsFilePath))
            {
                var json = File.ReadAllText(_statsFilePath);
                return JsonSerializer.Deserialize<StatsData>(json, Json) ?? new StatsData();
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"Statistik laden fehlgeschlagen: {ex.Message}");
        }
        return new StatsData();
    }

    private void Touch()
    {
        _data.LastUsedUtc = DateTime.UtcNow;
        Save();
    }
}

internal sealed class StatsData
{
    public int      TotalScans              { get; set; }
    public int      TotalUninstalls         { get; set; }
    public int      TotalCleanups           { get; set; }
    public long     TotalBytesFreed         { get; set; }
    public int      TotalFilesDeleted       { get; set; }
    public int      TotalRegistryKeysRemoved { get; set; }
    public int      TotalBackupsCreated     { get; set; }
    public int      TotalRestores           { get; set; }
    public DateTime FirstUsedUtc            { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedUtc             { get; set; } = DateTime.UtcNow;
    public int      LastScanProgramCount    { get; set; }
    public string?  LastUninstalledProgram  { get; set; }
}
