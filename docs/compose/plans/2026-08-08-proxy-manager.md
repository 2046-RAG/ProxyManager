# Proxy Manager Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use compose:subagent (recommended) or compose:execute to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a WPF desktop application that scans for common proxy software, analyzes executables, and provides a themed launcher interface with system tray integration.

**Architecture:** WPF (.NET 8) with MVVM pattern. XAML-based theme system, modular services for scanning/analysis/launching, JSON-based settings persistence. Single exe deployment via self-contained runtime.

**Tech Stack:** C#, .NET 8, WPF, XAML, CommunityToolkit.Mvvm, System.Text.Json

## Global Constraints

- Target: Windows 10 LTSC (no additional runtimes required)
- Deployment: Single self-contained exe
- UI Framework: WPF with XAML theming
- MVVM pattern with CommunityToolkit.Mvvm
- Settings stored in %APPDATA%\ProxyManager\settings.json
- All UI text in Chinese (zh-CN)

---

## File Structure

```
ProxyManager/
├── App.xaml                          # Application entry, theme initialization
├── App.xaml.cs                       # Application startup, tray icon setup
├── MainWindow.xaml                   # Main window layout
├── MainWindow.xaml.cs                # Window code-behind (minimal)
├── ProxyManager.csproj               # Project file
│
├── ViewModels/
│   ├── MainViewModel.cs              # Main window view model
│   ├── SettingsViewModel.cs          # Settings dialog view model
│   └── SoftwareCardViewModel.cs      # Individual software card
│
├── Views/
│   ├── SettingsDialog.xaml           # Settings window
│   └── SettingsDialog.xaml.cs
│
├── Models/
│   ├── SoftwareInfo.cs               # Software metadata model
│   ├── ScanResult.cs                 # Scan result model
│   └── AppSettings.cs                # Settings model
│
├── Services/
│   ├──IScanner.cs                    # Scanner interface
│   ├── Scanner.cs                    # Directory scanning implementation
│   ├── IAnalyzer.cs                  # Analyzer interface
│   ├── Analyzer.cs                   # PE header analysis
│   ├── ILauncher.cs                  # Launcher interface
│   ├── Launcher.cs                   # Process launching
│   ├── IThemeManager.cs             # Theme manager interface
│   ├── ThemeManager.cs              # Theme switching
│   ├── ISettingsService.cs          # Settings interface
│   └── SettingsService.cs           # JSON settings persistence
│
├── Themes/
│   ├── SciFi.xaml                    # Sci-Fi theme
│   ├── Cyberpunk.xaml                # Cyberpunk theme
│   ├── Romantic.xaml                 # Romantic theme
│   ├── Elegant.xaml                  # Elegant theme
│   └── Minimal.xaml                  # Minimal theme
│
├── Converters/
│   ├── BoolToVisibilityConverter.cs  # Boolean to visibility
│   └── IconConverter.cs             # Exe to icon converter
│
├── Resources/
│   ├── Icons/                        # Application icons
│   └── SoftwareDatabase.json         # Built-in software list
│
└── Helpers/
    ├── PEHelper.cs                   # PE header reading
    └── RegistryHelper.cs            # Registry operations
```

---

### Task 1: Project Setup & Basic Window

**Covers:** [S2, S3]

**Files:**
- Create: `ProxyManager/ProxyManager.csproj`
- Create: `ProxyManager/App.xaml`
- Create: `ProxyManager/App.xaml.cs`
- Create: `ProxyManager/MainWindow.xaml`
- Create: `ProxyManager/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: None (first task)
- Produces: Runnable WPF application with basic window

- [ ] **Step 1: Create project file**

```xml
<!-- ProxyManager/ProxyManager.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationIcon>Resources\Icons\app.ico</ApplicationIcon>
    <AssemblyName>ProxyManager</AssemblyName>
    <RootNamespace>ProxyManager</RootNamespace>
    <Version>1.0.0</Version>
    <Authors>ProxyManager</Authors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Resource Include="Resources\Icons\app.ico" />
    <Content Include="Resources\SoftwareDatabase.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Create App.xaml with theme resource**

```xml
<!-- ProxyManager/App.xaml -->
<Application x:Class="ProxyManager.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/Minimal.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 3: Create App.xaml.cs**

```csharp
// ProxyManager/App.xaml.cs
using System.Windows;

namespace ProxyManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }
}
```

- [ ] **Step 4: Create MainWindow.xaml**

```xml
<!-- ProxyManager/MainWindow.xaml -->
<Window x:Class="ProxyManager.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="ProxyManager" 
        Height="600" Width="800"
        MinHeight="400" MinWidth="600"
        WindowStartupLocation="CenterScreen"
        Background="{DynamicResource BackgroundBrush}">
    
    <Grid>
        <TextBlock Text="ProxyManager - Loading..." 
                   HorizontalAlignment="Center" 
                   VerticalAlignment="Center"
                   Foreground="{DynamicResource PrimaryBrush}"
                   FontSize="24"/>
    </Grid>
</Window>
```

- [ ] **Step 5: Create MainWindow.xaml.cs**

```csharp
// ProxyManager/MainWindow.xaml.cs
using System.Windows;

