using System.Text.Json;
using Microsoft.Extensions.Logging;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private AcumaticaConfig? _cachedConfig;

    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
        _configFilePath = Path.Combine(assemblyDirectory, "AcuInstallerHelper_config.json");
    }

    public AcumaticaConfig GetConfiguration()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        try
        {
            if (File.Exists(_configFilePath))
            {
                var jsonContent = File.ReadAllText(_configFilePath);
                _cachedConfig = JsonSerializer.Deserialize<AcumaticaConfig>(jsonContent) ?? new AcumaticaConfig();
                _logger.LogDebug("Configuration loaded from {ConfigFile}", _configFilePath);
            }
            else
            {
                _cachedConfig = new AcumaticaConfig();
                _logger.LogInformation("Configuration file not found, using defaults");
                
                // Save default configuration
                _ = Task.Run(async () => await SaveConfigurationAsync(_cachedConfig));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration, using defaults");
            _cachedConfig = new AcumaticaConfig();
        }

        return _cachedConfig;
    }

    public async Task SaveConfigurationAsync(AcumaticaConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var jsonContent = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configFilePath, jsonContent);
            
            _cachedConfig = config;
            _logger.LogDebug("Configuration saved to {ConfigFile}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {ConfigFile}", _configFilePath);
            throw;
        }
    }

    public string GetAcumaticaDirectory()
    {
        return GetConfiguration().AcumaticaDirectory;
    }

    public void SetAcumaticaDirectory(string path)
    {
        var config = GetConfiguration();
        config.AcumaticaDirectory = path;
        _ = Task.Run(async () => await SaveConfigurationAsync(config));
    }

    public string GetSiteDirectory()
    {
        return GetConfiguration().SiteDirectory;
    }

    public void SetSiteDirectory(string directory)
    {
        var config = GetConfiguration();
        config.SiteDirectory = directory;
        _ = Task.Run(async () => await SaveConfigurationAsync(config));
    }

    public string GetVersionDirectory()
    {
        return GetConfiguration().VersionDirectory;
    }

    public void SetVersionDirectory(string directory)
    {
        var config = GetConfiguration();
        config.VersionDirectory = directory;
        _ = Task.Run(async () => await SaveConfigurationAsync(config));
    }

    public SiteType GetDefaultSiteType()
    {
        var config = GetConfiguration();
        return Enum.TryParse<SiteType>(config.SiteType, out var siteType) ? siteType : Models.SiteType.Production;
    }

    public void SetDefaultSiteType(SiteType siteType)
    {
        var config = GetConfiguration();
        config.SiteType = siteType.ToString();
        _ = Task.Run(async () => await SaveConfigurationAsync(config));
    }

    public bool GetInstallDebugTools()
    {
        return GetConfiguration().InstallDebugTools;
    }

    public void SetInstallDebugTools(bool install)
    {
        var config = GetConfiguration();
        config.InstallDebugTools = install;
        _ = Task.Run(async () => await SaveConfigurationAsync(config));
    }

    public string GetDefaultSiteInstallPath()
    {
        return Path.Combine(GetAcumaticaDirectory(), GetSiteDirectory());
    }
}