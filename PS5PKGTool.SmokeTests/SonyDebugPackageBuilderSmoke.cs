using System.Text;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;

internal static class SonyDebugPackageBuilderSmoke
{
    private const string ContentId = "UP0000-PPSA12345_00-TESTPACKAGE00000";

    public static async Task RunAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool-DebugPkg-" + Guid.NewGuid().ToString("N"));
        string source = Path.Combine(root, "dump");
        Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(source, "nested", "empty"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(source, "sce_sys", "param.json"), $$"""
                { "contentId": "{{ContentId}}", "titleId": "PPSA12345", "localizedParameters": { "en-US": { "titleName": "Builder Smoke" } } }
                """);
            byte[] content = Enumerable.Range(0, 900_000).Select(index => (byte)(index * 37)).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(source, "nested", "payload.bin"), content);

            string packagePath = Path.Combine(root, "default.pkg");
            SonyDebugPackageBuildResult built = await ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(source,
                packagePath, new ProsperoDebugPackageBuildOptions { ContentId = ContentId });
            Require(built.UsesDefaultPasscode && built.SourceFiles == 2, "Default debug package result is incorrect.");
            SonyDebugPackageValidationResult valid = SonyDebugPackageBuilder.Validate(packagePath);
            Require(valid.IsValid && valid.IndexedFiles >= 2, "Default debug package validation failed: " + valid.Message);
            SonyPackageAcceptanceReport acceptance = SonyPackageAcceptanceValidator.Validate(packagePath);
            Require(acceptance.IsStructurallyReady,
                "The acceptance validator rejected a package produced by the builder: " +
                string.Join(" | ", acceptance.Checks.Where(check => check.State == SonyPackageCheckState.Fail)
                    .Select(check => check.Name + ": " + check.Message)));
            SonyPkgSummary summary = new SonyPkgReader().Read(packagePath);
            Require(summary.Kind == SonyPkgKind.FinalizedDebug && summary.ContentId == ContentId,
                "Created FIH or CNT metadata is incorrect.");
            Require(summary.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed,
                "Supplemental package file index is incomplete.");
            string extracted = Path.Combine(root, "extracted");
            SonyPackageExtractResult extraction = await SonyPackageExtraction.ExtractAsync(packagePath, extracted);
            Require(extraction.FileCount >= 2 && File.ReadAllBytes(Path.Combine(extracted, "nested", "payload.bin")).SequenceEqual(content),
                "Debug package extraction did not reproduce the source payload.");
            var game = new PS5PKGTool.Core.Services.SonyPkgGameReader().Read(packagePath);
            using (IReadOnlyGameFileSystem fileSystem = GameFileSystem.Open(game))
            using (Stream payload = fileSystem.OpenRead("nested/payload.bin"))
            {
                byte[] reopened = new byte[content.Length];
                payload.ReadExactly(reopened);
                Require(reopened.SequenceEqual(content), "A stored NAPS application file did not reopen exactly.");
            }
            string corruptPath = Path.Combine(root, "corrupt.pkg");
            File.Copy(packagePath, corruptPath);
            long containerOffset = (long)new SonyPkgReader().Read(packagePath).EmbeddedCntOffset;
            using (var corrupt = new FileStream(corruptPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                corrupt.Position = containerOffset + 0x10;
                int value = corrupt.ReadByte();
                corrupt.Position--;
                corrupt.WriteByte((byte)(value ^ 0x5A));
            }
            Require(!SonyDebugPackageBuilder.Validate(corruptPath).IsValid,
                "CNT corruption was not rejected by full validation.");
            Require(!SonyPackageAcceptanceValidator.Validate(corruptPath).IsStructurallyReady,
                "CNT corruption was not rejected by the acceptance validator.");

            byte[] packageBytes = File.ReadAllBytes(packagePath);
            int split = packageBytes.Length / 2;
            string piece0 = Path.Combine(root, "app_0.pkg"); string piece1 = Path.Combine(root, "app_1.pkg");
            File.WriteAllBytes(piece0, packageBytes[..split]); File.WriteAllBytes(piece1, packageBytes[split..]);
            string merged = Path.Combine(root, "merged.pkg");
            await SonyPackageMerge.MergeAtomicAsync([piece0, piece1], merged);
            Require(File.ReadAllBytes(merged).SequenceEqual(packageBytes), "Split package merge did not preserve bytes.");

            string splitDirectory = Path.Combine(root, "verified-split");
            SonyPackageSplitResult splitResult = await SonyPackageSplit.CreateAsync(packagePath, splitDirectory,
                new SonyPackageSplitOptions { MaximumPieceBytes = 0x10000 });
            SonyPackageMergeValidation splitValidation = await SonyPackageSplit.ValidateManifestAsync(splitResult.ManifestPath);
            Require(splitValidation.IsValid && splitValidation.CombinedBytes == packageBytes.Length,
                "Split manifest validation failed: " + splitValidation.Message);
            string manifestMerged = Path.Combine(root, "manifest-merged.pkg");
            await SonyPackageMerge.MergeAtomicAsync(splitResult.Manifest.Pieces.Select(piece =>
                Path.Combine(splitDirectory, piece.FileName)).ToArray(), manifestMerged);
            Require(File.ReadAllBytes(manifestMerged).SequenceEqual(packageBytes),
                "Manifest-backed split merge did not preserve bytes.");
            string verifiedManifestMerged = Path.Combine(root, "verified-manifest-merged.pkg");
            await SonyPackageMerge.MergeManifestAtomicAsync(splitResult.ManifestPath, verifiedManifestMerged);
            Require(File.ReadAllBytes(verifiedManifestMerged).SequenceEqual(packageBytes),
                "Verified-manifest package merge did not preserve bytes.");

            const string customPasscode = "0123456789ABCDEF0123456789ABCDEF";
            string customPath = Path.Combine(root, "custom.pkg");
            await ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(source, customPath,
                new ProsperoDebugPackageBuildOptions { ContentId = ContentId, Passcode = customPasscode });
            Require(!SonyDebugPackageBuilder.Validate(customPath).IsValid,
                "A custom passcode package unexpectedly validated with the default passcode.");
            Require(SonyDebugPackageBuilder.Validate(customPath, customPasscode).IsValid,
                "A custom passcode package failed with its matching passcode.");

            string preservedPath = Path.Combine(root, "preserved.pkg");
            byte[] sentinel = Encoding.ASCII.GetBytes("existing destination");
            await File.WriteAllBytesAsync(preservedPath, sentinel);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            bool cancelled = false;
            try
            {
                await ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(source, preservedPath,
                    new ProsperoDebugPackageBuildOptions { ContentId = ContentId }, cancellationToken: cancellation.Token);
            }
            catch (OperationCanceledException) { cancelled = true; }
            Require(cancelled && File.ReadAllBytes(preservedPath).SequenceEqual(sentinel),
                "Cancellation did not preserve the existing destination.");
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
