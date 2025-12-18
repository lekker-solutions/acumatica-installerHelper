using System;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services.AcArgBuilders;

public class RenameSiteArgBuilder : IAcArgBuilder
{
    public IEnumerable<string> BuildArgs(SiteConfiguration siteConfig)
    {
        return new List<string>
        {
            "-configmode:RenameSite",
            $"-sitename:\"{siteConfig.SiteName}\""
        };
    }
}
