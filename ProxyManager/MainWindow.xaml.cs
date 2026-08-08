using System.ComponentModel;
using System.Windows;
using ProxyManager.Services;
using ProxyManager.ViewModels;

namespace ProxyManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        var settingsService = new SettingsService();
        var scanner = new Scanner();
        var launcher = new Launcher();
        var themeManager = new ThemeManager(settingsService);
        
        DataContext = new MainViewModel(scanner, launcher, themeManager, settingsService);
        
        Closing += OnClosing;
        
        // 启动时自动扫描
        if (DataContext is MainViewModel vm)
        {
            vm.ScanCommand.Execute(null);
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (App.Current is App app && app.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
