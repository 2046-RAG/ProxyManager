using System.Diagnostics;
using System.Security.Principal;
using ProxyManager.Models;

namespace ProxyManager.Services;

public class Launcher : ILauncher
{
    public async Task<bool> LaunchAsync(ScanResult software, bool asAdmin = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = software.ExePath,
                WorkingDirectory = software.Directory,
                UseShellExecute = true
            };

            if (asAdmin && !IsRunningAsAdmin())
            {
                startInfo.Verb = "runas";
            }

            Process.Start(startInfo);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to launch {software.Name}: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    public bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void ElevateToAdmin()
    {
        var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath)) return;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to elevate: {ex.Message}");
        }
    }
}
