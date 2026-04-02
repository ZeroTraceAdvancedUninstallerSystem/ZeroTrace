// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Monitor;

/// <summary>
/// Monitors multiple directories simultaneously for file activity.
/// Useful for tracking what an installer or uninstaller does.
/// </summary>
public sealed class FileActivityMonitor : IDisposable
{
    private readonly IZeroTraceLogger _logger;
    private readonly List<ProcessWatcher> _watchers = [];
    private bool _isRunning;

    public event EventHandler<ActivityLogEntry>? ActivityDetected;

    public FileActivityMonitor(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Start monitoring standard Windows program directories.</summary>
    public void StartDefaultMonitoring()
    {
        var paths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        };

        foreach (var path in paths.Where(Directory.Exists).Distinct())
            AddWatch(path);

        _isRunning = true;
        _logger.Info($"[FileActivityMonitor] Ueberwache {_watchers.Count} Verzeichnisse");
    }

    /// <summary>Add a custom directory to monitor.</summary>
    public void AddWatch(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) return;

        try
        {
            var watcher = new ProcessWatcher(_logger, directoryPath);
            watcher.ActivityDetected += (_, entry) => ActivityDetected?.Invoke(this, entry);
            watcher.Start();
            _watchers.Add(watcher);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[FileActivityMonitor] Kann {directoryPath} nicht ueberwachen: {ex.Message}");
        }
    }

    /// <summary>Stop all monitoring and return combined activity log.</summary>
    public IReadOnlyList<ActivityLogEntry> StopAndGetResults()
    {
        var allEntries = new List<ActivityLogEntry>();

        foreach (var watcher in _watchers)
        {
            watcher.Stop();
            allEntries.AddRange(watcher.GetLog());
        }

        _isRunning = false;
        _logger.Info($"[FileActivityMonitor] Gestoppt. {allEntries.Count} Aktivitaeten gesamt.");

        return allEntries
            .OrderBy(e => e.TimestampUtc)
            .ToList()
            .AsReadOnly();
    }

    public bool IsRunning => _isRunning;

    public void Dispose()
    {
        foreach (var w in _watchers) w.Dispose();
        _watchers.Clear();
    }
}
