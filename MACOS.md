# macOS port — first milestone

This modified GPL-3.0 version of pearlxcore/PS5PKGTool adds a .NET 10 CLI.
Original attribution, licenses and third-party notices are retained.
Development branch: `macos-port`.

The subsequent [PKG conversion investigation and prototype](PKG-CONVERSION.md)
reuse the existing service; the results below record the original folder milestone.
The [Avalonia frontend and Apple Silicon publishing instructions](MACOS-GUI.md)
document the subsequent GUI milestone.

## Usage

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet run --project PS5PKGTool.Cli -- build-exfat /path/to/game-folder /path/to/game.exfat
dotnet run --project PS5PKGTool.Tests
```

The CLI references Core, Ffpfsc and Ufs2. It calls the existing
`ExfatImage.WriteDirectoryAsync`, then `ExfatImage.VerifyAsync`, with byte
progress on stderr and the result/manifest hash on stdout. Files are streamed
with the existing 1 MiB buffers. The first Ctrl+C requests cancellation; a second Ctrl+C terminates the process.
The writer uses a temporary sibling and publishes the completed image without
overwriting existing output. Cancellation during verification can leave the
completed image in place. Directory scanning is synchronous and does not poll
cancellation; writing and verification do.

Output must be outside the source directory. Existing backend behavior is
preserved: empty directories, Unicode names, fixed filesystem timestamps,
deterministic layout, metadata/symlink exclusions, and automatic AMPR index
generation when `fakelib/libSceAmpr.sprx` exists. AMPR behavior was not exercised
by this milestone's fixture.

The dependency-free test executable exits nonzero on failure; run it with
`dotnet run`, not `dotnet test`. It builds a fixture through the CLI and reads
it with `ExfatVolume`, checking paths, file sizes and streaming SHA-256 hashes.
It covers nested folders, an empty directory, a zero-byte file, Unicode names,
spaces in paths, a file larger than the streaming buffer, deterministic repeat
output, existing-output preservation, missing source, output inside source,
and cancellation requested before conversion starts.

For an independently created fixture without ignored metadata or an AMPR marker:

```sh
dotnet run --project PS5PKGTool.Tests -- /path/to/game-folder /path/to/game.exfat
```

## Commands and results on this Mac

Host: Darwin arm64 (Apple Silicon), SDK 10.0.401. Initially the SDK was
missing; installing .NET 10 resolved it. Commands below use `dotnet` on PATH
and portable placeholder paths.

| Command | Result |
| --- | --- |
| `dotnet build PS5PKGTool.Ffpfsc/PS5PKGTool.Ffpfsc.csproj` | Passed; 0 warnings, 0 errors |
| `dotnet build PS5PKGTool.Ufs2/PS5PKGTool.Ufs2.csproj` | Passed; 0 warnings, 0 errors |
| `dotnet build PS5PKGTool.Core/PS5PKGTool.Core.csproj --disable-build-servers -p:UseSharedCompilation=false` | Passed outside sandbox; 0 warnings, 0 errors |
| `dotnet build PS5PKGTool.Tests/PS5PKGTool.Tests.csproj --disable-build-servers -p:UseSharedCompilation=false` | Passed outside sandbox, including CLI and shared projects; 0 warnings, 0 errors |
| `dotnet run --project PS5PKGTool.Tests --no-build --no-restore` | Passed; exit 0 |
| `dotnet run --project PS5PKGTool.Cli --no-build --no-restore -- build-exfat /path/to/test-game /path/to/game.exfat` | Passed; exit 0; built and verified 3 files |
| `dotnet run --project PS5PKGTool.Tests --no-build --no-restore -- /path/to/test-game /path/to/game.exfat` | Passed; exit 0; paths, sizes and SHA-256 content match |

Sandboxed Core/CLI restore attempts stalled and exited 1 without compiler
diagnostics. Restore and builds outside the sandbox succeeded.
The independent fixture contains `eboot.bin` (256 bytes), `sce_sys/param.json`
(46 bytes), and `data/example.dat` (1,400,000 bytes). Its image is 1,736,704
bytes. Verified manifest SHA-256:
`1c26eabe4cb8f625a37b95da72c34f1f5ea16491c9c4c8bf1285ebb88c40f0e1`.

## Portability findings and boundaries

No source compile failures in categories A (paths), B (Windows APIs),
C (native binaries), D (project configuration), or E (unrelated compilation)
were found in the three shared libraries. The missing SDK and restricted
NuGet restore were environment issues. SDK startup also printed a macOS
`CSSM_ModuleLoad` certificate diagnostic in the sandbox; it did not prevent
the Ffpfsc/Ufs2 builds or conversion.

- `PS5PKGTool.Ufs2/DriveIO.cs` imports `kernel32.dll` for raw device operations;
  `Ufs2ImageCreator.CreateOnDevice` calls it. Folder → exFAT bypasses it.
- `PS5PKGTool.Core/Backends/LppBackend.cs` imports Windows `CreateHardLink`.
  This backend is outside the folder → exFAT path.
- Core references `Oodle.NET` 2.2.0 and the vendored `ProsperoPkgTool.dll`.
  ProsperoPkgTool's version notice identifies it as a managed net10.0 library;
  its `.dll` suffix alone does not make it a Windows native binary.
- The Windows application copies the native x86-64
  `ThirdParty/Oodle/oo2core_9_win64.dll`. The new CLI does not copy it, and
  conversion/read-back passed without it in the CLI output directory.
- `LibProsperoPkg12` loads vendored assemblies through an isolated
  `AssemblyLoadContext`; its payload includes a Windows Magick native DLL.
  Its package operations have not been validated on macOS.

No shared-library or Windows GUI source was changed. No Windows-output
comparison, macOS system mount, PS5 hardware validation, files above 4 GiB,
disk-exhaustion test or PKG conversion was performed. The verification here
proves read-back through the existing managed reader; it is not an independent
filesystem conformance check.

The subsequent [PKG investigation](PKG-CONVERSION.md) proves runtime conversion
with generated fixtures and a large real FPKG. The [GUI milestone](MACOS-GUI.md)
adds the Avalonia frontend, app publishing and CI. Those later results extend
the original folder-only checks recorded here.
