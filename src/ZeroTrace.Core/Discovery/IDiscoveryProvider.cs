using ZeroTrace.Core.Models;
namespace ZeroTrace.Core.Discovery;
public interface IDiscoveryProvider
{
    string ProviderName { get; }
    Task<List<InstalledProgram>> GetProgramsAsync();
}
