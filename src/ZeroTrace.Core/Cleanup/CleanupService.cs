using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Cleanup;

public sealed class CleanupResult
{
    public required int TotalItems { get; init; }
    public required int SuccessfullyDeleted { get; init; }
    public required int Failed { get; init; }
    public required int Skipped { get; init; }
    public required long FreedBytes { get; init; }
    public required TimeSpan Duration { get; init; }
    public required IReadOnlyList<CleanupItemResult> Details { get; init; }
    public required string BackupId { get; init; }

    public string FormattedFreedSize
    {
        get
        {
            if (FreedBytes < 1024) return FreedBytes + " B";
            if (FreedBytes < 1024 * 1024) return (FreedBytes / 1024.0).ToString("F1") + " KB";
            return (FreedBytes / (1024.0 * 1024)).ToString("F1") + " MB";
        }
    }
}

public sealed class CleanupItemResult
{
    public required string Path { get; init; }
    public required ResidualItemType ItemType { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool ScheduledForReboot { get; init; }
}

[SupportedOSPlatform("windows")]
public sealed class CleanupService
{
    private readonly IZeroTraceLogger _logger;
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);
    public event EventHandler<CleanupProgressEventArgs>? ProgressChanged;

    public CleanupService(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CleanupResult> CleanAsync(IReadOnlyList<ResidualItem> items,
        string backupId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupId))
            throw new InvalidOperationException("SICHERHEITSSPERRE: Loeschung ohne Backup-ID verboten!");

        var selected = items.Where(i => i.IsSelectedForDeletion).ToList();
        _logger.Info("Bereinigung: " + selected.Count + " Elemente, Backup-ID: " + backupId);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<CleanupItemResult>();
        int ok = 0, fail = 0, skip = 0;
        long freed = 0;

        var ordered = selected.OrderBy(i => i.ItemType switch
        {
            ResidualItemType.File => 0, ResidualItemType.RegistryValue => 1,
            ResidualItemType.RegistryKey => 2, ResidualItemType.Directory => 3, _ => 4
        }).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = ordered[i];
            ReportProgress(item.FullPath, ordered.Count, i);

            try
            {
                var result = await DeleteItemAsync(item);
                details.Add(result);
                if (result.Success) { ok++; freed += item.SizeInBytes ?? 0; }
                else { skip++; }
            }
            catch (Exception ex)
            {
                fail++;
                _logger.Warning("  Fehler: " + item.FullPath + " - " + ex.Message);
                details.Add(new CleanupItemResult
                { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = ex.Message });
            }
        }

        sw.Stop();
        var result2 = new CleanupResult
        {
            TotalItems = ordered.Count, SuccessfullyDeleted = ok, Failed = fail,
            Skipped = skip, FreedBytes = freed, Duration = sw.Elapsed,
            Details = details.AsReadOnly(), BackupId = backupId
        };
        _logger.Info("  Ergebnis: " + ok + " OK, " + fail + " Fehler, " + skip +
            " uebersprungen | " + result2.FormattedFreedSize + " frei");
        return result2;
    }

    private async Task<CleanupItemResult> DeleteItemAsync(ResidualItem item) => item.ItemType switch
    {
        ResidualItemType.File => await DeleteFileAsync(item),
        ResidualItemType.Directory => DeleteDirectory(item),
        ResidualItemType.RegistryKey => DeleteRegistryKey(item),
        ResidualItemType.RegistryValue => DeleteRegistryValue(item),
        _ => new CleanupItemResult { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = "Unbekannter Typ" }
    };

    private async Task<CleanupItemResult> DeleteFileAsync(ResidualItem item)
    {
        if (!File.Exists(item.FullPath))
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Existiert nicht mehr" };

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var attrs = File.GetAttributes(item.FullPath);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(item.FullPath, attrs & ~FileAttributes.ReadOnly);
                File.Delete(item.FullPath);
                return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = true };
            }
            catch (IOException) when (attempt < MaxRetries) { await Task.Delay(RetryDelay); }
            catch (UnauthorizedAccessException)
            {
                bool scheduled = ScheduleRebootDelete(item.FullPath);
                return new CleanupItemResult
                {
                    Path = item.FullPath, ItemType = ResidualItemType.File, Success = false,
                    ErrorMessage = scheduled ? "Wird nach Neustart geloescht" : "Zugriff verweigert",
                    ScheduledForReboot = scheduled
                };
            }
        }
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Nach Wiederholung nicht loeschbar" };
    }

    private CleanupItemResult DeleteDirectory(ResidualItem item)
    {
        if (!Directory.Exists(item.FullPath))
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = false, ErrorMessage = "Existiert nicht mehr" };
        Directory.Delete(item.FullPath, recursive: true);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = true };
    }

    private CleanupItemResult DeleteRegistryKey(ResidualItem item)
    {
        var parsed = ParseRegistryPath(item.FullPath);
        if (parsed.Hive is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Pfad nicht erkannt" };

        int lastSlash = parsed.SubPath.LastIndexOf('\\');
        if (lastSlash < 0)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Ungueltiger Pfad" };

        string parentPath = parsed.SubPath.Substring(0, lastSlash);
        string keyName = parsed.SubPath.Substring(lastSlash + 1);

        using var baseKey = RegistryKey.OpenBaseKey(parsed.Hive.Value, RegistryView.Default);
        using var parentKey = baseKey.OpenSubKey(parentPath, writable: true);
        if (parentKey is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Parent-Key nicht gefunden" };

        parentKey.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = true };
    }

    private CleanupItemResult DeleteRegistryValue(ResidualItem item)
    {
        int sep = item.FullPath.LastIndexOf("::");
        if (sep < 0)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Ungueltiges Format" };

        string keyPath = item.FullPath.Substring(0, sep);
        string valueName = item.FullPath.Substring(sep + 2);

        var parsed = ParseRegistryPath(keyPath);
        if (parsed.Hive is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Pfad nicht erkannt" };

        using var baseKey = RegistryKey.OpenBaseKey(parsed.Hive.Value, RegistryView.Default);
        using var key = baseKey.OpenSubKey(parsed.SubPath, writable: true);
        if (key is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Key nicht gefunden" };

        key.DeleteValue(valueName, throwOnMissingValue: false);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = true };
    }

    private bool ScheduleRebootDelete(string path)
    {
        try { return NativeMethods.MoveFileExW(path, null, NativeMethods.MOVEFILE_DELAY_UNTIL_REBOOT); }
        catch { return false; }
    }

    private static (RegistryHive? Hive, string SubPath) ParseRegistryPath(string path)
    {
        if (path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.CurrentUser, path.Substring(18));
        if (path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.LocalMachine, path.Substring(19));
        return (null, path);
    }

    private void ReportProgress(string path, int total, int current)
    {
        ProgressChanged?.Invoke(this, new CleanupProgressEventArgs
        { CurrentPath = path, TotalItems = total, CompletedItems = current });
    }
}

public sealed class CleanupProgressEventArgs : EventArgs
{
    public required string CurrentPath { get; init; }
    public required int TotalItems { get; init; }
    public required int CompletedItems { get; init; }
    public int PercentComplete => TotalItems == 0 ? 0 : (int)(CompletedItems * 100.0 / TotalItems);
}

internal static partial class NativeMethods
{
    internal const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;

    [System.Runtime.InteropServices.LibraryImport("kernel32.dll",
        SetLastError = true,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial bool MoveFileExW(
        string lpExistingFileName, string? lpNewFileName, int dwFlags);
}
