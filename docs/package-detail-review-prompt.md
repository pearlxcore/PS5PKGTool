# Review request: PS5PKGTool "Package" detail section

Audience: the maintainer AI of **ProsperoPkgTool** (the engine).
From: PS5PKGTool (the WinForms consumer).
Status: review request. No code changes are being asked of PS5PKGTool or the engine yet.

Pin the review to this engine build:

- Vendored library: `PS5PKGTool/ThirdParty/ProsperoPkgTool/ProsperoPkgTool.dll`
- Size: `625152` bytes
- SHA-256: `D06961C849978F0099B6145408A58EF08209FDB80C92C73A3982C0A3B3403962`
- Engine repo HEAD at handoff: `072435a feat: virtual file sources, reproducible builds, Kraken LZ/Huffman`

---

## 1. Context

PS5PKGTool is a GUI front end that consumes ProsperoPkgTool as a **managed library only** (never the
CLI). The engine owns every byte of PKG/NAPS/PFS/CNT/FIH parsing and reconstruction. PS5PKGTool
maps engine types onto a small public model and renders them.

Repo: `C:\Users\User\source\repos\PS5PKGTool`

Review the **General > Package** detail section (the `Package` tab and all of its sub-tabs). We want
you to confirm that everything we display is faithful to your parser's data model and to the PS5
package format, flag anything we misread or misrepresent, and tell us which of our app-side parsers
should instead call an engine API you already expose.

You may read the PS5PKGTool sources directly; the important files are listed per tab in section 3.
If you cannot read that repo, the per-tab summaries below are sufficient to review the semantics.

---

## 2. How PS5PKGTool binds to the engine

- `PS5PKGTool.Core/Parsers/SonyPkgReader.cs`
  - Calls `ProsperoPkgTool.Containers.ProsperoPackageReader.Read(path)` -> `ProsperoPackageInspection`.
  - Maps `inspection.Kind`, `inspection.Fih` (`CntOffset`, `SignedByte`, `FormatVersion`,
    `PfsOffset`, `PfsSize`), `inspection.Cnt` (`Flags`, `ScEntryCount`, `BodyOffset`, `BodySize`,
    `ContentId`, `DrmType`, `ContentType`, `ContentFlags`), `inspection.Entries` (`ProsperoCntEntry`),
    and `inspection.Segments` into `SonyPkgSummary`.
  - Has a fallback `TryReadHeaderOnly` that hand-parses the FIH header when the engine throws
    `InvalidDataException` for metadata-less finalized images (e.g. retail disc PKGs). It reads the
    FIH magic, `CntOffset` at `0x58`, signed byte at `0x05`, format version at `0x06`, PFS offset at
    `0x10`, PFS size at `0x18`, superblock offset at `0x20`.
- `PS5PKGTool.Core/Parsers/SonyEnginePackageAccess.cs`
  - Wraps `ProsperoInnerPfsReader.Entry`, `ProsperoDecodedPackage`, `ProsperoFileBackedPackage`.
- Public model: `PS5PKGTool.Core/Models/SonyPkgModels.cs`
  - `SonyPkgSummary`, `SonyPkgEntry`, `SonyPkgSegment`, `SonyPfsSummary`, `SonyPfsEntry`.
- UI: `PS5PKGTool/Forms/MainForm.cs` -> `PopulatePackageAsync` (~line 828) and
  `PopulatePlayGoAsync` (~line 943); layout in `PS5PKGTool/Forms/MainForm.Designer.cs`.

Engine types referenced across the app: `ProsperoPackageReader`, `ProsperoPackageInspection`,
`ProsperoPackageKind`, `ProsperoFihHeader`, `ProsperoCntEntry`, `ProsperoInnerPfsReader.Entry`,
`ProsperoDecodedPackage`, `ProsperoFileBackedPackage`, `ProsperoPackageContent`,
`ProsperoDebugPackageBuilder`.

---

## 3. The Package section, tab by tab

### 3.1 Container (`tabPkgContainer` -> `gridPkgHeader`)

Source: `SonyPkgSummary` fields mapped in `SonyPkgReader.Read`. Rows are only added when a value is
non-empty.

Displayed properties: Kind (`KindDisplayName`), Location, File size, Container offset, Signed byte,
Format version, PFS offset, PFS size, PFS superblock offset, Embedded CNT offset, Header flags,
System entry count, Body offset, Body size, Content ID, DRM type, Content type, Content flags,
CNT entries, Encrypted entries, plus nested PFS access state / block size / inode count / status.

Questions:
1. Are all of these mapped from the correct engine fields with correct signedness/width, and are any
   of them redundant, misleading, or missing a mask/derived meaning users would expect?
