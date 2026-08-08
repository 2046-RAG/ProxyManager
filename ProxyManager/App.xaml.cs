using System.Diagnostics;
using System.Drawing;
using System.IO;
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

    public static void Log(string message)
    {
        try
        {
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "debug.log"),
                $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}");
        }
        catch
        {
            // 日志失败不影响主流程
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log("AppDomain.UnhandledException: " + args.ExceptionObject);
        DispatcherUnhandledException += (_, args) =>
        {
            Log("DispatcherUnhandledException: " + args.Exception);
            args.Handled = true;
        };
        PresentationTraceSources.DataBindingSource.Listeners.Add(
            new TextWriterTraceListener(Path.Combine(AppContext.BaseDirectory, "binding.log")));
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

        Log("OnStartup begin");

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

        Log("OnStartup end");
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
