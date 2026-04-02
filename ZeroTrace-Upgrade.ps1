# ╔══════════════════════════════════════════════════════════════════════╗
# ║  ZeroTrace – Advanced Uninstaller System                           ║
# ║  Qualitäts-Upgrade & Admin-Modul – PowerShell Deploy-Script        ║
# ║  Eigentümer: Mario B. | Lizenz: MIT Open Source                    ║
# ║  Generiert von Claude (Anthropic) – 02.04.2026                     ║
# ╚══════════════════════════════════════════════════════════════════════╝
#
# ANLEITUNG:
#   1. PowerShell als Administrator öffnen
#   2. cd C:\Projekte\ZeroTrace
#   3. powershell -ExecutionPolicy Bypass -File ZeroTrace-Upgrade.ps1
#
# Was dieses Script macht:
#   ✅ Phase 1 Module [01-14] komplett neu mit Profi-Qualität
#   ✅ Admin-Bereich (AdminService.cs)
#   ✅ MIT-Lizenz mit Copyright Mario B.
#   ✅ README.md für GitHub/Open Source
#   ✅ .gitignore
#   ✅ Aktualisierte .csproj Dateien

param(
    [string]$ProjectRoot = "C:\Projekte\ZeroTrace"
)

$ErrorActionPreference = "Stop"
$srcCore = Join-Path $ProjectRoot "src\ZeroTrace.Core"
$srcUI   = Join-Path $ProjectRoot "src\ZeroTrace"

# ══════════════════════════════════════════════════════════════════════
# Hilfsfunktion: Datei sicher schreiben
# ══════════════════════════════════════════════════════════════════════
function Write-SourceFile {
    param([string]$RelativePath, [string]$Content, [string]$Base = $srcCore)
    $fullPath = Join-Path $Base $RelativePath
    $dir = Split-Path $fullPath -Parent
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    # BOM-freies UTF-8
    [System.IO.File]::WriteAllText($fullPath, $Content, [System.Text.UTF8Encoding]::new($false))
    Write-Host "  ✅ $RelativePath" -ForegroundColor Green
}

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  ZeroTrace – Advanced Uninstaller System                    ║" -ForegroundColor Cyan
Write-Host "║  Qualitäts-Upgrade wird installiert...                      ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ══════════════════════════════════════════════════════════════════════
# [00] LICENSE (MIT) + README.md + .gitignore
# ══════════════════════════════════════════════════════════════════════
Write-Host "[00] Projektdateien (Lizenz, README, .gitignore)..." -ForegroundColor Yellow

$license = @'
MIT License

Copyright (c) 2026 Mario B.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
'@
[System.IO.File]::WriteAllText((Join-Path $ProjectRoot "LICENSE"), $license, [System.Text.UTF8Encoding]::new($false))
Write-Host "  ✅ LICENSE (MIT)" -ForegroundColor Green

$readme = @'
# ZeroTrace – Advanced Uninstaller System

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6.svg)](https://www.microsoft.com/windows)

**ZeroTrace** is a professional-grade Windows application for complete and thorough
software removal. It goes beyond standard uninstallers by detecting and cleaning
residual files, folders, and registry entries that programs leave behind.

## Features

- **Deep Scan Engine** – Finds leftover files, folders, and registry keys after uninstallation
- **Safety Vault** – Automatically backs up everything before deletion for safe restoration
- **Smart Analysis** – Confidence-based scoring to prevent false positives
- **Admin Dashboard** – System health monitoring and administrative controls
- **Registry Cleaning** – Safely removes orphaned registry entries
- **Crypto-Verified Backups** – SHA-256/BLAKE3 integrity verification
- **Reboot-Scheduled Deletion** – Handles locked files via Windows API

## Architecture

```
ZeroTrace/
├── src/
│   ├── ZeroTrace/              # WPF UI Application
│   ├── ZeroTrace.Core/         # Core Business Logic
│   │   ├── Admin/              # Admin Dashboard Service
│   │   ├── Analysis/           # Residual Analysis Engine
│   │   ├── Cleanup/            # File & Registry Cleanup
│   │   ├── Crypto/             # Encryption & Hashing
│   │   ├── Discovery/          # Program Detection
│   │   ├── Logging/            # Async File Logger
│   │   ├── Models/             # Data Models
│   │   ├── Restore/            # Backup Restore Service
│   │   ├── Uninstall/          # Uninstall Process Manager
│   │   └── Vault/              # Backup Vault Service
│   └── ZeroTrace.Native/       # Native Windows Interop
├── LICENSE
└── README.md
```

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime
- Administrator privileges (for registry and system file access)

## Build

```bash
dotnet build src/ZeroTrace/ZeroTrace.csproj -c Release
```

## Author

**Mario B.** – Creator & Owner

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

---

> *ZeroTrace – Leave no trace behind.*
'@
[System.IO.File]::WriteAllText((Join-Path $ProjectRoot "README.md"), $readme, [System.Text.UTF8Encoding]::new($false))
Write-Host "  ✅ README.md" -ForegroundColor Green

$gitignore = @'
## .NET / Visual Studio
bin/
obj/
*.user
*.suo
*.cache
*.vs/
.vs/
*.DotSettings.user
packages/
*.nupkg
project.lock.json

## OS
Thumbs.db
Desktop.ini
.DS_Store

## IDE
.idea/
*.swp
*~

## Logs
*.log
Logs/

## Build output
publish/
'@
[System.IO.File]::WriteAllText((Join-Path $ProjectRoot ".gitignore"), $gitignore, [System.Text.UTF8Encoding]::new($false))
Write-Host "  ✅ .gitignore" -ForegroundColor Green

# ══════════════════════════════════════════════════════════════════════
# [01] Models/ProgramSource.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "[01] Models/ProgramSource.cs..." -ForegroundColor Yellow

Write-SourceFile "Models\ProgramSource.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

namespace ZeroTrace.Core.Models;

/// <summary>
/// Describes where an installed program was discovered.
/// Used for prioritising uninstall strategies and deduplication.
/// </summary>
public enum ProgramSource
{
    Unknown              = 0,
    RegistryLocalMachine = 1,
    RegistryCurrentUser  = 2,
    Msi                  = 3,
    Store                = 4,
    Portable             = 5,
    Custom               = 6
}
'@

# ══════════════════════════════════════════════════════════════════════
# [01] Models/InstalledProgram.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[01] Models/InstalledProgram.cs..." -ForegroundColor Yellow

Write-SourceFile "Models\InstalledProgram.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

namespace ZeroTrace.Core.Models;

/// <summary>
/// Represents a single installed program discovered on the system.
/// Immutable where possible; mutable setters only where the scan
/// pipeline needs to enrich data after initial construction.
/// </summary>
public sealed class InstalledProgram
{
    // ── Identity ─────────────────────────────────────────────────
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    // ── Display ──────────────────────────────────────────────────
    public required string DisplayName  { get; init; }
    public string?         DisplayVersion { get; init; }
    public string?         Publisher      { get; init; }
    public string?         IconPath       { get; init; }

    // ── Paths ────────────────────────────────────────────────────
    public string? InstallLocation      { get; init; }
    public string? UninstallString      { get; init; }
    public string? QuietUninstallString { get; init; }
    public string? RegistryKeyPath      { get; init; }
    public string? InstallSource        { get; init; }

    // ── Metadata ─────────────────────────────────────────────────
    public string         Architecture     { get; init; } = "Unknown";
    public DateTimeOffset? InstallDate     { get; init; }
    public bool           IsSystemComponent { get; init; }
    public long?          EstimatedSizeBytes { get; init; }
    public ProgramSource  Source           { get; init; } = ProgramSource.Unknown;
    public string?        ProductCode      { get; init; }
    public string?        MsiProductCode   { get; init; }

    // ── Computed ─────────────────────────────────────────────────
    public bool IsMsiInstallation => !string.IsNullOrEmpty(MsiProductCode);

    public bool CanBeUninstalled =>
        !string.IsNullOrEmpty(UninstallString) ||
        !string.IsNullOrEmpty(QuietUninstallString) ||
        IsMsiInstallation;

    public string FormattedSize => EstimatedSizeBytes switch
    {
        null              => "–",
        < 1024            => $"{EstimatedSizeBytes} B",
        < 1024 * 1024     => $"{EstimatedSizeBytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{EstimatedSizeBytes / (1024.0 * 1024):F1} MB",
        _                 => $"{EstimatedSizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };

    public string FormattedInstallDate =>
        InstallDate?.LocalDateTime.ToString("dd.MM.yyyy") ?? "–";

    public override string ToString() =>
        $"{DisplayName} {DisplayVersion}".Trim();
}
'@

# ══════════════════════════════════════════════════════════════════════
# [02] Models/ResidualItem.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[02] Models/ResidualItem.cs..." -ForegroundColor Yellow

Write-SourceFile "Models\ResidualItem.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

namespace ZeroTrace.Core.Models;

/// <summary>
/// A single residual artefact (file, directory, or registry entry)
/// left behind by an uninstalled program.
/// </summary>
public sealed class ResidualItem
{
    public required string                  Id               { get; init; }
    public required string                  FullPath         { get; init; }
    public required ResidualItemType        ItemType         { get; init; }
    public required double                  ConfidenceScore  { get; init; }
    public required string                  DetectionReason  { get; init; }
    public required ResidualDetectionSource DetectionSource  { get; init; }

    public long? SizeInBytes          { get; init; }
    public bool  IsSelectedForDeletion { get; set; }

    // ── Computed ─────────────────────────────────────────────────
    public string DisplayName => Path.GetFileName(FullPath.TrimEnd('\\')) ?? FullPath;

    public string CategoryDisplayName => ItemType switch
    {
        ResidualItemType.File          => "Dateien",
        ResidualItemType.Directory     => "Ordner",
        ResidualItemType.RegistryKey   => "Registry-Schlüssel",
        ResidualItemType.RegistryValue => "Registry-Werte",
        _                              => "Sonstiges"
    };

    public string FormattedSize => SizeInBytes switch
    {
        null       => "–",
        0          => "Leer",
        < 1024     => $"{SizeInBytes} B",
        < 1048576  => $"{SizeInBytes / 1024.0:F1} KB",
        _          => $"{SizeInBytes / 1048576.0:F1} MB"
    };
}

public enum ResidualItemType
{
    File,
    Directory,
    RegistryKey,
    RegistryValue
}

public enum ResidualDetectionSource
{
    InstallPath,
    NameMatch,
    AppDataScan,
    ProgramDataScan,
    RegistryScan,
    TempScan,
    Heuristic
}
'@

# ══════════════════════════════════════════════════════════════════════
# [03] Models/VaultBackup.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[03] Models/VaultBackup.cs..." -ForegroundColor Yellow

Write-SourceFile "Models\VaultBackup.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

namespace ZeroTrace.Core.Models;

/// <summary>
/// Metadata for a single vault backup created before cleanup.
/// </summary>
public sealed class VaultBackup
{
    public required string                       BackupId      { get; init; }
    public required DateTime                     CreatedAtUtc  { get; init; }
    public required string                       ProgramName   { get; init; }
    public          string?                      ProgramVersion { get; init; }
    public required IReadOnlyList<VaultEntry>     Entries       { get; init; }
    public          long                         TotalSizeBytes { get; init; }
    public required string                       IntegrityHash  { get; init; }
    public          bool                         IsEncrypted    { get; init; }
    public          VaultBackupStatus            Status        { get; set; } = VaultBackupStatus.Active;

    // ── Computed ─────────────────────────────────────────────────
    public int SuccessCount => Entries.Count(e => e.BackupSuccessful);
    public int FailedCount  => Entries.Count(e => !e.BackupSuccessful);

    public string FormattedSize => TotalSizeBytes switch
    {
        < 1024              => $"{TotalSizeBytes} B",
        < 1024 * 1024       => $"{TotalSizeBytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{TotalSizeBytes / (1024.0 * 1024):F1} MB",
        _                   => $"{TotalSizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };

    public string FormattedDate =>
        CreatedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
}

/// <summary>
/// A single file/directory/registry entry within a vault backup.
/// </summary>
public sealed class VaultEntry
{
    public required string         EntryId             { get; init; }
    public required string         OriginalPath        { get; init; }
    public required string         BackupRelativePath  { get; init; }
    public required VaultEntryType EntryType           { get; init; }
    public          long?          SizeInBytes         { get; init; }
    public          string?        ContentHash         { get; init; }
    public          bool           BackupSuccessful    { get; set; }
    public          bool           HasBeenRestored     { get; set; }
    public          string?        ErrorMessage        { get; set; }
}

public enum VaultEntryType  { File, Directory, RegistryKey, RegistryValue }
public enum VaultBackupStatus { Active, PartiallyRestored, FullyRestored, Expired, Corrupted }
'@

# ══════════════════════════════════════════════════════════════════════
# [04] Logging/IZeroTraceLogger.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[04] Logging/IZeroTraceLogger.cs..." -ForegroundColor Yellow

Write-SourceFile "Logging\IZeroTraceLogger.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

namespace ZeroTrace.Core.Logging;

/// <summary>
/// Central logging abstraction used by every ZeroTrace service.
/// Synchronous signatures keep call sites clean; the implementation
/// (<see cref="FileLogger"/>) handles async I/O internally via a
/// background flush queue.
/// </summary>
public interface IZeroTraceLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

/// <summary>
/// Minimum severity for log messages.
/// Messages below this level are discarded.
/// </summary>
public enum LogLevel
{
    Debug   = 0,
    Info    = 1,
    Warning = 2,
    Error   = 3
}
'@

# ══════════════════════════════════════════════════════════════════════
# [05] Logging/FileLogger.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[05] Logging/FileLogger.cs..." -ForegroundColor Yellow

Write-SourceFile "Logging\FileLogger.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Collections.Concurrent;
using System.Text;

namespace ZeroTrace.Core.Logging;

/// <summary>
/// High-performance async file logger.
/// Messages are enqueued lock-free and flushed to disk every 500 ms
/// by a background task.  Implements <see cref="IAsyncDisposable"/>
/// so the WPF host can flush remaining messages on shutdown.
/// </summary>
public sealed class FileLogger : IZeroTraceLogger, IAsyncDisposable
{
    private readonly string                     _logFilePath;
    private readonly LogLevel                   _minimumLevel;
    private readonly ConcurrentQueue<string>    _queue = new();
    private readonly SemaphoreSlim              _writeLock = new(1, 1);
    private readonly CancellationTokenSource    _cts = new();
    private readonly Task                       _flushTask;

    public FileLogger(string? logDirectory = null, LogLevel minimumLevel = LogLevel.Info)
    {
        _minimumLevel = minimumLevel;

        var directory = logDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZeroTrace", "Logs");

        Directory.CreateDirectory(directory);

        _logFilePath = Path.Combine(directory,
            $"ZeroTrace_{DateTime.Now:yyyy-MM-dd}.log");

        _flushTask = Task.Run(FlushLoopAsync);
    }

    // ── IZeroTraceLogger ─────────────────────────────────────────
    public void Debug(string message)                        => Enqueue(LogLevel.Debug, message);
    public void Info(string message)                         => Enqueue(LogLevel.Info, message);
    public void Warning(string message)                      => Enqueue(LogLevel.Warning, message);
    public void Error(string message, Exception? ex = null)  =>
        Enqueue(LogLevel.Error, ex is null
            ? message
            : $"{message} | {ex.GetType().Name}: {ex.Message}\n  StackTrace: {ex.StackTrace}");

    // ── Internals ────────────────────────────────────────────────
    private void Enqueue(LogLevel level, string message)
    {
        if (level < _minimumLevel) return;

        var line = string.Create(null, stackalloc char[256],
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
            $"[{level.ToString().ToUpperInvariant(),-7}] " +
            $"[T{Environment.CurrentManagedThreadId:D3}] {message}");

        _queue.Enqueue(line);
    }

    private async Task FlushLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try  { await Task.Delay(500, _cts.Token); await FlushAsync(); }
            catch (OperationCanceledException) { break; }
            catch { /* swallow – logging must not crash the app */ }
        }
        await FlushAsync();          // drain remaining messages
    }

    private async Task FlushAsync()
    {
        if (_queue.IsEmpty) return;
        await _writeLock.WaitAsync();
        try
        {
            var sb = new StringBuilder(4096);
            while (_queue.TryDequeue(out var msg)) sb.AppendLine(msg);
            if (sb.Length > 0)
                await File.AppendAllTextAsync(_logFilePath, sb.ToString());
        }
        finally { _writeLock.Release(); }
    }

    public string GetCurrentLogFilePath() => _logFilePath;

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        try { await _flushTask; } catch (OperationCanceledException) { }
        _cts.Dispose();
        _writeLock.Dispose();
    }
}
'@

