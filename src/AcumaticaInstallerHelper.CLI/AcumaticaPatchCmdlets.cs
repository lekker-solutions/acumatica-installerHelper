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

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var result = await AcumaticaManager.CheckForPatchesAsync(SiteName);
                WriteObject(result);

                if (result.HasPatch)
                {
                    WriteInformation($"Patch found for site '{SiteName}': {result.Version} P{result.PatchNumber}", new string[] { "PatchFound" });
                }
                else
                {
                    WriteInformation($"No patches available for site '{SiteName}'", new string[] { "NoPatch" });
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CheckPatchException", ErrorCategory.NotSpecified, SiteName));
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

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                PatchResult result;

                if (!string.IsNullOrEmpty(ArchivePath))
                {
                    result = await AcumaticaManager.ApplyPatchFromArchiveAsync(SiteName, ArchivePath, BackupPath);
                }
                else
                {
                    result = await AcumaticaManager.ApplyPatchAsync(SiteName, BackupPath);
                }

                WriteObject(result);

                if (result.Success)
                {
                    if (!string.IsNullOrEmpty(result.Version) && !string.IsNullOrEmpty(result.PatchNumber))
                    {
                        WriteInformation($"Successfully applied patch {result.Version} P{result.PatchNumber} to site '{SiteName}'", new string[] { "Success" });
                    }
                    else
                    {
                        WriteInformation($"Patch operation completed for site '{SiteName}'", new string[] { "Success" });
                    }
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

        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var result = await AcumaticaManager.RollbackPatchAsync(SiteName, BackupPath);
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
        protected override async Task ProcessRecordAsync()
        {
            try
            {
                var isAvailable = await AcumaticaManager.IsPatchToolAvailableAsync();
                WriteObject(isAvailable);

                if (isAvailable)
                {
                    WriteInformation("PatchTool is available and ready to use", new string[] { "Available" });
                }
                else
                {
                    WriteWarning("PatchTool is not available. It will be downloaded automatically when needed.");
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "TestPatchToolException", ErrorCategory.NotSpecified, null));
            }
        }
    }
}