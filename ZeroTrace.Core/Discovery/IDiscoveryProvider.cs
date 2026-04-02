using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Discovery;

public interface IDiscoveryProvider
{
    string Name { get; }

    Task<IReadOnlyCollection<InstalledProgram>> DiscoverInstalledProgramsAsync(
        CancellationToken cancellationToken = default);
}
