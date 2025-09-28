using AcumaticaInstallerHelper.Models;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper;

public class AcumaticaManager
{
    private readonly IVersionService _versionService;
    private readonly ISiteService _siteService;
    private readonly IConfigurationService _configService;
    private readonly ILoggingService _loggingService;
    private readonly IPatchService _patchService;

    public AcumaticaManager(
        IVersionService versionService,
        ISiteService siteService,
        IConfigurationService configService,
        ILoggingService loggingService,
        IPatchService patchService)
    {
        _versionService = versionService;
        _siteService = siteService;
        _configService = configService;
        _loggingService = loggingService;
        _patchService = patchService;
    }

    // Version Management
    public bool InstallVersion(string version, bool isPreview = false, bool installDebugTools = false)
    {
        return _versionService.InstallVersion(version, isPreview, installDebugTools);
    }

    public bool RemoveVersion(string version)
    {
        return _versionService.RemoveVersion(version);
    }

    public List<AcumaticaVersion> GetInstalledVersions()
    {
        return _versionService.GetInstalledVersions();
    }

    public List<AcumaticaVersion> GetAvailableVersions(string? majorRelease = null, bool preview = false)
    {
        return _versionService.GetAvailableVersions(majorRelease, preview);
    }

    public bool IsVersionInstalled(string version)
    {
        return _versionService.IsVersionInstalled(version);
    }

    // Site Management
    public bool CreateSite(string version, string siteName, string? siteInstallPath = null, SiteInstallOptions? options = null)
    {
        options ??= new SiteInstallOptions();
        
        var siteConfig = new SiteConfiguration
        {
            Version = version,
            SiteName = siteName,
            InstallPath = siteInstallPath ?? string.Empty,
            IsPortal = options.Portal,
            SiteType = options.SiteType ?? _configService.GetDefaultSiteType(),
            IsPreview = options.Preview
        };

        return _siteService.CreateSite(siteConfig, options);
    }

    public bool RemoveSite(string siteName)
    {
        return _siteService.RemoveSite(siteName);
    }

    public bool UpdateSite(string siteName, string newVersion)
    {
        return _siteService.UpdateSite(siteName, newVersion);
    }

    public List<string> GetInstalledSites()
    {
        return _siteService.GetInstalledSites();
    }

    public string? GetSiteVersion(string siteName)
    {
        return _siteService.GetSiteVersion(siteName);
    }

    // Configuration Management
    public string GetAcumaticaDirectory()
    {
        return _configService.GetAcumaticaDirectory();
    }

    public void SetAcumaticaDirectory(string path)
    {
        _configService.SetAcumaticaDirectory(path);
    }

    public string GetSiteDirectory()
    {
        return _configService.GetSiteDirectory();
    }

    public void SetSiteDirectory(string directory)
    {
        _configService.SetSiteDirectory(directory);
    }

    public string GetVersionDirectory()
    {
        return _configService.GetVersionDirectory();
    }

    public void SetVersionDirectory(string directory)
    {
        _configService.SetVersionDirectory(directory);
    }

    public SiteType GetDefaultSiteType()
    {
        return _configService.GetDefaultSiteType();
    }

    public void SetDefaultSiteType(SiteType siteType)
    {
        _configService.SetDefaultSiteType(siteType);
    }

    public bool GetInstallDebugTools()
    {
        return _configService.GetInstallDebugTools();
    }

    public void SetInstallDebugTools(bool install)
    {
        _configService.SetInstallDebugTools(install);
    }

    // Utility Methods
    public void ValidateVersionFormat(string version)
    {
        _versionService.ValidateVersionFormat(version);
    }

    public bool RequiresAdministratorPrivileges()
    {
        return _siteService.RequiresAdministratorPrivileges();
    }

    // Patch Management
    public PatchCheckResult CheckForPatches(string siteName)
    {
        var sitePath = GetSitePath(siteName);
        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }
        return _patchService.CheckForPatches(sitePath, version);
    }

    public PatchResult ApplyPatch(string siteName, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }
        return _patchService.ApplyPatch(sitePath, version, backupPath);
    }

    public PatchResult ApplyPatchFromArchive(string siteName, string archivePath, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }
        return _patchService.ApplyPatchFromArchive(sitePath, archivePath, version, backupPath);
    }

    public PatchResult RollbackPatch(string siteName, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }
        return _patchService.RollbackPatch(sitePath, version, backupPath);
    }

    public bool IsPatchToolAvailable(string version)
    {
        return _patchService.IsPatchToolAvailable(version);
    }

    private string GetSitePath(string siteName)
    {
        return Path.Combine(_configService.GetSiteDirectory(), siteName);
    }
}