# ══════════════════════════════════════════════════════════════════════
# [06] Discovery/IDiscoveryProvider.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[06] Discovery/IDiscoveryProvider.cs..." -ForegroundColor Yellow

Write-SourceFile "Discovery\IDiscoveryProvider.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Discovery;

/// <summary>
/// Contract for every module that can detect installed programs.
///
/// Implementations:
///   • <see cref="RegistryDiscoveryProvider"/> – Windows Registry (HKLM/HKCU)
///   • MsiDiscoveryProvider   (planned)       – MSI database
///   • AppxDiscoveryProvider  (planned)       – Windows Store apps
/// </summary>
public interface IDiscoveryProvider
{
    /// <summary>Human-readable name for logging, e.g. "Windows Registry".</summary>
    string ProviderName { get; }

    /// <summary>Discovers installed programs from this provider's source.</summary>
    Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}
'@

# ══════════════════════════════════════════════════════════════════════
# [07] Discovery/RegistryDiscoveryProvider.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[07] Discovery/RegistryDiscoveryProvider.cs..." -ForegroundColor Yellow

Write-SourceFile "Discovery\RegistryDiscoveryProvider.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

/// <summary>
/// Reads the three standard uninstall registry hives
/// (HKLM 64-bit, HKLM 32-bit, HKCU) and converts each entry
/// into an <see cref="InstalledProgram"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RegistryDiscoveryProvider : IDiscoveryProvider
{
    public string ProviderName => "Windows Registry (HKLM/HKCU)";

    private readonly IZeroTraceLogger _logger;

    private static readonly RegistrySource[] Sources =
    [
        new("HKLM 64-Bit", RegistryHive.LocalMachine,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            RegistryView.Registry64, ProgramSource.RegistryLocalMachine),
        new("HKLM 32-Bit", RegistryHive.LocalMachine,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            RegistryView.Registry32, ProgramSource.RegistryLocalMachine),
        new("HKCU", RegistryHive.CurrentUser,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            RegistryView.Registry64, ProgramSource.RegistryCurrentUser),
    ];

    public RegistryDiscoveryProvider(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(
        CancellationToken ct = default)
    {
        _logger.Info("Registry-Scan gestartet…");
        return Task.Run(() =>
        {
            var programs = new List<InstalledProgram>();
            var seen     = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var src in Sources)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var found = ReadSource(src);
                    foreach (var p in found)
                        if (seen.Add(p.Id)) programs.Add(p);

                    _logger.Debug($"  {src.Description}: {found.Count} Programm(e)");
                }
                catch (System.Security.SecurityException ex)
                { _logger.Warning($"  {src.Description}: Zugriff verweigert – {ex.Message}"); }
                catch (Exception ex)
                { _logger.Warning($"  {src.Description}: Fehler – {ex.Message}"); }
            }

            _logger.Info($"Registry-Scan abgeschlossen: {programs.Count} Programme.");
            return (IReadOnlyList<InstalledProgram>)programs.AsReadOnly();
        }, ct);
    }

    // ── Private ──────────────────────────────────────────────────

    private List<InstalledProgram> ReadSource(RegistrySource src)
    {
        var results = new List<InstalledProgram>();
        using var baseKey     = RegistryKey.OpenBaseKey(src.Hive, src.View);
        using var uninstallKey = baseKey.OpenSubKey(src.Path);
        if (uninstallKey is null) return results;

        foreach (var subKeyName in uninstallKey.GetSubKeyNames())
        {
            try
            {
                using var key = uninstallKey.OpenSubKey(subKeyName);
                if (key is null) continue;

                var name = key.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(name)) continue;

                results.Add(new InstalledProgram
                {
                    Id                   = BuildId(src, subKeyName),
                    DisplayName          = name.Trim(),
                    DisplayVersion       = Str(key, "DisplayVersion"),
                    Publisher            = Str(key, "Publisher"),
                    InstallLocation      = NormPath(Str(key, "InstallLocation")),
                    InstallDate          = ParseDate(Str(key, "InstallDate")),
                    EstimatedSizeBytes   = ParseSize(key),
                    UninstallString      = Str(key, "UninstallString"),
                    QuietUninstallString = Str(key, "QuietUninstallString"),
                    Source               = src.ProgramSource,
                    IsSystemComponent    = IsSystem(key),
                    RegistryKeyPath      = key.Name,
                    MsiProductCode       = Guid.TryParse(subKeyName, out _) ? subKeyName : null,
                    IconPath             = Str(key, "DisplayIcon"),
                });
            }
            catch (Exception ex)
            { _logger.Debug($"  Übersprungen: {subKeyName} – {ex.Message}"); }
        }
        return results;
    }

    private static string BuildId(RegistrySource s, string sub)
    {
        var hive = s.Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU";
        var view = s.View == RegistryView.Registry64   ? "64"   : "32";
        return $"{hive}_{view}_{sub}";
    }

    private static bool IsSystem(RegistryKey key)
    {
        if (key.GetValue("SystemComponent") is int sc && sc == 1) return true;
        if (key.GetValue("ReleaseType") is string rt
            && rt is "Update" or "Hotfix" or "Security Update") return true;
        if (key.GetValue("ParentKeyName") is string p
            && !string.IsNullOrEmpty(p)) return true;
        return false;
    }

    private static string? Str(RegistryKey k, string v)
    { try { return k.GetValue(v)?.ToString()?.Trim(); } catch { return null; } }

    private static DateTimeOffset? ParseDate(string? s) =>
        !string.IsNullOrWhiteSpace(s)
        && DateTime.TryParseExact(s.Trim(), "yyyyMMdd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? new DateTimeOffset(d)
            : null;

    private static long? ParseSize(RegistryKey key) =>
        key.GetValue("EstimatedSize") switch
        {
            int  i => (long)i * 1024,
            long l => l * 1024,
            _      => null
        };

    private static string? NormPath(string? p) =>
        string.IsNullOrWhiteSpace(p) ? null : p.Trim().Trim('"').TrimEnd('\\');

    private sealed record RegistrySource(
        string Description, RegistryHive Hive, string Path,
        RegistryView View, ProgramSource ProgramSource);
}
'@

