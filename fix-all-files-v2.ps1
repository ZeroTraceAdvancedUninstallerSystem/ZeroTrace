# ZeroTrace - Alle 12 Dateien reparieren (v2 - alle Fehler behoben)
# Ausfuehren mit: powershell -ExecutionPolicy Bypass -File fix-all-files-v2.ps1

$root = "C:\Projekte\ZeroTrace\src\ZeroTrace.Core"

Write-Host "=== ZeroTrace Dateien werden repariert (v2) ===" -ForegroundColor Cyan

# --- DATEI 1: Models/InstalledProgram.cs ---
$code = 'namespace ZeroTrace.Core.Models;

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
            long val = EstimatedSizeBytes.Value;
            if (val < 1024) return val + " B";
            if (val < 1024 * 1024) return (val / 1024.0).ToString("F1") + " KB";
            if (val < 1024 * 1024 * 1024) return (val / (1024.0 * 1024)).ToString("F1") + " MB";
            return (val / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }
    }

    public string FormattedInstallDate
    {
        get { return InstallDate.HasValue ? InstallDate.Value.ToString("dd.MM.yyyy") : "-"; }
    }

    public bool IsMsiInstallation => !string.IsNullOrEmpty(MsiProductCode);

    public bool CanBeUninstalled =>
        !string.IsNullOrEmpty(UninstallString) ||
        !string.IsNullOrEmpty(QuietUninstallString) ||
        !string.IsNullOrEmpty(MsiProductCode);

    public override string ToString()
    {
        return DisplayName + " " + DisplayVersion + " (" + Publisher + ")";
    }
}

