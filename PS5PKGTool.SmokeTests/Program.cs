using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.SmokeTests;

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
        SonyDebugPackageBuildResult result = await ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(args[1], args[2],
            new ProsperoDebugPackageBuildOptions { ContentId = args[3], Passcode = passcode }, progress);
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

if (args is ["--create-package-from-image", _, _, _] or ["--create-package-from-image", _, _, _, _])
{
    try
    {
        string passcode = args.Length == 5 ? args[4] : SonyDebugPackageCredentials.DefaultPasscode;
        var progress = new Progress<SonyDebugPackageProgress>(value =>
        {
            string amount = value.TotalBytes > 0
                ? $" {value.CompletedBytes:N0} / {value.TotalBytes:N0}" : string.Empty;
            Console.WriteLine(value.Stage + amount);
        });
        SonyDebugPackageBuildResult result = await VolumeDebugPackageBuilder.CreateFromImageAsync(args[1], args[2],
            new SonyDebugPackageBuildOptions { ContentId = args[3], Passcode = passcode }, progress);
        Console.WriteLine($"Created: {result.OutputPath}");
        Console.WriteLine($"Package bytes: {result.PackageSize:N0}");
        Console.WriteLine($"Source files: {result.SourceFiles:N0}");
        Console.WriteLine($"Source bytes: {result.SourceBytes:N0}");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 17;
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

if (args is ["--inspect-sony-pkg", _])
{
    try
    {
        var scanWatch = System.Diagnostics.Stopwatch.StartNew();
        var game = new SonyPkgGameReader().Read(args[1]);
        scanWatch.Stop();
        var package = game.Package!;
        Console.WriteLine($"Scan time: {scanWatch.ElapsedMilliseconds:N0} ms");
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
            foreach (GameFileRecord file in files.Files.Take(50))
            {
                using Stream input = files.OpenRead(file.RelativePath);
                byte[] head = new byte[checked((int)Math.Min(8, input.Length))];
                input.ReadExactly(head);
                Console.WriteLine($"READ {file.RelativePath}: {Convert.ToHexString(head)}");
            }
        }
        Ps5GameDetails details = await new Ps5DetailsLoader().LoadAsync(game);
        Console.WriteLine($"Details files: {details.Files.FileCount:N0}");
        Console.WriteLine($"Trophies: {details.TrophySet?.Trophies.Count ?? 0:N0}");
        Console.WriteLine($"Activities: {details.Uds?.EventCount ?? 0:N0}");
        Console.WriteLine($"Executable modules: {details.Executable?.Modules.Count ?? 0:N0}");
        Console.WriteLine($"Artwork: {new[] { details.Icon, details.Background, details.Background1, details.Background2 }.Count(value => value is { IsEmpty: false })}");
        foreach (string warning in details.Errors) Console.WriteLine("Details warning: " + warning);
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 9;
    }
}

if (args is ["--inspect-self", _])
{
    Ps5SelfInfo? self = new PS5PKGTool.Core.Parsers.Ps5SelfReader().Read(args[1]);
    if (self is null) { Console.WriteLine("No eboot.bin found."); return 1; }
    Console.WriteLine($"SELF magic: {self.SelfMagic}");
    Console.WriteLine($"File size: {self.FileSize}");
    Console.WriteLine($"ELF offset: 0x{self.ElfOffset:X}");
    Console.WriteLine($"ELF class: {self.ElfClass}, machine: 0x{self.Machine:X}, entry: 0x{self.EntryPoint:X}");
    Console.WriteLine($"Modules: {self.Modules.Count}");
    foreach (Ps5ModuleInfo module in self.Modules)
        Console.WriteLine($"  {module.Kind} {module.Name}");
    return 0;
}

if (args is ["--dump-uds", _])
{
    var game = new SonyPkgGameReader().Read(args[1]);
    using IReadOnlyGameFileSystem fs = GameFileSystem.Open(game);
    const string path = "sce_sys/uds/uds00.ucp";
    if (!fs.FileExists(path)) { Console.WriteLine("No UDS archive."); return 1; }
    var reader = new UcpReader();
    UcpArchive archive = reader.Read(path, () => fs.OpenRead(path));
    foreach (UcpEntry entry in archive.Entries)
    {
        Console.WriteLine($"=== {entry.Name} ({entry.Size}) ===");
        if (entry.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            Console.WriteLine(reader.ReadEntryText(archive, entry));
    }
    return 0;
}

if (args is ["--dump-trophy", _])
{
    var game = new SonyPkgGameReader().Read(args[1]);
    using IReadOnlyGameFileSystem fs = GameFileSystem.Open(game);
    const string path = "sce_sys/trophy2/trophy00.ucp";
    if (!fs.FileExists(path)) { Console.WriteLine("No trophy archive."); return 1; }
    var reader = new UcpReader();
    UcpArchive archive = reader.Read(path, () => fs.OpenRead(path));
    foreach (UcpEntry entry in archive.Entries)
        Console.WriteLine($"=== {entry.Name} ({entry.Size}) ===");
    return 0;
}

if (args is ["--dump-sfo", _])
{
    var game = new SonyPkgGameReader().Read(args[1]);
    byte[] sfo = GameFileSystem.ReadFileChunk(game, "sce_sys/param.sfo", 0, 4 * 1024 * 1024).Data;
    Console.WriteLine($"param.sfo bytes: {sfo.Length}");
    foreach (Ps5SfoEntry entry in Ps5SfoReader.Read(sfo))
        Console.WriteLine($"{entry.Key} [{entry.Format}] = {entry.Value}");
    return 0;
}

if (args is ["--dump-si", _])
{
    var game = new SonyPkgGameReader().Read(args[1]);
    SonyPkgSegment? segment = game.Package?.Segments.FirstOrDefault(s => s.Name.Equals("SI", StringComparison.OrdinalIgnoreCase));
    if (segment is null) { Console.WriteLine("No SI segment."); return 1; }
    Console.WriteLine($"SI segment 0x{segment.Offset:X} + {segment.Size:N0}");
    foreach (Ps5SiMember member in PS5PKGTool.Core.Services.Ps5SiReader.List(game.RootPath, segment.Offset, segment.Size))
        Console.WriteLine($"{member.Size,12:N0}  {member.Name}");
    return 0;
}

if (args is ["--dump-playgo", _])
{
    var playGoGame = new SonyPkgGameReader().Read(args[1]);
    Ps5GameDetails playGoDetails = await new Ps5DetailsLoader().LoadAsync(playGoGame);
    SonyPkgSegment? playGoSi = playGoGame.Package?.Segments.FirstOrDefault(
        s => s.Name.Equals("SI", StringComparison.OrdinalIgnoreCase));
    byte[]? plgx = playGoSi is null
        ? null
        : Ps5SiReader.ReadMember(playGoGame.RootPath, playGoSi.Offset, playGoSi.Size, "playgo-chunk.dat");
    Console.WriteLine($"playgo-chunk.dat: {plgx?.Length ?? 0} bytes");
    byte[] hashTable = GameFileSystem.ReadFileChunk(playGoGame, "sce_sys/playgo-hash-table.dat", 0, 16 * 1024 * 1024).Data;
    byte[] ficm = GameFileSystem.ReadFileChunk(playGoGame, "sce_sys/playgo-ficm.dat", 0, 16 * 1024 * 1024).Data;
    Console.WriteLine($"playgo-hash-table.dat: {hashTable.Length} bytes, playgo-ficm.dat: {ficm.Length} bytes");
    IReadOnlyDictionary<ulong, string> pathMap =
        Ps5PlayGoReader.BuildPathMap(playGoDetails.Files.Files.Select(file => file.RelativePath));
    Ps5PlayGoSummary playGo = Ps5PlayGoReader.Read(plgx, hashTable, ficm, pathMap);
    Console.WriteLine($"Content ID: {playGo.ContentId}  default scenario: {playGo.DefaultScenarioId}  flags 0x{playGo.HeaderFlags:X}");
    Console.WriteLine($"Chunks: {playGo.Chunks.Count}, Scenarios: {playGo.Scenarios.Count}, Files: {playGo.Files.Count}");
    foreach (Ps5PlayGoChunk chunk in playGo.Chunks.Take(20))
        Console.WriteLine($"  chunk {chunk.Id,4}  extents={chunk.ExtentCount,4}  bytes={chunk.TotalBytes,12:N0}  mask=0x{chunk.LanguageMask:X16}  {chunk.Label}");
    foreach (Ps5PlayGoScenario scenario in playGo.Scenarios)
        Console.WriteLine($"  scenario {scenario.Id,3}  initial={scenario.InitialChunkCount,4}  count={scenario.Chunks.Count,4}  {scenario.Label}");
    int resolved = playGo.Files.Count(file => !string.IsNullOrEmpty(file.Path));
    Console.WriteLine($"  file path hashes resolved: {resolved}/{playGo.Files.Count}");
    foreach (Ps5PlayGoFileChunk file in playGo.Files.Take(20))
        Console.WriteLine($"  0x{file.PathHash:X16} -> chunk {file.ChunkId}  {file.Path}");
    return 0;
}

if (args is ["--gen-playgo", _])
{
    PlayGoFixture fixture = PlayGoFixtureGenerator.Write(args[1], Ps5PlayGoReader.PathHash);
    Console.WriteLine($"Wrote PlayGo fixtures to {Path.GetFullPath(args[1])}");
    IReadOnlyDictionary<ulong, string> map = Ps5PlayGoReader.BuildPathMap(fixture.Paths);
    Ps5PlayGoSummary parsed = Ps5PlayGoReader.Read(fixture.ChunkDat, fixture.HashTable, fixture.Ficm, map);
    Console.WriteLine($"content_id={parsed.ContentId} flags=0x{parsed.HeaderFlags:X} chunks={parsed.Chunks.Count} scenarios={parsed.Scenarios.Count} files={parsed.Files.Count}");
    foreach (Ps5PlayGoChunk chunk in parsed.Chunks)
        Console.WriteLine($"  chunk {chunk.Id} label='{chunk.Label}' mask=0x{chunk.LanguageMask:X16} extents={chunk.ExtentCount} bytes={chunk.TotalBytes}");
    foreach (Ps5PlayGoScenario scenario in parsed.Scenarios)
        Console.WriteLine($"  scenario {scenario.Id} label='{scenario.Label}' initial={scenario.InitialChunkCount} sequence=[{string.Join(",", scenario.Chunks)}]");
    foreach (Ps5PlayGoFileChunk file in parsed.Files)
        Console.WriteLine($"  file 0x{file.PathHash:X16} -> chunk {file.ChunkId}  {file.Path}");
    return 0;
}

if (args is ["--verify-playgo-engine", _])
{
    string dll = Path.Combine(AppContext.BaseDirectory, "ProsperoPkgTool.dll");
    if (!File.Exists(dll))
        dll = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\PS5PKGTool\ThirdParty\ProsperoPkgTool\ProsperoPkgTool.dll"));
    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(dll);
    Type reader = assembly.GetType("ProsperoPkgTool.Gp5.PlayGoChunkReader")
        ?? throw new InvalidOperationException("PlayGoChunkReader type not found in the engine assembly.");
    System.Reflection.MethodInfo read = reader.GetMethod("Read", [typeof(string)])
        ?? throw new InvalidOperationException("PlayGoChunkReader.Read(string) not found.");
    object project = read.Invoke(null, [Path.Combine(args[1], "playgo-chunk.dat")])!;
    Type projectType = project.GetType();
    Console.WriteLine($"engine reader accepted playgo-chunk.dat: ContentId={projectType.GetProperty("ContentId")!.GetValue(project)}");
    foreach (object chunk in (System.Collections.IEnumerable)projectType.GetProperty("Chunks")!.GetValue(project)!)
    {
        Type chunkType = chunk.GetType();
        Console.WriteLine($"  chunk {chunkType.GetProperty("Id")!.GetValue(chunk)} label='{chunkType.GetProperty("Label")!.GetValue(chunk)}' mask=0x{(ulong)chunkType.GetProperty("LanguageMask")!.GetValue(chunk)!:X16}");
    }
    foreach (object scenario in (System.Collections.IEnumerable)projectType.GetProperty("Scenarios")!.GetValue(project)!)
    {
        Type scenarioType = scenario.GetType();
        Console.WriteLine($"  scenario {scenarioType.GetProperty("Id")!.GetValue(scenario)} label='{scenarioType.GetProperty("Label")!.GetValue(scenario)}' initial={scenarioType.GetProperty("InitialChunkCount")!.GetValue(scenario)}");
    }
    return 0;
}

if (args is ["--extract-ffpfsc", _, _])
{
    int count = await PS5PKGTool.Ffpfsc.FfpfscImage.ExtractToDirectoryAsync(args[1], args[2], overwrite: true);
    Console.WriteLine($"Extracted {count} files to {args[2]}");
    return 0;
}

if (args is ["--verify-ffpfsc", _])
{
    PS5PKGTool.Ffpfsc.FfpfscVerificationResult result =
        await PS5PKGTool.Ffpfsc.FfpfscImage.TryVerifyAsync(args[1]);
    Console.WriteLine($"StructureValid={result.StructureValid} EveryBlockDecodes={result.EveryPfscBlockDecodes} " +
                      $"Inner={result.Info?.InnerFileName} Error={result.Error}");
    return result.StructureValid && result.EveryPfscBlockDecodes ? 0 : 1;
}

if (args is ["--test-ufs2-wrap"])
{
    string testRoot = Path.Combine(Path.GetTempPath(), "ps5pkgtool-imgtest", Guid.NewGuid().ToString("N"));
    string dump = Path.Combine(testRoot, "dump");
    Directory.CreateDirectory(Path.Combine(dump, "sce_sys"));
    File.WriteAllText(Path.Combine(dump, "sce_sys", "param.json"), "{\"titleId\":\"PPSA00000\"}");
    File.WriteAllBytes(Path.Combine(dump, "payload.bin"), new byte[12345]);
    string ufs2 = Path.Combine(testRoot, "image.bin");
    await UFS2Tool.Ufs2Operations.CreateFromDirectoryAsync(dump, ufs2, "TEST");
    string wrap = Path.Combine(testRoot, "wrap.ffpfsc");
    await PS5PKGTool.Ffpfsc.FfpfscImage.CreateFromImageAsync(ufs2, wrap,
        new PS5PKGTool.Ffpfsc.FfpfscBuildOptions { InnerFileName = "image.bin", OverwriteExisting = true });
    using (PS5PKGTool.Ffpfsc.FfpfscVolume volume = PS5PKGTool.Ffpfsc.FfpfscVolume.Open(wrap))
        Console.WriteLine($"inner kind (signature): {volume.InnerFilesystemKind}");
    string outDir = Path.Combine(testRoot, "out");
    int extracted = await PS5PKGTool.Ffpfsc.FfpfscImage.ExtractToDirectoryAsync(wrap, outDir);
    bool payloadMatches = File.ReadAllBytes(Path.Combine(outDir, "payload.bin")).Length == 12345;
    Console.WriteLine($"extracted {extracted} files, payload ok={payloadMatches}");
    Directory.Delete(testRoot, true);
    return payloadMatches ? 0 : 1;
}

if (args is ["--test-conversions"])
{
    string testRoot = Path.Combine(Path.GetTempPath(), "ps5pkgtool-convert", Guid.NewGuid().ToString("N"));
    string dump = Path.Combine(testRoot, "dump");
    Directory.CreateDirectory(Path.Combine(dump, "sce_sys"));
    File.WriteAllText(Path.Combine(dump, "sce_sys", "param.json"), "{\"titleId\":\"PPSA00000\"}");
    byte[] payload = Enumerable.Range(0, 4096).Select(index => (byte)index).ToArray();
    File.WriteAllBytes(Path.Combine(dump, "payload.bin"), payload);
    try
    {
        const PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget exfatTarget = PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget.Exfat;
        const PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget ffpkgTarget = PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget.Ffpkg;
        const PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget ffpfscTarget = PS5PKGTool.Ffpfsc.Ps5ImageConversionTarget.Ffpfsc;
        string exfat = Path.Combine(testRoot, "game.exfat");
        await PS5PKGTool.Ffpfsc.Ps5ImageConversionService.ConvertAsync(dump, exfat, exfatTarget);
        byte[] exfatHead = File.ReadAllBytes(exfat);
        Console.WriteLine($"exfat size={exfatHead.Length} probe={PS5PKGTool.Ffpfsc.Ps5ImageFormatProbe.Detect(exfat)} " +
                          $"head={Convert.ToHexString(exfatHead.AsSpan(0, 16))}");
        string ffpkg = Path.Combine(testRoot, "game.ffpkg");
        await PS5PKGTool.Ffpfsc.Ps5ImageConversionService.ConvertAsync(exfat, ffpkg, ffpkgTarget);
        string exfat2 = Path.Combine(testRoot, "roundtrip.exfat");
        await PS5PKGTool.Ffpfsc.Ps5ImageConversionService.ConvertAsync(ffpkg, exfat2, exfatTarget);
        string ffpfsc = Path.Combine(testRoot, "wrap.ffpfsc");
        await PS5PKGTool.Ffpfsc.Ps5ImageConversionService.ConvertAsync(ffpkg, ffpfsc, ffpfscTarget);
        string ffpkg2 = Path.Combine(testRoot, "fromfsc.ffpkg");
        await PS5PKGTool.Ffpfsc.Ps5ImageConversionService.ConvertAsync(ffpfsc, ffpkg2, ffpkgTarget);

        var expect = new (string Path, PS5PKGTool.Ffpfsc.Ps5ImageFormat Format, string Label)[]
        {
            (exfat, PS5PKGTool.Ffpfsc.Ps5ImageFormat.Exfat, "dump->exfat"),
            (ffpkg, PS5PKGTool.Ffpfsc.Ps5ImageFormat.Ufs2, "exfat->ffpkg"),
            (exfat2, PS5PKGTool.Ffpfsc.Ps5ImageFormat.Exfat, "ffpkg->exfat"),
            (ffpfsc, PS5PKGTool.Ffpfsc.Ps5ImageFormat.Pfs, "ffpkg->ffpfsc"),
            (ffpkg2, PS5PKGTool.Ffpfsc.Ps5ImageFormat.Ufs2, "ffpfsc->ffpkg")
        };
        bool ok = true;
        foreach ((string path, PS5PKGTool.Ffpfsc.Ps5ImageFormat format, string label) in expect)
        {
            PS5PKGTool.Ffpfsc.Ps5ImageFormat detected = PS5PKGTool.Ffpfsc.Ps5ImageFormatProbe.Detect(path);
            bool matches = detected == format;
            ok &= matches;
            Console.WriteLine($"{label}: {detected} {(matches ? "OK" : "MISMATCH")}");
        }

        string verifyDir = Path.Combine(testRoot, "verify-tree");
        await PS5PKGTool.Ffpfsc.FfpfscImage.ExtractToDirectoryAsync(ffpfsc, verifyDir);
        bool payloadRoundTrip = File.ReadAllBytes(Path.Combine(verifyDir, "payload.bin")).SequenceEqual(payload);
        Console.WriteLine($"payload round-trip through ffpfsc->ffpkg->extract: {payloadRoundTrip}");
        return ok && payloadRoundTrip ? 0 : 1;
    }
    finally
    {
        if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
    }
}

if (args is ["--test-image-package"])
{
    try
    {
        await VolumePackageBuilderSmoke.RunAsync();
        Console.WriteLine("Image-to-package virtual-source checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 16;
    }
}

if (args is ["--gen-volume-fixtures", _])
{
    try
    {
        bool identical = await VolumeFixtureGenerator.RunAsync(args[1]);
        return identical ? 0 : 18;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 18;
    }
}

if (args is ["--test-package-image"])
{
    try
    {
        await VolumePackageBuilderSmoke.RunPackageToImageAsync();
        Console.WriteLine("Debug-package-to-image checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 19;
    }
}

if (args is ["--test-ffpkg-size"])
{
    string testRoot = Path.Combine(Path.GetTempPath(), "ps5pkgtool-ffpkg-size", Guid.NewGuid().ToString("N"));
    string dump = Path.Combine(testRoot, "dump");
    Directory.CreateDirectory(Path.Combine(dump, "sce_sys"));
    File.WriteAllText(Path.Combine(dump, "sce_sys", "param.json"), "{\"titleId\":\"PPSA00000\"}");
    var payload = new byte[4 * 1024 * 1024];
    Random.Shared.NextBytes(payload);
    File.WriteAllBytes(Path.Combine(dump, "payload.bin"), payload);
    File.WriteAllBytes(Path.Combine(dump, "small.bin"), new byte[4096]);
    try
    {
        long dumpBytes = Directory.EnumerateFiles(dump, "*", SearchOption.AllDirectories).Sum(path => new FileInfo(path).Length);
        string ffpkg = Path.Combine(testRoot, "sized.ffpkg");
        await UFS2Tool.Ufs2Operations.CreateFromDirectoryAsync(dump, ffpkg, "TEST");
        long ffpkgBytes = new FileInfo(ffpkg).Length;
        Console.WriteLine($"dump={dumpBytes:N0}  ffpkg={ffpkgBytes:N0}  overhead={ffpkgBytes - dumpBytes:N0}  ratio={ffpkgBytes / (double)dumpBytes:0.000}");
        return 0;
    }
    finally
    {
        if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
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
        Console.WriteLine($"Artwork: {new[] { details.Icon, details.Background, details.Background1, details.Background2 }.Count(value => value is { IsEmpty: false })}");
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

if (args is ["--test-task-queue"])
{
    try
    {
        await PackageTaskQueueSmoke.RunAsync();
        Console.WriteLine("Task queue checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 14;
    }
}

if (args is ["--test-assets"])
{
    try
    {
        AssetInspectorSmoke.Run();
        Console.WriteLine("Asset inspector checks passed.");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex);
        return 15;
    }
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
    await SonyDebugPackageBuilderSmoke.RunAsync();
    Console.WriteLine("Native Sony debug package builder checks passed.");
    await VolumePackageBuilderSmoke.RunAsync();
    Console.WriteLine("Image-to-package virtual-source checks passed.");
    await VolumePackageBuilderSmoke.RunPackageToImageAsync();
    Console.WriteLine("Debug-package-to-image checks passed.");
    Ps5ToolingSmoke.Run();
    Console.WriteLine("Native PS5 tooling checks passed.");
    await PackageTaskQueueSmoke.RunAsync();
    Console.WriteLine("Task queue checks passed.");
    AssetInspectorSmoke.Run();
    Console.WriteLine("Asset inspector checks passed.");
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
    int backgroundCount = new[] { details.Background, details.Background1, details.Background2 }
        .Count(background => background is { IsEmpty: false });
    // Sony packages whose inner PFS is not plaintext-indexed (retail / key-required / metadata-less)
    // cannot be fully validated on PC, so they are reported but not counted as failures.
    bool verifiable = game.Package is null ||
                      game.Package.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed;
    // Compressed containers (FFPFSC/PFSC and UFS2 FFPKG) expose their compressed container size as
    // SourceSize, which legitimately differs from the logical content total.
    bool sizeConsistent = game.SourceKind is Ps5SourceKind.Ffpfsc or Ps5SourceKind.Ffpkg ||
                          game.SourceSize == details.Files.TotalSize;
    bool valid = !verifiable || (details.TrophySet is not null && details.TrophySet.IntegrityValid &&
                 details.Uds is not null && details.Uds.IntegrityValid &&
                 details.Executable is not null && details.Files.FileCount > 0 &&
                 details.Files.Files.Count == details.Files.FileCount && backgroundCount == 3 &&
                 sizeConsistent);
    Console.WriteLine($"{game.TitleId,-12} {game.Title,-54} " +
                      $"trophies={details.TrophySet?.Trophies.Count ?? 0,-3} " +
                      $"events={details.Uds?.EventCount ?? 0,-3} files={details.Files.FileCount,-5} art={backgroundCount} " +
                      $"modules={details.Executable?.Modules.Count ?? 0,-3} " +
                      $"valid={(verifiable ? valid.ToString() : "n/a")}");
    foreach (string error in details.Errors) Console.Error.WriteLine($"DETAIL WARNING [{game.TitleId}]: {error}");
    if (verifiable && !valid) failures++;
}

if (failures > 0)
{
    Console.Error.WriteLine($"{failures} dump(s) failed validation.");
    return 4;
}

Console.WriteLine("All discovered dumps passed parser validation.");
return 0;
