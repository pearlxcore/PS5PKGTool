# HANDOFF — wire the build settings into PS5PKGTool

The engine now carries everything these settings need. This note maps each requested setting to the
engine option/CLI and says what to change in PS5PKGTool. **Do not touch the unrelated Library work.**

New engine build: **655,360 bytes, SHA-256 `3A112B5E3619587A90532AF2885A60BA05396A4FD2EE59D62940B643DB554079`**
(head `0a84955` + byte-progress fields). **Re-vendored** into `ThirdParty\ProsperoPkgTool\`. Every setting
in the table below is already wired in the GUI + Core; the engine now also carries the build-stage contract,
byte-accurate progress, and the single-pass / prepositioned perf work.

## Setting → engine → action
| Setting | Engine | PS5PKGTool action |
|---|---|---|
| **Title ID** | none needed — read from the source `sce_sys/param.json` (`titleId`, else `contentId[7..16]`) | auto-fill the field from the imported source (like Content ID). Display-only; the engine packages `param.json` verbatim. |
| **Temporary folder** | `DebugPackageBuildOptions.TempDirectory` / CLI `--temp <dir>` | add a folder picker; pass through. Blank ⇒ `%TEMP%`. |
| **Version** | none needed — read `contentVersion` from `param.json` | auto-fill (display). The engine uses the source value. |
| **Passcode** | default is 32×`'0'` (`Gp5Constants.DefaultPasscode`) | prefill `00000000000000000000000000000000`. |
| **Package type APP/AC** | APP only (`content_type 0x20`, flags `0x02020000`). **AC is rejected** ("DLC-data creation is not enabled: no clean-room oracle") | show **Application (APP)**; show **AC** disabled/experimental. |
| **DRM type** | `DebugPackageBuildOptions.DrmTypeOverride` / CLI `--drm-type <token>` | **Advanced section only**: an off-by-default checkbox **"Force DRM type to standard"** (checked → `DrmTypeOverride = "standard"`). Do **not** surface a raw DRM dropdown/next-to-compression; leave token entry to the CLI. Default off = preserve the source token. |
| **SDK override + list** | `SdkVersionOverride` (`ulong?`) + `ProsperoPkgTool.Content.ProsperoSdkVersions.Releases` (majors 1–11, each `.Version` e.g. `9.00.00.40`) | combo: “Auto (SDK)” = `null`, else `release.ExecutableVersion`. |
| **Compression** | `ProsperoInnerCompressionMode { Stored, Kraken, Auto }` | combo: **Kraken** = `Auto`, **Uncompressed** = `Stored`. **App default = Kraken (`Auto`)** — this is the engine's own default; the Core builders must stop hardcoding `Stored`. `Auto` only packs blocks that shrink, so it is never larger than `Stored`. |
| **Kraken level** | `KrakenLevel` (−4..9) | combo with Oodle names (below). ⚠️ our encoder is fixed-subset: the level is **header-only** and does not change bytes. |
| **Kraken threads** | `KrakenThreads` (0 = auto; 1..256) | numeric, default 0. |
| **PlayGo chunks** | `DebugPackageBuildOptions.PlayGoChunkCount` (default **1**) / CLI `--playgo-chunks <n>` | numeric, **default 1** (or reuse the source `playgo-chunk.dat`). Do **not** default to 64 — N>1 is structural/unverified and files still land in chunk 0. |
| **Free-space check** | `DiskSpaceGuard`; CLI exit code **5** | map exit 5 → dialog; optional preflight (see `HANDOFF_DISK_SPACE_GUARD.md`). |

## Kraken level combobox (Oodle `OodleLZ_CompressionLevel`)
```
-4 HyperFast4   -3 HyperFast3   -2 HyperFast2   -1 HyperFast1
 0 None          1 SuperFast     2 VeryFast      3 Fast
 4 Normal        5 Optimal1      6 Optimal2      7 Optimal3 (default)
 8 Optimal4      9 Optimal5
```

## Core wiring
Both `PS5PKGTool.Core/Builders/ProsperoDebugPackageBuilder.cs` (dump) and
`VolumeDebugPackageBuilder.cs` (image) build the same engine options — forward the same fields so
dump and image builds behave identically:
```
Compression   = <chosen>,        // Stored | Kraken | Auto   (was hardcoded Stored)
KrakenLevel   = <chosen>,
KrakenThreads = <chosen>,
PlayGoChunkCount = <chosen>,     // default 1
SdkVersionOverride = <…>,        // already forwarded
DrmTypeOverride = <…>,           // null = preserve source token (default); set to force e.g. standard
TempDirectory = <…>,             // already forwarded
```

## Notes / cautions
- **Compression default = `Auto` (Kraken where it helps).** This intentionally changes the bytes of a
  default build versus the old hardcoded-`Stored` behaviour — smaller packages, and it matches the
  engine default. Builds stay reproducible for the same settings/seed; anyone who needs the old bytes
  selects **Uncompressed**.
- `Auto` never packs a block larger than its input, so size can only improve versus `Stored`; the cost
  is CPU time only.
- **AC is not implemented**: show the option but disable Run ("content-type and entitlement generation
  are not implemented"). Never pass AC to the engine — it throws.
- **DRM type is preserved verbatim by default.** Do **not** force `standard`: the Sony-built fixture
  itself ships `applicationDrmType: "free"`, and the engine has tests asserting publisher tokens
  (`free`, `upgradable`, …) survive untouched. `--drm-type` is the opt-in escape hatch only.
- **`Kraken` level is cosmetic** on the clean-room encoder (recorded, not applied).
- **PFS v3/shuffle/native-Oodle are out of scope** (not in this engine).
- Verify: default build succeeds and is reproducible for a fixed seed; pick **Uncompressed** ⇒ matches
  the old hardcoded-`Stored` output; PlayGo 64 ⇒ `playgo-chunk.dat` parses with 64 chunks.
