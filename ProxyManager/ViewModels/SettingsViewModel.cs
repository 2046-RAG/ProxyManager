using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxyManager.Models;
using ProxyManager.Services;

namespace ProxyManager.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeManager _themeManager;

    [ObservableProperty]
    private bool _autoStart;

    [ObservableProperty]
    private bool _minimizeToTray;

    [ObservableProperty]
    private bool _checkUpdates;

    [ObservableProperty]
    private string _selectedTheme;

    [ObservableProperty]
    private double _scaleFactor;

    public ObservableCollection<string> AvailableThemes { get; }
    public ObservableCollection<string> CustomScanPaths { get; } = new();
    public List<double> ScaleFactors { get; } = new() { 0.75, 1.0, 1.25, 1.5, 1.75, 2.0 };

    public SettingsViewModel(ISettingsService settingsService, IThemeManager themeManager)
    {
        _settingsService = settingsService;
        _themeManager = themeManager;

        AvailableThemes = new ObservableCollection<string>(_themeManager.GetAvailableThemes());

        var settings = _settingsService.Load();
        _autoStart = settings.AutoStart;
        _minimizeToTray = settings.MinimizeToTray;
        _checkUpdates = settings.CheckUpdates;
        _selectedTheme = settings.Theme;
        _scaleFactor = settings.ScaleFactor;

        foreach (var path in settings.CustomScanPaths)
        {
            CustomScanPaths.Add(path);
        }
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new AppSettings
        {
            AutoStart = AutoStart,
            MinimizeToTray = MinimizeToTray,
            CheckUpdates = CheckUpdates,
            Theme = SelectedTheme,
            ScaleFactor = ScaleFactor,
            CustomScanPaths = CustomScanPaths.ToList()
        };

        _settingsService.Save(settings);
        _themeManager.SetTheme(SelectedTheme);
    }

    [RelayCommand]
    private void AddScanPath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !CustomScanPaths.Contains(path))
        {
            CustomScanPaths.Add(path);
        }
    }

    [RelayCommand]
    private void RemoveScanPath(string path)
    {
        CustomScanPaths.Remove(path);
    }
}
