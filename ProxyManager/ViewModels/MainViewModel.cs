using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxyManager.Models;
using ProxyManager.Services;

namespace ProxyManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IScanner _scanner;
    private readonly ILauncher _launcher;
    private readonly IThemeManager _themeManager;
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _currentTheme;

    [ObservableProperty]
    private bool _autoStart;

    [ObservableProperty]
    private bool _isScanning;

    public ObservableCollection<SoftwareCardViewModel> DiscoveredSoftware { get; } = new();
    public ObservableCollection<SoftwareCardViewModel> CustomSoftware { get; } = new();
    public List<string> AvailableThemes => _themeManager.GetAvailableThemes();

    public MainViewModel(IScanner scanner, ILauncher launcher, IThemeManager themeManager, ISettingsService settingsService)
    {
        _scanner = scanner;
        _launcher = launcher;
        _themeManager = themeManager;
        _settingsService = settingsService;

        var settings = _settingsService.Load();
        _currentTheme = settings.Theme;
        _autoStart = settings.AutoStart;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        DiscoveredSoftware.Clear();

        try
        {
            var results = await _scanner.ScanAllAsync();
            var settings = _settingsService.Load();

            foreach (var result in results)
            {
                result.IsHidden = settings.HiddenExePaths.Contains(result.ExePath);
                DiscoveredSoftware.Add(new SoftwareCardViewModel(result, _launcher));
            }
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void ChangeTheme(string themeName)
    {
        _themeManager.SetTheme(themeName);
        CurrentTheme = themeName;
    }

    [RelayCommand]
    private void ToggleAutoStart()
    {
        var settings = _settingsService.Load();
        settings.AutoStart = AutoStart;
        _settingsService.Save(settings);

        // TODO: Update registry for auto-start
    }

    [RelayCommand]
    private void RunAsAdmin()
    {
        _launcher.ElevateToAdmin();
    }

    [RelayCommand]
    private void AddCustomSoftware()
    {
        // TODO: Open add dialog
    }

    public void SaveHiddenStates()
    {
        var settings = _settingsService.Load();
        settings.HiddenExePaths = DiscoveredSoftware
            .Where(s => s.IsHidden)
            .Select(s => s.ExePath)
            .ToList();
        _settingsService.Save(settings);
    }
}