# ══════════════════════════════════════════════════════════════════════
# [08] Discovery/DiscoveryService.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[08] Discovery/DiscoveryService.cs..." -ForegroundColor Yellow

Write-SourceFile "Discovery\DiscoveryService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

/// <summary>
/// Orchestrates all registered <see cref="IDiscoveryProvider"/>s,
/// merges and deduplicates results.
/// </summary>
public sealed class DiscoveryService
{
    private readonly IReadOnlyList<IDiscoveryProvider> _providers;
    private readonly IZeroTraceLogger _logger;

    public event EventHandler<DiscoveryProgressEventArgs>? ProgressChanged;

    public DiscoveryService(
        IEnumerable<IDiscoveryProvider> providers,
        IZeroTraceLogger logger)
    {
        _providers = (providers ?? throw new ArgumentNullException(nameof(providers)))
            .ToList().AsReadOnly();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<InstalledProgram>> GetAllProgramsAsync(
        bool includeSystemComponents = false,
        CancellationToken ct = default)
    {
        _logger.Info("Programmsuche gestartet");
        var all = new List<InstalledProgram>();

        for (int i = 0; i < _providers.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var provider = _providers[i];

            ProgressChanged?.Invoke(this, new DiscoveryProgressEventArgs
            {
                CurrentProvider = provider.ProviderName,
                ProviderIndex   = i,
                TotalProviders  = _providers.Count
            });

            try
            {
                var found = await provider.DiscoverAsync(ct);
                all.AddRange(found);
                _logger.Info($"  {provider.ProviderName}: {found.Count} Programme");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            { _logger.Error($"  {provider.ProviderName} fehlgeschlagen", ex); }
        }

        var deduped  = Deduplicate(all);
        var filtered = includeSystemComponents
            ? deduped
            : deduped.Where(p => !p.IsSystemComponent).ToList();

        var sorted = filtered
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList().AsReadOnly();

        _logger.Info($"Programmsuche abgeschlossen: {sorted.Count} Programme");
        return sorted;
    }

    private List<InstalledProgram> Deduplicate(List<InstalledProgram> list)
    {
        var seen   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<InstalledProgram>(list.Count);

        foreach (var p in list)
        {
            var key = $"{p.DisplayName}|{p.DisplayVersion ?? "?"}";
            if (seen.Add(key)) unique.Add(p);
        }

        int removed = list.Count - unique.Count;
        if (removed > 0) _logger.Debug($"  {removed} Duplikat(e) entfernt");
        return unique;
    }
}

public sealed class DiscoveryProgressEventArgs : EventArgs
{
    public required string CurrentProvider { get; init; }
    public required int    ProviderIndex   { get; init; }
    public required int    TotalProviders  { get; init; }
}
'@

# ══════════════════════════════════════════════════════════════════════
# [09] Uninstall/UninstallService.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[09] Uninstall/UninstallService.cs..." -ForegroundColor Yellow

Write-SourceFile "Uninstall\UninstallService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Uninstall;

// ── Result & Enums ───────────────────────────────────────────────

public sealed class UninstallResult
{
    public required bool             Success        { get; init; }
    public required int              ExitCode       { get; init; }
    public required TimeSpan         Duration       { get; init; }
    public required UninstallMethod  MethodUsed     { get; init; }
    public          string?          ErrorMessage   { get; init; }
    public          bool             RequiresReboot { get; init; }
}

public enum UninstallMethod
{
    MsiStandard,
    MsiQuiet,
    ExecutableNormal,
    ExecutableQuiet,
    NotAvailable
}

// ── Service ──────────────────────────────────────────────────────

[SupportedOSPlatform("windows")]
public sealed class UninstallService
{
    private readonly IZeroTraceLogger _logger;
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    public event EventHandler<UninstallStatusEventArgs>? StatusChanged;

    public UninstallService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UninstallResult> UninstallAsync(
        InstalledProgram program,
        bool preferQuiet = false,
        CancellationToken ct = default)
    {
        _logger.Info($"╔═══ Deinstallation: '{program.DisplayName}' ═══╗");
        Report($"Vorbereitung: '{program.DisplayName}'…");
        var sw = Stopwatch.StartNew();

        try
        {
            var (cmd, method) = ChooseStrategy(program, preferQuiet);

            if (cmd is null)
            {
                _logger.Warning($"Kein Deinstaller für '{program.DisplayName}'");
                return Fail(sw, UninstallMethod.NotAvailable,
                    "Kein Deinstallationsbefehl verfügbar.");
            }

            _logger.Info($"  Methode: {method}");
            _logger.Debug($"  Befehl:  {cmd}");

            return await RunAsync(cmd, method, program.DisplayName, sw, ct);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning($"Abgebrochen: '{program.DisplayName}'");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error($"Fehler: '{program.DisplayName}'", ex);
            return Fail(sw, UninstallMethod.NotAvailable, ex.Message);
        }
    }

    // ── Strategy ─────────────────────────────────────────────────

    private static (string? Cmd, UninstallMethod Method) ChooseStrategy(
        InstalledProgram p, bool quiet)
    {
        if (!string.IsNullOrEmpty(p.MsiProductCode))
            return quiet
                ? ($"msiexec.exe /x {p.MsiProductCode} /qn /norestart", UninstallMethod.MsiQuiet)
                : ($"msiexec.exe /x {p.MsiProductCode} /norestart",     UninstallMethod.MsiStandard);

        if (quiet && !string.IsNullOrEmpty(p.QuietUninstallString))
            return (p.QuietUninstallString, UninstallMethod.ExecutableQuiet);

        if (!string.IsNullOrEmpty(p.UninstallString))
            return (p.UninstallString, UninstallMethod.ExecutableNormal);

        return (null, UninstallMethod.NotAvailable);
    }

    // ── Execution ────────────────────────────────────────────────

    private async Task<UninstallResult> RunAsync(
        string commandLine, UninstallMethod method, string name,
        Stopwatch sw, CancellationToken ct)
    {
        var (file, args) = SplitCommand(commandLine);
        Report($"Starte Deinstaller: '{name}'…");

        var psi = new ProcessStartInfo
        {
            FileName  = file,
            Arguments = args,
            UseShellExecute = true,
            Verb = "runas"
        };

        using var proc = new Process { StartInfo = psi };

        try
        {
            if (!proc.Start())
                return Fail(sw, method, "Prozess konnte nicht gestartet werden.");

            _logger.Info($"  PID: {proc.Id}");
            Report($"Warte auf Deinstaller: '{name}'…");

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(Timeout);

            try { await proc.WaitForExitAsync(linked.Token); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.Warning($"  TIMEOUT nach {Timeout.TotalMinutes:F0} min");
                Kill(proc);
                return Fail(sw, method, "Zeitüberschreitung – Deinstaller beendet.");
            }

            sw.Stop();
            int  code    = proc.ExitCode;
            bool ok      = code is 0 or 1605 or 1614 or 1641 or 3010;
            bool reboot  = code is 1641 or 3010;

            _logger.Info($"  Exit={code} Erfolg={ok} Reboot={reboot} Dauer={sw.Elapsed.TotalSeconds:F1}s");
            Report(ok ? $"'{name}' erfolgreich deinstalliert!" : $"Exit-Code {code}");

            return new UninstallResult
            {
                Success = ok, ExitCode = code, Duration = sw.Elapsed,
                MethodUsed = method, RequiresReboot = reboot,
                ErrorMessage = ok ? null : $"Exit-Code {code}"
            };
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            _logger.Warning($"  Start fehlgeschlagen: {ex.Message}");
            return Fail(sw, method,
                ex.NativeErrorCode == 1223
                    ? "Benutzer hat die UAC-Anfrage abgelehnt."
                    : ex.Message);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static (string File, string Args) SplitCommand(string cmd)
    {
        cmd = cmd.Trim();

        if (cmd.StartsWith('"'))
        {
            int close = cmd.IndexOf('"', 1);
            if (close > 0)
                return (cmd[1..close], cmd[(close + 1)..].Trim());
        }

        if (cmd.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            int sp = cmd.IndexOf(' ');
            return sp > 0 ? ("msiexec.exe", cmd[(sp + 1)..].Trim()) : ("msiexec.exe", "");
        }

        int exe = cmd.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exe > 0)
        {
            int at = exe + 4;
            return (cmd[..at].Trim(), at < cmd.Length ? cmd[at..].Trim() : "");
        }

        int first = cmd.IndexOf(' ');
        return first > 0 ? (cmd[..first], cmd[(first + 1)..]) : (cmd, "");
    }

    private void Kill(Process p)
    { try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { } }

    private static UninstallResult Fail(Stopwatch sw, UninstallMethod m, string msg) =>
        new() { Success = false, ExitCode = -1, Duration = sw.Elapsed,
                MethodUsed = m, ErrorMessage = msg };

    private void Report(string msg) =>
        StatusChanged?.Invoke(this, new UninstallStatusEventArgs(msg));
}

public sealed class UninstallStatusEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}
'@

# ══════════════════════════════════════════════════════════════════════
# [10] Analysis/ResidualAnalysisEngine.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[10] Analysis/ResidualAnalysisEngine.cs..." -ForegroundColor Yellow

Write-SourceFile "Analysis\ResidualAnalysisEngine.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Analysis;

// ── Progress event ───────────────────────────────────────────────

public sealed class ScanProgressEventArgs : EventArgs
{
    public required string CurrentAreaName  { get; init; }
    public required string CurrentPath      { get; init; }
    public required int    TotalAreas       { get; init; }
    public required int    CompletedAreas   { get; init; }
    public required int    ItemsFoundSoFar  { get; init; }
    public int PercentComplete =>
        TotalAreas == 0 ? 0 : CompletedAreas * 100 / TotalAreas;
}

// ── Engine ───────────────────────────────────────────────────────

[SupportedOSPlatform("windows")]
public sealed partial class ResidualAnalysisEngine
{
    private readonly IZeroTraceLogger _logger;
    public event EventHandler<ScanProgressEventArgs>? ProgressChanged;

    // System-critical paths that must never be flagged
    private static readonly HashSet<string> ProtectedPaths =
        new(StringComparer.OrdinalIgnoreCase)
    {
        @"C:\Windows", @"C:\Windows\System32", @"C:\Windows\SysWOW64",
        @"C:\Windows\WinSxS", @"C:\Windows\Fonts", @"C:\Windows\Installer",
        @"C:\Program Files\Windows Defender", @"C:\Program Files\Windows NT",
        @"C:\Program Files\WindowsApps",
        @"C:\ProgramData\Microsoft", @"C:\ProgramData\Windows",
    };

    private static readonly string[] ProtectedRegistryPrefixes =
    [
        @"HKEY_LOCAL_MACHINE\SYSTEM",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID",
    ];

    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
    {
        "The", "Inc", "Ltd", "LLC", "Corp", "Corporation", "Software",
        "Microsoft", "Windows", "System", "System32", "Update", "Setup",
        "Install", "Uninstall", "Common", "Shared", "Data", "Files",
        "Program", "App", "Application", "Tools", "Utility",
        "x86", "x64", "amd64", "arm64",
    };

    public ResidualAnalysisEngine(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    // ── Public API ───────────────────────────────────────────────

    public async Task<IReadOnlyList<ResidualItem>> ScanAsync(
        InstalledProgram program, CancellationToken ct = default)
    {
        _logger.Info($"Residual-Scan: {program.DisplayName}");

        var terms = BuildSearchTerms(program);
        _logger.Info($"  Suchbegriffe: [{string.Join(", ", terms)}]");

        if (terms.Count == 0)
        {
            _logger.Warning("  Keine Suchbegriffe – Scan abgebrochen.");
            return Array.Empty<ResidualItem>();
        }

        var areas   = BuildScanAreas(program);
        var results = new List<ResidualItem>();
        int done    = 0;

        foreach (var area in areas)
        {
            ct.ThrowIfCancellationRequested();
            ReportProgress(area.Name, area.BasePath, areas.Count, done, results.Count);

            try
            {
                var found = await Task.Run(() =>
                    area.IsRegistry
                        ? ScanRegistry(area, terms)
                        : ScanFileSystem(area, terms), ct);

                if (found.Count > 0)
                {
                    _logger.Info($"  {area.Name}: {found.Count} Rest(e)");
                    results.AddRange(found);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { _logger.Warning($"  {area.Name}: {ex.Message}"); }

            done++;
        }

        // Sort by confidence descending, auto-select high-confidence items
        var sorted = results
            .OrderByDescending(r => r.ConfidenceScore)
            .ThenBy(r => r.ItemType)
            .ThenBy(r => r.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var item in sorted)
            item.IsSelectedForDeletion = item.ConfidenceScore >= 0.5;

        _logger.Info($"Scan fertig: {sorted.Count} Reste gefunden");
        return sorted.AsReadOnly();
    }

    // ── Search-term extraction ───────────────────────────────────

    private static List<string> BuildSearchTerms(InstalledProgram program)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(program.DisplayName))
        {
            var clean = CleanName(program.DisplayName);
            if (clean.Length >= 3) terms.Add(clean);

            foreach (var w in clean.Split([' ', '-', '_', '.'],
                StringSplitOptions.RemoveEmptyEntries))
            {
                if (w.Length >= 3 && !StopWords.Contains(w)
                    && !w.All(c => char.IsDigit(c) || c == '.'))
                    terms.Add(w);
            }
        }

        if (!string.IsNullOrEmpty(program.Publisher))
        {
            foreach (var w in program.Publisher.Split([' ', ','],
                StringSplitOptions.RemoveEmptyEntries))
                if (w.Length >= 3 && !StopWords.Contains(w)) terms.Add(w);
        }

        if (!string.IsNullOrEmpty(program.MsiProductCode))
            terms.Add(program.MsiProductCode);

        return terms.ToList();
    }

    private static string CleanName(string name)
    {
        int p = name.IndexOf('(');
        var s = p > 0 ? name[..p] : name;
        return VersionSuffix().Replace(s, "").Trim();
    }

    [GeneratedRegex(@"\s+v?\d+[\.\d]*\s*$")]
    private static partial Regex VersionSuffix();

    // ── Scan-area construction ───────────────────────────────────

    private static List<ScanArea> BuildScanAreas(InstalledProgram program)
    {
        var list = new List<ScanArea>();

        if (!string.IsNullOrEmpty(program.InstallLocation)
            && Directory.Exists(program.InstallLocation))
            list.Add(new("Installationsordner", program.InstallLocation,
                false, ResidualDetectionSource.InstallPath));

        list.Add(new("Program Files",
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            false, ResidualDetectionSource.NameMatch));

        var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (pf86 != list[^1].BasePath)
            list.Add(new("Program Files (x86)", pf86,
                false, ResidualDetectionSource.NameMatch));

        list.Add(new("AppData Roaming",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            false, ResidualDetectionSource.AppDataScan));
        list.Add(new("AppData Local",
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            false, ResidualDetectionSource.AppDataScan));
        list.Add(new("ProgramData",
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            false, ResidualDetectionSource.ProgramDataScan));
        list.Add(new("Temp", Path.GetTempPath(),
            false, ResidualDetectionSource.TempScan));

        list.Add(new("Registry HKCU", @"HKEY_CURRENT_USER\SOFTWARE",
            true, ResidualDetectionSource.RegistryScan));
        list.Add(new("Registry HKLM", @"HKEY_LOCAL_MACHINE\SOFTWARE",
            true, ResidualDetectionSource.RegistryScan));

        return list;
    }

    // ── File-system scan ─────────────────────────────────────────

    private List<ResidualItem> ScanFileSystem(ScanArea area, List<string> terms)
    {
        var results = new List<ResidualItem>();
        if (!Directory.Exists(area.BasePath) || IsProtected(area.BasePath))
            return results;

        // The install-location itself is always confidence 1.0
        if (area.Source == ResidualDetectionSource.InstallPath)
        {
            try { results.Add(MakeDirItem(area.BasePath, 1.0,
                "Installationsordner existiert noch", area.Source)); }
            catch { /* access denied – skip */ }
            return results;
        }

        try
        {
            foreach (var dir in Directory.GetDirectories(area.BasePath))
            {
                if (IsProtected(dir)) continue;
                var score = Score(Path.GetFileName(dir), terms);
                if (score >= 0.5)
                {
                    try { results.Add(MakeDirItem(dir, score,
                        $"Ordnername passt (Score: {score:P0})", area.Source)); }
                    catch { /* skip */ }
                }
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (Exception ex) { _logger.Debug($"Scan-Fehler: {area.BasePath} – {ex.Message}"); }

        return results;
    }

    private static ResidualItem MakeDirItem(
        string path, double score, string reason, ResidualDetectionSource src)
    {
        long size = 0;
        try
        {
            size = new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0L; } });
        }
        catch { /* access denied */ }

        return new ResidualItem
        {
            Id = Guid.NewGuid().ToString("N"), FullPath = path,
            ItemType = ResidualItemType.Directory, SizeInBytes = size,
            ConfidenceScore = score, DetectionReason = reason,
            DetectionSource = src, IsSelectedForDeletion = score >= 0.5
        };
    }

    // ── Registry scan ────────────────────────────────────────────

    private List<ResidualItem> ScanRegistry(ScanArea area, List<string> terms)
    {
        var results = new List<ResidualItem>();
        if (IsProtectedReg(area.BasePath)) return results;

        try
        {
            var (hive, sub) = ParseRegPath(area.BasePath);
            if (hive is null) return results;

            using var baseKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Default);
            using var key     = baseKey.OpenSubKey(sub);
            if (key is null) return results;

            foreach (var name in key.GetSubKeyNames())
            {
                var full = $@"{area.BasePath}\{name}";
                if (IsProtectedReg(full)) continue;

                var score = Score(name, terms);
                if (score >= 0.6)
                {
                    results.Add(new ResidualItem
                    {
                        Id = Guid.NewGuid().ToString("N"), FullPath = full,
                        ItemType = ResidualItemType.RegistryKey,
                        ConfidenceScore = score,
                        DetectionReason = $"Registry-Key passt (Score: {score:P0})",
                        DetectionSource = ResidualDetectionSource.RegistryScan,
                        IsSelectedForDeletion = score >= 0.6
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }
        catch (Exception ex)
        { _logger.Debug($"Registry-Fehler: {area.BasePath} – {ex.Message}"); }

        return results;
    }

    // ── Scoring ──────────────────────────────────────────────────

    private static double Score(string name, List<string> terms)
    {
        if (string.IsNullOrWhiteSpace(name) || terms.Count == 0) return 0.0;
        double best = 0.0;
        foreach (var t in terms)
        {
            if (t.Length < 3) continue;
            if (name.Equals(t, StringComparison.OrdinalIgnoreCase)) return 1.0;
            if (name.Contains(t, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 0.5 + (double)t.Length / name.Length * 0.5);
        }
        return best;
    }

    // ── Guards ───────────────────────────────────────────────────

    private static bool IsProtected(string p) =>
        ProtectedPaths.Any(pp => p.StartsWith(pp, StringComparison.OrdinalIgnoreCase));

    private static bool IsProtectedReg(string p) =>
        ProtectedRegistryPrefixes.Any(pp => p.StartsWith(pp, StringComparison.OrdinalIgnoreCase));

    private static (RegistryHive? Hive, string Sub) ParseRegPath(string path) => path switch
    {
        _ when path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.CurrentUser, path[18..]),
        _ when path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.LocalMachine, path[19..]),
        _ => (null, path)
    };

    private void ReportProgress(string area, string path, int total, int done, int found)
    {
        ProgressChanged?.Invoke(this, new ScanProgressEventArgs
        {
            CurrentAreaName = area, CurrentPath = path,
            TotalAreas = total, CompletedAreas = done, ItemsFoundSoFar = found
        });
    }
}

// ── Internal helpers ─────────────────────────────────────────────

internal sealed record ScanArea(
    string Name, string BasePath, bool IsRegistry, ResidualDetectionSource Source);
'@

# ══════════════════════════════════════════════════════════════════════
# [11] Vault/VaultService.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[11] Vault/VaultService.cs..." -ForegroundColor Yellow

Write-SourceFile "Vault\VaultService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Vault;

/// <summary>
/// Creates, lists, verifies and deletes file/registry backups
/// stored in the local ZeroTrace vault.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class VaultService
{
    private readonly string          _basePath;
    private readonly IZeroTraceLogger _logger;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented          = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy   = JsonNamingPolicy.CamelCase
    };

    public event EventHandler<VaultProgressEventArgs>? ProgressChanged;

    public VaultService(IZeroTraceLogger logger, string? customPath = null)
    {
        _logger   = logger ?? throw new ArgumentNullException(nameof(logger));
        _basePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "Vault");
        Directory.CreateDirectory(_basePath);
    }

    // ── Create ───────────────────────────────────────────────────

    public async Task<VaultBackup> CreateBackupAsync(
        InstalledProgram program,
        IReadOnlyList<ResidualItem> items,
        CancellationToken ct = default)
    {
        var id       = Guid.NewGuid().ToString("N");
        var dir      = Path.Combine(_basePath, id);
        var filesDir = Path.Combine(dir, "files");
        var regDir   = Path.Combine(dir, "registry");

        _logger.Info($"Backup erstellen: {id} für {program.DisplayName} ({items.Count} Elemente)");
        Directory.CreateDirectory(filesDir);
        Directory.CreateDirectory(regDir);

        var entries   = new List<VaultEntry>();
        long totalSize = 0;

        for (int i = 0; i < items.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var item = items[i];
            Report($"Sichere: {item.DisplayName}", items.Count, i);

            try
            {
                var entry = item.ItemType switch
                {
                    ResidualItemType.File      => await BackupFileAsync(item, filesDir),
                    ResidualItemType.Directory => await BackupDirAsync(item, filesDir),
                    ResidualItemType.RegistryKey or ResidualItemType.RegistryValue
                        => BackupReg(item, regDir),
                    _  => FailEntry(item, "Unbekannter Typ")
                };
                entries.Add(entry);
                totalSize += entry.SizeInBytes ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Warning($"  Backup-Fehler: {item.FullPath} – {ex.Message}");
                entries.Add(FailEntry(item, ex.Message));
            }
        }

        var backup = new VaultBackup
        {
            BackupId       = id,
            CreatedAtUtc   = DateTime.UtcNow,
            ProgramName    = program.DisplayName,
            ProgramVersion = program.DisplayVersion,
            Entries        = entries.AsReadOnly(),
            TotalSizeBytes = totalSize,
            IntegrityHash  = Hash(entries),
            IsEncrypted    = false
        };

        var manifest = Path.Combine(dir, "manifest.json");
        await File.WriteAllTextAsync(manifest,
            JsonSerializer.Serialize(backup, Json), ct);

        _logger.Info($"  Backup fertig: {backup.SuccessCount}/{entries.Count} " +
                     $"gesichert ({backup.FormattedSize})");
        return backup;
    }

    // ── List / Delete ────────────────────────────────────────────

    public async Task<IReadOnlyList<VaultBackup>> GetAllBackupsAsync(
        CancellationToken ct = default)
    {
        var list = new List<VaultBackup>();
        if (!Directory.Exists(_basePath)) return list.AsReadOnly();

        foreach (var dir in Directory.GetDirectories(_basePath))
        {
            ct.ThrowIfCancellationRequested();
            var manifest = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                var json = await File.ReadAllTextAsync(manifest, ct);
                var b = JsonSerializer.Deserialize<VaultBackup>(json, Json);
                if (b is not null) list.Add(b);
            }
            catch (Exception ex)
            { _logger.Warning($"Manifest defekt: {dir} – {ex.Message}"); }
        }
        return list.OrderByDescending(b => b.CreatedAtUtc).ToList().AsReadOnly();
    }

    public void DeleteBackup(string backupId)
    {
        var dir = Path.Combine(_basePath, backupId);
        if (!Directory.Exists(dir)) return;
        Directory.Delete(dir, recursive: true);
        _logger.Info($"Backup gelöscht: {backupId}");
    }

    public string GetVaultPath() => _basePath;

    // ── Backup helpers ───────────────────────────────────────────

    private static async Task<VaultEntry> BackupFileAsync(
        ResidualItem item, string filesDir)
    {
        if (!File.Exists(item.FullPath))
            return FailEntry(item, "Datei existiert nicht");

        var rel  = SafeRel(item.FullPath);
        var dest = Path.Combine(filesDir, rel);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(item.FullPath, dest, overwrite: true);

        return new VaultEntry
        {
            EntryId            = Guid.NewGuid().ToString("N"),
            OriginalPath       = item.FullPath,
            BackupRelativePath = rel,
            EntryType          = VaultEntryType.File,
            SizeInBytes        = new FileInfo(item.FullPath).Length,
            ContentHash        = await FileHash(item.FullPath),
            BackupSuccessful   = true
        };
    }

    private static async Task<VaultEntry> BackupDirAsync(
        ResidualItem item, string filesDir)
    {
        if (!Directory.Exists(item.FullPath))
            return FailEntry(item, "Ordner existiert nicht");

        var rel  = SafeRel(item.FullPath);
        var dest = Path.Combine(filesDir, rel);
        long size = 0;

        foreach (var src in Directory.EnumerateFiles(
            item.FullPath, "*", SearchOption.AllDirectories))
        {
            try
            {
                var target = Path.Combine(dest,
                    Path.GetRelativePath(item.FullPath, src));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(src, target, overwrite: true);
                size += new FileInfo(src).Length;
            }
            catch { /* skip locked files */ }
        }

        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"),
            OriginalPath = item.FullPath,
            BackupRelativePath = rel,
            EntryType = VaultEntryType.Directory,
            SizeInBytes = size,
            BackupSuccessful = true
        };
    }

