using ProsperoPkgTool;
using ProsperoPkgTool.Content;
using ProsperoPkgTool.Containers;
using ProsperoPkgTool.Gp5;
using EngineBuilder = ProsperoPkgTool.Containers.ProsperoDebugPackageBuilder;
using EngineInnerFile = ProsperoPkgTool.Containers.ProsperoInnerImageAssembler.InnerFile;

namespace PS5PKGTool.Core.Builders;

public sealed class ProsperoDebugPackageBuildOptions
{
    public required string ContentId { get; init; }
    public string Passcode { get; init; } = SonyDebugPackageCredentials.DefaultPasscode;

    /// <summary>
    /// Optional 16-byte outer-PFS seed. When supplied the build is byte-reproducible
    /// (deterministic CNT entry keys, fixed outer PFS seed); otherwise the engine randomizes.
    /// </summary>
    public byte[]? Seed { get; init; }

    /// <summary>Optional full 64-bit executable SDK id to stamp (param.json + .sceversion).
    /// Null preserves the source metadata.</summary>
    public ulong? SdkVersionOverride { get; init; }

    /// <summary>Optional workspace folder for the multi-GiB file-backed path. Null uses the system temp folder.</summary>
    public string? TempDirectory { get; init; }

    /// <summary>Optional sink for engine log lines (free-space warnings, stale workspace sweep).</summary>
    public Action<string>? Log { get; init; }
}

/// <summary>
/// Builds a PS5 debug package from an unpacked dump folder using the vendored, validated
/// <c>ProsperoPkgTool</c> engine (MIT) as a managed library. Files are passed to the engine's
/// <c>Build</c> API directly (disk-backed by path).
/// </summary>
public static class ProsperoDebugPackageBuilder
{
    public static Task<SonyDebugPackageBuildResult> CreateFromDirectoryAsync(string sourceDirectory,
        string outputPath, ProsperoDebugPackageBuildOptions options,
        IProgress<SonyDebugPackageProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);
        return Task.Run(() => CreateFromDirectory(sourceDirectory, outputPath, options, progress, cancellationToken),
            cancellationToken);
    }

    private static SonyDebugPackageBuildResult CreateFromDirectory(string sourceDirectory, string outputPath,
        ProsperoDebugPackageBuildOptions options, IProgress<SonyDebugPackageProgress>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string source = Path.GetFullPath(sourceDirectory);
        string destination = Path.GetFullPath(outputPath);
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);

        SonyDebugPackageCredentials credentials = SonyDebugPackageCredentials.Create(options.ContentId, options.Passcode);
        progress?.Report(new SonyDebugPackageProgress("Building PS5 debug package", 0, 0, Path.GetFileName(destination)));

        var files = new List<EngineInnerFile>();
        var sceSys = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        long sourceBytes = 0;
        int sourceFiles = 0;

        foreach (string path in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
                     .OrderBy(item => item, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string relative = Path.GetRelativePath(source, path).Replace('\\', '/');
            sourceBytes += new FileInfo(path).Length;
            sourceFiles++;

            bool publishing = SourceFolderValidator.IsPublishingArtifact(relative) ||
                Path.GetExtension(relative).Equals(".gp4", StringComparison.OrdinalIgnoreCase);
            bool underSceSys = relative.StartsWith("sce_sys/", StringComparison.OrdinalIgnoreCase);
            string sceRelative = underSceSys ? relative["sce_sys/".Length..] : string.Empty;

            if (underSceSys)
            {
                byte[] data = File.ReadAllBytes(path);
                sceSys[sceRelative] = ProsperoUcpArchive.IsRepairableSceSysPath(sceRelative)
                    ? ProsperoUcpArchive.RepairIfNeeded(data)
                    : data;
            }

            if (publishing) continue;
            if (underSceSys && ProsperoSceSysMedia.IsOuterCntFile(sceRelative)) continue;

            if (underSceSys && ProsperoUcpArchive.IsRepairableSceSysPath(sceRelative))
            {
                files.Add(new EngineInnerFile
                {
                    Path = "/" + relative,
                    Data = sceSys[sceRelative],
                    PlayGoChunkId = 0
                });
                continue;
            }

            files.Add(new EngineInnerFile
            {
                Path = "/" + relative,
                SourcePath = path,
                Length = new FileInfo(path).Length,
                PlayGoChunkId = 0
            });
        }

        if (!sceSys.TryGetValue("param.json", out byte[]? paramJson) || paramJson.Length == 0)
            throw new InvalidDataException(
                "The dump does not contain sce_sys/param.json, which is required to build a package.");
        if (files.Count == 0)
            throw new InvalidDataException("The dump does not contain any package files.");

        var log = new DebugPackageBuildLog
        {
            Log = options.Log ?? (_ => { }),
            Progress = new RelayProgress<ProsperoBuildProgress>(value =>
                progress?.Report(new SonyDebugPackageProgress(value.Stage, value.Done, value.Total, string.Empty)))
        };

        DebugPackageBuildResult result = EngineBuilder.Build(files, new DebugPackageBuildOptions
        {
            OutputPath = destination,
            ContentId = credentials.ContentId,
            Passcode = credentials.Passcode,
            ParamJson = paramJson,
            Compression = ProsperoInnerCompressionMode.Stored,
            OuterSeed = options.Seed,
            DeterministicEntryKeys = options.Seed is { Length: 16 },
            SdkVersionOverride = options.SdkVersionOverride,
            TempDirectory = options.TempDirectory,
            SceSysFiles = sceSys,
            Log = log
        });

        return new SonyDebugPackageBuildResult
        {
            OutputPath = destination,
            ContentId = credentials.ContentId,
            KeyFingerprint = credentials.Fingerprint,
            PackageSize = new FileInfo(destination).Length,
            SourceBytes = sourceBytes,
            SourceFiles = sourceFiles,
            UsesDefaultPasscode = credentials.UsesDefaultPasscode
        };
    }

    private sealed class RelayProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
