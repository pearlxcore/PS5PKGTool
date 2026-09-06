using System.Diagnostics;
using System.Security.Cryptography;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;

internal static class NativeExfatSmoke
{
    public static async Task RunAsync()
    {
        Require(AmprIndex.HashPath("/app0/a.txt") == 0xD7A87E24EF2D648B,
            "The native AMPR FNV-1a hash is incompatible.");
        byte[] ampr = AmprIndex.Build([
            new AmprFileRecord("a.txt", 12, 100),
            new AmprFileRecord("sub/b.bin", 34, 200)
        ]);
        AmprIndexInfo amprInfo = AmprIndex.Inspect(ampr);
        Require(amprInfo.RecordCount == 2 && amprInfo.HashSlotCount == 4,
            "The native AMPRIDX3 layout is wrong.");

        string directory = Path.Combine(Path.GetTempPath(), "PS5PKGTool.ExfatSmoke." + Guid.NewGuid().ToString("N"));
        string sourceDirectory = Path.Combine(directory, "dump");
        string nestedDirectory = Path.Combine(sourceDirectory, "sce_sys", "nested");
        string imagePath = Path.Combine(directory, "PPSA00001.exfat");
        string disguisedFfpkgPath = Path.Combine(directory, "PPSA00001.ffpkg");
        string extractedPath = Path.Combine(directory, "extracted-eboot.bin");
        string extractedDirectory = Path.Combine(directory, "extracted-directory");
        string wrapperDirectory = Path.Combine(directory, "wrapper");
        string wrappedGameDirectory = Path.Combine(wrapperDirectory, "release", "PPSA12345-app");
        string wrappedImagePath = Path.Combine(directory, "PPSA12345.exfat");
        string amprImagePath = Path.Combine(directory, "PPSA00001.ampr.exfat");
        string missingAmprImagePath = Path.Combine(directory, "PPSA00001.missing-ampr.exfat");
        string cancelledMissingAmprImagePath = Path.Combine(directory, "PPSA00001.cancelled-missing-ampr.exfat");
        string resizedAmprImagePath = Path.Combine(directory, "PPSA00001.resized-ampr.exfat");
        string extendedRootImagePath = Path.Combine(directory, "PPSA00001.extended-root.exfat");
        string exactEditImagePath = Path.Combine(directory, "PPSA00001.exact-edit.exfat");
        string cancelledEditImagePath = Path.Combine(directory, "PPSA00001.cancelled-edit.exfat");
        string structuralEditImagePath = Path.Combine(directory, "PPSA00001.structural-edit.exfat");
        string repairImagePath = Path.Combine(directory, "PPSA00001.repair.exfat");
        Directory.CreateDirectory(nestedDirectory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "eboot.bin"), new string('E', 100_000));
            await File.WriteAllBytesAsync(Path.Combine(sourceDirectory, "sce_sys", "param.json"),
                "{\"titleId\":\"PPSA00001\"}"u8.ToArray());
            await File.WriteAllBytesAsync(Path.Combine(nestedDirectory, "café-ゲーム.bin"),
                Enumerable.Range(0, 70_123).Select(index => (byte)(index % 251)).ToArray());
            await File.WriteAllBytesAsync(Path.Combine(nestedDirectory, "empty.bin"), []);
            await File.WriteAllTextAsync(Path.Combine(sourceDirectory, "splash_sloclap.mp4"), "punctuation-order");
            Directory.CreateDirectory(Path.Combine(sourceDirectory, "splashscreen"));

            using (ExfatImageSource image = ExfatImage.OpenDirectory(sourceDirectory))
            {
                Require(image.Length >= 128 * 512, "The exFAT volume is shorter than its FAT offset.");
                Require(image.FileCount == 5 && image.DirectoryCount == 4, "The exFAT scan counts are wrong.");
                byte[] all = new byte[checked((int)image.Length)];
                await image.Stream.ReadExactlyAsync(all);
                Require(image.Stream.ReadByte() == -1, "The exFAT stream exceeded its planned length.");
                Require(all.AsSpan(3, 8).SequenceEqual("EXFAT   "u8), "The exFAT signature is missing.");
                Require(all.AsSpan(0, 12 * 512).SequenceEqual(all.AsSpan(12 * 512, 12 * 512)),
                    "The backup exFAT boot region differs from the main region.");
            }

