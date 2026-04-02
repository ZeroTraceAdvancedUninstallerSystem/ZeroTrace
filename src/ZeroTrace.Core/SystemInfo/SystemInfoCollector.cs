// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.SystemInfo;

/// <summary>
/// Collects system hardware and software information.
/// Provides a snapshot for the Admin Dashboard.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class SystemInfoCollector
{
    private readonly IZeroTraceLogger _logger;

    public SystemInfoCollector(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Collect a complete system info snapshot.</summary>
    public SystemSnapshot CollectSnapshot()
    {
        _logger.Info("Sammle Systeminfos...");

        var proc = Process.GetCurrentProcess();

        return new SystemSnapshot
        {
            TimestampUtc     = DateTime.UtcNow,
            MachineName      = Environment.MachineName,
            UserName         = Environment.UserName,
            OsVersion        = Environment.OSVersion.VersionString,
            Is64Bit          = Environment.Is64BitOperatingSystem,
            ProcessorCount   = Environment.ProcessorCount,
            DotNetVersion    = Environment.Version.ToString(),
            SystemDirectory  = Environment.SystemDirectory,
            TotalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            UsedMemoryBytes  = proc.WorkingSet64,
            Uptime           = TimeSpan.FromMilliseconds(Environment.TickCount64),
            Drives           = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => new DriveSnapshot
                {
                    Name           = d.Name,
                    Label          = d.VolumeLabel,
                    TotalBytes     = d.TotalSize,
                    FreeBytes      = d.AvailableFreeSpace,
                    Format         = d.DriveFormat,
                })
                .ToList()
                .AsReadOnly()
        };
    }
}

public sealed class SystemSnapshot
{
    public required DateTime  TimestampUtc     { get; init; }
    public required string    MachineName      { get; init; }
    public required string    UserName         { get; init; }
    public required string    OsVersion        { get; init; }
    public required bool      Is64Bit          { get; init; }
    public required int       ProcessorCount   { get; init; }
    public required string    DotNetVersion    { get; init; }
    public required string    SystemDirectory  { get; init; }
    public required long      TotalMemoryBytes { get; init; }
    public required long      UsedMemoryBytes  { get; init; }
    public required TimeSpan  Uptime           { get; init; }
    public required IReadOnlyList<DriveSnapshot> Drives { get; init; }

    public double MemoryUsagePercent =>
        TotalMemoryBytes > 0 ? (double)UsedMemoryBytes / TotalMemoryBytes * 100 : 0;

    public string FormattedUptime =>
        $"{Uptime.Days}d {Uptime.Hours}h {Uptime.Minutes}m";
}

public sealed class DriveSnapshot
{
    public required string Name       { get; init; }
    public required string Label      { get; init; }
    public required long   TotalBytes { get; init; }
    public required long   FreeBytes  { get; init; }
    public required string Format     { get; init; }

    public long UsedBytes => TotalBytes - FreeBytes;
    public double UsagePercent =>
        TotalBytes > 0 ? (double)UsedBytes / TotalBytes * 100 : 0;

    public string FormattedFree => FormatSize(FreeBytes);
    public string FormattedTotal => FormatSize(TotalBytes);

    private static string FormatSize(long b) => b switch
    {
        < 1024L * 1024 * 1024 => $"{b / (1024.0 * 1024):F1} MB",
        _                     => $"{b / (1024.0 * 1024 * 1024):F1} GB"
    };
}
