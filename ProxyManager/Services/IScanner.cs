using ProxyManager.Models;

namespace ProxyManager.Services;

public interface IScanner
{
    Task<List<ScanResult>> ScanAllAsync();
    Task<List<ScanResult>> ScanDirectoryAsync(string path);
    List<SoftwareInfo> LoadSoftwareDatabase();
}
