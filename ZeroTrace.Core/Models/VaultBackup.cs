namespace ZeroTrace.Core.Models;

public sealed class VaultBackup
{
    public string BackupId { get; set; } = Guid.NewGuid().ToString("N");
    public string? ProgramId { get; set; }

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public string ContainerPath { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = "AES-256-GCM";

    public string? ManifestHash { get; set; }
    public string? IntegrityRootHash { get; set; }

    public long BackupSizeBytes { get; set; }

    public bool IsEncrypted { get; set; } = true;
    public bool IntegrityVerified { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
