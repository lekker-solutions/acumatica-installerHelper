using System.Diagnostics;
using System.Text.RegularExpressions;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class VersionService : IVersionService
{
    private readonly IConfigurationService _configService;
    private readonly ILoggingService _loggingService;
    private readonly HttpClient _httpClient;
    
    private static readonly Regex VersionRegex = new(@"^\d{2}\.\d{3}\.\d{4}$", RegexOptions.Compiled);

    public VersionService(
        IConfigurationService configService,
        ILoggingService loggingService,
        HttpClient httpClient)
    {
        _configService = configService;
        _loggingService = loggingService;
        _httpClient = httpClient;
    }

    public void ValidateVersionFormat(string version)
    {
        _loggingService.WriteDebug($"Validating version format: {version}");
        
        if (!VersionRegex.IsMatch(version))
        {
            _loggingService.WriteError($"Invalid version format: {version}");
            throw new VersionFormatException(version);
        }
        
        _loggingService.WriteDebug("Version format is valid");
    }

    public bool IsVersionInstalled(string version)
    {
        ValidateVersionFormat(version);
        var acExePath = GetAcuExePath(version);
        return File.Exists(acExePath);
    }

    public string GetVersionPath(string version)
    {
        return Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetVersionDirectory(),
            version
        );
    }

    public string GetAcuExePath(string version)
    {
        return Path.Combine(GetVersionPath(version), "Data", "ac.exe");
    }

    public List<AcumaticaVersion> GetInstalledVersions()
    {
        var versionsPath = Path.Combine(_configService.GetAcumaticaDirectory(), _configService.GetVersionDirectory());
        
        if (!Directory.Exists(versionsPath))
        {
            _loggingService.WriteDebug($"Versions directory does not exist: {versionsPath}");
            return new List<AcumaticaVersion>();
        }

        var versions = new List<AcumaticaVersion>();
        
        foreach (var directory in Directory.GetDirectories(versionsPath))
        {
            var versionName = Path.GetFileName(directory);
            
            if (!VersionRegex.IsMatch(versionName))
                continue;

            var acExePath = Path.Combine(directory, "Data", "ac.exe");
            if (!File.Exists(acExePath))
                continue;

            var version = new AcumaticaVersion
            {
                Version = versionName,
                Path = directory,
                InstallDate = Directory.GetCreationTime(directory),
                Size = GetDirectorySize(directory)
            };

            versions.Add(version);
        }

        return versions.OrderByDescending(v => v.InstallDate).ToList();
    }

    public async Task<bool> InstallVersionAsync(string version, bool isPreview = false, bool installDebugTools = false)
    {
        ValidateVersionFormat(version);
        
        _loggingService.WriteHeader("Acumatica Version Installation", $"Version {version}");
        
        if (IsVersionInstalled(version))
        {
            _loggingService.WriteWarning($"Version {version} is already installed");
            return true;
        }

        try
        {
            _loggingService.WriteSection("Downloading Version");
            
            var downloadUrl = GetDownloadUrl(version, isPreview, installDebugTools);
            _loggingService.WriteInfo($"Download URL: {downloadUrl}");
            
            var tempFile = await DownloadVersionAsync(downloadUrl, version);
            
            _loggingService.WriteSection("Installing Version");
            
            var success = await InstallMsiAsync(tempFile, version);
            
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
                _loggingService.WriteDebug($"Temporary file deleted: {tempFile}");
            }

            if (success)
            {
                _loggingService.WriteSummary("Version Installation", "Completed Successfully", new Dictionary<string, string>
                {
                    ["Version"] = version,
                    ["Install Path"] = GetVersionPath(version),
                    ["Preview"] = isPreview ? "Yes" : "No",
                    ["Debug Tools"] = installDebugTools ? "Yes" : "No"
                });
            }

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to install version {version}: {ex.Message}");
            _loggingService.WriteError($"Failed to install version {version}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveVersionAsync(string version)
    {
        ValidateVersionFormat(version);
        
        _loggingService.WriteHeader("Acumatica Version Removal", $"Version {version}");
        
        if (!IsVersionInstalled(version))
        {
            _loggingService.WriteWarning($"Version {version} is not installed");
            return true;
        }

        try
        {
            var versionPath = GetVersionPath(version);
            var size = GetDirectorySize(versionPath);
            
            _loggingService.WriteInfo($"Removing version from: {versionPath}");
            _loggingService.WriteInfo($"Freeing up: {FormatBytes(size)}");
            
            if (!_loggingService.PromptYesNo($"Are you sure you want to remove version {version}?"))
            {
                _loggingService.WriteInfo("Removal cancelled");
                return false;
            }

            Directory.Delete(versionPath, true);
            
            _loggingService.WriteSummary("Version Removal", "Completed Successfully", new Dictionary<string, string>
            {
                ["Version"] = version,
                ["Freed Space"] = FormatBytes(size)
            });

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to remove version {version}: {ex.Message}");
            _loggingService.WriteError($"Failed to remove version {version}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<AcumaticaVersion>> GetAvailableVersionsAsync(string? majorRelease = null, bool preview = false)
    {
        // TODO: Implement S3 bucket listing
        // For now, return empty list - this would require AWS SDK implementation
        _loggingService.WriteWarning("Online version listing not yet implemented");
        return new List<AcumaticaVersion>();
    }

    private string GetDownloadUrl(string version, bool isPreview, bool installDebugTools)
    {
        var baseUrl = "https://acumatica-builds.s3.amazonaws.com";
        var buildType = isPreview ? "preview" : "release";
        var toolsType = installDebugTools ? "_Tools" : "";
        
        return $"{baseUrl}/{buildType}/{version}/AcumaticaERP{toolsType}_{version}.msi";
    }

    private async Task<string> DownloadVersionAsync(string url, string version)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"AcumaticaERP_{version}.msi");
        
        _loggingService.WriteProgress("Downloading installer...");
        
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        await using var fileStream = File.Create(tempFile);
        await response.Content.CopyToAsync(fileStream);
        
        _loggingService.WriteSuccess($"Downloaded to: {tempFile}");
        
        return tempFile;
    }

    private async Task<bool> InstallMsiAsync(string msiPath, string version)
    {
        var installPath = GetVersionPath(version);
        Directory.CreateDirectory(installPath);

        var arguments = $"/i \"{msiPath}\" /quiet INSTALLLOCATION=\"{installPath}\"";
        
        _loggingService.WriteProgress("Running MSI installer...");
        _loggingService.WriteDebug($"MSI install arguments: {arguments}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
        {
            _loggingService.WriteSuccess("MSI installation completed successfully");
            return true;
        }
        else
        {
            _loggingService.WriteError($"MSI installation failed with exit code: {process.ExitCode}");
            return false;
        }
    }

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        try
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                           .Sum(file => new FileInfo(file).Length);
        }
        catch
        {
            return 0;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }
}