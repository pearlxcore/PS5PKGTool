# HANDOFF — follow the engine's build staging

The engine is now the single source of truth for build staging. **Do not hardcode a step list in
PS5PKGTool**; render the stages the engine reports.

Engine build: **655,360 bytes, SHA-256 `EC2396D758C12FD3C8145B024C9600BB3A488AE5FFA4E69E24D5C0B49E26E819`**
(head `0a84955` + no-copy free-space model + byte progress). **Re-vendored** into `ThirdParty\ProsperoPkgTool\`.

## Contract

New public types in namespace `ProsperoPkgTool`:

```
public enum ProsperoBuildStage { Staging, InnerImage, Naps, OuterPfs, Cnt, Finalize }

public static class ProsperoBuildStages
{
    public static IReadOnlyList<ProsperoBuildStage> Build { get; } // InnerImage, Naps, OuterPfs, Cnt, Finalize
    public static string Name(ProsperoBuildStage stage);           // "Inner image", "NAPS", ...
    public static int IndexOf(ProsperoBuildStage stage);           // 0-based in Build; -1 for Staging
}
```

`ProsperoBuildProgress` gains an optional `ProsperoBuildStage? StageId` (init-only). The existing
`Stage` string is still populated for display but should **not** be parsed — use `StageId` +
`ProsperoBuildStages.Name`.

## Rules for PS5PKGTool

- Subscribe to `DebugPackageBuildOptions.Log` → `DebugPackageBuildLog.Progress` (`IProgress<ProsperoBuildProgress>`).
- On each report with a non-null `StageId`: set the current step to
  `Name(stageId)` and its position to `IndexOf(stageId) + 1` of `ProsperoBuildStages.Build.Count`.
- Ignore reports with a null `StageId` (extraction/other operations) for build progress.
- Stage 5 (`Finalize`) is the last report of a build.
- `Staging` (convert only) is reported by `ProsperoBackupConverter`; it is not part of `Build`
  (`IndexOf` returns -1) — show it as a separate pre-build step or map it to position 1 of 6.
- `Done`/`Total` are stage-native units (files or blocks); stages `Naps`/`Cnt`/`Finalize` report
  `1/1` as coarse boundaries — do not drive a percentage bar off them alone.

## Notes

- Progress reporting is read-only and never changes the produced bytes.
- The engine's stage model matches the reference publisher's observable 5-stage build
  (inner image → NAPS → outer PFS → CNT → finalization), so the two tools look the same.
