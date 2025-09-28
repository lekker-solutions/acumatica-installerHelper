using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface IVersionService
{
    bool InstallVersion(string version, bool isPreview = false, bool installDebugTools = false);
    bool RemoveVersion(string version);
    List<AcumaticaVersion> GetAvailableVersions(string? majorRelease = null, bool preview = false);
    List<AcumaticaVersion> GetInstalledVersions();
    bool IsVersionInstalled(string version);
    string GetVersionPath(string version);
    string GetAcuExePath(string version);
    string GetPatchUtilityPath(string version);
    void ValidateVersionFormat(string version);
}

public class VersionFormatException : Exception
{
    public VersionFormatException(string version) 
        : base($"Version '{version}' is invalid. Expected format is ##.###.####")
    {
    }
}