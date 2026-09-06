using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;

internal static class SonyPfsBuilderSmoke
{
    public static async Task RunAsync()
    {
        VerifyCredentialsAndCrypto();
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool-PfsBuild-" + Guid.NewGuid().ToString("N"));
        string source = Path.Combine(root, "source");
        string image = Path.Combine(root, "fixture.pfs");
        Directory.CreateDirectory(Path.Combine(source, "sce_sys", "nested", "empty"));
        try
        {
            byte[] small = "native pfs builder"u8.ToArray();
            byte[] large = new byte[13 * 0x10000 + 731];
            for (int index = 0; index < large.Length; index++) large[index] = (byte)(index * 31 + 7);
            await File.WriteAllBytesAsync(Path.Combine(source, "sce_sys", "param.json"), small);
            await File.WriteAllBytesAsync(Path.Combine(source, "sce_sys", "nested", "large.bin"), large);

            SonyPfsBuildResult built = await SonyPfsImageBuilder.CreateFromDirectoryAsync(source, image);
            Require(built.FileCount == 2, "PFS builder file count is incorrect.");
            Require(built.SourceBytes == small.Length + large.Length, "PFS builder source size is incorrect.");

            var summary = new SonyPfsReader().Inspect(image, 0, new FileInfo(image).Length);
            Require(summary.AccessState == SonyPfsAccessState.PlaintextIndexed,
                "Built PFS did not reopen as plaintext: " + summary.StatusMessage);
            Require(summary.Files.Count == 2, "Built PFS file inventory is incomplete.");
            Require(ReadFile(image, summary.Files.Single(file => file.RelativePath == "sce_sys/param.json"))
                .SequenceEqual(small), "Small PFS file did not round trip.");
            Require(ReadFile(image, summary.Files.Single(file => file.RelativePath == "sce_sys/nested/large.bin"))
                .SequenceEqual(large), "Large contiguous PFS file did not round trip.");
        }
        finally
        {
            try { Directory.Delete(root, true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static void VerifyCredentialsAndCrypto()
    {
        const string contentId = "UP0000-PPSA12345_00-TESTPACKAGE00000";
        SonyDebugPackageCredentials defaults = SonyDebugPackageCredentials.Create(contentId);
        Require(defaults.UsesDefaultPasscode, "Default debug passcode was not selected.");
        Require(defaults.ImageKey.Length == 32, "EKPFS derivation did not return 32 bytes.");
        SonyDebugPackageCredentials custom = SonyDebugPackageCredentials.Create(contentId,
            "abcdefghijklmnopqrstuvwxyz123456");
        Require(!custom.UsesDefaultPasscode && !custom.ImageKey.SequenceEqual(defaults.ImageKey),
            "Custom passcode did not change the derived image key.");

        byte[] seed = Enumerable.Range(0, 16).Select(index => (byte)(index * 11)).ToArray();
        SonyPfsCryptoContext context = SonyPfsCrypto.DeriveContext(custom.ImageKey, seed, 0x10000, newCrypt: true);
        byte[] plaintext = new byte[0x10000];
        for (int index = 0; index < plaintext.Length; index++) plaintext[index] = (byte)(index * 17 + 3);
        byte[] encrypted = (byte[])plaintext.Clone();
        SonyPfsCrypto.EncryptBlock(encrypted, context, 7, signedDomain: true);
        Require(!encrypted.SequenceEqual(plaintext), "AES XTS encryption left the block unchanged.");
        SonyPfsCrypto.DecryptBlock(encrypted, context, 7, signedDomain: true);
        Require(encrypted.SequenceEqual(plaintext), "AES XTS encrypt/decrypt round trip failed.");
    }

    private static byte[] ReadFile(string imagePath, SonyPfsEntry entry)
    {
        byte[] result = new byte[checked((int)entry.Size)];
        using var input = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        int target = 0;
        foreach (SonyPfsExtent extent in entry.Extents)
        {
            input.Position = extent.Offset;
            int count = checked((int)Math.Min(extent.Length, result.Length - target));
            input.ReadExactly(result.AsSpan(target, count));
            target += count;
        }
        Require(target == result.Length, "PFS file extents ended early.");
        return result;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
