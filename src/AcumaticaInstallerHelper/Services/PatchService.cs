using System.Diagnostics;
using System.Text.RegularExpressions;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class PatchService : IPatchService
{
    private readonly IVersionService _versionService;
    private readonly ILoggingService _loggingService;
    private readonly IProcessManagerService _processManagerService;

    private static readonly Regex PatchFoundRegex = new(@"A new patch found: (.+) P(\d+)", RegexOptions.Compiled);
    private static readonly Regex PatchAppliedRegex = new(@"The patch with the (.+) P(\d+) version has been applied", RegexOptions.Compiled);

    public PatchService(
        IVersionService versionService,
        ILoggingService loggingService,
        IProcessManagerService processManagerService)
    {
        _versionService = versionService;
        _loggingService = loggingService;
        _processManagerService = processManagerService;
    }

    public string GetPatchToolPath(AcumaticaVersion version)
    {
        if (!IsPatchingSupportedForVersion(version.MinorVersion))
        {
            _loggingService.WriteError($"Patching is not supported for version {version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later.");
            throw new NotSupportedException($"Patching is not supported for version {version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later.");
        }

        var patchToolPath = _versionService.GetPatchUtilityPath(version);
        
        if (!File.Exists(patchToolPath))
        {
            _loggingService.WriteError($"PatchTool not found at {patchToolPath}. Ensure version {version.MinorVersion} is installed.");
            throw new FileNotFoundException($"PatchTool not found for version {version.MinorVersion}", patchToolPath);
        }

        return patchToolPath;
    }

    public bool IsPatchToolAvailable(AcumaticaVersion version)
    {
        try
        {
            if (!IsPatchingSupportedForVersion(version.MinorVersion))
            {
                return false;
            }

            var patchToolPath = _versionService.GetPatchUtilityPath(version);
            return File.Exists(patchToolPath);
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to check patch tool availability for version {version.MinorVersion}: {ex.Message}");
            return false;
        }
    }

    public PatchCheckResult CheckForPatches(PatchConfiguration config)
    {
        _loggingService.WriteDebug($"Checking for patches at site: {config.SiteName} using version {config.Version.MinorVersion}");

        if (!IsPatchingSupportedForVersion(config.Version.MinorVersion))
        {
            return new PatchCheckResult
            {
                HasPatch = false,
                Message = $"Patching is not supported for version {config.Version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = GetPatchToolPath(config.Version);
            var sitePath = config.SitePath;
            var arguments = new[] { "check", "--path", sitePath };

            var output = ExecutePatchTool(patchToolPath, arguments);

            var match = PatchFoundRegex.Match(output);
            if (match.Success)
            {
                return new PatchCheckResult
                {
                    HasPatch = true,
                    Version = match.Groups[1].Value,
                    PatchNumber = match.Groups[2].Value,
                    Message = output
                };
            }

            return new PatchCheckResult
            {
                HasPatch = false,
                Message = output
            };
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to check for patches at {config.SiteName}: {ex.Message}");
            return new PatchCheckResult
            {
                HasPatch = false,
                Message = $"Error checking for patches: {ex.Message}"
            };
        }
    }

    public PatchResult ApplyPatch(PatchConfiguration config, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Applying patch at site: {config.SiteName} using version {config.Version.MinorVersion}");

        if (!IsPatchingSupportedForVersion(config.Version.MinorVersion))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {config.Version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = GetPatchToolPath(config.Version);
            var sitePath = config.SitePath;
            var argumentsList = new List<string> { "patch", "--path", sitePath };
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                argumentsList.AddRange(new[] { "--zip", backupPath });
            }

            var output = ExecutePatchTool(patchToolPath, argumentsList.ToArray());

            var match = PatchAppliedRegex.Match(output);
            if (match.Success)
            {
                return new PatchResult
                {
                    Success = true,
                    Version = match.Groups[1].Value,
                    PatchNumber = match.Groups[2].Value,
                    Message = output
                };
            }

            if (output.Contains("No patches are available for download"))
            {
                return new PatchResult
                {
                    Success = false,
                    Message = "No patches are available for download"
                };
            }

            return new PatchResult
            {
                Success = false,
                Message = output
            };
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to apply patch at {config.SiteName}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch: {ex.Message}"
            };
        }
    }

    public PatchResult ApplyPatchFromArchive(PatchConfiguration config, string archivePath, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Applying patch from archive {archivePath} at site: {config.SiteName} using version {config.Version.MinorVersion}");

        if (!IsPatchingSupportedForVersion(config.Version.MinorVersion))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {config.Version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = GetPatchToolPath(config.Version);
            var sitePath = config.SitePath;
            var argumentsList = new List<string> { "patch", "--path", sitePath, "--archive", archivePath };
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                argumentsList.AddRange(new[] { "--zip", backupPath });
            }

            var output = ExecutePatchTool(patchToolPath, argumentsList.ToArray());

            var match = PatchAppliedRegex.Match(output);
            if (match.Success)
            {
                return new PatchResult
                {
                    Success = true,
                    Version = match.Groups[1].Value,
                    PatchNumber = match.Groups[2].Value,
                    Message = output
                };
            }

            return new PatchResult
            {
                Success = false,
                Message = output
            };
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to apply patch from archive {archivePath} at {config.SiteName}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch from archive: {ex.Message}"
            };
        }
    }

    public PatchResult RollbackPatch(PatchConfiguration config, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Rolling back patch at site: {config.SiteName} using version {config.Version.MinorVersion}");

        if (!IsPatchingSupportedForVersion(config.Version.MinorVersion))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {config.Version.MinorVersion}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = GetPatchToolPath(config.Version);
            var sitePath = config.SitePath;
            var argumentsList = new List<string> { "rollback", "--path", sitePath };
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                argumentsList.AddRange(new[] { "--zip", backupPath });
            }

            var output = ExecutePatchTool(patchToolPath, argumentsList.ToArray());

            if (output.Contains("Rollback completed"))
            {
                return new PatchResult
                {
                    Success = true,
                    Message = "Rollback completed"
                };
            }

            if (output.Contains("Nothing to roll back"))
            {
                return new PatchResult
                {
                    Success = false,
                    Message = "Nothing to roll back. The site has not been patched."
                };
            }

            return new PatchResult
            {
                Success = false,
                Message = output
            };
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to rollback patch at {config.SiteName}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error rolling back patch: {ex.Message}"
            };
        }
    }


    private string ExecutePatchTool(string patchToolPath, string[] arguments)
    {
        _loggingService.WriteInfo($"Executing PatchTool: {patchToolPath} {string.Join(" ", arguments)}");

        var request = new ProcessExecutionRequest
        {
            ExecutablePath = patchToolPath,
            Arguments = arguments,
            UseRealTimeLogging = false,
            ThrowOnError = true
        };

        var result = _processManagerService.ExecuteProcess(request);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.ErrorMessage ?? "PatchTool execution failed");
        }

        return result.Output;
    }

    private static bool IsPatchingSupportedForVersion(string version)
    {
        // Parse version like "25.100.0248" or "24.200.0123"
        var parts = version.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[0], out var major) && int.TryParse(parts[1], out var minor))
        {
            // Patching is supported from 25R1 (25.100) onwards
            return major > 25 || (major == 25 && minor >= 100);
        }
        return false;
    }
}