            await ExfatImage.WriteDirectoryAsync(sourceDirectory, imagePath);
            Require(new FileInfo(imagePath).Length > 0, "The exFAT writer produced an empty image.");
            Ps5GameInfo exfatGame = new FilesystemImageGameReader().Read(imagePath);
            Require(exfatGame.SourceKind == Ps5SourceKind.FilesystemImage &&
                    exfatGame.TitleId == "PPSA00001" && exfatGame.VirtualRoot.Length == 0,
                "The raw exFAT library reader did not load param.json metadata.");
            File.Copy(imagePath, disguisedFfpkgPath);
            bool rejectedDisguisedFfpkg = false;
            try { _ = new FilesystemImageGameReader().Read(disguisedFfpkgPath); }
            catch (NotSupportedException) { rejectedDisguisedFfpkg = true; }
            Require(rejectedDisguisedFfpkg,
                "An exFAT image renamed to FFPKG was incorrectly accepted as a real UFS2 FFPKG.");
            Ps5GameDetails exfatDetails = await new Ps5DetailsLoader().LoadAsync(exfatGame);
            Require(exfatDetails.Files.FileCount == 5,
                "The raw exFAT details loader did not inventory the image.");
            GameFileChunk exfatChunk = GameFileSystem.ReadFileChunk(exfatGame, "eboot.bin", 65_530, 32);
            Require(exfatChunk.Data.Length == 32 && exfatChunk.Data.All(value => value == 'E') &&
                    exfatChunk.HasPrevious && exfatChunk.HasNext,
                "Random-access raw exFAT preview returned incorrect bytes.");
            await GameFileSystem.ExtractFileAsync(exfatGame, "eboot.bin", extractedPath);
            Require(new FileInfo(extractedPath).Length == 100_000 &&
                    (await File.ReadAllBytesAsync(extractedPath)).All(value => value == 'E'),
                "Extracting a file from raw exFAT changed its bytes.");
            ExfatVerificationResult imageVerification = await ExfatImage.VerifyAsync(imagePath);
            long expectedLogicalBytes = Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
            Require(imageVerification.FileCount == 5 && imageVerification.LogicalFileBytes == expectedLogicalBytes &&
                    imageVerification.ClusterSize is 32 * 1024 or 64 * 1024 &&
                    imageVerification.ManifestSha256.Length == 64,
                "Full exFAT verification returned incorrect filesystem statistics.");
            await ExfatImage.ExtractDirectoryAsync(imagePath, extractedDirectory);
            Require(File.ReadAllText(Path.Combine(extractedDirectory, "eboot.bin")) == new string('E', 100_000) &&
                    File.Exists(Path.Combine(extractedDirectory, "sce_sys", "nested", "empty.bin")),
                "Full exFAT extraction did not preserve the image tree.");

            string exactReplacement = Path.Combine(directory, "exact-replacement.bin");
            await File.WriteAllBytesAsync(exactReplacement, Enumerable.Repeat((byte)'R', 100_000).ToArray());
            File.Copy(imagePath, exactEditImagePath);
            ExfatEditResult exactEdit = await ExfatImageMaintenance.ApplyEditsAsync(exactEditImagePath,
                [ExfatEditOperation.Replace("eboot.bin", exactReplacement)]);
            Require(!exactEdit.Rebuilt && exactEdit.OperationCount == 1,
                "An exact-size exFAT replacement did not use the journaled in-place path.");
            using (var exactStream = File.OpenRead(exactEditImagePath))
            using (var exactVolume = new ExfatVolume(exactStream))
                Require(exactVolume.ReadAllBytes("eboot.bin").All(value => value == 'R'),
                    "The exact-size exFAT replacement has incorrect bytes.");

