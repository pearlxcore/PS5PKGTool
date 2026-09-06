using System.Buffers.Binary;
using System.Text;

namespace PS5PKGTool.Core.Builders;

public sealed class SonyPfsBuildOptions
{
    public int BlockSize { get; init; } = 0x10000;
    public bool CaseInsensitive { get; init; } = true;
}

public readonly record struct SonyPfsBuildProgress(string Stage, long CompletedBytes, long TotalBytes,
    string CurrentPath);

public sealed class SonyPfsBuildResult
{
    public required string OutputPath { get; init; }
    public required long ImageSize { get; init; }
    public required long SourceBytes { get; init; }
    public required int FileCount { get; init; }
    public required int DirectoryCount { get; init; }
    public required int BlockSize { get; init; }
}

public static class SonyPfsImageBuilder
{
    private const int InodeSize = 0xA8;
    private const int MaximumNodes = 500_000;
    private const int MaximumMetadataBytes = 128 * 1024 * 1024;

    public static async Task<SonyPfsBuildResult> CreateFromDirectoryAsync(string sourceDirectory,
        string outputPath, SonyPfsBuildOptions? options = null,
        IProgress<SonyPfsBuildProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        options ??= new SonyPfsBuildOptions();
        string source = Path.GetFullPath(sourceDirectory);
        string destination = Path.GetFullPath(outputPath);
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
        ValidateBlockSize(options.BlockSize);

        Layout layout = Scan(source, options.BlockSize, options.CaseInsensitive, cancellationToken);
        string? parent = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(parent)) throw new ArgumentException("Output path has no parent directory.", nameof(outputPath));
        Directory.CreateDirectory(parent);
        string temporary = Path.Combine(parent, "." + Path.GetFileName(destination) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await WriteAsync(source, temporary, layout, options, progress, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, destination, true);
            return new SonyPfsBuildResult
            {
                OutputPath = destination,
                ImageSize = layout.ImageSize,
                SourceBytes = layout.SourceBytes,
                FileCount = layout.Nodes.Count(node => !node.IsDirectory) - 0,
                DirectoryCount = layout.Nodes.Count(node => node.IsDirectory) - 1,
                BlockSize = options.BlockSize
            };
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static Layout Scan(string source, int blockSize, bool caseInsensitive,
        CancellationToken cancellationToken)
    {
        var comparer = caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var nodes = new List<Node>();
        var superRoot = new Node(0, string.Empty, string.Empty, true, -1);
        var appRoot = new Node(1, "uroot", string.Empty, true, 0);
        nodes.Add(superRoot);
        nodes.Add(appRoot);
        var directoryByPath = new Dictionary<string, Node>(comparer) { [string.Empty] = appRoot };

        string[] directories = Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(source, path).Replace('\\', '/'))
            .OrderBy(path => path.Count(character => character == '/'))
            .ThenBy(path => path, comparer)
            .ToArray();
        foreach (string relative in directories)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateRelativePath(relative);
            string parentPath = Parent(relative);
            if (!directoryByPath.TryGetValue(parentPath, out Node? parent))
                throw new InvalidDataException($"Directory parent was not indexed: {parentPath}");
            var node = new Node(nodes.Count, Name(relative), relative, true, parent.Index);
            nodes.Add(node);
            parent.Children.Add(node.Index);
            directoryByPath.Add(relative, node);
        }

