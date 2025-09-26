using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface ISiteService
{
    Task<bool> CreateSiteAsync(SiteConfiguration siteConfig, SiteInstallOptions options);
    Task<bool> RemoveSiteAsync(string siteName);
    Task<bool> UpdateSiteAsync(string siteName, string newVersion);
    List<string> GetInstalledSites();
    string? GetSiteVersion(string siteName);
    bool RequiresAdministratorPrivileges();
}