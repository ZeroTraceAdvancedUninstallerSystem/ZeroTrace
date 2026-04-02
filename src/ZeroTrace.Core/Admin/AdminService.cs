// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Admin;

public sealed class SystemHealthReport
{
    public required DateTime TimestampUtc        { get; init; }
    public required string   MachineName         { get; init; }
    public required string   OsVersion           { get; init; }
    public required bool     IsAdminMode         { get; init; }
    public required long     TotalDiskBytes      { get; init; }
    public required long     FreeDiskBytes       { get; init; }
    public required long     UsedMemoryBytes     { get; init; }
    public required long     TotalMemoryBytes    { get; init; }
    public required int      RunningProcesses    { get; init; }
    public required int      VaultBackupCount    { get; init; }
    public required long     VaultTotalSizeBytes { get; init; }
    public required long     LogTotalSizeBytes   { get; init; }
    public required string   ZeroTraceVersion    { get; init; }

    public double DiskUsagePercent =>
        TotalDiskBytes > 0 ? (1.0 - (double)FreeDiskBytes / TotalDiskBytes) * 100 : 0;
    public double MemoryUsagePercent =>
        TotalMemoryBytes > 0 ? (double)UsedMemoryBytes / TotalMemoryBytes * 100 : 0;

    public string FormattedFreeDisk => FormatSize(FreeDiskBytes);
    public string FormattedVaultSize => FormatSize(VaultTotalSizeBytes);
    public string FormattedLogSize => FormatSize(LogTotalSizeBytes);

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024                => $"{bytes} B",
        < 1024 * 1024         => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024  => $"{bytes / (1024.0 * 1024):F1} MB",
        _                     => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

public sealed class AdminConfig
{
    public int  MaxVaultAgeDays        { get; set; } = 30;
    public long MaxVaultSizeBytes      { get; set; } = 2L * 1024 * 1024 * 1024;
    public int  MaxLogAgeDays          { get; set; } = 14;
    public bool AutoCleanExpiredVaults { get; set; } = true;
    public bool EnableDebugLogging     { get; set; }
    public string? CustomVaultPath     { get; set; }
    public string? CustomLogPath       { get; set; }
}

[SupportedOSPlatform("windows")]
public sealed class AdminService
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _configPath;
    private readonly string _vaultPath;
    private readonly string _logPath;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AdminService(IZeroTraceLogger logger,
        string? vaultPath = null, string? logPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace");
        _configPath = Path.Combine(appData, "admin-config.json");
        _vaultPath = vaultPath ?? Path.Combine(appData, "Vault");
        _logPath = logPath ?? Path.Combine(appData, "Logs");
    }

    public static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RestartAsAdmin()
    {
        var exe = Environment.ProcessPath
            ?? throw new InvalidOperationException("Prozesspfad nicht ermittelbar.");
        Process.Start(new ProcessStartInfo
        {
            FileName = exe, UseShellExecute = true, Verb = "runas"
        });
        Environment.Exit(0);
    }

    public SystemHealthReport GetHealthReport()
    {
        var sysDrive = new DriveInfo(Path.GetPathRoot(
            Environment.GetFolderPath(Environment.SpecialFolder.System)) ?? "C:");
        long vaultSize = GetDirectorySize(_vaultPath);
        int vaultCount = Directory.Exists(_vaultPath)
            ? Directory.GetDirectories(_vaultPath).Length : 0;
        long logSize = GetDirectorySize(_logPath);
        var version = typeof(AdminService).Assembly.GetName().Version;
        return new SystemHealthReport
        {
            TimestampUtc = DateTime.UtcNow,
            MachineName = Environment.MachineName,
            OsVersion = Environment.OSVersion.VersionString,
            IsAdminMode = IsRunningAsAdmin(),
            TotalDiskBytes = sysDrive.TotalSize,
            FreeDiskBytes = sysDrive.AvailableFreeSpace,
            UsedMemoryBytes = Process.GetCurrentProcess().WorkingSet64,
            TotalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            RunningProcesses = Process.GetProcesses().Length,
            VaultBackupCount = vaultCount,
            VaultTotalSizeBytes = vaultSize,
            LogTotalSizeBytes = logSize,
            ZeroTraceVersion = version?.ToString() ?? "1.0.0"
        };
    }

    public int PurgeExpiredVaults(int maxAgeDays)
    {
        if (!Directory.Exists(_vaultPath)) return 0;
        var cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
        int deleted = 0;
        foreach (var dir in Directory.GetDirectories(_vaultPath))
        {
            var manifest = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                var created = File.GetCreationTimeUtc(manifest);
                if (created < cutoff)
                {
                    Directory.Delete(dir, recursive: true);
                    deleted++;
                    _logger.Info($"Vault bereinigt: {Path.GetFileName(dir)} (erstellt: {created:dd.MM.yyyy})");
                }
            }
            catch (Exception ex)
            { _logger.Warning($"Vault-Bereinigung fehlgeschlagen: {dir} - {ex.Message}"); }
        }
        _logger.Info($"Vault-Wartung: {deleted} abgelaufene Backups geloescht");
        return deleted;
    }

    public int PurgeOldLogs(int maxAgeDays)
    {
        if (!Directory.Exists(_logPath)) return 0;
        var cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
        int deleted = 0;
        foreach (var file in Directory.GetFiles(_logPath, "*.log"))
        {
            try
            {
                if (File.GetCreationTimeUtc(file) < cutoff)
                { File.Delete(file); deleted++; }
            }
            catch (Exception ex)
            { _logger.Warning($"Log-Bereinigung: {file} - {ex.Message}"); }
        }
        _logger.Info($"Log-Wartung: {deleted} alte Logs geloescht");
        return deleted;
    }

    public AdminConfig LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AdminConfig>(json, Json) ?? new AdminConfig();
            }
        }
        catch (Exception ex)
        { _logger.Warning($"Config laden fehlgeschlagen: {ex.Message}"); }
        return new AdminConfig();
    }

    public void SaveConfig(AdminConfig config)
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_configPath, JsonSerializer.Serialize(config, Json));
            _logger.Info("Admin-Konfiguration gespeichert");
        }
        catch (Exception ex)
        { _logger.Error("Config speichern fehlgeschlagen", ex); }
    }

    public void RunAutoMaintenance()
    {
        var config = LoadConfig();
        _logger.Info("Auto-Wartung gestartet...");
        if (config.AutoCleanExpiredVaults)
            PurgeExpiredVaults(config.MaxVaultAgeDays);
        PurgeOldLogs(config.MaxLogAgeDays);
        _logger.Info("Auto-Wartung abgeschlossen");
    }

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0L; } });
        }
        catch { return 0; }
    }
}