        long sourceBytes = 0;
        foreach (string filePath in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
                     .OrderBy(path => Path.GetRelativePath(source, path), comparer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string relative = Path.GetRelativePath(source, filePath).Replace('\\', '/');
            ValidateRelativePath(relative);
            string parentPath = Parent(relative);
            if (!directoryByPath.TryGetValue(parentPath, out Node? parent))
                throw new InvalidDataException($"File parent was not indexed: {parentPath}");
            long size = new FileInfo(filePath).Length;
            var node = new Node(nodes.Count, Name(relative), relative, false, parent.Index) { Size = size };
            nodes.Add(node);
            parent.Children.Add(node.Index);
            sourceBytes = checked(sourceBytes + size);
            if (nodes.Count > MaximumNodes) throw new InvalidDataException($"PFS input exceeds the {MaximumNodes:N0} node limit.");
        }

        superRoot.Children.Add(appRoot.Index);
        foreach (Node directory in nodes.Where(node => node.IsDirectory))
        {
            directory.DirectoryData = BuildDirectoryData(directory, nodes, blockSize, comparer);
            directory.Size = directory.DirectoryData.Length;
        }

        int inodeBlocks = checked((int)DivideRoundUp(checked((long)nodes.Count * InodeSize), blockSize));
        long metadataBytes = checked((long)inodeBlocks * blockSize);
        if (metadataBytes > MaximumMetadataBytes)
            throw new InvalidDataException($"PFS inode metadata exceeds the {MaximumMetadataBytes:N0} byte limit.");
        long nextBlock = checked(1L + inodeBlocks);
        foreach (Node node in nodes)
        {
            node.FirstBlock = checked((int)nextBlock);
            node.BlockCount = checked((uint)Math.Max(1, DivideRoundUp(node.Size, blockSize)));
            nextBlock = checked(nextBlock + node.BlockCount);
            if (nextBlock > int.MaxValue) throw new InvalidDataException("PFS image exceeds the supported 32 bit block-address space.");
        }
        return new Layout(nodes, inodeBlocks, nextBlock, checked(nextBlock * blockSize), sourceBytes);
    }

    private static byte[] BuildDirectoryData(Node directory, IReadOnlyList<Node> nodes, int blockSize,
        StringComparer comparer)
    {
        var output = new MemoryStream();
        foreach (Node child in directory.Children.Select(index => nodes[index]).OrderBy(node => node.Name, comparer))
        {
            byte[] name = Encoding.UTF8.GetBytes(child.Name);
            int recordSize = checked((name.Length + 16 + 7) & ~7);
            if (recordSize > blockSize) throw new InvalidDataException($"PFS name is too large: {child.RelativePath}");
            long positionInBlock = output.Position % blockSize;
            if (positionInBlock + recordSize > blockSize)
                output.Write(new byte[checked((int)(blockSize - positionInBlock))]);
            byte[] recordBytes = new byte[recordSize];
            Span<byte> record = recordBytes;
            BinaryPrimitives.WriteUInt32LittleEndian(record, checked((uint)child.Index));
            BinaryPrimitives.WriteInt32LittleEndian(record[4..], child.IsDirectory ? 3 : 2);
            BinaryPrimitives.WriteInt32LittleEndian(record[8..], name.Length);
            BinaryPrimitives.WriteInt32LittleEndian(record[12..], recordSize);
            name.CopyTo(record[16..]);
            output.Write(record);
        }
        if (output.Length == 0) output.Write(new byte[16]);
        return output.ToArray();
    }

    private static async Task WriteAsync(string source, string outputPath, Layout layout,
        SonyPfsBuildOptions options, IProgress<SonyPfsBuildProgress>? progress,
        CancellationToken cancellationToken)
    {
        int blockSize = options.BlockSize;
        await using var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] block = new byte[blockSize];
        WriteSuperblock(block, layout, options);
        await output.WriteAsync(block, cancellationToken).ConfigureAwait(false);

        byte[] inodeTable = new byte[checked(layout.InodeBlocks * blockSize)];
        for (int index = 0; index < layout.Nodes.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteInode(inodeTable.AsSpan(index * InodeSize, InodeSize), layout.Nodes[index]);
        }
        await output.WriteAsync(inodeTable, cancellationToken).ConfigureAwait(false);

