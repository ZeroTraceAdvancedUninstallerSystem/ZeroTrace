// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Monitor;

/// <summary>
/// Records a single file system or process activity event.
/// Used by FileActivityMonitor and ProcessWatcher.
/// </summary>
public sealed class ActivityLogEntry
{
    public required DateTime    TimestampUtc { get; init; }
    public required string      FullPath     { get; init; }
    public required ActivityType Type        { get; init; }
    public          string?     OldPath      { get; init; }  // for renames
    public          long?       SizeBytes    { get; init; }
    public          string?     ProcessName  { get; init; }

    public string FileName => Path.GetFileName(FullPath) ?? FullPath;
    public string TypeDisplay => Type switch
    {
        ActivityType.Created  => "Erstellt",
        ActivityType.Modified => "Geaendert",
        ActivityType.Deleted  => "Geloescht",
        ActivityType.Renamed  => "Umbenannt",
        _                     => "Unbekannt"
    };

    public string FormattedTime =>
        TimestampUtc.ToLocalTime().ToString("HH:mm:ss.fff");
}

public enum ActivityType
{
    Created,
    Modified,
    Deleted,
    Renamed
}
