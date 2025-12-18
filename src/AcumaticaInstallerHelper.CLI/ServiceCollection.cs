using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper.CLI
{
    public static class ServiceContainer
    {
        private static readonly Lazy<AcumaticaManager> _acumaticaManager = new(CreateAcumaticaManager);

        private static readonly Lazy<IConfigurationService> _configurationService = new(new ConfigurationService());
        private static readonly Lazy<ILoggingService> _loggingService = new(new ConsoleLoggingService());
        private static readonly Lazy<HttpClient> _httpClient = new(new HttpClient());
        private static readonly Lazy<IAcArgBuilderFactory> _argFactory = new(new AcArgFactory());

        public static T GetService<T>()
        {
            if (typeof(T) == typeof(AcumaticaManager))
                return (T)(object)_acumaticaManager.Value;

            if (typeof(T) == typeof(IConfigurationService))
                return (T)_configurationService.Value;

            if (typeof(T) == typeof(ILoggingService))
                return (T)_loggingService.Value;

            if (typeof(T) == typeof(IAcArgBuilderFactory))
                return (T)_argFactory.Value;

            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }

        private static AcumaticaManager CreateAcumaticaManager()
        {
            HttpClient httpClient = _httpClient.Value;
            IConfigurationService configService = _configurationService.Value;
            var loggingService = _loggingService.Value;
            IAcArgBuilderFactory argFactory = _argFactory.Value;

            var webConfigService = new WebConfigService(loggingService);
            var processManagerService = new ProcessManagerService(loggingService);
            var siteRegistryService = new SiteRegistryService(loggingService, webConfigService);
            var versionService = new VersionService(configService, loggingService, processManagerService, httpClient);
            var siteService = new SiteService(versionService, argFactory, loggingService, siteRegistryService, webConfigService, processManagerService);
            var patchService = new PatchService(versionService, loggingService, processManagerService);

            return new AcumaticaManager(
                versionService,
                siteService,
                configService,
                loggingService,
                patchService,
                siteRegistryService);
        }
    }
}