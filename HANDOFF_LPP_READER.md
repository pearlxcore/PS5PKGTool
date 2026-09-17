# HANDOFF — PS5PKGTool / PkgViewer side of the LPP reader compatibility fix

Full task brief: `C:\Users\User\Desktop\ASTRA-PROMPT-LPP-READER.md`.
Engine-side handoff: `C:\Users\User\source\repos\ProsperoPkgTool\HANDOFF_LPP_READER.md`.

The **reader** (NAPS dialect) fix belongs in the engine (`ProsperoPkgTool`). This file covers the
app-side changes.

## 1. Immediate fix (Problem B) — one line
`PS5PKGTool.Core/Parsers/SonyEnginePackageAccess.cs` (`EnsureDecoded`, ~line 81) chooses the
in-memory reader vs the file-backed reader using the **compressed** PFS size:

```csharp
if (fih.PfsSize > int.MaxValue)   // WRONG: compressed size, not logical mount size
```

Replace with the engine helper that measures the **uncompressed** mount size:

```csharp
if (ProsperoPackageContent.RequiresFileBacked(PackagePath, passcode))
```

Symptom this fixes: a PPT-built Bendy package (compressed ≈ 1.43 GB, logical mount ≈ 3.55 GB)
currently fails with `Implausible inner mount size 3549167616` and the app silently shows an
empty file list and "eboot.bin was not found". The engine CLI reads the same file fine
(76 files, `/uroot/eboot.bin` ≈ 34,277,858 bytes).

Then surface decode failures honestly: `SonyEnginePackageAccess` already keeps `_decodeError` —
do not present a decode failure as "eboot.bin was not found".

## 2. Re-vendor + rebuild (after the engine fix lands)
1. Rebuild the engine, then copy `ProsperoPkgTool.dll` (and `.pdb`) into
   `PS5PKGTool/ThirdParty/ProsperoPkgTool/` (update `VERSION.txt`).
2. Rebuild `PS5PKGTool` with `--no-incremental` so `deps.json` refreshes (known gotcha: a stale
   `deps.json` causes a runtime `FileNotFoundException`).
3. Rebuild `PkgViewer` (it references `PS5PKGTool.Core`, so it inherits the fix).

## 3. Do NOT
- Add an LPP reader path in `PS5PKGTool.Core` or `PkgViewer`.
- Route reads based on the selected builder (`PptBackend` / `LppBackend`).
- Extract/unpack an LPP package to disk as the viewing path.
- Duplicate NAPS dialect detection in the app (the engine owns it).

## 4. Later (cleanup, deferrable)
Split `IPackageBackend` so it is **builder-only** (`Build`), and move `Validate`/`Extract` to the
canonical reader service. This touches `PS5PKGTool`, `PkgViewer` and the smoke tests — do it only
after the compatibility fix is verified.

## Acceptance
- `PS5PKGTool.SmokeTests --inspect-sony-pkg "<PPT Bendy>"` → `PFS files: 76`,
  `Executable modules: >0`, and `READ /uroot/eboot.bin` present.
- Opening LPP Bendy / LPP Ufouria in PS5PKGTool and PkgViewer populates the file list and the
  Executable tab.
- Existing `PS5PKGTool` smoke tests still pass.
