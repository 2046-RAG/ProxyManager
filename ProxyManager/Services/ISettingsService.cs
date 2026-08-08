using ProxyManager.Models;

namespace ProxyManager.Services;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
    string GetSettingsPath();
}
