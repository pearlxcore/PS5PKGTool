using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Ffpfsc;
using UFS2Tool;

internal static class VolumePackageBuilderSmoke
{
    private const string ContentId = "UP0000-PPSA77777_00-VOLUMEPACKAGE000";

    public static async Task RunAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool-VolumePkg-" + Guid.NewGuid().ToString("N"));
        string dump = Path.Combine(root, "dump");
        Directory.CreateDirectory(Path.Combine(dump, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(dump, "nested"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dump, "sce_sys", "param.json"), $$"""
                { "contentId": "{{ContentId}}", "titleId": "PPSA77777", "localizedParameters": { "en-US": { "titleName": "Volume Builder Smoke" } } }
                """);
            byte[] content = Enumerable.Range(0, 300_000).Select(index => (byte)(index * 31)).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(dump, "nested", "payload.bin"), content);

            string exfat = Path.Combine(root, "game.exfat");
            await Ps5ImageConversionService.ConvertAsync(dump, exfat, Ps5ImageConversionTarget.Exfat);
            string ffpkg = Path.Combine(root, "game.ffpkg");
            await Ps5ImageConversionService.ConvertAsync(exfat, ffpkg, Ps5ImageConversionTarget.Ffpkg);
            string ffpfsc = Path.Combine(root, "game.ffpfsc");
            await Ps5ImageConversionService.ConvertAsync(ffpkg, ffpfsc, Ps5ImageConversionTarget.Ffpfsc);

            foreach ((string image, string label) in new[]
                     {
                         (exfat, "exfat"), (ffpkg, "ffpkg"), (ffpfsc, "ffpfsc")
                     })
            {
                string packagePath = Path.Combine(root, label + ".pkg");
                SonyDebugPackageBuildResult built = await VolumeDebugPackageBuilder.CreateFromImageAsync(image,
                    packagePath, new SonyDebugPackageBuildOptions { ContentId = ContentId });
                Require(built.PackageSize > 0, $"{label}: the builder produced no package.");
                SonyDebugPackageValidationResult valid = SonyDebugPackageBuilder.Validate(packagePath);
                Require(valid.IsValid, $"{label}: package validation failed: " + valid.Message);
                string extracted = Path.Combine(root, label + "-extracted");
                SonyPackageExtractResult extraction = await SonyPackageExtraction.ExtractAsync(packagePath, extracted);
                Require(extraction.FileCount >= 1 &&
                        File.ReadAllBytes(Path.Combine(extracted, "nested", "payload.bin")).SequenceEqual(content),
                    $"{label}: package extraction did not reproduce the source payload.");
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static async Task RunPackageToImageAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool-PkgImage-" + Guid.NewGuid().ToString("N"));
        string dump = Path.Combine(root, "dump");
        Directory.CreateDirectory(Path.Combine(dump, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(dump, "nested"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dump, "sce_sys", "param.json"), $$"""
                { "contentId": "{{ContentId}}", "titleId": "PPSA77777", "localizedParameters": { "en-US": { "titleName": "Package Image Smoke" } } }
                """);
            byte[] content = Enumerable.Range(0, 250_000).Select(index => (byte)(index * 17)).ToArray();
            await File.WriteAllBytesAsync(Path.Combine(dump, "nested", "payload.bin"), content);

            string packagePath = Path.Combine(root, "source.pkg");
            await SonyDebugPackageBuilder.CreateFromDirectoryAsync(dump, packagePath,
                new SonyDebugPackageBuildOptions { ContentId = ContentId });

            var cases = new (Ps5ImageConversionTarget Target, Ps5ImageFormat Format, string Extension, string Label)[]
            {
                (Ps5ImageConversionTarget.Exfat, Ps5ImageFormat.Exfat, ".exfat", "exfat"),
                (Ps5ImageConversionTarget.Ffpkg, Ps5ImageFormat.Ufs2, ".ffpkg", "ffpkg"),
                (Ps5ImageConversionTarget.Ffpfsc, Ps5ImageFormat.Pfs, ".ffpfsc", "ffpfsc")
            };
            foreach ((Ps5ImageConversionTarget target, Ps5ImageFormat format, string extension, string label) in cases)
            {
                string image = Path.Combine(root, "out-" + label + extension);
                await SonyPackageImageConversion.ConvertAsync(packagePath, image, target, overwrite: true);
                Require(Ps5ImageFormatProbe.Detect(image) == format, $"{label}: unexpected image format.");
                string tree = Path.Combine(root, label + "-tree");
                switch (format)
                {
                    case Ps5ImageFormat.Exfat:
                        await ExfatImage.ExtractDirectoryAsync(image, tree);
                        break;
                    case Ps5ImageFormat.Ufs2:
                        await Ufs2Operations.ExtractAsync(image, tree);
                        break;
                    default:
                        await FfpfscImage.ExtractToDirectoryAsync(image, tree, overwrite: true);
                        break;
                }
                Require(File.Exists(Path.Combine(tree, "sce_sys", "param.json")),
                    $"{label}: the outer CNT metadata was not carried into the image.");
                Require(File.ReadAllBytes(Path.Combine(tree, "nested", "payload.bin")).SequenceEqual(content),
                    $"{label}: the payload did not survive package-to-image conversion.");
                Require(!File.Exists(Path.Combine(tree, "inode_flat_path_table")),
                    $"{label}: an internal PFS table leaked into the image.");
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