    private static VaultEntry BackupReg(ResidualItem item, string regDir)
    {
        var safe = item.FullPath.Replace('\\', '_').Replace(':', '_') + ".reg";
        var dest = Path.Combine(regDir, safe);

        using var proc = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = $"export \"{item.FullPath}\" \"{dest}\" /y",
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        });
        proc?.WaitForExit(15_000);

        bool ok = proc?.ExitCode == 0 && File.Exists(dest);
        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"),
            OriginalPath = item.FullPath,
            BackupRelativePath = Path.Combine("registry", safe),
            EntryType = item.ItemType == ResidualItemType.RegistryKey
                ? VaultEntryType.RegistryKey : VaultEntryType.RegistryValue,
            SizeInBytes = ok ? new FileInfo(dest).Length : 0,
            BackupSuccessful = ok,
            ErrorMessage = ok ? null : "Registry-Export fehlgeschlagen"
        };
    }

    // ── Utility ──────────────────────────────────────────────────

    private static VaultEntry FailEntry(ResidualItem item, string error) => new()
    {
        EntryId = Guid.NewGuid().ToString("N"),
        OriginalPath = item.FullPath,
        BackupRelativePath = "",
        EntryType = item.ItemType switch
        {
            ResidualItemType.File          => VaultEntryType.File,
            ResidualItemType.Directory     => VaultEntryType.Directory,
            ResidualItemType.RegistryKey   => VaultEntryType.RegistryKey,
            ResidualItemType.RegistryValue => VaultEntryType.RegistryValue,
            _ => VaultEntryType.File
        },
        BackupSuccessful = false,
        ErrorMessage = error
    };

    private static string SafeRel(string path) =>
        path.Replace(':', '_').Replace(@"\\", "_").TrimStart('_');

    private static async Task<string> FileHash(string path)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash);
    }

    private static string Hash(List<VaultEntry> entries)
    {
        var sb = new StringBuilder();
        foreach (var e in entries)
            sb.Append(e.OriginalPath).Append(':')
              .Append(e.ContentHash ?? "none").Append('|');
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    private void Report(string msg, int total, int current) =>
        ProgressChanged?.Invoke(this, new VaultProgressEventArgs
        { Message = msg, TotalItems = total, CompletedItems = current });
}

