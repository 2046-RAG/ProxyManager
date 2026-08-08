using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using ProxyManager.Helpers;
using ProxyManager.Services;
using Application = System.Windows.Application;

namespace ProxyManager;

public partial class App : Application
{
    private NotifyIcon? _trayIcon;
    private ISettingsService _settingsService = null!;
    private IThemeManager _themeManager = null!;
    private bool _minimizeToTray;

    public bool MinimizeToTray => _minimizeToTray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        _themeManager = new ThemeManager(_settingsService);
        
        var settings = _settingsService.Load();
        _minimizeToTray = settings.MinimizeToTray;

        // 应用保存的主题
        _themeManager.SetTheme(settings.Theme);

        // 设置开机自启
        RegistryHelper.SetAutoStart(settings.AutoStart);

        // 初始化托盘图标
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "代理管理器",
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("显示", null, (_, _) => ShowMainWindow());
        contextMenu.Items.Add("-");
        contextMenu.Items.Add("退出", null, (_, _) => Shutdown());

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        foreach (Window window in Windows)
        {
            if (window is MainWindow mainWindow)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
                break;
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
