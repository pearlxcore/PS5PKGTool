# Library UI/workflow review — status

The top-half library review is audited here. The Critical items and the first High group are fixed
and committed; the remainder is tracked below. Nothing here is broken code — the remaining entries
are design/robustness work.

## Fixed

- **Critical — delete wording vs behavior:** `DeleteSelected` silently used permanent deletion when
  the hidden `PermanentDelete` setting was set while the dialog said "Recycle Bin". The confirmation
  now states the real verb ("Send N to the Recycle Bin" vs "Permanently delete N … cannot be undone")
  and the log/status use the matching verb.
- **Critical — OK-only confirmation:** the confirmation is now a cancellable **Yes/No** dialog
  (`ConfirmDelete`) listing up to 15 affected paths plus "… and N more".
- **High — DLC filter mismatch:** the category vocabulary was inconsistent (`CategoryOf` returned
  `DLC` for numeric 1 but `Add-on` for content type 0x21/0x22; the filter combo had `Add-on`/`Other`
  while rows could be `DLC`/`Unknown`). Normalized everywhere to `Game / Patch / DLC / App / Unknown`;
  content type 0x21/0x22 now classify as `DLC`, and the DLC preset selects `DLC`.
- **High — rejected rows retained:** the scanner now keeps an **unreadable record** (path + error) for a
  package, or an image whose volume will not open, instead of dropping it to a scan-error string.
- **High — mutation gating:** delete, rename and move all refuse sources that a queued/running task is
  using (same-path or parent/child overlap) through a shared `RefuseIfBusy` check, so a running
  operation's input cannot be renamed, moved or deleted out from under it.
- **High — typed sorting:** Version and Required System sort by parsed numeric value (1.10 after 1.9)
  via `VersionKey`, matching the filter's component comparison.
- **High — grouped sorting:** `GroupCellValueComparer` is now set and compares byte-size text
  numerically (2 GiB before 10 GiB) instead of as text.
- **High — multi-selection preserved:** rebuilds restore the full selected set and the focused row by
  root path; selections that are no longer visible are dropped instead of retained invisibly.
- **High (partial) — focused row / single-only actions:** `SelectedGame()` prefers the focused row and
  single-only actions (Reveal, Copy*, Save Artwork) are now disabled for a multi-selection.
- **Medium — reset/preset semantics:** the clear button is "Reset filters", grouping no longer keeps it
  active, and the `All` preset clears the query. Version-comparison overflow is guarded.
- **Medium — exact match:** `id:=PPSA12345` (also `title:=`, `content:=`) matches the whole value
  case-insensitively instead of searching for a substring.
- **Medium — scan ownership:** each scan carries a generation; a superseded scan can no longer apply its
  results or clear the newer scan's busy state when it finishes, and F5 honors the refresh command's
  enabled state.
- **High — context targeting:** the target is resolved per invocation — a mouse right-click keeps its hit
  row, while keyboard Shift+F10/Menu uses the focused row instead of a stale index — and group-only
  items are shown only for group rows.
- **Medium — Reset view:** a "Reset view" command restores the default Title-ascending sort, clears
  grouping and shows all columns in declared order, separate from "Reset filters".
- **Medium — query feedback:** a "Check query" chip reports unbalanced quotes, unknown field prefixes,
  missing values and malformed size/version comparisons while still running the query.
- **Medium — column widths:** columns build from the declared default weights and remember user-resized
  proportional widths across sessions (`LibraryColumnWeights`); Reset view and Reset column layout
  restore the defaults.
- **Medium — collision-safe artwork:** "Save all" uses a title-ID prefix and numbered suffix when a name
  such as `icon0.png` already exists, so several games can share one output folder.
- **Medium — settings grouping round-trip:** the Default group combo stores a label while the setting
  stores a key, so the saved grouping was reset to None on every open; it now maps back to the label.
- **Medium — thumbnail cache:** a superseded load re-checks its version before publishing (disposing a
  stale image), clearing the cache disposes images the grid no longer shows, and per-source removal is
  centralized for rename/move.
- **Medium — export scope:** Export is a submenu (selected / visible / all) rather than guessing from the
  selection, and library-wide Rename All moved from the row context menu to the File menu.
- **Medium — scan feedback:** Escape cancels an in-progress refresh, and scan warnings open a scrollable
  full report instead of a truncated dialog.

## Remaining

| Priority | Item |
|---|---|
| Medium | Rename/move previews enumerate all affected items and conflicts. |
| Optional | Family/relationship views (base/update/DLC), highest/older-local updates, possible/byte-identical duplicates; multi-sort editor; saved views; density; accessibility/DPI pass; Help update wording. |