public sealed class VaultProgressEventArgs : EventArgs
{
    public required string Message        { get; init; }
    public required int    TotalItems     { get; init; }
    public required int    CompletedItems { get; init; }
    public int PercentComplete =>
        TotalItems == 0 ? 0 : CompletedItems * 100 / TotalItems;
}
'@

# ══════════════════════════════════════════════════════════════════════
# [12] Cleanup/CleanupService.cs
# ══════════════════════════════════════════════════════════════════════
Write-Host "[12] Cleanup/CleanupService.cs..." -ForegroundColor Yellow

Write-SourceFile "Cleanup\CleanupService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Cleanup;

// ── Result types ─────────────────────────────────────────────────

public sealed class CleanupResult
{
    public required int    TotalItems          { get; init; }
    public required int    SuccessfullyDeleted { get; init; }
    public required int    Failed              { get; init; }
    public required int    Skipped             { get; init; }
    public required long   FreedBytes          { get; init; }
    public required TimeSpan Duration           { get; init; }
    public required IReadOnlyList<CleanupItemResult> Details { get; init; }
    public required string BackupId             { get; init; }

    public string FormattedFreedSize => FreedBytes switch
    {
        < 1024          => $"{FreedBytes} B",
        < 1024 * 1024   => $"{FreedBytes / 1024.0:F1} KB",
        _               => $"{FreedBytes / (1024.0 * 1024):F1} MB"
    };
}

