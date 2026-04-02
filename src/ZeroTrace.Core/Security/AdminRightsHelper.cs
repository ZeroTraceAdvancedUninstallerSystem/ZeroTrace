// ZeroTrace - Advanced Uninstaller System
// Copyright (c) 2026 Mario B. | MIT License

using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Principal;
using ZeroTrace.Core.Logging;

namespace ZeroTrace.Core.Security;

/// <summary>
/// Handles UAC elevation and administrator rights checking.
/// Required for registry access, service management, and system cleanup.
/// </summary>
[SupportedOSPlatform("windows")]
public static class AdminRightsHelper
{
    /// <summary>Returns true if the current process runs with admin privileges.</summary>
    public static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>Restart the application with elevated (admin) privileges.</summary>
    public static bool RestartAsAdmin()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return false;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = true,
                Verb = "runas" // Triggers the UAC dialog
            });
            Environment.Exit(0);
            return true;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User clicked "No" on the UAC dialog
            return false;
        }
    }

    /// <summary>
    /// Execute an action that requires admin rights.
    /// If not admin, shows a warning via logger.
    /// Returns false if not elevated and action was skipped.
    /// </summary>
    public static bool RequireAdmin(IZeroTraceLogger logger, string actionName)
    {
        if (IsRunningAsAdmin()) return true;

        logger.Warning($"Aktion '{actionName}' erfordert Administratorrechte. " +
                       "Bitte starte ZeroTrace als Administrator.");
        return false;
    }

    /// <summary>Get the current user's privilege level as display string.</summary>
    public static string GetPrivilegeDisplay() =>
        IsRunningAsAdmin() ? "Administrator" : "Standardbenutzer";

    /// <summary>Get the current Windows username.</summary>
    public static string GetCurrentUser() =>
        $"{Environment.UserDomainName}\\{Environment.UserName}";
}
