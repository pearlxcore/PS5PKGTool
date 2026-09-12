using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;

internal static class SonyPkgReaderSmoke
{
    private const string ContentId = "UP0000-PPSA12345_00-TESTPACKAGE00000";

    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PS5PKGTool-SonyPkg-" + Guid.NewGuid().ToString("N"));
        string source = Path.Combine(directory, "dump");
        Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        try
        {
            File.WriteAllText(Path.Combine(source, "sce_sys", "param.json"), $$"""
                { "contentId": "{{ContentId}}", "titleId": "PPSA12345", "contentVersion": "01.000.000",
                  "localizedParameters": { "en-US": { "titleName": "Synthetic PS5 Package" } } }
                """);
            byte[] content = Encoding.UTF8.GetBytes("hello from pfs");
            File.WriteAllBytes(Path.Combine(source, "nested", "payload.bin"), content);

            string packagePath = Path.Combine(directory, "test.pkg");
            ProsperoDebugPackageBuilder.CreateFromDirectoryAsync(source, packagePath,
                new ProsperoDebugPackageBuildOptions { ContentId = ContentId }).GetAwaiter().GetResult();

            var reader = new SonyPkgReader();
            SonyPkgSummary package = reader.Read(packagePath);
            Require(package.Kind == SonyPkgKind.FinalizedDebug, "FIH debug type detection failed.");
            Require(package.FormatVersion == 3, "FIH header fields were not decoded.");
            Require(package.ContentId == ContentId, "CNT content ID was not decoded.");
            Require(package.Entries.Any(entry => entry.Id == 0x2000), "Embedded CNT was not decoded.");

            SonyPkgEntry param = package.Entries.Single(entry => entry.Id == 0x2000);
            string paramJson = Encoding.UTF8.GetString(reader.ReadEntryBytes(packagePath, package, param, 1024 * 1024));
            Require(paramJson.Contains("Synthetic PS5 Package", StringComparison.Ordinal), "Bounded CNT entry read failed.");

            Ps5GameInfo game = new SonyPkgGameReader().Read(packagePath);
            Require(game.SourceKind == Ps5SourceKind.SonyPackage, "Package source kind was not set.");
            Require(game.Title == "Synthetic PS5 Package", "Package param.json was not integrated.");
            Require(game.TitleId == "PPSA12345", "Package title ID was not integrated.");
            Require(game.Package?.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed,
                "Plaintext nested PFS was not indexed.");
            using (IReadOnlyGameFileSystem files = GameFileSystem.Open(game))
            {
                Require(files.FileExists("nested/payload.bin"), "Nested PFS path reconstruction failed.");
                using Stream stream = files.OpenRead("nested/payload.bin");
                byte[] reopened = new byte[content.Length];
                stream.ReadExactly(reopened);
                Require(reopened.SequenceEqual(content), "Nested PFS direct read failed.");
            }

            // A package restored from the cached manifest (engine access is not serialized) must still
            // expose its inner filesystem for details, artwork, trophies, activities, and executable.
            string manifestJson = JsonSerializer.Serialize(game);
            Ps5GameInfo restored = JsonSerializer.Deserialize<Ps5GameInfo>(manifestJson)!;
            Ps5GameDetails restoredDetails = new Ps5DetailsLoader().LoadAsync(restored).GetAwaiter().GetResult();
            Require(restoredDetails.Files.FileCount >= 2 &&
                    restoredDetails.Files.Files.Any(file =>
                        file.RelativePath.Replace('\\', '/') == "nested/payload.bin"),
                "A manifest-restored package did not expose its inner filesystem.");

            string retailPath = Path.Combine(directory, "metadata-less-retail.pkg");
            File.WriteAllBytes(retailPath, BuildMetadataLessRetailFih());
            SonyPkgSummary retail = reader.Read(retailPath);
            Require(retail.Kind == SonyPkgKind.FinalizedRetail, "A metadata-less retail FIH was not detected.");
            Require(retail.SignedByte == 0x80 && retail.FormatVersion == 3,
                "A metadata-less retail FIH header was not decoded.");
            Require(retail.Entries.Count == 0, "A metadata-less retail FIH unexpectedly exposed CNT entries.");
            Require(retail.PfsImageOffset == 0x10000 && retail.PfsImageSize == 0x10000,
                "A metadata-less retail FIH PFS range was not decoded.");
            Require(retail.NestedPfs?.AccessState == SonyPfsAccessState.EncryptedKeyRequired,
                "A metadata-less retail FIH did not report the PFS as key-required.");
            Ps5GameInfo retailGame = new SonyPkgGameReader().Read(retailPath);
            Require(retailGame.SourceKind == Ps5SourceKind.SonyPackage && retailGame.Package?.Entries.Count == 0,
                "A metadata-less retail package was not loaded as a package without metadata.");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] BuildMetadataLessRetailFih()
    {
        const int pfsOffset = 0x10000;
        const int pfsSize = 0x10000;
        const int length = pfsOffset + pfsSize;
        byte[] image = new byte[length];
        image[0] = 0x7F;
        image[1] = (byte)'F';
        image[2] = (byte)'I';
        image[3] = (byte)'H';
        image[0x05] = 0x80;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(0x06, 2), 3);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x10, 8), pfsOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x18, 8), pfsSize);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x20, 8), pfsOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x58, 8), length);
        return image;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