public sealed class CleanupItemResult
{
    public required string           Path               { get; init; }
    public required ResidualItemType ItemType            { get; init; }
    public required bool             Success             { get; init; }
    public          string?          ErrorMessage        { get; init; }
    public          bool             ScheduledForReboot  { get; init; }
}

// ── Service ──────────────────────────────────────────────────────

[SupportedOSPlatform("windows")]
public sealed class CleanupService
{
    private readonly IZeroTraceLogger _logger;
    private const    int             MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

    public event EventHandler<CleanupProgressEventArgs>? ProgressChanged;

    public CleanupService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CleanupResult> CleanAsync(
        IReadOnlyList<ResidualItem> items,
        string backupId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(backupId))
            throw new InvalidOperationException(
                "SICHERHEITSSPERRE: Löschung ohne Backup-ID verboten!");

        var selected = items.Where(i => i.IsSelectedForDeletion).ToList();
        _logger.Info($"Bereinigung: {selected.Count} Elemente, Backup: {backupId}");

        var sw      = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<CleanupItemResult>();
        int ok = 0, fail = 0, skip = 0;
        long freed = 0;

        // Delete order: files → reg values → reg keys → directories
        var ordered = selected.OrderBy(i => i.ItemType switch
        {
            ResidualItemType.File          => 0,
            ResidualItemType.RegistryValue => 1,
            ResidualItemType.RegistryKey   => 2,
            ResidualItemType.Directory     => 3,
            _ => 4
        }).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var item = ordered[i];
            ReportProgress(item.FullPath, ordered.Count, i);

            try
            {
                var result = await DeleteAsync(item);
                details.Add(result);
                if (result.Success) { ok++; freed += item.SizeInBytes ?? 0; }
                else skip++;
            }
            catch (Exception ex)
            {
                fail++;
                _logger.Warning($"  Fehler: {item.FullPath} – {ex.Message}");
                details.Add(new CleanupItemResult
                { Path = item.FullPath, ItemType = item.ItemType,
                  Success = false, ErrorMessage = ex.Message });
            }
        }

        sw.Stop();
        var result = new CleanupResult
        {
            TotalItems = ordered.Count, SuccessfullyDeleted = ok,
            Failed = fail, Skipped = skip, FreedBytes = freed,
            Duration = sw.Elapsed, Details = details.AsReadOnly(),
            BackupId = backupId
        };

        _logger.Info($"  Ergebnis: {ok} OK, {fail} Fehler, {skip} übersprungen " +
                     $"| {result.FormattedFreedSize} frei");
        return result;
    }

    // ── Delete dispatch ──────────────────────────────────────────

    private async Task<CleanupItemResult> DeleteAsync(ResidualItem item) =>
        item.ItemType switch
        {
            ResidualItemType.File          => await DeleteFileAsync(item),
            ResidualItemType.Directory     => DeleteDir(item),
            ResidualItemType.RegistryKey   => DeleteRegKey(item),
            ResidualItemType.RegistryValue => DeleteRegVal(item),
            _ => new() { Path = item.FullPath, ItemType = item.ItemType,
                         Success = false, ErrorMessage = "Unbekannter Typ" }
        };

    private async Task<CleanupItemResult> DeleteFileAsync(ResidualItem item)
    {
        if (!File.Exists(item.FullPath))
            return Ok(item, "Existiert nicht mehr");

        for (int a = 1; a <= MaxRetries; a++)
        {
            try
            {
                var attrs = File.GetAttributes(item.FullPath);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(item.FullPath, attrs & ~FileAttributes.ReadOnly);
                File.Delete(item.FullPath);
                return Ok(item);
            }
            catch (IOException) when (a < MaxRetries) { await Task.Delay(RetryDelay); }
            catch (UnauthorizedAccessException)
            {
                bool scheduled = ScheduleReboot(item.FullPath);
                return new() { Path = item.FullPath, ItemType = ResidualItemType.File,
                    Success = false, ScheduledForReboot = scheduled,
                    ErrorMessage = scheduled ? "Wird nach Neustart gelöscht" : "Zugriff verweigert" };
            }
        }
        return new() { Path = item.FullPath, ItemType = ResidualItemType.File,
            Success = false, ErrorMessage = "Nach Wiederholung nicht löschbar" };
    }

    private static CleanupItemResult DeleteDir(ResidualItem item)
    {
        if (!Directory.Exists(item.FullPath))
            return new() { Path = item.FullPath, ItemType = ResidualItemType.Directory,
                Success = false, ErrorMessage = "Existiert nicht mehr" };
        Directory.Delete(item.FullPath, recursive: true);
        return Ok(item);
    }

    private static CleanupItemResult DeleteRegKey(ResidualItem item)
    {
        var (hive, sub) = ParseReg(item.FullPath);
        if (hive is null) return RegFail(item, "Pfad nicht erkannt");

        int slash = sub.LastIndexOf('\\');
        if (slash < 0) return RegFail(item, "Ungültiger Pfad");

        using var baseKey   = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Default);
        using var parentKey = baseKey.OpenSubKey(sub[..slash], writable: true);
        if (parentKey is null) return RegFail(item, "Parent-Key nicht gefunden");

        parentKey.DeleteSubKeyTree(sub[(slash + 1)..], throwOnMissingSubKey: false);
        return Ok(item);
    }

    private static CleanupItemResult DeleteRegVal(ResidualItem item)
    {
        int sep = item.FullPath.LastIndexOf("::");
        if (sep < 0) return RegFail(item, "Ungültiges Format");

        var (hive, sub) = ParseReg(item.FullPath[..sep]);
        if (hive is null) return RegFail(item, "Pfad nicht erkannt");

        using var baseKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Default);
        using var key     = baseKey.OpenSubKey(sub, writable: true);
        if (key is null) return RegFail(item, "Key nicht gefunden");

        key.DeleteValue(item.FullPath[(sep + 2)..], throwOnMissingValue: false);
        return Ok(item);
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static CleanupItemResult Ok(ResidualItem item, string? msg = null) =>
        new() { Path = item.FullPath, ItemType = item.ItemType,
                Success = msg is null, ErrorMessage = msg };

    private static CleanupItemResult RegFail(ResidualItem item, string msg) =>
        new() { Path = item.FullPath, ItemType = item.ItemType,
                Success = false, ErrorMessage = msg };

    private static bool ScheduleReboot(string path)
    { try { return NativeMethods.MoveFileExW(path, null, 0x04); } catch { return false; } }

    private static (RegistryHive? Hive, string Sub) ParseReg(string p) => p switch
    {
        _ when p.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.CurrentUser, p[18..]),
        _ when p.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase)
            => (RegistryHive.LocalMachine, p[19..]),
        _ => (null, p)
    };

    private void ReportProgress(string path, int total, int current) =>
        ProgressChanged?.Invoke(this, new CleanupProgressEventArgs
        { CurrentPath = path, TotalItems = total, CompletedItems = current });
}

public sealed class CleanupProgressEventArgs : EventArgs
{
    public required string CurrentPath    { get; init; }
    public required int    TotalItems     { get; init; }
    public required int    CompletedItems { get; init; }
    public int PercentComplete =>
        TotalItems == 0 ? 0 : CompletedItems * 100 / TotalItems;
}

