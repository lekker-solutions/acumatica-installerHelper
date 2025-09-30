using System.Management.Automation;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.CLI
{
    [Cmdlet(VerbsLifecycle.Install, "AcumaticaVersion")]
    [OutputType(typeof(bool))]
    public class InstallAcumaticaVersionCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Version to install (format: ##.###.####)")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Install preview version")]
        public SwitchParameter Preview { get; set; }

        [Parameter(HelpMessage = "Install debugger tools")]
        public SwitchParameter DebugTools { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var acumaticaVersion = new AcumaticaVersion
                {
                    Version = Version,
                    IsPreview = Preview.IsPresent,
                    DebuggerTools = DebugTools.IsPresent
                };
                var success = AcumaticaManager.InstallVersion(acumaticaVersion);
                WriteObject(success);
            
                if (success)
                {
                    WriteInformation($"Successfully installed Acumatica version {Version}", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to install version {Version}"),
                        "InstallVersionFailed",
                        ErrorCategory.OperationStopped,
                        Version));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "InstallVersionException", ErrorCategory.NotSpecified, Version));
            }
        }
    }

    [Cmdlet(VerbsLifecycle.Uninstall, "AcumaticaVersion")]
    [OutputType(typeof(bool))]
    public class UninstallAcumaticaVersionCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Version to remove")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                var acumaticaVersion = new AcumaticaVersion
                {
                    Version = Version
                };
                var success = AcumaticaManager.RemoveVersion(acumaticaVersion);
                WriteObject(success);
            
                if (success)
                {
                    WriteInformation($"Successfully removed Acumatica version {Version}", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to remove version {Version}"),
                        "RemoveVersionFailed",
                        ErrorCategory.OperationStopped,
                        Version));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemoveVersionException", ErrorCategory.NotSpecified, Version));
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "AcumaticaVersion")]
    [OutputType(typeof(AcumaticaVersion[]))]
    public class GetAcumaticaVersionCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(HelpMessage = "Show available versions instead of installed")]
        public SwitchParameter Available { get; set; }

        [Parameter(HelpMessage = "Major release to filter (e.g., '2023.2')")]
        public string? MajorRelease { get; set; }

        [Parameter(HelpMessage = "Include preview versions")]
        public SwitchParameter Preview { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                if (Available.IsPresent)
                {
                    var availableVersions = AcumaticaManager.GetAvailableVersions(MajorRelease, Preview.IsPresent);
                    WriteObject(availableVersions, true);
                }
                else
                {
                    var installedVersions = AcumaticaManager.GetInstalledVersions();
                    WriteObject(installedVersions, true);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetVersionException", ErrorCategory.NotSpecified, null));
            }
        }
    }
}