using System.Management.Automation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AcumaticaInstallerHelper;

namespace AcumaticaInstallerHelper.PowerShell.Cmdlets;

public abstract class AcumaticaBaseCmdlet : PSCmdlet
{
    private IHost? _host;
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
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        builder.Services.AddAcumaticaInstallerHelper();
        
        _host = builder.Build();
        _acumaticaManager = _host.Services.GetRequiredService<AcumaticaManager>();
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

    protected override void EndProcessing()
    {
        _host?.Dispose();
        base.EndProcessing();
    }
}