internal static partial class NativeMethods
{
    [System.Runtime.InteropServices.LibraryImport("kernel32.dll",
        SetLastError = true,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial bool MoveFileExW(
        string lpExistingFileName, string? lpNewFileName, int dwFlags);
}
'@

# ══════════════════════════════════════════════════════════════════════
# [13] Restore/RestoreService.cs (NEU)
# ══════════════════════════════════════════════════════════════════════
Write-Host "[13] Restore/RestoreService.cs (NEU)..." -ForegroundColor Yellow

Write-SourceFile "Restore\RestoreService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Runtime.Versioning;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Restore;

public sealed class RestoreResult
{
    public required int    TotalEntries { get; init; }
    public required int    Restored     { get; init; }
    public required int    Failed       { get; init; }
    public required TimeSpan Duration   { get; init; }
    public required IReadOnlyList<RestoreEntryResult> Details { get; init; }
}

public sealed class RestoreEntryResult
{
    public required string OriginalPath { get; init; }
    public required bool   Success      { get; init; }
    public          string? ErrorMessage { get; init; }
}

[SupportedOSPlatform("windows")]
public sealed class RestoreService
{
    private readonly string          _vaultPath;
    private readonly IZeroTraceLogger _logger;

    public event EventHandler<RestoreProgressEventArgs>? ProgressChanged;

    public RestoreService(IZeroTraceLogger logger, string? vaultPath = null)
    {
        _logger    = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultPath = vaultPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "Vault");
    }

    public async Task<RestoreResult> RestoreAsync(
        VaultBackup backup, CancellationToken ct = default)
    {
        _logger.Info($"Wiederherstellung: {backup.ProgramName} ({backup.BackupId})");
        var sw      = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<RestoreEntryResult>();
        int ok = 0, fail = 0;
        var backupDir = Path.Combine(_vaultPath, backup.BackupId);

        for (int i = 0; i < backup.Entries.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var entry = backup.Entries[i];
            if (!entry.BackupSuccessful) continue;

            Report($"Stelle wieder her: {Path.GetFileName(entry.OriginalPath)}",
                backup.Entries.Count, i);

            try
            {
                switch (entry.EntryType)
                {
                    case VaultEntryType.File:
                        await RestoreFileAsync(backupDir, entry);
                        break;
                    case VaultEntryType.Directory:
                        await RestoreDirAsync(backupDir, entry);
                        break;
                    case VaultEntryType.RegistryKey:
                    case VaultEntryType.RegistryValue:
                        RestoreRegistry(backupDir, entry);
                        break;
                }

                entry.HasBeenRestored = true;
                ok++;
                details.Add(new RestoreEntryResult
                { OriginalPath = entry.OriginalPath, Success = true });
            }
            catch (Exception ex)
            {
                fail++;
                _logger.Warning($"  Fehler: {entry.OriginalPath} – {ex.Message}");
                details.Add(new RestoreEntryResult
                { OriginalPath = entry.OriginalPath, Success = false,
                  ErrorMessage = ex.Message });
            }
        }

        sw.Stop();
        backup.Status = fail == 0
            ? VaultBackupStatus.FullyRestored
            : VaultBackupStatus.PartiallyRestored;

        _logger.Info($"  Ergebnis: {ok} OK, {fail} Fehler ({sw.Elapsed.TotalSeconds:F1}s)");

        return new RestoreResult
        {
            TotalEntries = backup.Entries.Count,
            Restored = ok, Failed = fail,
            Duration = sw.Elapsed,
            Details = details.AsReadOnly()
        };
    }

    private static async Task RestoreFileAsync(string backupDir, VaultEntry entry)
    {
        var src = Path.Combine(backupDir, "files", entry.BackupRelativePath);
        if (!File.Exists(src))
            throw new FileNotFoundException("Backup-Datei nicht gefunden", src);

        Directory.CreateDirectory(Path.GetDirectoryName(entry.OriginalPath)!);
        await Task.Run(() => File.Copy(src, entry.OriginalPath, overwrite: true));
    }

    private static async Task RestoreDirAsync(string backupDir, VaultEntry entry)
    {
        var src = Path.Combine(backupDir, "files", entry.BackupRelativePath);
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"Backup-Ordner nicht gefunden: {src}");

        await Task.Run(() =>
        {
            foreach (var file in Directory.EnumerateFiles(src, "*", SearchOption.AllDirectories))
            {
                var rel    = Path.GetRelativePath(src, file);
                var target = Path.Combine(entry.OriginalPath, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
            }
        });
    }

    private static void RestoreRegistry(string backupDir, VaultEntry entry)
    {
        var regFile = Path.Combine(backupDir, entry.BackupRelativePath);
        if (!File.Exists(regFile))
            throw new FileNotFoundException("Registry-Backup nicht gefunden", regFile);

        using var proc = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = $"import \"{regFile}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        });
        proc?.WaitForExit(15_000);

        if (proc?.ExitCode != 0)
            throw new InvalidOperationException("Registry-Import fehlgeschlagen");
    }

    private void Report(string msg, int total, int current) =>
        ProgressChanged?.Invoke(this, new RestoreProgressEventArgs
        { Message = msg, TotalItems = total, CompletedItems = current });
}

public sealed class RestoreProgressEventArgs : EventArgs
{
    public required string Message        { get; init; }
    public required int    TotalItems     { get; init; }
    public required int    CompletedItems { get; init; }
}
'@

# ══════════════════════════════════════════════════════════════════════
# [14] Crypto/CryptoService.cs (NEU)
# ══════════════════════════════════════════════════════════════════════
Write-Host "[14] Crypto/CryptoService.cs (NEU)..." -ForegroundColor Yellow

Write-SourceFile "Crypto\CryptoService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Security.Cryptography;
using System.Text;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Crypto;

/// <summary>
/// Provides hashing (SHA-256) and authenticated encryption (AES-256-GCM)
/// for vault backup integrity and optional encryption at rest.
/// </summary>
public sealed class CryptoService
{
    private readonly IZeroTraceLogger _logger;
    private const int KeySize   = 32; // 256 bit
    private const int NonceSize = 12; // AES-GCM standard
    private const int TagSize   = 16; // 128-bit auth tag

    public CryptoService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Derive a key from a password using Argon2-style PBKDF2.</summary>
    public byte[] DeriveKey(string password, byte[] salt, int iterations = 100_000)
    {
        using var kdf = new Rfc2898DeriveBytes(
            password, salt, iterations, HashAlgorithmName.SHA256);
        return kdf.GetBytes(KeySize);
    }

    /// <summary>Generate a cryptographically secure random salt.</summary>
    public static byte[] GenerateSalt(int length = 16) =>
        RandomNumberGenerator.GetBytes(length);

    /// <summary>Compute SHA-256 hash of a file.</summary>
    public async Task<string> ComputeFileHashAsync(
        string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream, ct);
        return Convert.ToHexString(hash);
    }

    /// <summary>Compute SHA-256 hash of a string.</summary>
    public static string ComputeStringHash(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    /// <summary>Encrypt data using AES-256-GCM.</summary>
    public byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag   = new byte[TagSize];
        var cipher = new byte[plaintext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, cipher, tag);

        // Layout: [nonce(12)] [tag(16)] [ciphertext(N)]
        var result = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce,  0, result, 0,                   NonceSize);
        Buffer.BlockCopy(tag,    0, result, NonceSize,            TagSize);
        Buffer.BlockCopy(cipher, 0, result, NonceSize + TagSize, cipher.Length);

        _logger.Debug($"Verschlüsselt: {plaintext.Length} → {result.Length} Bytes");
        return result;
    }

    /// <summary>Decrypt data encrypted with <see cref="Encrypt"/>.</summary>
    public byte[] Decrypt(byte[] combined, byte[] key)
    {
        if (combined.Length < NonceSize + TagSize)
            throw new CryptographicException("Daten zu kurz für AES-GCM.");

        var nonce  = combined.AsSpan(0, NonceSize).ToArray();
        var tag    = combined.AsSpan(NonceSize, TagSize).ToArray();
        var cipher = combined.AsSpan(NonceSize + TagSize).ToArray();
        var plain  = new byte[cipher.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);

        _logger.Debug($"Entschlüsselt: {combined.Length} → {plain.Length} Bytes");
        return plain;
    }

    /// <summary>Encrypt a file to a new file with .enc extension.</summary>
    public async Task EncryptFileAsync(
        string sourcePath, string destPath, byte[] key,
        CancellationToken ct = default)
    {
        var data      = await File.ReadAllBytesAsync(sourcePath, ct);
        var encrypted = Encrypt(data, key);
        await File.WriteAllBytesAsync(destPath, encrypted, ct);
        _logger.Info($"Datei verschlüsselt: {Path.GetFileName(sourcePath)}");
    }

    /// <summary>Decrypt a .enc file.</summary>
    public async Task DecryptFileAsync(
        string sourcePath, string destPath, byte[] key,
        CancellationToken ct = default)
    {
        var data      = await File.ReadAllBytesAsync(sourcePath, ct);
        var decrypted = Decrypt(data, key);
        await File.WriteAllBytesAsync(destPath, decrypted, ct);
        _logger.Info($"Datei entschlüsselt: {Path.GetFileName(sourcePath)}");
    }
}
'@

# ══════════════════════════════════════════════════════════════════════
# [NEW] Admin/AdminService.cs – ADMIN-BEREICH
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "[ADMIN] Admin/AdminService.cs (NEU – Admin-Bereich)..." -ForegroundColor Magenta

Write-SourceFile "Admin\AdminService.cs" @'
// ──────────────────────────────────────────────────────────────────
// ZeroTrace – Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License
// ──────────────────────────────────────────────────────────────────

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Admin;

// ── System Health Snapshot ────────────────────────────────────────

/// <summary>
/// A snapshot of system health metrics collected by AdminService.
/// </summary>
public sealed class SystemHealthReport
{
    public required DateTime       TimestampUtc        { get; init; }
    public required string         MachineName         { get; init; }
    public required string         OsVersion           { get; init; }
    public required bool           IsAdminMode         { get; init; }
    public required long           TotalDiskBytes      { get; init; }
    public required long           FreeDiskBytes       { get; init; }
    public required long           UsedMemoryBytes     { get; init; }
    public required long           TotalMemoryBytes    { get; init; }
    public required int            RunningProcesses    { get; init; }
    public required int            VaultBackupCount    { get; init; }
    public required long           VaultTotalSizeBytes { get; init; }
    public required long           LogTotalSizeBytes   { get; init; }
    public required string         ZeroTraceVersion    { get; init; }

    // Computed
    public double DiskUsagePercent =>
        TotalDiskBytes > 0 ? (1.0 - (double)FreeDiskBytes / TotalDiskBytes) * 100 : 0;
    public double MemoryUsagePercent =>
        TotalMemoryBytes > 0 ? (double)UsedMemoryBytes / TotalMemoryBytes * 100 : 0;

