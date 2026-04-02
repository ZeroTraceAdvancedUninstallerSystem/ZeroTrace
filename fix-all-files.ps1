# ZeroTrace - Alle 12 Dateien reparieren
# Ausfuehren mit: powershell -ExecutionPolicy Bypass -File fix-all-files.ps1

$root = "C:\Projekte\ZeroTrace\src\ZeroTrace.Core"

Write-Host "=== ZeroTrace Dateien werden repariert ===" -ForegroundColor Cyan

# ============================================================
# DATEI 1: Models/InstalledProgram.cs
# ============================================================
$file1 = @'
namespace ZeroTrace.Core.Models;

public sealed class InstalledProgram
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public string? DisplayVersion { get; init; }
    public string? Publisher { get; init; }
    public string? InstallLocation { get; init; }
    public DateTime? InstallDate { get; init; }
    public long? EstimatedSizeBytes { get; init; }
    public string? UninstallString { get; init; }
    public string? QuietUninstallString { get; init; }
    public ProgramSource Source { get; init; }
    public bool IsSystemComponent { get; init; }
    public string? RegistryKeyPath { get; init; }
    public string? MsiProductCode { get; init; }
    public string? IconPath { get; init; }

    public string FormattedSize
    {
        get
        {
            if (EstimatedSizeBytes is null) return "-";
            if (EstimatedSizeBytes < 1024) return EstimatedSizeBytes + " B";
            if (EstimatedSizeBytes < 1024 * 1024) return (EstimatedSizeBytes / 1024.0).ToString("F1") + " KB";
            if (EstimatedSizeBytes < 1024 * 1024 * 1024) return (EstimatedSizeBytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (EstimatedSizeBytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }

    public string FormattedInstallDate => InstallDate?.ToString("dd.MM.yyyy") ?? "-";
    public bool IsMsiInstallation => !string.IsNullOrEmpty(MsiProductCode);

    public bool CanBeUninstalled =>
        !string.IsNullOrEmpty(UninstallString) ||
        !string.IsNullOrEmpty(QuietUninstallString) ||
        !string.IsNullOrEmpty(MsiProductCode);

    public override string ToString() => DisplayName + " " + DisplayVersion + " (" + Publisher + ")";
}

public enum ProgramSource
{
    RegistryLocalMachine,
    RegistryCurrentUser,
    MsiDatabase,
    ManualDiscovery
}
'@
Set-Content -Path "$root\Models\InstalledProgram.cs" -Value $file1 -Encoding UTF8
Write-Host "  [1/12] InstalledProgram.cs" -ForegroundColor Green

# ============================================================
# DATEI 2: Models/ResidualItem.cs
# ============================================================
$file2 = @'
namespace ZeroTrace.Core.Models;

public sealed class ResidualItem
{
    public required string Id { get; init; }
    public required string FullPath { get; init; }

    public string DisplayName =>
        System.IO.Path.GetFileName(FullPath.TrimEnd('\\')) ?? FullPath;

    public required ResidualItemType ItemType { get; init; }
    public long? SizeInBytes { get; init; }
    public required double ConfidenceScore { get; init; }
    public required string DetectionReason { get; init; }
    public bool IsSelectedForDeletion { get; set; }
    public required ResidualDetectionSource DetectionSource { get; init; }

    public string CategoryDisplayName => ItemType switch
    {
        ResidualItemType.File => "Dateien",
        ResidualItemType.Directory => "Ordner",
        ResidualItemType.RegistryKey => "Registry-Schluessel",
        ResidualItemType.RegistryValue => "Registry-Werte",
        _ => "Sonstiges"
    };

    public string FormattedSize
    {
        get
        {
            if (SizeInBytes is null) return "-";
            if (SizeInBytes == 0) return "Leer";
            if (SizeInBytes < 1024) return SizeInBytes + " B";
            if (SizeInBytes < 1024 * 1024) return (SizeInBytes / 1024.0).ToString("F1") + " KB";
            return (SizeInBytes / (1024.0 * 1024)).ToString("F1") + " MB";
        }
    }
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
Set-Content -Path "$root\Models\ResidualItem.cs" -Value $file2 -Encoding UTF8
Write-Host "  [2/12] ResidualItem.cs" -ForegroundColor Green

# ============================================================
# DATEI 3: Models/VaultBackup.cs
# ============================================================
$file3 = @'
namespace ZeroTrace.Core.Models;

public sealed class VaultBackup
{
    public required string BackupId { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required string ProgramName { get; init; }
    public string? ProgramVersion { get; init; }
    public required IReadOnlyList<VaultEntry> Entries { get; init; }
    public long TotalSizeBytes { get; init; }
    public required string IntegrityHash { get; init; }
    public bool IsEncrypted { get; init; }
    public VaultBackupStatus Status { get; set; } = VaultBackupStatus.Active;

    public int SuccessCount => Entries.Count(e => e.BackupSuccessful);
    public int FailedCount => Entries.Count(e => !e.BackupSuccessful);

    public string FormattedSize
    {
        get
        {
            if (TotalSizeBytes < 1024) return TotalSizeBytes + " B";
            if (TotalSizeBytes < 1024 * 1024) return (TotalSizeBytes / 1024.0).ToString("F1") + " KB";
            if (TotalSizeBytes < 1024 * 1024 * 1024) return (TotalSizeBytes / (1024.0 * 1024)).ToString("F1") + " MB";
            return (TotalSizeBytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }

    public string FormattedDate => CreatedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
}

public sealed class VaultEntry
{
    public required string EntryId { get; init; }
    public required string OriginalPath { get; init; }
    public required string BackupRelativePath { get; init; }
    public required VaultEntryType EntryType { get; init; }
    public long? SizeInBytes { get; init; }
    public string? ContentHash { get; init; }
    public bool BackupSuccessful { get; set; }
    public bool HasBeenRestored { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum VaultEntryType
{
    File,
    Directory,
    RegistryKey,
    RegistryValue
}

public enum VaultBackupStatus
{
    Active,
    PartiallyRestored,
    FullyRestored,
    Expired,
    Corrupted
}
'@
Set-Content -Path "$root\Models\VaultBackup.cs" -Value $file3 -Encoding UTF8
Write-Host "  [3/12] VaultBackup.cs" -ForegroundColor Green

# ============================================================
# DATEI 4: Logging/IZeroTraceLogger.cs
# ============================================================
$file4 = @'
namespace ZeroTrace.Core.Logging;

public interface IZeroTraceLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3
}
'@
Set-Content -Path "$root\Logging\IZeroTraceLogger.cs" -Value $file4 -Encoding UTF8
Write-Host "  [4/12] IZeroTraceLogger.cs" -ForegroundColor Green

# ============================================================
# DATEI 5: Logging/FileLogger.cs
# ============================================================
$file5 = @'
using System.Collections.Concurrent;
using System.Text;

namespace ZeroTrace.Core.Logging;

public sealed class FileLogger : IZeroTraceLogger, IAsyncDisposable
{
    private readonly string _logFilePath;
    private readonly LogLevel _minimumLevel;
    private readonly ConcurrentQueue<string> _messageQueue = new();
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundFlushTask;

    public FileLogger(string? logDirectory = null, LogLevel minimumLevel = LogLevel.Info)
    {
        _minimumLevel = minimumLevel;
        string directory = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "Logs");
        Directory.CreateDirectory(directory);
        string fileName = "ZeroTrace_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
        _logFilePath = Path.Combine(directory, fileName);
        _backgroundFlushTask = Task.Run(BackgroundFlushLoopAsync);
    }

    public void Debug(string message) => Enqueue(LogLevel.Debug, message);
    public void Info(string message) => Enqueue(LogLevel.Info, message);
    public void Warning(string message) => Enqueue(LogLevel.Warning, message);

    public void Error(string message, Exception? exception = null)
    {
        string fullMessage = exception is null
            ? message
            : message + " | " + exception.GetType().Name + ": " + exception.Message +
              "\n  StackTrace: " + exception.StackTrace;
        Enqueue(LogLevel.Error, fullMessage);
    }

    private void Enqueue(LogLevel level, string message)
    {
        if (level < _minimumLevel) return;
        string formatted =
            "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " +
            "[" + level.ToString().ToUpperInvariant().PadRight(7) + "] " +
            "[Thread " + Environment.CurrentManagedThreadId.ToString("D3") + "] " +
            message;
        _messageQueue.Enqueue(formatted);
    }

    private async Task BackgroundFlushLoopAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(500, _cts.Token);
                await FlushQueueToFileAsync();
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
        await FlushQueueToFileAsync();
    }

    private async Task FlushQueueToFileAsync()
    {
        if (_messageQueue.IsEmpty) return;
        await _writeLock.WaitAsync();
        try
        {
            var sb = new StringBuilder();
            while (_messageQueue.TryDequeue(out string? message))
                sb.AppendLine(message);
            if (sb.Length > 0)
                await File.AppendAllTextAsync(_logFilePath, sb.ToString());
        }
        finally { _writeLock.Release(); }
    }

    public string GetCurrentLogFilePath() => _logFilePath;

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        try { await _backgroundFlushTask; }
        catch (OperationCanceledException) { }
        _cts.Dispose();
        _writeLock.Dispose();
    }
}
'@
Set-Content -Path "$root\Logging\FileLogger.cs" -Value $file5 -Encoding UTF8
Write-Host "  [5/12] FileLogger.cs" -ForegroundColor Green

# ============================================================
# DATEI 6: Discovery/IDiscoveryProvider.cs
# ============================================================
$file6 = @'
using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Discovery;

public interface IDiscoveryProvider
{
    Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(
        CancellationToken cancellationToken = default);

    string ProviderName { get; }
}
'@
Set-Content -Path "$root\Discovery\IDiscoveryProvider.cs" -Value $file6 -Encoding UTF8
Write-Host "  [6/12] IDiscoveryProvider.cs" -ForegroundColor Green

# ============================================================
# DATEI 7: Discovery/RegistryDiscoveryProvider.cs
# ============================================================
$file7 = @'
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

[SupportedOSPlatform("windows")]
public sealed class RegistryDiscoveryProvider : IDiscoveryProvider
{
    public string ProviderName => "Windows Registry (HKLM/HKCU)";
    private readonly IZeroTraceLogger _logger;

    private static readonly RegistrySource[] Sources =
    [
        new("HKLM (64-Bit)", RegistryHive.LocalMachine,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            RegistryView.Registry64, ProgramSource.RegistryLocalMachine),
        new("HKLM (32-Bit)", RegistryHive.LocalMachine,
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

    public async Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.Info("Registry-Scan gestartet...");
        return await Task.Run(() =>
        {
            var programs = new List<InstalledProgram>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var source in Sources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var found = ReadRegistrySource(source);
                    foreach (var program in found)
                    {
                        if (seenIds.Add(program.Id))
                            programs.Add(program);
                    }
                    _logger.Debug("  " + source.Description + ": " + found.Count + " Programm(e)");
                }
                catch (System.Security.SecurityException ex)
                {
                    _logger.Warning("  " + source.Description + ": Zugriff verweigert - " + ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Warning("  " + source.Description + ": Fehler - " + ex.Message);
                }
            }
            _logger.Info("Registry-Scan abgeschlossen: " + programs.Count + " Programme.");
            return programs.AsReadOnly();
        }, cancellationToken);
    }

    private List<InstalledProgram> ReadRegistrySource(RegistrySource source)
    {
        var results = new List<InstalledProgram>();
        using var baseKey = RegistryKey.OpenBaseKey(source.Hive, source.View);
        using var uninstallKey = baseKey.OpenSubKey(source.Path);
        if (uninstallKey is null) return results;

        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
        {
            try
            {
                using var programKey = uninstallKey.OpenSubKey(subKeyName);
                if (programKey is null) continue;
                string? displayName = programKey.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                bool isSystemComponent = IsSystemComponent(programKey);

                var program = new InstalledProgram
                {
                    Id = BuildUniqueId(source, subKeyName),
                    DisplayName = displayName.Trim(),
                    DisplayVersion = SafeGetString(programKey, "DisplayVersion"),
                    Publisher = SafeGetString(programKey, "Publisher"),
                    InstallLocation = NormalizePath(SafeGetString(programKey, "InstallLocation")),
                    InstallDate = ParseInstallDate(SafeGetString(programKey, "InstallDate")),
                    EstimatedSizeBytes = ParseEstimatedSize(programKey),
                    UninstallString = SafeGetString(programKey, "UninstallString"),
                    QuietUninstallString = SafeGetString(programKey, "QuietUninstallString"),
                    Source = source.ProgramSource,
                    IsSystemComponent = isSystemComponent,
                    RegistryKeyPath = programKey.Name,
                    MsiProductCode = Guid.TryParse(subKeyName, out _) ? subKeyName : null,
                    IconPath = SafeGetString(programKey, "DisplayIcon"),
                };
                results.Add(program);
            }
            catch (Exception ex)
            {
                _logger.Debug("  Eintrag uebersprungen: " + subKeyName + " - " + ex.Message);
            }
        }
        return results;
    }

    private static string BuildUniqueId(RegistrySource source, string subKeyName)
    {
        string hive = source.Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU";
        string view = source.View == RegistryView.Registry64 ? "64" : "32";
        return hive + "_" + view + "_" + subKeyName;
    }

    private static bool IsSystemComponent(RegistryKey key)
    {
        if (key.GetValue("SystemComponent") is int sc && sc == 1) return true;
        string? releaseType = key.GetValue("ReleaseType") as string;
        if (releaseType is "Update" or "Hotfix" or "Security Update") return true;
        if (key.GetValue("ParentKeyName") is string parent && !string.IsNullOrEmpty(parent)) return true;
        return false;
    }

    private static string? SafeGetString(RegistryKey key, string valueName)
    {
        try { return key.GetValue(valueName)?.ToString()?.Trim(); }
        catch { return null; }
    }

    private static DateTime? ParseInstallDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString)) return null;
        if (DateTime.TryParseExact(dateString.Trim(), "yyyyMMdd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            return result;
        return null;
    }

    private static long? ParseEstimatedSize(RegistryKey key)
    {
        var value = key.GetValue("EstimatedSize");
        return value switch
        {
            int intValue => (long)intValue * 1024,
            long longValue => longValue * 1024,
            _ => null
        };
    }

    private static string? NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        return path.Trim().Trim('"').TrimEnd('\\');
    }

    private sealed record RegistrySource(
        string Description, RegistryHive Hive, string Path,
        RegistryView View, ProgramSource ProgramSource);
}
'@
Set-Content -Path "$root\Discovery\RegistryDiscoveryProvider.cs" -Value $file7 -Encoding UTF8
Write-Host "  [7/12] RegistryDiscoveryProvider.cs" -ForegroundColor Green

# ============================================================
# DATEI 8: Discovery/DiscoveryService.cs
# ============================================================
$file8 = @'
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

public sealed class DiscoveryService
{
    private readonly IReadOnlyList<IDiscoveryProvider> _providers;
    private readonly IZeroTraceLogger _logger;
    public event EventHandler<DiscoveryProgressEventArgs>? ProgressChanged;

    public DiscoveryService(IEnumerable<IDiscoveryProvider> providers, IZeroTraceLogger logger)
    {
        _providers = providers?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<InstalledProgram>> GetAllProgramsAsync(
        bool includeSystemComponents = false, CancellationToken cancellationToken = default)
    {
        _logger.Info("Programmsuche gestartet");
        var allPrograms = new List<InstalledProgram>();
        int providerIndex = 0;

        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged?.Invoke(this, new DiscoveryProgressEventArgs
            {
                CurrentProvider = provider.ProviderName,
                ProviderIndex = providerIndex,
                TotalProviders = _providers.Count
            });

            try
            {
                var found = await provider.DiscoverAsync(cancellationToken);
                allPrograms.AddRange(found);
                _logger.Info("  Provider: " + provider.ProviderName + ": " + found.Count + " Programme.");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.Error("  Provider " + provider.ProviderName + " fehlgeschlagen.", ex);
            }
            providerIndex++;
        }

        var deduplicated = DeduplicatePrograms(allPrograms);
        var filtered = includeSystemComponents
            ? deduplicated
            : deduplicated.Where(p => !p.IsSystemComponent).ToList();

        var sorted = filtered
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList().AsReadOnly();

        _logger.Info("Programmsuche abgeschlossen: " + sorted.Count + " Programme");
        return sorted;
    }

    private List<InstalledProgram> DeduplicatePrograms(List<InstalledProgram> programs)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<InstalledProgram>();
        foreach (var program in programs)
        {
            string key = program.DisplayName + "|" + (program.DisplayVersion ?? "?");
            if (seen.Add(key)) unique.Add(program);
        }
        int removed = programs.Count - unique.Count;
        if (removed > 0) _logger.Debug("  " + removed + " Duplikat(e) entfernt.");
        return unique;
    }
}

public sealed class DiscoveryProgressEventArgs : EventArgs
{
    public required string CurrentProvider { get; init; }
    public required int ProviderIndex { get; init; }
    public required int TotalProviders { get; init; }
}
'@
Set-Content -Path "$root\Discovery\DiscoveryService.cs" -Value $file8 -Encoding UTF8
Write-Host "  [8/12] DiscoveryService.cs" -ForegroundColor Green

# ============================================================
# DATEI 9: Uninstall/UninstallService.cs
# ============================================================
$file9 = @'
using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Uninstall;

public sealed class UninstallResult
{
    public required bool Success { get; init; }
    public required int ExitCode { get; init; }
    public required TimeSpan Duration { get; init; }
    public required UninstallMethod MethodUsed { get; init; }
    public string? ErrorMessage { get; init; }
    public bool RequiresReboot { get; init; }
}

public enum UninstallMethod
{
    MsiStandard, MsiQuiet, ExecutableNormal, ExecutableQuiet, NotAvailable
}

[SupportedOSPlatform("windows")]
public sealed class UninstallService
{
    private readonly IZeroTraceLogger _logger;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);
    public event EventHandler<UninstallStatusEventArgs>? StatusChanged;

    public UninstallService(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UninstallResult> UninstallAsync(
        InstalledProgram program, bool preferQuietMode = false,
        CancellationToken cancellationToken = default)
    {
        _logger.Info("Deinstallation gestartet: " + program.DisplayName);
        ReportStatus("Bereite Deinstallation vor: " + program.DisplayName + "...");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var strategy = DetermineStrategy(program, preferQuietMode);
            if (strategy.CommandLine is null)
            {
                return new UninstallResult
                {
                    Success = false, ExitCode = -1, Duration = stopwatch.Elapsed,
                    MethodUsed = UninstallMethod.NotAvailable,
                    ErrorMessage = "Kein Deinstallationsbefehl verfuegbar."
                };
            }
            _logger.Info("  Methode: " + strategy.Method);
            return await ExecuteUninstallerAsync(strategy.CommandLine, strategy.Method,
                program.DisplayName, stopwatch, cancellationToken);
        }
        catch (OperationCanceledException) { _logger.Warning("Abgebrochen."); throw; }
        catch (Exception ex)
        {
            _logger.Error("Fehler bei Deinstallation.", ex);
            return new UninstallResult
            {
                Success = false, ExitCode = -1, Duration = stopwatch.Elapsed,
                MethodUsed = UninstallMethod.NotAvailable, ErrorMessage = ex.Message
            };
        }
    }

    private static (string? CommandLine, UninstallMethod Method) DetermineStrategy(
        InstalledProgram program, bool preferQuiet)
    {
        if (!string.IsNullOrEmpty(program.MsiProductCode))
        {
            return preferQuiet
                ? ("msiexec.exe /x " + program.MsiProductCode + " /qn /norestart", UninstallMethod.MsiQuiet)
                : ("msiexec.exe /x " + program.MsiProductCode + " /norestart", UninstallMethod.MsiStandard);
        }
        if (preferQuiet && !string.IsNullOrEmpty(program.QuietUninstallString))
            return (program.QuietUninstallString, UninstallMethod.ExecutableQuiet);
        if (!string.IsNullOrEmpty(program.UninstallString))
            return (program.UninstallString, UninstallMethod.ExecutableNormal);
        return (null, UninstallMethod.NotAvailable);
    }

    private async Task<UninstallResult> ExecuteUninstallerAsync(
        string commandLine, UninstallMethod method, string programName,
        Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        var parsed = ParseCommandLine(commandLine);
        ReportStatus("Starte Deinstaller fuer " + programName + "...");

        var startInfo = new ProcessStartInfo
        {
            FileName = parsed.FileName, Arguments = parsed.Arguments,
            UseShellExecute = true, Verb = "runas",
        };
        using var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                return new UninstallResult
                {
                    Success = false, ExitCode = -1, Duration = stopwatch.Elapsed,
                    MethodUsed = method, ErrorMessage = "Prozess konnte nicht starten."
                };
            }
            ReportStatus("Warte auf Deinstaller von " + programName + "...");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DefaultTimeout);

            try { await process.WaitForExitAsync(timeoutCts.Token); }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                try { if (!process.HasExited) process.Kill(true); } catch { }
                return new UninstallResult
                {
                    Success = false, ExitCode = -1, Duration = stopwatch.Elapsed,
                    MethodUsed = method, ErrorMessage = "Zeitueberschreitung."
                };
            }

            stopwatch.Stop();
            int exitCode = process.ExitCode;
            bool success = exitCode is 0 or 1605 or 1614 or 1641 or 3010;

            _logger.Info("  Exit-Code=" + exitCode + ", Erfolg=" + success +
                ", Dauer=" + stopwatch.Elapsed.TotalSeconds.ToString("F1") + "s");

            return new UninstallResult
            {
                Success = success, ExitCode = exitCode, Duration = stopwatch.Elapsed,
                MethodUsed = method, RequiresReboot = exitCode is 1641 or 3010,
                ErrorMessage = success ? null : "Exit-Code " + exitCode + "."
            };
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return new UninstallResult
            {
                Success = false, ExitCode = ex.NativeErrorCode, Duration = stopwatch.Elapsed,
                MethodUsed = method,
                ErrorMessage = ex.NativeErrorCode == 1223 ? "Administrator-Anfrage abgelehnt." : ex.Message
            };
        }
    }

    private static (string FileName, string Arguments) ParseCommandLine(string commandLine)
    {
        commandLine = commandLine.Trim();
        if (commandLine.StartsWith('"'))
        {
            int closingQuote = commandLine.IndexOf('"', 1);
            if (closingQuote > 0)
                return (commandLine.Substring(1, closingQuote - 1),
                    closingQuote + 1 < commandLine.Length ? commandLine.Substring(closingQuote + 1).Trim() : "");
        }
        if (commandLine.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            int firstSpace = commandLine.IndexOf(' ');
            return firstSpace > 0 ? ("msiexec.exe", commandLine.Substring(firstSpace + 1).Trim()) : ("msiexec.exe", "");
        }
        int exeIndex = commandLine.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (exeIndex > 0)
        {
            int splitAt = exeIndex + 4;
            return (commandLine.Substring(0, splitAt).Trim(),
                splitAt < commandLine.Length ? commandLine.Substring(splitAt).Trim() : "");
        }
        int space = commandLine.IndexOf(' ');
        return space > 0 ? (commandLine.Substring(0, space), commandLine.Substring(space + 1)) : (commandLine, "");
    }

    private void ReportStatus(string message)
    {
        StatusChanged?.Invoke(this, new UninstallStatusEventArgs(message));
    }
}

