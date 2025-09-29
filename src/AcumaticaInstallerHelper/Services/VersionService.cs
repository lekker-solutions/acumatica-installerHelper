using System.Diagnostics;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public class VersionService : IVersionService
{
    private readonly IConfigurationService _configService;
    private readonly ILoggingService       _loggingService;
    private readonly HttpClient            _httpClient;


    public VersionService(
        IConfigurationService configService,
        ILoggingService       loggingService,
        HttpClient            httpClient)
    {
        _configService  = configService;
        _loggingService = loggingService;
        _httpClient     = httpClient;
    }

    public bool IsVersionInstalled(AcumaticaVersion version)
    {
        string acExePath = GetAcuExePath(version);
        return File.Exists(acExePath);
    }

    public string GetVersionPath(AcumaticaVersion version)
    {
        return Path.Combine(
            _configService.GetAcumaticaDirectory(),
            _configService.GetVersionDirectory(),
            version.MinorVersion
        );
    }

    public string GetAcuExePath(AcumaticaVersion version)
    {
        return Path.Combine(GetVersionPath(version), "Data", "ac.exe");
    }

    public string GetPatchUtilityPath(AcumaticaVersion version)
    {
        return Path.Combine(GetVersionPath(version), "Data", "PatchUtility", "PatchTool.exe");
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

            string acExePath = Path.Combine(directory, "Data", "ac.exe");
            if (!File.Exists(acExePath))
                continue;

            AcumaticaVersion version = new()
            {
                Version     = versionName,
                Path        = directory,
                InstallDate = Directory.GetCreationTime(directory),
                Size        = GetDirectorySize(directory)
            };

            versions.Add(version);
        }

        return versions.OrderByDescending(v => v.InstallDate).ToList();
    }

    public bool InstallVersion(VersionConfiguration config)
    {
        _loggingService.WriteHeader("Acumatica Version Installation", $"Version {config.Version.MinorVersion}");

        if (!config.ForceInstall && IsVersionInstalled(config.Version))
        {
            _loggingService.WriteWarning($"Version {config.Version.MinorVersion} is already installed");
            return true;
        }

        try
        {
            _loggingService.WriteSection("Downloading Version");

            string downloadUrl = GetDownloadUrl(config.Version);
            _loggingService.WriteInfo($"Download URL: {downloadUrl}");

            string tempFile = DownloadVersion(downloadUrl, config.Version.MinorVersion);

            _loggingService.WriteSection("Installing Version");

            // Apply debug tools setting from config
            config.Version.DebuggerTools = config.InstallDebugTools;
            bool success = InstallMsi(tempFile, config.Version, config.VersionPath);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
                _loggingService.WriteDebug($"Temporary file deleted: {tempFile}");
            }

            if (success)
                _loggingService.WriteSummary("Version Installation", "Completed Successfully",
                    new Dictionary<string, string>
                    {
                        ["Version"]      = config.Version.MinorVersion,
                        ["Install Path"] = config.VersionPath,
                        ["Preview"]      = config.Version.IsPreview ? "Yes" : "No",
                        ["Debug Tools"]  = config.InstallDebugTools ? "Yes" : "No"
                    });

            return success;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to install version {config.Version.MinorVersion}: {ex.Message}");
            return false;
        }
    }

    public bool RemoveVersion(VersionConfiguration config)
    {
        _loggingService.WriteHeader("Acumatica Version Removal", $"Version {config.Version.MinorVersion}");

        if (!IsVersionInstalled(config.Version))
        {
            _loggingService.WriteWarning($"Version {config.Version.MinorVersion} is not installed");
            return true;
        }

        try
        {
            string versionPath = config.VersionPath ?? GetVersionPath(config.Version);
            long   size        = GetDirectorySize(versionPath);

            _loggingService.WriteInfo($"Removing version from: {versionPath}");
            _loggingService.WriteInfo($"Freeing up: {FormatBytes(size)}");

            if (!_loggingService.PromptYesNo($"Are you sure you want to remove version {config.Version.MinorVersion}?"))
            {
                _loggingService.WriteInfo("Removal cancelled");
                return false;
            }

            Directory.Delete(versionPath, true);

            _loggingService.WriteSummary("Version Removal", "Completed Successfully", new Dictionary<string, string>
            {
                ["Version"]     = config.Version.MinorVersion,
                ["Freed Space"] = FormatBytes(size)
            });

            return true;
        }
        catch (Exception ex)
        {
            _loggingService.WriteError($"Failed to remove version {config.Version.MinorVersion}: {ex.Message}");
            return false;
        }
    }

    public List<AcumaticaVersion> GetAvailableVersions(string? majorRelease = null, bool preview = false)
    {
        // TODO: Implement S3 bucket listing
        // For now, return empty list - this would require AWS SDK implementation
        _loggingService.WriteWarning("Online version listing not yet implemented");
        return new List<AcumaticaVersion>();
    }

    private string GetDownloadUrl(AcumaticaVersion version)
    {
        var baseUrl = "https://acumatica-builds.s3.amazonaws.com";

        // Get Major
        string[] parts        = version.MinorVersion.Split('.');
        int      minor        = int.Parse(parts[1]);
        var      roundedMinor = (int)Math.Round(minor / 100.0);
        var      majorVersion = $"{parts[0]}.{roundedMinor}";

        var url = $"{baseUrl}/builds/";
        if (version.IsPreview)
            // Preview versions follow the builds/preview/major.minor/version/AcumaticaERP/ pattern
            url += "preview/";

        url += $"{majorVersion}/{version}/AcumaticaERP/AcumaticaERPInstall.msi";
        return url;
    }

    private string DownloadVersion(string url, string version)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"AcumaticaERP_{version}.msi");
        _loggingService.WriteProgress("Downloading installer...");

        using HttpResponseMessage response = _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
        response.EnsureSuccessStatusCode();

        long             totalBytes    = response.Content.Headers.ContentLength ?? -1;
        using Stream     contentStream = response.Content.ReadAsStreamAsync().Result;
        using FileStream fileStream    = File.Create(tempFile);

        var  buffer          = new byte[8192];
        long downloadedBytes = 0;
        int  bytesRead;
        var  lastReportedPercentage = 0;

        _loggingService.WriteProgressBar("Downloading version " + version, 0);
        while ((bytesRead = contentStream.ReadAsync(buffer, 0, buffer.Length).Result) > 0)
        {
            fileStream.WriteAsync(buffer, 0, bytesRead).Wait();
            downloadedBytes += bytesRead;

            if (totalBytes > 0)
            {
                var percentage     = (int)(downloadedBytes * 100 / totalBytes);
                int percentageTens = percentage / 10 * 10; // Round down to nearest 10

                if (percentageTens > lastReportedPercentage)
                {
                    _loggingService.WriteProgressBar("                               ", percentageTens);
                    lastReportedPercentage = percentageTens;
                }
            }
        }

        _loggingService.WriteSuccess($"Downloaded to: {tempFile}");
        return tempFile;
    }

    private bool InstallMsi(string msiPath, AcumaticaVersion version, string versionPath)
    {
        var installPath = versionPath;
        Directory.CreateDirectory(installPath);

        string features                     = string.Empty;
        if (version.DebuggerTools) features = "ADDLOCAL=DEBUGGERTOOLS";
        var arguments                       = $"/a \"{msiPath}\" {features} /qb TARGETDIR=\"{installPath}\"";

        _loggingService.WriteProgress("Running MSI installer...");
        _loggingService.WriteDebug($"MSI install arguments: {arguments}");

        using Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "msiexec.exe",
                Arguments              = arguments,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                _loggingService.WriteDebug(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                _loggingService.WriteError(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            // Delete all MSI files
            foreach (var msiFile in Directory.GetFiles(installPath, "*.msi"))
            {
                File.Delete(msiFile);
            }

            // Move files from Acumatica ERP directory to installPath
            var acumaticaDir = Path.Combine(installPath, "Acumatica ERP");
            if (Directory.Exists(acumaticaDir))
            {
                foreach (var file in Directory.GetFiles(acumaticaDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath    = Path.GetRelativePath(acumaticaDir, file);
                    var    destinationPath = Path.Combine(installPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    File.Move(file, destinationPath);
                }

                Directory.Delete(acumaticaDir, true);
            }

            _loggingService.WriteSuccess("MSI installation completed successfully");
            return true;
        }

        _loggingService.WriteError($"MSI installation failed with exit code: {process.ExitCode}");
        return false;
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
        var      counter  = 0;
        decimal  number   = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n1} {suffixes[counter]}";
    }
}