using System.Text.Json.Serialization;

namespace AcumaticaInstallerHelper.Models;

public class AcumaticaConfig
{
    [JsonPropertyName("AcumaticaDir")]
    public string AcumaticaDirectory { get; set; } = @"C:\Acumatica";

    [JsonPropertyName("AcumaticaSiteDir")]
    public string SiteDirectory { get; set; } = "Sites";

    [JsonPropertyName("AcumaticaVersionDir")]
    public string VersionDirectory { get; set; } = "Versions";

    [JsonPropertyName("SiteType")]
    public string SiteType { get; set; } = "Production";

    [JsonPropertyName("InstallDebugTools")]
    public bool InstallDebugTools { get; set; } = false;
}

public enum SiteType
{
    Production,
    Development
}

public class AcumaticaVersion
{
    public string Version { get; set; } = string.Empty;
    public string MajorVersion => GetMajorVersion(Version);
    public string MinorVersion => Version;
    public bool IsPreview { get; set; }
    public DateTime? InstallDate { get; set; }
    public long? Size { get; set; }
    public string Path { get; set; } = string.Empty;

    private static string GetMajorVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return string.Empty;
        
        var parts = version.Split('.');
        if (parts.Length >= 2)
        {
            return $"{parts[0]}.{parts[1]}";
        }
        return version;
    }
}

public class SiteConfiguration
{
    public string SiteName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public bool IsPortal { get; set; }
    public SiteType SiteType { get; set; } = Models.SiteType.Production;
    public bool IsPreview { get; set; }
}

public class SiteInstallOptions
{
    public bool InstallNewVersion { get; set; }
    public bool Portal { get; set; }
    public SiteType? SiteType { get; set; }
    public bool Preview { get; set; }
    public bool DebuggerTools { get; set; }
}