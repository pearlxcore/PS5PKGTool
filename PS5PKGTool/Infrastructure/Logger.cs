using System.Text;

namespace PS5PKGTool.Infrastructure;

/// <summary>
/// Minimal thread-safe file logger. Writes to <c>%LocalAppData%\PS5PKGTool\logs</c>, rotates once at
/// a soft size cap, and never throws because logging must not break the application.
/// </summary>
public static class Logger
{
    private static readonly object Gate = new();
    private const long MaximumBytes = 5 * 1024 * 1024;

    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PS5PKGTool", "logs");

    public static string LogPath { get; } = Path.Combine(LogDirectory, "PS5PKGTool.log");

    public static event Action<string>? Logged;

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);
    public static void Exception(string context, Exception exception) => Write("ERROR", $"{context}: {exception}");

    private static void Write(string level, string message)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
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
        Logged?.Invoke(line);
    }
}
