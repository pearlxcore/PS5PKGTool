using System.Buffers.Binary;
using System.Text;

namespace PS5PKGTool.Core.Builders;

internal sealed record SonyDataFirstPfsFile(string RelativePath, long Offset, long Size);

internal sealed class SonyDataFirstPfsBuildResult
{
    public required long ImageSize { get; init; }
    public required long MetadataOffset { get; init; }
    public required IReadOnlyList<SonyDataFirstPfsFile> Files { get; init; }
}

internal static class SonyDataFirstPfsImageBuilder
{
    private const int BlockSize = 0x10000;
    private const int InodeSize = 0xA8;
    private const int MaximumNodes = 500_000;

    public static async Task<SonyDataFirstPfsBuildResult> CreateAsync(string sourceDirectory,
        string outputPath, IProgress<SonyDebugPackageProgress>? progress, long totalSourceBytes,
        CancellationToken cancellationToken)
    {
        Layout layout = Scan(sourceDirectory, cancellationToken);
        await using var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] zero = new byte[BlockSize];
        await output.WriteAsync(zero, cancellationToken).ConfigureAwait(false);
        long completed = 0;
        foreach (Node node in layout.Nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (output.Position != checked((long)node.FirstBlock * BlockSize))
                throw new InvalidDataException("Data first PFS layout position is inconsistent.");
            long start = output.Position;
            if (node.IsDirectory)
                await output.WriteAsync(node.DirectoryData, cancellationToken).ConfigureAwait(false);
            else
            {
                string physicalPath = Path.Combine(sourceDirectory,
                    node.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                await using var input = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
                await input.CopyToAsync(output, 1024 * 1024, cancellationToken).ConfigureAwait(false);
                completed = checked(completed + node.Size);
                progress?.Report(new SonyDebugPackageProgress("Building data first PFS", completed,
                    totalSourceBytes, node.RelativePath));
            }
            await WriteZerosAsync(output, checked((long)node.BlockCount * BlockSize - (output.Position - start)),
                zero, cancellationToken).ConfigureAwait(false);
        }
        if (output.Position != layout.MetadataOffset)
            throw new InvalidDataException("Data first PFS metadata offset is inconsistent.");
        byte[] superblock = new byte[BlockSize];
        WriteSuperblock(superblock, layout);
        await output.WriteAsync(superblock, cancellationToken).ConfigureAwait(false);
        byte[] inodes = new byte[checked(layout.InodeBlocks * BlockSize)];
        for (int index = 0; index < layout.Nodes.Count; index++)
            WriteInode(inodes.AsSpan(index * InodeSize, InodeSize), layout.Nodes[index]);
        await output.WriteAsync(inodes, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (output.Position != layout.ImageSize)
            throw new InvalidDataException("Data first PFS writer produced an unexpected image length.");
        return new SonyDataFirstPfsBuildResult
        {
            ImageSize = layout.ImageSize,
            MetadataOffset = layout.MetadataOffset,
            Files = layout.Nodes.Where(node => !node.IsDirectory)
                .Select(node => new SonyDataFirstPfsFile(node.RelativePath,
                    checked((long)node.FirstBlock * BlockSize), node.Size)).ToArray()
        };
    }

    private static Layout Scan(string sourceDirectory, CancellationToken cancellationToken)
    {
        var nodes = new List<Node>();
        var superRoot = new Node(0, string.Empty, string.Empty, true, -1);
        var appRoot = new Node(1, "uroot", string.Empty, true, 0);
        nodes.Add(superRoot); nodes.Add(appRoot);
        var directories = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase) { [string.Empty] = appRoot };
        foreach (string path in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories)
                     .Select(path => Path.GetRelativePath(sourceDirectory, path).Replace('\\', '/'))
                     .OrderBy(path => path.Count(character => character == '/'))
                     .ThenBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidatePath(path);
            Node parent = directories[Parent(path)];
            var node = new Node(nodes.Count, Name(path), path, true, parent.Index);
            nodes.Add(node); parent.Children.Add(node.Index); directories.Add(path, node);
            if (nodes.Count > MaximumNodes) throw new InvalidDataException("The PFS node safety limit was exceeded.");
        }
        foreach (string physical in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories)
                     .OrderBy(path => Path.GetRelativePath(sourceDirectory, path), StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = Path.GetRelativePath(sourceDirectory, physical).Replace('\\', '/');
            ValidatePath(path);
            Node parent = directories[Parent(path)];
            var node = new Node(nodes.Count, Name(path), path, false, parent.Index)
                { Size = new FileInfo(physical).Length };
            nodes.Add(node); parent.Children.Add(node.Index);
            if (nodes.Count > MaximumNodes) throw new InvalidDataException("The PFS node safety limit was exceeded.");
        }
        superRoot.Children.Add(appRoot.Index);
        foreach (Node directory in nodes.Where(node => node.IsDirectory))
        {
            directory.DirectoryData = BuildDirectory(directory, nodes);
            directory.Size = directory.DirectoryData.Length;
        }

