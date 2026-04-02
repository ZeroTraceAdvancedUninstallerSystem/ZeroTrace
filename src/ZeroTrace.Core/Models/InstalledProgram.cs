// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Models;

/// <summary>Represents a program found on the system.</summary>
public sealed class InstalledProgram
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public required string DisplayName       { get; init; }
    public string?         DisplayVersion    { get; init; }
    public string?         Publisher         { get; init; }
    public string?         IconPath          { get; init; }
    public string?         InstallLocation   { get; init; }
    public string?         UninstallString   { get; init; }
    public string?         QuietUninstallString { get; init; }
    public string?         RegistryKeyPath   { get; init; }
    public string?         InstallSource     { get; init; }
    public string          Architecture      { get; init; } = "Unknown";
    public DateTimeOffset? InstallDate       { get; init; }
    public bool            IsSystemComponent { get; init; }
    public long?           EstimatedSizeBytes { get; init; }
    public ProgramSource   Source            { get; init; } = ProgramSource.Unknown;
    public string?         ProductCode       { get; init; }
    public string?         MsiProductCode    { get; init; }

    // Computed
    public bool IsMsiInstallation => !string.IsNullOrEmpty(MsiProductCode);
    public bool CanBeUninstalled =>
        !string.IsNullOrEmpty(UninstallString) ||
        !string.IsNullOrEmpty(QuietUninstallString) ||
        IsMsiInstallation;

    public string FormattedSize => EstimatedSizeBytes switch
    {
        null                    => "-",
        < 1024                  => $"{EstimatedSizeBytes} B",
        < 1024 * 1024           => $"{EstimatedSizeBytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024   => $"{EstimatedSizeBytes / (1024.0 * 1024):F1} MB",
        _                       => $"{EstimatedSizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };

    public string FormattedInstallDate =>
        InstallDate?.LocalDateTime.ToString("dd.MM.yyyy") ?? "-";

    public override string ToString() => $"{DisplayName} {DisplayVersion}".Trim();
}