    public string FormattedFreeDisk => FormatSize(FreeDiskBytes);
    public string FormattedVaultSize => FormatSize(VaultTotalSizeBytes);
    public string FormattedLogSize => FormatSize(LogTotalSizeBytes);

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024              => $"{bytes} B",
        < 1024 * 1024       => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _                   => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

// ── Admin Configuration ──────────────────────────────────────────

public sealed class AdminConfig
{
    public int     MaxVaultAgeDays       { get; set; } = 30;
    public long    MaxVaultSizeBytes     { get; set; } = 2L * 1024 * 1024 * 1024; // 2 GB
    public int     MaxLogAgeDays         { get; set; } = 14;
    public bool    AutoCleanExpiredVaults { get; set; } = true;
    public bool    EnableDebugLogging    { get; set; }
    public string? CustomVaultPath       { get; set; }
    public string? CustomLogPath         { get; set; }
}

// ── Admin Service ────────────────────────────────────────────────

/// <summary>
/// Administrative dashboard service providing system health monitoring,
/// vault/log maintenance, and configuration management.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class AdminService
{
    private readonly IZeroTraceLogger _logger;
    private readonly string          _configPath;
    private readonly string          _vaultPath;
    private readonly string          _logPath;

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented        = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AdminService(IZeroTraceLogger logger,
        string? vaultPath = null, string? logPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace");

        _configPath = Path.Combine(appData, "admin-config.json");
        _vaultPath  = vaultPath ?? Path.Combine(appData, "Vault");
        _logPath    = logPath   ?? Path.Combine(appData, "Logs");
    }

    // ── Admin Check ──────────────────────────────────────────────

    /// <summary>Returns true if the current process is running elevated.</summary>
    public static bool IsRunningAsAdmin()
    {
        using var identity  = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>Restarts the application with elevated privileges.</summary>
    public static void RestartAsAdmin()
    {
        var exe = Environment.ProcessPath
            ?? throw new InvalidOperationException("Prozesspfad nicht ermittelbar.");

        Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = true,
            Verb = "runas"
        });

        Environment.Exit(0);
    }

    // ── System Health ────────────────────────────────────────────

    /// <summary>Collects a comprehensive system health report.</summary>
    public SystemHealthReport GetHealthReport()
    {
        var sysDrive = new DriveInfo(Path.GetPathRoot(
            Environment.GetFolderPath(Environment.SpecialFolder.System)) ?? "C:");

        long vaultSize  = GetDirectorySize(_vaultPath);
        int  vaultCount = Directory.Exists(_vaultPath)
            ? Directory.GetDirectories(_vaultPath).Length : 0;
        long logSize    = GetDirectorySize(_logPath);

        var version = typeof(AdminService).Assembly.GetName().Version;

        return new SystemHealthReport
        {
            TimestampUtc        = DateTime.UtcNow,
            MachineName         = Environment.MachineName,
            OsVersion           = Environment.OSVersion.VersionString,
            IsAdminMode         = IsRunningAsAdmin(),
            TotalDiskBytes      = sysDrive.TotalSize,
            FreeDiskBytes       = sysDrive.AvailableFreeSpace,
            UsedMemoryBytes     = Process.GetCurrentProcess().WorkingSet64,
            TotalMemoryBytes    = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            RunningProcesses    = Process.GetProcesses().Length,
            VaultBackupCount    = vaultCount,
            VaultTotalSizeBytes = vaultSize,
            LogTotalSizeBytes   = logSize,
            ZeroTraceVersion    = version?.ToString() ?? "1.0.0"
        };
    }

    // ── Vault Maintenance ────────────────────────────────────────

    /// <summary>Deletes vault backups older than maxAgeDays.</summary>
    public int PurgeExpiredVaults(int maxAgeDays)
    {
        if (!Directory.Exists(_vaultPath)) return 0;
        var cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
        int deleted = 0;

        foreach (var dir in Directory.GetDirectories(_vaultPath))
        {
            var manifest = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifest)) continue;

            try
            {
                var created = File.GetCreationTimeUtc(manifest);
                if (created < cutoff)
                {
                    Directory.Delete(dir, recursive: true);
                    deleted++;
                    _logger.Info($"Vault bereinigt: {Path.GetFileName(dir)} " +
                                $"(erstellt: {created:dd.MM.yyyy})");
                }
            }
            catch (Exception ex)
            { _logger.Warning($"Vault-Bereinigung fehlgeschlagen: {dir} – {ex.Message}"); }
        }

        _logger.Info($"Vault-Wartung: {deleted} abgelaufene Backups gelöscht");
        return deleted;
    }

    // ── Log Maintenance ──────────────────────────────────────────

    /// <summary>Deletes log files older than maxAgeDays.</summary>
    public int PurgeOldLogs(int maxAgeDays)
    {
        if (!Directory.Exists(_logPath)) return 0;
        var cutoff = DateTime.UtcNow.AddDays(-maxAgeDays);
        int deleted = 0;

        foreach (var file in Directory.GetFiles(_logPath, "*.log"))
        {
            try
            {
                if (File.GetCreationTimeUtc(file) < cutoff)
                {
                    File.Delete(file);
                    deleted++;
                }
            }
            catch (Exception ex)
            { _logger.Warning($"Log-Bereinigung: {file} – {ex.Message}"); }
        }

        _logger.Info($"Log-Wartung: {deleted} alte Logs gelöscht");
        return deleted;
    }

    // ── Configuration ────────────────────────────────────────────

    /// <summary>Loads admin configuration from disk.</summary>
    public AdminConfig LoadConfig()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AdminConfig>(json, Json)
                    ?? new AdminConfig();
            }
        }
        catch (Exception ex)
        { _logger.Warning($"Config laden fehlgeschlagen: {ex.Message}"); }

        return new AdminConfig();
    }

    /// <summary>Saves admin configuration to disk.</summary>
    public void SaveConfig(AdminConfig config)
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (dir is not null) Directory.CreateDirectory(dir);
            File.WriteAllText(_configPath,
                JsonSerializer.Serialize(config, Json));
            _logger.Info("Admin-Konfiguration gespeichert");
        }
        catch (Exception ex)
        { _logger.Error("Config speichern fehlgeschlagen", ex); }
    }

    // ── Auto-Maintenance ─────────────────────────────────────────

    /// <summary>
    /// Runs automatic maintenance based on saved configuration.
    /// Should be called on application startup.
    /// </summary>
    public void RunAutoMaintenance()
    {
        var config = LoadConfig();
        _logger.Info("Auto-Wartung gestartet…");

        if (config.AutoCleanExpiredVaults)
            PurgeExpiredVaults(config.MaxVaultAgeDays);

        PurgeOldLogs(config.MaxLogAgeDays);
        _logger.Info("Auto-Wartung abgeschlossen");
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path)) return 0;
        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Sum(f => { try { return f.Length; } catch { return 0L; } });
        }
        catch { return 0; }
    }
}
'@

# ══════════════════════════════════════════════════════════════════════
# Aktualisierte .csproj
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "[PROJ] ZeroTrace.Core.csproj aktualisieren..." -ForegroundColor Yellow

Write-SourceFile "ZeroTrace.Core.csproj" @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <AssemblyTitle>ZeroTrace.Core</AssemblyTitle>
    <Company>Mario B.</Company>
    <Copyright>Copyright (c) 2026 Mario B. MIT License</Copyright>
    <Description>Core library for ZeroTrace – Advanced Uninstaller System</Description>
    <Version>1.0.0</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Blake3" Version="2.2.1" />
    <PackageReference Include="Microsoft.Win32.Registry" Version="5.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ZeroTrace.Native\ZeroTrace.Native.csproj" />
  </ItemGroup>
</Project>
'@ -Base $srcCore

# ══════════════════════════════════════════════════════════════════════
# ZUSAMMENFASSUNG
# ══════════════════════════════════════════════════════════════════════
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  ✅ UPGRADE ABGESCHLOSSEN!                                  ║" -ForegroundColor Green
Write-Host "╠══════════════════════════════════════════════════════════════╣" -ForegroundColor Green
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "║  Aktualisierte Dateien:                                      ║" -ForegroundColor Green
Write-Host "║  ─────────────────────────                                   ║" -ForegroundColor Green
Write-Host "║  [01] Models/ProgramSource.cs        ✅                      ║" -ForegroundColor Green
Write-Host "║  [01] Models/InstalledProgram.cs      ✅                     ║" -ForegroundColor Green
Write-Host "║  [02] Models/ResidualItem.cs          ✅                     ║" -ForegroundColor Green
Write-Host "║  [03] Models/VaultBackup.cs           ✅                     ║" -ForegroundColor Green
Write-Host "║  [04] Logging/IZeroTraceLogger.cs     ✅                     ║" -ForegroundColor Green
Write-Host "║  [05] Logging/FileLogger.cs           ✅                     ║" -ForegroundColor Green
Write-Host "║  [06] Discovery/IDiscoveryProvider.cs ✅                     ║" -ForegroundColor Green
Write-Host "║  [07] Discovery/RegistryDiscoveryProv ✅                     ║" -ForegroundColor Green
Write-Host "║  [08] Discovery/DiscoveryService.cs   ✅                     ║" -ForegroundColor Green
Write-Host "║  [09] Uninstall/UninstallService.cs   ✅                     ║" -ForegroundColor Green
Write-Host "║  [10] Analysis/ResidualAnalysisEngine ✅                     ║" -ForegroundColor Green
Write-Host "║  [11] Vault/VaultService.cs           ✅                     ║" -ForegroundColor Green
Write-Host "║  [12] Cleanup/CleanupService.cs       ✅                     ║" -ForegroundColor Green
Write-Host "║  [13] Restore/RestoreService.cs       ✅ NEU                 ║" -ForegroundColor Green
Write-Host "║  [14] Crypto/CryptoService.cs         ✅ NEU                 ║" -ForegroundColor Green
Write-Host "║  [AD] Admin/AdminService.cs           ✅ NEU (Admin-Bereich) ║" -ForegroundColor Magenta
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "║  Projektdateien:                                             ║" -ForegroundColor Green
Write-Host "║  ─────────────────────────                                   ║" -ForegroundColor Green
Write-Host "║  LICENSE       (MIT, Copyright Mario B.)   ✅                ║" -ForegroundColor Green
Write-Host "║  README.md     (GitHub-ready)              ✅                ║" -ForegroundColor Green
Write-Host "║  .gitignore                                ✅                ║" -ForegroundColor Green
Write-Host "║  ZeroTrace.Core.csproj (aktualisiert)      ✅                ║" -ForegroundColor Green
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "║  Nächster Schritt:                                           ║" -ForegroundColor Green
Write-Host "║    cd C:\Projekte\ZeroTrace                                  ║" -ForegroundColor Cyan
Write-Host "║    dotnet build src\ZeroTrace.Core                           ║" -ForegroundColor Cyan
Write-Host "║                                                              ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""