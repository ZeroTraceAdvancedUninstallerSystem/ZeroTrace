using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Restore;

public interface IRestoreService
{
    Task RestoreBackupAsync(
        VaultBackup backup,
        string password,
        CancellationToken cancellationToken = default);
}
