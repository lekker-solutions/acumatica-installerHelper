using System.Management.Automation;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper.CLI
{
    [Cmdlet(VerbsDiagnostic.Test, "AcumaticaPatch")]
    [OutputType(typeof(PatchCheckResult))]
    public class TestAcumaticaPatchCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site name to check for patches")]
        [ValidateNotNullOrEmpty]
        public string SiteName { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                var result = AcumaticaManager.CheckForPatches(SiteName);
                WriteObject(result);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CheckForPatchesException", ErrorCategory.NotSpecified, SiteName));
            }
        }
    }

    [Cmdlet(VerbsLifecycle.Install, "AcumaticaPatch")]
    [OutputType(typeof(PatchResult))]
    public class InstallAcumaticaPatchCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site name to apply patch to")]
        [ValidateNotNullOrEmpty]
        public string SiteName { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Path to backup archive")]
        public string? BackupPath { get; set; }

        [Parameter(HelpMessage = "Path to local patch archive")]
        public string? ArchivePath { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                PatchResult result;
                
                if (!string.IsNullOrEmpty(ArchivePath))
                {
                    result = AcumaticaManager.ApplyPatchFromArchive(SiteName, ArchivePath, BackupPath);
                }
                else
                {
                    result = AcumaticaManager.ApplyPatch(SiteName, BackupPath);
                }
                
                WriteObject(result);
                
                if (result.Success)
                {
                    WriteInformation($"Successfully applied patch to site '{SiteName}'", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to apply patch to site '{SiteName}': {result.Message}"),
                        "ApplyPatchFailed",
                        ErrorCategory.OperationStopped,
                        SiteName));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ApplyPatchException", ErrorCategory.NotSpecified, SiteName));
            }
        }
    }

    [Cmdlet(VerbsData.Restore, "AcumaticaPatch")]
    [OutputType(typeof(PatchResult))]
    public class RestoreAcumaticaPatchCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Site name to rollback patch from")]
        [ValidateNotNullOrEmpty]
        public string SiteName { get; set; } = string.Empty;

        [Parameter(HelpMessage = "Path to backup archive")]
        public string? BackupPath { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var result = AcumaticaManager.RollbackPatch(SiteName, BackupPath);
                WriteObject(result);
                
                if (result.Success)
                {
                    WriteInformation($"Successfully rolled back patch for site '{SiteName}'", new string[] { "Success" });
                }
                else
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to rollback patch for site '{SiteName}': {result.Message}"),
                        "RollbackPatchFailed",
                        ErrorCategory.OperationStopped,
                        SiteName));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RollbackPatchException", ErrorCategory.NotSpecified, SiteName));
            }
        }
    }

    [Cmdlet(VerbsDiagnostic.Test, "AcumaticaPatchTool")]
    [OutputType(typeof(bool))]
    public class TestAcumaticaPatchToolCmdlet : AcumaticaBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Acumatica version to check patch tool for")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; } = string.Empty;

        protected override void ProcessRecord()
        {
            try
            {
                var isAvailable = AcumaticaManager.IsPatchToolAvailable(Version);
                WriteObject(isAvailable);
                
                if (isAvailable)
                {
                    WriteInformation($"Patch tool is available for version {Version}", new string[] { "Success" });
                }
                else
                {
                    WriteWarning($"Patch tool is not available for version {Version}");
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CheckPatchToolException", ErrorCategory.NotSpecified, Version));
            }
        }
    }
}