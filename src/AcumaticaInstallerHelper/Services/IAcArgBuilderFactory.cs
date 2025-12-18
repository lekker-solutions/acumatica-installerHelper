#region #Copyright

// ----------------------------------------------------------------------------------
//   COPYRIGHT (c) 2025 CONTOU CONSULTING
//   ALL RIGHTS RESERVED
//   AUTHOR: Kyle Vanderstoep
//   CREATED DATE: 2025/09/29
// ----------------------------------------------------------------------------------

#endregion

using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services
{
    public interface IAcArgBuilderFactory
    {
        public IAcArgBuilder Create(SiteConfiguration configuration);
    }
}