# Proxy Manager Design Specification

## [S1] Problem

Users who use multiple proxy/VPN software on Windows need a centralized launcher that:
- Automatically discovers installed proxy software
- Identifies the main executable from auxiliary files
- Provides one-click launch capability
- Offers a visually appealing, customizable interface

Current pain points:
- Proxy software scattered across different directories
- Hard to identify main exe vs helper processes
- No unified launcher with theme customization
- Manual configuration required for each software

## [S2] Solution Overview

A WPF desktop application (C#/.NET 8) that scans for common proxy software, analyzes executables, and provides a themed launcher interface with system tray integration.

### Core Features

1. **Auto-Scan & Discovery**
   - Built-in database of 15+ common proxy software with metadata
   - Scan common installation directories and PATH
   - File system watcher for real-time detection

2. **Executable Analysis**
   - PE header analysis to identify main executables
   - Signature verification (authenticode)
   - Process relationship mapping (parent-child)
   - Highlight main executables, allow hiding auxiliary ones

3. **Launch Interface**
   - One-click launch buttons for each discovered software
   - Auto-read exe metadata (icon, version, description)
   - Custom button creation for user-added software

4. **Theme System**
   - 5+ built-in themes with distinct visual styles
   - XAML-based theme switching
   - Support for sci-fi, cyberpunk, romantic, elegant, minimal styles

5. **System Features**
   - Auto-start on Windows boot (registry)
   - Run as administrator (UAC elevation)
   - Minimize to system tray
   - Window proportional scaling

## [S3] Technology Stack

### Primary: WPF + .NET 8

**Why WPF:**
- Native Windows 10 LTSC support
- XAML declarative UI perfect for themes
- Built-in support for:
  - System tray (NotifyIcon)
  - Window scaling
  - Admin elevation
  - Registry operations
- Single exe deployment with self-contained runtime

**Alternative considered:**
- Tauri (Rust + Web): Smaller binary but less native Windows integration
- Electron: Too heavy (~150MB+)
- PyQt: Requires Python runtime

## [S4] Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ProxyManager App                         │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  UI Layer   │  │ Theme Engine│  │   System Services   │ │
│  │  (XAML)     │  │             │  │                     │ │
│  │             │  │ - Sci-Fi    │  │ - Tray Icon         │ │
│  │ - MainWindow│  │ - Cyberpunk │  │ - Auto-Start        │ │
│  │ - Cards     │  │ - Romantic  │  │ - Admin Elevation   │ │
│  │ - Buttons   │  │ - Elegant   │  │ - File Watcher      │ │
│  │ - Settings  │  │ - Minimal   │  │                     │ │
│  └──────┬──────┘  └─────────────┘  └──────────┬──────────┘ │
│         │                                      │            │
│  ┌──────▼──────────────────────────────────────▼──────────┐ │
│  │                    Core Services                       │ │
│  │                                                        │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │ │
│  │  │   Scanner    │  │   Analyzer   │  │   Launcher   │ │ │
│  │  │              │  │              │  │              │ │ │
│  │  │ - Directory  │  │ - PE Header  │  │ - Process    │ │ │
│  │  │ - File Type  │  │ - Signature  │  │ - Elevation  │ │ │
│  │  │ - Watcher    │  │ - Relation   │  │ - Callback   │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │ │
│  │                                                        │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │              Software Database                   │  │ │
│  │  │                                                  │  │ │
│  │  │  Built-in: 15+ proxy software with metadata     │  │ │
│  │  │  User-added: Custom entries                      │  │ │
│  │  │  Scan results: Discovered instances              │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## [S5] Component Details

### 5.1 Software Database

Built-in proxy software list with metadata:

| Software | GitHub | Typical Path |
|----------|--------|--------------|
| Clash Verge Rev | github.com/clash-verge-rev/clash-verge-rev | %LOCALAPPDATA%\clash-verge-rev |
| v2rayN | github.com/2dust/v2rayN | %USERPROFILE%\v2rayN |
| Shadowsocks Windows | github.com/shadowsocks/shadowsocks-windows | %APPDATA%\shadowsocks |
| Hiddify | github.com/hiddify/hiddify-app | %LOCALAPPDATA%\Hiddify |
| Nekoray | github.com/MatsuriDayo/nekoray | %USERPROFILE%\nekoray |
| sing-box | github.com/SagerNet/sing-box | Custom path |
| mihomo (Clash Meta) | github.com/MetaCubeX/mihomo | Custom path |
| ClashN | github.com/2dust/ClashN | %USERPROFILE%\ClashN |
| Qv2ray | github.com/Qv2ray/Qv2ray | %LOCALAPPDATA%\Qv2ray |
| V2rayA | github.com/v2rayA/v2rayA | %LOCALAPPDATA%\v2raya |

*Each entry includes: GitHub URL, download URL, latest version, update date, typical install directory*

### 5.2 Scanner Module

```csharp
public interface IProxyScanner
{
    // Scan predefined directories
    Task<List<DiscoveredSoftware>> ScanAllAsync();
    
    // Scan specific directory
    Task<List<DiscoveredSoftware>> ScanDirectoryAsync(string path);
    
    // Watch for changes
    IDisposable WatchDirectories(Action<SoftwareChange> callback);
}

public class DiscoveredSoftware
{
    public string Name { get; set; }
    public string ExePath { get; set; }
    public string Version { get; set; }
    public DateTime LastModified { get; set; }
    public SoftwareMetadata Metadata { get; set; }
    public bool IsMainExecutable { get; set; }
    public bool IsHidden { get; set; }
}
```

### 5.3 Analyzer Module

```csharp
public interface IExecutableAnalyzer
{
    // Analyze PE header
    PEInfo AnalyzePEHeader(string exePath);
    
    // Verify digital signature
    SignatureInfo VerifySignature(string exePath);
    
    // Determine if main executable
    bool IsMainExecutable(string exePath, List<string> relatedFiles);
    
    // Find related executables
    List<string> FindRelatedExecutables(string directory);
}

public class PEInfo
{
    public string Description { get; set; }
    public string Company { get; set; }
    public string ProductName { get; set; }
    public Version FileVersion { get; set; }
    public Icon Icon { get; set; }
}
```

### 5.4 Theme System

XAML ResourceDictionary-based theming:

```xml
<!-- Themes/SciFiTheme.xaml -->
<ResourceDictionary>
    <Color x:Key="PrimaryColor">#00FF88</Color>
    <Color x:Key="BackgroundColor">#0A0A0F</Color>
    <Color x:Key="AccentColor">#FF00FF</Color>
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <!-- ... -->
</ResourceDictionary>
```

Theme implementations:
1. **Sci-Fi** - Neon greens/purples, dark background, glowing effects
2. **Cyberpunk** - Yellow/cyan on dark, glitch effects, angular elements
3. **Romantic** - Soft pinks/rose gold, rounded shapes, elegant typography
4. **Elegant** - Dark/gold, minimal, professional
5. **Minimal** - Light/dark, clean lines, no effects

### 5.5 System Services

```csharp
public interface ISystemService
{
    // Tray icon management
    void ShowTrayIcon();
    void HideTrayIcon();
    
    // Auto-start
    bool IsAutoStartEnabled();
    void SetAutoStart(bool enabled);
    
    // Admin elevation
    bool IsRunningAsAdmin();
    void ElevateToAdmin();
    
    // Window scaling
    void SetScaleFactor(double factor);
}
```

## [S6] User Interface

### Main Window Layout

```
┌─────────────────────────────────────────────────────────────┐
│ ProxyManager                              [_] [□] [X]      │
├─────────────────────────────────────────────────────────────┤
│ [Theme: Sci-Fi ▼]  [⚙ Settings]  [🔍 Scan]  [➕ Add]     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Discovered Software                                        │
│  ┌─────────────────┐ ┌─────────────────┐ ┌───────────────┐ │
│  │  ┌───────────┐  │ │  ┌───────────┐  │ │  ┌─────────┐ │ │
│  │  │   ICON    │  │ │  │   ICON    │  │ │  │  ICON   │ │ │
│  │  └───────────┘  │ │  └───────────┘  │ │  └─────────┘ │ │
│  │  Clash Verge    │ │  v2rayN         │ │  Shadowsocks │ │
│  │  v1.6.6         │ │  v6.23          │ │  v4.1.6      │ │
│  │  [▶ Launch]     │ │  [▶ Launch]     │ │  [▶ Launch]  │ │
│  │  [👁 Hide]      │ │  [👁 Hide]      │ │  [👁 Hide]   │ │
│  └─────────────────┘ └─────────────────┘ └───────────────┘ │
│                                                             │
│  User-Added Software                                        │
│  ┌─────────────────┐ ┌─────────────────┐                   │
│  │  Custom App 1   │ │  Custom App 2   │  [➕ Add Custom]  │
│  │  [▶ Launch]     │ │  [▶ Launch]     │                   │
│  └─────────────────┘ └─────────────────┘                   │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ [🔄 Auto-start: ON]  [👑 Run as Admin]  [📦 Minimize]     │
└─────────────────────────────────────────────────────────────┘
```

### Settings Dialog

```
┌─────────────────────────────────────────────────┐
│ Settings                                    [X] │
├─────────────────────────────────────────────────┤
│                                                 │
│ General                                         │
│ ☑ Start with Windows                           │
│ ☑ Minimize to tray on close                    │
│ ☐ Check for updates                            │
│                                                 │
│ Scanning                                        │
│ Scan directories:                              │
│ [☑] %LOCALAPPDATA%                             │
│ [☑] %APPDATA%                                  │
│ [☐] Custom: C:\MyApps                          │
│                                                 │
│ Interface                                       │
│ Window scale: [100% ▼]                         │
│ Theme: [Sci-Fi ▼]                              │
│                                                 │
│ [Save]  [Cancel]                               │
└─────────────────────────────────────────────────┘
```

## [S7] Data Flow

### Software Discovery Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Start App  │────▶│  Load DB     │────▶│  Scan Dirs   │
└──────────────┘     └──────────────┘     └──────┬───────┘
                                                  │
                                                  ▼
                     ┌──────────────┐     ┌──────────────┐
                     │  Update UI   │◀────│  Analyze     │
                     └──────────────┘     │  Executables │
                                          └──────────────┘
```

### Launch Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Click Launch│────▶│  Check Admin │────▶│  Start Process│
└──────────────┘     │  Requirements│     └──────────────┘
                     └──────────────┘
                            │
                            ▼ (if admin needed)
                     ┌──────────────┐     ┌──────────────┐
                     │  UAC Prompt  │────▶│  Relaunch    │
                     └──────────────┘     └──────────────┘
```

## [S8] Implementation Plan

### Phase 1: Core Infrastructure
- Project setup (.NET 8 WPF)
- Basic window with tray icon
- Settings persistence (JSON)
- Theme system foundation

### Phase 2: Scanner & Analyzer
- Software database
- Directory scanning logic
- PE header analysis
- Executable relationship detection

### Phase 3: UI & Interaction
- Main window layout
- Software cards with launch buttons
- Add custom software dialog
- Hide/show functionality

### Phase 4: System Integration
- Auto-start via registry
- Admin elevation (UAC)
- Window scaling
- File system watcher

### Phase 5: Theme Implementation
- Sci-Fi theme
- Cyberpunk theme
- Romantic theme
- Elegant theme
- Minimal theme

## [S9] Success Criteria

1. Application starts on Windows 10 LTSC without additional runtimes
2. Scans and discovers 80%+ of installed proxy software
3. Correctly identifies main executables vs auxiliary files
4. Launch buttons work for all discovered software
5. All 5 themes render correctly with smooth transitions
6. Auto-start, admin elevation, and tray minimize work reliably
7. Window scales proportionally without UI breaking

## [S10] Future Considerations

- Auto-update checking for discovered software
- Proxy subscription management
- Connection status monitoring
- Portable version (no installation required)
