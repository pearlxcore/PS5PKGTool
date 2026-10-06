using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text.RegularExpressions;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;

internal static class PackageConversionChecks
{
    public static async Task RunAsync(string root)
    {
        string source = Path.Combine(root, "pkg source");
        Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(source, "data", "nested"));
        File.WriteAllText(Path.Combine(source, "sce_sys", "param.json"),
            "{\"titleId\":\"PPSA00000\",\"contentId\":\"HP0000-PPSA00000_00-0000000000000000\",\"localizedParameters\":{\"defaultLanguage\":\"en-US\",\"en-US\":{\"titleName\":\"Generated macOS test\"}}}");
        File.WriteAllBytes(Path.Combine(source, "eboot.bin"), [0, 1, 2, 255]);
        File.WriteAllBytes(Path.Combine(source, "data", "nested", "example.dat"),
            Enumerable.Range(0, 1024 * 1024 + 17).Select(index => (byte)(index % 251)).ToArray());
        File.WriteAllBytes(Path.Combine(source, "zero.bin"), []);

        long storedSize = 0;
        foreach (Ps5InnerCompression compression in new[] { Ps5InnerCompression.Stored, Ps5InnerCompression.Auto })
        {
            string package = Path.Combine(root, compression + ".pkg");
            bool hasKraken = false;
            await SonyDebugPackageBuilder.CreateFromDirectoryAsync(source, package, new SonyDebugPackageBuildOptions
            {
                ContentId = "HP0000-PPSA00000_00-0000000000000000",
                Compression = compression,
                Seed = new byte[16],
                FakeSignModules = false,
                InjectRightSprx = false,
                Log = line => { if (Regex.IsMatch(line, @"Kraken [1-9]\d*")) hasKraken = true; }
            });
            if (compression == Ps5InnerCompression.Stored) storedSize = new FileInfo(package).Length;
            else Check(hasKraken && new FileInfo(package).Length < storedSize, "Fixture did not exercise Kraken compression");
            Check(new SonyPkgGameReader().Read(package).TitleId == "PPSA00000", "Metadata parsing failed");
            if (compression == Ps5InnerCompression.Stored)
                await GuiChecks.RunAsync(root, package, source);

            string image = Path.Combine(root, compression + ".exfat");
            Check(await CliCommand.RunAsync(["convert", package, image]) == 0, compression + " conversion failed");
            using var volume = new ExfatVolume(File.OpenRead(image));
            foreach (string relative in new[] { "eboot.bin", "data/nested/example.dat", "zero.bin" })
            {
                using Stream expected = File.OpenRead(Path.Combine(source, relative));
                using Stream actual = volume.OpenFile(relative);
                Check(volume.Find(relative)?.Size == expected.Length, "File size mismatch: " + relative);
                Check(SHA256.HashData(expected).AsSpan().SequenceEqual(SHA256.HashData(actual)), "File contents differ: " + relative);
            }
            Check(volume.ReadAllText("sce_sys/param.json").Contains("PPSA00000"), "Outer CNT metadata was lost");
            Check(volume.Find("inode_flat_path_table") is null, "Internal PFS table leaked into image");
            Check(await CliCommand.RunAsync(["convert", package, image]) == 1, "Existing package conversion output was accepted");
            if (compression == Ps5InnerCompression.Stored)
            {
                string metadataOnly = Path.Combine(root, "metadata-only.pkg");
                using (Stream input = File.OpenRead(package))
                using (Stream output = File.Create(metadataOnly))
                {
                    input.Position = (long)new SonyPkgGameReader().Read(package).Package!.EmbeddedCntOffset;
                    input.CopyTo(output);
                }
                Check(new SonyPkgGameReader().Read(metadataOnly).Package?.Kind == SonyPkgKind.MetadataContainer,
                    "Metadata-only fixture has the wrong package kind");
                string retail = Path.Combine(root, "retail-header.pkg");
                File.Copy(package, retail);
                using (var header = new FileStream(retail, FileMode.Open, FileAccess.Write))
                {
                    header.Position = 5;
                    header.WriteByte(0x80);
                }
                Check(new SonyPkgGameReader().Read(retail).Package?.Kind == SonyPkgKind.FinalizedRetail,
                    "Retail-header fixture has the wrong package kind");
                // Wrap our generated fixture in an LIH header to exercise the
                // patch-kind rejection; this is not a functional patch fixture.
                string patch = Path.Combine(root, "patch-header.pkg");
                byte[] lih = new byte[0x10000];
                new byte[] { 0x7f, (byte)'L', (byte)'I', (byte)'H' }.CopyTo(lih, 0);
                BinaryPrimitives.WriteUInt16LittleEndian(lih.AsSpan(6), 1);
                BinaryPrimitives.WriteUInt32LittleEndian(lih.AsSpan(16), 0x10000);
                ulong originalSize = (ulong)new FileInfo(package).Length;
                ulong cntOffset = new SonyPkgGameReader().Read(package).Package!.EmbeddedCntOffset;
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(24), originalSize + 0x10000);
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(32), 0x10000);
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(40), 0x10000);
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(48), cntOffset + 0x10000);
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(56), originalSize - cntOffset);
                BinaryPrimitives.WriteUInt64LittleEndian(lih.AsSpan(64), originalSize + 0x10000);
                using (Stream input = File.OpenRead(package))
                using (Stream output = File.Create(patch))
                {
                    output.Write(lih);
                    input.CopyTo(output);
                }
                Check(new SonyPkgGameReader().Read(patch).Package?.Kind == SonyPkgKind.FinalizedPatch,
                    "Patch-header fixture has the wrong package kind");
                foreach (string unsupported in new[] { metadataOnly, retail, patch })
                {
                    string unsupportedOutput = unsupported + ".exfat";
                    Check(await CliCommand.RunAsync(["convert", unsupported, unsupportedOutput]) == 1,
                        "Unsupported package was reported as a successful game conversion");
                    Check(!File.Exists(unsupportedOutput), "Unsupported package published an image");
                    var model = new PS5PKGTool.Mac.ConversionViewModel { Input = unsupported };
                    await model.DetectAsync();
                    Check(!model.Ready && !model.CanConvert && model.Error.Contains("Only finalized"),
                        "GUI accepted an unsupported package");
                    if (unsupported != patch)
                    {
                        bool rejected = false;
                        try { await SonyPackageImageConversion.ConvertAsync(unsupported, unsupportedOutput, Ps5ImageConversionTarget.Exfat); }
                        catch (InvalidDataException) { rejected = true; }
                        Check(rejected && !File.Exists(unsupportedOutput), "Shared service accepted an unsupported package");
                    }
                }
            }
            Console.WriteLine($"PASS: {compression} debug PKG → exFAT, metadata and file contents.");
        }
        string invalid = Path.Combine(root, "invalid.pkg");
        File.WriteAllBytes(invalid, [1, 2, 3, 4]);
        string invalidOutput = Path.Combine(root, "invalid.exfat");
        Check(await CliCommand.RunAsync(["convert", invalid, invalidOutput]) == 1, "Invalid package was accepted");
        Check(!File.Exists(invalidOutput), "Failed conversion published an image");
        if (Environment.GetEnvironmentVariable("PS5PKGTOOL_TEST_PKG") is { Length: > 0 } suppliedPackage)
            VerifyExistingImage(suppliedPackage, Environment.GetEnvironmentVariable("PS5PKGTOOL_TEST_EXFAT")
                ?? throw new ArgumentException("Set PS5PKGTOOL_TEST_EXFAT to the completed image path."));
        Check(!AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            assembly.GetName().Name is "Oodle.NET" or "LibProsperoPkg"), "Conversion unexpectedly loaded an alternate/native compression backend");
    }

    private static void VerifyExistingImage(string package, string image)
    {
        using var files = GameFileSystem.Open(new SonyPkgGameReader().Read(package));
        using var volume = new ExfatVolume(File.OpenRead(image));
        string[] excluded = ["inode_flat_path_table", "apr_flat_path_table", "afid_to_ino_table",
            "sce_sys/keystone", "sce_sys/about/right.sprx", "sce_sys/pfs-version.dat"];
        var expectedFiles = files.Files.Where(file => !excluded.Contains(file.RelativePath, StringComparer.OrdinalIgnoreCase)).ToArray();
        foreach (var file in expectedFiles)
            Check(volume.Find(file.RelativePath)?.Size == file.Size, "Missing file or size mismatch: " + file.RelativePath);
        foreach (var file in expectedFiles.Where(file => file.RelativePath is "eboot.bin" or "sce_sys/param.json")
                     .Concat(expectedFiles.OrderByDescending(file => file.Size).Take(1)).Distinct())
        {
            using Stream expected = files.OpenRead(file.RelativePath);
            using Stream actual = volume.OpenFile(file.RelativePath);
            int count = (int)Math.Min(1024 * 1024, file.Size);
            foreach (long offset in new[] { 0L, Math.Max(0, file.Size / 2 - count / 2), Math.Max(0, file.Size - count) }.Distinct())
            {
                byte[] a = new byte[count], b = new byte[count];
                expected.Position = actual.Position = offset;
                expected.ReadExactly(a);
                actual.ReadExactly(b);
                Check(a.AsSpan().SequenceEqual(b), $"Content mismatch: {file.RelativePath} at {offset}");
            }
            Console.WriteLine($"PASS: start/middle/end samples match for {file.RelativePath} ({file.Size} bytes).");
        }
        Console.WriteLine($"PASS: supplied PKG → exFAT, {expectedFiles.Length} paths and sizes match.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
