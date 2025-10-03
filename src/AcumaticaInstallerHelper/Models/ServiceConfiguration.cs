using System.Text.RegularExpressions;

namespace AcumaticaInstallerHelper.Models;

/// <summary>
///     Runtime configuration for site operations
/// </summary>
public record SiteConfiguration
{
    public required SiteAction       Action       { get; init; }
    public required string           SiteName     { get; init; }
    public required string           SitePath     { get; init; }
    public required AcumaticaVersion Version      { get; init; }
    public          bool             IsPortal     { get; init; }
    public          SiteType         SiteType     { get; init; } = SiteType.NotSet;
    public          bool             IsPreview    { get; init; }
    public          bool             ForceInstall { get; init; }
}

/// <summary>
///     Runtime configuration for version operations
/// </summary>
public record VersionConfiguration
{
    public required AcumaticaVersion Version           { get; init; }
    public required string           VersionPath       { get; init; }
    public          bool             InstallDebugTools { get; init; }
    public          bool             ForceInstall      { get; init; }
}

/// <summary>
///     Runtime configuration for patch operations
/// </summary>
public record PatchConfiguration
{
    public required PatchAction      Action      { get; init; }
    public required string           SiteName    { get; init; }
    public required string           SitePath    { get; init; }
    public required AcumaticaVersion Version     { get; init; }
    public          string?          BackupPath  { get; init; }
    public          string?          ArchivePath { get; init; }
}
public enum SiteType
{
    NotSet,
    Production,
    Development
}

public enum PatchAction
{
    Check,
    Patch,
    Rollback
}

public enum SiteAction
{
    Unknown,
    NewInstance,
    DBMaint,
    DBConnection,
    CompanyConfig,
    NewTrainingInstance,
    DeleteSite,
    RenameSite,
    UpgradeSite
}

public class AcumaticaVersion
{
    public string Version
    {
        set
        {
            string version = value?.Trim() ?? string.Empty;
            ValidateVersionFormat(version);
            MinorVersion = version;
        }
    }

    public string MajorVersion => GetMajorVersion(MinorVersion);
    public string MinorVersion { get; private set; } = string.Empty;

    public                  bool      IsPreview         { get; set; }
    public                  bool      InstallNewVersion { get; init; }
    public                  DateTime? InstallDate       { get; set; }
    public                  long?     Size              { get; set; }
    public                  string    Path              { get; set; } = string.Empty;
    public                  bool      DebuggerTools     { get; set; }
    private static readonly Regex     VersionRegex = new(@"^\d{2}\.\d{3}\.\d{4}$", RegexOptions.Compiled);

    public void ValidateVersionFormat(string version)
    {
        if (!VersionRegex.IsMatch(version)) throw new VersionFormatException(version);
    }

    private static string GetMajorVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return string.Empty;

        string[] parts        = version.Split('.');
        int      minor        = int.Parse(parts[1]);
        var      roundedMinor = (int)Math.Round(minor / 100.0);
        var      majorVersion = $"{parts[0]}.{roundedMinor}";
        return majorVersion;
    }
}