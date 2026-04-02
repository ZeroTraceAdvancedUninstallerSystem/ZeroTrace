// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Monitor;

/// <summary>
/// Watches a directory for file system changes in real-time.
/// Captures create, modify, delete, and rename events.
/// </summary>
public sealed class ProcessWatcher : IDisposable
{
    private readonly IZeroTraceLogger _logger;
    private readonly FileSystemWatcher _watcher;
    private readonly List<ActivityLogEntry> _log = [];
    private readonly object _lock = new();
    private bool _isRunning;

    public event EventHandler<ActivityLogEntry>? ActivityDetected;

    public ProcessWatcher(IZeroTraceLogger logger, string watchPath)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!Directory.Exists(watchPath))
            throw new DirectoryNotFoundException($"Watch-Pfad nicht gefunden: {watchPath}");

        _watcher = new FileSystemWatcher(watchPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                         | NotifyFilters.DirectoryName
                         | NotifyFilters.LastWrite
                         | NotifyFilters.Size,
            EnableRaisingEvents = false
        };

        _watcher.Created += (_, e) => RecordActivity(e.FullPath, ActivityType.Created);
        _watcher.Changed += (_, e) => RecordActivity(e.FullPath, ActivityType.Modified);
        _watcher.Deleted += (_, e) => RecordActivity(e.FullPath, ActivityType.Deleted);
        _watcher.Renamed += (_, e) => RecordActivity(e.FullPath, ActivityType.Renamed, e.OldFullPath);
        _watcher.Error   += (_, e) => _logger.Warning($"[Monitor] Fehler: {e.GetException().Message}");
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
        _isRunning = true;
        _logger.Info($"[Monitor] Gestartet: {_watcher.Path}");
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
        _isRunning = false;
        _logger.Info($"[Monitor] Gestoppt. {_log.Count} Ereignisse aufgezeichnet.");
    }

    public bool IsRunning => _isRunning;

    /// <summary>Returns all recorded activities since start.</summary>
    public IReadOnlyList<ActivityLogEntry> GetLog()
    {
        lock (_lock) { return _log.ToList().AsReadOnly(); }
    }

    /// <summary>Clears the activity log.</summary>
    public void ClearLog()
    {
        lock (_lock) { _log.Clear(); }
    }

    private void RecordActivity(string path, ActivityType type, string? oldPath = null)
    {
        var entry = new ActivityLogEntry
        {
            TimestampUtc = DateTime.UtcNow,
            FullPath = path,
            Type = type,
            OldPath = oldPath
        };

        lock (_lock) { _log.Add(entry); }
        ActivityDetected?.Invoke(this, entry);
        _logger.Debug($"[Monitor] {type}: {path}");
    }

    public void Dispose()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
}
