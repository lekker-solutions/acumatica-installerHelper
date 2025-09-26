using System.Management.Automation;
using AcumaticaInstallerHelper;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.PowerShell.Cmdlets;

[Cmdlet(VerbsCommon.Get, "AcumaticaConfig")]
[OutputType(typeof(object))]
public class GetAcumaticaConfigCmdlet : AcumaticaBaseCmdlet
{
    protected override async Task ProcessRecordAsync()
    {
        try
        {
            var config = new
            {
                AcumaticaDirectory = AcumaticaManager.GetAcumaticaDirectory(),
                SiteDirectory = AcumaticaManager.GetSiteDirectory(),
                VersionDirectory = AcumaticaManager.GetVersionDirectory(),
                DefaultSiteType = AcumaticaManager.GetDefaultSiteType().ToString(),
                InstallDebugTools = AcumaticaManager.GetInstallDebugTools()
            };

            WriteObject(config);
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "GetConfigException", ErrorCategory.NotSpecified, null));
        }
    }
}

[Cmdlet(VerbsCommon.Set, "AcumaticaDirectory")]
[OutputType(typeof(void))]
public class SetAcumaticaDirectoryCmdlet : AcumaticaBaseCmdlet
{
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "New Acumatica base directory path")]
    [ValidateNotNullOrEmpty]
    public string Path { get; set; } = string.Empty;

    protected override async Task ProcessRecordAsync()
    {
        try
        {
            AcumaticaManager.SetAcumaticaDirectory(Path);
            WriteInformation($"Acumatica directory set to: {Path}", new string[] { "Success" });
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SetAcumaticaDirectoryException", ErrorCategory.NotSpecified, Path));
        }
    }
}

[Cmdlet(VerbsCommon.Set, "AcumaticaSiteDirectory")]
[OutputType(typeof(void))]
public class SetAcumaticaSiteDirectoryCmdlet : AcumaticaBaseCmdlet
{
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "New site directory path")]
    [ValidateNotNullOrEmpty]
    public string Path { get; set; } = string.Empty;

    protected override async Task ProcessRecordAsync()
    {
        try
        {
            AcumaticaManager.SetSiteDirectory(Path);
            WriteInformation($"Site directory set to: {Path}", new string[] { "Success" });
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SetSiteDirectoryException", ErrorCategory.NotSpecified, Path));
        }
    }
}

[Cmdlet(VerbsCommon.Set, "AcumaticaVersionDirectory")]
[OutputType(typeof(void))]
public class SetAcumaticaVersionDirectoryCmdlet : AcumaticaBaseCmdlet
{
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "New version directory path")]
    [ValidateNotNullOrEmpty]
    public string Path { get; set; } = string.Empty;

    protected override async Task ProcessRecordAsync()
    {
        try
        {
            AcumaticaManager.SetVersionDirectory(Path);
            WriteInformation($"Version directory set to: {Path}", new string[] { "Success" });
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SetVersionDirectoryException", ErrorCategory.NotSpecified, Path));
        }
    }
}

[Cmdlet(VerbsCommon.Set, "AcumaticaDefaultSiteType")]
[OutputType(typeof(void))]
public class SetAcumaticaDefaultSiteTypeCmdlet : AcumaticaBaseCmdlet
{
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site type (Production or Development)")]
    [ValidateSet("Production", "Development")]
    public SiteType SiteType { get; set; }

    protected override async Task ProcessRecordAsync()
    {
        try
        {
            AcumaticaManager.SetDefaultSiteType(SiteType);
            WriteInformation($"Default site type set to: {SiteType}", new string[] { "Success" });
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SetDefaultSiteTypeException", ErrorCategory.NotSpecified, SiteType));
        }
    }
}

[Cmdlet(VerbsCommon.Set, "AcumaticaInstallDebugTools")]
[OutputType(typeof(void))]
public class SetAcumaticaInstallDebugToolsCmdlet : AcumaticaBaseCmdlet
{
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "Install debug tools by default")]
    public bool InstallDebugTools { get; set; }

    protected override async Task ProcessRecordAsync()
    {
        try
        {
            AcumaticaManager.SetInstallDebugTools(InstallDebugTools);
            WriteInformation($"Install debug tools set to: {InstallDebugTools}", new string[] { "Success" });
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SetInstallDebugToolsException", ErrorCategory.NotSpecified, InstallDebugTools));
        }
    }
}