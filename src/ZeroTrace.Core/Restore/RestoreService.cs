// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Runtime.Versioning;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Restore;

public sealed class RestoreResult
{
    public required int    TotalEntries { get; init; }
    public required int    Restored     { get; init; }
    public required int    Failed       { get; init; }
    public required TimeSpan Duration   { get; init; }
    public required IReadOnlyList<RestoreEntryResult> Details { get; init; }
}

public sealed class RestoreEntryResult
{
    public required string OriginalPath { get; init; }
    public required bool   Success      { get; init; }
    public          string? ErrorMessage { get; init; }
}

[SupportedOSPlatform("windows")]
public sealed class RestoreService
{
    private readonly string          _vaultPath;
    private readonly IZeroTraceLogger _logger;
    public event EventHandler<RestoreProgressEventArgs>? ProgressChanged;

    public RestoreService(IZeroTraceLogger logger, string? vaultPath = null)
    {
        _logger    = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultPath = vaultPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "Vault");
    }

    public async Task<RestoreResult> RestoreAsync(
        VaultBackup backup, CancellationToken ct = default)
    {
        _logger.Info($"Wiederherstellung: {backup.ProgramName} ({backup.BackupId})");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<RestoreEntryResult>();
        int ok = 0, fail = 0;
        var backupDir = Path.Combine(_vaultPath, backup.BackupId);

        for (int i = 0; i < backup.Entries.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var entry = backup.Entries[i];
            if (!entry.BackupSuccessful) continue;

            Report($"Stelle wieder her: {Path.GetFileName(entry.OriginalPath)}",
                backup.Entries.Count, i);
            try
            {
                switch (entry.EntryType)
                {
                    case VaultEntryType.File:
                        await RestoreFileAsync(backupDir, entry);
                        break;
                    case VaultEntryType.Directory:
                        await RestoreDirAsync(backupDir, entry);
                        break;
                    case VaultEntryType.RegistryKey:
                    case VaultEntryType.RegistryValue:
                        RestoreRegistry(backupDir, entry);
                        break;
                }
                entry.HasBeenRestored = true;
                ok++;
                details.Add(new RestoreEntryResult
                { OriginalPath = entry.OriginalPath, Success = true });
            }
            catch (Exception ex)
            {
                fail++;
                _logger.Warning($"  Fehler: {entry.OriginalPath} - {ex.Message}");
                details.Add(new RestoreEntryResult
                { OriginalPath = entry.OriginalPath, Success = false, ErrorMessage = ex.Message });
            }
        }

        sw.Stop();
        backup.Status = fail == 0
            ? VaultBackupStatus.FullyRestored
            : VaultBackupStatus.PartiallyRestored;

        _logger.Info($"  Ergebnis: {ok} OK, {fail} Fehler ({sw.Elapsed.TotalSeconds:F1}s)");
        return new RestoreResult
        {
            TotalEntries = backup.Entries.Count,
            Restored = ok, Failed = fail,
            Duration = sw.Elapsed, Details = details.AsReadOnly()
        };
    }

    private static async Task RestoreFileAsync(string backupDir, VaultEntry entry)
    {
        var src = Path.Combine(backupDir, "files", entry.BackupRelativePath);
        if (!File.Exists(src))
            throw new FileNotFoundException("Backup-Datei nicht gefunden", src);
        Directory.CreateDirectory(Path.GetDirectoryName(entry.OriginalPath)!);
        await Task.Run(() => File.Copy(src, entry.OriginalPath, overwrite: true));
    }

    private static async Task RestoreDirAsync(string backupDir, VaultEntry entry)
    {
        var src = Path.Combine(backupDir, "files", entry.BackupRelativePath);
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"Backup-Ordner nicht gefunden: {src}");
        await Task.Run(() =>
        {
            foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(src, file);
                var target = Path.Combine(entry.OriginalPath, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
        });
    }

    private static void RestoreRegistry(string backupDir, VaultEntry entry)
    {
        var regFile = Path.Combine(backupDir, entry.BackupRelativePath);
        if (!File.Exists(regFile))
            throw new FileNotFoundException("Registry-Backup nicht gefunden", regFile);
        using var proc = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = $"import \"{regFile}\"",
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardError = true
        });
        proc?.WaitForExit(15000);
        if (proc?.ExitCode != 0)
            throw new InvalidOperationException("Registry-Import fehlgeschlagen");
    }

    private void Report(string msg, int total, int current) =>
        ProgressChanged?.Invoke(this, new RestoreProgressEventArgs
        { Message = msg, TotalItems = total, CompletedItems = current });
}

public sealed class RestoreProgressEventArgs : EventArgs
{
    public required string Message        { get; init; }
    public required int    TotalItems     { get; init; }
    public required int    CompletedItems { get; init; }
}