            File.Copy(imagePath, cancelledEditImagePath);
            using (var cancelEdit = new CancellationTokenSource())
            {
                var cancelProgress = new InlineProgress<FfpfscProgress>(value =>
                {
                    if (value.Stage == "Replacing exFAT file" && value.BytesProcessed == value.TotalBytes)
                        cancelEdit.Cancel();
                });
                await RequireThrowsAsync<OperationCanceledException>(() => ExfatImageMaintenance.ApplyEditsAsync(
                        cancelledEditImagePath, [ExfatEditOperation.Replace("eboot.bin", exactReplacement)],
                        cancelProgress, cancelEdit.Token),
                    "Cancelling exact-size replacement did not trigger rollback.");
            }
            using (var cancelledEditStream = File.OpenRead(cancelledEditImagePath))
            using (var cancelledEditVolume = new ExfatVolume(cancelledEditStream))
                Require(cancelledEditVolume.ReadAllBytes("eboot.bin").All(value => value == 'E'),
                    "Cancelled exact-size replacement did not restore the original bytes.");

            string resizedReplacement = Path.Combine(directory, "resized-replacement.bin");
            string addedFile = Path.Combine(directory, "added.txt");
            string addedTree = Path.Combine(directory, "added-tree");
            Directory.CreateDirectory(Path.Combine(addedTree, "empty"));
            Directory.CreateDirectory(Path.Combine(addedTree, "nested"));
            await File.WriteAllTextAsync(resizedReplacement, "resized eboot");
            await File.WriteAllTextAsync(addedFile, "added file");
            await File.WriteAllTextAsync(Path.Combine(addedTree, "nested", "tree.txt"), "tree file");
            File.Copy(imagePath, structuralEditImagePath);
            ExfatEditResult structuralEdit = await ExfatImageMaintenance.ApplyEditsAsync(structuralEditImagePath,
            [
                ExfatEditOperation.Replace("eboot.bin", resizedReplacement),
                ExfatEditOperation.Delete("splash_sloclap.mp4"),
                ExfatEditOperation.AddDirectory("new-empty"),
                ExfatEditOperation.AddFile("new-files/added.txt", addedFile),
                ExfatEditOperation.AddDirectoryTree("imported", addedTree)
            ]);
            Require(structuralEdit.Rebuilt && structuralEdit.OperationCount == 5,
                "Structural exFAT edits did not use the verified rebuild path.");
            using (var structuralStream = File.OpenRead(structuralEditImagePath))
            using (var structuralVolume = new ExfatVolume(structuralStream))
                Require(structuralVolume.ReadAllText("eboot.bin") == "resized eboot" &&
                        structuralVolume.Find("splash_sloclap.mp4") is null &&
                        structuralVolume.Find("new-empty") is { IsDirectory: true } &&
                        structuralVolume.ReadAllText("new-files/added.txt") == "added file" &&
                        structuralVolume.ReadAllText("imported/nested/tree.txt") == "tree file" &&
                        structuralVolume.Find("imported/empty") is { IsDirectory: true },
                    "The rebuilt exFAT image does not contain the requested structural edits.");

            File.Copy(imagePath, repairImagePath);
            await using (var damagedBoot = new FileStream(repairImagePath, FileMode.Open, FileAccess.Write,
                             FileShare.Read, 4096, FileOptions.RandomAccess))
            {
                damagedBoot.Position = 3;
                await damagedBoot.WriteAsync(new byte[] { 0 });
            }
            ExfatRepairResult repair = await ExfatImageMaintenance.RepairAsync(repairImagePath);
            Require(repair.BootRegionRecovered && repair.Verification.FileCount == 5 &&
                    repair.Verification.ManifestSha256 == imageVerification.ManifestSha256,
                "Recoverable boot-region damage was not repaired through a verified filesystem rebuild.");

            IReadOnlyList<string> locatedImages = new Ps5DumpLocator()
                .FindFilesystemImageFiles(directory, recursive: false);
            Require(locatedImages.Contains(imagePath, StringComparer.OrdinalIgnoreCase),
                "Library discovery did not find the raw exFAT image.");
            Require(!locatedImages.Contains(disguisedFfpkgPath, StringComparer.OrdinalIgnoreCase),
                "Library discovery incorrectly treated FFPKG as exFAT.");
            Ps5ScanResult imageScan = await new Ps5LibraryScanner().ScanAsync([imagePath], recursive: false);
            Require(imageScan.Errors.Count == 0 && imageScan.Games is [{ SourceKind: Ps5SourceKind.FilesystemImage }],
                "The library scanner did not load an explicitly selected exFAT image.");

