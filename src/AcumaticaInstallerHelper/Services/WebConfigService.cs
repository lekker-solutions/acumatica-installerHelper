using System.Xml.Linq;

namespace AcumaticaInstallerHelper.Services;

public class WebConfigService : IWebConfigService
{
    private readonly ILoggingService _loggingService;

    public WebConfigService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public string GetSiteVersion(string webConfigPath)
    {
        try
        {
            if (!File.Exists(webConfigPath))
            {
                _loggingService.WriteDebug($"Web.config not found at: {webConfigPath}");
                return string.Empty;
            }

            // Parse web.config to extract version from appSettings
            var doc = XDocument.Load(webConfigPath);
            var versionElement = doc.Descendants("appSettings")
                                   .Elements("add")
                                   .FirstOrDefault(x => x.Attribute("key")?.Value == "Version");

            var version = versionElement?.Attribute("value")?.Value;
            if (!string.IsNullOrEmpty(version))
            {
                _loggingService.WriteDebug($"Found version {version} in web.config");
            }

            return version ?? string.Empty;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to get version from web.config: {ex.Message}");
            return string.Empty;
        }
    }

    public bool ApplyDevelopmentConfiguration(string webConfigPath)
    {
        if (!File.Exists(webConfigPath))
        {
            _loggingService.WriteWarning("web.config not found, skipping development configuration");
            return false;
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
                
                _loggingService.WriteDebug("Set optimizeCompilations=true and batch=false");
            }

            // Deliberately no pages compilationMode change: Acumatica's master
            // pages use CodeFile attributes, which no-compile pages reject —
            // setting compilationMode=Never breaks every request with
            // "The attribute 'autoeventwireup' is not allowed in this page".

            doc.Save(webConfigPath);
            _loggingService.WriteSuccess("Development configuration applied to web.config");
            return true;
        }
        catch (Exception ex)
        {
            _loggingService.WriteWarning($"Failed to apply development configuration: {ex.Message}");
            return false;
        }
    }

    public bool UpdateConnectionString(string webConfigPath, string connectionString)
    {
        try
        {
            if (!File.Exists(webConfigPath))
            {
                _loggingService.WriteError($"Web.config not found at: {webConfigPath}");
                return false;
            }

            var doc = XDocument.Load(webConfigPath);
            
            // Find the ProjectX connection string
            var connectionElement = doc.Descendants("connectionStrings")
                                      .Elements("add")
                                      .FirstOrDefault(x => x.Attribute("name")?.Value == "ProjectX");

            if (connectionElement != null)
            {
                connectionElement.SetAttributeValue("connectionString", connectionString);
                doc.Save(webConfigPath);
                _loggingService.WriteSuccess("Connection string updated in web.config");
                return true;
            }
            else
            {
                _loggingService.WriteError("ProjectX connection string not found in web.config");
                return false;
            }
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to update connection string: {ex.Message}");
            return false;
        }
    }

    public string? GetConnectionString(string webConfigPath)
    {
        try
        {
            if (!File.Exists(webConfigPath))
            {
                _loggingService.WriteDebug($"Web.config not found at: {webConfigPath}");
                return null;
            }

            var doc = XDocument.Load(webConfigPath);
            
            // Find the ProjectX connection string
            var connectionElement = doc.Descendants("connectionStrings")
                                      .Elements("add")
                                      .FirstOrDefault(x => x.Attribute("name")?.Value == "ProjectX");

            var connectionString = connectionElement?.Attribute("connectionString")?.Value;
            
            if (!string.IsNullOrEmpty(connectionString))
            {
                _loggingService.WriteDebug("Found ProjectX connection string in web.config");
            }
            
            return connectionString;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to get connection string from web.config: {ex.Message}");
            return null;
        }
    }
}