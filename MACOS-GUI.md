# macOS Avalonia development frontend

This modified GPL-3.0 port adds `PS5PKGTool.Mac` (net10.0, Avalonia 12.1.3).
The Windows UI is unchanged. Package parsing and conversion use the same
`SonyPkgGameReader` and `SonyPackageImageConversion.ConvertAsync` as the CLI.

## Run and publish

With the .NET 10 SDK installed, from the repository root:

```sh
dotnet run --project PS5PKGTool.Mac
bash scripts/publish-macos.sh
open "artifacts/PS5 PKG Tool.app"
```

The script runs `dotnet publish` with `-c Release -r osx-arm64 --self-contained true`,
creates `Contents/Info.plist`, places the app and runtime under `Contents/MacOS`,
and preserves licenses/notices under `Contents/Resources`. The bundle needs no
installed .NET runtime. Move an existing bundle before republishing; the script
refuses to replace it. Failed publishing removes the newly created partial bundle.
The completed bundle is ad hoc signed and verified. It targets Apple Silicon
macOS 14 or later, matching the [supported .NET 10 versions](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md).
Ad hoc signing seals local code/resources; it does not provide Developer ID
signing or notarization. No release has been uploaded. Distributions must include
the corresponding modified source, including changes currently uncommitted locally.

### Drag-and-drop DMG

After publishing the app:

```sh
bash scripts/create-dmg.sh
open artifacts/PS5PKGTool-Mac-osx-arm64.dmg
```

The DMG contains `PS5 PKG Tool.app`, an `Applications` shortcut and installation
instructions. Drag the app to Applications, eject the DMG, and open the installed
app. Neither installation format requires an installed .NET runtime. The script
refuses to overwrite an existing DMG. `hdiutil` is used only to package the macOS
installer; PS5 exFAT conversion continues to use the managed writer.

This development build is not Developer ID signed or notarized. If macOS blocks
it, follow Apple's [Open Anyway instructions](https://support.apple.com/en-us/102445).
For a locally built or otherwise verified copy that remains blocked by quarantine,
remove that attribute from this app only, then try opening it again:

```sh
xattr -dr com.apple.quarantine "/Applications/PS5 PKG Tool.app"
```

Provide corresponding modified source when sharing binaries; CI creates a source
archive alongside its app artifact.

Local packaging produced a roughly **49 MB** DMG; `hdiutil verify` reported its
checksum **VALID**. Creating the image required access outside the execution
sandbox to macOS's disk-image service. The updated workflow passes `actionlint`
and includes this DMG in its development artifact; hosted execution is pending.

## Conversion

Select a finalized debug/fake PKG, check the title and title ID, and choose a
**new** `.exfat` output filename. Retail, metadata-only and patch packages are
rejected by the frontend. No passcode or base-package selection is exposed.

Temporary extraction defaults to the output directory. Choose another existing
workspace if needed. Allow space for the logical extracted game plus the final
image. Temporary files are removed on success, cancellation or failure; a
force-quit or inaccessible volume can prevent cleanup. Conversion runs on a
background thread; progress shows extraction, writing and verification stages.
Cancelling waits for the current operation to reach a cancellation check.
Closing during work requests cancellation and keeps the window open for cleanup.

Large real FPKG conversion was verified through the CLI; see
[PKG-CONVERSION.md](PKG-CONVERSION.md). Choose an output volume with enough
space for both staging and the image. The GUI smoke check converts a small
generated package; optional external fixtures can also check metadata.

## Checks on Apple Silicon macOS

Verification used .NET SDK 10.0.401. With .NET 10 on PATH:

```sh
dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj --disable-build-servers -p:UseSharedCompilation=false
dotnet run --project PS5PKGTool.Tests --no-build --no-restore
PS5PKGTOOL_TEST_PKG=/path/to/game.pkg dotnet run --project PS5PKGTool.Tests --no-build --no-restore -- --gui-smoke
bash scripts/publish-macos.sh
```

The test project is an executable regression runner, not a `dotnet test` project.
Building it builds all six cross-platform projects. Checks cover nested/Unicode
folder images, stored and managed Kraken-compressed packages, GUI metadata,
conversion and content readback, progress, cancellation, workspace cleanup,
overwrite protection, unsupported packages and error states. The native smoke
check opens an actual Avalonia macOS window, exercises its view model, verifies
metadata bindings and saves a rendered window image to
`Path.GetTempPath()/ps5pkgtool-native-gui.png`.

