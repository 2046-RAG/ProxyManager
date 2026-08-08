namespace ProxyManager.Services;

public interface IThemeManager
{
    string GetCurrentTheme();
    void SetTheme(string themeName);
    List<string> GetAvailableThemes();
}
