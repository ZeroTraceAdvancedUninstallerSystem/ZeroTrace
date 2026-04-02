// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

namespace ZeroTrace.Core.Models;

/// <summary>A residual artefact left by an uninstalled program.</summary>
public sealed class ResidualItem
{
    public required string                  Id               { get; init; }
    public required string                  FullPath         { get; init; }
    public required ResidualItemType        ItemType         { get; init; }
    public required double                  ConfidenceScore  { get; init; }
    public required string                  DetectionReason  { get; init; }
    public required ResidualDetectionSource DetectionSource  { get; init; }
    public long?                            SizeInBytes      { get; init; }
    public bool                             IsSelectedForDeletion { get; set; }

    public string DisplayName => Path.GetFileName(FullPath.TrimEnd('\\')) ?? FullPath;

    public string CategoryDisplayName => ItemType switch
    {
        ResidualItemType.File          => "Dateien",
        ResidualItemType.Directory     => "Ordner",
        ResidualItemType.RegistryKey   => "Registry-Schluessel",
        ResidualItemType.RegistryValue => "Registry-Werte",
        _                              => "Sonstiges"
    };

    public string FormattedSize => SizeInBytes switch
    {
        null       => "-",
        0          => "Leer",
        < 1024     => $"{SizeInBytes} B",
        < 1048576  => $"{SizeInBytes / 1024.0:F1} KB",
        _          => $"{SizeInBytes / 1048576.0:F1} MB"
    };
}

public enum ResidualItemType { File, Directory, RegistryKey, RegistryValue }
public enum ResidualDetectionSource
{
    InstallPath, NameMatch, AppDataScan, ProgramDataScan,
    RegistryScan, TempScan, Heuristic
}
