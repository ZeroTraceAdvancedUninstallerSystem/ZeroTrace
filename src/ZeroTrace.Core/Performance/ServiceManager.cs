// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.ServiceProcess;
using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Performance;

/// <summary>
/// Lists, starts, and stops Windows services.
/// Helps reduce background resource usage.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ServiceManager
{
    private readonly IZeroTraceLogger _logger;

    public ServiceManager(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Get all Windows services with their status.</summary>
    public List<ServiceInfo> GetServices()
    {
        _logger.Info("Lese Windows-Dienste...");
        return ServiceController.GetServices()
            .Select(s => new ServiceInfo
            {
                ServiceName = s.ServiceName,
                DisplayName = s.DisplayName,
                Status = s.Status.ToString(),
                CanStop = s.CanStop
            })
            .OrderBy(s => s.DisplayName)
            .ToList();
    }

    /// <summary>Stop a running service.</summary>
    public bool StopService(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                _logger.Info($"Dienst bereits gestoppt: {serviceName}");
                return true;
            }
            if (!sc.CanStop)
            {
                _logger.Warning($"Dienst kann nicht gestoppt werden: {serviceName}");
                return false;
            }

            _logger.Info($"Stoppe Dienst: {serviceName}...");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
            _logger.Info($"Dienst gestoppt: {serviceName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Fehler beim Stoppen von {serviceName}", ex);
            return false;
        }
    }

    /// <summary>Start a stopped service.</summary>
    public bool StartService(string serviceName)
    {
        try
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status == ServiceControllerStatus.Running)
            {
                _logger.Info($"Dienst laeuft bereits: {serviceName}");
                return true;
            }

            _logger.Info($"Starte Dienst: {serviceName}...");
            sc.Start();
            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
            _logger.Info($"Dienst gestartet: {serviceName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error($"Fehler beim Starten von {serviceName}", ex);
            return false;
        }
    }
}

public sealed class ServiceInfo
{
    public required string ServiceName { get; init; }
    public required string DisplayName { get; init; }
    public required string Status      { get; init; }
    public required bool   CanStop     { get; init; }
}
