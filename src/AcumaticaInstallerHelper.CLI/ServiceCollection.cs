using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper.CLI
{

    public static class ServiceContainer
    {
        private static readonly Lazy<AcumaticaManager> _acumaticaManager = new(() => CreateAcumaticaManager());
        private static readonly Lazy<IConfigurationService> _configurationService = new(() => CreateConfigurationService());
        private static readonly Lazy<ILoggingService> _loggingService = new(() => CreateConsoleLoggingService());
        private static readonly Lazy<HttpClient> _httpClient = new(() => new HttpClient());

        public static T GetService<T>()
        {
            if (typeof(T) == typeof(AcumaticaManager))
                return (T)(object)_acumaticaManager.Value;
            
            if (typeof(T) == typeof(IConfigurationService))
                return (T)_configurationService.Value;
            
            if (typeof(T) == typeof(ILoggingService))
                return (T)_loggingService.Value;

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        private static IConfigurationService CreateConfigurationService()
        {
            return new ConfigurationService();
        }

        private static ILoggingService CreateConsoleLoggingService()
        {
            return new ConsoleLoggingService();
        }

        private static AcumaticaManager CreateAcumaticaManager()
        {
            var httpClient = _httpClient.Value;
            var configService = _configurationService.Value;
            var loggingService = _loggingService.Value;

            var versionService = new VersionService(configService, loggingService, httpClient);
            var siteService = new SiteService(versionService, configService, loggingService);
            var patchService = new PatchService(configService, loggingService, httpClient);

            return new AcumaticaManager(
                versionService,
                siteService,
                configService,
                loggingService,
                patchService);
        }
    }
}