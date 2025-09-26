using Microsoft.Extensions.Logging;

namespace AcumaticaInstallerHelper.Services;

public class ConsoleLoggingService : ILoggingService
{
    private readonly ILogger<ConsoleLoggingService> _logger;

    public ConsoleLoggingService(ILogger<ConsoleLoggingService> logger)
    {
        _logger = logger;
    }

    public void WriteLog(LogLevel level, string message)
    {
        var (icon, color) = GetLogLevelDisplay(level);
        
        Console.ForegroundColor = color;
        Console.Write($"{icon} ");
        Console.ResetColor();
        Console.WriteLine(message);

        // Also log to Microsoft.Extensions.Logging
        var msLogLevel = ConvertLogLevel(level);
        _logger.Log(msLogLevel, "{Message}", message);
    }

    public void WriteHeader(string title, string? subtitle = null)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {title} ===");
        if (!string.IsNullOrEmpty(subtitle))
        {
            Console.WriteLine($"    {subtitle}");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    public void WriteSection(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"--- {title} ---");
        Console.ResetColor();
    }

    public void WriteStep(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("▶ ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public void WriteTable(Dictionary<string, string> data, string? title = null)
    {
        if (!string.IsNullOrEmpty(title))
        {
            WriteSection(title);
        }

        var maxKeyLength = data.Keys.Max(k => k.Length);
        
        foreach (var kvp in data)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{kvp.Key.PadRight(maxKeyLength)} : ");
            Console.ResetColor();
            Console.WriteLine(kvp.Value);
        }
        Console.WriteLine();
    }

    public void WriteSummary(string operation, string status, Dictionary<string, string>? details = null)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {operation} - {status}");
        Console.ResetColor();
        
        if (details != null)
        {
            WriteTable(details);
        }
    }

    public void WriteProgress(string message, int? percentage = null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("⏳ ");
        Console.ResetColor();
        
        if (percentage.HasValue)
        {
            Console.WriteLine($"{message} ({percentage}%)");
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    public bool PromptYesNo(string message)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("? ");
            Console.ResetColor();
            Console.Write($"{message} (Y/N): ");
            
            var response = Console.ReadLine()?.Trim().ToUpperInvariant();
            
            switch (response)
            {
                case "Y":
                case "YES":
                    return true;
                case "N":
                case "NO":
                    return false;
                default:
                    WriteWarning("Invalid input. Please enter Y or N");
                    continue;
            }
        }
    }

    public void WriteDebug(string message) => WriteLog(LogLevel.Debug, message);
    public void WriteInfo(string message) => WriteLog(LogLevel.Info, message);
    public void WriteSuccess(string message) => WriteLog(LogLevel.Success, message);
    public void WriteWarning(string message) => WriteLog(LogLevel.Warning, message);
    public void WriteError(string message) => WriteLog(LogLevel.Error, message);

    private static (string Icon, ConsoleColor Color) GetLogLevelDisplay(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ("🔍", ConsoleColor.DarkGray),
            LogLevel.Info => ("ℹ", ConsoleColor.White),
            LogLevel.Success => ("✅", ConsoleColor.Green),
            LogLevel.Warning => ("⚠", ConsoleColor.Yellow),
            LogLevel.Error => ("❌", ConsoleColor.Red),
            LogLevel.Progress => ("⏳", ConsoleColor.Yellow),
            LogLevel.Prompt => ("?", ConsoleColor.Magenta),
            _ => ("•", ConsoleColor.White)
        };
    }

    private static Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Success => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            LogLevel.Progress => Microsoft.Extensions.Logging.LogLevel.Information,
            LogLevel.Prompt => Microsoft.Extensions.Logging.LogLevel.Information,
            _ => Microsoft.Extensions.Logging.LogLevel.Information
        };
    }
}