using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Vault;

public interface IVaultService
{
    Task<VaultBackup> CreateBackupAsync(
        InstalledProgram program,
        IReadOnlyCollection<ResidualItem> items,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyBackupIntegrityAsync(
        VaultBackup backup,
        CancellationToken cancellationToken = default);
}
