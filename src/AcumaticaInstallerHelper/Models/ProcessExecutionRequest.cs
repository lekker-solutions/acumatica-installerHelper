namespace AcumaticaInstallerHelper.Models;

public class ProcessExecutionRequest
{
    public required string ExecutablePath { get; set; }
    public IEnumerable<string>? Arguments { get; set; }
    public bool UseRealTimeLogging { get; set; } = false;
    public bool ThrowOnError { get; set; } = false;

    public string GetArgumentString()
    {
        if (Arguments != null)
            return string.Join(" ", Arguments);

        return string.Empty;
    }

    public string GetExecutableName()
    {
        return Path.GetFileNameWithoutExtension(ExecutablePath);
    }
}

public class ProcessResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ErrorOutput { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}