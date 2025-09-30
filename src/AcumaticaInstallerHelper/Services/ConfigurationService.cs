using System.Text.Json;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly string               _configFilePath;
    private          ModuleConfiguration? _cachedConfig;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ConfigurationService()
    {
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
        _configFilePath = Path.Combine(assemblyDirectory, "AcuInstallerHelper_config.json");
    }

    public ModuleConfiguration GetConfiguration()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        try
        {
            if (File.Exists(_configFilePath))
            {
                var jsonContent = File.ReadAllText(_configFilePath);
                _cachedConfig = JsonSerializer.Deserialize<ModuleConfiguration>(jsonContent) ??
                                new ModuleConfiguration();
            }
            else
            {
                _cachedConfig = new ModuleConfiguration();

                // Save default configuration
                SaveConfiguration(_cachedConfig);
            }
        }
        catch (Exception)
        {
            _cachedConfig = new ModuleConfiguration();
        }

        return _cachedConfig;
    }

    public void SaveConfiguration(ModuleConfiguration config)
    {
        try
        {
            string jsonContent = JsonSerializer.Serialize(config, _jsonOptions);
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
        return config.DefaultSiteType;
    }

    public void SetDefaultSiteType(SiteType siteType)
    {
        var config = GetConfiguration();
        config.DefaultSiteType = siteType;
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