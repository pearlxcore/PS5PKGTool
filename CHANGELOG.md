# Changelog

All notable changes to PS5 PKG Tool are documented here. This project uses semantic versioning.

## [1.1.0] - 2026-09-18

### New features

- **Debug package builder (Tools).** Build a debug package (FPKG) from a dump or image, with build settings for compression (Auto / Kraken / Uncompressed), Kraken level and thread count, PlayGo chunk count, package type, an optional SDK version override, an optional workspace folder, and a choice of builder. A free-space check runs before the build, and progress is shown stage by stage. Auto/Kraken keeps packages smaller; Uncompressed reproduces the older stored output.
- **Structure-only inspection.** Sources without a readable `param.json` are still listed and openable, with a warning explaining the limited view.
- **Richer game details.** Compute the eboot hash on demand, filter trophies and see clear scope states (present / protected / unsupported), and metadata-only labels.
- **Redesigned Tasks tab.** Status filter (All / Active / Needs attention / Finished), task search, a details panel, "Follow running", attempt count, remembered timing, last stage and failure across restarts, a result line, diagnostic export, and queue position for waiting jobs.
- **Saved library views.** Save the current filters, grouping, sort and columns under a name, and switch between them.
- **Family view and roles.** Group a title's base game, updates and DLC together; a Role column marks Base / Update / DLC / App and flags older, superseded updates.
- **Multi-column sorting.** Shift-click column headers to sort by more than one column.
- **Find Duplicates.** Reports duplicate sources and separates byte-identical copies from merely similar ones.
- **Rename and Move previews.** Review every affected item and any name or destination conflicts before anything is changed.

### Improvements

- **Faster, lighter startup and refresh.** Thumbnails load only as they are needed, and large dumps open and browse noticeably faster.
- **Reorganized Settings.** Clear pages, settings are saved before the dialog closes, and import shows exactly what will change before it applies.
- **Interface density and accessibility.** Compact / Normal / Comfortable presets and screen-reader names for key controls.
- **Remembered layout.** Library column widths are kept between sessions.
- **Toggle the file preview pane** to give the file list the full width.
- **Reset view** command to restore the default sort, grouping and columns.
- **Smarter search and sorting.** Helpful feedback for malformed queries; versions such as 1.10 sort after 1.9, and grouped sizes sort numerically.
- **Cleaner task list.** No more jumping to the top or flickering, and the Stage column clears once a task finishes.
- **Export with a scope.** Export the selected rows, the visible rows, or the whole library.
- **Scan control.** Press Esc to cancel a scan; scan warnings open in a full, scrollable report.
- **Layout polish** across the Tasks tab and Settings.

### Changes and fixes

- Package builds now work on Windows systems without CNG SHA-3 support.
- Removed the redundant Content ID / Title / Title ID / Version fields from package builds; the content ID is read from the source automatically.
- The default grouping is no longer reset just by opening and saving Settings.
- "Save all artwork" no longer overwrites images that share a file name.
- Right-click actions target the correct row when the menu is opened from the keyboard.
- Delete confirmation is accurate and cancellable; DLC detection is corrected, and permanent deletion always asks for confirmation.
- Rename and Move are blocked for sources a running task is using.
- Unreadable sources stay in the library instead of disappearing.
- "Rename All" moved out of the File menu (it remains in the library right-click menu), and the stale "Find Duplicates (Coming soon)" label was removed.

## [1.0.0] - 2026-09-12

First release. PS5 PKG Tool is a Windows library manager and image tool for PS5 dumps, filesystem images, and debug packages.

### Library

- Scan folders for unpacked dumps, Sony PKG, FFPFSC, FFPKG, and exFAT images.
- Manifest cache, so the library loads instantly after the first scan.
- Group the list by Title ID, Category, Region, Source format, or Firmware.
- Faceted filters for Category, Region, and Format, quick presets, and a search grammar with field prefixes, comparisons, negation, and OR groups.
- Right click actions: copy, rename, move into folders, delete to the Recycle Bin, and save artwork.
- Detail tabs for Overview, Artwork, Trophies, Activities, Files, Executable, Raw, and Package.
- File browser with image, text, hex, and media previews, plus per file and whole folder extraction.
- Rename files and dump root folders from preset or custom token formats (for example {TITLE} [{TITLE_ID}] [{VERSION}]), with tokens for {TITLE_ID}, {CONTENT_ID}, {PLATFORM}, {CATEGORY}, {REGION}, {VERSION}, {SYSTEM_VERSION}, {SOURCE}, and more. Includes Rename All and Rename by install order for packages.
- Cached items whose source was deleted or moved are marked as Missing (Source column) and reported at startup, with File > Remove Missing Items to drop them; a Refresh prunes them.
- Move items into a chosen destination folder, grouped by Title, Title ID, Category, Region, or Source, or all into one folder. Same drive uses a fast rename; across drives it copies, verifies, then deletes. Files and dump folders are both supported, collisions are skipped with a warning, and the destination can be added to the library.

