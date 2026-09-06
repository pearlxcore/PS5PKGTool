using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;

namespace PS5PKGTool.Core.Services;

public readonly record struct SonyPackageExtractProgress(long CompletedBytes, long TotalBytes, string CurrentPath);
public sealed class SonyPackageExtractResult
{
    public required string Destination { get; init; }
    public required int FileCount { get; init; }
    public required long ExtractedBytes { get; init; }
}

public static class SonyPackageExtraction
{
    public static async Task<SonyPackageExtractResult> ExtractAsync(string packagePath, string destination,
        string passcode = SonyDebugPackageCredentials.DefaultPasscode,
        IProgress<SonyPackageExtractProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        string output = Path.GetFullPath(destination);
        if (Directory.Exists(output)) throw new IOException("The extraction destination already exists: " + output);
        SonyPkgSummary package = new SonyPkgReader().Read(packagePath, passcode);
        SonyPfsSummary pfs = package.NestedPfs ?? throw new InvalidDataException("The package contains no indexed PFS image.");
        if (pfs.AccessState != SonyPfsAccessState.PlaintextIndexed || pfs.Files.Any(file => file.Extents.Count == 0))
            throw new InvalidDataException("The package contains compressed or unavailable files that cannot be extracted by the current reader.");
        long total = pfs.Files.Sum(file => file.Size); long completed = 0;
        string staging = output + ".extracting." + Guid.NewGuid().ToString("N");
        try
        {
            Directory.CreateDirectory(staging);
            foreach (SonyPfsEntry entry in pfs.Files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string target = ResolveContainedPath(staging, entry.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using var outputFile = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.Asynchronous);
                long position = 0;
                while (position < entry.Size)
                {
                    int count = checked((int)Math.Min(1024 * 1024, entry.Size - position));
                    byte[] data = SonyPfsEntryDataReader.ReadRange(packagePath, package, pfs, entry, position, count);
                    await outputFile.WriteAsync(data, cancellationToken).ConfigureAwait(false);
                    position += count; completed += count;
                    progress?.Report(new SonyPackageExtractProgress(completed, total, entry.RelativePath));
                }
            }
            Directory.Move(staging, output);
            return new SonyPackageExtractResult { Destination = output, FileCount = pfs.Files.Count, ExtractedBytes = completed };
        }
        catch
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch (IOException) { }
            throw;
        }
    }

    private static string ResolveContainedPath(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative)) throw new InvalidDataException("Package path is unsafe.");
        string rootFull = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        string target = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!target.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Package path escapes the extraction root.");
        return target;
    }
}
