# Debug/fake PKG → exFAT on macOS

## Existing conversion path

The Windows UI's `MainForm.ImageTools.ConvertImageAsync` and its restored-task
path, `MainForm.Tasks.RebuildImageConvert`, call
`SonyPackageImageConversion.ConvertAsync` for package conversion. The new CLI
calls the same service, after restricting input to finalized debug packages, for:

```sh
dotnet run --project PS5PKGTool.Cli -- convert input.pkg output.exfat
```

The path is:

1. `SonyPkgGameReader.Read` → `SonyPkgReader.Read` → the managed
   `ProsperoPackageReader.Read`: inspect FIH/CNT headers and entries.
   The conversion service requires a finalized debug/patch kind and a
   nonempty PFS payload before proceeding.
2. `SonyEnginePackageAccess.FromInspection` caches the inspection. Outer
   CNT `param.json` can be read without reconstructing the inner filesystem;
   if it is absent, the metadata reader may try the inner filesystem.
3. `GameFileSystem.Open` → `SonyPackageGameFileSystem`: enumerating
   `engine.Files` triggers lazy `SonyEnginePackageAccess.EnsureDecoded`.
4. `ProsperoPackageContent.RequiresFileBacked` selects either
   `ReadFileBacked` or `Read`. The file-backed path is selected for finalized
   debug packages with an outer PFS above 2 GiB or a reconstructed mount too
   large for a single managed array.
5. The engine reads outer PFS, derives default-passcode keys, decrypts AES-XTS
   sectors, selects a NAPS dialect, and reconstructs the inner PFS. Stored
   blocks are copied. Compressed blocks call
   `ProsperoPs5InnerImageReader.DecodeKrakenDeterministic` → the managed
   `ProsperoPkgTool.Compression.KrakenDecoder.DecodeBlock`.
6. `ProsperoInnerPfsReader.Enumerate` supplies logical entries.
   `GameFileSystem` merges these with CNT content paths; inner entries win
   when both sources expose the same path.
7. `OpenRead` → `SonyEnginePackageAccess.OpenInnerFile` returns a bounded
   stream over the inner mount. The file-backed `ProsperoInnerMountStream`
   decodes and caches a block as needed. CNT entries are currently loaded
   into a byte array and exposed through a `MemoryStream`.
8. `SonyPackageImageConversion.ExtractCleanTreeAsync` copies entry streams
   with 1 MiB buffers to a temporary directory, excluding the assembler's
   internal tables and debug bookkeeping files.
9. `Ps5ImageConversionService.ConvertAsync` → `BuildAsync` →
   `ExfatImage.WriteDirectoryAsync`, followed by `ExfatImage.VerifyAsync`.
   The service publishes the verified image and removes the staging tree.

This is an extraction prototype, not direct PKG-stream → exFAT serialization.
It needs space for the clean tree and completed image at the same time.
Game asset files are copied verbatim; the converter decodes package-level
NAPS blocks, not compression inside an asset format such as a game archive.
Set `TMPDIR` before starting the process to place its staging tree on a
different volume. PKG inspection and initial reconstruction are synchronous;
cancellation is polled while extracting, writing and verifying.

## Native dependencies: what actually gets called

The shipped engine is a managed `net10.0` assembly. Its version notice and
SHA-256 are in `PS5PKGTool/ThirdParty/ProsperoPkgTool/VERSION.txt`.
The exact vendored DLL was decompiled with ILSpy 11.1.0.9782.
Its assembly references are framework assemblies; it has no Oodle.NET reference, native imports, or native loader
calls. Both its encoder and decoder for supported Kraken blocks are managed.

| Component | Load/call boundary | Used by CLI conversion? |
| --- | --- | --- |
| `ProsperoPkgTool.dll` | Normal managed assembly resolution when Core calls the engine reader; `SonyPkgReader` and `SonyEnginePackageAccess` call its APIs | Yes |
| `Oodle.NET` 2.2.0 | `OodleDotNet.Oodle(string)` calls `NativeLibrary.Load`; its constructor resolves `OodleLZ_Compress`, `OodleLZ_Decompress`, and `OodleLZ_GetCompressedBufferSizeNeeded` | No source/engine call constructs this wrapper; regression tests assert the assembly remains unloaded |
| `oo2core_9_win64.dll` | Listed as copied content in the Windows GUI `.csproj`; no loader call was found in the repository source or the decompiled engine/LibProsperoPkg assemblies | No; it is absent from CLI output |
| `LibProsperoPkg.dll` 1.2.0 | `LibProsperoPkg12.Load` uses an isolated `AssemblyLoadContext`; called by `LppBackend`, whose build/validate/extract methods invoke it through reflection | No; image conversion bypasses backend selection |
| Publishing Tools / Reduced Oodle | In the alternate LibProsperoPkg builder, `ProsperoReducedKrakenEncoder.LoadBackend` requires 64-bit Windows and loads a supported Publishing Tools library through `NativeLibrary.Load`; it uses native function offsets | No |
| Windows raw-device APIs | Ufs2 `DriveIO` imports `kernel32.dll` | No; conversion writes ordinary files |

