using System.Globalization;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Core.Parsers;

internal sealed class SonySupplementalPfsIndexReader
{
    private const int MaximumSupplementalBytes = 64 * 1024 * 1024;
    private const int MaximumXmlBytes = 16 * 1024 * 1024;

    public SonyPfsSummary? TryRead(string packagePath, SonyPkgSummary package, SonyPfsSummary outer)
    {
        SonyPfsEntry? innerImage = outer.Files.FirstOrDefault(file =>
            Path.GetFileName(file.RelativePath).Equals("pfs_image.dat", StringComparison.OrdinalIgnoreCase));
        if (innerImage is null || innerImage.Extents.Count == 0 || package.BodySize == 0) return null;
        long supplementalOffset;
        try { supplementalOffset = checked(package.ContainerOffset + (long)package.BodyOffset + (long)package.BodySize); }
        catch (OverflowException) { return null; }

        using var stream = new FileStream(packagePath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, 64 * 1024, FileOptions.RandomAccess);
        long available = stream.Length - supplementalOffset;
        if (supplementalOffset < 0 || available < 4 || available > MaximumSupplementalBytes) return null;
        stream.Position = supplementalOffset;
        byte[] supplemental = new byte[checked((int)available)];
        stream.ReadExactly(supplemental);
        if (supplemental[0] != 0x50 || supplemental[1] != 0x4B) return null;

        using var memory = new MemoryStream(supplemental, writable: false);
        using var archive = new ZipArchive(memory, ZipArchiveMode.Read, leaveOpen: false);
        ZipArchiveEntry? xmlEntry = archive.Entries.FirstOrDefault(entry =>
            entry.FullName.Equals("common/etc/pfsimage.xml", StringComparison.OrdinalIgnoreCase));
        if (xmlEntry is null || xmlEntry.Length <= 0 || xmlEntry.Length > MaximumXmlBytes) return null;
        XDocument document;
        using (Stream xmlStream = xmlEntry.Open())
        using (var reader = XmlReader.Create(xmlStream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaximumXmlBytes
        })) document = XDocument.Load(reader, LoadOptions.None);

