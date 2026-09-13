using System.Text;

namespace PS5PKGTool.Infrastructure;

public enum LogLevel
{
    Info,
    Warn,
    Error
}

/// <summary>One structured log record, raised through <see cref="Logger.Logged"/>.</summary>
public readonly record struct LogEntry(DateTime Time, LogLevel Level, string Message);

/// <summary>
/// Minimal thread-safe file logger. Writes to <c>%LocalAppData%\PS5PKGTool\logs</c>, rotates once at
/// a soft size cap, and never throws because logging must not break the application. The file is
/// the full record of every log; <see cref="Logged"/> feeds the in-app Log tab.
/// </summary>
public static class Logger
{
    private static readonly object Gate = new();
    private const long MaximumBytes = 5 * 1024 * 1024;

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PS5PKGTool", "logs");

    public static string LogPath { get; } = Path.Combine(LogDirectory, "PS5PKGTool.log");

    public static event Action<LogEntry>? Logged;

    public static void Info(string message) => Write(LogLevel.Info, message);
    public static void Warn(string message) => Write(LogLevel.Warn, message);
    public static void Error(string message) => Write(LogLevel.Error, message);
    public static void Exception(string context, Exception exception) => Write(LogLevel.Error, $"{context}: {exception}");

    private static void Write(LogLevel level, string message)
    {
        var entry = new LogEntry(DateTime.Now, level, message);
        string line = $"{entry.Time:yyyy-MM-dd HH:mm:ss.fff} [{LevelText(level)}] {message}";
        try
        {
            lock (Gate)
            {
                System.IO.Directory.CreateDirectory(LogDirectory);
                if (File.Exists(LogPath) && new FileInfo(LogPath).Length > MaximumBytes)
                    File.Move(LogPath, LogPath + ".1", true);
                File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Logging is best-effort only.
        }
        Logged?.Invoke(entry);
    }

    private static string LevelText(LogLevel level) => level switch
    {
        LogLevel.Warn => "WARN",
        LogLevel.Error => "ERROR",
        _ => "INFO"
    };
}
