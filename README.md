# PS5 PKG Tool

A DarkUI WinForms library manager for unpacked PlayStation 5 game dumps, raw exFAT images, UFS2 FFPKG images, FFPFSC containers, and Sony PS5 CNT/FIH packages. The application is deliberately separate from PS4 PKG Tool while sharing its visual language through the local DarkUI project.

## Current features

- Cached PS5 dump library with manual folder refresh
- Recursive discovery using `sce_sys/param.json`, Sony `.pkg`, `.ffpfsc`, `.ffpkg`, and `.exfat` files
- Bounded, streaming CNT/FIH header and entry-table parsing without loading the package or PFS image into memory
- Debug/retail package detection, content ID, flags, offsets, entry names, encryption state, and package file inventory
- Direct reading of unencrypted package `param.json`, PNG artwork, and BC7 DDS artwork
- Robust title fallback for incomplete or modified metadata
- Localized titles, versions, firmware, SDK, age levels, DRM, game intents, and feature attributes
- 512×512 icon and all three 3840×2160 background slots (`PIC0`, `PIC1`, `PIC2`)
- BC7 DDS decoding for PS5 background artwork that has no PNG counterpart
- Native UCP archive reader with bounds and SHA-1 validation
- Complete PS5 trophy names, descriptions, grades, hidden state, conditions, languages, and icons
- UDS activity, event, statistic, enumeration, and extraction-rule summaries
- Adjustable three-pane file browser with directory tree, per-directory content list/filtering, and a persistent viewer for images, text/configuration, Windows-codec audio/video, and paged hex/ASCII fallback
- SELF header and embedded ELF summary for `eboot.bin`
- PRX/SPRX module inventory
- Raw `param.json` viewer
- Native single-pass dump-folder to `.ffpfsc` conversion (streamed exFAT → PFSC → unsigned PS5 PFS)
- Native `.exfat` / `.ffpkg` to `.ffpfsc` conversion
- Full PFS/PFSC bounds validation, block decompression, extraction, and SHA-256 verification
- Direct, random-access FFPFSC reading through an on-demand PFSC stream and read-only exFAT or UFS2 filesystem
- Direct raw `.exfat` library loading with metadata, artwork, trophies, activities, executable details, modules, and file browsing
- Direct UFS2 `.ffpkg` library loading with streamed metadata, artwork, trophies, activities, executable details, modules, and file browsing
- Native PS5-preset FFPKG creation, streamed full-file verification, filesystem checks, extraction, queued editing, and transactional rebuild
- Native PS5 debug PKG creation with a data first inner PFS, stored NAPS map, AES XTS encrypted outer PFS, CNT metadata, finalized FIH container, SHA3-256 digest tables, and post-build reopening
- Default 32 zero debug passcode plus an optional user supplied 32 character ASCII passcode
- Atomic extraction and temporary open/preview of individual files selected in FFPFSC, FFPKG, or raw exFAT images
- Automatic in-memory AMPRIDX3 generation when `fakelib/libSceAmpr.sprx` is present; the source dump is not modified
- Cancellation-safe temporary output and atomic final publication
- PS5 package utilities: stored debug-PKG extraction, structural acceptance checks, split-package merge, UCP archive write and SHA-1 repair, editable `param.json` and `manifest.json` documents, ELF header editing, NP system-file validation, and launch-readiness inspection

## Build

```powershell
dotnet build .\PS5PKGTool.slnx -c Debug
```

The application references DarkUI at `..\DarkUI\DarkUI\DarkUI.csproj`.

## Validation

The smoke test validates the native exFAT, UFS2/FFPKG, AMPRIDX3, PFSC, and FFPFSC codecs before scanning unpacked dumps and checking metadata, artwork, trophies, UDS, files, and executable headers:

```powershell
dotnet run --project .\PS5PKGTool.SmokeTests\PS5PKGTool.SmokeTests.csproj -- "H:\PS5\PS5 Dumps"
```

The application loads its cached manifest at startup. It does not rescan large folders until the user selects **Refresh**.

Use **File → Open PS5 Container / Image** to add one `.pkg`, `.ffpfsc`, `.ffpkg`, or `.exfat` directly, or add a library folder and refresh. FFPFSC reads decode only the required 64 KiB PFSC blocks and keep a small block cache; the complete inner exFAT image is not materialized. Raw exFAT and UFS2 FFPKG images are indexed and read in place.

## Sony package support

The Sony package reader covers bare `\x7FCNT` metadata containers and the embedded CNT inside finalized `\x7FFIH` debug and retail packages. It validates every header, table, name table, body, and entry range before allocating or reading data. Debug packages use the standard 32 zero passcode path to derive the outer PFS key. The reader indexes the data first inner image through `pfsimage.xml` and `naps_pkg_layout.dat`; known raw files such as `eboot.bin`, `keystone`, PRX, SPRX, SELF, and ELF files are directly readable.

