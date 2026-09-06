using System.Buffers.Binary;

namespace PS5PKGTool.Core.Parsers;

internal static class SonyInnerPfsMountReader
{
    private const int CompressionBlockSize = 0x40000;
    private const int MaximumMountBytes = 512 * 1024 * 1024;

    public static (byte[] Data, long LogicalOffset, long MountSize) ReconstructMetadata(
        long innerImageLength, Func<long, int, byte[]> readInnerImage, ReadOnlySpan<byte> naps,
        SonyOodleKrakenDecoder decoder)
    {
        SonyNapsLayout layout = SonyNapsLayoutReader.Read(naps);
        long mountSize = layout.FileBoundaries.Where(value => value > 0 && (value & 0xFFFF) == 0)
            .DefaultIfEmpty().Max();
        long[] boundaries = layout.FileBoundaries.Where(value => value > 0 && value <= mountSize)
            .Distinct().Order().ToArray();
        long metadataOffset = boundaries.LastOrDefault(value => value < mountSize);
        long metadataLength = mountSize - metadataOffset;
        if (metadataOffset <= 0 || metadataLength <= 0 || metadataLength > MaximumMountBytes)
            throw new InvalidDataException("The NAPS metadata region is outside the reconstruction limit.");
        byte[] metadata = new byte[checked((int)metadataLength)];
        long diskOffset = 0;
        long logicalOffset = 0;
        int? selectedFlags = null;
        for (int index = 0; index < layout.Blocks.Length; index++)
        {
            SonyNapsBlock block = layout.Blocks[index];
            if (block.IsRun)
            {
                long fraction = index + 1 < layout.Blocks.Length && !layout.Blocks[index + 1].IsRun
                    ? layout.Blocks[index + 1].CompressedOffset & 0x7FFF : 0;
                diskOffset = checked((long)block.RunTweakIndex * 0x8000 + fraction);
                continue;
            }
            if (index + 1 >= layout.Blocks.Length || logicalOffset >= mountSize) break;
            long fileEnd = boundaries.FirstOrDefault(value => value > logicalOffset, mountSize);
            int logicalLength = checked((int)Math.Min(CompressionBlockSize, fileEnd - logicalOffset));
            if (logicalLength <= 0) break;
            int compressedLength = checked((int)(layout.Blocks[index + 1].CompressedOffset - block.CompressedOffset));
            if (logicalOffset >= metadataOffset)
            {
                byte[] decoded;
                if (block.Predictor == 2)
                {
                    int evenLength = checked((int)(block.EvenLengthMinusOne / 2 + 1));
                    if (diskOffset < 0 || compressedLength <= 0 || diskOffset > innerImageLength - compressedLength)
                        throw new InvalidDataException("A Kraken metadata block is outside pfs_image.dat.");
                    byte[] compressed = readInnerImage(diskOffset, compressedLength);
                    IReadOnlyList<(int Flags, byte[] Data)> candidates = decoder.DecodeCandidates(
                        compressed, evenLength, logicalLength);
                    (int Flags, byte[] Data)? chosen = null;
                    foreach (var candidate in candidates)
                    {
                        bool valid = logicalOffset == metadataOffset
                            ? candidate.Data.Length >= 16 && BinaryPrimitives.ReadInt64LittleEndian(candidate.Data) == 2 &&
                              BinaryPrimitives.ReadInt64LittleEndian(candidate.Data.AsSpan(8)) == 20130315
                            : selectedFlags is not int expected || (logicalLength > 0x20000
                                ? candidate.Flags == expected : (candidate.Flags & 0x0F) == (expected & 0x0F));
                        if (valid) { chosen = candidate; break; }
                    }
                    if (chosen is null)
                        throw new InvalidDataException($"Oodle could not decode the Kraken metadata block at 0x{logicalOffset:X}.");
                    selectedFlags ??= chosen.Value.Flags;
                    decoded = chosen.Value.Data;
                    diskOffset += compressedLength;
                }
                else
                {
                    int readLength = block.Predictor == 4 ? Math.Min(logicalLength, Math.Max(0, compressedLength)) : logicalLength;
                    decoded = new byte[logicalLength];
                    if (readLength > 0) readInnerImage(diskOffset, readLength).CopyTo(decoded, 0);
                    diskOffset += block.Predictor == 4 ? readLength : logicalLength;
                }
                decoded.CopyTo(metadata, checked((int)(logicalOffset - metadataOffset)));
            }
            else
                diskOffset += block.Predictor == 2 ? compressedLength :
                    block.Predictor == 4 ? Math.Max(0, compressedLength) : logicalLength;
            logicalOffset += logicalLength;
        }
        return (metadata, metadataOffset, mountSize);
    }

    public static long FindSuperblock(ReadOnlySpan<byte> mount)
    {
        for (int offset = (mount.Length - 0x10000) & ~0xFFFF; offset >= 0; offset -= 0x10000)
            if (BinaryPrimitives.ReadInt64LittleEndian(mount[offset..]) == 2 &&
                BinaryPrimitives.ReadInt64LittleEndian(mount[(offset + 8)..]) == 20130315) return offset;
        return -1;
    }
}
