using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;

namespace PS5PKGTool.Core.Services;

public sealed class Ps5DetailsLoader
{
    public Task<Ps5GameDetails> LoadAsync(Ps5GameInfo game, CancellationToken cancellationToken = default) =>
        Task.Run(() => Load(game, cancellationToken), cancellationToken);

    private static Ps5GameDetails Load(Ps5GameInfo game, CancellationToken cancellationToken)
    {
        if (game.SourceKind == Ps5SourceKind.SonyPackage)
            return LoadPackage(game, cancellationToken);

        using IReadOnlyGameFileSystem files = GameFileSystem.Open(game, cancellationToken);
        Ps5TrophySet? trophy = null;
        Ps5UdsSummary? uds = null;
        Ps5SelfInfo? executable = null;
        var errors = new List<string>();
        try { trophy = new Ps5TrophyReader().Read(files, game.DefaultLanguage, cancellationToken); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException)
        { errors.Add("Trophies: " + ex.Message); }
        try { uds = new Ps5UdsReader().Read(files, cancellationToken); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException)
        { errors.Add("Activities: " + ex.Message); }
        try { executable = new Ps5SelfReader().Read(files, cancellationToken); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        { errors.Add("Executable: " + ex.Message); }

        Ps5FileInventory inventory = ReadFiles(files, cancellationToken);
        return new Ps5GameDetails
        {
            TrophySet = trophy,
            Uds = uds,
            Executable = executable,
            Files = inventory,
            IconPng = ReadOptional(files, "sce_sys/icon0.png"),
            BackgroundPng = ReadBackground(files, 0, errors),
            Background1Png = ReadBackground(files, 1, errors),
            Background2Png = ReadBackground(files, 2, errors),
            Errors = errors
        };
    }

    private static Ps5GameDetails LoadPackage(Ps5GameInfo game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var errors = new List<string>();
        var reader = new SonyPkgReader();
        SonyPkgSummary package = game.Package ?? reader.Read(game.RootPath);
        Ps5FileInventory files;

        if (package.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed)
        {
            using IReadOnlyGameFileSystem fileSystem = GameFileSystem.Open(game, cancellationToken);
            files = ReadFiles(fileSystem, cancellationToken);
            return LoadReadablePackage(game, package, reader, fileSystem, files, cancellationToken, errors);
        }

        files = ReadPackageEntries(package);

        byte[]? icon = ReadPackageImage(reader, game.RootPath, package, 0x1200, 0x1280, "icon", errors);
        byte[]? pic0 = ReadPackageImage(reader, game.RootPath, package, 0x1220, 0x12A0, "PIC0", errors);
        byte[]? pic1 = ReadPackageImage(reader, game.RootPath, package, null, 0x12C0, "PIC1", errors);
        if (package.NestedPfs is { } nested && nested.AccessState != SonyPfsAccessState.NotPresent)
            errors.Add(nested.StatusMessage);

        return new Ps5GameDetails
        {
            Files = files,
            IconPng = icon,
            BackgroundPng = pic0,
            Background1Png = pic1,
            Errors = errors
        };
    }

    private static Ps5GameDetails LoadReadablePackage(Ps5GameInfo game, SonyPkgSummary package,
        SonyPkgReader packageReader, IReadOnlyGameFileSystem fileSystem, Ps5FileInventory inventory,
        CancellationToken cancellationToken, List<string> errors)
    {
        Ps5TrophySet? trophy = null;
        Ps5UdsSummary? uds = null;
        Ps5SelfInfo? executable = null;
        try { trophy = new Ps5TrophyReader().Read(fileSystem, game.DefaultLanguage, cancellationToken); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or System.Text.Json.JsonException)
        { errors.Add("Trophies: " + ex.Message); }
        try { uds = new Ps5UdsReader().Read(fileSystem, cancellationToken); }
        catch (Exception ex) when (ex is IOException or InvalidDataException or System.Text.Json.JsonException)
        { errors.Add("Activities: " + ex.Message); }
        try { executable = new Ps5SelfReader().Read(fileSystem, cancellationToken); }
        catch (Exception ex) when (ex is IOException or InvalidDataException)
        { errors.Add("Executable: " + ex.Message); }

        byte[]? icon = ReadOptional(fileSystem, "sce_sys/icon0.png") ??
            ReadPackageImage(packageReader, game.RootPath, package, 0x1200, 0x1280, "icon", errors);
        byte[]? pic0 = ReadBackground(fileSystem, 0, errors) ??
            ReadPackageImage(packageReader, game.RootPath, package, 0x1220, 0x12A0, "PIC0", errors);
        byte[]? pic1 = ReadBackground(fileSystem, 1, errors) ??
            ReadPackageImage(packageReader, game.RootPath, package, null, 0x12C0, "PIC1", errors);

        return new Ps5GameDetails
        {
            TrophySet = trophy,
            Uds = uds,
            Executable = executable,
            Files = inventory,
            IconPng = icon,
            BackgroundPng = pic0,
            Background1Png = pic1,
            Background2Png = ReadBackground(fileSystem, 2, errors),
            Errors = errors
        };
    }

    private static Ps5FileInventory ReadPackageEntries(SonyPkgSummary package)
    {
        Ps5FileInfo[] entries = package.Entries.Select(entry =>
        {
            string path = entry.DisplayName.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Trim(Path.DirectorySeparatorChar);
            if (string.IsNullOrWhiteSpace(path)) path = $"entry_0x{entry.Id:X4}.bin";
            return new Ps5FileInfo
            {
                RelativePath = path,
                Extension = Path.GetExtension(path),
                Size = entry.DataSize,
                Offset = checked(package.ContainerOffset + entry.DataOffset),
                PackageEntryId = entry.Id,
                IsEncrypted = entry.IsEncrypted
            };
        }).OrderBy(entry => entry.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray();

        return new Ps5FileInventory
        {
            TotalSize = package.FileSize,
            FileCount = entries.Length,
            Files = entries,
            LargestFiles = entries.OrderByDescending(entry => entry.Size).Take(500).ToArray()
        };
    }

    private static byte[]? ReadPackageImage(SonyPkgReader reader, string path, SonyPkgSummary package,
        uint? pngId, uint? ddsId, string description, List<string> errors)
    {
        const int maximumImageBytes = 128 * 1024 * 1024;
        try
        {
            SonyPkgEntry? png = pngId.HasValue
                ? package.Entries.FirstOrDefault(entry => entry.Id == pngId && !entry.IsEncrypted)
                : null;
            if (png is not null) return reader.ReadEntryBytes(path, package, png, maximumImageBytes);

            SonyPkgEntry? dds = ddsId.HasValue
                ? package.Entries.FirstOrDefault(entry => entry.Id == ddsId && !entry.IsEncrypted)
                : null;
            if (dds is null) return null;
            byte[] ddsBytes = reader.ReadEntryBytes(path, package, dds, maximumImageBytes);
            using var input = new MemoryStream(ddsBytes, writable: false);
            return Ps5ImageCodec.DecodeDdsToPng(input);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            errors.Add($"Package artwork {description}: {ex.Message}");
            return null;
        }
    }

    private static Ps5FileInventory ReadFiles(IReadOnlyGameFileSystem fileSystem,
        CancellationToken cancellationToken)
    {
        var largest = new List<Ps5FileInfo>();
        long totalSize = 0;
        int count = 0;
        foreach (GameFileRecord file in fileSystem.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            totalSize = checked(totalSize + file.Size);
            count++;
            string relativePath = file.RelativePath.Replace('/', Path.DirectorySeparatorChar);
            largest.Add(new Ps5FileInfo
            {
                RelativePath = relativePath,
                Extension = Path.GetExtension(relativePath),
                Size = file.Size
            });
        }
        return new Ps5FileInventory
        {
            TotalSize = totalSize,
            FileCount = count,
            Files = largest.OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray(),
            LargestFiles = largest.OrderByDescending(file => file.Size).Take(500).ToArray()
        };
    }

    private static byte[]? ReadBackground(IReadOnlyGameFileSystem files, int index, List<string> errors)
    {
        string basePath = $"sce_sys/pic{index}";
        byte[]? png = ReadOptional(files, basePath + ".png");
        if (png is not null) return png;

        string ddsPath = basePath + ".dds";
        if (!files.FileExists(ddsPath)) return null;
        try
        {
            using Stream input = files.OpenRead(ddsPath);
            return Ps5ImageCodec.DecodeDdsToPng(input);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
        {
            errors.Add($"Artwork PIC{index}: {ex.Message}");
            return null;
        }
    }

    private static byte[]? ReadOptional(IReadOnlyGameFileSystem files, string path)
    {
        const int maximumBytes = 128 * 1024 * 1024;
        try
        {
            if (!files.FileExists(path)) return null;
            using Stream input = files.OpenRead(path);
            if (input.Length > maximumBytes || input.Length > int.MaxValue) return null;
            byte[] result = new byte[checked((int)input.Length)];
            input.ReadExactly(result);
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }
}