Kraken metadata decoding is enabled by default through the bundled Oodle 2.9.10 decoder. The application loads `oo2core_9_win64.dll` from its own installation directory, performs bounded headerless Kraken decoding, and validates the reconstructed inner PFS metadata. Direct streaming of arbitrary Kraken compressed application files is still pending. Retail PFS content requires matching image key material; a 32 zero debug passcode is not a retail decryption key. The native FFPFSC framework deals with unsigned jailbreak wrappers and does not claim to decrypt retail package payloads. See `THIRD_PARTY_NOTICES.md` for the Oodle license notice and redistribution restrictions.

Real `.ffpkg` files use UFS2 and are handled by the embedded managed UFS2 framework; they are never misidentified as exFAT images.

## PS5 debug PKG creation

The **Tools > PS5 debug PKG** workspace creates a package from the selected unpacked dump. The content ID is read from the selected game's `sce_sys/param.json` and must match the value entered in the tool. Creation is streamed to sibling temporary files and the finished output is published only after validation succeeds. Cancellation removes temporary output and preserves an existing destination.

The builder produces a data first PS5 PFS image, a stored NAPS layout that reconstructs its metadata, an encrypted outer PFS, CNT assets, per-entry and per-block SHA3-256 digests, CNT and FIH self seals, a stored install metadata ZIP, and an FIH version 3 container. Full Verify independently reopens the package and checks the FIH and CNT ranges, digest tables, passcode key derivation, encrypted sectors, NAPS reconstruction, PFS superblock, and supplemental file tree. The default passcode is exactly 32 zero characters. Selecting a custom passcode requires exactly 32 printable ASCII characters and the same value is required to validate or read that package.

The structural and cryptographic pipeline is covered by deterministic smoke tests, including rejection of an incorrect passcode. Installation and execution on a jailbroken PS5 have not been tested in this project, so the UI does not claim console compatibility until a real console test confirms it. Retail packages remain read only and require platform key material that this project does not contain.

For a bounded local regression test:

```powershell
dotnet run --project .\PS5PKGTool.SmokeTests\PS5PKGTool.SmokeTests.csproj -c Debug -- --test-debug-pkg
```

For a full dump, use the DarkUI workspace or run the long operation manually:

```powershell
dotnet run --project .\PS5PKGTool.SmokeTests\PS5PKGTool.SmokeTests.csproj -c Debug -- --create-debug-pkg "H:\PS5\PS5 Dumps\Sifu PS5 Dump v1.006" "H:\PS5\PS5 Dumps\PPSA05132.debug.pkg" "UP2703-PPSA05132_00-WUGUAN0000000001"
```

The last argument may be followed by a custom 32 character passcode. Omit it to use the 32 zero default. Validate an existing output with `--verify-debug-pkg <path> [passcode]`.

## Native image tools

The **Tools** menu can convert an unpacked game dump directly to `.ffpfsc`, wrap an existing `.exfat` or `.ffpkg`, and inspect or fully decode-verify an existing `.ffpfsc`. Conversion is implemented by `PS5PKGTool.Ffpfsc` in managed C# and has no Python, MkPFS, or external executable dependency.

The **Tools → exFAT** workspace creates a standalone exFAT image from the selected unpacked dump, defaults to the recommended 64 KiB cluster size, optionally generates AMPRIDX3, fully verifies every file after creation, validates existing images, and extracts an image back to a directory tree. **Refresh AMPR Index** updates an exact-fit index in place or safely creates, resizes, and relocates it without rebuilding. When required, it extends the root directory and grows only the image tail while updating both boot regions, the FAT, allocation bitmap, and entry checksums. Every changed range and the original image length are journaled and restored on cancellation or failed verification.

The **Tools → FFPKG** workspace creates a PS5-compatible UFS2 image from the selected dump using 4 KiB sectors/fragments, 32 KiB blocks, zero reserved-space percentage, space optimization, and no soft-updates journal. Full verification streams every file into a deterministic manifest hash and runs the embedded filesystem checker. Extraction is path-contained and atomic. **Edit Files** queues file replacement/addition, directory-tree import, empty-directory creation, and recursive deletion; **Verified Rebuild** reconstructs all readable UFS2 metadata. Both publish the replacement only after a successful full verification and restore the original if the final swap fails.

**Edit Files** provides queued replacement, addition, directory-tree import, empty-directory creation, and recursive deletion. A single equal-size replacement uses a disk-backed rollback journal and writes directly to the existing allocation. Size changes and structural edits are applied to a sibling staging tree, rebuilt with the original cluster size, fully verified, and atomically exchanged with the original image. **Repair Image** can recover one damaged exFAT boot region from its valid mirror, then reconstructs readable filesystem metadata through the same verified rebuild. If both boot copies or file data are unreadable, repair stops without retaining partial changes.

Dump conversion is one pass and bounded-memory: the exFAT image is generated as a forward-only stream, compressed in 64 KiB PFSC blocks, and written directly into its final PFS payload. Existing outputs are only replaced after the Save dialog confirms overwrite and a complete temporary image has passed structural validation.
