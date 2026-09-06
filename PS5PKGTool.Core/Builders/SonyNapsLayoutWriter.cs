using System.Buffers.Binary;

namespace PS5PKGTool.Core.Builders;

internal static class SonyNapsLayoutWriter
{
    private const int LogicalBlockSize = 0x40000;

    public static byte[] CreateStoredLayout(long logicalImageSize, long metadataOffset)
    {
        if (logicalImageSize <= 0 || logicalImageSize >= 1L << 40)
            throw new ArgumentOutOfRangeException(nameof(logicalImageSize));
        if (metadataOffset <= 0 || metadataOffset >= logicalImageSize || (metadataOffset & 0xFFFF) != 0)
            throw new ArgumentOutOfRangeException(nameof(metadataOffset));

        int dataBlockCount = checked((int)(DivideRoundUp(metadataOffset, LogicalBlockSize) +
                                               DivideRoundUp(logicalImageSize - metadataOffset, LogicalBlockSize)));
        if (dataBlockCount > 4_000_000)
            throw new InvalidDataException("The NAPS block table exceeds the supported record limit.");
        int recordCount = checked(dataBlockCount + 1);
        int length = checked(16 + 18 + 10 + recordCount * 9);
        byte[] output = new byte[length];

        ulong header0 = 1; // two logical regions, no shuffle or ublock tables
        ulong header1 = checked((ulong)(recordCount - 2)) << 24;
        BinaryPrimitives.WriteUInt64LittleEndian(output, header0);
        BinaryPrimitives.WriteUInt64LittleEndian(output.AsSpan(8), header1);

        WriteBoundary(output.AsSpan(16, 6), 0);
        WriteBoundary(output.AsSpan(22, 6), metadataOffset);
        WriteBoundary(output.AsSpan(28, 6), logicalImageSize);
        int cursor = 44;
        long logicalOffset = 0;
        int record = 0;
        foreach (long boundary in new[] { metadataOffset, logicalImageSize })
        {
            while (logicalOffset < boundary)
            {
                ulong compressedOffset = checked((ulong)logicalOffset) & 0x3FFFF;
                BinaryPrimitives.WriteUInt64LittleEndian(output.AsSpan(cursor + record * 9, 8), compressedOffset);
                logicalOffset += Math.Min(LogicalBlockSize, boundary - logicalOffset);
                record++;
            }
        }
        ulong terminalOffset = checked((ulong)logicalImageSize) & 0x3FFFF;
        BinaryPrimitives.WriteUInt64LittleEndian(output.AsSpan(cursor + record * 9, 8), terminalOffset);
        return output;
    }

    private static void WriteBoundary(Span<byte> destination, long value)
    {
        ulong number = checked((ulong)value);
        for (int index = 0; index < 5; index++) destination[index] = (byte)(number >> (index * 8));
    }

    private static long DivideRoundUp(long value, long divisor) => checked((value + divisor - 1) / divisor);
}
