using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

public sealed class RegistryDiscoveryProvider : IDiscoveryProvider
{

        public string ProviderName => "Windows Registry (HKLM/HKCU)";
    private readonly IZeroTraceLogger _logger;
    private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    private const string UninstallKeyWow64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

    public RegistryDiscoveryProvider(IZeroTraceLogger logger)
    {
        _logger = logger;
    }

    public async Task<List<InstalledProgram>> GetProgramsAsync()
    {
        return await Task.Run(() =>
        {
            var programs = new List<InstalledProgram>();

            programs.AddRange(ReadFromRegistry(RegistryHive.LocalMachine, UninstallKey));
            programs.AddRange(ReadFromRegistry(RegistryHive.LocalMachine, UninstallKeyWow64));
            programs.AddRange(ReadFromRegistry(RegistryHive.CurrentUser, UninstallKey));

            return programs
                .Where(p => !string.IsNullOrWhiteSpace(p.DisplayName))
                .GroupBy(
                    p => $"{p.DisplayName}|{p.DisplayVersion}|{p.Publisher}",
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        });
    }

    private IEnumerable<InstalledProgram> ReadFromRegistry(RegistryHive hive, string keyPath)
    {
        var result = new List<InstalledProgram>();

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var subKey = baseKey.OpenSubKey(keyPath);

            if (subKey is null)
                return result;

            foreach (var subKeyName in subKey.GetSubKeyNames())
            {
                using var appKey = subKey.OpenSubKey(subKeyName);
                if (appKey is null)
                    continue;

                var displayName = appKey.GetValue("DisplayName") as string;
                if (displayName == null) continue;

                var program = new InstalledProgram
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayName = displayName,
                    DisplayVersion = appKey.GetValue("DisplayVersion") as string ?? "0.0",
                    Publisher = appKey.GetValue("Publisher") as string ?? "Unbekannt",
                    InstallLocation = appKey.GetValue("InstallLocation") as string,
                    UninstallString = appKey.GetValue("UninstallString") as string,
                    QuietUninstallString = appKey.GetValue("QuietUninstallString") as string,
                    MsiProductCode = appKey.GetValue("ProductCode") as string,
                    Source = ProgramSource.RegistryLocalMachine
                };

                result.Add(program);
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Fehler beim Lesen der Registry ({hive}\\{keyPath})", ex);
        }

        return result;
    }
} // <--- Diese Klammer schließt die Klasse


