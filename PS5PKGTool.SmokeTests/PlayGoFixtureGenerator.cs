using System.Buffers.Binary;
using System.Text;

namespace PS5PKGTool.SmokeTests;

// Synthetic PlayGo fixture generator (local only; not shipped to ProsperoPkgTool).
// The upstream handoff is spec + fixture hex only, so this file stays in our repo as a test tool.
// Path hashing is injected so this file has no hard dependency on the engine:
// pass ProsperoOuterPfsBuilder.FltPathHash in production, or any equivalent in a test harness.

public sealed record PlayGoFixture(
    byte[] ChunkDat,
    byte[] HashTable,
    byte[] Ficm,
    IReadOnlyList<ulong> PathHashes,
    IReadOnlyList<string> Paths,
    IReadOnlyList<byte> PathChunks);

public static class PlayGoFixtureGenerator
{
    public const string ContentId = "UP0000-PPSA00000_00-SYNTHETIC0000001";
    public const int HeaderFlags = 0x85;

    public static IReadOnlyList<string> AssignmentPaths { get; } =
        ["eboot.bin", "sce_sys/param.sfo", "sce_sys/keystone"];

    public static IReadOnlyList<byte> AssignmentChunks { get; } = [0, 1, 2];

    public static PlayGoFixture Generate(Func<string, ulong> pathHash)
    {
        ArgumentNullException.ThrowIfNull(pathHash);
        byte[] chunkDat = BuildChunkDat();
        byte[] hashTable = BuildHashTable(pathHash, out ulong[] hashes);
        byte[] ficm = BuildFicm();
        return new PlayGoFixture(chunkDat, hashTable, ficm, hashes, AssignmentPaths, AssignmentChunks);
    }

    public static PlayGoFixture Write(string outputDirectory, Func<string, ulong> pathHash)
    {
        PlayGoFixture fixture = Generate(pathHash);
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllBytes(Path.Combine(outputDirectory, "playgo-chunk.dat"), fixture.ChunkDat);
        File.WriteAllBytes(Path.Combine(outputDirectory, "playgo-hash-table.dat"), fixture.HashTable);
        File.WriteAllBytes(Path.Combine(outputDirectory, "playgo-ficm.dat"), fixture.Ficm);
        File.WriteAllText(Path.Combine(outputDirectory, "expected.txt"), Describe(fixture));
        return fixture;
    }

    public static string Describe(PlayGoFixture fixture)
    {
        var text = new StringBuilder();
        text.AppendLine($"content_id        = {ContentId}");
        text.AppendLine($"header_flags      = 0x{HeaderFlags:X}");
        text.AppendLine($"chunks            = 3");
        text.AppendLine($"scenarios         = 2");
        text.AppendLine($"default_scenario  = 0");
        text.AppendLine("chunk 0: label='Chunk #0' mask=0xFFFFFFFFFFFFFFFF extents=[(0x100000,0x10000),(0x200000,0x8000)] total_bytes=98304");
        text.AppendLine("chunk 1: label='Chunk #1' mask=0x0000000000000001 extents=[(0x300000,0x20000)] total_bytes=131072");
        text.AppendLine("chunk 2: label='Chunk #2' mask=0x0000000000000002 extents=[(0x400000,0x4000),(0x500000,0x4000)] total_bytes=32768");
        text.AppendLine("scenario 0: label='Scenario #0' initial=2 sequence=[0,1,2]");
        text.AppendLine("scenario 1: label='Scenario #1' initial=1 sequence=[0,2]");
        text.AppendLine($"assignments       = {fixture.Paths.Count}");
        for (int i = 0; i < fixture.Paths.Count; i++)
            text.AppendLine($"  0x{fixture.PathHashes[i]:X16} -> chunk {fixture.PathChunks[i]}  {fixture.Paths[i]}");
        return text.ToString();
    }

