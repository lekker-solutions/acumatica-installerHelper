using System.Diagnostics;
using System.Security.Principal;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class SiteService : ISiteService
{
    private readonly IVersionService      _versionService;
    private readonly IAcArgBuilderFactory _argFactory;
    private readonly ILoggingService      _loggingService;
    private readonly ISiteRegistryService _siteRegistryService;
    private readonly IWebConfigService    _webConfigService;

    public SiteService(
        IVersionService      versionService,
        IAcArgBuilderFactory argFactory,
        ILoggingService      loggingService,
        ISiteRegistryService siteRegistryService,
        IWebConfigService    webConfigService)
    {
        _versionService      = versionService;
        _argFactory          = argFactory;
        _loggingService      = loggingService;
        _siteRegistryService = siteRegistryService;
        _webConfigService    = webConfigService;
    }

    public bool RequiresAdministratorPrivileges()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return !principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public bool CreateSite(SiteConfiguration siteConfig)
    {
        _loggingService.WriteHeader("Acumatica Site Installation", $"Version {siteConfig.Version} • Site: {siteConfig.SiteName}");

        try
        {
            _loggingService.WriteSection("Validating Prerequisites");

            // Check administrator privileges
            if (!IsAdministrator())
            {
                _loggingService.WriteError("Administrator privileges required");
                throw new UnauthorizedAccessException("This operation must be run as Administrator");
            }
            _loggingService.WriteSuccess("Administrator privileges confirmed");

            // Check if site already exists
            if (_siteRegistryService.SiteExists(siteConfig.SiteName))
            {
                _loggingService.WriteWarning($"Site '{siteConfig.SiteName}' already exists");
                
                if (!_loggingService.PromptYesNo($"Site '{siteConfig.SiteName}' already exists. Do you want to continue and potentially overwrite it?"))
                {
                    _loggingService.WriteError("Site creation cancelled - site already exists");
                    return false;
                }
                
                _loggingService.WriteInfo($"Continuing with existing site '{siteConfig.SiteName}'");
            }

            // Check if version exists
            if (!_versionService.IsVersionInstalled(siteConfig.Version))
            {
                _loggingService.WriteWarning($"Version {siteConfig.Version.MinorVersion} not found locally");

                bool shouldInstall = siteConfig.Version.InstallNewVersion ||
                                     _loggingService.PromptYesNo(
                                         $"You do not have version {siteConfig.Version.MinorVersion} installed, do you want to install?");

                if (!shouldInstall)
                {
                    _loggingService.WriteError("Site installation cancelled - version not available");
                    return false;
                }

                _loggingService.WriteSection("Installing Required Version");

                VersionConfiguration versionConfig = new()
                {
                    Version = siteConfig.Version,
                    VersionPath = _versionService.GetVersionPath(siteConfig.Version),
                    InstallDebugTools = siteConfig.SiteType == SiteType.Development,
                    ForceInstall      = siteConfig.ForceInstall
                };

                bool installSuccess =
                    _versionService.InstallVersion(versionConfig);

                if (!installSuccess)
                {
                    _loggingService.WriteError("Failed to install required version");
                    return false;
                }
            }
            else
            {
                _loggingService.WriteSuccess(
                    $"Version {siteConfig.Version} found at {_versionService.GetVersionPath(siteConfig.Version)}");
            }

            _loggingService.WriteSection("Configuring Site Parameters");

            // Apply dev configuration based on explicit parameter or config default
            bool isDev = siteConfig.SiteType == SiteType.Development;

            _loggingService.WriteTable(new Dictionary<string, string>
            {
                ["Site Name"]     = siteConfig.SiteName,
                ["Version"]       = siteConfig.Version.MinorVersion,
                ["Install Path"]  = siteConfig.SitePath,
                ["Portal Site"]   = siteConfig.IsPortal ? "Yes" : "No",
                ["Site Type"]     = isDev ? "Development" : "Production",
                ["Preview Build"] = siteConfig.Version.IsPreview ? "Yes" : "No"
            }, "Site Configuration");

            _loggingService.WriteSection("Installing Site");

            // Install site
            bool success = ExecuteAcuExe(siteConfig, _argFactory.Create(siteConfig));

            if (success && isDev)
            {
                _loggingService.WriteStep("Applying development configuration");
                string webConfigPath = Path.Combine(siteConfig.SitePath, "web.config");
                _webConfigService.ApplyDevelopmentConfiguration(webConfigPath);
            }

            if (success)
                _loggingService.WriteSummary("Site Installation", "Completed Successfully",
                    new Dictionary<string, string>
                    {
                        ["Site Name"] = siteConfig.SiteName,
                        ["Version"]   = siteConfig.Version.MinorVersion,
                        ["Path"]      = siteConfig.SitePath,
                        ["Type"]      = isDev ? "Development" : "Production",
                        ["Portal"]    = siteConfig.IsPortal ? "Yes" : "No"
                    });

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to create site {siteConfig.SiteName}: {ex.Message}");
            return false;
        }
    }

    public bool RemoveSite(SiteConfiguration siteConfig)
    {
        _loggingService.WriteHeader("Acumatica Site Removal", $"Site: {siteConfig.SiteName}");

        try
        {
            _loggingService.WriteSection("Removing Site Registration");

            bool success = ExecuteAcuExe(siteConfig, _argFactory.Create(siteConfig));

            if (success)
                _loggingService.WriteSummary("Site Removal", "Completed Successfully", new Dictionary<string, string>
                {
                    ["Site Name"] = siteConfig.SiteName
                });

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to remove site {siteConfig.SiteName}: {ex.Message}");
            return false;
        }
    }

    public bool UpdateSite(SiteConfiguration config)
    {
        _loggingService.WriteHeader("Acumatica Site Update",
            $"Site: {config.SiteName} → Version: {config.Version.MinorVersion}");
        _loggingService.WriteWarning("Update-AcuSite is not yet implemented");
        _loggingService.WriteInfo("This feature will be available in a future version");

        return false;
    }

    private bool ExecuteAcuExe(SiteConfiguration siteConfig, IAcArgBuilder argBuilder)
    {
        var acExePath = _versionService.GetAcuExePath(siteConfig.Version);

        if (!File.Exists(acExePath))
        {
            _loggingService.WriteError($"Configuration utility not found at: {acExePath}");
            return false;
        }

        var    arguments      = argBuilder.BuildArgs(siteConfig);
        string argumentString = string.Join(" ", arguments);
        _loggingService.WriteProgress("Executing Acumatica configuration utility...");
        _loggingService.WriteDebug($"ac.exe path: {acExePath}");
        _loggingService.WriteDebug($"Arguments: {argumentString}");

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = acExePath,
                Arguments              = argumentString,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            }
        };

        var outputBuffer = new List<string>();
        var errorBuffer = new List<string>();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuffer.Add(e.Data);
                _loggingService.WriteInfo($"ac.exe: {e.Data}");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuffer.Add(e.Data);
                _loggingService.WriteWarning($"ac.exe: {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            _loggingService.WriteSuccess("ac.exe completed successfully");
            return true;
        }

        _loggingService.WriteError($"ac.exe failed with exit code: {process.ExitCode}");
        if (errorBuffer.Count > 0)
        {
            _loggingService.WriteError("Error details:");
            foreach (string error in errorBuffer) _loggingService.WriteError($"  {error}");
        }

        return false;
    }
}