Core still has its pre-existing Oodle.NET package reference. No dependency
removal was needed to prove conversion; no native binaries were added or copied.
The alternate LibProsperoPkg payload's `libScePubTools.dll` has Windows imports
and is not established as macOS compatible. The selected conversion path
does not load that payload.

The Oodle.NET wrapper accepts a path and uses the platform's native loader,
so its API can load a suitable macOS dylib in principle. No suitable licensed
macOS binary was present or tested, and no ABI/architecture compatibility is
claimed. Supported conversion currently needs no optional Oodle provider.

An unsupported compression mode stops in the managed decoder; its status
includes `UnsupportedEntropy`/`UnsupportedExcessMode`, and the engine reports
the failing NAPS block's logical offset, source offset, length and flags.
It does not silently fall back to native Oodle. Retail encrypted payloads and
patches requiring a reference package remain outside this prototype's proven
support. The CLI and GUI reject patches. Core retains patch support for Windows callers.
No custom-passcode CLI option was added.

## Automated proof

`PS5PKGTool.Tests/PackageConversionChecks.cs` generates legal synthetic debug
packages using the existing builder, with fake signing and right.sprx injection
disabled. One fixture uses stored blocks; another uses Auto and asserts that
the build emitted Kraken blocks and produced a smaller package. For each:

- metadata reads successfully;
- the CLI converts it to exFAT;
- nested payload files, zero-byte files and `eboot.bin` match in size and
  streaming SHA-256;
- outer CNT `param.json` survives and internal PFS tables are excluded;
- existing output is rejected;
- Oodle.NET and LibProsperoPkg are not loaded.

An invalid package must fail without publishing output. The earlier folder
regression checks also remain enabled.

The investigation reproduced one shared-service bug: a CNT metadata-only
package was accepted and reported as a converted image (11 bookkeeping/content
entries, with no game PFS). The shared `SonyPackageImageConversion.ConvertAsync`
input guard now rejects metadata containers, retail declarations and missing
PFS payloads. Regression fixtures strip a generated debug package down to its
CNT, or change its FIH signed byte to a retail declaration; both are rejected
without output. Later GUI work also added configurable staging and early cancellation checks.
Windows GUI source and the vendored engine are unchanged.

Optional local fixtures use `PS5PKGTOOL_TEST_PKG` and
`PS5PKGTOOL_TEST_EXFAT`. The runner checks all expected package paths/sizes
after conversion exclusions, then compares up to 1 MiB at the beginning,
middle and end of `eboot.bin`, `param.json` and the largest payload file.
These are sampled content checks; the synthetic fixtures use full hashes.

Commands with the .NET 10 SDK on PATH:

```sh
dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj --disable-build-servers -p:UseSharedCompilation=false
dotnet run --project PS5PKGTool.Tests --no-build --no-restore
```

Results: all five projects build with 0 warnings and 0 errors; the regression
runner exits 0 for folder, stored PKG, Kraken PKG, and invalid-input checks.

## Large real FPKG verification

A ~15 GB real FPKG was identified as `FinalizedDebug`. Inventory: 110 files,
about 21.68 GB of logical content before conversion exclusions. File-backed
reading was selected, and metadata and executable samples were read without Oodle.

The local volume had insufficient space for the extracted tree and image.
Staging and output used another volume. Equivalent command with placeholders:

```sh
TMPDIR=/path/to/workspace dotnet run --project PS5PKGTool.Cli --no-build --no-restore -- convert /path/to/game.pkg /path/to/game.exfat
```

Full conversion result: **passed, exit 0**. The service extracted all selected
logical content, wrote the image, read all 21,683,107,148 logical bytes through
the exFAT verifier, published the output, and removed its staging files.

- Output: `/path/to/game.exfat`
- Image size: **21,689,532,416 bytes** (about 21.69 GB)
- Files: **104** after the six conversion exclusions
- Temporary workspace after completion: **0 bytes** (`du -sh`)

Final build (including the shared input guard) again passed with 0 warnings and
0 errors. The complete regression runner plus real-image comparison was run:

```sh
PS5PKGTOOL_TEST_PKG=/path/to/game.pkg PS5PKGTOOL_TEST_EXFAT=/path/to/game.exfat dotnet run --project PS5PKGTool.Tests --no-build --no-restore
```

Result: **passed, exit 0**. All 104 expected paths and sizes match. Beginning,
middle and end content samples match for:

| File | Size |
| --- | ---: |
| `eboot.bin` | 142,794,274 bytes |
| `sce_sys/param.json` | 2,413 bytes |
| Largest payload file | 4,565,111,890 bytes (above 4 GiB) |

The real package used the file-backed reader and the backend-load assertion
also passed: Oodle.NET and LibProsperoPkg stayed unloaded. No Windows-native
library was required by this conversion.

The large-package checks compare all paths/sizes and selected content ranges;
they do not compare full-file source hashes for every real asset. Generated
fixtures do use full hashes. No macOS system mount, Windows-output comparison,
or PS5 hardware test was performed. The existing extraction service recreates
parent directories from file paths and does not preserve empty PFS directories.

The [Avalonia frontend](MACOS-GUI.md) now calls this same verified conversion
service and publishes an `osx-arm64` development `.app`. Direct PKG-stream →
exFAT remains a future optimization; the current implementation requires
staging space for the extracted tree alongside the image.