        XElement? nested = document.Descendants("nested-image").FirstOrDefault();
        if (nested is null) return null;
        string codecStatus = string.Empty;
        bool storedIdentityLayout = false;
        SonyPfsEntry? layoutEntry = outer.Files.FirstOrDefault(file =>
            Path.GetFileName(file.RelativePath).Equals("naps_pkg_layout.dat", StringComparison.OrdinalIgnoreCase));
        if (layoutEntry is not null && layoutEntry.Size is > 0 and <= 16 * 1024 * 1024)
        {
            try
            {
                byte[] naps = SonyPfsEntryDataReader.ReadAll(packagePath, package, outer, layoutEntry, 16 * 1024 * 1024);
                SonyNapsLayout layout = SonyNapsLayoutReader.Read(naps);
                storedIdentityLayout = layout.Blocks.All(block => !block.IsRun && block.Predictor == 0);
                if (SonyOodleKrakenDecoder.IsAvailable)
                {
                    using var decoder = new SonyOodleKrakenDecoder();
                    var metadata = SonyInnerPfsMountReader.ReconstructMetadata(innerImage.Size,
                        (offset, count) => SonyPfsEntryDataReader.ReadRange(packagePath, package, outer,
                            innerImage, offset, count), naps, decoder);
                    if (SonyInnerPfsMountReader.FindSuperblock(metadata.Data) != 0)
                        throw new InvalidDataException("The decoded metadata does not begin with a PFS superblock.");
                    codecStatus = storedIdentityLayout
                        ? $" Stored NAPS metadata validated {metadata.Data.Length:N0} byte(s)."
                        : $" Kraken decoded and validated {metadata.Data.Length:N0} metadata byte(s).";
                }
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException or
                EntryPointNotFoundException or BadImageFormatException)
            {
                codecStatus = " Kraken is configured, but metadata validation failed: " + ex.Message;
            }
        }
        var files = new List<SonyPfsEntry>();
        int unavailable = 0;
        foreach (XElement file in nested.Descendants("file"))
        {
            if (!IsBelowUroot(file)) continue;
            string relativePath = BuildPath(file);
            if (relativePath.Length == 0) continue;
            long size = ReadInteger(file.Attribute("size")?.Value);
            long offset = ReadInteger(file.Attribute("offset")?.Value);
            if (size < 0 || offset < 0 || offset > innerImage.Size || size > innerImage.Size - offset) continue;
            bool raw = storedIdentityLayout || IsKnownRawFile(relativePath);
            IReadOnlyList<SonyPfsExtent> extents = raw
                ? SliceExtents(innerImage.Extents, offset, size)
                : [];
            if (!raw) unavailable++;
            files.Add(new SonyPfsEntry
            {
                RelativePath = relativePath,
                Size = size,
                StoredSize = raw ? size : 0,
                Flags = raw ? 0u : 1u,
                Extents = extents,
                UsesPlainDataSector = true
            });
        }
        if (files.Count == 0) return null;
        int readable = files.Count - unavailable;
        return new SonyPfsSummary
        {
            AccessState = SonyPfsAccessState.PlaintextIndexed,
            ImageOffset = outer.ImageOffset,
            ImageSize = outer.ImageSize,
            Version = outer.Version,
            Magic = outer.Magic,
            Mode = outer.Mode,
            BlockSize = outer.BlockSize,
            InodeCount = outer.InodeCount,
            DataBlockCount = outer.DataBlockCount,
            StatusMessage = unavailable == 0
                ? $"Indexed {files.Count:N0} inner application file(s) from the supplemental PFS tree; all are directly readable.{codecStatus}"
                : $"Indexed {files.Count:N0} inner application file(s) from the supplemental PFS tree; {readable:N0} raw file(s) are directly readable and {unavailable:N0} Kraken-compressed file(s) are not yet directly streamable.{codecStatus}",
            Files = files.OrderBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray(),
            CryptoContext = outer.CryptoContext
        };
    }

    private static bool IsBelowUroot(XElement element) => element.Ancestors("dir")
        .Any(dir => string.Equals(dir.Attribute("name")?.Value, "uroot", StringComparison.Ordinal));

    private static string BuildPath(XElement file)
    {
        var parts = file.Ancestors("dir").Reverse()
            .Select(dir => dir.Attribute("name")?.Value ?? string.Empty)
            .Where(name => name.Length > 0 && !name.Equals("uroot", StringComparison.Ordinal))
            .ToList();
        string name = file.Attribute("name")?.Value ?? string.Empty;
        if (name.Length > 0) parts.Add(name);
        return string.Join('/', parts);
    }

    private static bool IsKnownRawFile(string path)
    {
        string name = Path.GetFileName(path);
        if (name.Equals("keystone", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("eboot.bin", StringComparison.OrdinalIgnoreCase)) return true;
        string extension = Path.GetExtension(name);
        return extension.Equals(".elf", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".prx", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".sprx", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".self", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<SonyPfsExtent> SliceExtents(IReadOnlyList<SonyPfsExtent> source,
        long offset, long size)
    {
        var result = new List<SonyPfsExtent>();
        long skip = offset;
        long remaining = size;
        foreach (SonyPfsExtent extent in source)
        {
            if (skip >= extent.Length) { skip -= extent.Length; continue; }
            long take = Math.Min(remaining, extent.Length - skip);
            result.Add(new SonyPfsExtent(checked(extent.Offset + skip), take));
            remaining -= take;
            skip = 0;
            if (remaining == 0) break;
        }
        return remaining == 0 ? result : [];
    }

    private static long ReadInteger(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return -1;
        return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? long.TryParse(text.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out long hex) ? hex : -1
            : long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long value) ? value : -1;
    }
}
