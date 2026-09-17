# Settings audit — status

The settings lifecycle, validation, import/export, reset and page layout have been rebuilt. This file
records what changed and what remains deliberately out of scope.

## Lifecycle

- **Load** — `MainForm` loads settings in its constructor (before the builder combo, saved-views menu and
  task layout initialize), not in `Shown`. `AppStateStore.LoadSettingsWithDiagnostics` normalizes and, if
  the file is unreadable, moves it to `settings.json.invalid-<timestamp>` and returns a warning instead of
  silently reverting to defaults. Missing files simply yield defaults.
- **Normalize** — `AppSettingsNormalizer` is the one contract for startup, import and the dialog: null
  collections become empty, numbers are clamped, group/sort/column names are validated, the passcode must
  be blank or 32 printable ASCII characters, and paths are trimmed/full-pathed where possible.
- **Edit** — the dialog still edits a clone; theme preview is labelled live and reverts on Cancel.
- **Save** — the dialog validates, then persists through a callback before closing. On I/O failure the
  dialog stays open with the edits intact and offers Retry. "Saved" is only logged after the write
  succeeds; Save is disabled while nothing has changed.
- **Apply** — saving refreshes existing rows (row height), thumbnail visibility and the default grouping
  without rescanning sources; imported/reset column layout is reconciled onto the live grid so a later
  capture cannot overwrite it. Changing library folders or Scan subfolders still triggers a rescan.

## Pages

Seven pages now: Library, Appearance, Naming, Viewing & Cache, Output & Defaults, File Operations,
Maintenance.

- **Library** — managed folders and a separate manual-sources list, Add/Remove/Forget, Scan subfolders,
  Refresh on startup, and a rescan notice.
- **Appearance** — theme (live preview), density/row height, thumbnails, grid lines, default grouping,
  Reset column layout (applied on Save).
- **Naming** — rename format, preset selector, token picker + Insert, sample preview, unknown-token
  feedback.
- **Viewing & Cache** — preview limit (MiB) with scope, hex page (KiB), thumbnail cache entries with a
  minimum of 1 and an explanation.
- **Output & Defaults** — default output folder (also seeds Tools suggestions and Move/artwork dialogs),
  default builder, debug passcode with inline validation, reveal toggle, open-output-after-success.
- **File Operations** — confirm moves, confirm recycling, permanent-deletion policy; permanent deletion
  always confirms even when the Recycle Bin prompt is off.
- **Maintenance** — export (passcode excluded unless "Include the debug passcode" is ticked), import with
  a validation + change diff, preference-only reset, Clear Caches (on Save), Open Log Folder.

## Import / export / reset

- **Import** parses into a candidate, normalizes it, shows the changed keys and only replaces the draft on
  confirmation; nulls and out-of-range values can no longer corrupt state. The owner compares both
  library folders and manual sources so an imported source change rescans.
- **Export** uses the same JSON options as disk, and omits credentials by default.
- **Reset** restores preference defaults while keeping library folders, manual sources, recent folders,
  saved views and window/column layout; it does not silently set the reset-layout or clear-cache flags.

## Defaults precedence

- New jobs inherit the saved builder and passcode; the passcode boxes are seeded from the saved default
  (or the documented all-zero default) instead of a hard-coded value, and existing explicit jobs are not
  overwritten.

## Deliberately out of scope / still open

- The preview limit is a per-preview bound, not a global memory cap; decoded-image memory is not separately
  bounded.
- Thumbnail cache eviction clears the working set when the limit is reached rather than evicting the
  oldest entries one at a time (disposal is safe either way).
- Row-height minimum stays 16 with Compact/Normal/Comfortable presets; it is not yet font-metric aware.
- Layout is fixed-coordinate with tab and form minimum sizes; no dedicated DPI/contrast pass beyond the
  app's PerMonitorV2 mode.
