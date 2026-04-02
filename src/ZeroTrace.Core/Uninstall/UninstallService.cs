// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Models;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Uninstall;

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

[SupportedOSPlatform("windows")]
public sealed class UninstallService
{
    private readonly IZeroTraceLogger _logger;
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);
    public event EventHandler<UninstallStatusEventArgs>? StatusChanged;

    public UninstallService(IZeroTraceLogger logger) =>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UninstallResult> UninstallAsync(
        InstalledProgram program, bool preferQuiet = false,
        CancellationToken ct = default)
    {
        _logger.Info($"Deinstallation: '{program.DisplayName}'");
        Report($"Vorbereitung: '{program.DisplayName}'...");
        var sw = Stopwatch.StartNew();

        try
        {
            var (cmd, method) = ChooseStrategy(program, preferQuiet);
            if (cmd is null)
            {
                _logger.Warning($"Kein Deinstaller fuer '{program.DisplayName}'");
                return Fail(sw, UninstallMethod.NotAvailable, "Kein Deinstallationsbefehl verfuegbar.");
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

    private static (string? Cmd, UninstallMethod Method) ChooseStrategy(
        InstalledProgram p, bool quiet)
    {
        if (!string.IsNullOrEmpty(p.MsiProductCode))
            return quiet
                ? ($"msiexec.exe /x {p.MsiProductCode} /qn /norestart", UninstallMethod.MsiQuiet)
                : ($"msiexec.exe /x {p.MsiProductCode} /norestart", UninstallMethod.MsiStandard);
        if (quiet && !string.IsNullOrEmpty(p.QuietUninstallString))
            return (p.QuietUninstallString, UninstallMethod.ExecutableQuiet);
        if (!string.IsNullOrEmpty(p.UninstallString))
            return (p.UninstallString, UninstallMethod.ExecutableNormal);
        return (null, UninstallMethod.NotAvailable);
    }

    private async Task<UninstallResult> RunAsync(
        string commandLine, UninstallMethod method, string name,
        Stopwatch sw, CancellationToken ct)
    {
        var (file, args) = SplitCommand(commandLine);
        Report($"Starte Deinstaller: '{name}'...");
        var psi = new ProcessStartInfo
        {
            FileName = file, Arguments = args,
            UseShellExecute = true, Verb = "runas"
        };
        using var proc = new Process { StartInfo = psi };
        try
        {
            if (!proc.Start())
                return Fail(sw, method, "Prozess konnte nicht gestartet werden.");
            _logger.Info($"  PID: {proc.Id}");
            Report($"Warte auf Deinstaller: '{name}'...");
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(Timeout);
            try { await proc.WaitForExitAsync(linked.Token); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                _logger.Warning($"  TIMEOUT nach {Timeout.TotalMinutes:F0} min");
                Kill(proc);
                return Fail(sw, method, "Zeitueberschreitung - Deinstaller beendet.");
            }
            sw.Stop();
            int code = proc.ExitCode;
            bool ok = code is 0 or 1605 or 1614 or 1641 or 3010;
            bool reboot = code is 1641 or 3010;
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
