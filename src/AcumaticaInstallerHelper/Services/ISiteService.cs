using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface ISiteService
{
    bool CreateSite(SiteConfiguration siteConfig, SiteInstallOptions options);
    bool RemoveSite(string siteName);
    bool UpdateSite(string siteName, string newVersion);
    List<string> GetInstalledSites();
    string? GetSiteVersion(string siteName);
    bool RequiresAdministratorPrivileges();
}