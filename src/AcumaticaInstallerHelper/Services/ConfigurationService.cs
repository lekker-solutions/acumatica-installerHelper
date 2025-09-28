using System.Text.Json;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private AcumaticaConfig? _cachedConfig;

    public ConfigurationService()
    {
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
            }
            else
            {
                _cachedConfig = new AcumaticaConfig();
                
                // Save default configuration
                SaveConfiguration(_cachedConfig);
            }
        }
        catch (Exception)
        {
            _cachedConfig = new AcumaticaConfig();
        }

        return _cachedConfig;
    }

    public void SaveConfiguration(AcumaticaConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var jsonContent = JsonSerializer.Serialize(config, options);
            File.WriteAllText(_configFilePath, jsonContent);
            
            _cachedConfig = config;
        }
        catch (Exception)
        {
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
        SaveConfiguration(config);
    }

    public string GetSiteDirectory()
    {
        return GetConfiguration().SiteDirectory;
    }

    public void SetSiteDirectory(string directory)
    {
        var config = GetConfiguration();
        config.SiteDirectory = directory;
        SaveConfiguration(config);
    }

    public string GetVersionDirectory()
    {
        return GetConfiguration().VersionDirectory;
    }

    public void SetVersionDirectory(string directory)
    {
        var config = GetConfiguration();
        config.VersionDirectory = directory;
        SaveConfiguration(config);
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
        SaveConfiguration(config);
    }

    public bool GetInstallDebugTools()
    {
        return GetConfiguration().InstallDebugTools;
    }

    public void SetInstallDebugTools(bool install)
    {
        var config = GetConfiguration();
        config.InstallDebugTools = install;
        SaveConfiguration(config);
    }

    public string GetDefaultSiteInstallPath()
    {
        return Path.Combine(GetAcumaticaDirectory(), GetSiteDirectory());
    }
}