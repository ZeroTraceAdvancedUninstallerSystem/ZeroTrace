// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Cloud;

/// <summary>
/// Synchronizes ZeroTrace vault backups to a cloud folder (OneDrive, Dropbox, Google Drive).
/// The user chooses the sync folder - ZeroTrace copies encrypted backups there.
/// The actual cloud sync is handled by the cloud provider's desktop app.
/// This approach works offline and doesn't require API keys.
/// </summary>
public sealed class BackupSyncService
{
    private readonly IZeroTraceLogger _logger;
    private readonly string _configPath;
    private SyncConfig _config;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BackupSyncService(IZeroTraceLogger logger, string? configPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configPath = configPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "cloud-sync-config.json");
        _config = LoadConfig();
    }

    /// <summary>Whether cloud sync is configured and enabled.</summary>
    public bool IsEnabled => _config.IsEnabled && !string.IsNullOrEmpty(_config.SyncFolderPath);

    /// <summary>The configured cloud sync folder path.</summary>
    public string? SyncFolderPath => _config.SyncFolderPath;

    /// <summary>When the last sync was performed.</summary>
    public DateTime? LastSyncUtc => _config.LastSyncUtc;

    /// <summary>
    /// Configure the cloud sync folder.
    /// Point this to your OneDrive, Dropbox, or Google Drive folder.
    /// </summary>
    public bool SetSyncFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            _logger.Warning($"Cloud-Sync: Ordner existiert nicht: {folderPath}");
            return false;
        }

        _config.SyncFolderPath = folderPath;
        _config.IsEnabled = true;
        SaveConfig();
        _logger.Info($"Cloud-Sync konfiguriert: {folderPath}");
        return true;
    }

    /// <summary>Auto-detect common cloud folders on the system.</summary>
    public List<CloudFolderInfo> DetectCloudFolders()
    {
        var folders = new List<CloudFolderInfo>();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var candidates = new[]
        {
            (Name: "OneDrive", Path: Path.Combine(userProfile, "OneDrive")),
            (Name: "OneDrive Business", Path: Path.Combine(userProfile, "OneDrive - Business")),
            (Name: "Dropbox", Path: Path.Combine(userProfile, "Dropbox")),
            (Name: "Google Drive", Path: Path.Combine(userProfile, "Google Drive")),
            (Name: "Google Drive (Stream)", Path: @"G:\My Drive"),
            (Name: "iCloud Drive", Path: Path.Combine(userProfile, "iCloudDrive")),
        };

        foreach (var (name, path) in candidates)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var di = new DriveInfo(Path.GetPathRoot(path) ?? "C:");
                    folders.Add(new CloudFolderInfo
                    {
                        ProviderName = name,
                        FolderPath = path,
                        FreeSpaceBytes = di.AvailableFreeSpace
                    });
                }
                catch
                {
                    folders.Add(new CloudFolderInfo
                    {
                        ProviderName = name,
                        FolderPath = path,
                        FreeSpaceBytes = 0
                    });
                }
            }
        }

        _logger.Info($"Cloud-Erkennung: {folders.Count} Anbieter gefunden");
        return folders;
    }

    /// <summary>
    /// Sync a vault backup to the cloud folder.
    /// Copies the backup directory to {CloudFolder}/ZeroTrace-Backups/{backupId}/
    /// </summary>
    public async Task<CloudSyncResult> SyncBackupAsync(
        string vaultBackupPath, string backupId, CancellationToken ct = default)
    {
        if (!IsEnabled)
            return new CloudSyncResult { Success = false, ErrorMessage = "Cloud-Sync nicht konfiguriert" };

        if (!Directory.Exists(vaultBackupPath))
            return new CloudSyncResult { Success = false, ErrorMessage = "Backup-Ordner nicht gefunden" };

        _logger.Info($"Cloud-Sync: Synchronisiere Backup {backupId}...");

        try
        {
            var destBase = Path.Combine(_config.SyncFolderPath!, "ZeroTrace-Backups", backupId);
            Directory.CreateDirectory(destBase);

            long totalBytes = 0;
            int fileCount = 0;

            foreach (var file in Directory.EnumerateFiles(vaultBackupPath, "*", SearchOption.AllDirectories))
            {
                ct.ThrowIfCancellationRequested();

                var relative = Path.GetRelativePath(vaultBackupPath, file);
                var destFile = Path.Combine(destBase, relative);
                var destDir = Path.GetDirectoryName(destFile);
                if (destDir is not null) Directory.CreateDirectory(destDir);

                await Task.Run(() => File.Copy(file, destFile, overwrite: true), ct);
                totalBytes += new FileInfo(file).Length;
                fileCount++;
            }

            // Write sync marker
            var marker = new
            {
                backupId,
                syncedUtc = DateTime.UtcNow,
                fileCount,
                totalBytes,
                sourceHash = ComputeFolderHash(vaultBackupPath)
            };
            await File.WriteAllTextAsync(
                Path.Combine(destBase, ".zerotrace-sync"),
                JsonSerializer.Serialize(marker, Json), ct);

            _config.LastSyncUtc = DateTime.UtcNow;
            _config.TotalSyncedBackups++;
            SaveConfig();

            _logger.Info($"Cloud-Sync erfolgreich: {fileCount} Dateien, {totalBytes / 1024} KB");

            return new CloudSyncResult
            {
                Success = true,
                FilesCopied = fileCount,
                BytesCopied = totalBytes
            };
        }
        catch (Exception ex)
        {
            _logger.Error($"Cloud-Sync fehlgeschlagen: {ex.Message}");
            return new CloudSyncResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>Disable cloud sync.</summary>
    public void Disable()
    {
        _config.IsEnabled = false;
        SaveConfig();
        _logger.Info("Cloud-Sync deaktiviert");
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static string ComputeFolderHash(string folderPath)
    {
        try
        {
            var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                .OrderBy(f => f).ToList();
            var combined = string.Join("|", files.Select(f => $"{f}:{new FileInfo(f).Length}"));
            return Convert.ToHexString(SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(combined)))[..16];
        }
        catch { return "unknown"; }
    }

    private void SaveConfig()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_configPath, JsonSerializer.Serialize(_config, Json));
        }
        catch { }
    }

    private SyncConfig LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<SyncConfig>(json, Json) ?? new();
            }
        }
        catch { }
        return new();
    }
}

// ── Data Models ──────────────────────────────────────────────────

internal sealed class SyncConfig
{
    public bool IsEnabled { get; set; }
    public string? SyncFolderPath { get; set; }
    public DateTime? LastSyncUtc { get; set; }
    public int TotalSyncedBackups { get; set; }
}

public sealed class CloudFolderInfo
{
    public required string ProviderName   { get; init; }
    public required string FolderPath     { get; init; }
    public required long   FreeSpaceBytes { get; init; }

    public string FormattedFreeSpace => FreeSpaceBytes switch
    {
        < 1024L * 1024 * 1024 => $"{FreeSpaceBytes / (1024.0 * 1024):F0} MB frei",
        _                     => $"{FreeSpaceBytes / (1024.0 * 1024 * 1024):F1} GB frei"
    };
}

public sealed class CloudSyncResult
{
    public required bool   Success      { get; init; }
    public          int    FilesCopied  { get; init; }
    public          long   BytesCopied  { get; init; }
    public          string? ErrorMessage { get; init; }
}
