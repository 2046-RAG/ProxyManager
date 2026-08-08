using ProxyManager.Models;

namespace ProxyManager.Services;

public interface ILauncher
{
    Task<bool> LaunchAsync(ScanResult software, bool asAdmin = false);
    bool IsRunningAsAdmin();
    void ElevateToAdmin();
}
