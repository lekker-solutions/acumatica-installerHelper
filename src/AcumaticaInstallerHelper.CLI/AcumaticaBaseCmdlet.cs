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

        protected override void ProcessRecord()
        {
            try
            {
                ProcessRecordAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CmdletException", ErrorCategory.NotSpecified, null));
            }
        }

        protected abstract Task ProcessRecordAsync();
    }
}