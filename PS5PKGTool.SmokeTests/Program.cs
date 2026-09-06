using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Builders;

if (args is ["--create-debug-pkg", _, _, _] or ["--create-debug-pkg", _, _, _, _])
{
    try
    {
        string passcode = args.Length == 5 ? args[4] : SonyDebugPackageCredentials.DefaultPasscode;
        var progress = new Progress<SonyDebugPackageProgress>(value =>
        {
            string amount = value.TotalBytes > 0
                ? $" {value.CompletedBytes:N0} / {value.TotalBytes:N0} bytes" : string.Empty;
            Console.WriteLine(value.Stage + amount + (value.CurrentPath.Length > 0 ? "  " + value.CurrentPath : string.Empty));
        });
        SonyDebugPackageBuildResult result = await SonyDebugPackageBuilder.CreateFromDirectoryAsync(args[1], args[2],
            new SonyDebugPackageBuildOptions { ContentId = args[3], Passcode = passcode }, progress);
        Console.WriteLine($"Created: {result.OutputPath}");
        Console.WriteLine($"Package bytes: {result.PackageSize:N0}");
        Console.WriteLine($"Source files: {result.SourceFiles:N0}");
        Console.WriteLine($"Key fingerprint: {result.KeyFingerprint}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 12;
    }
}

if (args is ["--verify-debug-pkg", _] or ["--verify-debug-pkg", _, _])
{
    string passcode = args.Length == 3 ? args[2] : SonyDebugPackageCredentials.DefaultPasscode;
    SonyDebugPackageValidationResult result = SonyDebugPackageBuilder.Validate(args[1], passcode);
    Console.WriteLine(result.Message);
    Console.WriteLine($"Indexed files: {result.IndexedFiles:N0}");
    Console.WriteLine($"Package bytes: {result.PackageSize:N0}");
    return result.IsValid ? 0 : 13;
}

if (args is ["--test-sony-pfs"])
{
    try
    {
        await SonyPfsBuilderSmoke.RunAsync();
        Console.WriteLine("Native Sony PFS builder checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 10;
    }
}

if (args is ["--test-debug-pkg"])
{
    try
    {
        await SonyDebugPackageBuilderSmoke.RunAsync();
        Console.WriteLine("Native Sony debug package builder checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 11;
    }
}

if (args is ["--dump-naps", _])
{
    var package = new PS5PKGTool.Core.Parsers.SonyPkgReader().Read(args[1]);
    var outer = new PS5PKGTool.Core.Parsers.SonyDebugPfsReader().TryIndex(args[1],
        checked((long)package.PfsImageOffset), checked((long)package.PfsImageSize),
        checked((long)package.PfsSuperblockOffset), package.ContentId,
        PS5PKGTool.Core.Builders.SonyDebugPackageCredentials.DefaultPasscode)
        ?? throw new InvalidDataException("No indexed debug outer PFS.");
    if (outer.AccessState != PS5PKGTool.Core.Models.SonyPfsAccessState.PlaintextIndexed)
        throw new InvalidDataException(outer.StatusMessage);
    var entry = outer.Files.Single(file => Path.GetFileName(file.RelativePath)
        .Equals("naps_pkg_layout.dat", StringComparison.OrdinalIgnoreCase));
    byte[] bytes = PS5PKGTool.Core.Parsers.SonyPfsEntryDataReader.ReadAll(args[1], package, outer, entry, 16 * 1024 * 1024);
    var layout = PS5PKGTool.Core.Parsers.SonyNapsLayoutReader.Read(bytes);
    Console.WriteLine($"Bytes: {bytes.Length:N0}");
    Console.WriteLine("Boundaries: " + string.Join(", ", layout.FileBoundaries.Select(value => $"0x{value:X}")));
    for (int index = 0; index < layout.Blocks.Length; index++)
    {
        var item = layout.Blocks[index];
        Console.WriteLine(item.IsRun
            ? $"{index,3}: RUN disk={item.RunDiskBlock} tweak={item.RunTweakIndex} comp={item.CompressedOffset}"
            : $"{index,3}: DATA comp={item.CompressedOffset} uncomp={item.UncompressedOffset} even={item.EvenLengthMinusOne} pred={item.Predictor}");
    }
    return 0;
}

if (args is ["--inspect-sony-pkg", _])
{
    try
    {
        var game = new SonyPkgGameReader().Read(args[1]);
        var package = game.Package!;
        Console.WriteLine($"Type: {package.KindDisplayName}");
        Console.WriteLine($"Title: {game.Title}");
        Console.WriteLine($"Title ID: {game.TitleId}");
        Console.WriteLine($"Content ID: {game.ContentId}");
        Console.WriteLine($"CNT entries: {package.Entries.Count:N0}");
        Console.WriteLine($"PFS offset: 0x{package.PfsImageOffset:X}");
        Console.WriteLine($"PFS size: {package.PfsImageSize:N0}");
        Console.WriteLine($"PFS access: {package.NestedPfs?.AccessState.ToString() ?? "NotPresent"}");
        Console.WriteLine($"PFS status: {package.NestedPfs?.StatusMessage ?? "No nested PFS image."}");
        Console.WriteLine($"PFS files: {package.NestedPfs?.Files.Count ?? 0:N0}");
        foreach (var file in package.NestedPfs?.Files.Take(50) ?? [])
            Console.WriteLine($"{file.Size,14:N0}  {file.RelativePath}");
        using (IReadOnlyGameFileSystem files = GameFileSystem.Open(game))
        {
            Console.WriteLine($"Directly readable files: {files.Files.Count:N0}");
            foreach (GameFileRecord file in files.Files.Take(10))
            {
                using Stream input = files.OpenRead(file.RelativePath);
                byte[] head = new byte[checked((int)Math.Min(8, input.Length))];
                input.ReadExactly(head);
                Console.WriteLine($"READ {file.RelativePath}: {Convert.ToHexString(head)}");
            }
        }
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 9;
    }
}

if (args is ["--inspect-ffpfsc", _])
{
    try
    {
        var game = new FfpfscGameReader().Read(args[1]);
        var details = await new Ps5DetailsLoader().LoadAsync(game);
        Console.WriteLine($"Title: {game.Title}");
        Console.WriteLine($"Title ID: {game.TitleId}");
        Console.WriteLine($"Inner image: {game.ContainerInnerFileName}");
        Console.WriteLine($"Container bytes: {game.SourceSize:N0}");
        Console.WriteLine($"Logical exFAT bytes: {game.ContainerLogicalSize:N0}");
        Console.WriteLine($"PFSC blocks: {game.ContainerBlockCount:N0}");
        Console.WriteLine($"Files: {details.Files.FileCount:N0}");
        Console.WriteLine($"Trophies: {details.TrophySet?.Trophies.Count ?? 0:N0}");
        Console.WriteLine($"Activities: {details.Uds?.EventCount ?? 0:N0}");
        Console.WriteLine($"Modules: {details.Executable?.Modules.Count ?? 0:N0}");
        Console.WriteLine($"Artwork: {new[] { details.IconPng, details.BackgroundPng, details.Background1Png, details.Background2Png }.Count(value => value is { Length: > 0 })}");
        foreach (string warning in details.Errors) Console.WriteLine("Warning: " + warning);
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 8;
    }
}

if (args is ["--compare-ffpfsc", _, _])
{
    FfpfscFileComparison comparison = await FfpfscFileComparer.CompareAsync(args[1], args[2]);
    Console.WriteLine($"Same length: {comparison.SameLength}");
    Console.WriteLine($"Length: {comparison.Length:N0} bytes");
    Console.WriteLine($"Timestamp bytes that differ: {comparison.TimestampDifferenceBytes:N0}");
    Console.WriteLine($"Equal except PFS build timestamps: {comparison.EqualExceptBuildTimestamps}");
    if (comparison.FirstUnexpectedDifference is long offset)
        Console.WriteLine($"First unexpected difference: 0x{offset:X}");
    return comparison.EqualExceptBuildTimestamps ? 0 : 7;
}

try
{
    await NativeExfatSmoke.RunAsync();
    Console.WriteLine("Native streaming exFAT checks passed.");

    await NativeUfs2Smoke.RunAsync();
    Console.WriteLine("Native UFS2/FFPKG checks passed.");

    await NativePfscSmoke.RunAsync();
    Console.WriteLine("Native PFSC codec checks passed.");

    await NativeFfpfscSmoke.RunAsync();
    Console.WriteLine("Native FFPFSC image checks passed.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Native PFSC validation failed: " + ex);
    return 6;
}

try
{
    SonyPkgReaderSmoke.Run();
    Console.WriteLine("Synthetic Sony CNT/FIH parser checks passed.");
    await SonyPfsBuilderSmoke.RunAsync();
    Console.WriteLine("Native Sony PFS builder checks passed.");
    await SonyDebugPackageBuilderSmoke.RunAsync();
    Console.WriteLine("Native Sony debug package builder checks passed.");
    Ps5ToolingSmoke.Run();
    Console.WriteLine("Native PS5 tooling checks passed.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Sony PKG parser validation failed: " + ex);
    return 5;
}

if (args is ["--native-only"]) return 0;

string root = args.Length > 0 ? args[0] : @"H:\PS5\PS5 Dumps";
if (!Directory.Exists(root))
{
    Console.Error.WriteLine($"Smoke-test directory does not exist: {root}");
    return 2;
}

var scanner = new Ps5LibraryScanner();
Ps5ScanResult scan = await scanner.ScanAsync([root], recursive: true);
Console.WriteLine($"Discovered {scan.Games.Count} PS5 game source(s).");
foreach (string error in scan.Errors) Console.Error.WriteLine("SCAN WARNING: " + error);
if (scan.Games.Count == 0) return 3;

var loader = new Ps5DetailsLoader();
int failures = 0;
foreach (var game in scan.Games)
{
    var details = await loader.LoadAsync(game);
    int backgroundCount = new[] { details.BackgroundPng, details.Background1Png, details.Background2Png }
        .Count(background => background is { Length: > 0 });
    bool valid = details.TrophySet is not null && details.TrophySet.IntegrityValid &&
                 details.Uds is not null && details.Uds.IntegrityValid &&
                 details.Executable is not null && details.Files.FileCount > 0 &&
                 details.Files.Files.Count == details.Files.FileCount && backgroundCount == 3 &&
                 game.SourceSize == details.Files.TotalSize;
    Console.WriteLine($"{game.TitleId,-12} {game.Title,-54} " +
                      $"trophies={details.TrophySet?.Trophies.Count ?? 0,-3} " +
                      $"events={details.Uds?.EventCount ?? 0,-3} files={details.Files.FileCount,-5} art={backgroundCount} " +
                      $"modules={details.Executable?.Modules.Count ?? 0,-3} valid={valid}");
    foreach (string error in details.Errors) Console.Error.WriteLine($"DETAIL WARNING [{game.TitleId}]: {error}");
    if (!valid) failures++;
}

if (failures > 0)
{
    Console.Error.WriteLine($"{failures} dump(s) failed validation.");
    return 4;
}

Console.WriteLine("All discovered dumps passed parser validation.");
return 0;