        long nextBlock = 1;
        foreach (Node node in nodes)
        {
            node.FirstBlock = checked((int)nextBlock);
            node.BlockCount = checked((int)Math.Max(1, DivideRoundUp(node.Size, BlockSize)));
            nextBlock = checked(nextBlock + node.BlockCount);
            if (nextBlock > int.MaxValue) throw new InvalidDataException("The PFS block address space was exceeded.");
        }
        long metadataOffset = checked(nextBlock * BlockSize);
        int inodeBlocks = checked((int)DivideRoundUp(checked((long)nodes.Count * InodeSize), BlockSize));
        long totalBlocks = checked(nextBlock + 1 + inodeBlocks);
        return new Layout(nodes, inodeBlocks, metadataOffset, checked(totalBlocks * BlockSize), totalBlocks);
    }

    private static byte[] BuildDirectory(Node directory, IReadOnlyList<Node> nodes)
    {
        using var output = new MemoryStream();
        foreach (Node child in directory.Children.Select(index => nodes[index])
                     .OrderBy(node => node.Name, StringComparer.OrdinalIgnoreCase))
        {
            byte[] name = Encoding.UTF8.GetBytes(child.Name);
            int recordSize = checked((name.Length + 16 + 7) & ~7);
            long inBlock = output.Position % BlockSize;
            if (inBlock + recordSize > BlockSize) output.Write(new byte[checked((int)(BlockSize - inBlock))]);
            byte[] record = new byte[recordSize];
            BinaryPrimitives.WriteUInt32LittleEndian(record, checked((uint)child.Index));
            BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(4), child.IsDirectory ? 3 : 2);
            BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(8), name.Length);
            BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(12), recordSize);
            name.CopyTo(record, 16);
            output.Write(record);
        }
        if (output.Length == 0) output.Write(new byte[16]);
        return output.ToArray();
    }

    private static void WriteSuperblock(Span<byte> block, Layout layout)
    {
        BinaryPrimitives.WriteInt64LittleEndian(block, 2);
        BinaryPrimitives.WriteInt64LittleEndian(block[8..], 20130315);
        BinaryPrimitives.WriteUInt16LittleEndian(block[0x1C..], 0x0008);
        BinaryPrimitives.WriteUInt32LittleEndian(block[0x20..], BlockSize);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x28..], 1);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x30..], layout.Nodes.Count);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x38..], layout.TotalBlocks);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x40..], layout.InodeBlocks);
    }

    private static void WriteInode(Span<byte> inode, Node node)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(inode, node.IsDirectory ? (ushort)0x4000 : (ushort)0x8000);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[2..], 1);
        BinaryPrimitives.WriteInt64LittleEndian(inode[8..], node.Size);
        BinaryPrimitives.WriteInt64LittleEndian(inode[0x10..], node.Size);
        BinaryPrimitives.WriteUInt32LittleEndian(inode[0x60..], checked((uint)node.BlockCount));
        BinaryPrimitives.WriteInt32LittleEndian(inode[0x64..], node.FirstBlock);
        for (int index = 1; index < 12; index++) BinaryPrimitives.WriteInt32LittleEndian(inode[(0x64 + index * 4)..], -1);
        for (int index = 0; index < 5; index++) BinaryPrimitives.WriteInt32LittleEndian(inode[(0x94 + index * 4)..], -1);
    }

    private static async Task WriteZerosAsync(Stream output, long count, byte[] zero, CancellationToken cancellationToken)
    {
        if (count < 0) throw new InvalidDataException("PFS data exceeded its allocated extent.");
        while (count > 0) { int take = checked((int)Math.Min(count, zero.Length)); await output.WriteAsync(zero.AsMemory(0, take), cancellationToken).ConfigureAwait(false); count -= take; }
    }

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Split('/').Any(part => part is "" or "." or "..") || path.Any(char.IsControl))
            throw new InvalidDataException("The dump contains an unsafe path: " + path);
    }
    private static string Parent(string path) => path.Contains('/') ? path[..path.LastIndexOf('/')] : string.Empty;
    private static string Name(string path) => path.Contains('/') ? path[(path.LastIndexOf('/') + 1)..] : path;
    private static long DivideRoundUp(long value, long divisor) => checked((value + divisor - 1) / divisor);

    private sealed class Node(int index, string name, string relativePath, bool isDirectory, int parent)
    {
        public int Index { get; } = index;
        public string Name { get; } = name;
        public string RelativePath { get; } = relativePath;
        public bool IsDirectory { get; } = isDirectory;
        public int Parent { get; } = parent;
        public List<int> Children { get; } = [];
        public byte[] DirectoryData { get; set; } = [];
        public long Size { get; set; }
        public int FirstBlock { get; set; }
        public int BlockCount { get; set; }
    }
    private sealed record Layout(IReadOnlyList<Node> Nodes, int InodeBlocks, long MetadataOffset,
        long ImageSize, long TotalBlocks);
}
