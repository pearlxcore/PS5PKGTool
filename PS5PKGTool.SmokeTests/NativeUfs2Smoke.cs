using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;
using UFS2Tool;

internal static class NativeUfs2Smoke
{
    public static async Task RunAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool.Ufs2Smoke." + Guid.NewGuid().ToString("N"));
        string source = Path.Combine(root, "dump");
        string imagePath = Path.Combine(root, "PPSA99999.ffpkg");
        string wrappedPath = Path.Combine(root, "PPSA99999.ffpfsc");
        string extracted = Path.Combine(root, "extracted");
        Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(source, "nested", "empty"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(source, "sce_sys", "param.json"),
                "{\"titleId\":\"PPSA99999\",\"localizedParameters\":{\"defaultLanguage\":\"en-US\",\"en-US\":{\"titleName\":\"UFS2 Smoke\"}}}");
            byte[] large = Enumerable.Range(0, 900_000).Select(index => (byte)(index % 251)).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(source, "nested", "large.bin"), large);
            await File.WriteAllTextAsync(Path.Combine(source, "eboot.bin"), "synthetic eboot");

            var creationProgress = new List<Ufs2Progress>();
            Ufs2VerificationResult created = await Ufs2Operations.CreateFromDirectoryAsync(source, imagePath,
                "PPSA99999", new InlineProgress<Ufs2Progress>(creationProgress.Add));
            Require(created.Format == "UFS2" && created.FileCount == 3 && created.DirectoryCount == 4 &&
                    created.FsckErrors == 0,
                "The PS5 UFS2 creator did not produce a valid FFPKG image.");
            Require(creationProgress.Any(item => item.Stage.StartsWith("Scanning source dump", StringComparison.Ordinal)) &&
                    creationProgress.Any(item => item.Stage == "Writing UFS2 metadata" && item.TotalBytes > 0) &&
                    creationProgress.Any(item => item.Stage == "Copying files into FFPKG" && item.TotalBytes > 0) &&
                    creationProgress.Any(item => item.Stage == "Verifying FFPKG" && item.TotalBytes > 0) &&
                    creationProgress.Any(item => item.Stage == "Comparing source with FFPKG" && item.TotalBytes > 0) &&
                    creationProgress.Any(item => item.Stage == "Source and FFPKG match"),
                "FFPKG creation did not report all long-running stages.");
            using (var volume = new Ufs2Volume(imagePath))
            {
                Require(volume.Find("sce_sys/param.json") is { IsDirectory: false } &&
                        volume.Find("nested/empty") is { IsDirectory: true },
                    "The UFS2 directory inventory is incomplete.");
                using Stream stream = volume.OpenFile("nested/large.bin");
                stream.Position = 12 * 32768L - 17;
                byte[] sample = new byte[100_000];
                stream.ReadExactly(sample);
                Require(sample.AsSpan().SequenceEqual(large.AsSpan(checked((int)stream.Position - sample.Length),
                        sample.Length)),
                    "Random UFS2 reading across direct and indirect blocks returned incorrect bytes.");
            }

            Ufs2VerificationResult verified = await Ufs2Operations.VerifyAsync(imagePath);
            Require(verified.ManifestSha256 == created.ManifestSha256 && verified.FsckErrors == 0,
                "The created FFPKG failed repeat verification.");

            Ps5GameInfo game = new FfpkgGameReader().Read(imagePath);
            Require(game.SourceKind == Ps5SourceKind.Ffpkg && game.TitleId == "PPSA99999" &&
                    game.Title == "UFS2 Smoke" && game.SourceSize == new FileInfo(imagePath).Length,
                "The Core FFPKG metadata reader did not identify the synthetic game.");
            GameFileChunk chunk = GameFileSystem.ReadFileChunk(game, "nested/large.bin", 393_199, 65_777);
            Require(chunk.Data.AsSpan().SequenceEqual(large.AsSpan(393_199, 65_777)),
                "Core random-access FFPKG file reading returned incorrect data.");
            Ps5GameDetails details = await new Ps5DetailsLoader().LoadAsync(game);
            Require(details.Files.FileCount == 3 && details.Files.Files.Any(file =>
                    file.RelativePath.Replace('\\', '/').Equals("sce_sys/param.json", StringComparison.OrdinalIgnoreCase)),
                "The details loader did not inventory the FFPKG virtual filesystem.");

            _ = await FfpfscImage.CreateFromImageAsync(imagePath, wrappedPath,
                new FfpfscBuildOptions { Compression = new PfscCompressionOptions { CompressionLevel = 1 } });
            using (var wrapped = FfpfscVolume.Open(wrappedPath))
            {
                Require(wrapped.InnerFilesystemKind == FfpfscInnerFilesystemKind.Ufs2 &&
                        wrapped.Find("sce_sys/param.json") is { IsDirectory: false },
                    "FFPFSC did not mount its inner FFPKG as UFS2.");
                using Stream wrappedLarge = wrapped.OpenFile("nested/large.bin");
                wrappedLarge.Position = 410_003;
                byte[] wrappedSample = new byte[70_001];
                wrappedLarge.ReadExactly(wrappedSample);
                Require(wrappedSample.AsSpan().SequenceEqual(large.AsSpan(410_003, wrappedSample.Length)),
                    "Random reading through PFSC into UFS2 returned incorrect bytes.");
            }
            Ps5GameInfo wrappedGame = new FfpfscGameReader().Read(wrappedPath);
            Ps5GameDetails wrappedDetails = await new Ps5DetailsLoader().LoadAsync(wrappedGame);
            Require(wrappedGame.SourceKind == Ps5SourceKind.Ffpfsc && wrappedGame.TitleId == "PPSA99999" &&
                    wrappedDetails.Files.FileCount == 3,
                "Core did not load metadata and files from FFPKG-inside-FFPFSC.");

            Ps5ScanResult scan = await new Ps5LibraryScanner().ScanAsync([root], recursive: false);
            Require(scan.Errors.Count == 0 && scan.Games.Any(item => item.SourceKind == Ps5SourceKind.Ffpkg &&
                    item.RootPath.Equals(imagePath, StringComparison.OrdinalIgnoreCase)) &&
                    scan.Games.Any(item => item.SourceKind == Ps5SourceKind.Ffpfsc &&
                    item.RootPath.Equals(wrappedPath, StringComparison.OrdinalIgnoreCase)),
                "The library scanner did not discover the FFPKG and wrapped FFPFSC images.");

            string editSource = Path.Combine(root, "edit-source");
            Directory.CreateDirectory(Path.Combine(editSource, "tree", "empty"));
            byte[] replacement = Enumerable.Range(0, 1_100_000).Select(index => (byte)(250 - index % 239)).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(editSource, "replacement.bin"), replacement);
            await File.WriteAllTextAsync(Path.Combine(editSource, "added.txt"), "added through transactional rebuild");
            await File.WriteAllTextAsync(Path.Combine(editSource, "tree", "inside.txt"), "directory tree payload");
            Ufs2EditResult edited = await Ufs2Operations.ApplyEditsAsync(imagePath,
            [
                Ufs2EditOperation.Replace("nested/large.bin", Path.Combine(editSource, "replacement.bin")),
                Ufs2EditOperation.AddFile("added.txt", Path.Combine(editSource, "added.txt")),
                Ufs2EditOperation.AddDirectory("manual/empty"),
                Ufs2EditOperation.AddDirectoryTree("imported", Path.Combine(editSource, "tree")),
                Ufs2EditOperation.Delete("eboot.bin")
            ]);
            Require(edited.OperationCount == 5 && edited.Verification.FileCount == 4 &&
                    edited.Verification.FsckErrors == 0,
                "The transactional FFPKG editor produced an unexpected inventory.");
            using (var editedVolume = new Ufs2Volume(imagePath))
            {
                Require(editedVolume.Find("eboot.bin") is null &&
                        editedVolume.Find("manual/empty") is { IsDirectory: true } &&
                        editedVolume.Find("imported/empty") is { IsDirectory: true },
                    "The FFPKG editor did not preserve queued directory/delete operations.");
                Require(editedVolume.ReadAllBytes("nested/large.bin", 2_000_000).AsSpan().SequenceEqual(replacement),
                    "The FFPKG replacement file data is incorrect.");
            }
            Ufs2VerificationResult rebuilt = await Ufs2Operations.RebuildWithEditsAsync(imagePath, static _ => { });
            Require(rebuilt.ManifestSha256 == edited.Verification.ManifestSha256 && rebuilt.FsckErrors == 0,
                "The no-edit verified FFPKG rebuild changed the logical manifest.");

            await Ufs2Operations.ExtractAsync(imagePath, extracted);
            Require((await File.ReadAllBytesAsync(Path.Combine(extracted, "nested", "large.bin")))
                        .AsSpan().SequenceEqual(replacement) &&
                    Directory.Exists(Path.Combine(extracted, "nested", "empty")) &&
                    Directory.Exists(Path.Combine(extracted, "manual", "empty")) &&
                    !File.Exists(Path.Combine(extracted, "eboot.bin")),
                "FFPKG extraction did not preserve file data and empty directories.");
        }
        finally
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
