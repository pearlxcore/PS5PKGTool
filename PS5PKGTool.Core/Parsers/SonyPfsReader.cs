using System.Buffers.Binary;
using System.Text;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Core.Parsers;

/// <summary>
/// Bounded read only parser for plaintext, superblock first PS5 PFS images.
/// Encrypted and data first layouts are classified but are not guessed or decrypted.
/// </summary>
public sealed class SonyPfsReader
{
    private const long PfsMagic = 20130315;
    private const long PfsVersion = 2;
    private const int SuperblockReadSize = 0x400;
    private const int UnsignedInodeSize = 0xA8;
    private const int MaximumInodes = 2_000_000;
    private const int MaximumFiles = 2_000_000;
    private const int MaximumDirectoryDepth = 256;
    private const int MaximumDirectoryBlocks = 1_000_000;

    public SonyPfsSummary Inspect(string packagePath, long imageOffset, long imageSize,
        long? absoluteSuperblockOffset = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        if (imageOffset < 0 || imageSize <= 0)
            return Status(SonyPfsAccessState.NotPresent, imageOffset, imageSize, "No nested PFS image is present.");

        try
        {
            using var stream = new FileStream(packagePath, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete, 64 * 1024, FileOptions.RandomAccess);
            ValidateRange(imageOffset, imageSize, stream.Length, "PFS image");
            byte[] header = ReadAt(stream, imageOffset, SuperblockReadSize, imageOffset, imageSize, "PFS superblock");
            long version = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x00, 8));
            long magic = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x08, 8));
            ushort mode = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(0x1C, 2));
            uint blockSize = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x20, 4));
            long inodeCount = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x30, 8));
            long dataBlocks = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x38, 8));
            long inodeBlockCount = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x40, 8));

            bool dataFirst = version != PfsVersion || magic != PfsMagic;
            if (dataFirst && absoluteSuperblockOffset is long superblockOffset &&
                superblockOffset >= imageOffset && superblockOffset <= checked(imageOffset + imageSize - SuperblockReadSize))
            {
                header = ReadAt(stream, superblockOffset, SuperblockReadSize, imageOffset, imageSize, "PFS data first superblock");
                version = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x00, 8));
                magic = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x08, 8));
                mode = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(0x1C, 2));
                blockSize = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0x20, 4));
                inodeCount = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x30, 8));
                dataBlocks = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x38, 8));
                inodeBlockCount = BinaryPrimitives.ReadInt64LittleEndian(header.AsSpan(0x40, 8));
            }
            if (version != PfsVersion || magic != PfsMagic)
                return new SonyPfsSummary
                {
                    AccessState = SonyPfsAccessState.UnsupportedLayout, ImageOffset = imageOffset,
                    ImageSize = imageSize, Version = version, Magic = magic,
                    StatusMessage = "No readable PFS superblock was found at the image start or the FIH superblock offset."
                };

            var basic = new SonyPfsSummary
            {
                ImageOffset = imageOffset,
                ImageSize = imageSize,
                Version = version,
                Magic = magic,
                Mode = mode,
                BlockSize = blockSize,
                InodeCount = inodeCount,
                DataBlockCount = dataBlocks
            };
            ValidateHeader(blockSize, inodeCount, dataBlocks, inodeBlockCount, imageSize);

            if ((mode & 0x0004) != 0)
                return CopyStatus(basic, SonyPfsAccessState.EncryptedKeyRequired,
                    dataFirst
                        ? "The data first nested PFS is AES XTS encrypted. A matching image key is required before it can be indexed."
                        : "The nested PFS is AES XTS encrypted. A matching image key is required before it can be indexed.");
            if (dataFirst)
                return CopyStatus(basic, SonyPfsAccessState.UnsupportedLayout,
                    "The data first nested PFS superblock is readable, but its payload reconstruction is not implemented yet.");
            if ((mode & 0x0001) != 0 || (mode & 0x0002) != 0)
                return CopyStatus(basic, SonyPfsAccessState.UnsupportedLayout,
                    "This plaintext PFS uses signed or 64 bit inodes, which are not indexed by the bounded reader yet.");

            Inode[] inodes = ReadUnsignedInodes(stream, imageOffset, imageSize, blockSize,
                checked((int)inodeCount), checked((int)inodeBlockCount));
            var files = new List<SonyPfsEntry>();
            var activeDirectories = new HashSet<uint>();
            ReadDirectory(stream, imageOffset, imageSize, blockSize, inodes, 0, string.Empty,
                0, activeDirectories, files);

            int compressedCount = files.Count(file => file.IsCompressed);
            return new SonyPfsSummary
            {
                AccessState = SonyPfsAccessState.PlaintextIndexed,
                ImageOffset = imageOffset,
                ImageSize = imageSize,
                Version = version,
                Magic = magic,
                Mode = mode,
                BlockSize = blockSize,
                InodeCount = inodeCount,
                DataBlockCount = dataBlocks,
                StatusMessage = compressedCount == 0
                    ? $"Indexed {files.Count:N0} file(s) from the plaintext nested PFS."
                    : $"Indexed {files.Count:N0} file(s) from the plaintext nested PFS; {compressedCount:N0} compressed file(s) are listed by stored extent.",
                Files = files.OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray()
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or OverflowException)
        {
            return Status(SonyPfsAccessState.Invalid, imageOffset, imageSize, ex.Message);
        }
    }

    private static Inode[] ReadUnsignedInodes(FileStream stream, long imageOffset, long imageSize,
        uint blockSize, int inodeCount, int inodeBlockCount)
    {
        long tableOffset = checked((long)blockSize);
        long tableBytes = checked((long)inodeBlockCount * blockSize);
        ValidateImageRange(tableOffset, tableBytes, imageSize, "PFS inode table");
        if (checked((long)inodeCount * UnsignedInodeSize) > tableBytes)
            throw new InvalidDataException("The PFS inode table is too small for its declared inode count.");

        var result = new Inode[inodeCount];
        byte[] record = new byte[UnsignedInodeSize];
        for (int index = 0; index < inodeCount; index++)
        {
            long relative = checked(tableOffset + (long)index * UnsignedInodeSize);
            ReadAt(stream, checked(imageOffset + relative), record, imageOffset, imageSize, "PFS inode");
            ushort mode = BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(0x00, 2));
            uint flags = BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan(0x04, 4));
            long size = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(0x08, 8));
            long storedSize = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(0x10, 8));
            uint blocks = BinaryPrimitives.ReadUInt32LittleEndian(record.AsSpan(0x60, 4));
            int[] direct = new int[12];
            for (int block = 0; block < direct.Length; block++)
                direct[block] = BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(0x64 + block * 4, 4));
            result[index] = new Inode(mode, flags, size, storedSize, blocks, direct);
        }
        return result;
    }

    private static void ReadDirectory(FileStream stream, long imageOffset, long imageSize, uint blockSize,
        Inode[] inodes, uint inodeNumber, string parentPath, int depth, HashSet<uint> activeDirectories,
        List<SonyPfsEntry> files)
    {
        if (depth > MaximumDirectoryDepth) throw new InvalidDataException("PFS directory nesting exceeds the safety limit.");
        if (inodeNumber >= inodes.Length) throw new InvalidDataException("A PFS directory references an invalid inode.");
        if (!activeDirectories.Add(inodeNumber)) throw new InvalidDataException("A PFS directory cycle was detected.");
        try
        {
            Inode inode = inodes[inodeNumber];
            if (inode.Blocks is 0 or > MaximumDirectoryBlocks)
                throw new InvalidDataException("A PFS directory has an invalid block count.");
            if (inode.DirectBlocks[0] <= 0)
                throw new InvalidDataException("A PFS directory has an invalid first block.");

            long remainingDirectoryBytes = inode.Size;
            for (uint blockIndex = 0; blockIndex < inode.Blocks && remainingDirectoryBytes > 0; blockIndex++)
            {
                int block = ResolveContiguousBlock(inode, blockIndex);
                long blockOffset = checked((long)block * blockSize);
                int bytesToRead = checked((int)Math.Min(blockSize, remainingDirectoryBytes));
                byte[] data = ReadAt(stream, checked(imageOffset + blockOffset), bytesToRead,
                    imageOffset, imageSize, "PFS directory block");
                int cursor = 0;
                while (cursor + 16 <= data.Length)
                {
                    uint childInode = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(cursor, 4));
                    int type = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 4, 4));
                    int nameLength = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 8, 4));
                    int entrySize = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 12, 4));
                    if (entrySize == 0) break;
                    if (entrySize < 16 || entrySize > data.Length - cursor || nameLength < 0 || nameLength > entrySize - 16)
                        throw new InvalidDataException("A PFS directory entry is malformed.");
                    string name = Encoding.UTF8.GetString(data, cursor + 16, nameLength).TrimEnd('\0');
                    ValidateName(name);
                    if (type is 2 or 3)
                    {
                        if (childInode >= inodes.Length) throw new InvalidDataException("A PFS entry references an invalid inode.");
                        string path = Combine(parentPath, name);
                        if (type == 3)
                            ReadDirectory(stream, imageOffset, imageSize, blockSize, inodes, childInode,
                                depth == 0 && name.Equals("uroot", StringComparison.Ordinal) ? string.Empty : path,
                                depth + 1, activeDirectories, files);
                        else
                        {
                            if (files.Count >= MaximumFiles) throw new InvalidDataException("PFS file count exceeds the safety limit.");
                            files.Add(CreateFile(path, inodes[childInode], blockSize, imageSize));
                        }
                    }
                    cursor += entrySize;
                }
                remainingDirectoryBytes -= bytesToRead;
            }
        }
        finally { activeDirectories.Remove(inodeNumber); }
    }

    private static SonyPfsEntry CreateFile(string path, Inode inode, uint blockSize, long imageSize)
    {
        if (inode.Size < 0 || inode.StoredSize < 0) throw new InvalidDataException("A PFS file has a negative size.");
        bool compressed = (inode.Flags & 1) != 0;
        long remaining = compressed ? inode.StoredSize : inode.Size;
        var extents = new List<SonyPfsExtent>();
        for (uint index = 0; index < inode.Blocks && remaining > 0; index++)
        {
            int block = ResolveContiguousBlock(inode, index);
            long offset = checked((long)block * blockSize);
            long length = Math.Min(blockSize, remaining);
            ValidateImageRange(offset, length, imageSize, $"PFS file '{path}'");
            if (extents.Count > 0 && extents[^1].Offset + extents[^1].Length == offset)
            {
                SonyPfsExtent previous = extents[^1];
                extents[^1] = new SonyPfsExtent(previous.Offset, previous.Length + length);
            }
            else extents.Add(new SonyPfsExtent(offset, length));
            remaining -= length;
        }
        if (remaining != 0) throw new InvalidDataException($"PFS file '{path}' has too few data blocks.");
        return new SonyPfsEntry { RelativePath = path, Size = inode.Size, StoredSize = inode.StoredSize, Flags = inode.Flags, Extents = extents };
    }

    private static int ResolveContiguousBlock(Inode inode, uint index)
    {
        if (index < inode.DirectBlocks.Length && inode.DirectBlocks[index] > 0) return inode.DirectBlocks[index];
        if (inode.DirectBlocks[0] <= 0) throw new InvalidDataException("A PFS inode has no valid data block.");
        return checked(inode.DirectBlocks[0] + (int)index);
    }

    private static void ValidateHeader(uint blockSize, long inodeCount, long dataBlocks, long inodeBlockCount, long imageSize)
    {
        if (blockSize < 0x1000 || blockSize > 0x100000 || (blockSize & (blockSize - 1)) != 0)
            throw new InvalidDataException("The PFS block size is invalid.");
        if (inodeCount <= 0 || inodeCount > MaximumInodes) throw new InvalidDataException("The PFS inode count is invalid.");
        if (dataBlocks <= 0 || dataBlocks > imageSize / blockSize + 1) throw new InvalidDataException("The PFS data block count is invalid.");
        if (inodeBlockCount <= 0 || inodeBlockCount > imageSize / blockSize) throw new InvalidDataException("The PFS inode block count is invalid.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name) || name is "." or "..") throw new InvalidDataException("A PFS entry has an unsafe name.");
        if (name.Contains('/') || name.Contains('\\') || name.Any(char.IsControl)) throw new InvalidDataException("A PFS entry has an unsafe name.");
    }

    private static string Combine(string parent, string name) => string.IsNullOrEmpty(parent) ? name : parent + "/" + name;

    private static SonyPfsSummary Status(SonyPfsAccessState state, long offset, long size, string message) =>
        new() { AccessState = state, ImageOffset = offset, ImageSize = size, StatusMessage = message };

    private static SonyPfsSummary CopyStatus(SonyPfsSummary source, SonyPfsAccessState state, string message) => new()
    {
        AccessState = state, ImageOffset = source.ImageOffset, ImageSize = source.ImageSize,
        Version = source.Version, Magic = source.Magic, Mode = source.Mode, BlockSize = source.BlockSize,
        InodeCount = source.InodeCount, DataBlockCount = source.DataBlockCount, StatusMessage = message
    };

    private static byte[] ReadAt(FileStream stream, long absoluteOffset, int count, long imageOffset,
        long imageSize, string description)
    {
        byte[] result = new byte[count];
        ReadAt(stream, absoluteOffset, result, imageOffset, imageSize, description);
        return result;
    }

    private static void ReadAt(FileStream stream, long absoluteOffset, byte[] result, long imageOffset,
        long imageSize, string description)
    {
        long relative = checked(absoluteOffset - imageOffset);
        ValidateImageRange(relative, result.LongLength, imageSize, description);
        stream.Position = absoluteOffset;
        stream.ReadExactly(result);
    }

    private static void ValidateRange(long offset, long size, long length, string description)
    {
        if (offset < 0 || size < 0 || offset > length || size > length - offset)
            throw new InvalidDataException($"The {description} range is outside the package.");
    }

    private static void ValidateImageRange(long offset, long size, long imageSize, string description)
    {
        if (offset < 0 || size < 0 || offset > imageSize || size > imageSize - offset)
            throw new InvalidDataException($"The {description} range is outside the nested PFS image.");
    }

    private sealed record Inode(ushort Mode, uint Flags, long Size, long StoredSize, uint Blocks, int[] DirectBlocks);
}
