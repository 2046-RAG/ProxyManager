using System.Diagnostics;
using System.Reflection.PortableExecutable;

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
