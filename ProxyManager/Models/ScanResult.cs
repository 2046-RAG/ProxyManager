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
