using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface IVersionService
{
    bool                   InstallVersion(VersionConfiguration config);
    bool                   RemoveVersion(VersionConfiguration  config);
    List<AcumaticaVersion> GetAvailableVersions(string?       majorRelease = null, bool preview = false);
    List<AcumaticaVersion> GetInstalledVersions();
    bool                   IsVersionInstalled(AcumaticaVersion    version);
    string                 GetVersionPath(AcumaticaVersion        version);
    string                 GetAcuExePath(AcumaticaVersion         version);
    string                 GetPatchUtilityPath(AcumaticaVersion   version);
}
