#region #Copyright

// ----------------------------------------------------------------------------------
//   COPYRIGHT (c) 2025 CONTOU CONSULTING
//   ALL RIGHTS RESERVED
//   AUTHOR: Kyle Vanderstoep
//   CREATED DATE: 2025/09/29
// ----------------------------------------------------------------------------------

#endregion

using AcumaticaInstallerHelper.Models;
using AcumaticaInstallerHelper.Services.AcArgBuilders;

namespace AcumaticaInstallerHelper.Services
{
    public class AcArgFactory : IAcArgBuilderFactory
    {
        public IAcArgBuilder Create(SiteConfiguration configuration)
        {
            return configuration.Action switch
            {
                SiteAction.NewInstance => new NewInstanceArgBuilder(),
                SiteAction.DeleteSite => new DeleteSiteArgBuilder(),
                SiteAction.UpgradeSite => new UpgradeSiteArgBuilder(),
                _ => throw new NotSupportedException($"Site action {configuration.Action} is not yet supported")
            };
        }
    }
}