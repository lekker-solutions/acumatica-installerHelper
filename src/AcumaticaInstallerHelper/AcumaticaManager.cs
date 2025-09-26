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
    public async Task<bool> InstallVersionAsync(string version, bool isPreview = false, bool installDebugTools = false)
    {
        return await _versionService.InstallVersionAsync(version, isPreview, installDebugTools);
    }

    public async Task<bool> RemoveVersionAsync(string version)
    {
        return await _versionService.RemoveVersionAsync(version);
    }

    public List<AcumaticaVersion> GetInstalledVersions()
    {
        return _versionService.GetInstalledVersions();
    }

    public async Task<List<AcumaticaVersion>> GetAvailableVersionsAsync(string? majorRelease = null, bool preview = false)
    {
        return await _versionService.GetAvailableVersionsAsync(majorRelease, preview);
    }

    public bool IsVersionInstalled(string version)
    {
        return _versionService.IsVersionInstalled(version);
    }

    // Site Management
    public async Task<bool> CreateSiteAsync(string version, string siteName, string? siteInstallPath = null, SiteInstallOptions? options = null)
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

        return await _siteService.CreateSiteAsync(siteConfig, options);
    }

    public async Task<bool> RemoveSiteAsync(string siteName)
    {
        return await _siteService.RemoveSiteAsync(siteName);
    }

    public async Task<bool> UpdateSiteAsync(string siteName, string newVersion)
    {
        return await _siteService.UpdateSiteAsync(siteName, newVersion);
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
    public async Task<PatchCheckResult> CheckForPatchesAsync(string siteName)
    {
        var sitePath = GetSitePath(siteName);
        return await _patchService.CheckForPatchesAsync(sitePath);
    }

    public async Task<PatchResult> ApplyPatchAsync(string siteName, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        return await _patchService.ApplyPatchAsync(sitePath, backupPath);
    }

    public async Task<PatchResult> ApplyPatchFromArchiveAsync(string siteName, string archivePath, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        return await _patchService.ApplyPatchFromArchiveAsync(sitePath, archivePath, backupPath);
    }

    public async Task<PatchResult> RollbackPatchAsync(string siteName, string? backupPath = null)
    {
        var sitePath = GetSitePath(siteName);
        return await _patchService.RollbackPatchAsync(sitePath, backupPath);
    }

    public async Task<bool> IsPatchToolAvailableAsync()
    {
        return await _patchService.IsPatchToolAvailableAsync();
    }

    private string GetSitePath(string siteName)
    {
        return Path.Combine(_configService.GetSiteDirectory(), siteName);
    }
}