2. `ContainerOffset` is taken from `Fih.CntOffset` while `EmbeddedCntOffset` is taken from the same
   `Fih.CntOffset`. Is that intentional/meaningful, or should one of them come from `Cnt`?
3. Is `SignedByte` (`Fih.SignedByte`) the right discriminator to present as "signed byte", and do
   `0x00`/`0x80` (and any other observed values) map to kind as we assume?
4. Should `HeaderFlags`, `DrmType`, `ContentType`, `ContentFlags`, etc. be decoded into named flags
   in the UI? If so, what are the authoritative bit definitions?
5. `PfsSuperblockOffset`, `FormatVersion`, `SignedByte` are blank for the normal (non-fallback)
   path because they are only set in `TryReadHeaderOnly`. Should `SonyPkgReader` populate them from
   the engine for all packages? Confirm the correct engine source fields.

### 3.2 Segments (`tabPkgSegments` -> `gridPkgSegments`)

Source: `inspection.Segments` -> `SonyPkgSegment(Name, Offset, Size)`. Columns: Name, Offset, Size,
Bytes.

Questions:
1. What exactly does the engine enumerate in `Segments` (CNT, SI, FIH-derived, PFS)? Are the
   `Offset`/`Size` values package-relative or segment-relative?
2. Is `Size` a byte length or a block count? Are sizes `ulong` values being safely narrowed to
   `long` anywhere?
3. Any segment kinds we should label differently or exclude from a user-facing list?

### 3.3 CNT entries (`tabPkgEntries` -> `gridPkgEntries`)

Source: `inspection.Entries` -> `SonyPkgEntry`. Columns: Id (`0x{Id:X4}`), Name
(`DisplayName` fallback `entry_0x{Id:X4}.bin`), Offset (`DataOffset`), Size (`DataSize`),
Encrypted (`Flags1 & 0x80000000`), Key (`(Flags2 >> 12) & 0x0F`).

Questions:
1. Are our `IsEncrypted` and `KeyIndex` bit extractions correct for `ProsperoCntEntry.Flags1` /
   `Flags2`? Are there other meaningful flag bits/fields (`NameOffset`, etc.) worth showing or
   interpreting?
2. Is `DataOffset` absolute in the file or relative to a segment/container? Is `DataSize` the
   stored (compressed/encrypted) size or the plaintext size?
3. Is `Name` always populated by the engine, and is the `NameOffset` table something we should
   expose or validate?
4. Does the engine expose content-key/IV/metadata per entry that we are currently hiding?

### 3.4 param.sfo (`tabPkgSfo` -> `gridParamSfo`)

Source: PS5PKGTool reads `sce_sys/param.sfo` bytes via `ReadGameFileAsync` and parses them with the
app-side `PS5PKGTool.Core/Parsers/Ps5SfoReader.cs`. Columns: Key, Format, Value.

Questions:
1. Does the engine already expose a parsed `param.sfo` (or the inner-image file entry + a content
   reader) that we should use instead of `Ps5SfoReader`? If yes, give the API.
2. Are there SFO types/keys our reader drops or mis-formats (UTF-8 vs UTF-16, arrays, nested)?
3. Is reading `sce_sys/param.sfo` through `ReadGameFileAsync` (inner PFS) correct for all package
   kinds, including metadata-less retail PKGs where there is no readable PFS?

### 3.5 Keystone / NP (`tabPkgKeystone` -> `gridKeystone`)

Source: three files probed via the inner filesystem and summarized by `AddExtraFileRowAsync`:
`sce_sys/keystone`, `sce_sys/nptitle.dat`, `sce_sys/about/right.sprx`. Columns: File, Status, Size,
Leading bytes.

Questions:
1. Is `sce_sys/about/right.sprx` the correct path, or is it `sce_sys/about/right.sprx` only for
   some titles? Should we also probe `nptitle.dat` variants or `keystone` semantics (the keystone is
   a signed blob with a known structure - should we parse it rather than show "leading bytes")?
2. Does the engine validate keystone/NP-signature presence and expose that status? If so, we should
   surface the engine verdict instead of a raw hex prefix.
3. What is the correct status vocabulary (present/missing/encrypted/unsupported) the engine uses?

### 3.6 SI contents (`tabPkgSi` -> `gridSi`)

Source: the `SI` segment (`package.Segments` where `Name == "SI"`) is opened as a ZIP by the
app-side `PS5PKGTool.Core/Services/Ps5SiReader.cs`; each entry becomes a member row (Name, Size).
Columns: Member, Size, Bytes.

Questions:
1. Is the SI segment always a plaintext ZIP archive, and is treating it as ZIP (with a 64 MiB cap)
   correct across package kinds? What about debug vs retail, and packages whose SI is absent?
