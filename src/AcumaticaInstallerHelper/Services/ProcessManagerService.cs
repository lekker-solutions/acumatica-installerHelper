using System.Diagnostics;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Services;

public interface IProcessManagerService
{
    ProcessResult ExecuteProcess(ProcessExecutionRequest request);
}

public class ProcessManagerService : IProcessManagerService
{
    private readonly ILoggingService _loggingService;

    public ProcessManagerService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public ProcessResult ExecuteProcess(ProcessExecutionRequest request)
    {
        if (!File.Exists(request.ExecutablePath))
        {
            _loggingService.WriteError($"Executable not found at: {request.ExecutablePath}");
            return new ProcessResult { Success = false, ErrorMessage = $"Executable not found at: {request.ExecutablePath}" };
        }

        _loggingService.WriteProgress($"Executing {Path.GetFileName(request.ExecutablePath)}...");
        _loggingService.WriteDebug($"Executable path: {request.ExecutablePath}");
        _loggingService.WriteDebug($"Arguments: {request.GetArgumentString()}");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(request.ExecutablePath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        if (request.Arguments != null)
        {
            foreach (var argument in request.Arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }
        }

        if (request.UseRealTimeLogging)
        {
            return ExecuteWithRealTimeLogging(process, request);
        }
        else
        {
            return ExecuteWithSynchronousOutput(process, request);
        }
    }

    private ProcessResult ExecuteWithRealTimeLogging(Process process, ProcessExecutionRequest request)
    {
        var outputBuffer = new List<string>();
        var errorBuffer = new List<string>();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuffer.Add(e.Data);
                _loggingService.WriteInfo($"{request.GetExecutableName()}: {e.Data}");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuffer.Add(e.Data);
                _loggingService.WriteWarning($"{request.GetExecutableName()}: {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var success = process.ExitCode == 0;

        if (success)
        {
            _loggingService.WriteSuccess($"{request.GetExecutableName()} completed successfully");
        }
        else
        {
            _loggingService.WriteError($"{request.GetExecutableName()} failed with exit code: {process.ExitCode}");
            if (errorBuffer.Count > 0)
            {
                _loggingService.WriteError("Error details:");
                foreach (string error in errorBuffer)
                    _loggingService.WriteError($"  {error}");
            }
        }

        return new ProcessResult
        {
            Success = success,
            ExitCode = process.ExitCode,
            Output = string.Join(Environment.NewLine, outputBuffer),
            ErrorOutput = string.Join(Environment.NewLine, errorBuffer),
            ErrorMessage = success ? null : $"Process failed with exit code {process.ExitCode}"
        };
    }

    private ProcessResult ExecuteWithSynchronousOutput(Process process, ProcessExecutionRequest request)
    {
        process.Start();

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        var success = process.ExitCode == 0;

        if (!success && !string.IsNullOrEmpty(error))
        {
            var errorMessage = $"{request.GetExecutableName()} failed with exit code {process.ExitCode}: {error}";
            if (request.ThrowOnError)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return new ProcessResult
            {
                Success = false,
                ExitCode = process.ExitCode,
                Output = output.Trim(),
                ErrorOutput = error.Trim(),
                ErrorMessage = errorMessage
            };
        }

        return new ProcessResult
        {
            Success = success,
            ExitCode = process.ExitCode,
            Output = output.Trim(),
            ErrorOutput = error.Trim()
        };
    }
}