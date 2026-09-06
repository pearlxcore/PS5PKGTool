using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Core.Parsers;

internal sealed class SonyDebugPfsReader
{
    private const int BlockSize = 0x10000;
    private const int Signed32InodeSize = 0x2C8;
    private const int MaximumInodes = 2_000_000;
    private const int MaximumFiles = 2_000_000;
    private const int MaximumDepth = 256;

    public SonyPfsSummary? TryIndex(string path, long imageOffset, long imageSize, long superblockOffset,
        string contentId, string passcode)
    {
        if (imageSize <= 0 || imageSize % BlockSize != 0 || superblockOffset < imageOffset ||
            (superblockOffset - imageOffset) % BlockSize != 0 || contentId.Length != 36) return null;
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, BlockSize, FileOptions.RandomAccess);
        byte[] superblock = ReadRaw(stream, superblockOffset, BlockSize);
        if (BinaryPrimitives.ReadInt64LittleEndian(superblock.AsSpan(0x00, 8)) != 2 ||
            BinaryPrimitives.ReadInt64LittleEndian(superblock.AsSpan(0x08, 8)) != 20130315) return null;
        ushort mode = BinaryPrimitives.ReadUInt16LittleEndian(superblock.AsSpan(0x1C, 2));
        uint declaredBlockSize = BinaryPrimitives.ReadUInt32LittleEndian(superblock.AsSpan(0x20, 4));
        long inodeCount64 = BinaryPrimitives.ReadInt64LittleEndian(superblock.AsSpan(0x30, 8));
        long dataBlockCount = BinaryPrimitives.ReadInt64LittleEndian(superblock.AsSpan(0x38, 8));
        long inodeBlockCount64 = BinaryPrimitives.ReadInt64LittleEndian(superblock.AsSpan(0x40, 8));
        if (declaredBlockSize != BlockSize || inodeCount64 <= 0 || inodeCount64 > MaximumInodes ||
            inodeBlockCount64 <= 0 || inodeBlockCount64 > imageSize / BlockSize) return null;
        if ((mode & 0x0001) == 0 || (mode & 0x0002) != 0 || (mode & 0x0004) == 0) return null;

