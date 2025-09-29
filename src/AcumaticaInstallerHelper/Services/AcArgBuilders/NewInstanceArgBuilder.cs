using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services.AcArgBuilders;

public class NewInstanceArgBuilder : IAcArgBuilder
{
    public IEnumerable<string> BuildArgs(SiteConfiguration siteConfig)
    {
        var args = new List<string>
        {
            "-configmode:NewInstance",
            $"-sitename:\"{siteConfig.SiteName}\"",
            $"-sitepath:\"{siteConfig.SitePath}\""
        };

        if (siteConfig.IsPortal)
        {
            args.Add("-portal");
        }

        if (siteConfig.SiteType == SiteType.Development)
        {
            args.Add("-developmentmode");
        }

        return args;
    }
}