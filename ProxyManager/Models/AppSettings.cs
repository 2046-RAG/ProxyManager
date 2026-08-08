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