public sealed class UninstallStatusEventArgs : EventArgs
{
    public string Message { get; }
    public UninstallStatusEventArgs(string message) { Message = message; }
}
'@
Set-Content -Path "$root\Uninstall\UninstallService.cs" -Value $file9 -Encoding UTF8
Write-Host "  [9/12] UninstallService.cs" -ForegroundColor Green

# ============================================================
# DATEI 10: Analysis/ResidualAnalysisEngine.cs
# ============================================================
$file10 = @'
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Analysis;

public sealed class ScanProgressEventArgs : EventArgs
{
    public required string CurrentAreaName { get; init; }
    public required string CurrentPath { get; init; }
    public required int TotalAreas { get; init; }
    public required int CompletedAreas { get; init; }
    public required int ItemsFoundSoFar { get; init; }
    public int PercentComplete => TotalAreas == 0 ? 0 : (int)(CompletedAreas * 100.0 / TotalAreas);
}

[SupportedOSPlatform("windows")]
public sealed class ResidualAnalysisEngine
{
    private readonly IZeroTraceLogger _logger;

    private static readonly HashSet<string> ProtectedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        @"C:\Windows", @"C:\Windows\System32", @"C:\Windows\SysWOW64",
        @"C:\Windows\WinSxS", @"C:\Windows\Fonts", @"C:\Windows\Installer",
        @"C:\Program Files\Windows Defender", @"C:\Program Files\Windows NT",
        @"C:\Program Files\WindowsApps", @"C:\ProgramData\Microsoft", @"C:\ProgramData\Windows",
    };

    private static readonly string[] ProtectedRegistryPrefixes =
    [
        @"HKEY_LOCAL_MACHINE\SYSTEM",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography",
        @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID",
    ];

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "The", "Inc", "Ltd", "LLC", "Corp", "Corporation", "Software", "Microsoft",
        "Windows", "System", "System32", "Update", "Setup", "Install", "Uninstall",
        "Common", "Shared", "Data", "Files", "Program", "App", "Application",
        "Tools", "Utility", "x86", "x64", "amd64", "arm64",
    };

    public event EventHandler<ScanProgressEventArgs>? ProgressChanged;

    public ResidualAnalysisEngine(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ResidualItem>> ScanAsync(
        InstalledProgram program, CancellationToken cancellationToken = default)
    {
        _logger.Info("Residual-Scan: " + program.DisplayName);
        var searchTerms = BuildSearchTerms(program);
        _logger.Info("  Suchbegriffe: [" + string.Join(", ", searchTerms) + "]");

        if (searchTerms.Count == 0)
        {
            _logger.Warning("  Keine Suchbegriffe.");
            return Array.Empty<ResidualItem>();
        }

        var scanAreas = BuildScanAreas(program);
        var allResults = new List<ResidualItem>();
        int completedAreas = 0;

        foreach (var area in scanAreas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReportProgress(area.Name, area.BasePath, scanAreas.Count, completedAreas, allResults.Count);

            try
            {
                var results = await Task.Run(() =>
                    area.AreaType == ScanAreaType.FileSystem
                        ? ScanFileSystem(area, searchTerms)
                        : ScanRegistry(area, searchTerms), cancellationToken);

                if (results.Count > 0)
                {
                    _logger.Info("  " + area.Name + ": " + results.Count + " Rest(e)");
                    allResults.AddRange(results);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Warning("  " + area.Name + ": " + ex.Message);
            }
            completedAreas++;
        }

        var sorted = allResults
            .OrderByDescending(r => r.ConfidenceScore)
            .ThenBy(r => r.ItemType)
            .ThenBy(r => r.FullPath, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var item in sorted)
            item.IsSelectedForDeletion = item.ConfidenceScore >= 0.5;

        _logger.Info("Scan fertig: " + sorted.Count + " Reste gefunden");
        return sorted.AsReadOnly();
    }

    private List<string> BuildSearchTerms(InstalledProgram program)
    {
        var terms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(program.DisplayName))
        {
            string cleanName = CleanDisplayName(program.DisplayName);
            if (cleanName.Length >= 3) terms.Add(cleanName);
            foreach (string word in cleanName.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 3 && !StopWords.Contains(word) && !word.All(c => char.IsDigit(c) || c == '.'))
                    terms.Add(word);
            }
        }
        if (!string.IsNullOrEmpty(program.Publisher))
        {
            foreach (string word in program.Publisher.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 3 && !StopWords.Contains(word)) terms.Add(word);
            }
        }
        if (!string.IsNullOrEmpty(program.MsiProductCode)) terms.Add(program.MsiProductCode);
        return terms.ToList();
    }

    private static string CleanDisplayName(string name)
    {
        string cleaned = name;
        int paren = cleaned.IndexOf('(');
        if (paren > 0) cleaned = cleaned.Substring(0, paren);
        cleaned = Regex.Replace(cleaned, @"\s+v?\d+[\.\d]*\s*$", "");
        return cleaned.Trim();
    }

    private List<ScanArea> BuildScanAreas(InstalledProgram program)
    {
        var areas = new List<ScanArea>();
        if (!string.IsNullOrEmpty(program.InstallLocation) && Directory.Exists(program.InstallLocation))
            areas.Add(new ScanArea("Installationsordner", program.InstallLocation, ScanAreaType.FileSystem, ResidualDetectionSource.InstallPath));

        string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string pfx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        areas.Add(new ScanArea("Program Files", pf, ScanAreaType.FileSystem, ResidualDetectionSource.NameMatch));
        if (pf != pfx86)
            areas.Add(new ScanArea("Program Files (x86)", pfx86, ScanAreaType.FileSystem, ResidualDetectionSource.NameMatch));
        areas.Add(new ScanArea("AppData Roaming", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ScanAreaType.FileSystem, ResidualDetectionSource.AppDataScan));
        areas.Add(new ScanArea("AppData Local", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ScanAreaType.FileSystem, ResidualDetectionSource.AppDataScan));
        areas.Add(new ScanArea("ProgramData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ScanAreaType.FileSystem, ResidualDetectionSource.ProgramDataScan));
        areas.Add(new ScanArea("Temp", Path.GetTempPath(), ScanAreaType.FileSystem, ResidualDetectionSource.TempScan));
        areas.Add(new ScanArea("Registry HKCU", @"HKEY_CURRENT_USER\SOFTWARE", ScanAreaType.Registry, ResidualDetectionSource.RegistryScan));
        areas.Add(new ScanArea("Registry HKLM", @"HKEY_LOCAL_MACHINE\SOFTWARE", ScanAreaType.Registry, ResidualDetectionSource.RegistryScan));
        return areas;
    }

    private List<ResidualItem> ScanFileSystem(ScanArea area, List<string> searchTerms)
    {
        var results = new List<ResidualItem>();
        if (!Directory.Exists(area.BasePath) || IsProtectedPath(area.BasePath)) return results;

        if (area.Source == ResidualDetectionSource.InstallPath)
        {
            try { results.Add(CreateDirItem(area.BasePath, 1.0, "Installationsordner existiert noch", area.Source)); } catch { }
            return results;
        }

        try
        {
            foreach (string dir in Directory.GetDirectories(area.BasePath))
            {
                if (IsProtectedPath(dir)) continue;
                string dirName = Path.GetFileName(dir);
                double score = CalculateScore(dirName, searchTerms);
                if (score >= 0.5)
                {
                    try { results.Add(CreateDirItem(dir, score, "Ordnername stimmt ueberein (Score: " + score.ToString("P0") + ")", area.Source)); } catch { }
                }
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (Exception ex) { _logger.Debug("Scan-Fehler: " + area.BasePath + " - " + ex.Message); }
        return results;
    }

    private static ResidualItem CreateDirItem(string path, double score, string reason, ResidualDetectionSource source)
    {
        long size = 0;
        try { size = new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => { try { return f.Length; } catch { return 0L; } }); } catch { }
        return new ResidualItem
        {
            Id = Guid.NewGuid().ToString("N"), FullPath = path, ItemType = ResidualItemType.Directory,
            SizeInBytes = size, ConfidenceScore = score, DetectionReason = reason,
            DetectionSource = source, IsSelectedForDeletion = score >= 0.5
        };
    }

    private List<ResidualItem> ScanRegistry(ScanArea area, List<string> searchTerms)
    {
        var results = new List<ResidualItem>();
        if (IsProtectedRegistry(area.BasePath)) return results;
        try
        {
            var parsed = ParseRegistryPath(area.BasePath);
            if (parsed.Hive is null) return results;
            using var baseKey = RegistryKey.OpenBaseKey(parsed.Hive.Value, RegistryView.Default);
            using var searchKey = baseKey.OpenSubKey(parsed.SubPath);
            if (searchKey is null) return results;

            foreach (string subKeyName in searchKey.GetSubKeyNames())
            {
                string fullPath = area.BasePath + "\\" + subKeyName;
                if (IsProtectedRegistry(fullPath)) continue;
                double score = CalculateScore(subKeyName, searchTerms);
                if (score >= 0.6)
                {
                    results.Add(new ResidualItem
                    {
                        Id = Guid.NewGuid().ToString("N"), FullPath = fullPath,
                        ItemType = ResidualItemType.RegistryKey, ConfidenceScore = score,
                        DetectionReason = "Registry-Key stimmt ueberein (Score: " + score.ToString("P0") + ")",
                        DetectionSource = ResidualDetectionSource.RegistryScan, IsSelectedForDeletion = score >= 0.6
                    });
                }
            }
        }
        catch (System.Security.SecurityException) { }
        catch (Exception ex) { _logger.Debug("Registry-Fehler: " + area.BasePath + " - " + ex.Message); }
        return results;
    }

    private static double CalculateScore(string name, List<string> searchTerms)
    {
        if (string.IsNullOrWhiteSpace(name) || searchTerms.Count == 0) return 0.0;
        double best = 0.0;
        foreach (string term in searchTerms)
        {
            if (term.Length < 3) continue;
            if (name.Equals(term, StringComparison.OrdinalIgnoreCase)) return 1.0;
            if (name.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                double ratio = (double)term.Length / name.Length;
                best = Math.Max(best, 0.5 + (ratio * 0.5));
            }
        }
        return best;
    }

    private static bool IsProtectedPath(string path) =>
        ProtectedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    private static bool IsProtectedRegistry(string path) =>
        ProtectedRegistryPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    private static (RegistryHive? Hive, string SubPath) ParseRegistryPath(string path)
    {
        if (path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.CurrentUser, path.Substring(18));
        if (path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.LocalMachine, path.Substring(19));
        return (null, path);
    }

    private void ReportProgress(string area, string path, int total, int completed, int found)
    {
        ProgressChanged?.Invoke(this, new ScanProgressEventArgs
        { CurrentAreaName = area, CurrentPath = path, TotalAreas = total, CompletedAreas = completed, ItemsFoundSoFar = found });
    }
}

internal sealed class ScanArea
{
    public string Name { get; }
    public string BasePath { get; }
    public ScanAreaType AreaType { get; }
    public ResidualDetectionSource Source { get; }
    public ScanArea(string name, string basePath, ScanAreaType areaType, ResidualDetectionSource source)
    { Name = name; BasePath = basePath; AreaType = areaType; Source = source; }
}

internal enum ScanAreaType { FileSystem, Registry }
'@
Set-Content -Path "$root\Analysis\ResidualAnalysisEngine.cs" -Value $file10 -Encoding UTF8
Write-Host "  [10/12] ResidualAnalysisEngine.cs" -ForegroundColor Green

# ============================================================
# DATEI 11: Vault/VaultService.cs
# ============================================================
$file11 = @'
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Vault;

[SupportedOSPlatform("windows")]
public sealed class VaultService
{
    private readonly string _vaultBasePath;
    private readonly IZeroTraceLogger _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public event EventHandler<VaultProgressEventArgs>? ProgressChanged;

    public VaultService(IZeroTraceLogger logger, string? customVaultPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultBasePath = customVaultPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZeroTrace", "Vault");
        Directory.CreateDirectory(_vaultBasePath);
    }

    public async Task<VaultBackup> CreateBackupAsync(InstalledProgram program,
        IReadOnlyList<ResidualItem> itemsToBackup, CancellationToken cancellationToken = default)
    {
        string backupId = Guid.NewGuid().ToString("N");
        string backupDir = Path.Combine(_vaultBasePath, backupId);
        string filesDir = Path.Combine(backupDir, "files");
        string registryDir = Path.Combine(backupDir, "registry");

        _logger.Info("Backup erstellen: " + backupId + " fuer " + program.DisplayName +
            " (" + itemsToBackup.Count + " Elemente)");

        Directory.CreateDirectory(filesDir);
        Directory.CreateDirectory(registryDir);

        var entries = new List<VaultEntry>();
        long totalSize = 0;

        for (int i = 0; i < itemsToBackup.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = itemsToBackup[i];
            ReportProgress("Sichere: " + item.DisplayName, itemsToBackup.Count, i);

            try
            {
                VaultEntry entry = item.ItemType switch
                {
                    ResidualItemType.File => await BackupFileAsync(item, filesDir),
                    ResidualItemType.Directory => await BackupDirectoryAsync(item, filesDir),
                    ResidualItemType.RegistryKey or ResidualItemType.RegistryValue =>
                        BackupRegistry(item, registryDir),
                    _ => CreateFailedEntry(item, "Unbekannter Typ")
                };
                entries.Add(entry);
                totalSize += entry.SizeInBytes ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Warning("  Backup fehlgeschlagen: " + item.FullPath + " - " + ex.Message);
                entries.Add(CreateFailedEntry(item, ex.Message));
            }
        }

        string integrityHash = ComputeIntegrityHash(entries);
        var backup = new VaultBackup
        {
            BackupId = backupId, CreatedAtUtc = DateTime.UtcNow,
            ProgramName = program.DisplayName, ProgramVersion = program.DisplayVersion,
            Entries = entries.AsReadOnly(), TotalSizeBytes = totalSize,
            IntegrityHash = integrityHash, IsEncrypted = false
        };

        string manifestPath = Path.Combine(backupDir, "manifest.json");
        string json = JsonSerializer.Serialize(backup, JsonOptions);
        await File.WriteAllTextAsync(manifestPath, json, cancellationToken);

        int ok = entries.Count(e => e.BackupSuccessful);
        _logger.Info("  Backup fertig: " + ok + "/" + entries.Count + " gesichert (" + backup.FormattedSize + ")");
        return backup;
    }

    private async Task<VaultEntry> BackupFileAsync(ResidualItem item, string filesDir)
    {
        if (!File.Exists(item.FullPath)) return CreateFailedEntry(item, "Datei existiert nicht");
        string relPath = SafeRelativePath(item.FullPath);
        string dest = Path.Combine(filesDir, relPath);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(item.FullPath, dest, overwrite: true);
        string hash = await ComputeFileHashAsync(item.FullPath);
        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath,
            BackupRelativePath = relPath, EntryType = VaultEntryType.File,
            SizeInBytes = new FileInfo(item.FullPath).Length, ContentHash = hash, BackupSuccessful = true
        };
    }

    private async Task<VaultEntry> BackupDirectoryAsync(ResidualItem item, string filesDir)
    {
        if (!Directory.Exists(item.FullPath)) return CreateFailedEntry(item, "Ordner existiert nicht");
        string relPath = SafeRelativePath(item.FullPath);
        string destDir = Path.Combine(filesDir, relPath);
        long totalSize = 0;
        foreach (string sourceFile in Directory.EnumerateFiles(item.FullPath, "*", SearchOption.AllDirectories))
        {
            try
            {
                string relFile = Path.GetRelativePath(item.FullPath, sourceFile);
                string destFile = Path.Combine(destDir, relFile);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(sourceFile, destFile, overwrite: true);
                totalSize += new FileInfo(sourceFile).Length;
            }
            catch { }
        }
        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath,
            BackupRelativePath = relPath, EntryType = VaultEntryType.Directory,
            SizeInBytes = totalSize, BackupSuccessful = true
        };
    }

    private VaultEntry BackupRegistry(ResidualItem item, string registryDir)
    {
        string safeFileName = item.FullPath.Replace('\\', '_').Replace(':', '_') + ".reg";
        string dest = Path.Combine(registryDir, safeFileName);
        using var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "reg.exe",
            Arguments = "export \"" + item.FullPath + "\" \"" + dest + "\" /y",
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        });
        process?.WaitForExit(15000);
        bool success = process?.ExitCode == 0 && File.Exists(dest);
        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath,
            BackupRelativePath = Path.Combine("registry", safeFileName),
            EntryType = item.ItemType == ResidualItemType.RegistryKey
                ? VaultEntryType.RegistryKey : VaultEntryType.RegistryValue,
            SizeInBytes = success ? new FileInfo(dest).Length : 0,
            BackupSuccessful = success,
            ErrorMessage = success ? null : "Registry-Export fehlgeschlagen"
        };
    }

    public async Task<IReadOnlyList<VaultBackup>> GetAllBackupsAsync(CancellationToken cancellationToken = default)
    {
        var backups = new List<VaultBackup>();
        if (!Directory.Exists(_vaultBasePath)) return backups.AsReadOnly();
        foreach (string dir in Directory.GetDirectories(_vaultBasePath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string manifest = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifest)) continue;
            try
            {
                string json = await File.ReadAllTextAsync(manifest, cancellationToken);
                var backup = JsonSerializer.Deserialize<VaultBackup>(json, JsonOptions);
                if (backup is not null) backups.Add(backup);
            }
            catch (Exception ex) { _logger.Warning("Manifest beschaedigt: " + dir + " - " + ex.Message); }
        }
        return backups.OrderByDescending(b => b.CreatedAtUtc).ToList().AsReadOnly();
    }

    public void DeleteBackup(string backupId)
    {
        string dir = Path.Combine(_vaultBasePath, backupId);
        if (Directory.Exists(dir)) { Directory.Delete(dir, recursive: true); _logger.Info("Backup geloescht: " + backupId); }
    }

    public string GetVaultPath() => _vaultBasePath;

    private static string SafeRelativePath(string path) =>
        path.Replace(':', '_').Replace("\\\\", "_").TrimStart('_');

    private static async Task<string> ComputeFileHashAsync(string path)
    {
        using var sha = SHA256.Create();
        await using var stream = File.OpenRead(path);
        byte[] hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }

    private static string ComputeIntegrityHash(List<VaultEntry> entries)
    {
        using var sha = SHA256.Create();
        var sb = new StringBuilder();
        foreach (var e in entries) { sb.Append(e.OriginalPath); sb.Append(':'); sb.Append(e.ContentHash ?? "none"); sb.Append('|'); }
        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash);
    }

    private static VaultEntry CreateFailedEntry(ResidualItem item, string error)
    {
        VaultEntryType entryType = item.ItemType switch
        {
            ResidualItemType.File => VaultEntryType.File,
            ResidualItemType.Directory => VaultEntryType.Directory,
            ResidualItemType.RegistryKey => VaultEntryType.RegistryKey,
            ResidualItemType.RegistryValue => VaultEntryType.RegistryValue,
            _ => VaultEntryType.File
        };
        return new VaultEntry
        {
            EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath,
            BackupRelativePath = "", EntryType = entryType,
            BackupSuccessful = false, ErrorMessage = error
        };
    }

    private void ReportProgress(string msg, int total, int current)
    {
        ProgressChanged?.Invoke(this, new VaultProgressEventArgs
        { Message = msg, TotalItems = total, CompletedItems = current });
    }
}

