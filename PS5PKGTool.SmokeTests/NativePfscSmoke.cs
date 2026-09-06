using System.Buffers.Binary;
using PS5PKGTool.Ffpfsc;

internal static class NativePfscSmoke
{
    public static async Task RunAsync()
    {
        byte[] source = new byte[PfscCodec.LogicalBlockSize * 3 + 12345];
        Array.Fill(source, (byte)0x41, 0, PfscCodec.LogicalBlockSize);
        var random = new Random(0x50534653);
        random.NextBytes(source.AsSpan(PfscCodec.LogicalBlockSize, PfscCodec.LogicalBlockSize));
        for (int index = PfscCodec.LogicalBlockSize * 2; index < source.Length; index++)
            source[index] = (byte)(index % 17);

        await using var input = new MemoryStream(source, writable: false);
        await using var encoded = new MemoryStream();
        PfscWriteResult result = await PfscCodec.EncodeAsync(input, source.Length, encoded,
            new PfscCompressionOptions { CompressionLevel = 9, MinimumGainPercent = 5 });
        Require(result.BlockCount == 4, "PFSC block count is incorrect.");
        Require(result.CompressedBlockCount >= 2, "PFSC did not retain compressible blocks.");
        Require(result.StoredLength < result.PaddedLogicalLength, "PFSC output did not save space.");

        encoded.Position = 0;
        PfscInfo info = PfscCodec.Inspect(encoded);
        Require(info.BlockCount == result.BlockCount, "PFSC inspection disagrees with the writer.");
        Require(info.BlockOffsets.Zip(info.BlockOffsets.Skip(1)).All(pair => pair.First <= pair.Second),
            "PFSC offset table is not monotonic.");

        encoded.Position = 0;
        await using var decoded = new MemoryStream();
        await PfscCodec.DecodeAsync(encoded, decoded, source.Length);
        Require(decoded.ToArray().AsSpan().SequenceEqual(source), "Native PFSC round trip changed the payload.");

        byte[] corrupt = encoded.ToArray();
        ulong first = BinaryPrimitives.ReadUInt64LittleEndian(corrupt.AsSpan(PfscCodec.OffsetTableOffset, 8));
        BinaryPrimitives.WriteUInt64LittleEndian(corrupt.AsSpan(PfscCodec.OffsetTableOffset + 8, 8), first - 1);
        using var corruptStream = new MemoryStream(corrupt, writable: false);
        RequireThrows<InvalidDataException>(() => PfscCodec.Inspect(corruptStream),
            "PFSC accepted a non-monotonic block table.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void RequireThrows<T>(Action action, string message) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException(message);
    }
}
