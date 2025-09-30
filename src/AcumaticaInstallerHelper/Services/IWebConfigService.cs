namespace AcumaticaInstallerHelper.Services;

public interface IWebConfigService
{
    string GetSiteVersion(string webConfigPath);
    bool ApplyDevelopmentConfiguration(string webConfigPath);
    bool UpdateConnectionString(string webConfigPath, string connectionString);
    string? GetConnectionString(string webConfigPath);
}