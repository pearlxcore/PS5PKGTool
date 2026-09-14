using System.Diagnostics;
using System.Text.Json;
using DarkUI.Controls;
using DarkUI.Forms;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Tasks;
using PS5PKGTool.Ffpfsc;
using PS5PKGTool.Infrastructure;
using UFS2Tool;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private readonly PackageTaskQueue _taskQueue = new();
    private readonly System.Windows.Forms.Timer _taskRefreshTimer = new() { Interval = 250 };
    private bool _taskRefreshPending;
    private readonly HashSet<string> _notifiedTasks = new(StringComparer.Ordinal);
    private bool _autoFollowRunning = true;
    private bool _suppressTaskSelection;
    private bool _taskGroupByStatus;

    private void InitializeTaskQueue()
    {
        _taskQueue.TasksChanged += TaskQueue_Changed;
        _taskRefreshTimer.Tick += (_, _) => FlushTaskRefresh();
        _taskRefreshTimer.Start();
        _taskQueue.SetPersistencePath(TaskQueuePath());
        int restored = _taskQueue.RestoreFromDisk(RebuildTask);
        if (restored > 0) _taskRefreshPending = true;
        chkTaskAutoStart.Checked = _taskQueue.AutoStart;
        cboTaskGroup.SelectedIndex = 0;
        RefreshTaskGrid();
    }

    private void chkTaskAutoStart_CheckedChanged(object? sender, EventArgs e) => _taskQueue.AutoStart = chkTaskAutoStart.Checked;

    private void cboTaskGroup_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _taskGroupByStatus = cboTaskGroup.SelectedIndex == 1;
        RefreshTaskGrid();
    }

    private void btnTaskStart_Click(object? sender, EventArgs e) => _taskQueue.StartNext();

    private void btnTaskCancel_Click(object? sender, EventArgs e) => CancelSelectedTask();

    private void btnTaskRetry_Click(object? sender, EventArgs e) => RetrySelectedTask();

    private void btnTaskRemove_Click(object? sender, EventArgs e) => RemoveSelectedTask();

    private void btnTaskOpen_Click(object? sender, EventArgs e) => OpenSelectedTaskOutput();

    private void btnTaskClear_Click(object? sender, EventArgs e)
    {
        _taskQueue.ClearCompleted();
        RefreshTaskGrid();
    }

    private void gridTasks_SelectionChanged(object? sender, EventArgs e)
    {
        if (!_suppressTaskSelection) _autoFollowRunning = false;
        UpdateTaskDetails();
    }

    private void gridTasks_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.RowIndex >= gridTasks.Rows.Count) return;
        DataGridViewRow row = gridTasks.Rows[e.RowIndex];
        if (gridTasks.IsGroupRow(row)) return;
        gridTasks.ClearSelection();
        row.Selected = true;
        if (e.ColumnIndex >= 0) gridTasks.CurrentCell = row.Cells[e.ColumnIndex];
    }

    private void gridTasks_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0) OpenSelectedTaskOutput();
    }

    private static string TaskQueuePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PS5PKGTool", "tasks.json");

    private QueuedPackageTask EnqueueTask(string type, string displayName,
        Func<IProgress<PackageTaskProgress>, CancellationToken, Task> execute,
        string sourcePath = "", string outputPath = "", Action<QueuedPackageTask>? onFinished = null,
        string? payload = null, string operation = "", string sourceFormat = "", string targetFormat = "",
        PackageTaskStage[]? stagePlan = null)
    {
        var task = new QueuedPackageTask
        {
            Id = Guid.NewGuid().ToString("N"),
            Type = type,
            DisplayName = displayName,
            SourcePath = sourcePath,
            OutputPath = outputPath,
            PersistencePayload = payload,
            Operation = operation,
            SourceFormat = sourceFormat,
            TargetFormat = targetFormat,
            StagePlan = stagePlan ?? [],
            Execute = execute
        };
        PackageTaskStatus lastLoggedStatus = PackageTaskStatus.Queued;
        task.Changed += (sender, args) =>
        {
            TaskQueue_Changed(sender, args);
            if (task.Status != lastLoggedStatus)
            {
                LogTaskStatus(task);
                lastLoggedStatus = task.Status;
            }
            if (task.IsTerminal && _notifiedTasks.Add(task.Id))
                RunOnUi(() => onFinished?.Invoke(task));
            if (task.IsTerminal && task.Status == PackageTaskStatus.Completed &&
                _settings.OpenOutputAfterTask && !string.IsNullOrEmpty(task.OutputPath) &&
                _notifiedTasks.Add("open:" + task.Id))
                RunOnUi(() => OpenOutputFolder(task.OutputPath));
        };
        _taskQueue.Enqueue(task);
        Logger.Info($"Queued task: {displayName}");
        _taskRefreshPending = true;
        if (tabTasks is not null) tabsWorkspace.SelectedTab = tabTasks;
        _autoFollowRunning = true;
        RefreshTaskGrid();
        return task;
    }

    private void LogTaskStatus(QueuedPackageTask task)
    {
        switch (task.Status)
        {
            case PackageTaskStatus.Running:
                Logger.Info($"Task started: {task.DisplayName}");
                break;
            case PackageTaskStatus.Completed:
                Logger.Info($"Task completed: {task.DisplayName}");
                break;
            case PackageTaskStatus.Failed:
                Logger.Error($"Task failed: {task.DisplayName}: {task.Message}");
                if (task.Failure is not null) Logger.Error(task.Failure.ToString());
                if (task.Failure is not null && Ps5DiskSpace.IsInsufficient(task.Failure))
                {
                    string message = Ps5DiskSpace.Describe(task.Failure);
                    RunOnUi(() => AppDialog.ShowError(
                        "Not enough free disk space to complete the build.\n\n" + message +
                        "\n\nFree space, or choose a different workspace or output volume.",
                        "Package build"));
                }
                break;
            case PackageTaskStatus.Cancelled:
                Logger.Warn($"Task cancelled: {task.DisplayName}");
                break;
        }
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing) return;
        if (InvokeRequired)
        {
            try { BeginInvoke(action); }
            catch (InvalidOperationException) { }
        }
        else action();
    }

    private void TaskQueue_Changed(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing) return;
        try
        {
            if (InvokeRequired) BeginInvoke(new Action(() => _taskRefreshPending = true));
            else _taskRefreshPending = true;
        }
        catch (InvalidOperationException) { }
    }

    private void FlushTaskRefresh()
    {
        if (!_taskRefreshPending || IsDisposed) return;
        _taskRefreshPending = false;
        RefreshTaskGrid();
    }

    private void RefreshTaskGrid()
    {
        string? selectedId = SelectedTask()?.Id;
        string? followId = _autoFollowRunning
            ? _taskQueue.Tasks.FirstOrDefault(task =>
                task.Status is PackageTaskStatus.Running or PackageTaskStatus.Cancelling)?.Id
            : null;
        string? targetId = followId ?? selectedId;
        IReadOnlyList<QueuedPackageTask> tasks = _taskQueue.Tasks;

        gridTasks.SuspendLayout();
        try
        {
            if (_taskGroupByStatus)
            {
                gridTasks.SetGroups(tasks, task => StatusText(task.Status), FillTaskRow, TaskGroupComparer);
            }
            else
            {
                gridTasks.ClearGroups();
                gridTasks.Rows.Clear();
                foreach (QueuedPackageTask task in tasks)
                {
                    DataGridViewRow row = gridTasks.Rows[gridTasks.Rows.Add()];
                    row.Tag = task;
                    FillTaskRow(row, task);
                }
            }

            if (targetId is not null)
                foreach (DataGridViewRow row in gridTasks.Rows)
                    if (row.Tag is QueuedPackageTask candidate &&
                        string.Equals(candidate.Id, targetId, StringComparison.Ordinal))
                    {
                        _suppressTaskSelection = true;
                        try
                        {
                            row.Selected = true;
                            if (row.Index >= 0) gridTasks.CurrentCell = row.Cells[0];
                        }
                        finally
                        {
                            _suppressTaskSelection = false;
                        }
                        if (row.Index >= 0) gridTasks.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
        }
        finally
        {
            gridTasks.ResumeLayout();
        }
        UpdateTaskSummary();
        UpdateTaskDetails();
    }

    private void FillTaskRow(DataGridViewRow row, QueuedPackageTask task)
    {
        row.Cells[0].Value = task.SourcePath.Length > 0 ? Path.GetFileName(task.SourcePath) : task.DisplayName;
        row.Cells[1].Value = task.Operation.Length > 0 ? task.Operation : task.Type;
        row.Cells[2].Value = task.FormatRoute;
        row.Cells[3].Value = StatusText(task.Status);
        row.Cells[4].Value = StageText(task);
        row.Cells[5].Value = ProgressText(task);
        row.Cells[6].Value = ElapsedText(task);
        Color? tint = task.Status switch
        {
            PackageTaskStatus.Running => Color.FromArgb(45, 74, 110),
            PackageTaskStatus.Cancelling => Color.FromArgb(120, 80, 40),
            PackageTaskStatus.Failed or PackageTaskStatus.Interrupted => Color.FromArgb(90, 45, 45),
            _ => null
        };
        if (tint is { } color)
        {
            row.DefaultCellStyle.BackColor = color;
            row.DefaultCellStyle.ForeColor = Color.FromArgb(232, 232, 232);
        }
    }

    private void UpdateTaskSummary()
    {
        IReadOnlyList<QueuedPackageTask> tasks = _taskQueue.Tasks;
        if (tasks.Count == 0)
        {
            lblTaskSummary.Text = "Queue is empty.";
            return;
        }
        int running = tasks.Count(task => task.Status is PackageTaskStatus.Running or PackageTaskStatus.Cancelling);
        int queued = tasks.Count(task => task.Status == PackageTaskStatus.Queued);
        int completed = tasks.Count(task => task.Status == PackageTaskStatus.Completed);
        int failed = tasks.Count(task => task.Status is PackageTaskStatus.Failed or PackageTaskStatus.Interrupted);
        lblTaskSummary.Text =
            $"{tasks.Count:N0} task(s)   |   {running} running   |   {queued} queued   |   {completed} done   |   {failed} failed";
    }

    private void UpdateTaskDetails()
    {
        UpdateTaskControls();
        QueuedPackageTask? task = SelectedTask();
        if (task is null)
        {
            lblTaskStage.Text = "No task selected.";
            barTaskCurrent.Value = 0;
            barTaskOverall.Value = 0;
            lblTaskMessage.Text = string.Empty;
            lblTaskMeta.Text = string.Empty;
            return;
        }
        PackageTaskProgress progress = task.Progress;
        string stage = string.IsNullOrWhiteSpace(progress.Stage) ? task.DisplayName : progress.Stage;
        string stepText = progress.TotalSteps > 0
            ? $"Step {Math.Min(progress.Step + 1, progress.TotalSteps)} of {progress.TotalSteps}"
            : string.Empty;
        lblTaskStage.Text = stepText.Length > 0
            ? $"{stepText}: {stage}  -  {StatusText(task.Status)}"
            : $"{stage}  -  {StatusText(task.Status)}";
        lblTaskCurrentCaption.Text = "Step progress";
        barTaskCurrent.Value = Math.Clamp((int)Math.Round(progress.OperationPercent * 100d),
            barTaskCurrent.Minimum, barTaskCurrent.Maximum);
        lblTaskOverallCaption.Text = progress.TotalSteps > 1
            ? $"Task steps ({Math.Min(progress.Step, progress.TotalSteps)}/{progress.TotalSteps})"
            : "Task steps";
        barTaskOverall.Value = Math.Clamp((int)Math.Round(TaskPercent(task) * 100d),
            barTaskOverall.Minimum, barTaskOverall.Maximum);
        string message = task.Message;
        if (!string.IsNullOrWhiteSpace(progress.CurrentFile)) message = $"{message}  {progress.CurrentFile}";
        lblTaskMessage.Text = message.Trim();
        var parts = new List<string>();
        if (task.Operation.Length > 0) parts.Add(task.Operation);
        if (task.FormatRoute.Length > 0) parts.Add(task.FormatRoute);
        if (task.SourcePath.Length > 0) parts.Add("From " + Path.GetFileName(task.SourcePath));
        if (task.OutputPath.Length > 0) parts.Add("To " + Path.GetFileName(task.OutputPath));
        parts.Add("Elapsed " + ElapsedText(task));
        if (EtaText(task) is { } eta) parts.Add(eta);
        lblTaskMeta.Text = string.Join("   |   ", parts);
    }

    private static double TaskPercent(QueuedPackageTask task) => task.Status == PackageTaskStatus.Completed
        ? 1d
        : task.Progress.TaskPercent;

    private static string StageText(QueuedPackageTask task) => task.Status is PackageTaskStatus.Running
        or PackageTaskStatus.Cancelling && !string.IsNullOrWhiteSpace(task.Progress.Stage)
        ? task.Progress.Stage + "\u2026"
        : string.Empty;

    private static string ElapsedText(QueuedPackageTask task)
    {
        DateTime? start = task.StartedUtc ?? (task.Status == PackageTaskStatus.Queued ? null : task.CreatedUtc);
        if (start is null) return "-";
        DateTime end = task.CompletedUtc ?? DateTime.UtcNow;
        return FormatDuration(end - start.Value);
    }

    private static string? EtaText(QueuedPackageTask task)
    {
        if (task.Status != PackageTaskStatus.Running || task.StartedUtc is null) return null;
        double percent = task.Progress.TaskPercent;
        if (percent is <= 0.03d or >= 0.999d) return null;
        TimeSpan elapsed = DateTime.UtcNow - task.StartedUtc.Value;
        double totalSeconds = elapsed.TotalSeconds / percent;
        return "ETA " + FormatDuration(TimeSpan.FromSeconds(Math.Max(0d, totalSeconds - elapsed.TotalSeconds)));
    }

    private static string FormatDuration(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        return value.TotalHours >= 1d
            ? $"{(int)value.TotalHours}:{value.Minutes:00}:{value.Seconds:00}"
            : value.TotalMinutes >= 1d
                ? $"{value.Minutes:00}:{value.Seconds:00}"
                : $"{value.Seconds}s";
    }

    private static readonly IComparer<object> TaskGroupComparer = Comparer<object>.Create(
        (left, right) => TaskGroupOrder(left as string).CompareTo(TaskGroupOrder(right as string)));

    private static int TaskGroupOrder(string? status) => status switch
    {
        "Running" => 0,
        "Cancelling" => 1,
        "Queued" => 2,
        "Failed" => 3,
        "Interrupted" => 4,
        "Completed" => 5,
        "Cancelled" => 6,
        _ => 7
    };

    private QueuedPackageTask? SelectedTask()
    {
        DataGridViewRow? row = gridTasks.CurrentRow;
        if (row is null || gridTasks.IsGroupRow(row)) return null;
        return row.Tag as QueuedPackageTask;
    }

    private void CancelSelectedTask()
    {
        if (SelectedTask() is { } task) _taskQueue.Cancel(task.Id);
        RefreshTaskGrid();
    }

    private void RetrySelectedTask()
    {
        if (SelectedTask() is { } task) _taskQueue.Retry(task.Id);
        RefreshTaskGrid();
    }

    private void RemoveSelectedTask()
    {
        if (SelectedTask() is { } task) _taskQueue.Remove(task.Id);
        RefreshTaskGrid();
    }

    private void OpenSelectedTaskOutput()
    {
        if (SelectedTask() is not { } task) return;
        string output = task.OutputPath;
        try
        {
            if (File.Exists(output))
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{output}\"") { UseShellExecute = true });
            else if (Directory.Exists(output))
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{output}\"") { UseShellExecute = true });
            else if (File.Exists(task.SourcePath))
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{task.SourcePath}\"") { UseShellExecute = true });
            else
                AppDialog.ShowInformation("The task has no existing output or source path to open.", "PS5 PKG Tool");
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or ArgumentException)
        {
            AppDialog.ShowError(ex.Message, "Open task output");
        }
    }

    private void UpdateTaskControls()
    {
        IReadOnlyList<QueuedPackageTask> tasks = _taskQueue.Tasks;
        QueuedPackageTask? task = SelectedTask();
        bool busy = task is { Status: PackageTaskStatus.Running or PackageTaskStatus.Cancelling };

        btnTaskStart.Enabled = tasks.Any(candidate => candidate.Status == PackageTaskStatus.Queued);
        btnTaskCancel.Enabled = task is { Status: PackageTaskStatus.Queued or PackageTaskStatus.Running };
        btnTaskRetry.Enabled = task is
        {
            Status: PackageTaskStatus.Failed or PackageTaskStatus.Cancelled or PackageTaskStatus.Interrupted
        };
        btnTaskRemove.Enabled = task is not null && !busy;
        btnTaskOpen.Enabled = task is not null && (task.OutputPath.Length > 0 || task.SourcePath.Length > 0);
        btnTaskClear.Enabled = tasks.Any(candidate => candidate.IsTerminal);

        menuTaskStart.Enabled = btnTaskStart.Enabled;
        menuTaskCancel.Enabled = btnTaskCancel.Enabled;
        menuTaskRetry.Enabled = btnTaskRetry.Enabled;
        menuTaskRemove.Enabled = btnTaskRemove.Enabled;
        menuTaskOpen.Enabled = btnTaskOpen.Enabled;
        menuTaskClear.Enabled = btnTaskClear.Enabled;
    }

    private void contextTasks_Opening(object? sender, System.ComponentModel.CancelEventArgs e) => UpdateTaskControls();

    private void ShutdownTaskQueue()
    {
        _taskRefreshTimer.Stop();
        try { _taskQueue.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(3)); }
        catch (AggregateException) { }
    }

    private void CancelQueueTasks()
    {
        foreach (QueuedPackageTask task in _taskQueue.Tasks)
            if (task.Status is PackageTaskStatus.Running or PackageTaskStatus.Queued)
                _taskQueue.Cancel(task.Id);
    }

    private static string StatusText(PackageTaskStatus status) => status switch
    {
        PackageTaskStatus.Queued => "Queued",
        PackageTaskStatus.Running => "Running",
        PackageTaskStatus.Cancelling => "Cancelling",
        PackageTaskStatus.Completed => "Completed",
        PackageTaskStatus.Failed => "Failed",
        PackageTaskStatus.Cancelled => "Cancelled",
        PackageTaskStatus.Interrupted => "Interrupted",
        _ => status.ToString()
    };

    private static string ProgressText(QueuedPackageTask task) =>
        task.IsTerminal || task.Status == PackageTaskStatus.Queued
            ? StatusText(task.Status)
            : $"{TaskPercent(task) * 100d:N0}%";

    private static IProgress<SonyDebugPackageProgress> AdaptSonyPkgProgress(IProgress<PackageTaskProgress> target) =>
        new Progress<SonyDebugPackageProgress>(value => target.Report(new PackageTaskProgress(
            value.Stage, 0, 0, value.CompletedBytes, value.TotalBytes, 0, 0, value.CurrentPath)));

    private static IProgress<FfpfscProgress> AdaptFfpfscProgress(IProgress<PackageTaskProgress> target) =>
        new Progress<FfpfscProgress>(value => target.Report(new PackageTaskProgress(
            value.Stage, 0, 0, value.BytesProcessed, value.TotalBytes, 0, 0, string.Empty)));

    private static IProgress<Ufs2Progress> AdaptUfs2Progress(IProgress<PackageTaskProgress> target) =>
        new Progress<Ufs2Progress>(value => target.Report(new PackageTaskProgress(
            value.Stage, 0, 0, value.BytesProcessed, value.TotalBytes,
            checked((int)Math.Min(value.Completed, int.MaxValue)),
            checked((int)Math.Min(value.Total, int.MaxValue)), value.Unit)));

    private static IProgress<SonyPackageExtractProgress> AdaptSonyExtractProgress(IProgress<PackageTaskProgress> target) =>
        new Progress<SonyPackageExtractProgress>(value => target.Report(new PackageTaskProgress(
            "Extracting", 0, 0, value.CompletedBytes, value.TotalBytes, 0, 0, value.CurrentPath)));

    // ---------------------------------------------------------------- persistence / resume

    private static string Payload(params (string Key, string? Value)[] fields)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string key, string? value) in fields)
            if (value is not null) map[key] = value;
        return JsonSerializer.Serialize(map);
    }

    private static Dictionary<string, string> ReadPayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(payload)
                   ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch (JsonException) { return new Dictionary<string, string>(StringComparer.Ordinal); }
    }

    private static string Get(Dictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) ? value : string.Empty;

    private static int GetInt(Dictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) && int.TryParse(value, out int parsed) ? parsed : 0;

    private static bool GetBool(Dictionary<string, string> fields, string key) =>
        fields.TryGetValue(key, out string? value) && bool.TryParse(value, out bool parsed) && parsed;

    private QueuedPackageTask? RebuildTask(PersistedPackageTask entry)
    {
        Dictionary<string, string> fields = ReadPayload(entry.Payload);
        Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? execute = entry.Type switch
        {
            PackageTaskTypes.SonyDebugPkg => RebuildSonyDebugPkg(fields),
            PackageTaskTypes.FfpfscFromDump => RebuildFfpfscFromDump(fields),
            PackageTaskTypes.FfpfscFromImage => RebuildFfpfscFromImage(fields),
            PackageTaskTypes.ExfatCreate => RebuildExfatCreate(fields),
            PackageTaskTypes.ExfatAmpr => RebuildExfatAmpr(fields),
            PackageTaskTypes.ExfatRepair => RebuildExfatRepair(fields),
            PackageTaskTypes.FfpkgCreate => RebuildFfpkgCreate(fields),
            PackageTaskTypes.FfpkgRebuild => RebuildFfpkgRebuild(fields),
            PackageTaskTypes.PackageExtract => RebuildExtract(fields),
            PackageTaskTypes.PackageVerify => RebuildPackageVerify(fields),
            PackageTaskTypes.ImageConvert => RebuildImageConvert(fields),
            PackageTaskTypes.ImageVerify => RebuildImageVerify(fields),
            PackageTaskTypes.ImageBuildPackage => RebuildImageBuildPackage(fields),
            _ => null
        };
        if (execute is null) return null;
        var task = new QueuedPackageTask
        {
            Id = entry.Id,
            Type = entry.Type,
            DisplayName = entry.DisplayName,
            SourcePath = entry.SourcePath,
            OutputPath = entry.OutputPath,
            PersistencePayload = entry.Payload,
            Operation = entry.Operation,
            SourceFormat = entry.SourceFormat,
            TargetFormat = entry.TargetFormat,
            StagePlan = PlanForType(entry.Type, fields),
            Execute = execute
        };
        task.Changed += TaskQueue_Changed;
        return task;
    }

    private static PackageTaskStage[] PlanForType(string type, Dictionary<string, string> fields) => type switch
    {
        PackageTaskTypes.ImageConvert => GetBool(fields, "package")
            ? PackageTaskPlans.ConvertPackage
            : PackageTaskPlans.ConvertImage,
        PackageTaskTypes.ImageBuildPackage => PackageTaskPlans.BuildPackageFor(Get(fields, "source")),
        PackageTaskTypes.ImageVerify => PackageTaskPlans.Verify,
        PackageTaskTypes.PackageExtract => PackageTaskPlans.Extract,
        PackageTaskTypes.PackageVerify => PackageTaskPlans.Verify,
        PackageTaskTypes.ExfatAmpr => PackageTaskPlans.Ampr,
        PackageTaskTypes.SonyDebugPkg => PackageTaskPlans.Build,
        PackageTaskTypes.FfpfscFromDump or PackageTaskTypes.FfpfscFromImage or PackageTaskTypes.ExfatCreate
            => PackageTaskPlans.BuildAndVerify,
        _ => PackageTaskPlans.Single
    };

    private static FfpfscBuildOptions FfpfscOptions(Dictionary<string, string> fields) => new()
    {
        OverwriteExisting = true,
        Compression = new PfscCompressionOptions
        {
            CompressionLevel = GetInt(fields, "level"),
            MinimumGainPercent = GetInt(fields, "gain")
        }
    };

    private static ExfatBuildOptions ExfatOptions(Dictionary<string, string> fields)
    {
        int cluster = GetInt(fields, "cluster");
        return new ExfatBuildOptions
        {
            ClusterSize = cluster > 0 ? cluster : null,
            GenerateAmprIndex = GetBool(fields, "ampr")
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildSonyDebugPkg(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        if (source.Length == 0 || output.Length == 0) return null;
        string contentId = Get(fields, "contentId");
        string passcode = Get(fields, "passcode");
        return (progress, token) => ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(source, output,
            new ProsperoDebugPackageBuildOptions { ContentId = contentId, Passcode = passcode },
            AdaptSonyPkgProgress(progress), token);
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildFfpfscFromDump(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        if (source.Length == 0 || output.Length == 0) return null;
        FfpfscBuildOptions build = FfpfscOptions(fields);
        ExfatBuildOptions exfat = ExfatOptions(fields);
        return async (progress, token) =>
        {
            IProgress<FfpfscProgress> inner = AdaptFfpfscProgress(progress);
            await FfpfscImage.CreateFromDirectoryAsync(source, output, build, exfat, inner, token).ConfigureAwait(false);
            await FfpfscImage.VerifyAsync(output, progress: inner, cancellationToken: token).ConfigureAwait(false);
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildFfpfscFromImage(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        if (source.Length == 0 || output.Length == 0) return null;
        FfpfscBuildOptions build = FfpfscOptions(fields);
        return async (progress, token) =>
        {
            IProgress<FfpfscProgress> inner = AdaptFfpfscProgress(progress);
            await FfpfscImage.CreateFromImageAsync(source, output, build, inner, token).ConfigureAwait(false);
            FfpfscVerificationResult verification = await FfpfscImage.VerifyAsync(output, source, inner, token)
                .ConfigureAwait(false);
            if (verification.SourceMatches != true)
                throw new InvalidDataException("The completed FFPFSC image did not match the source image.");
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildImageConvert(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        string targetText = Get(fields, "target");
        if (source.Length == 0 || output.Length == 0 ||
            !Enum.TryParse(targetText, out Ps5ImageConversionTarget target))
            return null;
        bool overwrite = GetBool(fields, "overwrite");
        bool fromPackage = GetBool(fields, "package");
        return async (progress, token) =>
        {
            var bridge = new Progress<Ps5ImageConversionProgress>(value =>
                progress.Report(new PackageTaskProgress(value.Stage, 0, 0, value.Completed, value.Total, 0, 0,
                    string.Empty)));
            if (fromPackage)
                await SonyPackageImageConversion.ConvertAsync(source, output, target, overwrite, bridge, token)
                    .ConfigureAwait(false);
            else
                await Ps5ImageConversionService.ConvertAsync(source, output, target, overwrite, bridge, token)
                    .ConfigureAwait(false);
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildImageVerify(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string formatText = Get(fields, "format");
        if (source.Length == 0 || !Enum.TryParse(formatText, out Ps5ImageFormat format)) return null;
        return (progress, token) => VerifyImageAsync(source, format, token);
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildImageBuildPackage(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        string contentId = Get(fields, "contentId");
        string passcode = Get(fields, "passcode");
        if (source.Length == 0 || output.Length == 0 || contentId.Length == 0) return null;
        if (passcode.Length == 0) passcode = SonyDebugPackageCredentials.DefaultPasscode;
        bool overwrite = GetBool(fields, "overwrite");
        ulong? sdkVersionOverride = GetSdkOverride(fields);
        string tempText = Get(fields, "temp");
        string? tempDirectory = tempText.Length > 0 ? tempText : null;
        ImageBuildSettings settings = GetImageBuildSettings(fields);
        return (progress, token) => BuildPackageFromSourceAsync(source, output, contentId, passcode, overwrite,
            sdkVersionOverride, tempDirectory, settings, progress, token);
    }

    private static ImageBuildSettings GetImageBuildSettings(Dictionary<string, string> fields)
    {
        Ps5InnerCompression compression = Enum.TryParse(Get(fields, "compression"), out Ps5InnerCompression parsed)
            ? parsed
            : Ps5InnerCompression.Auto;
        int krakenLevel = int.TryParse(Get(fields, "krakenLevel"), out int level) ? level : 7;
        int krakenThreads = int.TryParse(Get(fields, "krakenThreads"), out int threads) ? threads : 0;
        int playgo = int.TryParse(Get(fields, "playgo"), out int chunks) ? chunks : 1;
        string drm = Get(fields, "drm");
        return new ImageBuildSettings(compression, krakenLevel, krakenThreads, playgo,
            drm.Length > 0 ? drm : null);
    }

    private static ulong? GetSdkOverride(Dictionary<string, string> fields)
    {
        string value = Get(fields, "sdk");
        return value.Length > 0 && ulong.TryParse(value, System.Globalization.NumberStyles.AllowHexSpecifier,
            System.Globalization.CultureInfo.InvariantCulture, out ulong parsed)
            ? parsed
            : null;
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildExfatCreate(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        if (source.Length == 0 || output.Length == 0) return null;
        ExfatBuildOptions options = ExfatOptions(fields);
        return async (progress, token) =>
        {
            IProgress<FfpfscProgress> inner = AdaptFfpfscProgress(progress);
            await ExfatImage.WriteDirectoryAsync(source, output, options, inner, token).ConfigureAwait(false);
            await ExfatImage.VerifyAsync(output, inner, token).ConfigureAwait(false);
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildExfatAmpr(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        if (source.Length == 0) return null;
        return (progress, token) => ExfatAmprPatcher.RefreshAsync(source, AdaptFfpfscProgress(progress), token);
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildExfatRepair(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        if (source.Length == 0) return null;
        return (progress, token) => ExfatImageMaintenance.RepairAsync(source, AdaptFfpfscProgress(progress), token);
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildFfpkgCreate(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        if (source.Length == 0 || output.Length == 0) return null;
        string titleId = Get(fields, "titleId");
        bool replacing = GetBool(fields, "replacing");
        string working = replacing
            ? output + "." + Guid.NewGuid().ToString("N") + ".replacement"
            : output;
        return async (progress, token) =>
        {
            try
            {
                await Ufs2Operations.CreateFromDirectoryAsync(source, working, titleId, AdaptUfs2Progress(progress), token)
                    .ConfigureAwait(false);
                if (replacing) File.Move(working, output, true);
            }
            catch
            {
                TryDeleteFile(working);
                throw;
            }
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildFfpkgRebuild(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        if (source.Length == 0) return null;
        return (progress, token) => Ufs2Operations.RebuildWithEditsAsync(source, static _ => { },
            AdaptUfs2Progress(progress), token);
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildExtract(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        string output = Get(fields, "output");
        string kind = Get(fields, "kind");
        string passcode = Get(fields, "passcode");
        if (source.Length == 0 || output.Length == 0) return null;
        return kind switch
        {
            "sony" => (progress, token) => SonyPackageExtraction.ExtractAsync(source, output, passcode,
                AdaptSonyExtractProgress(progress), token),
            "ffpfsc" => (progress, token) => FfpfscImage.ExtractAsync(source, output,
                AdaptFfpfscProgress(progress), token),
            "ffpkg" => (progress, token) => Ufs2Operations.ExtractAsync(source, output,
                AdaptUfs2Progress(progress), token),
            "exfat" => (progress, token) => ExfatImage.ExtractDirectoryAsync(source, output,
                AdaptFfpfscProgress(progress), token),
            _ => null
        };
    }

    private static Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? RebuildPackageVerify(
        Dictionary<string, string> fields)
    {
        string source = Get(fields, "source");
        if (source.Length == 0) return null;
        string passcode = Get(fields, "passcode");
        if (passcode.Length == 0) passcode = SonyDebugPackageCredentials.DefaultPasscode;
        return (progress, token) =>
        {
            SonyDebugPackageBuilder.Validate(source, passcode);
            return Task.CompletedTask;
        };
    }
}