using System.Security.Cryptography;
using System.Text.Json;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Ffpfsc;

internal static class VolumeFixtureGenerator
{
    private const string ContentId = "UP0000-PPSA55555_00-FIXTUREPARITY000";
    private const string SeedHex = "000102030405060708090A0B0C0D0E0F";

    public static async Task<bool> RunAsync(string outputDirectory)
    {
        string root = Path.GetFullPath(outputDirectory);
        string tree = Path.Combine(root, "tree");
        string volumes = Path.Combine(root, "volumes");
        string reference = Path.Combine(root, "reference");
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        Directory.CreateDirectory(Path.Combine(tree, "sce_sys"));
        Directory.CreateDirectory(Path.Combine(tree, "nested"));
        Directory.CreateDirectory(volumes);
        Directory.CreateDirectory(reference);

        await File.WriteAllTextAsync(Path.Combine(tree, "sce_sys", "param.json"), $$"""
            { "contentId": "{{ContentId}}", "titleId": "PPSA55555", "localizedParameters": { "en-US": { "titleName": "Volume Fixture" } } }
            """);
        File.WriteAllBytes(Path.Combine(tree, "nested", "payload.bin"),
            Enumerable.Range(0, 200_000).Select(index => (byte)(index * 31)).ToArray());
        File.WriteAllBytes(Path.Combine(tree, "empty.bin"), []);
        File.WriteAllBytes(Path.Combine(tree, "boundary.bin"),
            Enumerable.Range(0, 64 * 1024 + 123).Select(index => (byte)(index * 7)).ToArray());

        byte[] seed = Convert.FromHexString(SeedHex);
        var options = new SonyDebugPackageBuildOptions { ContentId = ContentId, Seed = seed };

        string exfat = Path.Combine(volumes, "game.exfat");
        await Ps5ImageConversionService.ConvertAsync(tree, exfat, Ps5ImageConversionTarget.Exfat);
        string ffpkg = Path.Combine(volumes, "game.ffpkg");
        await Ps5ImageConversionService.ConvertAsync(exfat, ffpkg, Ps5ImageConversionTarget.Ffpkg);
        string ffpfsc = Path.Combine(volumes, "game.ffpfsc");
        await Ps5ImageConversionService.ConvertAsync(ffpkg, ffpfsc, Ps5ImageConversionTarget.Ffpfsc);

        string fromDirectory = Path.Combine(reference, "from-directory.pkg");
        await SonyDebugPackageBuilder.CreateFromDirectoryAsync(tree, fromDirectory, options);
        string fromExfat = Path.Combine(reference, "from-exfat.pkg");
        await VolumeDebugPackageBuilder.CreateFromImageAsync(exfat, fromExfat, options);
        string fromFfpkg = Path.Combine(reference, "from-ffpkg.pkg");
        await VolumeDebugPackageBuilder.CreateFromImageAsync(ffpkg, fromFfpkg, options);
        string fromFfpfsc = Path.Combine(reference, "from-ffpfsc.pkg");
        await VolumeDebugPackageBuilder.CreateFromImageAsync(ffpfsc, fromFfpfsc, options);

        byte[] directoryBytes = File.ReadAllBytes(fromDirectory);
        bool identical = true;
        foreach (string candidate in new[] { fromExfat, fromFfpkg, fromFfpfsc })
        {
            bool match = File.ReadAllBytes(candidate).SequenceEqual(directoryBytes);
            identical &= match;
            Console.WriteLine($"{Path.GetFileName(candidate)} == from-directory.pkg: {match}");
        }

        var manifest = new
        {
            contentId = ContentId,
            seed = SeedHex,
            files = Directory.EnumerateFiles(tree, "*", SearchOption.AllDirectories)
                .Select(path => new
                {
                    path = Path.GetRelativePath(tree, path).Replace('\\', '/'),
                    size = new FileInfo(path).Length,
                    sha256 = Sha256(path)
                })
                .OrderBy(entry => entry.path, StringComparer.Ordinal)
                .ToArray(),
            volumes = Directory.EnumerateFiles(volumes).OrderBy(path => path, StringComparer.Ordinal)
                .ToDictionary(path => Path.GetFileName(path), Sha256),
            packages = Directory.EnumerateFiles(reference).OrderBy(path => path, StringComparer.Ordinal)
                .ToDictionary(path => Path.GetFileName(path), Sha256),
            byteIdentical = identical
        };
        await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"),
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"Fixture root: {root}");
        Console.WriteLine($"Byte-identical across directory/exfat/ffpkg/ffpfsc: {identical}");
        return identical;
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}
