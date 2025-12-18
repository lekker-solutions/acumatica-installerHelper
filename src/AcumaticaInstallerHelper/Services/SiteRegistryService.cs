using Microsoft.Win32;

namespace AcumaticaInstallerHelper.Services;

public class SiteRegistryService : ISiteRegistryService
{
    private const    string            ACUMATICA_REGISTRY_KEY = @"SOFTWARE\ACUMATICA ERP";
    private readonly ILoggingService   _loggingService;
    private readonly IWebConfigService _webConfigService;

    public SiteRegistryService(ILoggingService loggingService, IWebConfigService webConfigService)
    {
        _loggingService   = loggingService;
        _webConfigService = webConfigService;
    }

    public string GetSitePath(string siteName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey($@"{ACUMATICA_REGISTRY_KEY}\{siteName}");
            if (key == null)
            {
                _loggingService.WriteDebug($"Registry key not found for site: {siteName}");
                return string.Empty;
            }

            var path = key.GetValue("Path") as string;
            if (string.IsNullOrEmpty(path))
            {
                _loggingService.WriteDebug($"Path value not found or empty for site: {siteName}");
                return string.Empty;
            }

            _loggingService.WriteDebug($"Found site path in registry: {path}");
            return path;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to read site path from registry for {siteName}: {ex.Message}");
            return string.Empty;
        }
    }

    public List<string> GetInstalledSites()
    {
        var sites = new List<string>();
        
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(ACUMATICA_REGISTRY_KEY);
            if (key == null)
            {
                _loggingService.WriteDebug("Acumatica ERP registry key not found");
                return sites;
            }

            var subKeyNames = key.GetSubKeyNames();
            foreach (var subKeyName in subKeyNames)
            {
                // Verify it's a valid site by checking if it has a Path value
                using var siteKey = key.OpenSubKey(subKeyName);
                if (siteKey?.GetValue("Path") != null)
                {
                    sites.Add(subKeyName);
                }
            }

            _loggingService.WriteDebug($"Found {sites.Count} sites in registry");
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to enumerate sites from registry: {ex.Message}");
        }

        return sites;
    }

    public bool SiteExists(string siteName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey($@"{ACUMATICA_REGISTRY_KEY}\{siteName}");
            return key != null && key.GetValue("Path") != null;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to check if site exists in registry for {siteName}: {ex.Message}");
            return false;
        }
    }

    public string GetSiteVersion(string siteName)
    {
        string sitePath = GetSitePath(siteName);
        if (string.IsNullOrEmpty(sitePath))
        {
            _loggingService.WriteDebug($"Could not find site path for: {siteName}");
            return string.Empty;
        }

        string webConfigPath = Path.Combine(sitePath, "web.config");
        return _webConfigService.GetSiteVersion(webConfigPath);
    }
}