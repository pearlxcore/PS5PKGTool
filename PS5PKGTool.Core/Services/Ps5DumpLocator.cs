namespace PS5PKGTool.Core.Services;

public sealed class Ps5DumpLocator
{
    public IReadOnlyList<string> FindFfpfscFiles(string selectedPath, bool recursive,
        CancellationToken cancellationToken = default) => FindFiles(selectedPath, recursive, ".ffpfsc", cancellationToken);

    public IReadOnlyList<string> FindFilesystemImageFiles(string selectedPath, bool recursive,
        CancellationToken cancellationToken = default) => FindFiles(selectedPath, recursive, ".exfat", cancellationToken);

    public IReadOnlyList<string> FindFfpkgFiles(string selectedPath, bool recursive,
        CancellationToken cancellationToken = default) => FindFiles(selectedPath, recursive, ".ffpkg", cancellationToken);

    public IReadOnlyList<string> FindPackageFiles(string selectedPath, bool recursive, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        if (File.Exists(selectedPath))
            return Path.GetExtension(selectedPath).Equals(".pkg", StringComparison.OrdinalIgnoreCase)
                ? [Path.GetFullPath(selectedPath)]
                : [];
        if (!Directory.Exists(selectedPath)) return [];

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            MatchCasing = MatchCasing.CaseInsensitive
        };
        var results = new List<string>();
        foreach (string path in Directory.EnumerateFiles(selectedPath, "*.pkg", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(Path.GetFullPath(path));
        }
        results.Sort(StringComparer.OrdinalIgnoreCase);
        return results;
    }

    private static IReadOnlyList<string> FindFiles(string selectedPath, bool recursive, string extension,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        if (File.Exists(selectedPath))
            return Path.GetExtension(selectedPath).Equals(extension, StringComparison.OrdinalIgnoreCase)
                ? [Path.GetFullPath(selectedPath)]
                : [];
        if (!Directory.Exists(selectedPath)) return [];
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            MatchCasing = MatchCasing.CaseInsensitive
        };
        var results = new List<string>();
        foreach (string path in Directory.EnumerateFiles(selectedPath, "*" + extension, options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            results.Add(Path.GetFullPath(path));
        }
        results.Sort(StringComparer.OrdinalIgnoreCase);
        return results;
    }

    public IReadOnlyList<string> FindParamFiles(string selectedPath, bool recursive, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedPath);
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (File.Exists(selectedPath))
        {
            string fileName = Path.GetFileName(selectedPath);
            string? parentDirectory = Path.GetDirectoryName(selectedPath);
            if (fileName.Equals("param.json", StringComparison.OrdinalIgnoreCase) &&
                parentDirectory is not null &&
                Path.GetFileName(parentDirectory).Equals("sce_sys", StringComparison.OrdinalIgnoreCase))
                results.Add(Path.GetFullPath(selectedPath));
            else if (fileName.Equals("eboot.bin", StringComparison.OrdinalIgnoreCase))
                AddDirect(parentDirectory!, results);
            return results.ToArray();
        }
        if (!Directory.Exists(selectedPath)) return [];

        AddDirect(selectedPath, results);
        if (!recursive || results.Count > 0) return results.ToArray();

        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            MatchCasing = MatchCasing.CaseInsensitive
        };
        foreach (string file in Directory.EnumerateFiles(selectedPath, "param.json", options))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string? parentDirectory = Path.GetDirectoryName(file);
            if (parentDirectory is not null &&
                Path.GetFileName(parentDirectory).Equals("sce_sys", StringComparison.OrdinalIgnoreCase))
                results.Add(Path.GetFullPath(file));
        }
        return results.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddDirect(string root, HashSet<string> results)
    {
        string paramPath = Path.Combine(root, "sce_sys", "param.json");
        if (File.Exists(paramPath)) results.Add(Path.GetFullPath(paramPath));
    }
}
