namespace AcumaticaInstallerHelper.Services;

public interface IPatchService
{
    Task<string> GetPatchToolPathAsync();
    Task<bool> IsPatchToolAvailableAsync();
    Task<PatchCheckResult> CheckForPatchesAsync(string sitePath);
    Task<PatchResult> ApplyPatchAsync(string sitePath, string? backupPath = null);
    Task<PatchResult> ApplyPatchFromArchiveAsync(string sitePath, string archivePath, string? backupPath = null);
    Task<PatchResult> RollbackPatchAsync(string sitePath, string? backupPath = null);
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