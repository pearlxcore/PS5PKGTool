using OodleDotNet;
using System.Runtime.InteropServices;

namespace PS5PKGTool.Core.Parsers;

internal sealed class SonyOodleKrakenDecoder : IDisposable
{
    private const string LibraryFileName = "oo2core_9_win64.dll";
    private static readonly int[] OneChunkFlags = [0x02, 0x00, 0x03, 0x01];
    private static readonly int[] TwoChunkFlags = [0x22, 0x02, 0x12, 0x32, 0x23, 0x03, 0x13, 0x33, 0x00, 0x20];
    private readonly Oodle _oodle;
    private readonly HeaderlessDecode _headerlessDecode;

    public SonyOodleKrakenDecoder()
    {
        string resolved = ResolveLibraryPath();
        _oodle = new Oodle(resolved);
        IntPtr export = NativeLibrary.GetExport(_oodle.Handle, "OodleKraken_Decode_Headerless");
        _headerlessDecode = Marshal.GetDelegateForFunctionPointer<HeaderlessDecode>(export);
    }

    public static bool IsAvailable => File.Exists(GetBundledLibraryPath());

    public IReadOnlyList<(int Flags, byte[] Data)> DecodeCandidates(ReadOnlySpan<byte> payload,
        int firstChunkLength, int outputLength)
    {
        bool twoChunks = outputLength > 0x20000;
        if (payload.Length <= 0 || outputLength <= 0 ||
            (twoChunks && (firstChunkLength <= 0 || firstChunkLength >= payload.Length))) return [];
        var results = new List<(int Flags, byte[] Data)>();
        foreach (int flags in twoChunks ? TwoChunkFlags : OneChunkFlags)
        {
            byte[] output = new byte[outputLength];
            byte[] compressed = payload.ToArray();
            GCHandle outputHandle = default;
            GCHandle compressedHandle = default;
            try
            {
                outputHandle = GCHandle.Alloc(output, GCHandleType.Pinned);
                compressedHandle = GCHandle.Alloc(compressed, GCHandleType.Pinned);
                IntPtr outputPointer = outputHandle.AddrOfPinnedObject();
                IntPtr compressedPointer = compressedHandle.AddrOfPinnedObject();
                int firstOutputLength = Math.Min(0x20000, outputLength);
                bool firstOk = DecodeChunk(outputPointer, firstOutputLength, compressedPointer,
                    twoChunks ? firstChunkLength : compressed.Length, 0,
                    (flags & 0x02) != 0, (flags & 0x01) != 0);
                if (!firstOk) continue;
                if (twoChunks)
                {
                    int secondOutputLength = outputLength - 0x20000;
                    bool secondOk = DecodeChunk(IntPtr.Add(outputPointer, 0x20000), secondOutputLength,
                        IntPtr.Add(compressedPointer, firstChunkLength), compressed.Length - firstChunkLength,
                        0x20000, (flags & 0x20) != 0, (flags & 0x10) != 0);
                    if (!secondOk) continue;
                }
                results.Add((flags, output));
            }
            catch (AccessViolationException) { }
            finally
            {
                if (compressedHandle.IsAllocated) compressedHandle.Free();
                if (outputHandle.IsAllocated) outputHandle.Free();
            }
        }
        return results;
    }

    private bool DecodeChunk(IntPtr output, int outputLength, IntPtr compressed, int compressedLength,
        long bytesSinceReset, bool newLz, bool subLiteral)
    {
        if (outputLength <= 0 || outputLength > 0x20000 || compressedLength <= 0) return false;
        long decoded = _headerlessDecode(output, outputLength, compressed, compressedLength,
            bytesSinceReset, 0, newLz ? 1 : 0, subLiteral ? 1 : 0, IntPtr.Zero, 0);
        return decoded == outputLength;
    }

    private static string ResolveLibraryPath()
    {
        string path = GetBundledLibraryPath();
        if (File.Exists(path)) return path;
        throw new FileNotFoundException(
            $"The bundled Kraken decoder is missing from the application directory: {LibraryFileName}", path);
    }

    private static string GetBundledLibraryPath() => Path.Combine(AppContext.BaseDirectory, LibraryFileName);

    public void Dispose() => _oodle.Dispose();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate long HeaderlessDecode(IntPtr output, long outputLength, IntPtr compressed,
        long compressedLength, long bytesSinceReset, int copy, int newLz, int subLiteral,
        IntPtr scratch, long scratchLength);
}
