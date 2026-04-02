using System.Security.Principal;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ZeroTrace.Core.Security;

[SupportedOSPlatform("windows")]
public static class AdminRightsHelper
{
    public static bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RestartAsAdmin()
    {
        var exeName = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exeName)) return;

        var startInfo = new ProcessStartInfo(exeName)
        {
            Verb = "runas", // Triggert den UAC-Dialog (Windows-Schild)
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
            Environment.Exit(0); // Beendet die aktuelle Instanz ohne Admin-Rechte
        }
        catch
        {
            // Nutzer hat "Nein" im UAC-Dialog geklickt
        }
    }
}