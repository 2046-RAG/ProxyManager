using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProxyManager.Models;
using ProxyManager.Services;

namespace ProxyManager.ViewModels;

public partial class SoftwareCardViewModel : ObservableObject
{
    private readonly ILauncher _launcher;
    private readonly ScanResult _scanResult;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _exePath;

    [ObservableProperty]
    private bool _isMainExecutable;

    [ObservableProperty]
    private bool _isHidden;

    [ObservableProperty]
    private bool _isAdminRequired;

    public SoftwareCardViewModel(ScanResult scanResult, ILauncher launcher)
    {
        _scanResult = scanResult;
        _launcher = launcher;
        _name = scanResult.Name;
        _version = scanResult.Version;
        _description = scanResult.Description;
        _exePath = scanResult.ExePath;
        _isMainExecutable = scanResult.IsMainExecutable;
        _isHidden = scanResult.IsHidden;
    }

    [RelayCommand]
    private async Task LaunchAsync()
    {
        await _launcher.LaunchAsync(_scanResult, IsAdminRequired);
    }

    [RelayCommand]
    private void ToggleHide()
    {
        IsHidden = !IsHidden;
    }

    public ScanResult GetScanResult() => _scanResult;
}