    private static byte[] BuildChunkDat()
    {
        (string Label, ulong Mask, (ulong Start, ulong Length)[] Extents)[] chunks =
        [
            ("Chunk #0", 0xFFFFFFFFFFFFFFFF, [(0x100000UL, 0x10000UL), (0x200000UL, 0x08000UL)]),
            ("Chunk #1", 0x0000000000000001, [(0x300000UL, 0x20000UL)]),
            ("Chunk #2", 0x0000000000000002, [(0x400000UL, 0x04000UL), (0x500000UL, 0x04000UL)])
        ];
        (string Label, int Initial, int[] Sequence)[] scenarios =
        [
            ("Scenario #0", 2, [0, 1, 2]),
            ("Scenario #1", 1, [0, 2])
        ];

        var extentIds = new List<uint>();
        var chunkExtentListOffset = new int[chunks.Length];
        var chunkExtentCount = new int[chunks.Length];
        for (int c = 0; c < chunks.Length; c++)
        {
            chunkExtentListOffset[c] = extentIds.Count * 4;
            chunkExtentCount[c] = chunks[c].Extents.Length;
            for (int e = 0; e < chunks[c].Extents.Length; e++)
                extentIds.Add((uint)extentIds.Count);
        }

        var chunkLabelSection = new SectionBuilder();
        var chunkLabelOffsets = new int[chunks.Length];
        for (int c = 0; c < chunks.Length; c++)
            chunkLabelOffsets[c] = chunkLabelSection.Add(chunks[c].Label);

        var extents = new byte[extentIds.Count * 16];
        int extentIndex = 0;
        foreach ((string _, ulong _, (ulong Start, ulong Length)[] chunkExtents) in chunks)
        {
            foreach ((ulong start, ulong length) in chunkExtents)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(extents.AsSpan(extentIndex * 16), start);
                BinaryPrimitives.WriteUInt64LittleEndian(extents.AsSpan(extentIndex * 16 + 8), length);
                extentIndex++;
            }
        }

        var scenarioLabelSection = new SectionBuilder();
        var scenarioLabelOffsets = new int[scenarios.Length];
        for (int s = 0; s < scenarios.Length; s++)
            scenarioLabelOffsets[s] = scenarioLabelSection.Add(scenarios[s].Label);

