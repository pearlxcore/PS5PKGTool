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
        IProgress<Ps5ScanProgress>? progress = null, CancellationToken cancellationToken = default) =>
        Task.Run(() => Scan(libraryFolders, recursive, progress, cancellationToken), cancellationToken);

    private Ps5ScanResult Scan(IEnumerable<string> libraryFolders, bool recursive,
        IProgress<Ps5ScanProgress>? progress, CancellationToken cancellationToken)
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
                foreach (string path in _locator.FindParamFiles(folder, recursive, cancellationToken)) paramPaths.Add(path);
                foreach (string path in _locator.FindPackageFiles(folder, recursive, cancellationToken)) packagePaths.Add(path);
                foreach (string path in _locator.FindFfpfscFiles(folder, recursive, cancellationToken)) ffpfscPaths.Add(path);
                foreach (string path in _locator.FindFilesystemImageFiles(folder, recursive, cancellationToken))
                    filesystemImagePaths.Add(path);
                foreach (string path in _locator.FindFfpkgFiles(folder, recursive, cancellationToken)) ffpkgPaths.Add(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                result.Errors.Add($"{folder}: {ex.Message}");
            }
        }

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
                Ps5GameInfo game = kind switch
                {
                    Ps5SourceKind.SonyPackage => _packageReader.Read(path),
                    Ps5SourceKind.Ffpfsc => _ffpfscReader.Read(path),
                    Ps5SourceKind.FilesystemImage => _filesystemImageReader.Read(path),
                    Ps5SourceKind.Ffpkg => _ffpkgReader.Read(path),
                    _ => _paramReader.Read(path)
                };
                if (kind == Ps5SourceKind.LooseDump)
                    game.SourceSize = CalculateDirectorySize(game.RootPath, cancellationToken);
                result.Games.Add(game);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
            {
                result.Errors.Add($"{path}: {ex.Message}");
            }
            processed++;
        }
        progress?.Report(new Ps5ScanProgress { Processed = processed, Total = sources.Length });
        result.Games.Sort((left, right) => StringComparer.CurrentCultureIgnoreCase.Compare(left.Title, right.Title));
        return result;
    }

    private static long CalculateDirectorySize(string rootPath, CancellationToken cancellationToken)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
        long total = 0;
        foreach (string filePath in Directory.EnumerateFiles(rootPath, "*", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                total = checked(total + new FileInfo(filePath).Length);
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }
        return total;
    }
}
