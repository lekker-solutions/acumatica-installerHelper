using System.Diagnostics;
using System.Security.Principal;
using System.Xml.Linq;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class SiteService : ISiteService
{
    private readonly IVersionService _versionService;
    private readonly IConfigurationService _configService;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<SiteService> _logger;

    public SiteService(
        IVersionService versionService,
        IConfigurationService configService,
        ILoggingService loggingService,
        ILogger<SiteService> logger)
    {
        _versionService = versionService;
        _configService = configService;
        _loggingService = loggingService;
        _logger = logger;
    }

    public bool RequiresAdministratorPrivileges()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return !principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public async Task<bool> CreateSiteAsync(SiteConfiguration siteConfig, SiteInstallOptions options)
    {
        _loggingService.WriteHeader("Acumatica Site Installation", $"Version {siteConfig.Version} • Site: {siteConfig.SiteName}");
        
        try
        {
            _loggingService.WriteSection("Validating Prerequisites");
            
            // Validate version format
            _versionService.ValidateVersionFormat(siteConfig.Version);
            _loggingService.WriteSuccess($"Version format validated: {siteConfig.Version}");
            
            // Check administrator privileges
            if (RequiresAdministratorPrivileges())
            {
                _loggingService.WriteError("Administrator privileges required");
                throw new UnauthorizedAccessException("This operation must be run as Administrator");
            }
            _loggingService.WriteSuccess("Administrator privileges confirmed");

            // Check if version exists
            if (!_versionService.IsVersionInstalled(siteConfig.Version))
            {
                _loggingService.WriteWarning($"Version {siteConfig.Version} not found locally");
                
                bool shouldInstall = options.InstallNewVersion || 
                    _loggingService.PromptYesNo($"You do not have version {siteConfig.Version} installed, do you want to install?");
                
                if (!shouldInstall)
                {
                    _loggingService.WriteError("Site installation cancelled - version not available");
                    return false;
                }
                
                _loggingService.WriteSection("Installing Required Version");
                
                bool useDebugTools = options.DebuggerTools || _configService.GetInstallDebugTools();
                bool installSuccess = await _versionService.InstallVersionAsync(siteConfig.Version, options.Preview, useDebugTools);
                
                if (!installSuccess)
                {
                    _loggingService.WriteError("Failed to install required version");
                    return false;
                }
            }
            else
            {
                _loggingService.WriteSuccess($"Version {siteConfig.Version} found at {_versionService.GetVersionPath(siteConfig.Version)}");
            }

            _loggingService.WriteSection("Configuring Site Parameters");
            
            // Set default site path if not provided
            if (string.IsNullOrWhiteSpace(siteConfig.InstallPath))
            {
                siteConfig.InstallPath = Path.Combine(_configService.GetDefaultSiteInstallPath(), siteConfig.SiteName);
                _loggingService.WriteInfo($"Using default site path: {siteConfig.InstallPath}");
            }
            else
            {
                _loggingService.WriteInfo($"Using specified site path: {siteConfig.InstallPath}");
            }

            // Apply dev configuration based on explicit parameter or config default
            var isDev = options.SiteType == Models.SiteType.Development || 
                       (options.SiteType == null && _configService.GetDefaultSiteType() == Models.SiteType.Development);
            
            _loggingService.WriteTable(new Dictionary<string, string>
            {
                ["Site Name"] = siteConfig.SiteName,
                ["Version"] = siteConfig.Version,
                ["Install Path"] = siteConfig.InstallPath,
                ["Portal Site"] = options.Portal ? "Yes" : "No",
                ["Site Type"] = isDev ? "Development" : "Production",
                ["Preview Build"] = options.Preview ? "Yes" : "No"
            }, "Site Configuration");

            _loggingService.WriteSection("Installing Site");
            
            // Install site
            var success = await ExecuteAcuExeAsync(siteConfig, options);
            
            if (success && isDev)
            {
                _loggingService.WriteStep("Applying development configuration");
                ApplyDevelopmentConfiguration(siteConfig.InstallPath);
            }
            
            if (success)
            {
                _loggingService.WriteSummary("Site Installation", "Completed Successfully", new Dictionary<string, string>
                {
                    ["Site Name"] = siteConfig.SiteName,
                    ["Version"] = siteConfig.Version,
                    ["Path"] = siteConfig.InstallPath,
                    ["Type"] = isDev ? "Development" : "Production",
                    ["Portal"] = options.Portal ? "Yes" : "No"
                });
            }

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to create site: {ex.Message}");
            _logger.LogError(ex, "Failed to create site {SiteName}", siteConfig.SiteName);
            return false;
        }
    }

    public async Task<bool> RemoveSiteAsync(string siteName)
    {
        _loggingService.WriteHeader("Acumatica Site Removal", $"Site: {siteName}");
        
        try
        {
            _loggingService.WriteSection("Removing Site Registration");
            
            var version = GetSiteVersion(siteName);
            if (string.IsNullOrEmpty(version))
            {
                _loggingService.WriteError($"Could not determine version for site: {siteName}");
                return false;
            }
            
            _loggingService.WriteInfo($"Found site using version: {version}");
            
            var success = await ExecuteAcuExeRemovalAsync(siteName, version);
            
            if (success)
            {
                _loggingService.WriteSummary("Site Removal", "Completed Successfully", new Dictionary<string, string>
                {
                    ["Site Name"] = siteName,
                    ["Version"] = version
                });
            }

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to remove site: {ex.Message}");
            _logger.LogError(ex, "Failed to remove site {SiteName}", siteName);
            return false;
        }
    }

    public Task<bool> UpdateSiteAsync(string siteName, string newVersion)
    {
        _loggingService.WriteHeader("Acumatica Site Update", $"Site: {siteName} → Version: {newVersion}");
        
        _versionService.ValidateVersionFormat(newVersion);
        
        _loggingService.WriteWarning("Update-AcuSite is not yet implemented");
        _loggingService.WriteInfo("This feature will be available in a future version");
        
        return Task.FromResult(false);
    }

    public List<string> GetInstalledSites()
    {
        var sites = new List<string>();
        
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Acumatica ERP");
            if (key == null)
                return sites;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                sites.Add(subKeyName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read installed sites from registry");
        }

        return sites;
    }

    public string? GetSiteVersion(string siteName)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Acumatica ERP\{siteName}");
            if (key == null)
                return null;

            var sitePath = key.GetValue("SitePath") as string;
            if (string.IsNullOrEmpty(sitePath))
                return null;

            var webConfigPath = Path.Combine(sitePath, "web.config");
            if (!File.Exists(webConfigPath))
                return null;

            // Parse web.config to extract version
            var doc = XDocument.Load(webConfigPath);
            var versionElement = doc.Descendants("add")
                                   .FirstOrDefault(x => x.Attribute("key")?.Value == "version");
            
            return versionElement?.Attribute("value")?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get version for site {SiteName}", siteName);
            return null;
        }
    }

    private async Task<bool> ExecuteAcuExeAsync(SiteConfiguration siteConfig, SiteInstallOptions options)
    {
        var acExePath = _versionService.GetAcuExePath(siteConfig.Version);
        
        if (!File.Exists(acExePath))
        {
            _loggingService.WriteError($"Configuration utility not found at: {acExePath}");
            return false;
        }

        var arguments = BuildAcuExeArguments(siteConfig, options, isNewInstance: true);
        
        _loggingService.WriteProgress("Executing Acumatica configuration utility...");
        _logger.LogDebug("ac.exe path: {AcExePath}", acExePath);
        _logger.LogDebug("Arguments: {Arguments}", string.Join(" ", arguments));

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = acExePath,
                Arguments = string.Join(" ", arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
        {
            _loggingService.WriteSuccess("Site creation completed successfully");
            return true;
        }
        else
        {
            _loggingService.WriteError($"Site creation failed with exit code: {process.ExitCode}");
            return false;
        }
    }

    private async Task<bool> ExecuteAcuExeRemovalAsync(string siteName, string version)
    {
        var acExePath = _versionService.GetAcuExePath(version);
        var arguments = new[] { "-d", siteName };
        
        _loggingService.WriteProgress("Executing site removal...");
        _logger.LogDebug("ac.exe path: {AcExePath}", acExePath);
        _logger.LogDebug("Arguments: {Arguments}", string.Join(" ", arguments));

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = acExePath,
                Arguments = string.Join(" ", arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    private static List<string> BuildAcuExeArguments(SiteConfiguration siteConfig, SiteInstallOptions options, bool isNewInstance)
    {
        var args = new List<string>();
        
        if (isNewInstance)
        {
            args.Add("-ni");
        }
        
        args.AddRange(new[] { "-sn", $"\"{siteConfig.SiteName}\"" });
        args.AddRange(new[] { "-sp", $"\"{siteConfig.InstallPath}\"" });
        args.AddRange(new[] { "-ds", "(local)" });
        args.AddRange(new[] { "-dn", siteConfig.SiteName });
        args.AddRange(new[] { "-ws", "\"Default Web Site\"" });
        
        if (options.Portal)
        {
            args.AddRange(new[] { "-vd", $"\"{siteConfig.SiteName}Portal\"" });
        }
        else
        {
            args.AddRange(new[] { "-vd", $"\"{siteConfig.SiteName}\"" });
        }
        
        args.AddRange(new[] { "-ap", "\"DefaultAppPool\"" });
        
        return args;
    }

    private void ApplyDevelopmentConfiguration(string sitePath)
    {
        var webConfigPath = Path.Combine(sitePath, "web.config");
        
        if (!File.Exists(webConfigPath))
        {
            _loggingService.WriteWarning("web.config not found, skipping development configuration");
            return;
        }

        try
        {
            var doc = XDocument.Load(webConfigPath);
            
            // Find compilation element
            var compilationElement = doc.Descendants("compilation").FirstOrDefault();
            if (compilationElement != null)
            {
                compilationElement.SetAttributeValue("optimizeCompilations", "true");
                compilationElement.SetAttributeValue("batch", "false");
                
                // Find pages element within compilation
                var pagesElement = compilationElement.Element("pages");
                if (pagesElement != null)
                {
                    pagesElement.SetAttributeValue("compilationMode", "Never");
                }
            }

            doc.Save(webConfigPath);
            _loggingService.WriteSuccess("Development configuration applied to web.config");
        }
        catch (Exception ex)
        {
            _loggingService.WriteWarning($"Failed to apply development configuration: {ex.Message}");
            _logger.LogError(ex, "Failed to update web.config for development");
        }
    }
}