using System.IO;
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
