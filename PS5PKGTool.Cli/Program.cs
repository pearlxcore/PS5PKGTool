using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Ffpfsc;

return await CliCommand.RunAsync(args);

public static class CliCommand
{
    public static async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length != 3 || args[0] is not ("build-exfat" or "convert"))
        {
            Console.Error.WriteLine("Usage: ps5pkgtool build-exfat <source-directory> <output.exfat>");
            Console.Error.WriteLine("       ps5pkgtool convert <input.pkg> <output.exfat>");
            return args is ["--help"] or ["-h"] ? 0 : 2;
        }

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        int interrupts = 0;
        ConsoleCancelEventHandler cancel = (_, e) =>
        {
            e.Cancel = Interlocked.Increment(ref interrupts) == 1;
            if (e.Cancel) cancellation.Cancel();
        };
        Console.CancelKeyPress += cancel;
        try
        {
            string source = Path.GetFullPath(args[1]);
            string output = Path.GetFullPath(args[2]);
            if (File.Exists(output)) throw new IOException($"The output file already exists: {output}");

            var progress = new ConsoleProgress();
            Console.Error.WriteLine($"Scanning {source}");
            cancellation.Token.ThrowIfCancellationRequested();
            if (args[0] == "convert")
            {
                if (new SonyPkgGameReader().Read(source).Package is not { Kind: SonyPkgKind.FinalizedDebug, PfsImageSize: > 0 })
                    throw new InvalidDataException("Only finalized debug/fake PKGs are supported by the CLI; patch packages require a reference package.");
                cancellation.Token.ThrowIfCancellationRequested();
                Ps5ImageConversionResult converted = await SonyPackageImageConversion.ConvertAsync(source, output, Ps5ImageConversionTarget.Exfat,
                    progress: progress, cancellationToken: cancellation.Token);
                Console.WriteLine($"Verified: {converted.OutputPath} ({converted.OutputBytes} bytes, {converted.FileCount} files)");
                return 0;
            }
            else
            {
                if (!Directory.Exists(source)) throw new DirectoryNotFoundException($"Source directory not found: {source}");
                string relative = Path.GetRelativePath(source, output);
                if (!Path.IsPathRooted(relative) && relative != ".." &&
                    !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                    throw new IOException("The output must be outside the source directory.");
                await ExfatImage.WriteDirectoryAsync(source, output, progress: progress,
                    cancellationToken: cancellation.Token);
            }
            ExfatVerificationResult result = await ExfatImage.VerifyAsync(output, progress, cancellation.Token);
            Console.WriteLine($"Verified: {result.ImagePath} ({result.ImageSize} bytes, {result.FileCount} files)");
            Console.WriteLine($"Manifest SHA-256: {result.ManifestSha256}");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled.");
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error ({ex.GetType().Name}): {ex.Message}");
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
        }
    }

    private sealed class ConsoleProgress : IProgress<FfpfscProgress>, IProgress<Ps5ImageConversionProgress>
    {
        private string? _stage;
        private int _percent = -1;

        public void Report(Ps5ImageConversionProgress value) =>
            Report(new FfpfscProgress(value.Stage, value.Completed, value.Total));

        public void Report(FfpfscProgress value)
        {
            int percent = value.TotalBytes == 0 ? 100 : (int)(100.0 * value.BytesProcessed / value.TotalBytes);
            if (value.Stage == _stage && percent == _percent) return;
            _stage = value.Stage;
            _percent = percent;
            Console.Error.WriteLine($"{value.Stage}: {percent}% ({value.BytesProcessed}/{value.TotalBytes} bytes)");
        }
    }
}
