using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface IPatchService
{
    string GetPatchToolPath(AcumaticaVersion version);
    bool IsPatchToolAvailable(AcumaticaVersion version);
    PatchCheckResult CheckForPatches(PatchConfiguration config);
    PatchResult ApplyPatch(PatchConfiguration config, string? backupPath = null);
    PatchResult ApplyPatchFromArchive(PatchConfiguration config, string archivePath, string? backupPath = null);
    PatchResult RollbackPatch(PatchConfiguration config, string? backupPath = null);
}

public class PatchCheckResult
{
    public bool HasPatch { get; set; }
    public string? Version { get; set; }
    public string? PatchNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PatchResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? PatchNumber { get; set; }
}