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

## Remaining

| Priority | Item |
|---|---|
| High | Typed version/system/size sorting everywhere (flat grid sorts some strings lexically; unify with the filter's component comparison). |
| High | Grouped cell sorting uses formatted strings (`GroupCellValueComparer` is null). Compare raw values in grouped mode (needs raw values on the cells). |
| High | Preserve the **full multi-selection** (and focus/scroll anchor) across filter/sort rebuilds; `PopulateLibraryGrid` restores only one root. |
| High | Single active row: single-row actions currently use the first selected row; disable single-only actions for multi-selection or define a focused row. |
| High | Context targeting: resolve row/group/background at invocation; separate row/group/background menus; handle Shift+F10/Menu key without stale `_contextRowIndex`. |
| High | Retain rejected **PKG** rows: the scanner's structure fallback returns null for `SonyPackage`, so unreadable PKGs become scan-error strings and can vanish on refresh. |
| High | Gate filesystem mutations (rename/move/delete) against running operations on the same path or an active scan. |
| Medium | `Clear all`/preset semantics: separate Reset filters vs Reset view; `All` should clear the query too. |
| Medium | Safe query parser: unknown prefixes, unmatched quotes, numeric overflow must not throw and should give inline feedback. |
| Medium | Exact ID match (`id:=PPSA...`); clarify OR syntax. |
| Medium | Columns: apply declared default widths, persist widths, default compact layout, keep Format and Status separate. |
| Medium | Rename/move previews enumerate all affected items and conflicts; remove `Rename All` from row context; export scope selector. |
| Medium | Collision-safe Save Artwork; recent-source persistence consistency; scan lifecycle ownership/generation. |
| Medium | Thumbnail cancellation state, bounded cache with image disposal, existence-check caching; parse-once filters. |
| Medium | Settings default-group key/label mismatch; scan feedback (summary, cancel, full error report). |
| Optional | Family/relationship views (base/update/DLC), highest/older-local updates, possible/byte-identical duplicates; multi-sort editor; saved views; density; accessibility/DPI pass; Help update wording. |
