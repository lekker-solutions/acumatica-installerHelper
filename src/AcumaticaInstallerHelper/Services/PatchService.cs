using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AcumaticaInstallerHelper.Services;

public class PatchService : IPatchService
{
    private readonly IVersionService _versionService;
    private readonly ILoggingService _loggingService;
    
    private static readonly Regex PatchFoundRegex = new(@"A new patch found: (.+) P(\d+)", RegexOptions.Compiled);
    private static readonly Regex PatchAppliedRegex = new(@"The patch with the (.+) P(\d+) version has been applied", RegexOptions.Compiled);

    public PatchService(
        IVersionService versionService,
        ILoggingService loggingService)
    {
        _versionService = versionService;
        _loggingService = loggingService;
    }

    public Task<string> GetPatchToolPathAsync(string version)
    {
        if (!IsPatchingSupportedForVersion(version))
        {
            _loggingService.WriteError($"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later.");
            throw new NotSupportedException($"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later.");
        }

        var patchToolPath = _versionService.GetPatchUtilityPath(version);
        
        if (!File.Exists(patchToolPath))
        {
            _loggingService.WriteError($"PatchTool not found at {patchToolPath}. Ensure version {version} is installed.");
            throw new FileNotFoundException($"PatchTool not found for version {version}", patchToolPath);
        }

        return Task.FromResult(patchToolPath);
    }

    public Task<bool> IsPatchToolAvailableAsync(string version)
    {
        try
        {
            if (!IsPatchingSupportedForVersion(version))
            {
                return Task.FromResult(false);
            }

            var patchToolPath = _versionService.GetPatchUtilityPath(version);
            return Task.FromResult(File.Exists(patchToolPath));
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to check patch tool availability for version {version}: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public async Task<PatchCheckResult> CheckForPatchesAsync(string sitePath, string version)
    {
        _loggingService.WriteDebug($"Checking for patches at site path: {sitePath} using version {version}");

        if (!IsPatchingSupportedForVersion(version))
        {
            return new PatchCheckResult
            {
                HasPatch = false,
                Message = $"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = await GetPatchToolPathAsync(version);
            var arguments = $"check --path \"{sitePath}\"";

            var output = await ExecutePatchToolAsync(patchToolPath, arguments);

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
            _loggingService.WriteError($"Failed to check for patches at {sitePath}: {ex.Message}");
            return new PatchCheckResult
            {
                HasPatch = false,
                Message = $"Error checking for patches: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> ApplyPatchAsync(string sitePath, string version, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Applying patch at site path: {sitePath} using version {version}");

        if (!IsPatchingSupportedForVersion(version))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = await GetPatchToolPathAsync(version);
            var arguments = $"patch --path \"{sitePath}\"";
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                arguments += $" --zip \"{backupPath}\"";
            }

            var output = await ExecutePatchToolAsync(patchToolPath, arguments);

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
            _loggingService.WriteError($"Failed to apply patch at {sitePath}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> ApplyPatchFromArchiveAsync(string sitePath, string archivePath, string version, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Applying patch from archive {archivePath} at site path: {sitePath} using version {version}");

        if (!IsPatchingSupportedForVersion(version))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = await GetPatchToolPathAsync(version);
            var arguments = $"patch --path \"{sitePath}\" --archive \"{archivePath}\"";
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                arguments += $" --zip \"{backupPath}\"";
            }

            var output = await ExecutePatchToolAsync(patchToolPath, arguments);

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
            _loggingService.WriteError($"Failed to apply patch from archive {archivePath} at {sitePath}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch from archive: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> RollbackPatchAsync(string sitePath, string version, string? backupPath = null)
    {
        _loggingService.WriteDebug($"Rolling back patch at site path: {sitePath} using version {version}");

        if (!IsPatchingSupportedForVersion(version))
        {
            return new PatchResult
            {
                Success = false,
                Message = $"Patching is not supported for version {version}. Patching is only available for version 25R1 (25.100) and later."
            };
        }

        try
        {
            var patchToolPath = await GetPatchToolPathAsync(version);
            var arguments = $"rollback --path \"{sitePath}\"";
            
            if (!string.IsNullOrEmpty(backupPath))
            {
                arguments += $" --zip \"{backupPath}\"";
            }

            var output = await ExecutePatchToolAsync(patchToolPath, arguments);

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
            _loggingService.WriteError($"Failed to rollback patch at {sitePath}: {ex.Message}");
            return new PatchResult
            {
                Success = false,
                Message = $"Error rolling back patch: {ex.Message}"
            };
        }
    }


    private async Task<string> ExecutePatchToolAsync(string patchToolPath, string arguments)
    {
        _loggingService.WriteDebug($"Executing PatchTool with arguments: {arguments}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = patchToolPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
        {
            throw new InvalidOperationException($"PatchTool failed with exit code {process.ExitCode}: {error}");
        }

        return output.Trim();
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