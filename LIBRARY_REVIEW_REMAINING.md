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
- **High — mutation gating:** delete refuses sources that a queued/running task is using (same-path or
  parent/child overlap). Rename/delete wiring still to extend to rename/move.
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

## Remaining

| Priority | Item |
|---|---|
| High | Context targeting: resolve row/group/background at invocation; separate row/group/background menus; handle Shift+F10/Menu key without stale `_contextRowIndex`. |
| High | Context targeting: resolve row/group/background at invocation; separate row/group/background menus; handle Shift+F10/Menu key without stale `_contextRowIndex`. |
| High | Extend the busy-path gate to rename/move (delete is done) and account for an active scan. |
| Medium | Reset semantics: the button is now "Reset filters" and grouping no longer keeps it active; the `All` preset clears the query. A separate **Reset view** (default sort/group/columns) is still to do. |
| Medium | Query parser: numeric overflow in version comparison is guarded (no throw). Still to do: inline feedback for unknown prefixes, unmatched quotes and malformed comparisons. |
| Medium | Exact ID match (`id:=PPSA...`); clarify OR syntax. |
| Medium | Columns: apply declared default widths, persist widths, default compact layout, keep Format and Status separate. |
| Medium | Rename/move previews enumerate all affected items and conflicts; remove `Rename All` from row context; export scope selector. |
| Medium | Collision-safe Save Artwork; recent-source persistence consistency; scan lifecycle ownership/generation. |
| Medium | Thumbnail cancellation state, bounded cache with image disposal, existence-check caching; parse-once filters. |
| Medium | Settings default-group key/label mismatch; scan feedback (summary, cancel, full error report). |
| Optional | Family/relationship views (base/update/DLC), highest/older-local updates, possible/byte-identical duplicates; multi-sort editor; saved views; density; accessibility/DPI pass; Help update wording. |
