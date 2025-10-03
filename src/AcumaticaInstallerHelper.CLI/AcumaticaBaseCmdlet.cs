using System.Management.Automation;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper.CLI
{
    public abstract class AcumaticaBaseCmdlet : PSCmdlet
    {
        private AcumaticaManager? _acumaticaManager;
        
        [Parameter(HelpMessage = "Override all prompts to Yes")]
        public SwitchParameter Force { get; set; }

        protected AcumaticaManager AcumaticaManager
        {
            get
            {
                if (_acumaticaManager == null)
                {
                    InitializeServices();
                }
                return _acumaticaManager!;
            }
        }

        private void InitializeServices()
        {
            _acumaticaManager = ServiceContainer.GetService<AcumaticaManager>();
            
            // Set the override flag on the logging service if Force is specified
            if (Force.IsPresent)
            {
                var loggingService = ServiceContainer.GetService<ILoggingService>();
                loggingService.OverridePromptsToYes = true;
            }
        }
    }
}