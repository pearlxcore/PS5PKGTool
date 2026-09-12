using System.Text.Json;
using System.Text.Json.Serialization;

namespace PS5PKGTool.Core.Tasks;

/// <summary>Serializable projection of a <see cref="QueuedPackageTask"/> for restart-resume.</summary>
public sealed record PersistedPackageTask(
    string Id,
    string Type,
    string DisplayName,
    string SourcePath,
    string OutputPath,
    string? Payload,
    PackageTaskStatus Status,
    string Operation = "",
    string SourceFormat = "",
    string TargetFormat = "");

/// <summary>
/// Sequential, resumable task queue modelled after a desktop download manager: long package
/// operations are enqueued once and processed one at a time. The queue owns ordering, status,
/// cancellation, retry, and persistence; feature code supplies the executable action. This mirrors
/// the PS4 multitool's task-queue concept but is an independent clean-room implementation.
/// </summary>
public sealed class PackageTaskQueue : IAsyncDisposable
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<QueuedPackageTask> _tasks = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly object _gate = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly Task _worker;
    private string? _persistencePath;
    private QueuedPackageTask? _requested;
    private bool _shutdown;

    public PackageTaskQueue()
    {
        _worker = Task.Run(() => WorkerAsync(_lifetime.Token));
    }

    /// <summary>When true (default) enqueued tasks begin automatically; otherwise use <see cref="StartNext"/>.</summary>
    public bool AutoStart { get; set; } = true;

    public IReadOnlyList<QueuedPackageTask> Tasks
    {
        get { lock (_gate) return _tasks.ToArray(); }
    }

    /// <summary>Raised whenever the task list or any task state/progress changes.</summary>
    public event EventHandler? TasksChanged;

    public QueuedPackageTask? Find(string id)
    {
        lock (_gate) return _tasks.FirstOrDefault(task => string.Equals(task.Id, id, StringComparison.Ordinal));
    }

    public QueuedPackageTask Enqueue(QueuedPackageTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        lock (_gate) _tasks.Add(task);
        Notify();
        Persist();
        if (AutoStart) _signal.Release();
        return task;
    }

    /// <summary>Queues a one-shot task without persistence (for actions reconstructed at runtime).</summary>
    public QueuedPackageTask Enqueue(string type, string displayName,
        Func<IProgress<PackageTaskProgress>, CancellationToken, Task> execute,
        string sourcePath = "", string outputPath = "", string? payload = null,
        string operation = "", string sourceFormat = "", string targetFormat = "",
        PackageTaskStage[]? stagePlan = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        return Enqueue(new QueuedPackageTask
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
        });
    }

    public void StartNext()
    {
        lock (_gate) _requested = _tasks.FirstOrDefault(task => task.Status == PackageTaskStatus.Queued);
        if (_requested is not null) _signal.Release();
    }

    public bool Cancel(string id)
    {
        QueuedPackageTask? task = Find(id);
        if (task is null) return false;
        lock (_gate)
        {
            switch (task.Status)
            {
                case PackageTaskStatus.Queued:
                    task.Apply(PackageTaskStatus.Cancelled, "Cancelled");
                    break;
                case PackageTaskStatus.Running:
                    task.Apply(PackageTaskStatus.Cancelling, "Cancelling…");
                    task.Cancellation?.Cancel();
                    break;
                default:
                    return false;
            }
        }
        Notify();
        Persist();
        return true;
    }

    public bool Retry(string id)
    {
        QueuedPackageTask? task = Find(id);
        if (task is null || task.Status is not (PackageTaskStatus.Failed or PackageTaskStatus.Cancelled or PackageTaskStatus.Interrupted))
            return false;
        lock (_gate)
        {
            task.Apply(PackageTaskStatus.Queued, "Queued");
            task.StartedUtc = null;
            task.CompletedUtc = null;
            task.Progress = PackageTaskProgress.Empty;
        }
        Notify();
        Persist();
        if (AutoStart) _signal.Release();
        return true;
    }

    public bool Remove(string id)
    {
        bool removed;
        lock (_gate)
        {
            QueuedPackageTask? task = _tasks.FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.Ordinal));
            if (task is null || task.Status is PackageTaskStatus.Running or PackageTaskStatus.Cancelling) return false;
            removed = _tasks.Remove(task);
        }
        if (removed) { Notify(); Persist(); }
        return removed;
    }

    public int ClearCompleted()
    {
        int removed;
        lock (_gate) removed = _tasks.RemoveAll(task => task.IsTerminal);
        if (removed > 0) { Notify(); Persist(); }
        return removed;
    }

    // ---------------------------------------------------------------- persistence

    /// <summary>Sets the persistence path without writing immediately (use before <see cref="RestoreFromDisk"/>).</summary>
    public void SetPersistencePath(string path) => _persistencePath = path;

    /// <summary>Enables automatic persistence to <paramref name="path"/> on every state change.</summary>
    public void EnablePersistence(string path)
    {
        _persistencePath = path;
        Persist();
    }

    public void Persist()
    {
        string? path = _persistencePath;
        if (path is null) return;
        try
        {
            PersistedPackageTask[] entries = Tasks.Select(task => new PersistedPackageTask(
                task.Id, task.Type, task.DisplayName, task.SourcePath, task.OutputPath,
                task.PersistencePayload, task.Status, task.Operation, task.SourceFormat, task.TargetFormat)).ToArray();
            string directory = Path.GetDirectoryName(path) ?? ".";
            Directory.CreateDirectory(directory);
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(entries, s_jsonOptions));
            File.Move(temporary, path, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // Persistence is best-effort; never let it break the running queue.
        }
    }

    /// <summary>
    /// Restores tasks previously written by <see cref="Persist"/>. The supplied factory rebuilds a
    /// runnable task from its payload (return null to drop an unknown entry). Tasks that were
    /// running when the app last exited are restored as <see cref="PackageTaskStatus.Interrupted"/>.
    /// </summary>
    public int RestoreFromDisk(Func<PersistedPackageTask, QueuedPackageTask?> restore)
    {
        ArgumentNullException.ThrowIfNull(restore);
        string? path = _persistencePath;
        if (path is null || !File.Exists(path)) return 0;

        List<PersistedPackageTask>? saved;
        try
        {
            saved = JsonSerializer.Deserialize<List<PersistedPackageTask>>(File.ReadAllText(path), s_jsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return 0;
        }
        if (saved is null) return 0;

        int restored = 0;
        foreach (PersistedPackageTask entry in saved)
        {
            QueuedPackageTask? task = restore(entry);
            if (task is null) continue;
            if (entry.Status is PackageTaskStatus.Running or PackageTaskStatus.Cancelling)
                task.Apply(PackageTaskStatus.Interrupted, "Interrupted during a previous session; retry to resume.");
            else
                task.Apply(entry.Status, task.Message);
            lock (_gate) _tasks.Add(task);
            restored++;
        }
        if (restored > 0)
        {
            Notify();
            // One signal per restored queued task, matching the one-signal-per-task
            // worker so auto-start resumes every task that was still pending.
            if (AutoStart)
            {
                int queued = _tasks.Count(task => task.Status == PackageTaskStatus.Queued);
                if (queued > 0) _signal.Release(queued);
            }
        }
        return restored;
    }

    // ---------------------------------------------------------------- worker

    private async Task WorkerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try { await _signal.WaitAsync(cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }

            // One signal runs exactly one task. Auto-start releases a signal per
            // enqueued/retried task, so the queue still drains on its own; manual
            // "Start Next" releases a single signal and starts a single task.
            QueuedPackageTask? task = TakeNext();
            if (task is not null)
                await RunAsync(task, cancellationToken).ConfigureAwait(false);
        }
    }

    private QueuedPackageTask? TakeNext()
    {
        lock (_gate)
        {
            QueuedPackageTask? task = _requested is { Status: PackageTaskStatus.Queued } requested
                ? requested
                : _tasks.FirstOrDefault(candidate => candidate.Status == PackageTaskStatus.Queued);
            _requested = null;
            return task;
        }
    }

    private async Task RunAsync(QueuedPackageTask task, CancellationToken lifetime)
    {
        task.Cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime);
        CancellationToken token = task.Cancellation.Token;
        task.Apply(PackageTaskStatus.Running, "Starting…");

        var progress = new PackageTaskProgressTracker(new RelayProgress<PackageTaskProgress>(value =>
        {
            if (task.Status == PackageTaskStatus.Running) task.Apply(PackageTaskStatus.Running, null, value);
        }), task.StagePlan);

        try
        {
            Func<IProgress<PackageTaskProgress>, CancellationToken, Task>? execute = task.Execute;
            if (execute is null)
                throw new InvalidOperationException($"Task '{task.DisplayName}' has no executable action.");
            await execute(progress, token).ConfigureAwait(false);
            task.Apply(PackageTaskStatus.Running, null, task.Progress with
            {
                StagePercent = 1d,
                Step = task.Progress.TotalSteps > 0 ? task.Progress.TotalSteps : task.Progress.Step
            });
            task.Apply(PackageTaskStatus.Completed, "Completed");
        }
        catch (OperationCanceledException)
        {
            task.Apply(PackageTaskStatus.Cancelled, "Cancelled");
        }
        catch (Exception ex)
        {
            task.Apply(PackageTaskStatus.Failed, ex.Message);
        }
        finally
        {
            task.Cancellation?.Dispose();
            task.Cancellation = null;
            Persist();
        }
    }

    private void Notify() => TasksChanged?.Invoke(this, EventArgs.Empty);

    private sealed class RelayProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    public async ValueTask DisposeAsync()
    {
        if (_shutdown) return;
        _shutdown = true;
        _lifetime.Cancel();
        lock (_gate)
        {
            foreach (QueuedPackageTask task in _tasks)
                if (task.Status is PackageTaskStatus.Running or PackageTaskStatus.Cancelling)
                    task.Cancellation?.Cancel();
        }
        try { await _worker.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        Persist();
        _lifetime.Dispose();
        _signal.Dispose();
    }
}
