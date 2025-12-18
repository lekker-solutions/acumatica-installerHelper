using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface ISiteService
{
    bool CreateSite(SiteConfiguration siteConfig);
    bool RemoveSite(SiteConfiguration siteName);
    bool UpdateSite(SiteConfiguration siteName);
    bool RequiresAdministratorPrivileges();
}