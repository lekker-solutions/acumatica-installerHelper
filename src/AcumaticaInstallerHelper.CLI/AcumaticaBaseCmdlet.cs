using System.Management.Automation;

namespace AcumaticaInstallerHelper.CLI
{
    public abstract class AcumaticaBaseCmdlet : PSCmdlet
    {
        private AcumaticaManager? _acumaticaManager;

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
        }
    }
}