        long completed = 0;
        foreach (Node node in layout.Nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new SonyPfsBuildProgress("Writing PFS", completed, layout.SourceBytes,
                node.RelativePath.Length == 0 ? "uroot" : node.RelativePath));
            long start = output.Position;
            if (node.IsDirectory)
                await output.WriteAsync(node.DirectoryData, cancellationToken).ConfigureAwait(false);
            else
            {
                await using var input = new FileStream(Path.Combine(source, node.RelativePath.Replace('/', Path.DirectorySeparatorChar)),
                    FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                await input.CopyToAsync(output, 1024 * 1024, cancellationToken).ConfigureAwait(false);
                completed = checked(completed + node.Size);
            }
            long allocated = checked((long)node.BlockCount * blockSize);
            long written = output.Position - start;
            await WriteZerosAsync(output, allocated - written, block, cancellationToken).ConfigureAwait(false);
        }
        if (output.Position != layout.ImageSize) throw new InvalidDataException("PFS writer produced an unexpected image length.");
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(new SonyPfsBuildProgress("PFS complete", layout.SourceBytes, layout.SourceBytes, string.Empty));
    }

    private static void WriteSuperblock(Span<byte> block, Layout layout, SonyPfsBuildOptions options)
    {
        BinaryPrimitives.WriteInt64LittleEndian(block, 2);
        BinaryPrimitives.WriteInt64LittleEndian(block[8..], 20130315);
        BinaryPrimitives.WriteUInt16LittleEndian(block[0x1C..], options.CaseInsensitive ? (ushort)0x0008 : (ushort)0);
        BinaryPrimitives.WriteUInt32LittleEndian(block[0x20..], checked((uint)options.BlockSize));
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
        BinaryPrimitives.WriteUInt32LittleEndian(inode[0x60..], node.BlockCount);
        BinaryPrimitives.WriteInt32LittleEndian(inode[0x64..], node.FirstBlock);
        for (int index = 1; index < 12; index++)
            BinaryPrimitives.WriteInt32LittleEndian(inode[(0x64 + index * 4)..], -1);
        for (int index = 0; index < 5; index++)
            BinaryPrimitives.WriteInt32LittleEndian(inode[(0x94 + index * 4)..], -1);
    }

    private static async Task WriteZerosAsync(Stream output, long count, byte[] zeroBuffer,
        CancellationToken cancellationToken)
    {
        if (count < 0) throw new InvalidDataException("PFS payload exceeded its allocated blocks.");
        Array.Clear(zeroBuffer);
        while (count > 0)
        {
            int write = checked((int)Math.Min(count, zeroBuffer.Length));
            await output.WriteAsync(zeroBuffer.AsMemory(0, write), cancellationToken).ConfigureAwait(false);
            count -= write;
        }
    }

    private static void ValidateBlockSize(int value)
    {
        if (value < 0x1000 || value > 0x100000 || (value & (value - 1)) != 0)
            throw new ArgumentOutOfRangeException(nameof(value), "PFS block size must be a power of two from 4 KiB through 1 MiB.");
    }

    private static void ValidateRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Split('/').Any(part => part is "" or "." or ".."))
            throw new InvalidDataException($"Unsafe PFS source path: {path}");
        if (path.Any(char.IsControl)) throw new InvalidDataException($"PFS source path contains control characters: {path}");
    }

    private static string Parent(string path) => path.Contains('/') ? path[..path.LastIndexOf('/')] : string.Empty;
    private static string Name(string path) => path.Contains('/') ? path[(path.LastIndexOf('/') + 1)..] : path;
    private static long DivideRoundUp(long value, long divisor) => checked((value + divisor - 1) / divisor);

    private sealed class Node(int index, string name, string relativePath, bool isDirectory, int parentIndex)
    {
        public int Index { get; } = index;
        public string Name { get; } = name;
        public string RelativePath { get; } = relativePath;
        public bool IsDirectory { get; } = isDirectory;
        public int ParentIndex { get; } = parentIndex;
        public List<int> Children { get; } = [];
        public byte[] DirectoryData { get; set; } = [];
        public long Size { get; set; }
        public int FirstBlock { get; set; }
        public uint BlockCount { get; set; }
    }

    private sealed record Layout(IReadOnlyList<Node> Nodes, int InodeBlocks, long TotalBlocks,
        long ImageSize, long SourceBytes);
}
