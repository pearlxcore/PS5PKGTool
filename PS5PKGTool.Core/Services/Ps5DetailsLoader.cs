using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;

namespace PS5PKGTool.Core.Services;

public sealed class Ps5DetailsLoader
{
    public Task<Ps5GameDetails> LoadAsync(Ps5GameInfo game, CancellationToken cancellationToken = default,
        IProgress<Ps5Artwork>? artwork = null) =>
        Task.Run(() => Load(game, cancellationToken, artwork), cancellationToken);

    private static Ps5GameDetails Load(Ps5GameInfo game, CancellationToken cancellationToken,
        IProgress<Ps5Artwork>? artwork)
    {
        if (game.SourceKind == Ps5SourceKind.SonyPackage)
            return LoadPackage(game, cancellationToken, artwork);

        using IReadOnlyGameFileSystem files = GameFileSystem.Open(game, cancellationToken);
        var errors = new List<string>();

        // Artwork first: the icon is a cheap PNG, so report it immediately, then report the
        // backgrounds once decoded, before the heavier trophy/activity/executable/inventory work.
        byte[]? iconPng = ReadOptional(files, "sce_sys/icon0.png");
        Ps5ImageData? icon = iconPng is null ? null : Ps5ImageData.FromPng(iconPng);
        artwork?.Report(new Ps5Artwork(icon, null, null, null));
        Ps5ImageData? pic0 = ReadBackground(files, 0, errors);
        Ps5ImageData? pic1 = ReadBackground(files, 1, errors);
        Ps5ImageData? pic2 = ReadBackground(files, 2, errors);
        artwork?.Report(new Ps5Artwork(icon, pic0, pic1, pic2));

        Ps5TrophySet? trophy = null;
        Ps5UdsSummary? uds = null;
        Ps5SelfInfo? executable = null;
        try { trophy = new Ps5TrophyReader().Read(files, game.DefaultLanguage, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Trophies: " + ex.Message); }
        try { uds = new Ps5UdsReader().Read(files, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Activities: " + ex.Message); }
        try { executable = new Ps5SelfReader().Read(files, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Executable: " + ex.Message); }

        Ps5FileInventory inventory = ReadFiles(files, cancellationToken);
        Ps5GameDetails details = new()
        {
            TrophySet = trophy,
            Uds = uds,
            Executable = executable,
            Files = inventory,
            Icon = icon,
            Background = pic0,
            Background1 = pic1,
            Background2 = pic2,
            Errors = errors
        };
        // The size of a loose dump is not computed during the scan (that would walk every dump
        // folder). Fill it in here, where the inventory walk already produced the total.
        if (game.SourceKind == Ps5SourceKind.LooseDump && game.SourceSize <= 0 && details.Files.TotalSize > 0)
            game.SourceSize = details.Files.TotalSize;
        return details;
    }

    private static Ps5GameDetails LoadPackage(Ps5GameInfo game, CancellationToken cancellationToken,
        IProgress<Ps5Artwork>? artwork)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var errors = new List<string>();
        var reader = new SonyPkgReader();
        SonyPkgSummary package = ResolvePackage(game, reader);
        Ps5FileInventory files;
        Ps5ImageData? icon;

        using (IReadOnlyGameFileSystem fileSystem = GameFileSystem.Open(game, cancellationToken))
        {
            // Report the icon as soon as the file system opens, before the inventory/asset decode.
            byte[]? iconPng = ReadOptional(fileSystem, "sce_sys/icon0.png");
            icon = iconPng is not null
                ? Ps5ImageData.FromPng(iconPng)
                : ReadPackageImage(reader, game.RootPath, package, 0x1200, 0x1280, "icon", errors);
            artwork?.Report(new Ps5Artwork(icon, null, null, null));

            Ps5FileInventory readable = ReadFiles(fileSystem, cancellationToken);
            if (readable.FileCount > 0)
                return LoadReadablePackage(game, package, reader, fileSystem, readable, icon,
                    cancellationToken, errors, artwork);
        }

        files = ReadPackageEntries(package);

        Ps5ImageData? pic0 = ReadPackageImage(reader, game.RootPath, package, 0x1220, 0x12A0, "PIC0", errors);
        Ps5ImageData? pic1 = ReadPackageImage(reader, game.RootPath, package, null, 0x12C0, "PIC1", errors);
        artwork?.Report(new Ps5Artwork(icon, pic0, pic1, null));
        if (package.NestedPfs is { } nested && nested.AccessState != SonyPfsAccessState.NotPresent)
            errors.Add(nested.StatusMessage);

        return new Ps5GameDetails
        {
            Files = files,
            Icon = icon,
            Background = pic0,
            Background1 = pic1,
            Errors = errors
        };
    }

    private static Ps5GameDetails LoadReadablePackage(Ps5GameInfo game, SonyPkgSummary package,
        SonyPkgReader packageReader, IReadOnlyGameFileSystem fileSystem, Ps5FileInventory inventory,
        Ps5ImageData? icon, CancellationToken cancellationToken, List<string> errors,
        IProgress<Ps5Artwork>? artwork)
    {
        Ps5ImageData? pic0 = ReadBackground(fileSystem, 0, errors) ??
            ReadPackageImage(packageReader, game.RootPath, package, 0x1220, 0x12A0, "PIC0", errors);
        Ps5ImageData? pic1 = ReadBackground(fileSystem, 1, errors) ??
            ReadPackageImage(packageReader, game.RootPath, package, null, 0x12C0, "PIC1", errors);
        Ps5ImageData? pic2 = ReadBackground(fileSystem, 2, errors);
        artwork?.Report(new Ps5Artwork(icon, pic0, pic1, pic2));

        Ps5TrophySet? trophy = null;
        Ps5UdsSummary? uds = null;
        Ps5SelfInfo? executable = null;
        try { trophy = new Ps5TrophyReader().Read(fileSystem, game.DefaultLanguage, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Trophies: " + ex.Message); }
        try { uds = new Ps5UdsReader().Read(fileSystem, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Activities: " + ex.Message); }
        try { executable = new Ps5SelfReader().Read(fileSystem, cancellationToken); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { errors.Add("Executable: " + ex.Message); }

        return new Ps5GameDetails
        {
            TrophySet = trophy,
            Uds = uds,
            Executable = executable,
            Files = inventory,
            Icon = icon,
            Background = pic0,
            Background1 = pic1,
            Background2 = pic2,
            Errors = errors
        };
    }

    private static SonyPkgSummary ResolvePackage(Ps5GameInfo game, SonyPkgReader reader)
    {
        // A package restored from the cached manifest has no live engine access (the field is
        // internal and therefore not serialized), so re-open it before reading inner files.
        SonyPkgSummary? cached = game.Package;
        if (cached is not null && (cached.NestedPfs is null || cached.NestedPfs.EngineAccess is not null))
            return cached;
        SonyPkgSummary fresh = reader.Read(game.RootPath);
        game.Package = fresh;
        return fresh;
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
                Offset = checked((long)package.EmbeddedCntOffset + entry.DataOffset),
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

    private static Ps5ImageData? ReadPackageImage(SonyPkgReader reader, string path, SonyPkgSummary package,
        uint? pngId, uint? ddsId, string description, List<string> errors)
    {
        const int maximumImageBytes = 128 * 1024 * 1024;
        try
        {
            SonyPkgEntry? png = pngId.HasValue
                ? package.Entries.FirstOrDefault(entry => entry.Id == pngId && !entry.IsEncrypted)
                : null;
            if (png is not null)
                return Ps5ImageData.FromPng(reader.ReadEntryBytes(path, package, png, maximumImageBytes));

            SonyPkgEntry? dds = ddsId.HasValue
                ? package.Entries.FirstOrDefault(entry => entry.Id == ddsId && !entry.IsEncrypted)
                : null;
            if (dds is null) return null;
            byte[] ddsBytes = reader.ReadEntryBytes(path, package, dds, maximumImageBytes);
            using var input = new MemoryStream(ddsBytes, writable: false);
            (byte[] rgba, int width, int height) = Ps5ImageCodec.DecodeDdsToRgba(input);
            return Ps5ImageData.FromRgba(rgba, width, height);
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

    private static Ps5ImageData? ReadBackground(IReadOnlyGameFileSystem files, int index, List<string> errors)
    {
        string basePath = $"sce_sys/pic{index}";
        byte[]? png = ReadOptional(files, basePath + ".png");
        if (png is not null) return Ps5ImageData.FromPng(png);

        string ddsPath = basePath + ".dds";
        if (!files.FileExists(ddsPath)) return null;
        try
        {
            using Stream input = files.OpenRead(ddsPath);
            (byte[] rgba, int width, int height) = Ps5ImageCodec.DecodeDdsToRgba(input);
            return Ps5ImageData.FromRgba(rgba, width, height);
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
