namespace AcumaticaInstallerHelper.Services;

public interface ISiteRegistryService
{
    string       GetSitePath(string siteName);
    List<string> GetInstalledSites();
    bool         SiteExists(string     siteName);
    string       GetSiteVersion(string siteName);
}