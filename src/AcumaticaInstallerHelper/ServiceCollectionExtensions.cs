using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAcumaticaInstallerHelper(this IServiceCollection services)
    {
        services.AddHttpClient<IVersionService, VersionService>();
        services.AddHttpClient<IPatchService, PatchService>();
        
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ILoggingService, ConsoleLoggingService>();
        services.AddTransient<IVersionService, VersionService>();
        services.AddTransient<ISiteService, SiteService>();
        services.AddTransient<IPatchService, PatchService>();
        services.AddTransient<AcumaticaManager>();
        
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        return services;
    }
}