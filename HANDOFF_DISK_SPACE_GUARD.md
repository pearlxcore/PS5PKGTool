# HANDOFF — surface the engine free-space preflight in PS5PKGTool

Handoff from `ProsperoPkgTool`. The engine and CLI now fail fast with a clear message when a build
cannot fit; your job is the thin GUI surface. Do **not** touch the unrelated Library work in the
working tree.

## What the engine now provides

- `ProsperoPkgTool.Containers.DiskSpaceGuard` (public):
  - `Estimate(long rawPayloadBytes, string outputPath, string? tempDirectory)` → `IReadOnlyList<SpaceRequirement>`.
  - `Check(IReadOnlyList<SpaceRequirement> requirements, Func<string,long>? freeSpaceProbe = null)` → `DiskSpaceReport`
    with `Status` `Ok` / `NearLimit` / `Insufficient`.
- Model: peak ≈ **2× the image** (inner image + output package; the CNT is metadata-only and the outer
  PFS is written directly into the final package, so there is no full-image copy). Error threshold =
  estimate + 12% + 512 MB; near-limit warning = estimate + 25% + 512 MB. (It deliberately assumes no
  compression, so it never blocks a build that could fit.)
- `ProsperoPkgTool.Containers.ProsperoInsufficientSpaceException` (`: IOException`).
- `ProsperoDebugPackageBuilder.Build` runs the preflight (unless `DebugPackageBuildOptions.SkipFreeSpaceCheck`),
  throws the exception on insufficient space, and logs a `warning: low free space …` line on near-limit.
- CLI exit code **5** on insufficient space, with `Error: not enough free space: need ~X GB on <drive> (<what>), have Y GB free`.

New engine build: **655,360 bytes, SHA-256 `EC2396D758C12FD3C8145B024C9600BB3A488AE5FFA4E69E24D5C0B49E26E819`**
(head `0a84955` + no-copy free-space model).

## Step 0 — re-vendor
1. `dotnet build C:\Users\User\source\repos\ProsperoPkgTool\ProsperoPkgTool\ProsperoPkgTool.csproj -c Release`
2. Copy `ProsperoPkgTool\bin\Release\net10.0\ProsperoPkgTool.dll` (and `.pdb`) over
   `PS5PKGTool\ThirdParty\ProsperoPkgTool\`.
3. Build PS5PKGTool.

## Step 1 — GUI
1. The build paths already spawn the CLI; map **exit code 5** to a specific dialog
   ("Not enough free disk space — see the log; free space or change the temp/output volume").
2. Optional preflight before launching a build: call `DiskSpaceGuard.Estimate(...)` with the sum of
   the source files' sizes and the chosen output/temp paths, then `Check(...)`:
   - `Insufficient` → block with an error dialog;
   - `NearLimit` → warn (non-blocking) then proceed;
   - `Ok` → proceed.
   This gives the user the warning *before* a long run. (`ProsperoPkgTool.Core` already references the
   engine; expose a small `DiskSpaceStatus` facade like the existing `Ps5SdkVersions` one so the UI
   layer stays off the engine types if you prefer.)

## Notes
- The estimate is conservative (raw payload is the upper bound), so it will not green-light a doomed build.
- Same-volume temp+output is collapsed into one requirement; different volumes are checked separately.
- Near-limit is advisory only; the build still proceeds.
