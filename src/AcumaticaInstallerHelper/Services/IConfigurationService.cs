using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface IConfigurationService
{
    AcumaticaConfig GetConfiguration();
    Task SaveConfigurationAsync(AcumaticaConfig config);
    
    string GetAcumaticaDirectory();
    void SetAcumaticaDirectory(string path);
    
    string GetSiteDirectory();
    void SetSiteDirectory(string directory);
    
    string GetVersionDirectory();
    void SetVersionDirectory(string directory);
    
    SiteType GetDefaultSiteType();
    void SetDefaultSiteType(SiteType siteType);
    
    bool GetInstallDebugTools();
    void SetInstallDebugTools(bool install);
    
    string GetDefaultSiteInstallPath();
}