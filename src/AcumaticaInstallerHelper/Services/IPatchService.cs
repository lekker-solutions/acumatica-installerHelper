namespace AcumaticaInstallerHelper.Services;

public interface IPatchService
{
    string GetPatchToolPath(string version);
    bool IsPatchToolAvailable(string version);
    PatchCheckResult CheckForPatches(string sitePath, string version);
    PatchResult ApplyPatch(string sitePath, string version, string? backupPath = null);
    PatchResult ApplyPatchFromArchive(string sitePath, string archivePath, string version, string? backupPath = null);
    PatchResult RollbackPatch(string sitePath, string version, string? backupPath = null);
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