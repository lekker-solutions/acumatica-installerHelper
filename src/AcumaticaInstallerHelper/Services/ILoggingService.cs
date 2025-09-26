namespace AcumaticaInstallerHelper.Services;

public enum LogLevel
{
    Debug,
    Info,
    Success,
    Warning,
    Error,
    Progress,
    Prompt
}

public interface ILoggingService
{
    void WriteLog(LogLevel level, string message);
    void WriteHeader(string title, string? subtitle = null);
    void WriteSection(string title);
    void WriteStep(string message);
    void WriteTable(Dictionary<string, string> data, string? title = null);
    void WriteSummary(string operation, string status, Dictionary<string, string>? details = null);
    void WriteProgress(string message, int? percentage = null);
    bool PromptYesNo(string message);
    void WriteDebug(string message);
    void WriteInfo(string message);
    void WriteSuccess(string message);
    void WriteWarning(string message);
    void WriteError(string message);
}