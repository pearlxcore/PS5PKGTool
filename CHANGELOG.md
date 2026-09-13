# Changelog

All notable changes to PS5 PKG Tool are documented here. This project uses semantic versioning.

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

### Image tools

- Convert between dump folders and exFAT, FFPKG (UFS2), and FFPFSC images.
- Extract, verify, edit files, repair, refresh AMPR, and rebuild images.
- Build a debug package (FPKG) from a dump or image, and convert a debug package to exFAT, FFPKG, or FFPFSC.
- Built on managed exFAT, UFS2/FFPKG, PFSC, and PFS implementations.

### Packages

- Read, verify, and extract debug PS5 packages, including inner PFS browsing, trophies, activities, and PlayGo data.
- Patch (LIH) packages are recognized as their own kind instead of being reported as plain debug packages.
- The Package detail view reports the block aligned SC segment, CNT relative entry offsets, stored sizes for encrypted entries, and canonical engine entry names.
- param.sfo is read from the outer CNT entry first, with the inner image used only as a fallback.
- PlayGo chunks, scenarios, and per file chunk assignments are read through the engine PlayGoChunkReader, with a lenient fallback and a diagnostic note for user files the strict engine reader rejects.
- Configurable Content ID and passcode, with an optional seed for byte reproducible builds.
- Engine refresh (ProsperoPkgTool a13fd5a, builder side): more tolerant param.json parsing, NAPS padding RUN re-anchors, imagedigs on-disk byte order, FIH content-version BCD, param.json packaged verbatim, and a fixed single-chunk PlayGo profile.
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

- Dark text viewers (Raw param.json, hex, and file preview) now fill their tab when first shown on a tab that was not selected at load time.
- Opening a loose dump now updates its Size in the library list immediately instead of staying blank until a refresh; dumps whose size is not known yet show a tooltip.

### Notes

- Self-contained, single file Windows x64 release; no separate runtime install required.
- Kraken metadata decoding uses the bundled Oodle 2.9.10 decoder. See `THIRD_PARTY_NOTICES.md` for license and redistribution terms.
- Images and packages produced by the app are experimental and have not been tested on a jailbroken PS5. Keep originals and verify output before relying on it.
