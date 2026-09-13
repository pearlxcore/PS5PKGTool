# HANDOFF — re-vendor ProsperoPkgTool engine (7c50180) into PS5PKGTool

The `ProsperoPkgTool` (PPT) engine moved ahead of the copy you currently vendor.

- **Your vendored DLL:** `074C5C65…` (630,272 B)
- **New engine head:** `7c50180`
- **New engine DLL:** 642,048 bytes, SHA-256 `77AD92EA8A82FE9E74AB8612C8E4EE9C48624376939C07CF50C4A0AD3ABB68C3`

## What changed in the engine (since your vendor)
| Commit | Change |
|---|---|
| `b7dc0e6` | SDK version override (`SdkVersionOverride`, `--sdk`) — you already integrated this |
| `c683934` | **Kraken decoder**: decode **verbatim** sub-chunks (by size) and honour the chunk-1 **restart** bit `0x40` |
| `7c50180` | **Stale build-workspace sweep** + **free-space preflight** + `--temp` + GUI exit-5 dialog |

**No produced-package bytes change.** `c683934` only widens what we can *read*, and `7c50180` is operational (temp/disk/preflight) — the encoder and FIH/CNT writers are untouched. Packages built before these commits remain valid.

## Step 0 — re-vendor
1. `dotnet build C:\Users\User\source\repos\ProsperoPkgTool\ProsperoPkgTool\ProsperoPkgTool.csproj -c Release`
2. Copy `ProsperoPkgTool\bin\Release\net10.0\ProsperoPkgTool.dll` (and `.pdb`) over `PS5PKGTool\ThirdParty\ProsperoPkgTool\`.
3. Build PS5PKGTool.

## Step 1 — new CLI surface to expose
1. **Free-space preflight** (see `HANDOFF_DISK_SPACE_GUARD.md`): map CLI **exit code 5** to a “not enough free space” dialog; optionally call `DiskSpaceGuard` before launching. Near-limit prints a `warning:` but proceeds.
2. **`--temp <dir>`** (new on `img_create` / `convert`): add an optional “Temporary / workspace folder” picker in the build options and pass it through. Blank ⇒ `%TEMP%`.
3. **Sweep options** (optional): `--no-temp-sweep`, `--stale-hours <n>` (default 12).

## Engine API added (for a preflight / advanced UI)
- `ProsperoPkgTool.Containers.DiskSpaceGuard.Estimate(rawPayloadBytes, outputPath, tempDirectory)` → `SpaceRequirement[]`
- `DiskSpaceGuard.Check(requirements, probe?)` → `DiskSpaceReport` (`DiskSpaceStatus` = Ok / NearLimit / Insufficient)
- `ProsperoInsufficientSpaceException` (`: IOException`)
- `ProsperoPkgTool.Containers.TempWorkspace` (workspace create / sweep / delete)
- `DebugPackageBuildOptions`: `TempDirectory`, `SweepStaleTempWorkspaces` (true), `StaleWorkspaceHours` (12), `SkipFreeSpaceCheck` (false) — plus the existing `SdkVersionOverride`

## Transparent (no GUI work)
- Kraken decoder now decodes verbatim-copy and restarted chunk-1 sub-chunks. Verified against LibProsperoPkg `748eabf`’s encoder (LZ / BareEntropy / Verbatim, all byte-perfect). This only affects reading/verification.

## Guardrails / verify
- Do **not** touch the unrelated uncommitted Library work.
- Re-vendor from the clean engine commit `7c50180`.
- Build with default options → output byte-identical to today; build with the target on a full drive → exit 5 + space dialog.