Results on this machine: all six projects built with **0 warnings, 0 errors**;
the regression runner and native GUI smoke check both exited **0**. The supplied
package metadata was displayed correctly as **FPKG**. Publishing exited **0**,
`plutil -lint` reported **OK**, and `file` identified the apphost as
**Mach-O 64-bit executable arm64**. The bundle occupies approximately **115 MB**.
Launching its apphost directly succeeded without using the temporary SDK;
`open "artifacts/PS5 PKG Tool.app"` also exited **0**.

During implementation, the initial Avalonia 12 XAML build required an explicit
binding data type and replacement of deprecated `Watermark`/bitmap-save APIs.
Those project/API issues were fixed; no macOS-only compile failure was found.

No Windows-native Oodle, LibProsperoPkg or PublishingTools binaries are added
to the bundle. The pre-existing managed Oodle.NET reference remains in Core but
is not invoked by this conversion path. Avalonia uses macOS-native rendering
libraries. System mounting and PS5 hardware playback remain unverified.

## GitHub Actions

`.github/workflows/macos-build.yml` runs on pushes, pull requests and manual
dispatches using `macos-latest` and .NET 10. It restores/builds the test project
(and its five project references), runs the executable regression checks in
Release, and reuses `scripts/publish-macos.sh` to publish `osx-arm64`.
Native window smoke checks remain a local check; CI does not require a desktop
session or a user-supplied game package.

The `PS5PKGTool-Mac-osx-arm64` artifact contains the installation DMG, a gzip tar archive of the `.app`
and its source revision, plus a corresponding source archive from the same
checked-out commit. Extract the app archive with `tar -xzf` to preserve executable
permissions. The source archive includes the macOS projects, build scripts,
licenses and the existing managed ProsperoPkgTool dependency. It excludes the
Windows GUI and its native third-party payloads. The solution's Windows project
entry is retained; build the macOS/test project directly.

Artifacts expire after 14 days. The workflow does not create GitHub releases.
Commit and push all of the port's files before expecting a hosted run; local
uncommitted files are not included by GitHub checkout or `git archive HEAD`.

Local CI verification ran every workflow shell step from a clean temporary
checkout containing the current port files:

| Command/check | Result |
| --- | --- |
| `actionlint .github/workflows/macos-build.yml` (1.7.12) | Passed |
| `dotnet restore PS5PKGTool.Tests/PS5PKGTool.Tests.csproj` | Passed |
| `dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj -c Release --no-restore --disable-build-servers -p:UseSharedCompilation=false` | All six projects; 0 warnings, 0 errors |
| `dotnet run --project PS5PKGTool.Tests -c Release --no-build --no-restore` | All regression checks passed; exit 0 |
| `bash scripts/publish-macos.sh` | Passed; arm64 `.app`, valid plist |
| Workflow `tar` / `git archive` commands | Passed; executable permissions and matching source revision preserved |
| Extracted source archive: `dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj -c Release --disable-build-servers -p:UseSharedCompilation=false` | All six projects rebuilt independently; 0 warnings, 0 errors |

The source archive contains the required projects/licenses/managed engine and
excludes the Windows-native third-party payloads. Local verification covers the
shell steps and workflow syntax; GitHub action setup and artifact upload await
the first hosted run. No branch was pushed and no hosted run is claimed here.

## Audit follow-up verification

Public documentation uses placeholder paths and omits identifying details of
the external game fixture. Custom staging uses one unique `.ps5pkgtool-*`
directory, removed in full. CLI and GUI both reject patch packages; the shared
service retains patch support for Windows callers. The first CLI Ctrl+C requests
cancellation; a second Ctrl+C terminates the process.

| Check | Result |
| --- | --- |
| `dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj -c Release --disable-build-servers -p:UseSharedCompilation=false` | 0 warnings, 0 errors |
| `dotnet run --project PS5PKGTool.Tests -c Release --no-build --no-restore` | Exit 0; includes full staging cleanup, patch-header rejection and direct shared-service rejection checks |
| `dotnet run --project PS5PKGTool.Tests -c Release --no-build --no-restore -- --gui-smoke` | Exit 0 |
| `bash scripts/check-publish.sh` | Passed; failed publishing removes partial output and preserves pre-existing bundles |
| `bash scripts/publish-macos.sh` then `codesign --verify --deep --strict "artifacts/PS5 PKG Tool.app"` | Passed; ad hoc signature seals plist and resources |
| `bash scripts/create-dmg.sh` | Updated DMG; checksum VALID |
| `actionlint .github/workflows/macos-build.yml` and `git diff --check` | Passed |
