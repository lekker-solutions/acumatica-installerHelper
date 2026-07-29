using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface ISqlAccessService
{
    /// <summary>
    ///     Grants the site's IIS application pool identity (IIS APPPOOL\&lt;pool&gt;)
    ///     a Windows login on the SQL Server and db_owner membership on the
    ///     site database, so the site can connect with integrated security.
    /// </summary>
    bool GrantAppPoolAccess(SiteConfiguration siteConfig);
}
