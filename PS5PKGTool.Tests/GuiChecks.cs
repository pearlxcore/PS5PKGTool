using System.Security.Cryptography;
using PS5PKGTool.Mac;
using PS5PKGTool.Ffpfsc;

internal static class GuiChecks
{
    public static async Task RunAsync(string root, string package, string source, ConversionViewModel? model = null)
    {
        model ??= new ConversionViewModel();
        model.Input = package;
        await model.DetectAsync();
        Check(model.Ready && model.TitleId == "PPSA00000" && model.CanConvert, "GUI metadata detection failed");
        string workspace = Path.Combine(root, "GUI workspace 日本語");
        Directory.CreateDirectory(workspace);
        model.Workspace = workspace;
        model.Output = Path.Combine(root, "GUI image.exfat");
        bool sawProgress = false;
        model.PropertyChanged += (_, _) => { if (model.Busy && model.Percent > 0) sawProgress = true; };
        await model.ConvertAsync();
        Check(model.Error.Length == 0 && model.Percent == 100 && !model.Busy && model.CanConvert, "GUI conversion failed: " + model.Error);
        using (var volume = new ExfatVolume(File.OpenRead(model.Output)))
        using (Stream actual = volume.OpenFile("data/nested/example.dat"))
        using (Stream expected = File.OpenRead(Path.Combine(source, "data/nested/example.dat")))
            Check(SHA256.HashData(actual).AsSpan().SequenceEqual(SHA256.HashData(expected)), "GUI image contents differ");
        Check(sawProgress, "GUI did not report progress");
        Check(!Directory.EnumerateFileSystemEntries(workspace).Any(), "GUI staging files or directories were not cleaned up");
        byte[] before = SHA256.HashData(File.ReadAllBytes(model.Output));
        await model.ConvertAsync();
        Check(model.Error.Contains("already exists") && before.AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(model.Output))), "GUI overwrite protection failed");
        model.Output = Path.Combine(root, "GUI cancelled.exfat");
        Task conversion = model.ConvertAsync();
        model.Cancel();
        await conversion;
        Check(model.Status.StartsWith("Cancelled.") && !model.Busy && !File.Exists(model.Output), "GUI cancellation failed");
        Check(!Directory.EnumerateFileSystemEntries(workspace).Any(), "Cancelled GUI conversion left staging entries");
        model.Workspace = Path.Combine(root, "missing workspace");
        await model.ConvertAsync();
        Check(model.Error.Contains("workspace") && !File.Exists(model.Output), "Missing GUI workspace was accepted");
        model.Input = Path.Combine(root, "missing.pkg");
        await model.DetectAsync();
        Check(!model.Ready && !model.CanConvert && model.Error.Length > 0 && !model.Busy, "GUI metadata error handling failed");
        Console.WriteLine("PASS: GUI view model metadata, conversion/readback, progress, cancellation, workspace cleanup, overwrite and error states.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
