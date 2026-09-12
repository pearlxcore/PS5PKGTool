using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using PS5PKGTool.Core.Tasks;

internal static class PackageTaskQueueSmoke
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task RunAsync()
    {
        VerifyStagePlanTracking();
        await VerifyManualStartAsync().ConfigureAwait(false);
        string directory = Path.Combine(Path.GetTempPath(), "PS5PKGTool.TaskQueue." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "queue.json");
            string persistedJson;
            await using (var queue = new PackageTaskQueue())
            {
                queue.EnablePersistence(path);
                var order = new List<int>();
                var gate = new object();
                for (int index = 1; index <= 3; index++)
                {
                    int value = index;
                    queue.Enqueue("Unit", "Task " + value, async (progress, token) =>
                    {
                        progress.Report(new PackageTaskProgress("Working", value, 3));
                        await Task.Delay(10, token).ConfigureAwait(false);
                        lock (gate) order.Add(value);
                    }, operation: "Convert", sourceFormat: "FFPKG", targetFormat: "FFPFSC");
                }

                WaitUntil(() => queue.Tasks.All(task => task.IsTerminal), "Queued tasks did not run to completion.");
                Require(order.SequenceEqual([1, 2, 3]), "Tasks did not run in enqueue order.");
                Require(queue.Tasks.All(task => task.Status == PackageTaskStatus.Completed), "A task did not complete.");

                string cancelId = queue.Enqueue("Unit", "Cancel me",
                    (_, token) => Task.Delay(Timeout.Infinite, token)).Id;
                WaitUntil(() => queue.Find(cancelId)!.Status == PackageTaskStatus.Running, "Cancel task did not start.");
                Require(queue.Cancel(cancelId), "Cancel was rejected.");
                WaitUntil(() => queue.Find(cancelId)!.IsTerminal, "Cancel task did not stop.");
                Require(queue.Find(cancelId)!.Status == PackageTaskStatus.Cancelled, "Cancelled task has the wrong status.");

                int attempts = 0;
                string retryId = queue.Enqueue("Unit", "Retry me", (_, _) =>
                {
                    attempts++;
                    return attempts == 1
                        ? Task.FromException(new InvalidOperationException("first attempt fails"))
                        : Task.CompletedTask;
                }).Id;
                WaitUntil(() => queue.Find(retryId)!.Status == PackageTaskStatus.Failed, "Failing task did not fail.");
                Require(queue.Retry(retryId), "Retry was rejected.");
                WaitUntil(() => queue.Find(retryId)!.Status == PackageTaskStatus.Completed, "Retried task did not complete.");
                Require(queue.Tasks.Count == 5 && queue.Tasks.All(task => task.IsTerminal),
                    "Unexpected task set after the queue drained.");

                persistedJson = File.ReadAllText(path);
                queue.ClearCompleted();
                Require(queue.Tasks.Count == 0, "ClearCompleted did not remove the terminal tasks.");
            }

            List<PersistedPackageTask> saved =
                JsonSerializer.Deserialize<List<PersistedPackageTask>>(persistedJson, Options)!;
            Require(saved.Count == 5, "Unexpected persisted entry count.");
            Require(saved[0].Operation == "Convert" && saved[0].SourceFormat == "FFPKG" &&
                    saved[0].TargetFormat == "FFPFSC", "Task metadata was not persisted.");

            string restoredPath = Path.Combine(directory, "restored.json");
            await using (var queue = new PackageTaskQueue())
            {
                queue.EnablePersistence(restoredPath);
                File.WriteAllText(restoredPath, JsonSerializer.Serialize(saved, Options));
                int restored = queue.RestoreFromDisk(entry => Rebuild(entry));
                Require(restored == saved.Count, "Restore did not return the saved tasks.");
            }

            string interruptedPath = Path.Combine(directory, "interrupted.json");
            await using (var queue = new PackageTaskQueue())
            {
                queue.EnablePersistence(interruptedPath);
                PersistedPackageTask[] running =
                    [new PersistedPackageTask("r1", "Unit", "Was running", "", "", null, PackageTaskStatus.Running)];
                File.WriteAllText(interruptedPath, JsonSerializer.Serialize(running, Options));
                int restored = queue.RestoreFromDisk(entry => Rebuild(entry));
                Require(restored == 1 && queue.Tasks[0].Status == PackageTaskStatus.Interrupted,
                    "A task running at shutdown was not restored as Interrupted.");
            }
        }
        finally
        {
            try { Directory.Delete(directory, true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static QueuedPackageTask Rebuild(PersistedPackageTask entry) => new()
    {
        Id = entry.Id,
        Type = entry.Type,
        DisplayName = entry.DisplayName,
        SourcePath = entry.SourcePath,
        OutputPath = entry.OutputPath,
        PersistencePayload = entry.Payload,
        Execute = (_, _) => Task.CompletedTask
    };

    private static async Task VerifyManualStartAsync()
    {
        await using var queue = new PackageTaskQueue { AutoStart = false };
        var gate = new object();
        var started = new List<int>();
        for (int index = 1; index <= 3; index++)
        {
            int value = index;
            queue.Enqueue("Unit", "Manual " + value, (_, _) =>
            {
                lock (gate) started.Add(value);
                return Task.CompletedTask;
            });
        }

        Require(queue.Tasks.All(task => task.Status == PackageTaskStatus.Queued),
            "Manual-mode tasks started before StartNext was called.");

        queue.StartNext();
        WaitUntil(() => Count(gate, started) >= 1, "StartNext did not run a task.");
        await Task.Delay(100).ConfigureAwait(false);
        Require(Count(gate, started) == 1, "StartNext ran more than one task in manual mode.");

        queue.StartNext();
        WaitUntil(() => Count(gate, started) >= 2, "The second StartNext did not run a task.");
        await Task.Delay(100).ConfigureAwait(false);
        Require(Count(gate, started) == 2, "The second StartNext ran more than one task in manual mode.");

        static int Count(object gate, List<int> list)
        {
            lock (gate) return list.Count;
        }
    }

    private static void VerifyStagePlanTracking()
    {
        var plan = new[]
        {
            new PackageTaskStage("Extract", "extract"),
            new PackageTaskStage("Build", "build")
        };
        PackageTaskProgress last = PackageTaskProgress.Empty;
        var tracker = new PackageTaskProgressTracker(new Relay(value => last = value), plan);
        tracker.Report(new PackageTaskProgress("Extracting", 0, 0, 5, 10));
        Require(last.TotalSteps == 2 && last.Step == 0 && last.TaskPercent == 0d,
            $"First step should report 0 of 2 steps done (got {last.Step}/{last.TotalSteps}).");
        Require(Math.Abs(last.OperationPercent - 0.5d) < 0.001d, "Step percent was not tracked.");
        tracker.Report(new PackageTaskProgress("Building", 0, 0, 10, 10));
        Require(last.TotalSteps == 2 && last.Step == 1 && Math.Abs(last.TaskPercent - 0.5d) < 0.001d,
            $"Second step should report 1 of 2 steps done (got {last.Step}/{last.TotalSteps}).");
        Require(Math.Abs(last.OperationPercent - 1d) < 0.001d, "Operation percent was not tracked.");
    }

    private sealed class Relay(Action<PackageTaskProgress> report) : IProgress<PackageTaskProgress>
    {
        public void Report(PackageTaskProgress value) => report(value);
    }

    private static void WaitUntil(Func<bool> predicate, string message, int timeoutMs = 15000)
    {
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            if (predicate()) return;
            Thread.Sleep(15);
        }
        if (!predicate()) throw new InvalidOperationException(message);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
