using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services.AcArgBuilders;

public class DeleteSiteArgBuilder : IAcArgBuilder
{
    public IEnumerable<string> BuildArgs(SiteConfiguration siteConfig)
    {
        return new List<string>
        {
            "-configmode:DeleteSite",
            $"-sitename:\"{siteConfig.SiteName}\""
        };
    }
}