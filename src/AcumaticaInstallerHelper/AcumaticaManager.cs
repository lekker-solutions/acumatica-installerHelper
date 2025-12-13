using AcumaticaInstallerHelper.Models;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper;

public class AcumaticaManager
{
    private readonly IVersionService       _versionService;
    private readonly ISiteService          _siteService;
    private readonly IConfigurationService _configService;
    private readonly ILoggingService       _loggingService;
    private readonly IPatchService         _patchService;
    private readonly ISiteRegistryService  _siteRegistryService;

    public AcumaticaManager(
        IVersionService       versionService,
        ISiteService          siteService,
        IConfigurationService configService,
        ILoggingService       loggingService,
        IPatchService         patchService,
        ISiteRegistryService  siteRegistryService)
    {
        _versionService      = versionService;
        _siteService         = siteService;
        _configService       = configService;
        _loggingService      = loggingService;
        _patchService        = patchService;
        _siteRegistryService = siteRegistryService;
    }

    // Version Management
    public bool InstallVersion(AcumaticaVersion version)
    {
        return InstallVersion(version, false);
    }

    public bool InstallVersion(AcumaticaVersion version, bool force)
    {
        var versionPath = Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetVersionDirectory(),
            version.MinorVersion
        );
        
        var config = new VersionConfiguration
        {
            Version = version,
            VersionPath = versionPath,
            InstallDebugTools = _configService.GetInstallDebugTools(),
            ForceInstall = force || version.InstallNewVersion
        };
        
        return _versionService.InstallVersion(config);
    }

    public bool RemoveVersion(AcumaticaVersion version)
    {
        var versionPath = Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetVersionDirectory(),
            version.MinorVersion
        );
        
        var config = new VersionConfiguration
        {
            Version = version,
            VersionPath = versionPath,
            InstallDebugTools = false,
            ForceInstall = false
        };
        
        return _versionService.RemoveVersion(config);
    }

    public List<AcumaticaVersion> GetInstalledVersions()
    {
        return _versionService.GetInstalledVersions();
    }

    public List<AcumaticaVersion> GetAvailableVersions(string? majorRelease = null, bool preview = false)
    {
        return _versionService.GetAvailableVersions(majorRelease, preview);
    }

    public bool IsVersionInstalled(AcumaticaVersion version)
    {
        return _versionService.IsVersionInstalled(version);
    }

    // Site Management
    public bool CreateSite(SiteConfiguration siteConfig)
    {
        // Set defaults if not provided
        if (siteConfig.SiteType == SiteType.NotSet)
            siteConfig = siteConfig with { SiteType = _configService.GetDefaultSiteType() };

        if (string.IsNullOrEmpty(siteConfig.SitePath))
        {
            string sitePath = Path.Combine(
                _configService.GetAcumaticaDirectory(),
                _configService.GetSiteDirectory(),
                siteConfig.SiteName
            );
            siteConfig = siteConfig with { SitePath = sitePath };
        }

        return _siteService.CreateSite(siteConfig);
    }

    public bool RemoveSite(string siteName)
    {
        string sitePath = _siteRegistryService.GetSitePath(siteName) ?? Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetSiteDirectory(),
            siteName
        );

        return _siteService.RemoveSite(new SiteConfiguration
        {
            Action       = SiteAction.DeleteSite,
            SiteName     = siteName,
            SitePath     = sitePath,
            Version      = new AcumaticaVersion(),
            ForceInstall = false
        });
    }

    public bool UpdateSite(string siteName, string newVersion)
    {
        string sitePath = _siteRegistryService.GetSitePath(siteName) ?? Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetSiteDirectory(),
            siteName
        );

        return _siteService.UpdateSite(new SiteConfiguration
        {
            Action       = SiteAction.UpgradeSite,
            SiteName     = siteName,
            SitePath     = sitePath,
            Version = new AcumaticaVersion
            {
                Version = newVersion
            },
            ForceInstall = false
        });
    }

    public List<string> GetInstalledSites()
    {
        return _siteRegistryService.GetInstalledSites();
    }

    public string? GetSiteVersion(string siteName)
    {
        return _siteRegistryService.GetSiteVersion(siteName);
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

    public bool RequiresAdministratorPrivileges()
    {
        return _siteService.RequiresAdministratorPrivileges();
    }

    // Patch Management
    public PatchCheckResult CheckForPatches(string siteName)
    {
        string? sitePath = _siteRegistryService.GetSitePath(siteName);
        if (string.IsNullOrEmpty(sitePath))
            throw new InvalidOperationException($"Could not find site path for: {siteName}");

        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }

        PatchConfiguration patchConfig = new()
        {
            Action   = PatchAction.Check,
            SiteName = siteName,
            SitePath = sitePath,
            Version  = new AcumaticaVersion { Version = version }
        };

        return _patchService.CheckForPatches(patchConfig);
    }

    public PatchResult ApplyPatch(string siteName, string? sitePath = null, string? backupPath = null)
    {
        sitePath ??= _siteRegistryService.GetSitePath(siteName);
        if (string.IsNullOrEmpty(sitePath))
            throw new InvalidOperationException($"Could not find site path for: {siteName}");

        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }

        PatchConfiguration patchConfig = new()
        {
            Action     = PatchAction.Patch,
            SiteName   = siteName,
            SitePath   = sitePath,
            Version    = new AcumaticaVersion { Version = version },
            BackupPath = backupPath
        };

        return _patchService.ApplyPatch(patchConfig, backupPath);
    }

    public PatchResult ApplyPatchFromArchive(string siteName, string archivePath, string? backupPath = null)
    {
        string? sitePath = _siteRegistryService.GetSitePath(siteName);
        if (string.IsNullOrEmpty(sitePath))
            throw new InvalidOperationException($"Could not find site path for: {siteName}");

        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }

        PatchConfiguration patchConfig = new()
        {
            Action      = PatchAction.Patch,
            SiteName    = siteName,
            SitePath    = sitePath,
            Version     = new AcumaticaVersion { Version = version },
            BackupPath  = backupPath,
            ArchivePath = archivePath
        };

        return _patchService.ApplyPatchFromArchive(patchConfig, archivePath, backupPath);
    }

    public PatchResult RollbackPatch(string siteName, string? backupPath = null)
    {
        string? sitePath = _siteRegistryService.GetSitePath(siteName);
        if (string.IsNullOrEmpty(sitePath))
            throw new InvalidOperationException($"Could not find site path for: {siteName}");

        var version = GetSiteVersion(siteName);
        if (string.IsNullOrEmpty(version))
        {
            throw new InvalidOperationException($"Could not determine version for site: {siteName}");
        }

        PatchConfiguration patchConfig = new()
        {
            Action     = PatchAction.Rollback,
            SiteName   = siteName,
            SitePath   = sitePath,
            Version    = new AcumaticaVersion { Version = version },
            BackupPath = backupPath
        };

        return _patchService.RollbackPatch(patchConfig, backupPath);
    }

    public bool IsPatchToolAvailable(string version)
    {
        AcumaticaVersion acumaticaVersion = new() { Version = version };
        return _patchService.IsPatchToolAvailable(acumaticaVersion);
    }
}