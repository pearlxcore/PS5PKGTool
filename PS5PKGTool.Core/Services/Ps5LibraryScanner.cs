using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using System.Text.Json;

namespace PS5PKGTool.Core.Services;

public sealed class Ps5ScanProgress
{
    public int Processed { get; init; }
    public int Total { get; init; }
    public string CurrentPath { get; init; } = string.Empty;
}

public sealed class Ps5ScanResult
{
    public List<Ps5GameInfo> Games { get; } = [];
    public List<string> Errors { get; } = [];
}

public sealed class Ps5LibraryScanner
{
    private readonly Ps5DumpLocator _locator = new();
    private readonly Ps5ParamReader _paramReader = new();
    private readonly SonyPkgGameReader _packageReader = new();
    private readonly FfpfscGameReader _ffpfscReader = new();
    private readonly FilesystemImageGameReader _filesystemImageReader = new();
    private readonly FfpkgGameReader _ffpkgReader = new();

    public Task<Ps5ScanResult> ScanAsync(IEnumerable<string> libraryFolders, bool recursive,
        IReadOnlyList<Ps5GameInfo>? cached = null,
        IProgress<Ps5ScanProgress>? progress = null, CancellationToken cancellationToken = default) =>
        Task.Run(() => Scan(libraryFolders, recursive, cached, progress, cancellationToken), cancellationToken);

    private Ps5ScanResult Scan(IEnumerable<string> libraryFolders, bool recursive,
        IReadOnlyList<Ps5GameInfo>? cached, IProgress<Ps5ScanProgress>? progress, CancellationToken cancellationToken)
    {
        var result = new Ps5ScanResult();
        var paramPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var packagePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ffpfscPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var filesystemImagePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ffpkgPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string folder in libraryFolders.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // One traversal per folder; every candidate kind is classified from the same walk.
                Ps5DumpLocator.LocatedSources located = _locator.FindAll(folder, recursive, cancellationToken);
                foreach (string path in located.ParamFiles) paramPaths.Add(path);
                foreach (string path in located.Packages) packagePaths.Add(path);
                foreach (string path in located.Ffpfsc) ffpfscPaths.Add(path);
                foreach (string path in located.FilesystemImages) filesystemImagePaths.Add(path);
                foreach (string path in located.Ffpkg) ffpkgPaths.Add(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                result.Errors.Add($"{folder}: {ex.Message}");
            }
        }

        Dictionary<string, Ps5GameInfo> cachedByRoot = cached is null
            ? new Dictionary<string, Ps5GameInfo>(StringComparer.OrdinalIgnoreCase)
            : cached.Where(game => !string.IsNullOrWhiteSpace(game.RootPath))
                .GroupBy(game => game.RootPath, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

        int processed = 0;
        var sources = paramPaths.Select(path => (Path: path, Kind: Ps5SourceKind.LooseDump))
            .Concat(packagePaths.Select(path => (Path: path, Kind: Ps5SourceKind.SonyPackage)))
            .Concat(ffpfscPaths.Select(path => (Path: path, Kind: Ps5SourceKind.Ffpfsc)))
            .Concat(filesystemImagePaths.Select(path => (Path: path, Kind: Ps5SourceKind.FilesystemImage)))
            .Concat(ffpkgPaths.Select(path => (Path: path, Kind: Ps5SourceKind.Ffpkg)))
            .OrderBy(source => source.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        foreach ((string path, Ps5SourceKind kind) in sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new Ps5ScanProgress { Processed = processed, Total = sources.Length, CurrentPath = path });
            try
            {
                // Reuse the cached metadata when the source is unchanged (same root and timestamp),
                // so a refresh only re-parses new or modified sources.
                string rootPath = ResolveRootPath(path, kind);
                DateTime stamp = ReadStamp(path);
                if (cachedByRoot.TryGetValue(rootPath, out Ps5GameInfo? reuse) &&
                    reuse.SourceKind == kind && reuse.LastWriteTimeUtc == stamp)
                {
                    result.Games.Add(reuse);
                    processed++;
                    continue;
                }

                Ps5GameInfo game = kind switch
                {
                    Ps5SourceKind.SonyPackage => _packageReader.Read(path),
                    Ps5SourceKind.Ffpfsc => _ffpfscReader.Read(path),
                    Ps5SourceKind.FilesystemImage => _filesystemImageReader.Read(path),
                    Ps5SourceKind.Ffpkg => _ffpkgReader.Read(path),
                    _ => _paramReader.Read(path)
                };
                result.Games.Add(game);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                // A source the strict readers reject (no readable param.json, or several game roots)
                // is still listed as a structure-only record so it can be inspected rather than dropped.
                Ps5GameInfo? structure = TryReadStructure(kind, path);
                if (structure is not null)
                {
                    structure.DataWarnings.Add("Structure-only view: " + ex.Message);
                    result.Games.Add(structure);
                }
                else
                {
                    result.Errors.Add($"{path}: {ex.Message}");
                }
            }
            processed++;
        }
        progress?.Report(new Ps5ScanProgress { Processed = processed, Total = sources.Length });
        result.Games.Sort((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Title, right.Title));
        return result;
    }

    /// <summary>Structure-only fallback for a source the strict readers rejected; null when it cannot open.</summary>
    private Ps5GameInfo? TryReadStructure(Ps5SourceKind kind, string path)
    {
        try
        {
            return kind switch
            {
                Ps5SourceKind.Ffpfsc => _ffpfscReader.ReadStructure(path),
                Ps5SourceKind.FilesystemImage => _filesystemImageReader.ReadStructure(path),
                Ps5SourceKind.Ffpkg => _ffpkgReader.ReadStructure(path),
                _ => null
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            return null;
        }
    }

    private static string ResolveRootPath(string path, Ps5SourceKind kind)
    {
        if (kind != Ps5SourceKind.LooseDump) return path;
        string? sceSys = Path.GetDirectoryName(path);
        string? root = sceSys is null ? null : Path.GetDirectoryName(sceSys);
        return string.IsNullOrEmpty(root) ? path : root;
    }

    private static DateTime ReadStamp(string path)
    {
        try { return File.GetLastWriteTimeUtc(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return DateTime.MinValue; }
    }
}