2. Does the engine already expose the SI archive (members and/or their bytes)? We currently use
   `Ps5SiReader.ReadMember` to pull `playgo-chunk.dat`; the engine probably has an equivalent.
3. Are there additional SI members (`pfsimage.xml`, `npbind.dat`, `trophy*.dat`, ...) we should
   present with special handling rather than as raw ZIP entries?

### 3.7 PlayGo (`tabPkgPlayGo` -> `tabsPlayGo` -> Chunks / Scenarios / File chunks)

Source: `PopulatePlayGoAsync`.
- `playgo-chunk.dat` bytes come from the SI ZIP via `Ps5SiReader.ReadMember`.
- `sce_sys/playgo-hash-table.dat` and `sce_sys/playgo-ficm.dat` are read via `ReadGameFileAsync`.
- `PS5PKGTool.Core/Parsers/Ps5PlayGoReader.cs` parses:
  - `BuildPathMap(relativePaths)` and `Read(plgx, hashTable, ficm, pathMap)`.
- Displayed:
  - Chunks: Chunk, Label, Extents (count), Language mask (`0x{X16}`), Size (sum of extents).
  - Scenarios: Scenario, Label, Initial (chunk count), Chunks (count), Sequence.
  - File chunks: Path, Chunk, Path hash (`0x{X16}`).
  - Summary: chunk/scenario counts, default scenario, resolved-files/total, content ID.

This overlaps the engine's `ProsperoPkgTool.Gp5.PlayGoChunkReader` / `PlayGoProject` and the
`docs/playgo-reader-addendum.md` handoff in this repo.

Questions:
1. Our `Ps5PlayGoReader` hand-rolls the PLGX directory at `0xC0`-`0xF8` and the hash-table/FICM
   layout. Does the vendored engine now parse all of this? If so, list the exact API and we will
   replace `Ps5PlayGoReader`. Note the engine's documented strictness policy (throw on malformed
   extent ids) vs our intentional GUI leniency.
2. Confirm our extent accounting: we show `Extents` as the count and `Size` as the summed extent
   lengths. Is that the right user-facing meaning, and does the engine's `PlayGoChunk` expose
   `Extents`?
3. Confirm the file-to-chunk resolution (`FltPathHash` over the plain inner-image relative path,
   no leading slash) and the FICM count rules (`u32@0x0C / 2`, cross-check against hash count
   `u32@0x24`). Does the engine expose a `ReadAssignments(...)` we should call?
4. Our `Chunks` column reads `chunk.ExtentCount` and `LanguageMask`; verify offsets
   (`+0x04` extent count, `+0x10` language mask, `+0x1C` label offset) against your current reader.
5. Do we correctly represent chunk `TotalBytes` when extents are absent/malformed, and should we
   surface `HeaderFlags` (section 4 of the addendum) in the UI?
6. For the summary we say "Files mapped: resolved / total". Is "total" the full inner-image file
   count, and is that the right denominator, or should it be the FICM/hash entry count?

---

## 4. What we want back

A single review document, organized by tab, with:

1. **Findings** - each with severity (bug / risk / cosmetic / question), the engine type and field
   (or PKG format offset) that establishes the correct behavior, the PS5PKGTool location
   (`file:line`), and a concrete recommendation.
2. **Duplication list** - every app-side parser (`Ps5SfoReader`, `Ps5SiReader`, `Ps5PlayGoReader`,
   `Ps5ParamReader`, `TryReadHeaderOnly`) that duplicates an engine capability, with the exact
   engine API to use instead. Include signatures and any caveats (e.g. leniency vs strictness).
3. **Engine gaps** - anything the Package UI cannot do because the engine does not expose it yet.
   Propose the smallest API addition, with a signature, if that is the right fix.
4. **Engine bugs** - any parsing/decoding defect you find while cross-checking, even if triggered by
   our usage rather than confirmed in your tests.
5. **Confirmation** - for each tab, state explicitly what is correct, so we know what not to change.

Please cite PKG/PFS/CNT/FIH/PLGX offsets or your own type names as evidence; do not guess. Retail
title bytes are not to be attached. If you want test vectors, say which package kinds you need and
we will describe synthetic expectations rather than shipping game content.

## 5. Boundaries

- Do not modify PS5PKGTool's files as part of this review; findings only.
- If you propose engine changes, describe them against the pinned commit above.
- The package sample matrix used by PS5PKGTool is: a debug FPKG (`SIFU.pkg`), a retail disc PKG
  (`IP9100-PPSA01280_00-...pkg`), and a real FFPFSC image with an SI/PLGX payload
  (`PPSA27616.ffpfsc`). Ask if you want specific values from any of them.