public enum ProgramSource
{
    RegistryLocalMachine,
    RegistryCurrentUser,
    MsiDatabase,
    ManualDiscovery
}'
[System.IO.File]::WriteAllText("$root\Models\InstalledProgram.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [01/12] InstalledProgram.cs" -ForegroundColor Green

# --- DATEI 2: Models/ResidualItem.cs ---
$code = 'namespace ZeroTrace.Core.Models;

public sealed class ResidualItem
{
    public required string Id { get; init; }
    public required string FullPath { get; init; }

    public string DisplayName
    {
        get { return System.IO.Path.GetFileName(FullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar)) ?? FullPath; }
    }

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
            long val = SizeInBytes.Value;
            if (val == 0) return "Leer";
            if (val < 1024) return val + " B";
            if (val < 1024 * 1024) return (val / 1024.0).ToString("F1") + " KB";
            return (val / (1024.0 * 1024)).ToString("F1") + " MB";
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
}'
[System.IO.File]::WriteAllText("$root\Models\ResidualItem.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [02/12] ResidualItem.cs" -ForegroundColor Green

# --- DATEI 3: Models/VaultBackup.cs ---
$code = 'namespace ZeroTrace.Core.Models;

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

    public string FormattedDate
    {
        get { return CreatedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss"); }
    }
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

public enum VaultEntryType { File, Directory, RegistryKey, RegistryValue }

public enum VaultBackupStatus { Active, PartiallyRestored, FullyRestored, Expired, Corrupted }'
[System.IO.File]::WriteAllText("$root\Models\VaultBackup.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [03/12] VaultBackup.cs" -ForegroundColor Green

# --- DATEI 4: Logging/IZeroTraceLogger.cs ---
$code = 'namespace ZeroTrace.Core.Logging;

public interface IZeroTraceLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
}

public enum LogLevel { Debug = 0, Info = 1, Warning = 2, Error = 3 }'
[System.IO.File]::WriteAllText("$root\Logging\IZeroTraceLogger.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [04/12] IZeroTraceLogger.cs" -ForegroundColor Green

# --- DATEI 5: Logging/FileLogger.cs ---
$code = 'using System.Collections.Concurrent;
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
            try { await Task.Delay(500, _cts.Token); await FlushQueueToFileAsync(); }
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
            while (_messageQueue.TryDequeue(out string? message)) sb.AppendLine(message);
            if (sb.Length > 0) await File.AppendAllTextAsync(_logFilePath, sb.ToString());
        }
        finally { _writeLock.Release(); }
    }

    public string GetCurrentLogFilePath() => _logFilePath;

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        try { await _backgroundFlushTask; } catch (OperationCanceledException) { }
        _cts.Dispose();
        _writeLock.Dispose();
    }
}'
[System.IO.File]::WriteAllText("$root\Logging\FileLogger.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [05/12] FileLogger.cs" -ForegroundColor Green

# --- DATEI 6: Discovery/IDiscoveryProvider.cs ---
$code = 'using ZeroTrace.Core.Models;

namespace ZeroTrace.Core.Discovery;

public interface IDiscoveryProvider
{
    Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(CancellationToken cancellationToken = default);
    string ProviderName { get; }
}'
[System.IO.File]::WriteAllText("$root\Discovery\IDiscoveryProvider.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [06/12] IDiscoveryProvider.cs" -ForegroundColor Green

# --- DATEI 7: Discovery/RegistryDiscoveryProvider.cs ---
$code = 'using System.Globalization;
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
        new("HKLM (64-Bit)", RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryView.Registry64, ProgramSource.RegistryLocalMachine),
        new("HKLM (32-Bit)", RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryView.Registry32, ProgramSource.RegistryLocalMachine),
        new("HKCU", RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", RegistryView.Registry64, ProgramSource.RegistryCurrentUser),
    ];

    public RegistryDiscoveryProvider(IZeroTraceLogger logger)
    { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public async Task<IReadOnlyList<InstalledProgram>> DiscoverAsync(CancellationToken cancellationToken = default)
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
                        if (seenIds.Add(program.Id)) programs.Add(program);
                    _logger.Debug("  " + source.Description + ": " + found.Count + " Programm(e)");
                }
                catch (System.Security.SecurityException ex) { _logger.Warning("  " + source.Description + ": Zugriff verweigert - " + ex.Message); }
                catch (Exception ex) { _logger.Warning("  " + source.Description + ": Fehler - " + ex.Message); }
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
                using var pk = uninstallKey.OpenSubKey(subKeyName);
                if (pk is null) continue;
                string? displayName = pk.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;
                results.Add(new InstalledProgram
                {
                    Id = (source.Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU") + "_" + (source.View == RegistryView.Registry64 ? "64" : "32") + "_" + subKeyName,
                    DisplayName = displayName.Trim(),
                    DisplayVersion = SafeGet(pk, "DisplayVersion"),
                    Publisher = SafeGet(pk, "Publisher"),
                    InstallLocation = NormPath(SafeGet(pk, "InstallLocation")),
                    InstallDate = ParseDate(SafeGet(pk, "InstallDate")),
                    EstimatedSizeBytes = ParseSize(pk),
                    UninstallString = SafeGet(pk, "UninstallString"),
                    QuietUninstallString = SafeGet(pk, "QuietUninstallString"),
                    Source = source.ProgramSource,
                    IsSystemComponent = IsSystemComp(pk),
                    RegistryKeyPath = pk.Name,
                    MsiProductCode = Guid.TryParse(subKeyName, out _) ? subKeyName : null,
                    IconPath = SafeGet(pk, "DisplayIcon"),
                });
            }
            catch { }
        }
        return results;
    }

    private static bool IsSystemComp(RegistryKey key)
    {
        if (key.GetValue("SystemComponent") is int sc && sc == 1) return true;
        string? rt = key.GetValue("ReleaseType") as string;
        if (rt is "Update" or "Hotfix" or "Security Update") return true;
        if (key.GetValue("ParentKeyName") is string p && !string.IsNullOrEmpty(p)) return true;
        return false;
    }
    private static string? SafeGet(RegistryKey k, string n) { try { return k.GetValue(n)?.ToString()?.Trim(); } catch { return null; } }
    private static DateTime? ParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParseExact(s.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
    private static long? ParseSize(RegistryKey k) { var v = k.GetValue("EstimatedSize"); return v is int i ? (long)i * 1024 : v is long l ? l * 1024 : null; }
    private static string? NormPath(string? p) { if (string.IsNullOrWhiteSpace(p)) return null; return p.Trim().Trim('\'', '"').TrimEnd('\\'); }
    private sealed record RegistrySource(string Description, RegistryHive Hive, string Path, RegistryView View, ProgramSource ProgramSource);
}'
[System.IO.File]::WriteAllText("$root\Discovery\RegistryDiscoveryProvider.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [07/12] RegistryDiscoveryProvider.cs" -ForegroundColor Green

# --- DATEI 8: Discovery/DiscoveryService.cs ---
$code = 'using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Discovery;

public sealed class DiscoveryService
{
    private readonly IReadOnlyList<IDiscoveryProvider> _providers;
    private readonly IZeroTraceLogger _logger;
    public event EventHandler<DiscoveryProgressEventArgs>? ProgressChanged;

    public DiscoveryService(IEnumerable<IDiscoveryProvider> providers, IZeroTraceLogger logger)
    {
        _providers = providers?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(providers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<InstalledProgram>> GetAllProgramsAsync(
        bool includeSystemComponents = false, CancellationToken cancellationToken = default)
    {
        _logger.Info("Programmsuche gestartet");
        var allPrograms = new List<InstalledProgram>();
        int idx = 0;
        foreach (var provider in _providers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged?.Invoke(this, new DiscoveryProgressEventArgs { CurrentProvider = provider.ProviderName, ProviderIndex = idx, TotalProviders = _providers.Count });
            try
            {
                var found = await provider.DiscoverAsync(cancellationToken);
                allPrograms.AddRange(found);
                _logger.Info("  Provider: " + provider.ProviderName + ": " + found.Count + " Programme.");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { _logger.Error("  Provider " + provider.ProviderName + " fehlgeschlagen.", ex); }
            idx++;
        }
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<InstalledProgram>();
        foreach (var p in allPrograms)
        {
            string key = p.DisplayName + "|" + (p.DisplayVersion ?? "?");
            if (seen.Add(key)) unique.Add(p);
        }
        var filtered = includeSystemComponents ? unique : unique.Where(p => !p.IsSystemComponent).ToList();
        var sorted = filtered.OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList().AsReadOnly();
        _logger.Info("Programmsuche abgeschlossen: " + sorted.Count + " Programme");
        return sorted;
    }
}

public sealed class DiscoveryProgressEventArgs : EventArgs
{
    public required string CurrentProvider { get; init; }
    public required int ProviderIndex { get; init; }
    public required int TotalProviders { get; init; }
}'
[System.IO.File]::WriteAllText("$root\Discovery\DiscoveryService.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [08/12] DiscoveryService.cs" -ForegroundColor Green

# --- DATEI 9: Uninstall/UninstallService.cs ---
$code = 'using System.Diagnostics;
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

public enum UninstallMethod { MsiStandard, MsiQuiet, ExecutableNormal, ExecutableQuiet, NotAvailable }

[SupportedOSPlatform("windows")]
public sealed class UninstallService
{
    private readonly IZeroTraceLogger _logger;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);
    public event EventHandler<UninstallStatusEventArgs>? StatusChanged;

    public UninstallService(IZeroTraceLogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public async Task<UninstallResult> UninstallAsync(InstalledProgram program, bool preferQuietMode = false, CancellationToken cancellationToken = default)
    {
        _logger.Info("Deinstallation gestartet: " + program.DisplayName);
        ReportStatus("Bereite Deinstallation vor: " + program.DisplayName + "...");
        var sw = Stopwatch.StartNew();
        try
        {
            string? cmd = null; UninstallMethod method = UninstallMethod.NotAvailable;
            if (!string.IsNullOrEmpty(program.MsiProductCode))
            { cmd = preferQuietMode ? "msiexec.exe /x " + program.MsiProductCode + " /qn /norestart" : "msiexec.exe /x " + program.MsiProductCode + " /norestart"; method = preferQuietMode ? UninstallMethod.MsiQuiet : UninstallMethod.MsiStandard; }
            else if (preferQuietMode && !string.IsNullOrEmpty(program.QuietUninstallString))
            { cmd = program.QuietUninstallString; method = UninstallMethod.ExecutableQuiet; }
            else if (!string.IsNullOrEmpty(program.UninstallString))
            { cmd = program.UninstallString; method = UninstallMethod.ExecutableNormal; }

            if (cmd is null) return new UninstallResult { Success = false, ExitCode = -1, Duration = sw.Elapsed, MethodUsed = UninstallMethod.NotAvailable, ErrorMessage = "Kein Deinstallationsbefehl." };

            var parsed = ParseCmd(cmd);
            ReportStatus("Starte Deinstaller...");
            var si = new ProcessStartInfo { FileName = parsed.File, Arguments = parsed.Args, UseShellExecute = true, Verb = "runas" };
            using var proc = new Process { StartInfo = si };
            if (!proc.Start()) return new UninstallResult { Success = false, ExitCode = -1, Duration = sw.Elapsed, MethodUsed = method, ErrorMessage = "Start fehlgeschlagen." };

            using var tCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tCts.CancelAfter(DefaultTimeout);
            try { await proc.WaitForExitAsync(tCts.Token); }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            { try { if (!proc.HasExited) proc.Kill(true); } catch { } return new UninstallResult { Success = false, ExitCode = -1, Duration = sw.Elapsed, MethodUsed = method, ErrorMessage = "Timeout." }; }

            sw.Stop();
            int ec = proc.ExitCode;
            bool ok = ec is 0 or 1605 or 1614 or 1641 or 3010;
            return new UninstallResult { Success = ok, ExitCode = ec, Duration = sw.Elapsed, MethodUsed = method, RequiresReboot = ec is 1641 or 3010, ErrorMessage = ok ? null : "Exit-Code " + ec };
        }
        catch (OperationCanceledException) { throw; }
        catch (System.ComponentModel.Win32Exception ex) { return new UninstallResult { Success = false, ExitCode = ex.NativeErrorCode, Duration = sw.Elapsed, MethodUsed = UninstallMethod.NotAvailable, ErrorMessage = ex.NativeErrorCode == 1223 ? "Admin abgelehnt." : ex.Message }; }
        catch (Exception ex) { return new UninstallResult { Success = false, ExitCode = -1, Duration = sw.Elapsed, MethodUsed = UninstallMethod.NotAvailable, ErrorMessage = ex.Message }; }
    }

    private static (string File, string Args) ParseCmd(string c)
    {
        c = c.Trim();
        if (c.StartsWith('"')) { int q = c.IndexOf('"', 1); if (q > 0) return (c.Substring(1, q - 1), q + 1 < c.Length ? c.Substring(q + 1).Trim() : ""); }
        if (c.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase)) { int s = c.IndexOf(' '); return s > 0 ? ("msiexec.exe", c.Substring(s + 1).Trim()) : ("msiexec.exe", ""); }
        int ei = c.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
        if (ei > 0) { int sp = ei + 4; return (c.Substring(0, sp).Trim(), sp < c.Length ? c.Substring(sp).Trim() : ""); }
        int si2 = c.IndexOf(' '); return si2 > 0 ? (c.Substring(0, si2), c.Substring(si2 + 1)) : (c, "");
    }

    private void ReportStatus(string m) { StatusChanged?.Invoke(this, new UninstallStatusEventArgs(m)); }
}

public sealed class UninstallStatusEventArgs : EventArgs
{
    public string Message { get; }
    public UninstallStatusEventArgs(string message) { Message = message; }
}'
[System.IO.File]::WriteAllText("$root\Uninstall\UninstallService.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [09/12] UninstallService.cs" -ForegroundColor Green

# --- DATEI 10: Analysis/ResidualAnalysisEngine.cs ---
$code = 'using System.Runtime.Versioning;
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
    { @"C:\Windows", @"C:\Windows\System32", @"C:\Windows\SysWOW64", @"C:\Windows\WinSxS", @"C:\Windows\Fonts", @"C:\Windows\Installer", @"C:\Program Files\Windows Defender", @"C:\Program Files\Windows NT", @"C:\Program Files\WindowsApps", @"C:\ProgramData\Microsoft", @"C:\ProgramData\Windows" };
    private static readonly string[] ProtRegPfx = [ @"HKEY_LOCAL_MACHINE\SYSTEM", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT", @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography", @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\CLSID" ];
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase) { "The","Inc","Ltd","LLC","Corp","Corporation","Software","Microsoft","Windows","System","System32","Update","Setup","Install","Uninstall","Common","Shared","Data","Files","Program","App","Application","Tools","Utility","x86","x64","amd64","arm64" };
    public event EventHandler<ScanProgressEventArgs>? ProgressChanged;

    public ResidualAnalysisEngine(IZeroTraceLogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public async Task<IReadOnlyList<ResidualItem>> ScanAsync(InstalledProgram program, CancellationToken ct = default)
    {
        _logger.Info("Residual-Scan: " + program.DisplayName);
        var terms = BuildTerms(program);
        _logger.Info("  Suchbegriffe: [" + string.Join(", ", terms) + "]");
        if (terms.Count == 0) { _logger.Warning("  Keine Suchbegriffe."); return Array.Empty<ResidualItem>(); }

        var areas = BuildAreas(program);
        var all = new List<ResidualItem>(); int done = 0;
        foreach (var a in areas)
        {
            ct.ThrowIfCancellationRequested();
            ProgressChanged?.Invoke(this, new ScanProgressEventArgs { CurrentAreaName = a.Name, CurrentPath = a.Base, TotalAreas = areas.Count, CompletedAreas = done, ItemsFoundSoFar = all.Count });
            try
            {
                var r = await Task.Run(() => a.IsFs ? ScanFs(a, terms) : ScanReg(a, terms), ct);
                if (r.Count > 0) { _logger.Info("  " + a.Name + ": " + r.Count + " Rest(e)"); all.AddRange(r); }
            }
            catch (Exception ex) when (ex is not OperationCanceledException) { _logger.Warning("  " + a.Name + ": " + ex.Message); }
            done++;
        }
        var sorted = all.OrderByDescending(r => r.ConfidenceScore).ThenBy(r => r.ItemType).ThenBy(r => r.FullPath, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var i in sorted) i.IsSelectedForDeletion = i.ConfidenceScore >= 0.5;
        _logger.Info("Scan fertig: " + sorted.Count + " Reste");
        return sorted.AsReadOnly();
    }

    private List<string> BuildTerms(InstalledProgram p)
    {
        var t = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(p.DisplayName))
        {
            string cn = Regex.Replace(p.DisplayName, @"\(.*?\)", "");
            cn = Regex.Replace(cn, @"\s+v?\d+[\.\d]*\s*$", "").Trim();
            if (cn.Length >= 3) t.Add(cn);
            foreach (var w in cn.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries))
                if (w.Length >= 3 && !StopWords.Contains(w) && !w.All(c => char.IsDigit(c) || c == '.')) t.Add(w);
        }
        if (!string.IsNullOrEmpty(p.Publisher))
            foreach (var w in p.Publisher.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                if (w.Length >= 3 && !StopWords.Contains(w)) t.Add(w);
        if (!string.IsNullOrEmpty(p.MsiProductCode)) t.Add(p.MsiProductCode);
        return t.ToList();
    }

    private List<Area> BuildAreas(InstalledProgram p)
    {
        var a = new List<Area>();
        if (!string.IsNullOrEmpty(p.InstallLocation) && Directory.Exists(p.InstallLocation))
            a.Add(new Area("Installationsordner", p.InstallLocation, true, ResidualDetectionSource.InstallPath));
        string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string px = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        a.Add(new Area("Program Files", pf, true, ResidualDetectionSource.NameMatch));
        if (pf != px) a.Add(new Area("Program Files (x86)", px, true, ResidualDetectionSource.NameMatch));
        a.Add(new Area("AppData Roaming", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), true, ResidualDetectionSource.AppDataScan));
        a.Add(new Area("AppData Local", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), true, ResidualDetectionSource.AppDataScan));
        a.Add(new Area("ProgramData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), true, ResidualDetectionSource.ProgramDataScan));
        a.Add(new Area("Temp", Path.GetTempPath(), true, ResidualDetectionSource.TempScan));
        a.Add(new Area("Registry HKCU", @"HKEY_CURRENT_USER\SOFTWARE", false, ResidualDetectionSource.RegistryScan));
        a.Add(new Area("Registry HKLM", @"HKEY_LOCAL_MACHINE\SOFTWARE", false, ResidualDetectionSource.RegistryScan));
        return a;
    }

    private List<ResidualItem> ScanFs(Area area, List<string> terms)
    {
        var r = new List<ResidualItem>();
        if (!Directory.Exists(area.Base) || IsProt(area.Base)) return r;
        if (area.Source == ResidualDetectionSource.InstallPath)
        { try { r.Add(MkDir(area.Base, 1.0, "Installationsordner existiert noch", area.Source)); } catch { } return r; }
        try
        {
            foreach (string d in Directory.GetDirectories(area.Base))
            {
                if (IsProt(d)) continue;
                string n = Path.GetFileName(d); double s = Score(n, terms);
                if (s >= 0.5) { try { r.Add(MkDir(d, s, "Ordnername stimmt ueberein (Score: " + s.ToString("P0") + ")", area.Source)); } catch { } }
            }
        }
        catch (UnauthorizedAccessException) { } catch (Exception ex) { _logger.Debug("Scan-Fehler: " + area.Base + " - " + ex.Message); }
        return r;
    }

    private static ResidualItem MkDir(string path, double score, string reason, ResidualDetectionSource src)
    {
        long sz = 0; try { sz = new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => { try { return f.Length; } catch { return 0L; } }); } catch { }
        return new ResidualItem { Id = Guid.NewGuid().ToString("N"), FullPath = path, ItemType = ResidualItemType.Directory, SizeInBytes = sz, ConfidenceScore = score, DetectionReason = reason, DetectionSource = src, IsSelectedForDeletion = score >= 0.5 };
    }

    private List<ResidualItem> ScanReg(Area area, List<string> terms)
    {
        var r = new List<ResidualItem>();
        if (IsProtReg(area.Base)) return r;
        try
        {
            RegistryHive? h = null; string sub = area.Base;
            if (area.Base.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase)) { h = RegistryHive.CurrentUser; sub = area.Base.Substring(18); }
            else if (area.Base.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase)) { h = RegistryHive.LocalMachine; sub = area.Base.Substring(19); }
            if (h is null) return r;
            using var bk = RegistryKey.OpenBaseKey(h.Value, RegistryView.Default);
            using var sk = bk.OpenSubKey(sub); if (sk is null) return r;
            foreach (string kn in sk.GetSubKeyNames())
            {
                string fp = area.Base + "\\" + kn; if (IsProtReg(fp)) continue;
                double s = Score(kn, terms);
                if (s >= 0.6) r.Add(new ResidualItem { Id = Guid.NewGuid().ToString("N"), FullPath = fp, ItemType = ResidualItemType.RegistryKey, ConfidenceScore = s, DetectionReason = "Registry-Key stimmt ueberein (Score: " + s.ToString("P0") + ")", DetectionSource = ResidualDetectionSource.RegistryScan, IsSelectedForDeletion = s >= 0.6 });
            }
        }
        catch (System.Security.SecurityException) { } catch (Exception ex) { _logger.Debug("Registry-Fehler: " + area.Base + " - " + ex.Message); }
        return r;
    }

    private static double Score(string name, List<string> terms)
    {
        if (string.IsNullOrWhiteSpace(name) || terms.Count == 0) return 0.0;
        double best = 0.0;
        foreach (var t in terms)
        {
            if (t.Length < 3) continue;
            if (name.Equals(t, StringComparison.OrdinalIgnoreCase)) return 1.0;
            if (name.Contains(t, StringComparison.OrdinalIgnoreCase)) { double ratio = (double)t.Length / name.Length; best = Math.Max(best, 0.5 + ratio * 0.5); }
        }
        return best;
    }

    private static bool IsProt(string p) => ProtectedPaths.Any(x => p.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    private static bool IsProtReg(string p) => ProtRegPfx.Any(x => p.StartsWith(x, StringComparison.OrdinalIgnoreCase));

    private sealed record Area(string Name, string Base, bool IsFs, ResidualDetectionSource Source);
}'
[System.IO.File]::WriteAllText("$root\Analysis\ResidualAnalysisEngine.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [10/12] ResidualAnalysisEngine.cs" -ForegroundColor Green

# --- DATEI 11: Vault/VaultService.cs ---
$code = 'using System.Runtime.Versioning;
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
    private static readonly JsonSerializerOptions JOpt = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public event EventHandler<VaultProgressEventArgs>? ProgressChanged;

    public VaultService(IZeroTraceLogger logger, string? customVaultPath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vaultBasePath = customVaultPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZeroTrace", "Vault");
        Directory.CreateDirectory(_vaultBasePath);
    }

    public async Task<VaultBackup> CreateBackupAsync(InstalledProgram program, IReadOnlyList<ResidualItem> items, CancellationToken ct = default)
    {
        string bid = Guid.NewGuid().ToString("N");
        string bd = Path.Combine(_vaultBasePath, bid), fd = Path.Combine(bd, "files"), rd = Path.Combine(bd, "registry");
        _logger.Info("Backup erstellen: " + bid + " fuer " + program.DisplayName + " (" + items.Count + " Elemente)");
        Directory.CreateDirectory(fd); Directory.CreateDirectory(rd);
        var entries = new List<VaultEntry>(); long totalSize = 0;
        for (int i = 0; i < items.Count; i++)
        {
            ct.ThrowIfCancellationRequested(); var item = items[i];
            ProgressChanged?.Invoke(this, new VaultProgressEventArgs { Message = "Sichere: " + item.DisplayName, TotalItems = items.Count, CompletedItems = i });
            try
            {
                VaultEntry entry = item.ItemType switch
                {
                    ResidualItemType.File => await BkFile(item, fd),
                    ResidualItemType.Directory => await BkDir(item, fd),
                    ResidualItemType.RegistryKey or ResidualItemType.RegistryValue => BkReg(item, rd),
                    _ => FailEntry(item, "Unbekannter Typ")
                };
                entries.Add(entry); totalSize += entry.SizeInBytes ?? 0;
            }
            catch (Exception ex) { _logger.Warning("  Backup fehlgeschlagen: " + item.FullPath + " - " + ex.Message); entries.Add(FailEntry(item, ex.Message)); }
        }
        string hash; using (var sha = SHA256.Create()) { var sb2 = new StringBuilder(); foreach (var e in entries) { sb2.Append(e.OriginalPath); sb2.Append(':'); sb2.Append(e.ContentHash ?? "none"); sb2.Append('|'); } hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(sb2.ToString()))); }
        var backup = new VaultBackup { BackupId = bid, CreatedAtUtc = DateTime.UtcNow, ProgramName = program.DisplayName, ProgramVersion = program.DisplayVersion, Entries = entries.AsReadOnly(), TotalSizeBytes = totalSize, IntegrityHash = hash, IsEncrypted = false };
        await File.WriteAllTextAsync(Path.Combine(bd, "manifest.json"), JsonSerializer.Serialize(backup, JOpt), ct);
        _logger.Info("  Backup fertig: " + entries.Count(e => e.BackupSuccessful) + "/" + entries.Count + " gesichert (" + backup.FormattedSize + ")");
        return backup;
    }

    private async Task<VaultEntry> BkFile(ResidualItem item, string fd)
    {
        if (!File.Exists(item.FullPath)) return FailEntry(item, "Datei existiert nicht");
        string rp = item.FullPath.Replace(':', '_').Replace("\\\\", "_").TrimStart('_');
        string dest = Path.Combine(fd, rp); Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(item.FullPath, dest, true);
        string h; using (var sha = SHA256.Create()) { await using var s = File.OpenRead(item.FullPath); h = Convert.ToHexString(await sha.ComputeHashAsync(s)); }
        return new VaultEntry { EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath, BackupRelativePath = rp, EntryType = VaultEntryType.File, SizeInBytes = new FileInfo(item.FullPath).Length, ContentHash = h, BackupSuccessful = true };
    }

    private async Task<VaultEntry> BkDir(ResidualItem item, string fd)
    {
        if (!Directory.Exists(item.FullPath)) return FailEntry(item, "Ordner existiert nicht");
        string rp = item.FullPath.Replace(':', '_').Replace("\\\\", "_").TrimStart('_');
        string dd = Path.Combine(fd, rp); long ts = 0;
        foreach (string sf in Directory.EnumerateFiles(item.FullPath, "*", SearchOption.AllDirectories))
        { try { string rf = Path.GetRelativePath(item.FullPath, sf); string df = Path.Combine(dd, rf); Directory.CreateDirectory(Path.GetDirectoryName(df)!); File.Copy(sf, df, true); ts += new FileInfo(sf).Length; } catch { } }
        return new VaultEntry { EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath, BackupRelativePath = rp, EntryType = VaultEntryType.Directory, SizeInBytes = ts, BackupSuccessful = true };
    }

    private VaultEntry BkReg(ResidualItem item, string rd)
    {
        string fn = item.FullPath.Replace('\\', '_').Replace(':', '_') + ".reg";
        string dest = Path.Combine(rd, fn);
        using var proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "reg.exe", Arguments = "export \"" + item.FullPath + "\" \"" + dest + "\" /y", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true });
        proc?.WaitForExit(15000); bool ok = proc?.ExitCode == 0 && File.Exists(dest);
        return new VaultEntry { EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath, BackupRelativePath = Path.Combine("registry", fn), EntryType = item.ItemType == ResidualItemType.RegistryKey ? VaultEntryType.RegistryKey : VaultEntryType.RegistryValue, SizeInBytes = ok ? new FileInfo(dest).Length : 0, BackupSuccessful = ok, ErrorMessage = ok ? null : "Registry-Export fehlgeschlagen" };
    }

    public async Task<IReadOnlyList<VaultBackup>> GetAllBackupsAsync(CancellationToken ct = default)
    {
        var list = new List<VaultBackup>(); if (!Directory.Exists(_vaultBasePath)) return list.AsReadOnly();
        foreach (string d in Directory.GetDirectories(_vaultBasePath))
        { ct.ThrowIfCancellationRequested(); string m = Path.Combine(d, "manifest.json"); if (!File.Exists(m)) continue;
          try { var b = JsonSerializer.Deserialize<VaultBackup>(await File.ReadAllTextAsync(m, ct), JOpt); if (b is not null) list.Add(b); } catch (Exception ex) { _logger.Warning("Manifest beschaedigt: " + d + " - " + ex.Message); } }
        return list.OrderByDescending(b => b.CreatedAtUtc).ToList().AsReadOnly();
    }

    public void DeleteBackup(string id) { string d = Path.Combine(_vaultBasePath, id); if (Directory.Exists(d)) { Directory.Delete(d, true); _logger.Info("Backup geloescht: " + id); } }
    public string GetVaultPath() => _vaultBasePath;

    private static VaultEntry FailEntry(ResidualItem item, string err) => new()
    { EntryId = Guid.NewGuid().ToString("N"), OriginalPath = item.FullPath, BackupRelativePath = "",
      EntryType = item.ItemType switch { ResidualItemType.File => VaultEntryType.File, ResidualItemType.Directory => VaultEntryType.Directory, ResidualItemType.RegistryKey => VaultEntryType.RegistryKey, ResidualItemType.RegistryValue => VaultEntryType.RegistryValue, _ => VaultEntryType.File },
      BackupSuccessful = false, ErrorMessage = err };
}

