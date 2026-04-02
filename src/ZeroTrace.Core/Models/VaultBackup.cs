// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Models;

public sealed class VaultBackup
{
    public required string                   BackupId       { get; init; }
    public required DateTime                 CreatedAtUtc   { get; init; }
    public required string                   ProgramName    { get; init; }
    public          string?                  ProgramVersion { get; init; }
    public required IReadOnlyList<VaultEntry> Entries        { get; init; }
    public          long                     TotalSizeBytes { get; init; }
    public required string                   IntegrityHash  { get; init; }
    public          bool                     IsEncrypted    { get; init; }
    public          VaultBackupStatus        Status         { get; set; } = VaultBackupStatus.Active;

    public int SuccessCount => Entries.Count(e => e.BackupSuccessful);
    public int FailedCount  => Entries.Count(e => !e.BackupSuccessful);

    public string FormattedSize => TotalSizeBytes switch
    {
        < 1024                => $"{TotalSizeBytes} B",
        < 1024 * 1024         => $"{TotalSizeBytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{TotalSizeBytes / (1024.0 * 1024):F1} MB",
        _                     => $"{TotalSizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };

    public string FormattedDate =>
        CreatedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
}

public sealed class VaultEntry
{
    public required string         EntryId            { get; init; }
    public required string         OriginalPath       { get; init; }
    public required string         BackupRelativePath { get; init; }
    public required VaultEntryType EntryType          { get; init; }
    public          long?          SizeInBytes        { get; init; }
    public          string?        ContentHash        { get; init; }
    public          bool           BackupSuccessful   { get; set; }
    public          bool           HasBeenRestored    { get; set; }
    public          string?        ErrorMessage       { get; set; }
}

public enum VaultEntryType    { File, Directory, RegistryKey, RegistryValue }
public enum VaultBackupStatus { Active, PartiallyRestored, FullyRestored, Expired, Corrupted }
