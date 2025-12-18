using System.Text.Json.Serialization;

namespace AcumaticaInstallerHelper.Models;

/// <summary>
///     Module configuration defaults loaded from JSON
/// </summary>
public class ModuleConfiguration
{
    [JsonPropertyName("AcumaticaDir")]
    public string AcumaticaDirectory { get; set; } = @"C:\Acumatica";

    [JsonPropertyName("AcumaticaSiteDir")]
    public string SiteDirectory { get; set; } = "Sites";

    [JsonPropertyName("AcumaticaVersionDir")]
    public string VersionDirectory { get; set; } = "Versions";

    [JsonPropertyName("SiteType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SiteType DefaultSiteType { get; set; } = SiteType.Production;

    [JsonPropertyName("InstallDebugTools")]
    public bool InstallDebugTools { get; set; } = false;
}