namespace ProxyManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
```

- [ ] **Step 6: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 7: Commit**

```bash
git add ProxyManager/
git commit -m "feat: initialize WPF project with basic window"
```

---

### Task 2: Models & Settings Service

**Covers:** [S4, S5]

**Files:**
- Create: `ProxyManager/Models/SoftwareInfo.cs`
- Create: `ProxyManager/Models/ScanResult.cs`
- Create: `ProxyManager/Models/AppSettings.cs`
- Create: `ProxyManager/Services/ISettingsService.cs`
- Create: `ProxyManager/Services/SettingsService.cs`
- Create: `ProxyManager/Resources/SoftwareDatabase.json`

**Interfaces:**
- Consumes: None
- Produces: Data models and settings persistence for later tasks

- [ ] **Step 1: Create SoftwareInfo model**

```csharp
// ProxyManager/Models/SoftwareInfo.cs
using System.Text.Json.Serialization;

namespace ProxyManager.Models;

public class SoftwareInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("github_url")]
    public string GitHubUrl { get; set; } = string.Empty;

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("latest_version")]
    public string LatestVersion { get; set; } = string.Empty;

    [JsonPropertyName("update_date")]
    public string UpdateDate { get; set; } = string.Empty;

    [JsonPropertyName("typical_paths")]
    public List<string> TypicalPaths { get; set; } = new();

    [JsonPropertyName("exe_names")]
    public List<string> ExeNames { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
```

- [ ] **Step 2: Create ScanResult model**

```csharp
// ProxyManager/Models/ScanResult.cs
namespace ProxyManager.Models;

public class ScanResult
{
    public string Name { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public bool IsMainExecutable { get; set; }
    public bool IsHidden { get; set; }
    public SoftwareInfo? Metadata { get; set; }
    public List<string> RelatedFiles { get; set; } = new();
}
```

- [ ] **Step 3: Create AppSettings model**

```csharp
// ProxyManager/Models/AppSettings.cs
namespace ProxyManager.Models;

public class AppSettings
{
    public bool AutoStart { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool CheckUpdates { get; set; } = false;
    public string Theme { get; set; } = "Minimal";
    public double ScaleFactor { get; set; } = 1.0;
    public List<string> CustomScanPaths { get; set; } = new();
    public List<string> HiddenExePaths { get; set; } = new();
    public List<CustomSoftware> CustomSoftware { get; set; } = new();
}

public class CustomSoftware
{
    public string Name { get; set; } = string.Empty;
    public string ExePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create ISettingsService interface**

```csharp
// ProxyManager/Services/ISettingsService.cs
using ProxyManager.Models;

namespace ProxyManager.Services;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
    string GetSettingsPath();
}
```

- [ ] **Step 5: Create SettingsService implementation**

```csharp
// ProxyManager/Services/SettingsService.cs
using System.Text.Json;
using ProxyManager.Models;

namespace ProxyManager.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appDir = Path.Combine(appData, "ProxyManager");
        Directory.CreateDirectory(appDir);
        _settingsPath = Path.Combine(appDir, "settings.json");
    }

    public string GetSettingsPath() => _settingsPath;

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
            return new AppSettings();

        var json = File.ReadAllText(_settingsPath);
        return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }
}
```

- [ ] **Step 6: Create SoftwareDatabase.json**

```json
// ProxyManager/Resources/SoftwareDatabase.json
[
  {
    "name": "Clash Verge Rev",
    "github_url": "https://github.com/clash-verge-rev/clash-verge-rev",
    "download_url": "https://github.com/clash-verge-rev/clash-verge-rev/releases",
    "latest_version": "1.6.6",
    "update_date": "2024-01-15",
    "typical_paths": ["%LOCALAPPDATA%\\clash-verge-rev"],
    "exe_names": ["Clash Verge.exe", "clash-verge.exe"],
    "description": "Clash GUI client with Tauri"
  },
  {
    "name": "v2rayN",
    "github_url": "https://github.com/2dust/v2rayN",
    "download_url": "https://github.com/2dust/v2rayN/releases",
    "latest_version": "6.23",
    "update_date": "2024-01-10",
    "typical_paths": ["%USERPROFILE%\\v2rayN"],
    "exe_names": ["v2rayN.exe", "v2rayN-core.exe"],
    "description": "V2Ray GUI client for Windows"
  },
  {
    "name": "Shadowsocks Windows",
    "github_url": "https://github.com/shadowsocks/shadowsocks-windows",
    "download_url": "https://github.com/shadowsocks/shadowsocks-windows/releases",
    "latest_version": "4.1.6",
    "update_date": "2023-06-20",
    "typical_paths": ["%APPDATA%\\shadowsocks"],
    "exe_names": ["Shadowsocks.exe", "ss-local.exe"],
    "description": "Shadowsocks GUI client"
  },
  {
    "name": "Hiddify",
    "github_url": "https://github.com/hiddify/hiddify-app",
    "download_url": "https://github.com/hiddify/hiddify-app/releases",
    "latest_version": "2.0.5",
    "update_date": "2024-01-12",
    "typical_paths": ["%LOCALAPPDATA%\\Hiddify"],
    "exe_names": ["Hiddify.exe"],
    "description": "Multi-protocol proxy client"
  },
  {
    "name": "Nekoray",
    "github_url": "https://github.com/MatsuriDayo/nekoray",
    "download_url": "https://github.com/MatsuriDayo/nekoray/releases",
    "latest_version": "3.26",
    "update_date": "2024-01-08",
    "typical_paths": ["%USERPROFILE%\\nekoray"],
    "exe_names": ["nekoray.exe", "nekoray-core.exe"],
    "description": "Qt-based GUI client for sing-box/xray"
  },
  {
    "name": "mihomo (Clash Meta)",
    "github_url": "https://github.com/MetaCubeX/mihomo",
    "download_url": "https://github.com/MetaCubeX/mihomo/releases",
    "latest_version": "1.18.0",
    "update_date": "2024-01-14",
    "typical_paths": ["%USERPROFILE%\\mihomo"],
    "exe_names": ["mihomo.exe"],
    "description": " Clash Meta kernel"
  },
  {
    "name": "ClashN",
    "github_url": "https://github.com/2dust/ClashN",
    "download_url": "https://github.com/2dust/ClashN/releases",
    "latest_version": "3.16",
    "update_date": "2024-01-05",
    "typical_paths": ["%USERPROFILE%\\ClashN"],
    "exe_names": ["ClashN.exe"],
    "description": "Clash GUI client"
  },
  {
    "name": "Qv2ray",
    "github_url": "https://github.com/Qv2ray/Qv2ray",
    "download_url": "https://github.com/Qv2ray/Qv2ray/releases",
    "latest_version": "2.7.0",
    "update_date": "2023-08-15",
    "typical_paths": ["%LOCALAPPDATA%\\Qv2ray"],
    "exe_names": ["qv2ray.exe"],
    "description": "Qt-based V2Ray client"
  },
  {
    "name": "sing-box",
    "github_url": "https://github.com/SagerNet/sing-box",
    "download_url": "https://github.com/SagerNet/sing-box/releases",
    "latest_version": "1.8.0",
    "update_date": "2024-01-11",
    "typical_paths": ["%USERPROFILE%\\sing-box"],
    "exe_names": ["sing-box.exe"],
    "description": "Universal proxy platform"
  },
  {
    "name": "V2rayA",
    "github_url": "https://github.com/v2rayA/v2rayA",
    "download_url": "https://github.com/v2rayA/v2rayA/releases",
    "latest_version": "2.0.0",
    "update_date": "2024-01-03",
    "typical_paths": ["%LOCALAPPDATA%\\v2raya"],
    "exe_names": ["v2raya.exe", "v2ray.exe"],
    "description": "Web GUI for V2Ray"
  }
]
```

- [ ] **Step 7: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 8: Commit**

```bash
git add ProxyManager/Models/ ProxyManager/Services/ISettingsService.cs ProxyManager/Services/SettingsService.cs ProxyManager/Resources/SoftwareDatabase.json
git commit -m "feat: add data models and settings service"
```

---

### Task 3: Theme System

**Covers:** [S2, S4, S5.4]

**Files:**
- Create: `ProxyManager/Themes/SciFi.xaml`
- Create: `ProxyManager/Themes/Cyberpunk.xaml`
- Create: `ProxyManager/Themes/Romantic.xaml`
- Create: `ProxyManager/Themes/Elegant.xaml`
- Create: `ProxyManager/Themes/Minimal.xaml`
- Create: `ProxyManager/Services/IThemeManager.cs`
- Create: `ProxyManager/Services/ThemeManager.cs`

**Interfaces:**
- Consumes: ISettingsService (Task 2)
- Produces: Theme switching for UI tasks

- [ ] **Step 1: Create Minimal theme (default)**

```xml
<!-- ProxyManager/Themes/Minimal.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#2196F3</Color>
    <Color x:Key="SecondaryColor">#1976D2</Color>
    <Color x:Key="BackgroundColor">#FFFFFF</Color>
    <Color x:Key="SurfaceColor">#F5F5F5</Color>
    <Color x:Key="AccentColor">#03A9F4</Color>
    <Color x:Key="ErrorColor">#F44336</Color>
    <Color x:Key="TextPrimaryColor">#212121</Color>
    <Color x:Key="TextSecondaryColor">#757575</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    
    <!-- Card Style -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="8"/>
        <Setter Property="Padding" Value="16"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="Effect">
            <Setter.Value>
                <DropShadowEffect BlurRadius="8" ShadowDepth="2" Opacity="0.2"/>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Button Style -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Window Style -->
    <Style x:Key="WindowStyle" TargetType="Window">
        <Setter Property="Background" Value="{StaticResource BackgroundBrush}"/>
    </Style>
</ResourceDictionary>
```

- [ ] **Step 2: Create Sci-Fi theme**

```xml
<!-- ProxyManager/Themes/SciFi.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#00FF88</Color>
    <Color x:Key="SecondaryColor">#00CC6A</Color>
    <Color x:Key="BackgroundColor">#0A0A0F</Color>
    <Color x:Key="SurfaceColor">#12121A</Color>
    <Color x:Key="AccentColor">#FF00FF</Color>
    <Color x:Key="ErrorColor">#FF3366</Color>
    <Color x:Key="TextPrimaryColor">#E0E0E0</Color>
    <Color x:Key="TextSecondaryColor">#808080</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    
    <!-- Card Style with glow effect -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="4"/>
        <Setter Property="Padding" Value="16"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="Effect">
            <Setter.Value>
                <DropShadowEffect Color="{StaticResource PrimaryColor}" 
                                  BlurRadius="16" ShadowDepth="0" Opacity="0.5"/>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Button Style with neon glow -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="Transparent"/>
        <Setter Property="Foreground" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="BorderThickness" Value="2"/>
        <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="border"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="2"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="border" Property="Background" Value="{StaticResource PrimaryBrush}"/>
                            <Setter Property="Foreground" Value="{StaticResource BackgroundBrush}"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Window Style -->
    <Style x:Key="WindowStyle" TargetType="Window">
        <Setter Property="Background" Value="{StaticResource BackgroundBrush}"/>
    </Style>
</ResourceDictionary>
```

- [ ] **Step 3: Create Cyberpunk theme**

```xml
<!-- ProxyManager/Themes/Cyberpunk.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#FFD700</Color>
    <Color x:Key="SecondaryColor">#FFA500</Color>
    <Color x:Key="BackgroundColor">#0D0D0D</Color>
    <Color x:Key="SurfaceColor">#1A1A1A</Color>
    <Color x:Key="AccentColor">#00FFFF</Color>
    <Color x:Key="ErrorColor">#FF0040</Color>
    <Color x:Key="TextPrimaryColor">#FFFFFF</Color>
    <Color x:Key="TextSecondaryColor">#B0B0B0</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    
    <!-- Card Style with angular design -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="0"/>
        <Setter Property="Padding" Value="16"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="2,2,0,0"/>
    </Style>
    
    <!-- Button Style -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource BackgroundBrush}"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontWeight" Value="Bold"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Window Style -->
    <Style x:Key="WindowStyle" TargetType="Window">
        <Setter Property="Background" Value="{StaticResource BackgroundBrush}"/>
    </Style>
</ResourceDictionary>
```

- [ ] **Step 4: Create Romantic theme**

```xml
<!-- ProxyManager/Themes/Romantic.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#E91E63</Color>
    <Color x:Key="SecondaryColor">#C2185B</Color>
    <Color x:Key="BackgroundColor">#FFF8F9</Color>
    <Color x:Key="SurfaceColor">#FFFFFF</Color>
    <Color x:Key="AccentColor">#F8BBD0</Color>
    <Color x:Key="ErrorColor">#D32F2F</Color>
    <Color x:Key="TextPrimaryColor">#4A4A4A</Color>
    <Color x:Key="TextSecondaryColor">#8A8A8A</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    
    <!-- Card Style with soft shadows -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="16"/>
        <Setter Property="Padding" Value="20"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="Effect">
            <Setter.Value>
                <DropShadowEffect Color="{StaticResource AccentColor}" 
                                  BlurRadius="20" ShadowDepth="4" Opacity="0.3"/>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Button Style with rounded corners -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="20,10"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            CornerRadius="20"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Window Style -->
    <Style x:Key="WindowStyle" TargetType="Window">
        <Setter Property="Background" Value="{StaticResource BackgroundBrush}"/>
    </Style>
</ResourceDictionary>
```

- [ ] **Step 5: Create Elegant theme**

```xml
<!-- ProxyManager/Themes/Elegant.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- Colors -->
    <Color x:Key="PrimaryColor">#D4AF37</Color>
    <Color x:Key="SecondaryColor">#B8860B</Color>
    <Color x:Key="BackgroundColor">#1A1A1A</Color>
    <Color x:Key="SurfaceColor">#2A2A2A</Color>
    <Color x:Key="AccentColor">#FFD700</Color>
    <Color x:Key="ErrorColor">#C0392B</Color>
    <Color x:Key="TextPrimaryColor">#F5F5F5</Color>
    <Color x:Key="TextSecondaryColor">#A0A0A0</Color>
    
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="ErrorBrush" Color="{StaticResource ErrorColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
    
    <!-- Card Style with gold border -->
    <Style x:Key="CardStyle" TargetType="Border">
        <Setter Property="Background" Value="{StaticResource SurfaceBrush}"/>
        <Setter Property="CornerRadius" Value="4"/>
        <Setter Property="Padding" Value="16"/>
        <Setter Property="Margin" Value="8"/>
        <Setter Property="BorderBrush" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="BorderThickness" Value="1"/>
    </Style>
    
    <!-- Button Style -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="{StaticResource BackgroundBrush}"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="FontSize" Value="14"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}"
                            CornerRadius="2"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
    
    <!-- Window Style -->
    <Style x:Key="WindowStyle" TargetType="Window">
        <Setter Property="Background" Value="{StaticResource BackgroundBrush}"/>
    </Style>
</ResourceDictionary>
```

- [ ] **Step 6: Create IThemeManager interface**

```csharp
// ProxyManager/Services/IThemeManager.cs
namespace ProxyManager.Services;

public interface IThemeManager
{
    string GetCurrentTheme();
    void SetTheme(string themeName);
    List<string> GetAvailableThemes();
}
```

- [ ] **Step 7: Create ThemeManager implementation**

```csharp
// ProxyManager/Services/ThemeManager.cs
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

        var app = Application.Current;
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
```

- [ ] **Step 8: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 9: Commit**

```bash
git add ProxyManager/Themes/ ProxyManager/Services/IThemeManager.cs ProxyManager/Services/ThemeManager.cs
git commit -m "feat: add theme system with 5 built-in themes"
```

---

### Task 4: Scanner Service

**Covers:** [S2, S5.2]

**Files:**
- Create: `ProxyManager/Services/IScanner.cs`
- Create: `ProxyManager/Services/Scanner.cs`
- Create: `ProxyManager/Helpers/PEHelper.cs`

**Interfaces:**
- Consumes: SoftwareInfo (Task 2), SoftwareDatabase.json (Task 2)
- Produces: List<ScanResult> for UI tasks

- [ ] **Step 1: Create IScanner interface**

```csharp
// ProxyManager/Services/IScanner.cs
using ProxyManager.Models;

namespace ProxyManager.Services;

public interface IScanner
{
    Task<List<ScanResult>> ScanAllAsync();
    Task<List<ScanResult>> ScanDirectoryAsync(string path);
    List<SoftwareInfo> LoadSoftwareDatabase();
}
```

- [ ] **Step 2: Create PEHelper**

```csharp
// ProxyManager/Helpers/PEHelper.cs
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;

namespace ProxyManager.Helpers;

public static class PEHelper
{
    public static (string description, string company, string version) GetFileInfo(string exePath)
    {
        try
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
            return (
                versionInfo.FileDescription ?? string.Empty,
                versionInfo.CompanyName ?? string.Empty,
                versionInfo.FileVersion ?? string.Empty
            );
        }
        catch
        {
            return (string.Empty, string.Empty, string.Empty);
        }
    }

    public static bool IsPEFile(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var peReader = new PEReader(stream);
            return peReader.PEHeaders.PEHeader != null;
        }
        catch
        {
            return false;
        }
    }

    public static List<string> FindRelatedExecutables(string directory, string mainExeName)
    {
        var related = new List<string>();
        var exeFiles = Directory.GetFiles(directory, "*.exe");

        foreach (var exe in exeFiles)
        {
            if (Path.GetFileName(exe).Equals(mainExeName, StringComparison.OrdinalIgnoreCase))
                continue;

            var (desc, _, _) = GetFileInfo(exe);
            if (!string.IsNullOrEmpty(desc))
            {
                related.Add(exe);
            }
        }

        return related;
    }
}
```

- [ ] **Step 3: Create Scanner implementation**

```csharp
// ProxyManager/Services/Scanner.cs
using System.Text.Json;
using ProxyManager.Helpers;
using ProxyManager.Models;

namespace ProxyManager.Services;

public class Scanner : IScanner
{
    private readonly List<SoftwareInfo> _softwareDatabase;

    public Scanner()
    {
        _softwareDatabase = LoadSoftwareDatabase();
    }

    public List<SoftwareInfo> LoadSoftwareDatabase()
    {
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SoftwareDatabase.json");
        if (!File.Exists(dbPath))
            return new List<SoftwareInfo>();

        var json = File.ReadAllText(dbPath);
        return JsonSerializer.Deserialize<List<SoftwareInfo>>(json) ?? new List<SoftwareInfo>();
    }

    public async Task<List<ScanResult>> ScanAllAsync()
    {
        var results = new List<ScanResult>();

        foreach (var software in _softwareDatabase)
        {
            foreach (var pathTemplate in software.TypicalPaths)
            {
                var path = Environment.ExpandEnvironmentVariables(pathTemplate);
                if (Directory.Exists(path))
                {
                    var found = await ScanDirectoryForSoftwareAsync(path, software);
                    results.AddRange(found);
                }
            }
        }

        return results;
    }

    public async Task<List<ScanResult>> ScanDirectoryAsync(string path)
    {
        var results = new List<ScanResult>();

        if (!Directory.Exists(path))
            return results;

        var exeFiles = Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories);

        foreach (var exe in exeFiles)
        {
            var result = await AnalyzeExecutableAsync(exe);
            if (result != null)
            {
                results.Add(result);
            }
        }

        return results;
    }

    private async Task<List<ScanResult>> ScanDirectoryForSoftwareAsync(string directory, SoftwareInfo software)
    {
        var results = new List<ScanResult>();

        foreach (var exeName in software.ExeNames)
        {
            var exePath = Path.Combine(directory, exeName);
            if (File.Exists(exePath))
            {
                var (desc, company, version) = PEHelper.GetFileInfo(exePath);
                var relatedFiles = PEHelper.FindRelatedExecutables(directory, exeName);

                results.Add(new ScanResult
                {
                    Name = software.Name,
                    ExePath = exePath,
                    Directory = directory,
                    Version = version,
                    LastModified = File.GetLastWriteTime(exePath),
                    Description = desc,
                    Company = company,
                    IsMainExecutable = true,
                    Metadata = software,
                    RelatedFiles = relatedFiles
                });
            }
        }

        return await Task.FromResult(results);
    }

    private async Task<ScanResult?> AnalyzeExecutableAsync(string exePath)
    {
        try
        {
            var (desc, company, version) = PEHelper.GetFileInfo(exePath);
            var directory = Path.GetDirectoryName(exePath) ?? string.Empty;
            var exeName = Path.GetFileName(exePath);

            var matchingSoftware = _softwareDatabase.FirstOrDefault(s =>
                s.ExeNames.Any(name => name.Equals(exeName, StringComparison.OrdinalIgnoreCase)));

            return await Task.FromResult(new ScanResult
            {
                Name = matchingSoftware?.Name ?? Path.GetFileNameWithoutExtension(exePath),
                ExePath = exePath,
                Directory = directory,
                Version = version,
                LastModified = File.GetLastWriteTime(exePath),
                Description = desc,
                Company = company,
                IsMainExecutable = matchingSoftware != null,
                Metadata = matchingSoftware
            });
        }
        catch
        {
            return null;
        }
    }
}
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add ProxyManager/Services/IScanner.cs ProxyManager/Services/Scanner.cs ProxyManager/Helpers/PEHelper.cs
git commit -m "feat: add scanner service with PE header analysis"
```

---

### Task 5: Launcher Service

**Covers:** [S2, S5.5]

**Files:**
- Create: `ProxyManager/Services/ILauncher.cs`
- Create: `ProxyManager/Services/Launcher.cs`

**Interfaces:**
- Consumes: ScanResult (Task 4)
- Produces: Launch capability for UI tasks

- [ ] **Step 1: Create ILauncher interface**

```csharp
// ProxyManager/Services/ILauncher.cs
using ProxyManager.Models;

namespace ProxyManager.Services;

public interface ILauncher
{
    Task<bool> LaunchAsync(ScanResult software, bool asAdmin = false);
    bool IsRunningAsAdmin();
    void ElevateToAdmin();
}
```

- [ ] **Step 2: Create Launcher implementation**

```csharp
// ProxyManager/Services/Launcher.cs
using System.Diagnostics;
using System.Security.Principal;
using ProxyManager.Models;

namespace ProxyManager.Services;

public class Launcher : ILauncher
{
    public async Task<bool> LaunchAsync(ScanResult software, bool asAdmin = false)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = software.ExePath,
                WorkingDirectory = software.Directory,
                UseShellExecute = true
            };

            if (asAdmin && !IsRunningAsAdmin())
            {
                startInfo.Verb = "runas";
            }

            Process.Start(startInfo);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to launch {software.Name}: {ex.Message}");
            return await Task.FromResult(false);
        }
    }

    public bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void ElevateToAdmin()
    {
        var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(exePath)) return;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to elevate: {ex.Message}");
        }
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add ProxyManager/Services/ILauncher.cs ProxyManager/Services/Launcher.cs
git commit -m "feat: add launcher service with admin elevation"
```

---

### Task 6: ViewModels

**Covers:** [S2, S5.1]

**Files:**
- Create: `ProxyManager/ViewModels/MainViewModel.cs`
- Create: `ProxyManager/ViewModels/SoftwareCardViewModel.cs`
- Create: `ProxyManager/ViewModels/SettingsViewModel.cs`

**Interfaces:**
- Consumes: All services (Tasks 2-5)
- Produces: MVVM binding for UI tasks

- [ ] **Step 1: Create SoftwareCardViewModel**

```csharp
// ProxyManager/ViewModels/SoftwareCardViewModel.cs
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
```

- [ ] **Step 2: Create MainViewModel**

```csharp
// ProxyManager/ViewModels/MainViewModel.cs
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
```

- [ ] **Step 3: Create SettingsViewModel**

```csharp
// ProxyManager/ViewModels/SettingsViewModel.cs
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
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add ProxyManager/ViewModels/
git commit -m "feat: add MVVM view models"
```

---

### Task 7: Main Window UI

**Covers:** [S2, S6]

**Files:**
- Modify: `ProxyManager/MainWindow.xaml`
- Modify: `ProxyManager/MainWindow.xaml.cs`

**Interfaces:**
- Consumes: MainViewModel (Task 6), themes (Task 3)
- Produces: Complete main window UI

- [ ] **Step 1: Update MainWindow.xaml**

```xml
<!-- ProxyManager/MainWindow.xaml -->
<Window x:Class="ProxyManager.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:ProxyManager.ViewModels"
        Title="代理管理器" 
        Height="600" Width="800"
        MinHeight="400" MinWidth="600"
        WindowStartupLocation="CenterScreen"
        Background="{DynamicResource BackgroundBrush}"
        Style="{StaticResource WindowStyle}">
    
    <Window.DataContext>
        <vm:MainViewModel/>
    </Window.DataContext>
    
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Grid Grid.Row="0" Margin="0,0,0,16">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <TextBlock Grid.Column="0" 
                       Text="代理管理器" 
                       FontSize="24" 
                       FontWeight="Bold"
                       Foreground="{DynamicResource PrimaryBrush}"
                       VerticalAlignment="Center"/>
            
            <StackPanel Grid.Column="1" Orientation="Horizontal">
                <ComboBox ItemsSource="{Binding AvailableThemes}"
                          SelectedItem="{Binding CurrentTheme}"
                          Margin="0,0,8,0"
                          Width="120"/>
                <Button Content="扫描"
                        Command="{Binding ScanCommand}"
                        Style="{StaticResource PrimaryButtonStyle}"
                        Margin="0,0,8,0"/>
                <Button Content="添加"
                        Command="{Binding AddCustomSoftwareCommand}"
                        Style="{StaticResource PrimaryButtonStyle}"/>
            </StackPanel>
        </Grid>
        
        <!-- Main Content -->
        <ScrollViewer Grid.Row="1" VerticalScrollBarVisibility="Auto">
            <StackPanel>
                <!-- Discovered Software -->
                <TextBlock Text="已发现的软件" 
                           FontSize="18" 
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource TextPrimaryBrush}"
                           Margin="0,0,0,8"/>
                
                <ItemsControl ItemsSource="{Binding DiscoveredSoftware}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Style="{StaticResource CardStyle}"
                                    Visibility="{Binding IsHidden, Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=Inverse}">
                                <Grid>
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="Auto"/>
                                        <ColumnDefinition Width="*"/>
                                        <ColumnDefinition Width="Auto"/>
                                    </Grid.ColumnDefinitions>
                                    
                                    <!-- Icon placeholder -->
                                    <Border Grid.Column="0" 
                                            Width="48" Height="48"
                                            Background="{DynamicResource SurfaceBrush}"
                                            CornerRadius="4"
                                            Margin="0,0,12,0">
                                        <TextBlock Text="📦" 
                                                   FontSize="24"
                                                   HorizontalAlignment="Center"
                                                   VerticalAlignment="Center"/>
                                    </Border>
                                    
                                    <!-- Info -->
                                    <StackPanel Grid.Column="1" VerticalAlignment="Center">
                                        <TextBlock Text="{Binding Name}" 
                                                   FontSize="16" 
                                                   FontWeight="SemiBold"
                                                   Foreground="{DynamicResource TextPrimaryBrush}"/>
                                        <TextBlock Text="{Binding Version}" 
                                                   FontSize="12"
                                                   Foreground="{DynamicResource TextSecondaryBrush}"/>
                                        <TextBlock Text="{Binding Description}" 
                                                   FontSize="12"
                                                   Foreground="{DynamicResource TextSecondaryBrush}"
                                                   TextTrimming="CharacterEllipsis"/>
                                    </StackPanel>
                                    
                                    <!-- Actions -->
                                    <StackPanel Grid.Column="2" Orientation="Horizontal">
                                        <CheckBox Content="管理员" 
                                                  IsChecked="{Binding IsAdminRequired}"
                                                  Margin="0,0,8,0"/>
                                        <Button Content="启动"
                                                Command="{Binding LaunchCommand}"
                                                Style="{StaticResource PrimaryButtonStyle}"
                                                Margin="0,0,8,0"/>
                                        <Button Content="隐藏"
                                                Command="{Binding ToggleHideCommand}"
                                                Style="{StaticResource PrimaryButtonStyle}"/>
                                    </StackPanel>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
                
                <!-- Custom Software -->
                <TextBlock Text="自定义软件" 
                           FontSize="18" 
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource TextPrimaryBrush}"
                           Margin="0,16,0,8"/>
                
                <ItemsControl ItemsSource="{Binding CustomSoftware}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <Border Style="{StaticResource CardStyle}">
                                <!-- Similar layout to discovered -->
                            </Border>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Footer -->
        <Grid Grid.Row="2" Margin="0,16,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            
            <StackPanel Grid.Column="0" Orientation="Horizontal">
                <CheckBox Content="开机自启" 
                          IsChecked="{Binding AutoStart}"
                          Command="{Binding ToggleAutoStartCommand}"
                          Margin="0,0,16,0"/>
                <Button Content="以管理员身份运行"
                        Command="{Binding RunAsAdminCommand}"
                        Style="{StaticResource PrimaryButtonStyle}"/>
            </StackPanel>
            
            <TextBlock Grid.Column="1" 
                       Text="v1.0.0"
                       Foreground="{DynamicResource TextSecondaryBrush}"
                       VerticalAlignment="Center"/>
        </Grid>
    </Grid>
</Window>
```

- [ ] **Step 2: Add BoolToVisibilityConverter**

```csharp
// ProxyManager/Converters/BoolToVisibilityConverter.cs
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ProxyManager.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            var invert = parameter?.ToString() == "Inverse";
            var result = invert ? !boolValue : boolValue;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            var invert = parameter?.ToString() == "Inverse";
            var result = visibility == Visibility.Visible;
            return invert ? !result : result;
        }
        return false;
    }
}
```

- [ ] **Step 3: Update App.xaml with converter**

```xml
<!-- ProxyManager/App.xaml -->
<Application x:Class="ProxyManager.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:converters="clr-namespace:ProxyManager.Converters"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Themes/Minimal.xaml"/>
            </ResourceDictionary.MergedDictionaries>
            
            <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add ProxyManager/MainWindow.xaml ProxyManager/Converters/
git commit -m "feat: implement main window UI with software cards"
```

---

### Task 8: System Tray & Auto-Start

**Covers:** [S2, S5.5]

**Files:**
- Modify: `ProxyManager/App.xaml.cs`
- Create: `ProxyManager/Helpers/RegistryHelper.cs`

**Interfaces:**
- Consumes: SettingsService (Task 2)
- Produces: Tray icon and auto-start functionality

- [ ] **Step 1: Create RegistryHelper**

```csharp
// ProxyManager/Helpers/RegistryHelper.cs
using Microsoft.Win32;

namespace ProxyManager.Helpers;

public static class RegistryHelper
{
    private const string AutoStartKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ProxyManager";

    public static bool IsAutoStartEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey);
        return key?.GetValue(AppName) != null;
    }

    public static void SetAutoStart(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AutoStartKey, true);
        if (key == null) return;

        if (enabled)
        {
            var exePath = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exePath))
            {
                key.SetValue(AppName, $"\"{exePath}\"");
            }
        }
        else
        {
            key.DeleteValue(AppName, false);
        }
    }
}
```

- [ ] **Step 2: Update App.xaml.cs for tray icon**

```csharp
// ProxyManager/App.xaml.cs
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
    private bool _minimizeToTray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        var settings = _settingsService.Load();
        _minimizeToTray = settings.MinimizeToTray;

        // Set auto-start
        RegistryHelper.SetAutoStart(settings.AutoStart);

        // Initialize tray icon
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
```

- [ ] **Step 3: Update MainWindow for minimize to tray**

```csharp
// ProxyManager/MainWindow.xaml.cs
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace ProxyManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
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
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add ProxyManager/App.xaml.cs ProxyManager/MainWindow.xaml.cs ProxyManager/Helpers/RegistryHelper.cs
git commit -m "feat: add system tray and auto-start via registry"
```

---

### Task 9: Settings Dialog

**Covers:** [S2, S6]

**Files:**
- Create: `ProxyManager/Views/SettingsDialog.xaml`
- Create: `ProxyManager/Views/SettingsDialog.xaml.cs`
- Modify: `ProxyManager/MainWindow.xaml` (add settings button)

**Interfaces:**
- Consumes: SettingsViewModel (Task 6)
- Produces: Settings UI

- [ ] **Step 1: Create SettingsDialog.xaml**

```xml
<!-- ProxyManager/Views/SettingsDialog.xaml -->
<Window x:Class="ProxyManager.Views.SettingsDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:ProxyManager.ViewModels"
        Title="设置" 
        Height="400" Width="500"
        WindowStartupLocation="CenterOwner"
        Background="{DynamicResource BackgroundBrush}"
        Style="{StaticResource WindowStyle}">
    
    <Window.DataContext>
        <vm:SettingsViewModel/>
    </Window.DataContext>
    
    <Grid Margin="16">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <ScrollViewer Grid.Row="0">
            <StackPanel>
                <!-- General -->
                <TextBlock Text="常规" 
                           FontSize="16" 
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource PrimaryBrush}"
                           Margin="0,0,0,8"/>
                
                <CheckBox Content="开机自启" 
                          IsChecked="{Binding AutoStart}"
                          Margin="0,0,0,8"/>
                <CheckBox Content="关闭时最小化到托盘" 
                          IsChecked="{Binding MinimizeToTray}"
                          Margin="0,0,0,8"/>
                <CheckBox Content="检查更新" 
                          IsChecked="{Binding CheckUpdates}"
                          Margin="0,0,0,16"/>
                
                <!-- Interface -->
                <TextBlock Text="界面" 
                           FontSize="16" 
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource PrimaryBrush}"
                           Margin="0,0,0,8"/>
                
                <StackPanel Orientation="Horizontal" Margin="0,0,0,8">
                    <TextBlock Text="主题: " 
                               VerticalAlignment="Center"
                               Margin="0,0,8,0"/>
                    <ComboBox ItemsSource="{Binding AvailableThemes}"
                              SelectedItem="{Binding SelectedTheme}"
                              Width="150"/>
                </StackPanel>
                
                <StackPanel Orientation="Horizontal" Margin="0,0,0,16">
                    <TextBlock Text="缩放: " 
                               VerticalAlignment="Center"
                               Margin="0,0,8,0"/>
                    <ComboBox ItemsSource="{Binding ScaleFactors}"
                              SelectedItem="{Binding ScaleFactor}"
                              Width="100"/>
                </StackPanel>
                
                <!-- Scan Paths -->
                <TextBlock Text="自定义扫描路径" 
                           FontSize="16" 
                           FontWeight="SemiBold"
                           Foreground="{DynamicResource PrimaryBrush}"
                           Margin="0,0,0,8"/>
                
                <ItemsControl ItemsSource="{Binding CustomScanPaths}">
                    <ItemsControl.ItemTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" Margin="0,0,0,4">
                                <TextBlock Text="{Binding}" 
                                           VerticalAlignment="Center"
                                           Margin="0,0,8,0"/>
                                <Button Content="移除"
                                        Command="{Binding DataContext.RemoveScanPathCommand, 
                                            RelativeSource={RelativeSource AncestorType=Window}}"
                                        CommandParameter="{Binding}"
                                        Style="{StaticResource PrimaryButtonStyle}"
                                        Padding="8,4"/>
                            </StackPanel>
                        </DataTemplate>
                    </ItemsControl.ItemTemplate>
                </ItemsControl>
            </StackPanel>
        </ScrollViewer>
        
        <!-- Buttons -->
        <StackPanel Grid.Row="1" 
                    Orientation="Horizontal" 
                    HorizontalAlignment="Right"
                    Margin="0,16,0,0">
            <Button Content="取消"
                    Click="Cancel_Click"
                    Style="{StaticResource PrimaryButtonStyle}"
                    Margin="0,0,8,0"/>
            <Button Content="保存"
                    Command="{Binding SaveCommand}"
                    Click="Save_Click"
                    Style="{StaticResource PrimaryButtonStyle}"/>
        </StackPanel>
    </Grid>
</Window>
```

- [ ] **Step 2: Create SettingsDialog.xaml.cs**

```csharp
// ProxyManager/Views/SettingsDialog.xaml.cs
using System.Windows;

namespace ProxyManager.Views;

public partial class SettingsDialog : Window
{
    public SettingsDialog()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

- [ ] **Step 4: Commit**

```bash
git add ProxyManager/Views/
git commit -m "feat: add settings dialog"
```

---

### Task 10: Final Integration & Testing

**Covers:** [S2, S9]

**Files:**
- Modify: `ProxyManager/App.xaml.cs` (theme initialization)
- Modify: `ProxyManager/MainWindow.xaml` (wiring)
- Modify: `ProxyManager/MainWindow.xaml.cs` (wiring)

**Interfaces:**
- Consumes: All previous tasks
- Produces: Working application

- [ ] **Step 1: Update App.xaml.cs for theme initialization**

```csharp
// ProxyManager/App.xaml.cs
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

        // Apply saved theme
        _themeManager.SetTheme(settings.Theme);

        // Set auto-start
        RegistryHelper.SetAutoStart(settings.AutoStart);

        // Initialize tray icon
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
```

- [ ] **Step 2: Update MainWindow to use DI**

```csharp
// ProxyManager/MainWindow.xaml.cs
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
        
        // Auto-scan on startup
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
```

- [ ] **Step 3: Build and run**

Run: `dotnet build ProxyManager/ProxyManager.csproj`
Expected: Build succeeded

Run: `dotnet run --project ProxyManager/ProxyManager.csproj`
Expected: Application window appears with theme

- [ ] **Step 4: Final commit**

```bash
git add ProxyManager/App.xaml.cs ProxyManager/MainWindow.xaml.cs
git commit -m "feat: complete integration and initialization"
```

---

## Summary

This plan implements a WPF proxy manager application with:

1. **Core Infrastructure** - Project setup, models, settings
2. **Theme System** - 5 built-in themes (Minimal, SciFi, Cyberpunk, Romantic, Elegant)
3. **Scanner Service** - Auto-discover proxy software from known directories
4. **PE Analysis** - Identify main executables vs auxiliary files
5. **Launcher Service** - One-click launch with admin elevation support
6. **MVVM Architecture** - Clean separation of concerns
7. **System Integration** - Tray icon, auto-start, minimize to tray
8. **Settings UI** - Theme selection, scale factor, custom paths

**Total Tasks:** 10
**Estimated Time:** 2-3 hours for experienced developer
**Dependencies:** .NET 8 SDK, Windows 10 LTSC