### Image tools

- Convert between dump folders and exFAT, FFPKG (UFS2), and FFPFSC images.
- Extract, verify, edit files, repair, refresh AMPR, and rebuild images.
- Build a debug package (FPKG) from a dump or image, and convert a debug package to exFAT, FFPKG, or FFPFSC.
- Optional SDK version override when building a debug package: pick an SDK release and it is stamped into param.json and each fake-signed module's .sceversion; off by default.
- Optional workspace folder for building a debug package, with a free-space check before the build starts; blank uses the system temp folder.
- Built on managed exFAT, UFS2/FFPKG, PFSC, and PFS implementations.

### Packages

- Read, verify, and extract debug PS5 packages, including inner PFS browsing, trophies, activities, and PlayGo data.
- Patch (LIH) packages are recognized as their own kind instead of being reported as plain debug packages.
- The Package detail view reports the block aligned SC segment, CNT relative entry offsets, stored sizes for encrypted entries, and canonical engine entry names.
- param.sfo is read from the outer CNT entry first, with the inner image used only as a fallback.
- PlayGo chunks, scenarios, and per file chunk assignments are read through the engine PlayGoChunkReader, with a lenient fallback and a diagnostic note for user files the strict engine reader rejects.
- Configurable Content ID and passcode, with an optional seed for byte reproducible builds.
- Engine refresh (ProsperoPkgTool a13fd5a, builder side): more tolerant param.json parsing, NAPS padding RUN re-anchors, imagedigs on-disk byte order, FIH content-version BCD, param.json packaged verbatim, and a fixed single-chunk PlayGo profile.
- Engine refresh (ProsperoPkgTool 7c50180, operational): the free-space preflight blocks a build that cannot fit with a clear dialog and warns first when space is low; stale build workspaces are swept; the Kraken decoder now reads verbatim and restarted sub-chunks. Produced packages are unchanged.
- Retail packages are recognized but need matching key material to decode.

### Tasks

- Sequential queue for every long operation, with auto or manual start, cancel, retry, remove, and clear completed.
- Start Next starts exactly one task when auto start is off; each task can be cancelled, retried, or removed.
- The task buttons and context menu enable and disable according to the selected task state.
- Tasks persist between launches. A task that was running at shutdown comes back as interrupted and can be retried.
- Each task shows a current step bar and an overall task step bar.

### Settings and app

- Tabbed settings for Library, Appearance, Viewing, Paths and Build, Safety, and Maintenance.
- Live theme preview, grid options, preview and cache limits, output folder, default debug passcode, and confirmation prompts.
- Export and import settings, reset settings, clear caches, and open the log folder.
- Help menu with Check for Updates, Buy me a Ko-fi, and Support via PayPal, plus an About dialog.
- Log tab that shows live operations, warnings, and errors in a console view, with a level filter, auto-scroll, clear, and open log folder. The log file stays the full record.
- GPL-3.0 licensed, with third party notices.

### Fixes

- Package builds no longer depend on the OS SHA3 provider. The EKPFS key now uses the engine's managed SHA3-256, which fixes "Operation is not supported on this platform" on Windows builds without CNG SHA-3 (for example Windows Server or older Windows 10 LTSC).
- Failed tasks now record and log the full exception (type and stack), and the task row shows the exception type.
- Dark text viewers (Raw param.json, hex, and file preview) now fill their tab when first shown on a tab that was not selected at load time.
- Opening a loose dump now updates its Size in the library list immediately instead of staying blank until a refresh; dumps whose size is not known yet show a tooltip.

### Notes

- Self-contained, single file Windows x64 release; no separate runtime install required.
- Kraken metadata decoding uses the bundled Oodle 2.9.10 decoder. See `THIRD_PARTY_NOTICES.md` for license and redistribution terms.
- Images and packages produced by the app are experimental and have not been tested on a jailbroken PS5. Keep originals and verify output before relying on it.