        int inodeCount = checked((int)inodeCount64);
        int inodeBlockCount = checked((int)inodeBlockCount64);
        long superblockRelative = superblockOffset - imageOffset;
        byte[] ekpfs = SonyPfsCrypto.DeriveEkpfs(contentId, passcode);
        ReadOnlySpan<byte> seed = superblock.AsSpan(0x370, 16);
        var failures = new List<string>();
        foreach (bool newCrypt in new[] { true, false })
        {
            SonyPfsCryptoContext context = SonyPfsCrypto.DeriveContext(ekpfs, seed, BlockSize, newCrypt);
            try
            {
                Inode[] inodes = ReadInodes(stream, imageOffset, imageSize, superblockRelative,
                    inodeCount, inodeBlockCount, context);
                var files = new List<SonyPfsEntry>();
                ReadDirectory(stream, imageOffset, imageSize, inodes, 0, string.Empty, 0,
                    new HashSet<uint>(), files, context);
                if (files.Count == 0) continue;
                return new SonyPfsSummary
                {
                    AccessState = SonyPfsAccessState.PlaintextIndexed,
                    ImageOffset = imageOffset,
                    ImageSize = imageSize,
                    Version = 2,
                    Magic = 20130315,
                    Mode = mode,
                    BlockSize = BlockSize,
                    InodeCount = inodeCount,
                    DataBlockCount = dataBlockCount,
                    StatusMessage = $"Derived the debug PFS key from the 32-zero passcode and indexed {files.Count:N0} outer file(s).",
                    Files = files.OrderBy(file => file.RelativePath, StringComparer.OrdinalIgnoreCase).ToArray(),
                    CryptoContext = context
                };
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or OverflowException or CryptographicException)
            {
                failures.Add((newCrypt ? "new" : "classic") + ": " + ex.Message);
            }
        }
        return new SonyPfsSummary
        {
            AccessState = SonyPfsAccessState.Invalid, ImageOffset = imageOffset, ImageSize = imageSize,
            Version = 2, Magic = 20130315, Mode = mode, BlockSize = BlockSize,
            InodeCount = inodeCount, DataBlockCount = dataBlockCount,
            StatusMessage = "Debug PFS key derivation failed validation: " + string.Join(" | ", failures)
        };
    }

    private static Inode[] ReadInodes(FileStream stream, long imageOffset, long imageSize,
        long superblockRelative, int inodeCount, int inodeBlockCount, SonyPfsCryptoContext context)
    {
        long tableRelative = checked(superblockRelative + BlockSize);
        if (tableRelative < 0 || checked(tableRelative + (long)inodeBlockCount * BlockSize) > imageSize)
            throw new InvalidDataException("The debug PFS inode table is outside the image.");
        var inodes = new Inode[inodeCount];
        int perBlock = BlockSize / Signed32InodeSize;
        int written = 0;
        for (int tableBlock = 0; tableBlock < inodeBlockCount && written < inodeCount; tableBlock++)
        {
            long relative = checked(tableRelative + (long)tableBlock * BlockSize);
            byte[] data = ReadDecryptedBlock(stream, imageOffset, imageSize, relative, context, true);
            for (int slot = 0; slot < perBlock && written < inodeCount; slot++, written++)
            {
                ReadOnlySpan<byte> record = data.AsSpan(slot * Signed32InodeSize, Signed32InodeSize);
                ushort mode = BinaryPrimitives.ReadUInt16LittleEndian(record[0x00..0x02]);
                uint flags = BinaryPrimitives.ReadUInt32LittleEndian(record[0x04..0x08]);
                long size = BinaryPrimitives.ReadInt64LittleEndian(record[0x08..0x10]);
                long storedSize = BinaryPrimitives.ReadInt64LittleEndian(record[0x10..0x18]);
                uint blocks = BinaryPrimitives.ReadUInt32LittleEndian(record[0x60..0x64]);
                int[] direct = new int[12];
                for (int index = 0; index < direct.Length; index++)
                    direct[index] = BinaryPrimitives.ReadInt32LittleEndian(record.Slice(0x64 + index * 36 + 32, 4));
                inodes[written] = new Inode(mode, flags, size, storedSize, blocks, direct);
            }
        }
        if (written != inodeCount) throw new InvalidDataException("The debug PFS inode table ended unexpectedly.");
        return inodes;
    }

    private static void ReadDirectory(FileStream stream, long imageOffset, long imageSize, Inode[] inodes,
        uint inodeNumber, string parent, int depth, HashSet<uint> active, List<SonyPfsEntry> files,
        SonyPfsCryptoContext context)
    {
        if (depth > MaximumDepth || inodeNumber >= inodes.Length || !active.Add(inodeNumber))
            throw new InvalidDataException("The debug PFS directory tree is invalid.");
        try
        {
            Inode inode = inodes[inodeNumber];
            if (inode.Blocks == 0 || inode.Direct[0] <= 0)
                throw new InvalidDataException($"A debug PFS directory inode is invalid (inode {inodeNumber}, blocks {inode.Blocks}, first {inode.Direct[0]}, mode 0x{inode.Mode:X4}, size {inode.Size}).");
            long remaining = inode.Size;
            for (uint index = 0; index < inode.Blocks && remaining > 0; index++)
            {
                int block = ResolveBlock(inode, index);
                byte[] data = ReadDecryptedBlock(stream, imageOffset, imageSize, checked((long)block * BlockSize), context, true);
                int limit = checked((int)Math.Min(BlockSize, remaining));
                int cursor = 0;
                while (cursor + 16 <= limit)
                {
                    uint child = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(cursor, 4));
                    int type = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 4, 4));
                    int nameLength = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 8, 4));
                    int entrySize = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(cursor + 12, 4));
                    if (entrySize == 0) break;
                    if (entrySize < 16 || entrySize > limit - cursor || nameLength <= 0 || nameLength > entrySize - 16)
                        throw new InvalidDataException("A debug PFS directory entry is malformed.");
                    string name = Encoding.UTF8.GetString(data, cursor + 16, nameLength).TrimEnd('\0');
                    if (type is 2 or 3)
                    {
                        ValidateName(name);
                        if (child >= inodes.Length) throw new InvalidDataException("A debug PFS entry references an invalid inode.");
                        string path = string.IsNullOrEmpty(parent) ? name : parent + "/" + name;
                        if (type == 3)
                            ReadDirectory(stream, imageOffset, imageSize, inodes, child,
                                depth == 0 && name == "uroot" ? string.Empty : path, depth + 1, active, files, context);
                        else
                        {
                            if (files.Count >= MaximumFiles) throw new InvalidDataException("Debug PFS file count exceeds the safety limit.");
                            files.Add(CreateFile(path, inodes[child], imageSize));
                        }
                    }
                    cursor += entrySize;
                }
                remaining -= limit;
            }
        }
        finally { active.Remove(inodeNumber); }
    }

    private static SonyPfsEntry CreateFile(string path, Inode inode, long imageSize)
    {
        if (inode.Size < 0 || inode.StoredSize < 0) throw new InvalidDataException("A debug PFS file size is invalid.");
        long remaining = inode.Size;
        var extents = new List<SonyPfsExtent>();
        for (uint index = 0; index < inode.Blocks && remaining > 0; index++)
        {
            int block = ResolveBlock(inode, index);
            long offset = checked((long)block * BlockSize);
            long length = Math.Min(BlockSize, remaining);
            if (offset < 0 || offset > imageSize || length > imageSize - offset)
                throw new InvalidDataException("A debug PFS file extent is outside the image.");
            extents.Add(new SonyPfsExtent(offset, length));
            remaining -= length;
        }
        if (remaining != 0) throw new InvalidDataException("A debug PFS file has too few blocks.");
        return new SonyPfsEntry
        {
            RelativePath = path,
            Size = inode.Size,
            StoredSize = inode.StoredSize,
            Flags = inode.Flags,
            Extents = extents,
            UsesPlainDataSector = Path.GetFileName(path).Equals("pfs_image.dat", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static int ResolveBlock(Inode inode, uint index)
    {
        if (index < inode.Direct.Length && inode.Direct[index] >= 0) return inode.Direct[index];
        if (inode.Direct[0] < 0) throw new InvalidDataException("A debug PFS inode has no first block.");
        return checked(inode.Direct[0] + (int)index);
    }

    private static byte[] ReadDecryptedBlock(FileStream stream, long imageOffset, long imageSize,
        long relativeOffset, SonyPfsCryptoContext context, bool signedDomain)
    {
        if (relativeOffset < 0 || relativeOffset % BlockSize != 0 || relativeOffset > imageSize - BlockSize)
            throw new InvalidDataException("A debug PFS block is outside the image.");
        byte[] data = ReadRaw(stream, checked(imageOffset + relativeOffset), BlockSize);
        SonyPfsCrypto.DecryptBlock(data, context, checked((ulong)(relativeOffset / BlockSize)), signedDomain);
        return data;
    }

    private static byte[] ReadRaw(FileStream stream, long offset, int count)
    {
        byte[] data = new byte[count];
        stream.Position = offset;
        stream.ReadExactly(data);
        return data;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.Contains('/') || name.Contains('\\') || name.Any(char.IsControl))
            throw new InvalidDataException("A debug PFS entry name is unsafe.");
    }

    private sealed record Inode(ushort Mode, uint Flags, long Size, long StoredSize, uint Blocks, int[] Direct);
}