            Directory.CreateDirectory(Path.Combine(wrappedGameDirectory, "sce_sys"));
            await File.WriteAllTextAsync(Path.Combine(wrappedGameDirectory, "sce_sys", "param.json"),
                "{\"titleId\":\"PPSA12345\"}");
            await File.WriteAllTextAsync(Path.Combine(wrappedGameDirectory, "eboot.bin"), "wrapped-root");
            await File.WriteAllTextAsync(Path.Combine(wrapperDirectory, "download-note.txt"), "outside-game-root");
            await ExfatImage.WriteDirectoryAsync(wrapperDirectory, wrappedImagePath);
            Ps5GameInfo wrappedGame = new FilesystemImageGameReader().Read(wrappedImagePath);
            Require(wrappedGame.VirtualRoot == "release/PPSA12345-app" && wrappedGame.TitleId == "PPSA12345",
                "A nested game root was not resolved inside raw exFAT.");
            using (IReadOnlyGameFileSystem wrappedFiles = GameFileSystem.Open(wrappedGame))
                Require(wrappedFiles.FileExists("eboot.bin") && !wrappedFiles.FileExists("download-note.txt"),
                    "Raw exFAT browsing did not isolate the selected game root.");

            string? referenceDirectory = Environment.GetEnvironmentVariable("MKPFS_REFERENCE_DIR");
            if (!string.IsNullOrWhiteSpace(referenceDirectory))
            {
                string referenceImagePath = Path.Combine(directory, "PPSA00001.mkpfs.exfat");
                await RunReferenceBuildAsync(referenceDirectory, sourceDirectory, referenceImagePath);
                byte[] nativeHash;
                byte[] referenceHash;
                await using (FileStream nativeStream = File.OpenRead(imagePath))
                    nativeHash = await SHA256.HashDataAsync(nativeStream);
                await using (FileStream referenceStream = File.OpenRead(referenceImagePath))
                    referenceHash = await SHA256.HashDataAsync(referenceStream);
                Require(new FileInfo(imagePath).Length == new FileInfo(referenceImagePath).Length &&
                        nativeHash.AsSpan().SequenceEqual(referenceHash),
                    $"Native exFAT bytes differ from MkPFS. Native={Convert.ToHexString(nativeHash)}, " +
                    $"MkPFS={Convert.ToHexString(referenceHash)}");

                var start = new ProcessStartInfo
                {
                    FileName = "python",
                    WorkingDirectory = referenceDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                start.ArgumentList.Add("-m");
                start.ArgumentList.Add("mkpfs");
                start.ArgumentList.Add("verify");
                start.ArgumentList.Add(imagePath);
                start.ArgumentList.Add("--source-dir");
                start.ArgumentList.Add(sourceDirectory);
                start.ArgumentList.Add("--format");
                start.ArgumentList.Add("exfat");
                using Process process = Process.Start(start) ?? throw new InvalidOperationException("Unable to start compatibility verifier.");
                string stdout = await process.StandardOutput.ReadToEndAsync();
                string stderr = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                Require(process.ExitCode == 0, "Independent exFAT compatibility verification failed:\n" + stdout + stderr);
            }

            Directory.CreateDirectory(Path.Combine(sourceDirectory, "fakelib"));
            await File.WriteAllBytesAsync(Path.Combine(sourceDirectory, "fakelib", "libSceAmpr.sprx"), [1, 2, 3]);
            using ExfatImageSource amprImage = ExfatImage.OpenDirectory(sourceDirectory);
            Require(amprImage.FileCount == 7, "The virtual AMPR index was not added to the exFAT plan.");
            await ExfatImage.WriteDirectoryAsync(sourceDirectory, amprImagePath);
            byte[] originalAmprIndex;
            using (var amprStream = File.OpenRead(amprImagePath))
            using (var amprVolume = new ExfatVolume(amprStream))
                originalAmprIndex = amprVolume.ReadAllBytes("ampr_emu.index");

            using (var cancelRefresh = new CancellationTokenSource())
            {
                var cancelProgress = new InlineProgress<FfpfscProgress>(value =>
                {
                    if (value.Stage == "Writing AMPR index" && value.BytesProcessed == value.TotalBytes)
                        cancelRefresh.Cancel();
                });
                await RequireThrowsAsync<OperationCanceledException>(() =>
                        ExfatAmprPatcher.RefreshAsync(amprImagePath, cancelProgress, cancelRefresh.Token),
                    "Cancelling after the AMPR write did not roll back the original index.");
            }
            using (var rolledBackStream = File.OpenRead(amprImagePath))
            using (var rolledBackVolume = new ExfatVolume(rolledBackStream))
                Require(rolledBackVolume.ReadAllBytes("ampr_emu.index").AsSpan().SequenceEqual(originalAmprIndex),
                    "A cancelled AMPR refresh did not restore the original index bytes.");

            ExfatAmprRefreshResult refreshedAmpr = await ExfatAmprPatcher.RefreshAsync(amprImagePath);
            Require(refreshedAmpr.Changed && refreshedAmpr.RecordCount == 6 && refreshedAmpr.IndexBytes > 0,
                "The managed exFAT AMPR index was not refreshed in place.");
            ExfatAmprRefreshResult currentAmpr = await ExfatAmprPatcher.RefreshAsync(amprImagePath);
            Require(!currentAmpr.Changed && currentAmpr.Sha256 == refreshedAmpr.Sha256,
                "Refreshing a current AMPR index changed it a second time.");
            await ExfatImage.WriteDirectoryAsync(sourceDirectory, missingAmprImagePath,
                new ExfatBuildOptions { GenerateAmprIndex = false });
            long missingOriginalLength = new FileInfo(missingAmprImagePath).Length;
            File.Copy(missingAmprImagePath, cancelledMissingAmprImagePath);
            using (var cancelCreate = new CancellationTokenSource())
            {
                var cancelProgress = new InlineProgress<FfpfscProgress>(value =>
                {
                    if (value.Stage == "Writing AMPR index" && value.BytesProcessed == value.TotalBytes)
                        cancelCreate.Cancel();
                });
                await RequireThrowsAsync<OperationCanceledException>(() =>
                        ExfatAmprPatcher.RefreshAsync(cancelledMissingAmprImagePath, cancelProgress,
                            cancelCreate.Token),
                    "Cancelling AMPR insertion did not stop the transactional exFAT update.");
            }
            Require(new FileInfo(cancelledMissingAmprImagePath).Length == missingOriginalLength,
                "Cancelled AMPR insertion did not restore the original exFAT image length.");
            using (var cancelledStream = File.OpenRead(cancelledMissingAmprImagePath))
            using (var cancelledVolume = new ExfatVolume(cancelledStream))
                Require(cancelledVolume.Find("ampr_emu.index") is null,
                    "Cancelled AMPR insertion left a root index entry behind.");

            ExfatAmprRefreshResult insertedAmpr = await ExfatAmprPatcher.RefreshAsync(missingAmprImagePath);
            Require(insertedAmpr.Created && insertedAmpr.Changed && insertedAmpr.ImageGrew &&
                    new FileInfo(missingAmprImagePath).Length > missingOriginalLength,
                "A missing AMPR index was not inserted using safe exFAT tail growth.");
            using (var insertedStream = File.OpenRead(missingAmprImagePath))
            using (var insertedVolume = new ExfatVolume(insertedStream))
                Require(AmprIndex.Inspect(insertedVolume.ReadAllBytes("ampr_emu.index")).RecordCount == 6,
                    "The inserted root AMPR index is invalid.");
            ExfatVerificationResult insertedVerification = await ExfatImage.VerifyAsync(missingAmprImagePath);
            Require(insertedVerification.FileCount == 7,
                "The grown exFAT image failed full filesystem verification after AMPR insertion.");

            await File.WriteAllBytesAsync(Path.Combine(sourceDirectory, "ampr_emu.index"), new byte[17]);
            await ExfatImage.WriteDirectoryAsync(sourceDirectory, resizedAmprImagePath,
                new ExfatBuildOptions { GenerateAmprIndex = false, ClusterSize = 4096 });
            File.Delete(Path.Combine(sourceDirectory, "ampr_emu.index"));
            ExfatAmprRefreshResult resizedAmpr = await ExfatAmprPatcher.RefreshAsync(resizedAmprImagePath);
            Require(!resizedAmpr.Created && resizedAmpr.Changed && resizedAmpr.IndexBytes != 17,
                "An existing differently sized AMPR index was not safely reallocated.");
            Require((await ExfatImage.VerifyAsync(resizedAmprImagePath)).FileCount == 7,
                "The exFAT image failed verification after AMPR index resizing.");

            for (int index = 0; index < 36; index++)
                await File.WriteAllTextAsync(Path.Combine(sourceDirectory, $"r{index:00}.bin"), index.ToString());
            await ExfatImage.WriteDirectoryAsync(sourceDirectory, extendedRootImagePath,
                new ExfatBuildOptions { GenerateAmprIndex = false, ClusterSize = 4096 });
            ExfatAmprRefreshResult extendedRootAmpr = await ExfatAmprPatcher.RefreshAsync(extendedRootImagePath);
            Require(extendedRootAmpr.Created && extendedRootAmpr.ImageGrew,
                "AMPR insertion did not extend a full exFAT root directory.");
            Require((await ExfatImage.VerifyAsync(extendedRootImagePath)).FileCount == 43,
                "The extended exFAT root directory failed full filesystem verification.");
            for (int index = 0; index < 36; index++)
                File.Delete(Path.Combine(sourceDirectory, $"r{index:00}.bin"));
            if (!string.IsNullOrWhiteSpace(referenceDirectory))
            {
                string nativeAmprImagePath = Path.Combine(directory, "PPSA00001.native-ampr.exfat");
                string referenceAmprImagePath = Path.Combine(directory, "PPSA00001.mkpfs-ampr.exfat");
                await ExfatImage.WriteDirectoryAsync(sourceDirectory, nativeAmprImagePath);
                await RunReferenceAmprAsync(referenceDirectory, sourceDirectory);
                await RunReferenceBuildAsync(referenceDirectory, sourceDirectory, referenceAmprImagePath);
                byte[] nativeAmprHash;
                byte[] referenceAmprHash;
                await using (FileStream nativeStream = File.OpenRead(nativeAmprImagePath))
                    nativeAmprHash = await SHA256.HashDataAsync(nativeStream);
                await using (FileStream referenceStream = File.OpenRead(referenceAmprImagePath))
                    referenceAmprHash = await SHA256.HashDataAsync(referenceStream);
                Require(new FileInfo(nativeAmprImagePath).Length == new FileInfo(referenceAmprImagePath).Length &&
                        nativeAmprHash.AsSpan().SequenceEqual(referenceAmprHash),
                    $"Native AMPR exFAT bytes differ from MkPFS. Native={Convert.ToHexString(nativeAmprHash)}, " +
                    $"MkPFS={Convert.ToHexString(referenceAmprHash)}");
            }
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

    private static async Task RequireThrowsAsync<T>(Func<Task> action, string message) where T : Exception
    {
        try { await action(); }
        catch (T) { return; }
        throw new InvalidOperationException(message);
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }

    private static async Task RunReferenceBuildAsync(string workingDirectory, string sourceDirectory,
        string outputPath)
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
        start.ArgumentList.Add("pack");
        start.ArgumentList.Add("exfat");
        start.ArgumentList.Add(sourceDirectory);
        start.ArgumentList.Add(outputPath);
        start.ArgumentList.Add("--no-progress");
        using Process process = Process.Start(start) ??
                                throw new InvalidOperationException("Unable to start the MkPFS exFAT writer.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Require(process.ExitCode == 0, "Reference exFAT creation failed:\n" + stdout + stderr);
    }

    private static async Task RunReferenceAmprAsync(string workingDirectory, string sourceDirectory)
    {
        var start = new ProcessStartInfo
        {
            FileName = "python",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(
            "import pathlib,sys; from mkpfs.ampr import ensure_ampr_index; " +
            "sys.exit(0 if ensure_ampr_index(pathlib.Path(sys.argv[1])) is not None else 1)");
        start.ArgumentList.Add(sourceDirectory);
        using Process process = Process.Start(start) ??
                                throw new InvalidOperationException("Unable to start the MkPFS AMPR writer.");
        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Require(process.ExitCode == 0, "Reference AMPR creation failed:\n" + stdout + stderr);
    }
}
