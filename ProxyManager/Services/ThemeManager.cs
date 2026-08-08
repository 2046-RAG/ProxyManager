using System.Windows;

namespace ProxyManager.Services;

public class ThemeManager : IThemeManager
{
    private readonly ISettingsService _settingsService;
    private string _currentTheme;

    private static readonly Dictionary<string, string> ThemePaths = new()
    {
        ["Minimal"] = "Themes/Minimal.xaml",
        ["SciFi"] = "Themes/SciFi.xaml",
        ["Cyberpunk"] = "Themes/Cyberpunk.xaml",
        ["Romantic"] = "Themes/Romantic.xaml",
        ["Elegant"] = "Themes/Elegant.xaml"
    };

    public ThemeManager(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _currentTheme = _settingsService.Load().Theme;
    }

    public string GetCurrentTheme() => _currentTheme;

    public void SetTheme(string themeName)
    {
        if (!ThemePaths.ContainsKey(themeName))
            return;

        var app = System.Windows.Application.Current;
        if (app == null) return;

        var resourceDict = new ResourceDictionary
        {
            Source = new Uri(ThemePaths[themeName], UriKind.Relative)
        };

        app.Resources.MergedDictionaries.Clear();
        app.Resources.MergedDictionaries.Add(resourceDict);

        _currentTheme = themeName;

        var settings = _settingsService.Load();
        settings.Theme = themeName;
        _settingsService.Save(settings);
    }

    public List<string> GetAvailableThemes() => ThemePaths.Keys.ToList();
}
