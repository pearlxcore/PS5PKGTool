# Tasks tab design review — remaining work

Concrete defects from the review are fixed and committed. This file records the remaining
**design/UX** work so it is tracked. Nothing here is broken; these are layout, presentation and
history-depth improvements.

## Fixed (for reference)
- **Queue scheduling:** `AutoStart` is now a real hold/run — holding stops the queue after the
  current task, waiting tasks stay queued; `RunQueue()` drains; `StartNext()` runs exactly one even
  while held (verified by `PackageTaskQueueSmoke.VerifyManualStartAsync`).
- **Clear successful:** `ClearCompleted()` now removes only `Completed` records; failed/cancelled/
  interrupted records are retained.
- **History retained:** `RebuildTask` no longer drops records it cannot rebuild (e.g. LibraryMove);
  they persist and display, and `Retry` refuses a record with no executable action.
- **Stage:** terminal tasks keep their last stage (so a failure shows where it stopped) instead of
  blanking.
- **Show output:** opens the task's actual output only; it no longer falls back to the source.
- Control text: `Auto-start` → `Run queue`, `Clear Completed` → `Clear successful`.
- **History depth:** `PersistedPackageTask` now stores created/started/completed times, the task
  message, the last stage and the failure text; `RestoreFromDisk` reapplies them, and the detail
  panel shows Started/Ended alongside Elapsed.

## Remaining

| Area | Item |
|---|---|
| Layout | Queue strip (running/waiting/needs attention) done; task text filter added; Details toggle collapses the details panel to enlarge the list; the list/details split is remembered. Status filter added, plus a Result line and a Diagnostic button (copies the full report). Still to do: per-operation result fields, wide-window side panel. |
| Controls | "Show source" and "Export report…" now in the task context menu; attempt count is tracked and shown. Still to do: explicit "Follow running" toggle naming; full separate attempt history records. |
| Columns | Operation-aware Task/source title; builder label; queue position for waiting jobs; in-place row updates preserving selection/scroll/group/column widths; stop scrolling to selection when follow is off. |
| Details | Captured configuration, masked passcode, timestamps, attempt number; per-operation live/result fields (builder/version, route, staging cleanup, counts, verification scope). |
| State/result | "Completed with warnings" badge; verification failure as a domain result distinct from an exception; truthful Cancelling. |
| Progress/time | Stage checklist with measured counters (`Files x/y`, `Bytes a/b`) instead of equal-stage weighting; validity-checked ETA; running clock independent of progress notifications; visible adapter fallback and plan reset. |
| History | Timing (created/started/completed), last stage and failure text are now persisted and restored. Still to do: separate attempt records, revalidate waiting inputs before running, do not auto-restart interrupted tasks. |
| Error/logs | An Export report (context menu) now writes a task report with masked configuration, timings, stage and failure. Still to do: task-filtered diagnostic log lines, task/attempt IDs in the global log, fallback visible in the final result. |
| Interaction | Empty-queue/empty-filter messages with actions; Ctrl+F scoping; queued notification without leaving Tools; accessible status text surviving row updates; selectable long paths/errors. |
