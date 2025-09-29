using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services.AcArgBuilders;

public class UpgradeSiteArgBuilder : IAcArgBuilder
{
    public IEnumerable<string> BuildArgs(SiteConfiguration siteConfig)
    {
        return new List<string>
        {
            "-configmode:UpgradeSite",
            $"-sitename:\"{siteConfig.SiteName}\"",
            $"-targetversion:\"{siteConfig.Version.MinorVersion}\""
        };
    }
}