public sealed class VaultProgressEventArgs : EventArgs
{
    public required string Message { get; init; }
    public required int TotalItems { get; init; }
    public required int CompletedItems { get; init; }
    public int PercentComplete => TotalItems == 0 ? 0 : (int)(CompletedItems * 100.0 / TotalItems);
}'
[System.IO.File]::WriteAllText("$root\Vault\VaultService.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [11/12] VaultService.cs" -ForegroundColor Green

# --- DATEI 12: Cleanup/CleanupService.cs ---
$code = 'using System.Runtime.Versioning;
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
    public string FormattedFreedSize { get { if (FreedBytes < 1024) return FreedBytes + " B"; if (FreedBytes < 1024 * 1024) return (FreedBytes / 1024.0).ToString("F1") + " KB"; return (FreedBytes / (1024.0 * 1024)).ToString("F1") + " MB"; } }
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
    public CleanupService(IZeroTraceLogger logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

    public async Task<CleanupResult> CleanAsync(IReadOnlyList<ResidualItem> items, string backupId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(backupId)) throw new InvalidOperationException("SICHERHEITSSPERRE: Loeschung ohne Backup-ID verboten!");
        var sel = items.Where(i => i.IsSelectedForDeletion).ToList();
        _logger.Info("Bereinigung: " + sel.Count + " Elemente, Backup-ID: " + backupId);
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var details = new List<CleanupItemResult>(); int ok = 0, fail = 0, skip = 0; long freed = 0;
        var ord = sel.OrderBy(i => i.ItemType switch { ResidualItemType.File => 0, ResidualItemType.RegistryValue => 1, ResidualItemType.RegistryKey => 2, ResidualItemType.Directory => 3, _ => 4 }).ToList();
        for (int i = 0; i < ord.Count; i++)
        {
            ct.ThrowIfCancellationRequested(); var item = ord[i];
            ProgressChanged?.Invoke(this, new CleanupProgressEventArgs { CurrentPath = item.FullPath, TotalItems = ord.Count, CompletedItems = i });
            try
            {
                var r = await DelItem(item); details.Add(r);
                if (r.Success) { ok++; freed += item.SizeInBytes ?? 0; } else skip++;
            }
            catch (Exception ex) { fail++; details.Add(new CleanupItemResult { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = ex.Message }); }
        }
        sw.Stop();
        _logger.Info("  Ergebnis: " + ok + " OK, " + fail + " Fehler, " + skip + " uebersprungen");
        return new CleanupResult { TotalItems = ord.Count, SuccessfullyDeleted = ok, Failed = fail, Skipped = skip, FreedBytes = freed, Duration = sw.Elapsed, Details = details.AsReadOnly(), BackupId = backupId };
    }

    private async Task<CleanupItemResult> DelItem(ResidualItem item) => item.ItemType switch
    {
        ResidualItemType.File => await DelFile(item), ResidualItemType.Directory => DelDir(item),
        ResidualItemType.RegistryKey => DelRegKey(item), ResidualItemType.RegistryValue => DelRegVal(item),
        _ => new CleanupItemResult { Path = item.FullPath, ItemType = item.ItemType, Success = false, ErrorMessage = "Unbekannt" }
    };

    private async Task<CleanupItemResult> DelFile(ResidualItem item)
    {
        if (!File.Exists(item.FullPath)) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Existiert nicht" };
        for (int a = 1; a <= MaxRetries; a++)
        {
            try { var at = File.GetAttributes(item.FullPath); if (at.HasFlag(FileAttributes.ReadOnly)) File.SetAttributes(item.FullPath, at & ~FileAttributes.ReadOnly); File.Delete(item.FullPath); return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = true }; }
            catch (IOException) when (a < MaxRetries) { await Task.Delay(RetryDelay); }
            catch (UnauthorizedAccessException) { bool s = false; try { s = NativeMethods.MoveFileExW(item.FullPath, null, 4); } catch { } return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = s ? "Neustart-Loesch" : "Zugriff verweigert", ScheduledForReboot = s }; }
        }
        return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.File, Success = false, ErrorMessage = "Nicht loeschbar" };
    }

    private CleanupItemResult DelDir(ResidualItem item)
    {
        if (!Directory.Exists(item.FullPath)) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = false, ErrorMessage = "Existiert nicht" };
        Directory.Delete(item.FullPath, true); return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.Directory, Success = true };
    }

    private CleanupItemResult DelRegKey(ResidualItem item)
    {
        var (h, sp) = ParseReg(item.FullPath); if (h is null) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Pfad?" };
        int ls = sp.LastIndexOf('\\'); if (ls < 0) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Ungueltig" };
        using var bk = RegistryKey.OpenBaseKey(h.Value, RegistryView.Default);
        using var pk = bk.OpenSubKey(sp.Substring(0, ls), true); if (pk is null) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = false, ErrorMessage = "Parent?" };
        pk.DeleteSubKeyTree(sp.Substring(ls + 1), false); return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryKey, Success = true };
    }

    private CleanupItemResult DelRegVal(ResidualItem item)
    {
        int sep = item.FullPath.LastIndexOf("::"); if (sep < 0) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Format?" };
        var (h, sp) = ParseReg(item.FullPath.Substring(0, sep)); if (h is null) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Pfad?" };
        using var bk = RegistryKey.OpenBaseKey(h.Value, RegistryView.Default);
        using var k = bk.OpenSubKey(sp, true); if (k is null) return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = false, ErrorMessage = "Key?" };
        k.DeleteValue(item.FullPath.Substring(sep + 2), false); return new CleanupItemResult { Path = item.FullPath, ItemType = ResidualItemType.RegistryValue, Success = true };
    }

    private static (RegistryHive? H, string S) ParseReg(string p)
    {
        if (p.StartsWith(@"HKEY_CURRENT_USER\", StringComparison.OrdinalIgnoreCase)) return (RegistryHive.CurrentUser, p.Substring(18));
        if (p.StartsWith(@"HKEY_LOCAL_MACHINE\", StringComparison.OrdinalIgnoreCase)) return (RegistryHive.LocalMachine, p.Substring(19));
        return (null, p);
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
    [System.Runtime.InteropServices.LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf16)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static partial bool MoveFileExW(string lpExistingFileName, string? lpNewFileName, int dwFlags);
}'
[System.IO.File]::WriteAllText("$root\Cleanup\CleanupService.cs", $code, [System.Text.Encoding]::UTF8)
Write-Host "  [12/12] CleanupService.cs" -ForegroundColor Green

Write-Host "`n=== Alle 12 Dateien repariert! ===" -ForegroundColor Cyan
Write-Host "Fuehre jetzt 'dotnet build' aus..." -ForegroundColor Yellow