        var scenarioSequence = new byte[scenarios.Sum(s => s.Sequence.Length) * 2];
        var scenarioSequenceOffsets = new int[scenarios.Length];
        int sequenceOffset = 0;
        for (int s = 0; s < scenarios.Length; s++)
        {
            scenarioSequenceOffsets[s] = sequenceOffset;
            foreach (int chunkId in scenarios[s].Sequence)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(scenarioSequence.AsSpan(sequenceOffset), (ushort)chunkId);
                sequenceOffset += 2;
            }
        }

        var chunkAttrs = new byte[chunks.Length * 0x20];
        for (int c = 0; c < chunks.Length; c++)
        {
            int at = c * 0x20;
            BinaryPrimitives.WriteUInt32LittleEndian(chunkAttrs.AsSpan(at + 0x00), 0x00030080);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkAttrs.AsSpan(at + 0x04), (uint)chunkExtentCount[c]);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkAttrs.AsSpan(at + 0x08), 0x11);
            BinaryPrimitives.WriteUInt64LittleEndian(chunkAttrs.AsSpan(at + 0x10), chunks[c].Mask);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkAttrs.AsSpan(at + 0x18), (uint)chunkExtentListOffset[c]);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkAttrs.AsSpan(at + 0x1C), (uint)chunkLabelOffsets[c]);
        }

        var scenarioAttrs = new byte[scenarios.Length * 0x20];
        for (int s = 0; s < scenarios.Length; s++)
        {
            int at = s * 0x20;
            BinaryPrimitives.WriteUInt32LittleEndian(scenarioAttrs.AsSpan(at + 0x00), 0x21);
            BinaryPrimitives.WriteUInt16LittleEndian(scenarioAttrs.AsSpan(at + 0x14), (ushort)scenarios[s].Initial);
            BinaryPrimitives.WriteUInt16LittleEndian(scenarioAttrs.AsSpan(at + 0x16), (ushort)scenarios[s].Sequence.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(scenarioAttrs.AsSpan(at + 0x18), (uint)scenarioSequenceOffsets[s]);
            BinaryPrimitives.WriteUInt32LittleEndian(scenarioAttrs.AsSpan(at + 0x1C), (uint)scenarioLabelOffsets[s]);
        }

        byte[] chunkLabels = chunkLabelSection.ToArray();
        byte[] scenarioLabels = scenarioLabelSection.ToArray();
        byte[] extentIdBytes = new byte[extentIds.Count * 4];
        for (int i = 0; i < extentIds.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(extentIdBytes.AsSpan(i * 4), extentIds[i]);

        byte[][] sections =
        [
            chunkAttrs, extentIdBytes, chunkLabels, extents,
            scenarioAttrs, scenarioSequence, scenarioLabels, []
        ];
        var sectionOffsets = new int[sections.Length];
        int offset = 0x100;
        for (int i = 0; i < sections.Length; i++)
        {
            offset = Align(offset, 0x10);
            sectionOffsets[i] = offset;
            offset += sections[i].Length;
        }

        byte[] file = new byte[offset];
        Write32(file, 0x00, 0x78676C70);
        Write16(file, 0x04, 0x1000);
        Write16(file, 0x06, 0);
        Write16(file, 0x08, 1);
        Write16(file, 0x0A, (ushort)chunks.Length);
        Write16(file, 0x0E, (ushort)scenarios.Length);
        Write32(file, 0x10, (uint)offset);
        Write16(file, 0x14, 0);
        Write16(file, 0x16, 1);
        Write32(file, 0x1C, HeaderFlags);
        Write32(file, 0x20, 4);
        Write32(file, 0x24, 1);
        Write32(file, 0x30, 0x11);
        Write64(file, 0x38, ulong.MaxValue);
        byte[] contentId = Encoding.ASCII.GetBytes(ContentId);
        Array.Copy(contentId, 0, file, 0x40, Math.Min(contentId.Length, 127));
        for (int i = 0; i < sections.Length; i++)
        {
            Write32(file, 0xC0 + (i * 8), (uint)sectionOffsets[i]);
            Write32(file, 0xC0 + (i * 8) + 4, (uint)sections[i].Length);
            Array.Copy(sections[i], 0, file, sectionOffsets[i], sections[i].Length);
        }
        return file;
    }

    private static byte[] BuildHashTable(Func<string, ulong> pathHash, out ulong[] hashes)
    {
        hashes = AssignmentPaths.Select(pathHash).ToArray();
        byte[] table = new byte[0x38 + (hashes.Length * 8)];
        BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(0x0C), (uint)(hashes.Length * 8));
        BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(0x24), (uint)hashes.Length);
        for (int i = 0; i < hashes.Length; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(table.AsSpan(0x38 + (i * 8)), hashes[i]);
        return table;
    }

    private static byte[] BuildFicm()
    {
        byte[] ficm = new byte[0x10 + (AssignmentChunks.Count * 2)];
        BinaryPrimitives.WriteUInt32LittleEndian(ficm.AsSpan(0x0C), (uint)(AssignmentChunks.Count * 2));
        for (int i = 0; i < AssignmentChunks.Count; i++)
            BinaryPrimitives.WriteUInt16LittleEndian(ficm.AsSpan(0x10 + (i * 2)), AssignmentChunks[i]);
        return ficm;
    }

    private static int Align(int value, int alignment) => (value + alignment - 1) & ~(alignment - 1);

    private static void Write16(byte[] data, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), value);

    private static void Write32(byte[] data, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);

    private static void Write64(byte[] data, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(offset), value);

    private sealed class SectionBuilder
    {
        private readonly List<byte> _bytes = [];

        public int Add(string label)
        {
            int offset = _bytes.Count;
            _bytes.AddRange(Encoding.ASCII.GetBytes(label));
            _bytes.Add(0);
            return offset;
        }

        public byte[] ToArray() => _bytes.ToArray();
    }
}
