using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;
namespace ZeroTrace.Core.Discovery;
public class DiscoveryService
{
    private readonly IZeroTraceLogger _logger;
    private readonly IEnumerable<IDiscoveryProvider> _providers;
    public DiscoveryService(IZeroTraceLogger logger, IEnumerable<IDiscoveryProvider> providers)
    {
        _logger = logger;
        _providers = providers;
    }
    public async Task<List<InstalledProgram>> GetAllInstalledProgramsAsync()
    {
        var allPrograms = new List<InstalledProgram>();
        foreach (var provider in _providers)
        {
            try
            {
                var programs = await provider.GetProgramsAsync();
                allPrograms.AddRange(programs);
                _logger.Info($"{provider.ProviderName}: {programs.Count} Programme");
            }
            catch (Exception ex)
            {
                _logger.Error($"Provider {provider.ProviderName} fehlgeschlagen", ex);
            }
        }
        return allPrograms;
    }
}
