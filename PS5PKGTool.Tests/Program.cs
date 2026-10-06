using System.Security.Cryptography;
using PS5PKGTool.Ffpfsc;

// Dependency-free regression runner; throws (nonzero exit) on any failed check.
if (args is ["--gui-smoke"])
{
    Environment.ExitCode = NativeGuiChecks.Run();
    return;
}
if (args.Length == 2)
{
    VerifyContents(args[0], args[1]);
    Console.WriteLine("PASS: image paths, sizes and SHA-256 contents match the source.");
    return;
}
if (args.Length != 0) throw new ArgumentException("Usage: PS5PKGTool.Tests [source-directory image.exfat]");

string root = Path.Combine(Path.GetTempPath(), "ps5pkgtool-tests-" + Guid.NewGuid().ToString("N"));
try
{
    string source = Path.Combine(root, "test game 日本語");
    Directory.CreateDirectory(Path.Combine(source, "sce_sys"));
    Directory.CreateDirectory(Path.Combine(source, "data", "nested"));
    Directory.CreateDirectory(Path.Combine(source, "empty directory"));
    File.WriteAllBytes(Path.Combine(source, "eboot.bin"), [0, 1, 2, 255]);
    File.WriteAllText(Path.Combine(source, "sce_sys", "param.json"), "{\"titleId\":\"PPSA00000\"}");
    File.WriteAllBytes(Path.Combine(source, "data", "nested", "日本語.dat"), RandomNumberGenerator.GetBytes(2 * 1024 * 1024 + 17));
    File.WriteAllBytes(Path.Combine(source, "zero.bin"), []);
    string output = Path.Combine(root, "game.exfat");
    Check(await CliCommand.RunAsync(["build-exfat", source, output]) == 0, "CLI build failed");
    VerifyContents(source, output);
    byte[] originalHash = SHA256.HashData(File.ReadAllBytes(output));
    Check(await CliCommand.RunAsync(["build-exfat", source, output]) == 1, "Existing output was accepted");
    Check(originalHash.AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(output))), "Existing output changed");
    Check(await CliCommand.RunAsync(["build-exfat", source, Path.Combine(source, "inside.exfat")]) == 1, "Output inside source was accepted");
    Check(await CliCommand.RunAsync(["build-exfat", Path.Combine(root, "missing"), Path.Combine(root, "missing.exfat")]) == 1, "Missing source was accepted");
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();
    string cancelledOutput = Path.Combine(root, "cancelled.exfat");
    Check(await CliCommand.RunAsync(["build-exfat", source, cancelledOutput], cancellation.Token) == 130, "Cancellation was ignored");
    Check(!File.Exists(cancelledOutput), "Cancelled command published an image");
    string secondOutput = Path.Combine(root, "second.exfat");
    await ExfatImage.WriteDirectoryAsync(source, secondOutput);
    Check(originalHash.AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(secondOutput))), "Non-AMPR output is not deterministic");
    Console.WriteLine("PASS: nested/Unicode/empty files and directories, streamed contents, deterministic output, overwrite protection, missing source, cancellation.");
    await PackageConversionChecks.RunAsync(root);
}
finally
{
    if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
}

static void VerifyContents(string source, string image)
{
    using var volume = new ExfatVolume(File.OpenRead(image));
    string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
    Check(volume.Entries.Count(entry => !entry.IsDirectory) == files.Length, "File count mismatch");
    foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        Check(volume.Find(Path.GetRelativePath(source, directory).Replace(Path.DirectorySeparatorChar, '/'))?.IsDirectory == true, "Missing directory: " + directory);
    foreach (string file in files)
    {
        string relative = Path.GetRelativePath(source, file).Replace(Path.DirectorySeparatorChar, '/');
        Check(volume.Find(relative)?.Size == new FileInfo(file).Length, "Size mismatch: " + relative);
        using Stream actual = volume.OpenFile(relative);
        using Stream expected = File.OpenRead(file);
        Check(SHA256.HashData(actual).AsSpan().SequenceEqual(SHA256.HashData(expected)), "Content mismatch: " + relative);
    }
}

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