public sealed class VaultProgressEventArgs : EventArgs
{
    public required string Message { get; init; }
    public required int TotalItems { get; init; }
    public required int CompletedItems { get; init; }
    public int PercentComplete => TotalItems == 0 ? 0 : (int)(CompletedItems * 100.0 / TotalItems);
}
'@
Set-Content -Path "$root\Vault\VaultService.cs" -Value $file11 -Encoding UTF8
Write-Host "  [11/12] VaultService.cs" -ForegroundColor Green

# ============================================================
# DATEI 12: Cleanup/CleanupService.cs
# ============================================================
$file12 = @'
using System.Runtime.Versioning;
using Microsoft.Win32;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Cleanup;

public sealed class CleanupResult
{
    public required int TotalItems { get; init; }
    public required int SuccessfullyDeleted { get; init; }
    public required int Failed { get; init; }
    public required int Skipped { get; init; }
    public required long FreedBytes { get; init; }
    public required TimeSpan Duration { get; init; }
    public required IReadOnlyList<CleanupItemResult> Details { get; init; }
    public required string BackupId { get; init; }

    public string FormattedFreedSize
    {
        get
        {
            if (FreedBytes < 1024) return FreedBytes + " B";
            if (FreedBytes < 1024 * 1024) return (FreedBytes / 1024.0).ToString("F1") + " KB";
            return (FreedBytes / (1024.0 * 1024)).ToString("F1") + " MB";
        }
    }
}

