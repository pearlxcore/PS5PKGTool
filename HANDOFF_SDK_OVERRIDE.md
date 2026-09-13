# HANDOFF — expose the engine SDK override in PS5PKGTool

Handoff from the `ProsperoPkgTool` engine work. Your job: surface the new engine
`SdkVersionOverride` in PS5PKGTool (Core + GUI). Do **not** touch the existing uncommitted
Library work (`PS5PKGTool.Core/Services/LibraryFileMover.cs`, `MainForm.Library*.cs`, etc.).

## What the engine now provides

`ProsperoPkgTool.Containers.DebugPackageBuildOptions` gained:

```csharp
/// <summary>Full 64-bit executable SDK id; stamped into param.json + every fake-signed module's
/// .sceversion. Null preserves the source metadata verbatim.</summary>
public ulong? SdkVersionOverride { get; init; }
```

It rewrites `sce_sys/param.json` (`sdkVersion` and clamped `requiredSystemSoftwareVersion`) and the
`.sceversion` tuple of each fake-signed module. Related public helpers:
`ProsperoPkgTool.Content.ProsperoSdkVersions` (`Releases`, `TryGetByMajor`, `GetByMajor`,
`TryParse`, `ToPackageVersion`). The CLI already accepts `img_create ... --sdk <major|0x-id>` and
`convert ... --sdk ...`.

Engine source: `C:\Users\User\source\repos\ProsperoPkgTool` (change set is **uncommitted** as of this
handoff).

## Step 0 — re-vendor the engine DLL

1. `dotnet build C:\Users\User\source\repos\ProsperoPkgTool\ProsperoPkgTool\ProsperoPkgTool.csproj -c Release`
2. Copy `ProsperoPkgTool\bin\Release\net10.0\ProsperoPkgTool.dll` over
   `PS5PKGTool\ThirdParty\ProsperoPkgTool\ProsperoPkgTool.dll`.

Expected engine build: **630,272 bytes, SHA-256 `074C5C651CAA256FAF715EFD4EBF5AEBBECBCB9A740B133C0F6503C949510FE0`**
(currently vendored: `EB88242E1D85B6D2CBED42F5DBEC2649B5D1B559ACC0A31758C793E3A464EC09` — stale).
3. Build PS5PKGTool and confirm it still compiles.

## Step 1 — Core plumbing

1. Add to both option types a passthrough property:
   - `PS5PKGTool.Core/Builders/SonyDebugPackageBuilder.cs` → `SonyDebugPackageBuildOptions`
   - `PS5PKGTool.Core/Builders/ProsperoDebugPackageBuilder.cs` → `ProsperoDebugPackageBuildOptions`

   ```csharp
   /// <summary>Optional full 64-bit executable SDK id to stamp (param.json + .sceversion).
   /// Null preserves the source metadata.</summary>
   public ulong? SdkVersionOverride { get; init; }
   ```

2. Forward it into the engine's `DebugPackageBuildOptions` in:
   - `PS5PKGTool.Core/Builders/ProsperoDebugPackageBuilder.cs` (`CreateFromDirectory`), and
   - `PS5PKGTool.Core/Builders/VolumeDebugPackageBuilder.cs` (`CreateFromImage`):
   ```csharp
   SdkVersionOverride = options.SdkVersionOverride,
   ```
3. Forward `SonyDebugPackageBuildOptions.SdkVersionOverride` into the `ProsperoDebugPackageBuildOptions`
   created in `SonyDebugPackageBuilder.CreateFromDirectoryAsync`.

## Step 2 — GUI

1. In the build options UI (where ContentId / passcode / seed are collected) add:
   - a checkbox **“Override SDK version”**, and
   - a dropdown bound to `ProsperoPkgTool.Content.ProsperoSdkVersions.Releases` showing `Release.Version`
     (e.g. `9.00.00.40`), defaulting to the major **9** entry.
2. When enabled, set:
   ```csharp
   SdkVersionOverride = ProsperoSdkVersions.GetByMajor(selectedMajor).ExecutableVersion;
   ```
   otherwise leave it `null`.
3. Pass it through the two build call sites in `PS5PKGTool/Forms/MainForm.ImageTools.cs`
   (~line 778 directory build, ~line 781 image build).

## Guardrails

- **Default off** must yield `SdkVersionOverride = null` → output byte-identical to today.
- Preserve the current uncommitted Library changes; do not reformat/revert them.
- The engine change is uncommitted — if you prefer to vendor from a clean commit, coordinate first.

## Verification

1. Build PS5PKGTool.
2. Build/convert an image with the override **off** → `SonyDebugPackageBuilder.Validate` still passes.
3. Build with SDK 9 → read the produced CNT `param.json` and assert
   `"sdkVersion": "0x0900000000000000"`.
4. If the source carries a real `eboot.bin`, confirm the module `.sceversion` tuple reflects the
   override (engine `ProsperoSelfBuilder.TryGetSceVersionRecord`).
