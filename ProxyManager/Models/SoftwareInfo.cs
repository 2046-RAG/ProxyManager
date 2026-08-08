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