public sealed class CleanupItemResult
{
    public required string Path { get; init; }
    public required ResidualItemType ItemType { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public bool ScheduledForReboot { get; init; }
}

[SupportedOSPlatform("windows")]
public sealed class CleanupService
{
    private readonly IZeroTraceLogger _logger;
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);
    public event EventHandler<CleanupProgressEventArgs>? ProgressChanged;

    public CleanupService(IZeroTraceLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CleanupResult> CleanAsync(IReadOnlyList<ResidualItem> items,
        string backupId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(backupId))
            throw new InvalidOperationException("SICHERHEITSSPERRE: Loeschung ohne Backup-ID verboten!");

        var selected = items.Where(i => i.IsSelectedForDeletion).ToList();
        _logger.Info("Bereinigung: " + selected.Count + " Elemente, Backup-ID: " + backupId);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<CleanupItemResult>();
        int ok = 0, fail = 0, skip = 0;
        long freed = 0;

        var ordered = selected.OrderBy(i => i.ItemType switch
        {
            ResidualItemType.File => 0, ResidualItemType.RegistryValue => 1,
            ResidualItemType.RegistryKey => 2, ResidualItemType.Directory => 3, _ => 4
        }).ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var item = ordered[i];
            ReportProgress(item.FullPath, ordered.Count, i);

            try
            {
                var result = await DeleteItemAsync(item);
                details.Add(result);
                if (result.Success) { ok++; freed += item.SizeInBytes ?? 0; }
                else { skip++; }
            }
            catch (Exception ex)
            {
                fail++;
                _logger.Warning("  Fehler: " + item.FullPath + " - " + ex.Message);
                details.Add(new CleanupItemResult
                { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = ex.Message });
            }
        }

