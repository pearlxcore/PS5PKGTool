using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;

internal static class NativeFfpfscSmoke
{
    public static async Task RunAsync()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PS5PKGTool.FfpfscSmoke." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string sourcePath = Path.Combine(directory, "PPSA00001.exfat");
        string imagePath = Path.Combine(directory, "PPSA00001.exfat.ffpfsc");
        string extractedPath = Path.Combine(directory, "extracted.exfat");
        string dumpDirectory = Path.Combine(directory, "dump");
        string dumpImagePath = Path.Combine(directory, "dump.ffpfsc");
        string dumpExtractedPath = Path.Combine(directory, "dump.exfat");
        string extractedParamPath = Path.Combine(directory, "extracted-param.json");
        string wrapperDirectory = Path.Combine(directory, "download-wrapper");
        string wrappedGameDirectory = Path.Combine(wrapperDirectory, "release", "PPSA54321-app");
        string wrappedImagePath = Path.Combine(directory, "wrapped.ffpfsc");
        string ambiguousDirectory = Path.Combine(directory, "ambiguous-wrapper");
        string ambiguousImagePath = Path.Combine(directory, "ambiguous.ffpfsc");
        try
        {
            byte[] source = new byte[PfscCodec.LogicalBlockSize * 4 + 321];
            Array.Fill(source, (byte)'P', 0, PfscCodec.LogicalBlockSize * 2);
            new Random(0x46465046).NextBytes(source.AsSpan(PfscCodec.LogicalBlockSize * 2, PfscCodec.LogicalBlockSize));
            for (int index = PfscCodec.LogicalBlockSize * 3; index < source.Length; index++)
                source[index] = (byte)(index % 29);
            await File.WriteAllBytesAsync(sourcePath, source);

            FfpfscBuildResult build = await FfpfscImage.CreateFromImageAsync(sourcePath, imagePath);
            Require(build.InnerFileName == Path.GetFileName(sourcePath), "The inner image name changed.");
            Require(build.SourceLength == source.Length, "The FFPFSC source length is wrong.");
            Require(build.ContainerLength == new FileInfo(imagePath).Length, "The PFS block count is wrong.");

            await using (var image = File.OpenRead(imagePath))
            {
                FfpfscInfo info = FfpfscImage.Inspect(image);
                Require(info.InnerFileName == "PPSA00001.exfat", "The PFS directory payload is wrong.");
                Require(info.LogicalLength == source.Length, "The logical payload size is wrong.");
                Require(info.StoredLength == build.PfscStoredLength, "The PFSC inode size is wrong.");
            }

            FfpfscVerificationResult verification = await FfpfscImage.VerifyAsync(imagePath, sourcePath);
            Require(verification.StructureValid && verification.EveryPfscBlockDecodes,
                "The native FFPFSC image did not verify.");
            Require(verification.SourceMatches == true, "The native FFPFSC payload differs from its source.");

            await FfpfscImage.ExtractAsync(imagePath, extractedPath);
            Require((await File.ReadAllBytesAsync(extractedPath)).AsSpan().SequenceEqual(source),
                "Extracting the native FFPFSC image changed the source bytes.");

            Directory.CreateDirectory(Path.Combine(dumpDirectory, "sce_sys"));
            await File.WriteAllTextAsync(Path.Combine(dumpDirectory, "eboot.bin"), new string('B', 180_000));
            await File.WriteAllTextAsync(Path.Combine(dumpDirectory, "sce_sys", "param.json"),
                "{\"titleId\":\"PPSA00002\"}");
            await File.WriteAllBytesAsync(Path.Combine(dumpDirectory, "sce_sys", "icon0.png"),
                Enumerable.Range(0, 40_000).Select(index => (byte)(index % 239)).ToArray());
            FfpfscBuildResult dumpBuild = await FfpfscImage.CreateFromDirectoryAsync(dumpDirectory, dumpImagePath);
            Require(dumpBuild.InnerFileName == "PPSA00002.exfat", "The dump title ID was not used for the inner image.");
            FfpfscVerificationResult dumpVerification = await FfpfscImage.VerifyAsync(dumpImagePath);
            Require(dumpVerification.StructureValid && dumpVerification.EveryPfscBlockDecodes,
                "The streamed dump FFPFSC image did not verify.");
            using (FfpfscVolume volume = FfpfscVolume.Open(dumpImagePath))
            {
                Require(volume.Find("sce_sys/param.json") is { IsDirectory: false },
                    "The direct FFPFSC reader did not index param.json.");
                Require(volume.ReadAllText("sce_sys/param.json") == "{\"titleId\":\"PPSA00002\"}",
                    "The direct FFPFSC reader changed param.json.");
                using Stream eboot = volume.OpenFile("eboot.bin");
                Require(eboot.Length == 180_000 && eboot.ReadByte() == 'B',
                    "The direct FFPFSC reader returned incorrect eboot data.");
                eboot.Seek(-1, SeekOrigin.End);
                Require(eboot.ReadByte() == 'B', "Random access through PFSC/exFAT returned incorrect data.");
            }
            Ps5GameInfo ffpfscGame = new FfpfscGameReader().Read(dumpImagePath);
            Require(ffpfscGame.SourceKind == Ps5SourceKind.Ffpfsc && ffpfscGame.TitleId == "PPSA00002",
                "The library reader did not load FFPFSC param.json metadata.");
            Ps5GameDetails ffpfscDetails = await new Ps5DetailsLoader().LoadAsync(ffpfscGame);
            Require(ffpfscDetails.Files.FileCount == 3 && ffpfscDetails.Icon is { Bytes.Length: 40_000 },
                "The details loader did not read the FFPFSC inventory and artwork bytes.");
            GameFileChunk ffpfscChunk = GameFileSystem.ReadFileChunk(ffpfscGame, "eboot.bin", 65_530, 32);
            Require(ffpfscChunk.Offset == 65_530 && ffpfscChunk.FileSize == 180_000 &&
                    ffpfscChunk.Data.Length == 32 && ffpfscChunk.Data.All(value => value == 'B') &&
                    ffpfscChunk.HasPrevious && ffpfscChunk.HasNext,
                "Random-access FFPFSC file preview returned an incorrect chunk.");
            var looseGame = new Ps5GameInfo { SourceKind = Ps5SourceKind.LooseDump, RootPath = dumpDirectory };
            GameFileChunk looseChunk = GameFileSystem.ReadFileChunk(looseGame, "sce_sys/param.json", 2, 8);
            Require(Encoding.UTF8.GetString(looseChunk.Data) == "titleId\"" && looseChunk.HasPrevious &&
                    looseChunk.HasNext,
                "Random-access loose-dump file preview returned an incorrect chunk.");
            await GameFileSystem.ExtractFileAsync(ffpfscGame, "sce_sys/param.json", extractedParamPath);
            Require(await File.ReadAllTextAsync(extractedParamPath) == "{\"titleId\":\"PPSA00002\"}",
                "Single-file FFPFSC extraction changed the selected file.");
            await FfpfscImage.ExtractAsync(dumpImagePath, dumpExtractedPath);
            Require(new FileInfo(dumpExtractedPath).Length == dumpBuild.SourceLength,
                "The streamed inner exFAT length changed during extraction.");

            Directory.CreateDirectory(Path.Combine(wrappedGameDirectory, "sce_sys"));
            Directory.CreateDirectory(Path.Combine(wrappedGameDirectory, "fakelib"));
            await File.WriteAllTextAsync(Path.Combine(wrappedGameDirectory, "eboot.bin"), new string('W', 190_000));
            await File.WriteAllTextAsync(Path.Combine(wrappedGameDirectory, "sce_sys", "param.json"),
                "{\"titleId\":\"PPSA54321\"}");
            await File.WriteAllBytesAsync(Path.Combine(wrappedGameDirectory, "fakelib", "libSceAmpr.sprx"),
                [1, 2, 3, 4]);
            byte[] staleAmpr = "stale-index-must-not-be-reused"u8.ToArray();
            await File.WriteAllBytesAsync(Path.Combine(wrappedGameDirectory, "ampr_emu.index"), staleAmpr);
            await File.WriteAllTextAsync(Path.Combine(wrapperDirectory, "Note.txt"), new string('N', 220_000));

            FfpfscBuildResult wrappedBuild = await FfpfscImage.CreateFromDirectoryAsync(wrapperDirectory,
                wrappedImagePath);
            Require(wrappedBuild.InnerFileName == "PPSA54321.exfat",
                "A nested game root was not resolved from sce_sys/param.json.");
            using (ExfatImageSource expectedWrappedImage = ExfatImage.OpenDirectory(wrappedGameDirectory))
                Require(wrappedBuild.SourceLength == expectedWrappedImage.Length,
                    "Wrapper directories or download notes leaked into the inner exFAT image.");
            Require((await File.ReadAllBytesAsync(Path.Combine(wrappedGameDirectory, "ampr_emu.index")))
                    .AsSpan().SequenceEqual(staleAmpr),
                "Building a wrapped dump modified its source AMPR index.");

            foreach (string name in new[] { "PPSA10001-app", "PPSA10002-app" })
            {
                string gameDirectory = Path.Combine(ambiguousDirectory, name);
                Directory.CreateDirectory(Path.Combine(gameDirectory, "sce_sys"));
                await File.WriteAllTextAsync(Path.Combine(gameDirectory, "sce_sys", "param.json"),
                    $"{{\"titleId\":\"{name[..9]}\"}}");
            }
            await RequireThrowsAsync<InvalidDataException>(
                () => FfpfscImage.CreateFromDirectoryAsync(ambiguousDirectory, ambiguousImagePath),
                "A wrapper containing multiple PS5 game roots was accepted.");
            Require(!File.Exists(ambiguousImagePath), "An ambiguous conversion left an output image behind.");

            string? referenceDirectory = Environment.GetEnvironmentVariable("MKPFS_REFERENCE_DIR");
            if (!string.IsNullOrWhiteSpace(referenceDirectory))
            {
                await RunReferenceVerifyAsync(referenceDirectory, dumpImagePath, "pfs", null);
                await RunReferenceVerifyAsync(referenceDirectory, dumpExtractedPath, "exfat", dumpDirectory);

                foreach (int compressionLevel in new[] { 9 })
                {
                    string referenceImagePath = Path.Combine(directory,
                        $"PPSA00001.mkpfs.level{compressionLevel}.ffpfsc");
                    string parityImagePath = Path.Combine(directory,
                        $"PPSA00001.native.level{compressionLevel}.ffpfsc");
                    await RunReferencePackFileAsync(referenceDirectory, sourcePath, referenceImagePath,
                        compressionLevel);
                    long referenceTimestamp = ReadBuildTimestamp(referenceImagePath);
                    await FfpfscImage.CreateFromImageAsync(sourcePath, parityImagePath, new FfpfscBuildOptions
                    {
                        BuildTimestampUnixSeconds = referenceTimestamp,
                        Compression = new PfscCompressionOptions
                        {
                            CompressionLevel = compressionLevel,
                            MinimumGainPercent = 5
                        }
                    });
                    byte[] nativeHash;
                    byte[] referenceHash;
                    await using (FileStream nativeStream = File.OpenRead(parityImagePath))
                        nativeHash = await SHA256.HashDataAsync(nativeStream);
                    await using (FileStream referenceStream = File.OpenRead(referenceImagePath))
                        referenceHash = await SHA256.HashDataAsync(referenceStream);
                    Require(new FileInfo(parityImagePath).Length == new FileInfo(referenceImagePath).Length &&
                            nativeHash.AsSpan().SequenceEqual(referenceHash),
                        $"Native FFPFSC level {compressionLevel} bytes differ from MkPFS. " +
                        $"Native={Convert.ToHexString(nativeHash)}, MkPFS={Convert.ToHexString(referenceHash)}, " +
                        $"native length={new FileInfo(parityImagePath).Length}, " +
                        $"MkPFS length={new FileInfo(referenceImagePath).Length}, " +
                        "first difference=" + FindFirstDifference(parityImagePath, referenceImagePath));
                }

                foreach (int compressionLevel in new[] { 0, 9 })
                {
                    string referenceFolderImagePath = Path.Combine(directory,
                        $"PPSA54321.mkpfs-folder.level{compressionLevel}.ffpfsc");
                    string nativeFolderImagePath = Path.Combine(directory,
                        $"PPSA54321.native-folder.level{compressionLevel}.ffpfsc");
                    await RunReferencePackFolderAsync(referenceDirectory, wrappedGameDirectory,
                        referenceFolderImagePath, compressionLevel);
                    long referenceTimestamp = ReadBuildTimestamp(referenceFolderImagePath);
                    await FfpfscImage.CreateFromDirectoryAsync(wrapperDirectory, nativeFolderImagePath,
                        new FfpfscBuildOptions
                        {
                            BuildTimestampUnixSeconds = referenceTimestamp,
                            Compression = new PfscCompressionOptions
                            {
                                CompressionLevel = compressionLevel,
                                MinimumGainPercent = 5
                            }
                        });
                    byte[] nativeHash;
                    byte[] referenceHash;
                    await using (FileStream nativeStream = File.OpenRead(nativeFolderImagePath))
                        nativeHash = await SHA256.HashDataAsync(nativeStream);
                    await using (FileStream referenceStream = File.OpenRead(referenceFolderImagePath))
                        referenceHash = await SHA256.HashDataAsync(referenceStream);
                    Require(new FileInfo(nativeFolderImagePath).Length ==
                            new FileInfo(referenceFolderImagePath).Length &&
                            nativeHash.AsSpan().SequenceEqual(referenceHash),
                        $"Native selected-dump level {compressionLevel} bytes differ from MkPFS. " +
                        $"Native={Convert.ToHexString(nativeHash)}, MkPFS={Convert.ToHexString(referenceHash)}, " +
                        $"native length={new FileInfo(nativeFolderImagePath).Length}, " +
                        $"MkPFS length={new FileInfo(referenceFolderImagePath).Length}, " +
                        "first difference=" + FindFirstDifference(nativeFolderImagePath, referenceFolderImagePath));
                }
            }

            byte[] corrupt = await File.ReadAllBytesAsync(imagePath);
            corrupt[0] ^= 0xFF;
            using var corruptStream = new MemoryStream(corrupt, writable: false);
            RequireThrows<InvalidDataException>(() => FfpfscImage.Inspect(corruptStream),
                "The PFS reader accepted corrupt header magic.");
        }
        finally
        {
            try { Directory.Delete(directory, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void RequireThrows<T>(Action action, string message) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException(message);
    }

    private static async Task RequireThrowsAsync<T>(Func<Task> action, string message) where T : Exception
    {
        try { await action(); }
        catch (T) { return; }
        throw new InvalidOperationException(message);
    }

    private static async Task RunReferenceVerifyAsync(string workingDirectory, string imagePath, string format,
        string? sourceDirectory)
    {
        var start = new ProcessStartInfo
        {
            FileName = "python",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("-m");
        start.ArgumentList.Add("mkpfs");
        start.ArgumentList.Add("verify");
        start.ArgumentList.Add(imagePath);
        start.ArgumentList.Add("--format");
        start.ArgumentList.Add(format);
        if (sourceDirectory is not null)
        {
            start.ArgumentList.Add("--source-dir");
            start.ArgumentList.Add(sourceDirectory);
        }
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("Unable to start compatibility verifier.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Require(process.ExitCode == 0, $"Independent {format} compatibility verification failed:\n{stdout}{stderr}");
    }

    private static async Task RunReferencePackFileAsync(string workingDirectory, string sourcePath,
        string outputPath, int compressionLevel)
    {
        var start = new ProcessStartInfo
        {
            FileName = "python",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.Environment["PYTHONIOENCODING"] = "utf-8";
        foreach (string argument in new[]
                 {
                     "-m", "mkpfs", "pack", "file", sourcePath, outputPath,
                     "--no-adjust-output-file-extension", "--version", "PS5", "--inode-bits", "32",
                     "--case-insensitive", "--block-size", "65536", "--compression-level", compressionLevel.ToString(),
                     "--threshold-gain", "5", "--cpu-count", "1", "--no-rename-inner-image"
                 })
            start.ArgumentList.Add(argument);
        using Process process = Process.Start(start) ??
                                throw new InvalidOperationException("Unable to start the MkPFS file writer.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Require(process.ExitCode == 0, "Reference FFPFSC creation failed:\n" + stdout + stderr);
    }

    private static async Task RunReferencePackFolderAsync(string workingDirectory, string sourceDirectory,
        string outputPath, int compressionLevel)
    {
        var start = new ProcessStartInfo
        {
            FileName = "python",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.Environment["PYTHONIOENCODING"] = "utf-8";
        foreach (string argument in new[]
                 {
                     "-m", "mkpfs", "pack", "folder", sourceDirectory, outputPath,
                     "--no-adjust-output-file-extension", "--version", "PS5", "--inode-bits", "32",
                     "--case-insensitive", "--block-size", "65536", "--compression-level", compressionLevel.ToString(),
                     "--threshold-gain", "5", "--cpu-count", "1"
                 })
            start.ArgumentList.Add(argument);
        using Process process = Process.Start(start) ??
                                throw new InvalidOperationException("Unable to start the MkPFS folder writer.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Require(process.ExitCode == 0, "Reference folder FFPFSC creation failed:\n" + stdout + stderr);
    }

    private static long ReadBuildTimestamp(string imagePath)
    {
        using FileStream stream = File.OpenRead(imagePath);
        stream.Position = FfpfscBuildOptions.DefaultPfsBlockSize + 0x18;
        Span<byte> value = stackalloc byte[8];
        stream.ReadExactly(value);
        return BinaryPrimitives.ReadInt64LittleEndian(value);
    }

    private static string FindFirstDifference(string leftPath, string rightPath)
    {
        using FileStream left = File.OpenRead(leftPath);
        using FileStream right = File.OpenRead(rightPath);
        long commonLength = Math.Min(left.Length, right.Length);
        for (long offset = 0; offset < commonLength; offset++)
            if (left.ReadByte() != right.ReadByte()) return $"0x{offset:X}";
        return left.Length == right.Length ? "none" : $"0x{commonLength:X}";
    }
}
