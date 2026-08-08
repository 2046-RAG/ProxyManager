using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProxyManager.Helpers;
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

    public string ScanButtonText => IsScanning ? "扫描中..." : "扫描";
    public bool CanScan => !IsScanning;
    public string DiscoveredCountText => $"已发现的软件（{DiscoveredSoftware.Count}）";
    public bool ShowEmptyScanHint => DiscoveredSoftware.Count == 0 && !IsScanning;

    public MainViewModel(IScanner scanner, ILauncher launcher, IThemeManager themeManager, ISettingsService settingsService)
    {
        _scanner = scanner;
        _launcher = launcher;
        _themeManager = themeManager;
        _settingsService = settingsService;

        var settings = _settingsService.Load();
        _currentTheme = settings.Theme;
        _autoStart = settings.AutoStart;
        LoadCustomSoftware(settings);
    }

    partial void OnCurrentThemeChanged(string value)
    {
        _themeManager.SetTheme(value);
    }

    partial void OnIsScanningChanged(bool value)
    {
        OnPropertyChanged(nameof(ScanButtonText));
        OnPropertyChanged(nameof(CanScan));
        OnPropertyChanged(nameof(ShowEmptyScanHint));
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        if (IsScanning) return;

        IsScanning = true;
        DiscoveredSoftware.Clear();
        OnPropertyChanged(nameof(DiscoveredCountText));

        try
        {
            var results = await _scanner.ScanAllAsync();
            var settings = _settingsService.Load();

            foreach (var result in results)
            {
                result.IsHidden = settings.HiddenExePaths.Contains(result.ExePath);
                DiscoveredSoftware.Add(new SoftwareCardViewModel(result, _launcher));
            }

            OnPropertyChanged(nameof(DiscoveredCountText));
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void ToggleAutoStart()
    {
        var settings = _settingsService.Load();
        settings.AutoStart = AutoStart;
        _settingsService.Save(settings);
        RegistryHelper.SetAutoStart(AutoStart);
    }

    [RelayCommand]
    private void RunAsAdmin()
    {
        _launcher.ElevateToAdmin();
    }

    [RelayCommand]
    private void AddCustomSoftware()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择要添加的软件",
            Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        var exePath = dialog.FileName;
        var name = Path.GetFileNameWithoutExtension(exePath);
        var (description, _, version) = PEHelper.GetFileInfo(exePath);

        CustomSoftware.Add(new SoftwareCardViewModel(new ScanResult
        {
            Name = name,
            ExePath = exePath,
            Directory = Path.GetDirectoryName(exePath) ?? string.Empty,
            Version = version,
            LastModified = File.GetLastWriteTime(exePath),
            Description = description,
            IsMainExecutable = true
        }, _launcher));

        var settings = _settingsService.Load();
        settings.CustomSoftware.Add(new CustomSoftware
        {
            Name = name,
            ExePath = exePath,
            Description = description
        });
        _settingsService.Save(settings);
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

    private void LoadCustomSoftware(AppSettings settings)
    {
        foreach (var custom in settings.CustomSoftware)
        {
            CustomSoftware.Add(new SoftwareCardViewModel(new ScanResult
            {
                Name = custom.Name,
                ExePath = custom.ExePath,
                Directory = Path.GetDirectoryName(custom.ExePath) ?? string.Empty,
                Description = custom.Description
            }, _launcher));
        }
    }
}
