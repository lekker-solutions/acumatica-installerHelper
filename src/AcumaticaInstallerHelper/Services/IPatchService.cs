namespace AcumaticaInstallerHelper.Services;

public interface IPatchService
{
    Task<string> GetPatchToolPathAsync(string version);
    Task<bool> IsPatchToolAvailableAsync(string version);
    Task<PatchCheckResult> CheckForPatchesAsync(string sitePath, string version);
    Task<PatchResult> ApplyPatchAsync(string sitePath, string version, string? backupPath = null);
    Task<PatchResult> ApplyPatchFromArchiveAsync(string sitePath, string archivePath, string version, string? backupPath = null);
    Task<PatchResult> RollbackPatchAsync(string sitePath, string version, string? backupPath = null);
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