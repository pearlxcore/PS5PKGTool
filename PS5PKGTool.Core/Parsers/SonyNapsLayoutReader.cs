using System.Buffers.Binary;

namespace PS5PKGTool.Core.Parsers;

internal sealed record SonyNapsBlock(bool IsRun, uint CompressedOffset, uint UncompressedOffset,
    uint EvenLengthMinusOne, byte Predictor, uint RunDiskBlock, uint RunTweakIndex);

internal sealed class SonyNapsLayout
{
    public required long[] FileBoundaries { get; init; }
    public required SonyNapsBlock[] Blocks { get; init; }
}

internal static class SonyNapsLayoutReader
{
    private const int HeaderSize = 16;
    private const int MaximumRecords = 4_000_000;

    public static SonyNapsLayout Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize) throw new InvalidDataException("The NAPS layout is truncated.");
        ulong word0 = BinaryPrimitives.ReadUInt64LittleEndian(data);
        ulong word1 = BinaryPrimitives.ReadUInt64LittleEndian(data[8..]);
        int fileCount = checked((int)(word0 & 0xFFFFFF) + 1);
        int shuffleCount = checked((int)((word0 >> 28) & 0xF));
        int ublockCount = checked((int)((word0 >> 32) & 0xFFFFFF));
        int outerBlockCount = checked((int)(word1 & 0xFFFFFF));
        int blockCount = checked((int)((word1 >> 24) & 0xFFFFFF) + 2);
        if (fileCount <= 0 || fileCount > MaximumRecords || ublockCount < 0 ||
            outerBlockCount < 0 || outerBlockCount > MaximumRecords || blockCount <= 1 || blockCount > MaximumRecords)
            throw new InvalidDataException("The NAPS layout contains implausible record counts.");

        int cursor = checked(HeaderSize + outerBlockCount * 8 + shuffleCount * 8);
        int fixedTail = checked(((ublockCount + 8) >> 3) * 10 + blockCount * 9);
        int availableForFiles = data.Length - cursor - fixedTail;
        int fileOffsetCount = checked(fileCount + 1);
        if (availableForFiles < fileOffsetCount * 6)
            throw new InvalidDataException("The NAPS file boundary table is truncated.");
        var boundaries = new long[fileOffsetCount];
        for (int index = 0; index < fileOffsetCount; index++, cursor += 6)
        {
            ulong value = 0;
            for (int octet = 0; octet < 5; octet++) value |= (ulong)data[cursor + octet] << (octet * 8);
            boundaries[index] = checked((long)value);
        }
        cursor = checked(cursor + ((ublockCount + 8) >> 3) * 10);
        if (cursor + blockCount * 9 > data.Length)
            throw new InvalidDataException("The NAPS compressed block table is truncated.");

        var blocks = new SonyNapsBlock[blockCount];
        for (int index = 0; index < blockCount; index++, cursor += 9)
        {
            ulong low = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(cursor, 8));
            byte high = data[cursor + 8];
            bool run = ((low >> 18) & 1) != 0;
            blocks[index] = run
                ? new SonyNapsBlock(true, (uint)(low & 0x3FFFF), 0, 0, 0,
                    (uint)(((low >> 49) & 0x7FFF) | ((ulong)high << 15)),
                    (uint)((low >> 19) & 0xFFFFFFF))
                : new SonyNapsBlock(false, (uint)(low & 0x3FFFF),
                    (uint)((low >> 19) & 0x3FFFF), (uint)((low >> 37) & 0x1FFFF),
                    (byte)((low >> 56) & 0x7), 0, 0);
        }
        return new SonyNapsLayout { FileBoundaries = boundaries, Blocks = blocks };
    }
}
