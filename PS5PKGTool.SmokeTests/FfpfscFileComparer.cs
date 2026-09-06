internal sealed record FfpfscFileComparison(
    bool SameLength,
    bool EqualExceptBuildTimestamps,
    long Length,
    long TimestampDifferenceBytes,
    long? FirstUnexpectedDifference);

internal static class FfpfscFileComparer
{
    private const int BufferSize = 64 * 1024 * 1024;
    private static readonly (long Start, long End)[] TimestampRanges =
    [
        (0x68, 0x88),
        (0x10018, 0x10038),
        (0x100C0, 0x100E0),
        (0x10168, 0x10188),
        (0x10210, 0x10230)
    ];

    public static async Task<FfpfscFileComparison> CompareAsync(string leftPath, string rightPath)
    {
        await using var left = new FileStream(leftPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var right = new FileStream(rightPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (left.Length != right.Length)
            return new FfpfscFileComparison(false, false, Math.Max(left.Length, right.Length), 0,
                Math.Min(left.Length, right.Length));

        byte[] leftBuffer = new byte[BufferSize];
        byte[] rightBuffer = new byte[BufferSize];
        long offset = 0;
        long timestampDifferences = 0;
        long nextProgress = 1024L * 1024 * 1024;
        while (offset < left.Length)
        {
            int wanted = checked((int)Math.Min(BufferSize, left.Length - offset));
            Task leftRead = left.ReadExactlyAsync(leftBuffer.AsMemory(0, wanted)).AsTask();
            Task rightRead = right.ReadExactlyAsync(rightBuffer.AsMemory(0, wanted)).AsTask();
            await Task.WhenAll(leftRead, rightRead);

            ReadOnlySpan<byte> leftSpan = leftBuffer.AsSpan(0, wanted);
            ReadOnlySpan<byte> rightSpan = rightBuffer.AsSpan(0, wanted);
            if (!leftSpan.SequenceEqual(rightSpan))
            {
                for (int index = 0; index < wanted; index++)
                {
                    if (leftSpan[index] == rightSpan[index]) continue;
                    long absoluteOffset = offset + index;
                    if (!IsTimestampOffset(absoluteOffset))
                        return new FfpfscFileComparison(true, false, left.Length, timestampDifferences,
                            absoluteOffset);
                    timestampDifferences++;
                }
            }

            offset += wanted;
            if (offset >= nextProgress)
            {
                Console.WriteLine($"Compared {Math.Min(offset, left.Length):N0} / {left.Length:N0} bytes");
                nextProgress += 1024L * 1024 * 1024;
            }
        }

        return new FfpfscFileComparison(true, true, left.Length, timestampDifferences, null);
    }

    private static bool IsTimestampOffset(long offset) =>
        TimestampRanges.Any(range => offset >= range.Start && offset < range.End);
}
