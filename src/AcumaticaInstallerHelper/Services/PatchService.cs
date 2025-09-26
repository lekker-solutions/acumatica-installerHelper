using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AcumaticaInstallerHelper.Services;

public class PatchService : IPatchService
{
    private readonly IConfigurationService _configService;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<PatchService> _logger;
    private readonly HttpClient _httpClient;
    
    private static readonly string PatchToolUrl = "https://update.acumatica.com/rest/api/patch/tool";
    private static readonly Regex PatchFoundRegex = new(@"A new patch found: (.+) P(\d+)", RegexOptions.Compiled);
    private static readonly Regex PatchAppliedRegex = new(@"The patch with the (.+) P(\d+) version has been applied", RegexOptions.Compiled);

    public PatchService(
        IConfigurationService configService,
        ILoggingService loggingService,
        ILogger<PatchService> logger,
        HttpClient httpClient)
    {
        _configService = configService;
        _loggingService = loggingService;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<string> GetPatchToolPathAsync()
    {
        var toolsPath = Path.Combine(_configService.GetAcumaticaDirectory(), "Tools");
        var patchToolPath = Path.Combine(toolsPath, "PatchTool", "PatchTool.exe");

        if (!File.Exists(patchToolPath))
        {
            _logger.LogDebug("PatchTool not found at {Path}, downloading...", patchToolPath);
            await DownloadAndExtractPatchToolAsync(toolsPath);
        }

        return patchToolPath;
    }

    public async Task<bool> IsPatchToolAvailableAsync()
    {
        try
        {
            var patchToolPath = await GetPatchToolPathAsync();
            return File.Exists(patchToolPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check patch tool availability");
            return false;
        }
    }

    public async Task<PatchCheckResult> CheckForPatchesAsync(string sitePath)
    {
        _logger.LogDebug("Checking for patches at site path: {SitePath}", sitePath);

        try
        {
            var patchToolPath = await GetPatchToolPathAsync();
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
            _logger.LogError(ex, "Failed to check for patches at {SitePath}", sitePath);
            return new PatchCheckResult
            {
                HasPatch = false,
                Message = $"Error checking for patches: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> ApplyPatchAsync(string sitePath, string? backupPath = null)
    {
        _logger.LogDebug("Applying patch at site path: {SitePath}", sitePath);

        try
        {
            var patchToolPath = await GetPatchToolPathAsync();
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
            _logger.LogError(ex, "Failed to apply patch at {SitePath}", sitePath);
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> ApplyPatchFromArchiveAsync(string sitePath, string archivePath, string? backupPath = null)
    {
        _logger.LogDebug("Applying patch from archive {ArchivePath} at site path: {SitePath}", archivePath, sitePath);

        try
        {
            var patchToolPath = await GetPatchToolPathAsync();
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
            _logger.LogError(ex, "Failed to apply patch from archive {ArchivePath} at {SitePath}", archivePath, sitePath);
            return new PatchResult
            {
                Success = false,
                Message = $"Error applying patch from archive: {ex.Message}"
            };
        }
    }

    public async Task<PatchResult> RollbackPatchAsync(string sitePath, string? backupPath = null)
    {
        _logger.LogDebug("Rolling back patch at site path: {SitePath}", sitePath);

        try
        {
            var patchToolPath = await GetPatchToolPathAsync();
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
            _logger.LogError(ex, "Failed to rollback patch at {SitePath}", sitePath);
            return new PatchResult
            {
                Success = false,
                Message = $"Error rolling back patch: {ex.Message}"
            };
        }
    }

    private async Task DownloadAndExtractPatchToolAsync(string toolsPath)
    {
        _loggingService.WriteProgress("Downloading PatchTool from Acumatica...");
        
        try
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "PatchTool.zip");
            
            using var response = await _httpClient.GetAsync(PatchToolUrl);
            response.EnsureSuccessStatusCode();
            
            await using var fileStream = File.Create(tempFile);
            await response.Content.CopyToAsync(fileStream);
            fileStream.Close();

            var patchToolDir = Path.Combine(toolsPath, "PatchTool");
            Directory.CreateDirectory(patchToolDir);

            _loggingService.WriteProgress("Extracting PatchTool...");
            ZipFile.ExtractToDirectory(tempFile, patchToolDir, true);

            File.Delete(tempFile);
            _loggingService.WriteSuccess("PatchTool downloaded and cached successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download and extract PatchTool");
            throw;
        }
    }

    private async Task<string> ExecutePatchToolAsync(string patchToolPath, string arguments)
    {
        _logger.LogDebug("Executing PatchTool with arguments: {Arguments}", arguments);

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
}