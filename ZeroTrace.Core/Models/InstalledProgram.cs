namespace ZeroTrace.Core.Models;

public sealed class InstalledProgram
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string DisplayName { get; set; } = string.Empty;
    public string? DisplayVersion { get; set; }

    public string? Publisher { get; set; }

    public string? InstallLocation { get; set; }
    public string? UninstallString { get; set; }
    public string? QuietUninstallString { get; set; }
    public string? RegistryKeyPath { get; set; }

    public string Architecture { get; set; } = "Unknown";
    public DateTimeOffset? InstallDate { get; set; }

    public bool IsSystemComponent { get; set; }
    public long? EstimatedSizeBytes { get; set; }

    public ProgramSource Source { get; set; } = ProgramSource.Unknown;

    public string? ProductCode { get; set; }
    public string? MsiProductCode { get; set; }
    public string? InstallSource { get; set; }
    public string? IconPath { get; set; }

    public override string ToString()
        => $"{DisplayName} {DisplayVersion}".Trim();
}