        sw.Stop();
        var result2 = new CleanupResult
        {
            TotalItems = ordered.Count, SuccessfullyDeleted = ok, Failed = fail,
            Skipped = skip, FreedBytes = freed, Duration = sw.Elapsed,
            Details = details.AsReadOnly(), BackupId = backupId
        };
        _logger.Info("  Ergebnis: " + ok + " OK, " + fail + " Fehler, " + skip +
            " uebersprungen | " + result2.FormattedFreedSize + " frei");
        return result2;
    }

    private async Task<CleanupItemResult> DeleteItemAsync(ResidualItem item) => item.ItemType switch
    {
        ResidualItemType.File => await DeleteFileAsync(item),
        ResidualItemType.Directory => DeleteDirectory(item),
        ResidualItemType.RegistryKey => DeleteRegistryKey(item),
        ResidualItemType.RegistryValue => DeleteRegistryValue(item),
        _ => new CleanupItemResult { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = "Unbekannter Typ" }
    };

    private async Task<CleanupItemResult> DeleteFileAsync(ResidualItem item)
    {
        if (!File.Exists(item.FullPath))
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Existiert nicht mehr" };

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var attrs = File.GetAttributes(item.FullPath);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(item.FullPath, attrs & ~FileAttributes.ReadOnly);
                File.Delete(item.FullPath);
                return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = true };
            }
            catch (IOException) when (attempt < MaxRetries) { await Task.Delay(RetryDelay); }
            catch (UnauthorizedAccessException)
            {
                bool scheduled = ScheduleRebootDelete(item.FullPath);
                return new CleanupItemResult
                {
                    Path = item.FullPath, ItemType = ResidualItemType.File, Success = false,
                    ErrorMessage = scheduled ? "Wird nach Neustart geloescht" : "Zugriff verweigert",
                    ScheduledForReboot = scheduled
                };
            }
        }
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Nach Wiederholung nicht loeschbar" };
    }

    private CleanupItemResult DeleteDirectory(ResidualItem item)
    {
        if (!Directory.Exists(item.FullPath))
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = false, ErrorMessage = "Existiert nicht mehr" };
        Directory.Delete(item.FullPath, recursive: true);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = true };
    }

    private CleanupItemResult DeleteRegistryKey(ResidualItem item)
    {
        var parsed = ParseRegistryPath(item.FullPath);
        if (parsed.Hive is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Pfad nicht erkannt" };

        int lastSlash = parsed.SubPath.LastIndexOf('\\');
        if (lastSlash < 0)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Ungueltiger Pfad" };

        string parentPath = parsed.SubPath.Substring(0, lastSlash);
        string keyName = parsed.SubPath.Substring(lastSlash + 1);

        using var baseKey = RegistryKey.OpenBaseKey(parsed.Hive.Value, RegistryView.Default);
        using var parentKey = baseKey.OpenSubKey(parentPath, writable: true);
        if (parentKey is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Parent-Key nicht gefunden" };

        parentKey.DeleteSubKeyTree(keyName, throwOnMissingSubKey: false);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = true };
    }

    private CleanupItemResult DeleteRegistryValue(ResidualItem item)
    {
        int sep = item.FullPath.LastIndexOf("::");
        if (sep < 0)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Ungueltiges Format" };

        string keyPath = item.FullPath.Substring(0, sep);
        string valueName = item.FullPath.Substring(sep + 2);

        var parsed = ParseRegistryPath(keyPath);
        if (parsed.Hive is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Pfad nicht erkannt" };

        using var baseKey = RegistryKey.OpenBaseKey(parsed.Hive.Value, RegistryView.Default);
        using var key = baseKey.OpenSubKey(parsed.SubPath, writable: true);
        if (key is null)
            return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Key nicht gefunden" };

        key.DeleteValue(valueName, throwOnMissingValue: false);
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = true };
    }

    private bool ScheduleRebootDelete(string path)
    {
        try { return NativeMethods.MoveFileExW(path, null, NativeMethods.MOVEFILE_DELAY_UNTIL_REBOOT); }
        catch { return false; }
    }

    private static (RegistryHive? Hive, string SubPath) ParseRegistryPath(string path)
    {
        if (path.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.CurrentUser, path.Substring(18));
        if (path.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase))
            return (RegistryHive.LocalMachine, path.Substring(19));
        return (null, path);
    }

    private void ReportProgress(string path, int total, int current)
    {
        ProgressChanged?.Invoke(this, new CleanupProgressEventArgs
        { CurrentPath = path, TotalItems = total, CompletedItems = current });
    }
}

public sealed class CleanupProgressEventArgs : EventArgs
{
    public required string CurrentPath { get; init; }
    public required int TotalItems { get; init; }
    public required int CompletedItems { get; init; }
    public int PercentComplete => TotalItems == 0 ? 0 : (int)(CompletedItems * 100.0 / TotalItems);
}

internal static partial class NativeMethods
{
    internal const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;

    [System.Runtime.InteropServices.LibraryImport("kernel32.dll",
        SetLastError = true,
        StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial bool MoveFileExW(
        string lpExistingFileName, string? lpNewFileName, int dwFlags);
}
'@
Set-Content -Path "$root\Cleanup\CleanupService.cs" -Value $file12 -Encoding UTF8
Write-Host "  [12/12] CleanupService.cs" -ForegroundColor Green

Write-Host "`n=== Alle 12 Dateien geschrieben! ===" -ForegroundColor Cyan
Write-Host "Jetzt 'dotnet build' ausfuehren..." -ForegroundColor Yellow