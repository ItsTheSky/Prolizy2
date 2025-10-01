using System;
using System.IO;
using System.Text;
using Avalonia.Media;
using Prolizy2.Views.Tabs;

namespace Prolizy2.Utilities;

/// <summary>
/// Logging class; sort of "wrapper" to handle <see cref="DebugTab"/>'s logs system.
/// </summary>
public static class Logs
{

    private static LogWriter LogWriter;
    public static void Init()
    {
        var systemOut = Console.Out;
        LogWriter = new LogWriter(systemOut);
        Console.SetOut(LogWriter);
    }
    
    public static void Info(string message)
    {
        LogWriter.WriteLine(message, LogType.Info);
    }
    
    public static void Warn(string message)
    {
        LogWriter.WriteLine(message, LogType.Warning);
    }
    
    public static void Error(string message)
    {
        LogWriter.WriteLine(message, LogType.Error);
    }
    
    public static void Debug(string message)
    {
        LogWriter.WriteLine(message, LogType.Debug);
    }
}

public class LogWriter : TextWriter
{
    private readonly TextWriter _originalOut;

    public LogWriter(TextWriter originalOut)
    {
        _originalOut = originalOut;
    }

    public override Encoding Encoding => _originalOut.Encoding;

    public override void WriteLine(string? value)
    {
        WriteLine(value, LogType.Info);
    }

    public void WriteLine(string? value, LogType logType)
    {
        // Write to the original console output
        _originalOut.WriteLine(value);

        // Also write to the DebugTab if it exists
        var prefix = logType.Prefix;
        var formattedMessage = $"[{prefix}] {value}";

        DebugTab.AddDebugText(formattedMessage);
    }
}

public record LogType
{
    public static readonly LogType Info = new("INFO", Brushes.Gray);
    public static readonly LogType Warning = new("WARN", Brushes.Orange);
    public static readonly LogType Error = new("ERR!", Brushes.Red);
    public static readonly LogType Debug = new("DBUG", Brushes.MediumPurple);
    
    public string Prefix { get; }
    public IBrush Color { get; }

    private LogType(string prefix, IBrush color)
    {
        Prefix = prefix;
        Color = color;
    }

    public static IBrush? MatchPrefix(string line)
    {
        // remove first char (it's a '[') then check remaining
        if (line.Length < 6) return null;
        var stripped = line[1..6];
        return stripped switch
        {
            "INFO]" => Info.Color,
            "WARN]" => Warning.Color,
            "ERR!]" => Error.Color,
            "DBUG]" => Debug.Color,
            _ => null
        };
    }
}