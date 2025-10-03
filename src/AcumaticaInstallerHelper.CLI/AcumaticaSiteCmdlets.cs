using System.Management.Automation;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.CLI
{
    [Cmdlet(VerbsCommon.New, "AcumaticaSite")]
    [OutputType(typeof(bool))]
    public class NewAcumaticaSiteCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Acumatica version to use")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; } = string.Empty;

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Site name")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Site installation path")]
        public string? Path { get; set; }

        [Parameter(HelpMessage = "Install version if not present")]
        public SwitchParameter InstallVersion { get; set; }

        [Parameter(HelpMessage = "Create portal site")]
        public SwitchParameter Portal { get; set; }

        [Parameter(HelpMessage = "Create development site")]
        public SwitchParameter Development { get; set; }

        [Parameter(HelpMessage = "Use preview version")]
        public SwitchParameter Preview { get; set; }

        [Parameter(HelpMessage = "Install debug tools with version")]
        public SwitchParameter DebugTools { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var acumaticaVersion = new AcumaticaVersion
                {
                    Version = Version,
                    IsPreview = Preview.IsPresent,
                    InstallNewVersion = InstallVersion.IsPresent,
                    DebuggerTools = DebugTools.IsPresent
                };

                var siteConfig = new SiteConfiguration
                {
                    Action = SiteAction.NewInstance,
                    SiteName = Name,
                    SitePath = Path ?? string.Empty,
                    Version = acumaticaVersion,
                    IsPortal = Portal.IsPresent,
                    SiteType = Development.IsPresent ? SiteType.Development : SiteType.Production,
                    IsPreview = Preview.IsPresent,
                    ForceInstall = Force.IsPresent
                };

                var success = AcumaticaManager.CreateSite(siteConfig);
                WriteObject(success);
            
                if (success)
                {
                    WriteInformation($"Successfully created Acumatica site '{Name}' with version {Version}", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to create site '{Name}'"),
                        "CreateSiteFailed",
                        ErrorCategory.OperationStopped,
                        Name));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CreateSiteException", ErrorCategory.NotSpecified, Name));
            }
        }
    }

    [Cmdlet(VerbsCommon.Remove, "AcumaticaSite")]
    [OutputType(typeof(bool))]
    public class RemoveAcumaticaSiteCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site name to remove")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                var success = AcumaticaManager.RemoveSite(Name);
                WriteObject(success);
            
                if (success)
                {
                    WriteInformation($"Successfully removed Acumatica site '{Name}'", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to remove site '{Name}'"),
                        "RemoveSiteFailed",
                        ErrorCategory.OperationStopped,
                        Name));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveSiteException", ErrorCategory.NotSpecified, Name));
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "AcumaticaSite")]
    [OutputType(typeof(string[]))]
    public class GetAcumaticaSiteCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(HelpMessage = "Include version information")]
        public SwitchParameter IncludeVersion { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var sites = AcumaticaManager.GetInstalledSites();
            
                if (IncludeVersion.IsPresent)
                {
                    var siteInfo = sites.Select(site => new
                    {
                        Name    = site,
                        Version = AcumaticaManager.GetSiteVersion(site) ?? "Unknown"
                    }).ToArray();
                
                    WriteObject(siteInfo, true);
                }
                else
                {
                    WriteObject(sites, true);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetSiteException", ErrorCategory.NotSpecified, null));
            }
        }
    }

    [Cmdlet(VerbsData.Update, "AcumaticaSite")]
    [OutputType(typeof(bool))]
    public class UpdateAcumaticaSiteCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site name to update")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; } = string.Empty;

        [Parameter(Mandatory = true, Position = 1, HelpMessage = "New version to update to")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                var success = AcumaticaManager.UpdateSite(Name, Version);
                WriteObject(success);
            
                if (success)
                {
                    WriteInformation($"Successfully updated site '{Name}' to version {Version}", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to update site '{Name}' to version {Version}"),
                        "UpdateSiteFailed",
                        ErrorCategory.OperationStopped,
                        Name));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UpdateSiteException", ErrorCategory.NotSpecified, Name));
            }
        }
    }
}