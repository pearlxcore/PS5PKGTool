using System.Collections.Concurrent;
using System.Diagnostics;
using DarkUI.Config;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private readonly ConcurrentQueue<LogEntry> _pendingLog = new();
    private readonly List<LogEntry> _logEntries = [];
    private readonly System.Windows.Forms.Timer _logRefreshTimer = new() { Interval = 250 };
    private int _logFilterLevel = -1;
    private bool _logAutoScroll = true;
    private const int MaxLogEntries = 5000;

    private void InitializeLog()
    {
        Logger.Logged -= OnLogged;
        Logger.Logged += OnLogged;
        _logRefreshTimer.Tick += (_, _) => FlushLog();
        _logRefreshTimer.Start();
    }

    private void ShutdownLog()
    {
        _logRefreshTimer.Stop();
        Logger.Logged -= OnLogged;
    }

    private void OnLogged(LogEntry entry) => _pendingLog.Enqueue(entry);

    private void FlushLog()
    {
        if (_pendingLog.IsEmpty) return;
        bool appended = false;
        while (_pendingLog.TryDequeue(out LogEntry entry))
        {
            _logEntries.Add(entry);
            if (MatchesLogFilter(entry))
            {
                AppendLogLine(entry);
                appended = true;
            }
        }
        TrimLogBuffer();
        if (appended && _logAutoScroll) txtLogView.ScrollToCaret();
    }

    private void AppendLogLine(LogEntry entry)
    {
        System.Windows.Forms.RichTextBox rtb = txtLogView.InnerRichTextBox;
        rtb.SelectionStart = rtb.TextLength;
        rtb.SelectionLength = 0;
        rtb.SelectionColor = LogLevelColor(entry.Level);
        rtb.AppendText($"{entry.Time:HH:mm:ss.fff} [{LogLevelText(entry.Level)}] {entry.Message}{Environment.NewLine}");
    }

    private void RebuildLogView()
    {
        txtLogView.Clear();
        foreach (LogEntry entry in _logEntries)
            if (MatchesLogFilter(entry))
                AppendLogLine(entry);
        if (_logAutoScroll) txtLogView.ScrollToCaret();
    }

    private void TrimLogBuffer()
    {
        if (_logEntries.Count <= MaxLogEntries + 1000) return;
        _logEntries.RemoveRange(0, _logEntries.Count - MaxLogEntries);
        RebuildLogView();
    }

    private bool MatchesLogFilter(LogEntry entry) => _logFilterLevel < 0 || (int)entry.Level == _logFilterLevel;

    private static Color LogLevelColor(LogLevel level) => level switch
    {
        LogLevel.Warn => Color.FromArgb(230, 180, 80),
        LogLevel.Error => Color.FromArgb(240, 110, 110),
        _ => Colors.LightText
    };

    private static string LogLevelText(LogLevel level) => level switch
    {
        LogLevel.Warn => "WARN",
        LogLevel.Error => "ERROR",
        _ => "INFO"
    };

    private void cboLogLevel_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _logFilterLevel = cboLogLevel.SelectedIndex - 1;
        RebuildLogView();
    }

    private void chkLogAutoScroll_CheckedChanged(object? sender, EventArgs e)
    {
        _logAutoScroll = chkLogAutoScroll.Checked;
        _settings.LogAutoScroll = _logAutoScroll;
        SaveSettingsQuietly();
    }

    private void btnLogClear_Click(object? sender, EventArgs e)
    {
        _logEntries.Clear();
        txtLogView.Clear();
    }

    private void btnLogOpenFolder_Click(object? sender, EventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Logger.LogDirectory);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Logger.LogDirectory}\"") { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            Logger.Warn($"Could not open the log folder: {ex.Message}");
        }
    }
}
