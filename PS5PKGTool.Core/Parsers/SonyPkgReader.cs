using System.Buffers.Binary;
using System.Text;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Core.Parsers;

/// <summary>
/// Reads the public CNT/FIH metadata layer of a Sony PS5 package. The reader never
/// materializes the package or PFS image in memory; every allocation has a fixed limit.
/// </summary>
public sealed class SonyPkgReader
{
    private const int CntHeaderSize = 0x5A0;
    private const int FihHeaderReadSize = 0x100;
    private const int EntryRecordSize = 0x20;
    private const int MaximumEntryCount = 0x10000;
    private const int MaximumNameTableSize = 16 * 1024 * 1024;

    private static ReadOnlySpan<byte> CntMagic => [0x7F, (byte)'C', (byte)'N', (byte)'T'];
    private static ReadOnlySpan<byte> FihMagic => [0x7F, (byte)'F', (byte)'I', (byte)'H'];

    public bool IsSonyPackage(string path)
    {
        try
        {
            using FileStream stream = Open(path);
            Span<byte> magic = stackalloc byte[4];
            if (stream.Length < magic.Length || stream.Read(magic) != magic.Length) return false;
            return magic.SequenceEqual(CntMagic) || magic.SequenceEqual(FihMagic);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
    }

    public SonyPkgSummary Read(string path) => Read(path, null);

    public SonyPkgSummary Read(string path, string? debugPasscode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using FileStream stream = Open(path);
        if (stream.Length < 4) throw new InvalidDataException("The file is too small to be a PS5 package.");

        byte[] magic = ReadAt(stream, 0, 4, "package magic");
        if (magic.AsSpan().SequenceEqual(CntMagic))
            return ReadCnt(stream, SonyPkgKind.MetadataContainer, 0, null, null, 0, 0, 0);
        if (!magic.AsSpan().SequenceEqual(FihMagic))
            throw new InvalidDataException("The file does not contain PS5 CNT or FIH magic.");

        byte[] fih = ReadAt(stream, 0, FihHeaderReadSize, "FIH header");
        byte signedByte = fih[0x05];
        SonyPkgKind kind = signedByte switch
        {
            0x00 => SonyPkgKind.FinalizedDebug,
            0x80 => SonyPkgKind.FinalizedRetail,
            _ => throw new InvalidDataException($"Unknown PS5 FIH signed byte 0x{signedByte:X2}.")
        };
        ushort formatVersion = BinaryPrimitives.ReadUInt16LittleEndian(fih.AsSpan(0x06, 2));
        ulong pfsOffset = BinaryPrimitives.ReadUInt64LittleEndian(fih.AsSpan(0x10, 8));
        ulong pfsSize = BinaryPrimitives.ReadUInt64LittleEndian(fih.AsSpan(0x18, 8));
        ulong pfsSuperblockOffset = BinaryPrimitives.ReadUInt64LittleEndian(fih.AsSpan(0x20, 8));
        ulong cntOffset = BinaryPrimitives.ReadUInt64LittleEndian(fih.AsSpan(0x58, 8));

        if (pfsOffset != 0 || pfsSize != 0)
            ValidateRange(pfsOffset, pfsSize, stream.Length, "FIH PFS image");
        if (cntOffset > long.MaxValue) throw new InvalidDataException("The embedded CNT offset is too large.");

        SonyPkgSummary package;
        if (cntOffset == (ulong)stream.Length)
        {
            package = new SonyPkgSummary
            {
                Kind = kind,
                FileSize = stream.Length,
                ContainerOffset = (long)cntOffset,
                SignedByte = signedByte,
                FormatVersion = formatVersion,
                PfsImageOffset = pfsOffset,
                PfsImageSize = pfsSize,
                PfsSuperblockOffset = pfsSuperblockOffset,
                EmbeddedCntOffset = cntOffset
            };
        }
        else
        {
            ValidateRange(cntOffset, CntHeaderSize, stream.Length, "embedded CNT header");
            package = ReadCnt(stream, kind, (long)cntOffset, signedByte, formatVersion, pfsOffset, pfsSize, cntOffset);
            package = CopyWithPfsSuperblock(package, pfsSuperblockOffset);
        }
        SonyPfsSummary? nested = pfsSize > 0 && pfsOffset <= long.MaxValue && pfsSize <= long.MaxValue
            ? new SonyPfsReader().Inspect(path, (long)pfsOffset, (long)pfsSize,
                pfsSuperblockOffset <= long.MaxValue ? (long)pfsSuperblockOffset : null)
            : null;
        if (kind == SonyPkgKind.FinalizedDebug && nested?.AccessState == SonyPfsAccessState.EncryptedKeyRequired &&
            pfsOffset <= long.MaxValue && pfsSize <= long.MaxValue && pfsSuperblockOffset <= long.MaxValue &&
            package.ContentId.Length == 36)
        {
            string passcode = debugPasscode ?? "00000000000000000000000000000000";
            SonyPfsSummary? indexed = new SonyDebugPfsReader().TryIndex(path, (long)pfsOffset, (long)pfsSize,
                (long)pfsSuperblockOffset, package.ContentId, passcode);
            if (indexed is not null) nested = indexed;
        }
        if (nested?.AccessState == SonyPfsAccessState.PlaintextIndexed && nested.CryptoContext is not null)
        {
            SonyPfsSummary? inner = new SonySupplementalPfsIndexReader().TryRead(path, package, nested);
            if (inner is not null) nested = inner;
        }
        return CopyWithNestedPfs(package, nested);
    }

    private static SonyPkgSummary CopyWithNestedPfs(SonyPkgSummary source, SonyPfsSummary? nested) => new()
    {
        Kind = source.Kind,
        FileSize = source.FileSize,
        ContainerOffset = source.ContainerOffset,
        SignedByte = source.SignedByte,
        FormatVersion = source.FormatVersion,
        PfsImageOffset = source.PfsImageOffset,
        PfsImageSize = source.PfsImageSize,
        PfsSuperblockOffset = source.PfsSuperblockOffset,
        EmbeddedCntOffset = source.EmbeddedCntOffset,
        HeaderFlags = source.HeaderFlags,
        SystemEntryCount = source.SystemEntryCount,
        BodyOffset = source.BodyOffset,
        BodySize = source.BodySize,
        ContentId = source.ContentId,
        DrmType = source.DrmType,
        ContentType = source.ContentType,
        ContentFlags = source.ContentFlags,
        Entries = source.Entries,
        NestedPfs = nested
    };

    private static SonyPkgSummary CopyWithPfsSuperblock(SonyPkgSummary source, ulong superblockOffset) => new()
    {
        Kind = source.Kind, FileSize = source.FileSize, ContainerOffset = source.ContainerOffset,
        SignedByte = source.SignedByte, FormatVersion = source.FormatVersion,
        PfsImageOffset = source.PfsImageOffset, PfsImageSize = source.PfsImageSize,
        PfsSuperblockOffset = superblockOffset, EmbeddedCntOffset = source.EmbeddedCntOffset,
        HeaderFlags = source.HeaderFlags, SystemEntryCount = source.SystemEntryCount,
        BodyOffset = source.BodyOffset, BodySize = source.BodySize, ContentId = source.ContentId,
        DrmType = source.DrmType, ContentType = source.ContentType, ContentFlags = source.ContentFlags,
        Entries = source.Entries, NestedPfs = source.NestedPfs
    };

    public byte[] ReadEntryBytes(string path, SonyPkgSummary package, SonyPkgEntry entry, int maximumBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(entry);
        if (maximumBytes < 0) throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        if (entry.IsEncrypted)
            throw new InvalidDataException($"PKG entry 0x{entry.Id:X4} is encrypted (key index {entry.KeyIndex}).");
        if (entry.DataSize > maximumBytes)
            throw new InvalidDataException($"PKG entry 0x{entry.Id:X4} is larger than the {maximumBytes:N0}-byte read limit.");

        using FileStream stream = Open(path);
        long absoluteOffset = CheckedRelativeOffset(package.ContainerOffset, entry.DataOffset, "PKG entry");
        ValidateRange((ulong)absoluteOffset, entry.DataSize, stream.Length, $"PKG entry 0x{entry.Id:X4}");
        return ReadAt(stream, absoluteOffset, checked((int)entry.DataSize), $"PKG entry 0x{entry.Id:X4}");
    }

    private static SonyPkgSummary ReadCnt(FileStream stream, SonyPkgKind kind, long containerOffset,
        byte? signedByte, ushort? formatVersion, ulong pfsOffset, ulong pfsSize, ulong embeddedCntOffset)
    {
        byte[] header = ReadAt(stream, containerOffset, CntHeaderSize, "CNT header");
        ReadOnlySpan<byte> span = header;
        if (!span[..4].SequenceEqual(CntMagic))
            throw new InvalidDataException("The embedded container does not contain PS5 CNT magic.");

        uint headerFlags = BinaryPrimitives.ReadUInt32BigEndian(span[0x04..0x08]);
        uint entryCountValue = BinaryPrimitives.ReadUInt32BigEndian(span[0x10..0x14]);
        ushort systemEntryCount = BinaryPrimitives.ReadUInt16BigEndian(span[0x14..0x16]);
        uint entryTableOffset = BinaryPrimitives.ReadUInt32BigEndian(span[0x18..0x1C]);
        ulong bodyOffset = BinaryPrimitives.ReadUInt64BigEndian(span[0x20..0x28]);
        ulong bodySize = BinaryPrimitives.ReadUInt64BigEndian(span[0x28..0x30]);
        string contentId = ReadAscii(span.Slice(0x40, 0x30));
        uint drmType = BinaryPrimitives.ReadUInt32BigEndian(span[0x70..0x74]);
        uint contentType = BinaryPrimitives.ReadUInt32BigEndian(span[0x74..0x78]);
        uint contentFlags = BinaryPrimitives.ReadUInt32BigEndian(span[0x78..0x7C]);

        if (entryCountValue > MaximumEntryCount)
            throw new InvalidDataException($"CNT entry count {entryCountValue:N0} exceeds the safety limit.");
        int entryCount = checked((int)entryCountValue);
        long tableOffset = CheckedRelativeOffset(containerOffset, entryTableOffset, "CNT entry table");
        long tableSize = checked((long)entryCount * EntryRecordSize);
        ValidateRange((ulong)tableOffset, (ulong)tableSize, stream.Length, "CNT entry table");
        if (bodySize != 0)
        {
            long absoluteBody = CheckedRelativeOffset(containerOffset, bodyOffset, "CNT body");
            ValidateRange((ulong)absoluteBody, bodySize, stream.Length, "CNT body");
        }

        var entries = new List<SonyPkgEntry>(entryCount);
        byte[] records = ReadAt(stream, tableOffset, checked((int)tableSize), "CNT entry table");
        for (int index = 0; index < entryCount; index++)
        {
            ReadOnlySpan<byte> record = records.AsSpan(index * EntryRecordSize, EntryRecordSize);
            var entry = new SonyPkgEntry
            {
                Id = BinaryPrimitives.ReadUInt32BigEndian(record[0x00..0x04]),
                NameTableOffset = BinaryPrimitives.ReadUInt32BigEndian(record[0x04..0x08]),
                Flags1 = BinaryPrimitives.ReadUInt32BigEndian(record[0x08..0x0C]),
                Flags2 = BinaryPrimitives.ReadUInt32BigEndian(record[0x0C..0x10]),
                DataOffset = BinaryPrimitives.ReadUInt32BigEndian(record[0x10..0x14]),
                DataSize = BinaryPrimitives.ReadUInt32BigEndian(record[0x14..0x18])
            };
            long dataOffset = CheckedRelativeOffset(containerOffset, entry.DataOffset, $"CNT entry {index}");
            ValidateRange((ulong)dataOffset, entry.DataSize, stream.Length, $"CNT entry {index} (0x{entry.Id:X4})");
            entries.Add(entry);
        }

        ResolveEntryNames(stream, containerOffset, entries);
        foreach (SonyPkgEntry entry in entries)
            if (string.IsNullOrWhiteSpace(entry.Name)) entry.Name = KnownEntryName(entry.Id);

        return new SonyPkgSummary
        {
            Kind = kind,
            FileSize = stream.Length,
            ContainerOffset = containerOffset,
            SignedByte = signedByte,
            FormatVersion = formatVersion,
            PfsImageOffset = pfsOffset,
            PfsImageSize = pfsSize,
            EmbeddedCntOffset = embeddedCntOffset,
            HeaderFlags = headerFlags,
            SystemEntryCount = systemEntryCount,
            BodyOffset = bodyOffset,
            BodySize = bodySize,
            ContentId = contentId,
            DrmType = drmType,
            ContentType = contentType,
            ContentFlags = contentFlags,
            Entries = entries
        };
    }

    private static void ResolveEntryNames(FileStream stream, long containerOffset, List<SonyPkgEntry> entries)
    {
        SonyPkgEntry? nameEntry = entries.FirstOrDefault(entry => entry.Id == 0x0200);
        if (nameEntry is null || nameEntry.DataSize == 0) return;
        if (nameEntry.IsEncrypted) return;
        if (nameEntry.DataSize > MaximumNameTableSize)
            throw new InvalidDataException($"CNT name table exceeds the {MaximumNameTableSize:N0}-byte safety limit.");

        long offset = CheckedRelativeOffset(containerOffset, nameEntry.DataOffset, "CNT name table");
        byte[] names = ReadAt(stream, offset, checked((int)nameEntry.DataSize), "CNT name table");
        foreach (SonyPkgEntry entry in entries)
        {
            if (entry.NameTableOffset == 0 || entry.NameTableOffset >= names.Length) continue;
            int start = checked((int)entry.NameTableOffset);
            int end = Array.IndexOf(names, (byte)0, start);
            if (end < 0) end = names.Length;
            string name = Encoding.UTF8.GetString(names, start, end - start).Trim();
            if (name.Length > 0 && name.All(character => !char.IsControl(character))) entry.Name = name;
        }
    }

    private static string KnownEntryName(uint id) => id switch
    {
        0x0001 => "digests.bin",
        0x0010 => "entry_keys.bin",
        0x0020 => "image_key.bin",
        0x0080 => "general_digests.bin",
        0x0100 => "metas.bin",
        0x0200 => "entry_names.bin",
        0x0400 => "license.dat",
        0x0401 => "license.info",
        0x040A => "imagedigs.bin",
        0x1000 => "sce_sys/param.sfo",
        0x1001 => "sce_sys/playgo-chunk.dat",
        0x1002 => "sce_sys/playgo-chunk.sha",
        0x1003 => "sce_sys/playgo-manifest.xml",
        0x1200 => "sce_sys/icon0.png",
        0x1220 => "sce_sys/pic0.png",
        0x1240 => "sce_sys/snd0.at9",
        0x1280 => "sce_sys/icon0.dds",
        0x12A0 => "sce_sys/pic0.dds",
        0x12C0 => "sce_sys/pic1.dds",
        0x2000 => "sce_sys/param.json",
        0x2010 => "sce_sys/playgo-hash-table.bin",
        0x2011 => "sce_sys/playgo-ficm.dat",
        _ => $"entry_0x{id:X4}.bin"
    };

    private static FileStream Open(string path) => new(path, FileMode.Open, FileAccess.Read,
        FileShare.ReadWrite | FileShare.Delete, 64 * 1024, FileOptions.RandomAccess);

    private static byte[] ReadAt(FileStream stream, long offset, int count, string description)
    {
        if (offset < 0 || count < 0) throw new InvalidDataException($"Invalid {description} range.");
        ValidateRange((ulong)offset, (ulong)count, stream.Length, description);
        byte[] buffer = new byte[count];
        stream.Position = offset;
        stream.ReadExactly(buffer);
        return buffer;
    }

    private static long CheckedRelativeOffset(long baseOffset, ulong relativeOffset, string description)
    {
        if (baseOffset < 0 || relativeOffset > long.MaxValue - (ulong)baseOffset)
            throw new InvalidDataException($"The {description} offset overflows the package address space.");
        return baseOffset + (long)relativeOffset;
    }

    private static void ValidateRange(ulong offset, ulong size, long fileLength, string description)
    {
        ulong length = checked((ulong)fileLength);
        if (offset > length || size > length - offset)
            throw new InvalidDataException($"The {description} range is outside the package ({offset:X}+{size:X} > {length:X}).");
    }

    private static string ReadAscii(ReadOnlySpan<byte> value)
    {
        int end = value.IndexOf((byte)0);
        if (end < 0) end = value.Length;
        return Encoding.ASCII.GetString(value[..end]).Trim();
    }
}
