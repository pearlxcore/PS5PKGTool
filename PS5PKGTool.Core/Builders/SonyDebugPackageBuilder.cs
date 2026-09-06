using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;

namespace PS5PKGTool.Core.Builders;

public sealed class SonyDebugPackageBuildOptions
{
    public required string ContentId { get; init; }
    public string Passcode { get; init; } = SonyDebugPackageCredentials.DefaultPasscode;
}

public readonly record struct SonyDebugPackageProgress(string Stage, long CompletedBytes,
    long TotalBytes, string CurrentPath);

public sealed class SonyDebugPackageBuildResult
{
    public required string OutputPath { get; init; }
    public required string ContentId { get; init; }
    public required string KeyFingerprint { get; init; }
    public required long PackageSize { get; init; }
    public required long SourceBytes { get; init; }
    public required int SourceFiles { get; init; }
    public required bool UsesDefaultPasscode { get; init; }
}

public sealed class SonyDebugPackageValidationResult
{
    public required bool IsValid { get; init; }
    public required string Message { get; init; }
    public required int IndexedFiles { get; init; }
    public required long PackageSize { get; init; }
}

public static class SonyDebugPackageBuilder
{
    private const int BlockSize = 0x10000;
    private const int SignedInodeSize = 0x2C8;
    private const int FihSize = BlockSize;

    public static async Task<SonyDebugPackageBuildResult> CreateFromDirectoryAsync(string sourceDirectory,
        string outputPath, SonyDebugPackageBuildOptions options,
        IProgress<SonyDebugPackageProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(options);
        string source = Path.GetFullPath(sourceDirectory);
        string destination = Path.GetFullPath(outputPath);
        if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
        SonyDebugPackageCredentials credentials = SonyDebugPackageCredentials.Create(options.ContentId, options.Passcode);
        ValidateSourceIdentity(source, credentials.ContentId);
        FileInfo[] sourceFiles = Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path)).ToArray();
        long sourceBytes = sourceFiles.Sum(file => file.Length);
        if (sourceFiles.Length == 0) throw new InvalidDataException("The selected game dump is empty.");

        string? parent = Path.GetDirectoryName(destination);
        if (string.IsNullOrWhiteSpace(parent)) throw new ArgumentException("Output path has no parent directory.", nameof(outputPath));
        Directory.CreateDirectory(parent);
        string token = Guid.NewGuid().ToString("N");
        string innerPath = Path.Combine(parent, "." + Path.GetFileName(destination) + "." + token + ".inner.tmp");
        string outerPath = Path.Combine(parent, "." + Path.GetFileName(destination) + "." + token + ".outer.tmp");
        string packagePath = Path.Combine(parent, "." + Path.GetFileName(destination) + "." + token + ".pkg.tmp");
        try
        {
            SonyDataFirstPfsBuildResult inner = await SonyDataFirstPfsImageBuilder.CreateAsync(source,
                innerPath, progress, sourceBytes, cancellationToken).ConfigureAwait(false);
            byte[] innerDigest = HashFileSha3(innerPath);
            cancellationToken.ThrowIfCancellationRequested();

            byte[] naps = SonyNapsLayoutWriter.CreateStoredLayout(inner.ImageSize, inner.MetadataOffset);
            OuterPfsResult outer = await CreateOuterPfsAsync(innerPath, naps, outerPath, credentials,
                progress, sourceBytes, cancellationToken).ConfigureAwait(false);
            byte[] supplemental = CreateSupplementalIndex(inner);
            byte[] cnt = CreateCnt(source, credentials, outerPath, supplemental);
            await AssemblePackageAsync(packagePath, outerPath, cnt, outer.SuperblockRelativeOffset,
                outer.SuperblockDigest, innerDigest, progress, sourceBytes, cancellationToken).ConfigureAwait(false);

            SonyDebugPackageValidationResult validation = Validate(packagePath, credentials.Passcode);
            if (!validation.IsValid) throw new InvalidDataException("Created package did not pass validation: " + validation.Message);
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(packagePath, destination, true);
            return new SonyDebugPackageBuildResult
            {
                OutputPath = destination, ContentId = credentials.ContentId,
                KeyFingerprint = credentials.Fingerprint, PackageSize = new FileInfo(destination).Length,
                SourceBytes = sourceBytes, SourceFiles = sourceFiles.Length,
                UsesDefaultPasscode = credentials.UsesDefaultPasscode
            };
        }
        finally
        {
            DeleteTemporary(innerPath);
            DeleteTemporary(outerPath);
            DeleteTemporary(packagePath);
        }
    }

    public static SonyDebugPackageValidationResult Validate(string packagePath,
        string passcode = SonyDebugPackageCredentials.DefaultPasscode)
    {
        try
        {
            SonyPkgSummary package = new SonyPkgReader().Read(packagePath, passcode);
            SonyPfsSummary? outer = new SonyDebugPfsReader().TryIndex(packagePath,
                checked((long)package.PfsImageOffset), checked((long)package.PfsImageSize),
                checked((long)package.PfsSuperblockOffset), package.ContentId, passcode);
            SonyPfsEntry? image = outer?.Files.FirstOrDefault(file => Path.GetFileName(file.RelativePath)
                .Equals("pfs_image.dat", StringComparison.OrdinalIgnoreCase));
            SonyPfsEntry? napsEntry = outer?.Files.FirstOrDefault(file => Path.GetFileName(file.RelativePath)
                .Equals("naps_pkg_layout.dat", StringComparison.OrdinalIgnoreCase));
            bool napsValid = false;
            if (outer?.AccessState == SonyPfsAccessState.PlaintextIndexed && image is not null && napsEntry is not null)
            {
                byte[] naps = SonyPfsEntryDataReader.ReadAll(packagePath, package, outer, napsEntry, 32 * 1024 * 1024);
                using var decoder = new SonyOodleKrakenDecoder();
                var metadata = SonyInnerPfsMountReader.ReconstructMetadata(image.Size,
                    (offset, count) => SonyPfsEntryDataReader.ReadRange(packagePath, package, outer, image, offset, count),
                    naps, decoder);
                napsValid = SonyInnerPfsMountReader.FindSuperblock(metadata.Data) == 0;
            }
            SonyPkgEntry? imageDigestEntry = package.Entries.FirstOrDefault(entry => entry.Id == 0x040A);
            bool digestValid = imageDigestEntry is not null && !imageDigestEntry.IsEncrypted &&
                new SonyPkgReader().ReadEntryBytes(packagePath, package, imageDigestEntry, 64 * 1024 * 1024)
                    .SequenceEqual(BuildBlockDigests(packagePath, checked((long)package.PfsImageOffset), checked((long)package.PfsImageSize)));
            bool fihValid = outer is not null && image is not null &&
                            ValidateFihDigests(packagePath, package, outer, image);
            bool cntValid = ValidateCntDigests(packagePath, package);
            bool valid = package.Kind == SonyPkgKind.FinalizedDebug && package.ContentId.Length == 36 &&
                         package.PfsImageSize > 0 && package.PfsSuperblockOffset > package.PfsImageOffset &&
                         package.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed &&
                         package.NestedPfs.Files.Count > 0 && napsValid && digestValid && fihValid && cntValid;
            return new SonyDebugPackageValidationResult
            {
                IsValid = valid,
                Message = valid ? "FIH, CNT self seals, block and image digests, encrypted outer PFS, passcode derivation, NAPS reconstruction, and file index are valid."
                    : package.NestedPfs?.StatusMessage ?? "The package structure is incomplete.",
                IndexedFiles = package.NestedPfs?.Files.Count ?? 0,
                PackageSize = package.FileSize
            };
        }
        catch (Exception ex)
        {
            return new SonyDebugPackageValidationResult { IsValid = false, Message = ex.Message, IndexedFiles = 0,
                PackageSize = File.Exists(packagePath) ? new FileInfo(packagePath).Length : 0 };
        }
    }

    private static async Task<OuterPfsResult> CreateOuterPfsAsync(string innerPath, byte[] naps,
        string outputPath, SonyDebugPackageCredentials credentials, IProgress<SonyDebugPackageProgress>? progress,
        long totalSourceBytes, CancellationToken cancellationToken)
    {
        long innerSize = new FileInfo(innerPath).Length;
        int innerBlocks = checked((int)DivideRoundUp(innerSize, BlockSize));
        int napsBlocks = checked((int)DivideRoundUp(naps.Length, BlockSize));
        int rootBlock = checked(innerBlocks + napsBlocks);
        int urootBlock = rootBlock + 1;
        int superblock = urootBlock + 1;
        int inodeBlocks = checked((int)DivideRoundUp(4L * SignedInodeSize, BlockSize));
        int totalBlocks = checked(superblock + 1 + inodeBlocks);
        byte[] seed = RandomNumberGenerator.GetBytes(16);
        SonyPfsCryptoContext crypto = SonyPfsCrypto.DeriveContext(credentials.ImageKey, seed, BlockSize, true);

        await using var output = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await using var inner = new FileStream(innerPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] block = new byte[BlockSize];
        var blockDigests = new List<byte[]>(totalBlocks);
        long copied = 0;
        for (int index = 0; index < innerBlocks; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Array.Clear(block);
            int wanted = checked((int)Math.Min(BlockSize, innerSize - copied));
            if (wanted > 0) await inner.ReadExactlyAsync(block.AsMemory(0, wanted), cancellationToken).ConfigureAwait(false);
            blockDigests.Add(SHA3_256.HashData(block));
            SonyPfsCrypto.EncryptBlock(block, crypto, checked((ulong)index), false);
            await output.WriteAsync(block, cancellationToken).ConfigureAwait(false);
            copied += wanted;
            progress?.Report(new SonyDebugPackageProgress("Encrypting inner PFS", Math.Min(copied, totalSourceBytes),
                totalSourceBytes, "pfs_image.dat"));
        }
        for (int index = 0; index < napsBlocks; index++)
        {
            Array.Clear(block);
            naps.AsSpan(index * BlockSize, Math.Min(BlockSize, naps.Length - index * BlockSize)).CopyTo(block);
            blockDigests.Add(SHA3_256.HashData(block));
            EncryptAndWrite(output, block, crypto, checked((ulong)(innerBlocks + index)), true);
        }
        byte[] rootDirectory = CreateDirectoryRecord(1, 3, "uroot");
        Array.Clear(block); rootDirectory.CopyTo(block, 0);
        blockDigests.Add(SHA3_256.HashData(block));
        EncryptAndWrite(output, block, crypto, checked((ulong)rootBlock), true);
        byte[] urootDirectory = CombineRecords(CreateDirectoryRecord(2, 2, "pfs_image.dat"),
            CreateDirectoryRecord(3, 2, "naps_pkg_layout.dat"));
        Array.Clear(block); urootDirectory.CopyTo(block, 0);
        blockDigests.Add(SHA3_256.HashData(block));
        EncryptAndWrite(output, block, crypto, checked((ulong)urootBlock), true);

        byte[] inodeTable = new byte[inodeBlocks * BlockSize];
        WriteSignedInode(inodeTable.AsSpan(0 * SignedInodeSize, SignedInodeSize), true, rootDirectory.Length, rootBlock, 1, false,
            blockDigests.Skip(rootBlock).Take(1).ToArray());
        WriteSignedInode(inodeTable.AsSpan(1 * SignedInodeSize, SignedInodeSize), true, urootDirectory.Length, urootBlock, 1, false,
            blockDigests.Skip(urootBlock).Take(1).ToArray());
        WriteSignedInode(inodeTable.AsSpan(2 * SignedInodeSize, SignedInodeSize), false, innerSize, 0, innerBlocks, false,
            blockDigests.Take(innerBlocks).ToArray());
        WriteSignedInode(inodeTable.AsSpan(3 * SignedInodeSize, SignedInodeSize), false, naps.Length, innerBlocks, napsBlocks, false,
            blockDigests.Skip(innerBlocks).Take(napsBlocks).ToArray());
        byte[] inodeTableDigest = SHA3_256.HashData(inodeTable);
        Array.Clear(block);
        WriteSuperblock(block, totalBlocks, inodeBlocks, seed, superblock + 1, inodeTableDigest);
        byte[] superblockDigest = SHA3_256.HashData(block);
        await output.WriteAsync(block, cancellationToken).ConfigureAwait(false);
        for (int index = 0; index < inodeBlocks; index++)
        {
            Span<byte> inodeBlock = inodeTable.AsSpan(index * BlockSize, BlockSize);
            SonyPfsCrypto.EncryptBlock(inodeBlock, crypto, checked((ulong)(superblock + 1 + index)), true);
            await output.WriteAsync(inodeBlock.ToArray(), cancellationToken).ConfigureAwait(false);
        }
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        return new OuterPfsResult(checked((long)superblock * BlockSize), checked((long)totalBlocks * BlockSize),
            superblockDigest);
    }

    private static void EncryptAndWrite(Stream output, byte[] block, SonyPfsCryptoContext crypto,
        ulong blockIndex, bool signedDomain)
    {
        SonyPfsCrypto.EncryptBlock(block, crypto, blockIndex, signedDomain);
        output.Write(block);
    }

    private static void WriteSuperblock(Span<byte> block, int totalBlocks, int inodeBlocks, byte[] seed,
        int inodeTableBlock, ReadOnlySpan<byte> inodeTableDigest)
    {
        BinaryPrimitives.WriteInt64LittleEndian(block, 2);
        BinaryPrimitives.WriteInt64LittleEndian(block[8..], 20130315);
        BinaryPrimitives.WriteUInt16LittleEndian(block[0x1C..], 0x000D);
        BinaryPrimitives.WriteUInt32LittleEndian(block[0x20..], BlockSize);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x28..], 1);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x30..], 4);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x38..], totalBlocks);
        BinaryPrimitives.WriteInt64LittleEndian(block[0x40..], inodeBlocks);
        seed.CopyTo(block[0x370..]);
        inodeTableDigest.CopyTo(block[0xB8..]);
        BinaryPrimitives.WriteInt32LittleEndian(block[0xD8..], inodeTableBlock);
        SHA3_256.HashData(block[..0x5A0]).CopyTo(block[0x380..]);
    }

    private static void WriteSignedInode(Span<byte> inode, bool directory, long size,
        int firstBlock, int blocks, bool compressed, IReadOnlyList<byte[]> blockDigests)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(inode, directory ? (ushort)0x4000 : (ushort)0x8000);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[2..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(inode[4..], compressed ? 1u : 0u);
        BinaryPrimitives.WriteInt64LittleEndian(inode[8..], size);
        BinaryPrimitives.WriteInt64LittleEndian(inode[0x10..], size);
        BinaryPrimitives.WriteUInt32LittleEndian(inode[0x60..], checked((uint)blocks));
        for (int index = 0; index < 12; index++)
        {
            if (index < blockDigests.Count) blockDigests[index].CopyTo(inode[(0x64 + index * 36)..]);
            BinaryPrimitives.WriteInt32LittleEndian(inode[(0x64 + index * 36 + 32)..], index < blocks ? firstBlock + index : -1);
        }
    }

    private static byte[] CreateDirectoryRecord(uint inode, int type, string name)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        int length = checked((nameBytes.Length + 16 + 7) & ~7);
        byte[] record = new byte[length];
        BinaryPrimitives.WriteUInt32LittleEndian(record, inode);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(4), type);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(8), nameBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(12), length);
        nameBytes.CopyTo(record, 16);
        return record;
    }

    private static byte[] CombineRecords(params byte[][] records)
    {
        byte[] output = new byte[records.Sum(record => record.Length)];
        int cursor = 0;
        foreach (byte[] record in records) { record.CopyTo(output, cursor); cursor += record.Length; }
        return output;
    }

    private static byte[] CreateSupplementalIndex(SonyDataFirstPfsBuildResult inner)
    {
        var text = new StringBuilder();
        using (var writer = XmlWriter.Create(text, new XmlWriterSettings { OmitXmlDeclaration = true }))
        {
            writer.WriteStartElement("pfs-image");
            writer.WriteStartElement("nested-image");
            writer.WriteStartElement("dir"); writer.WriteAttributeString("name", "uroot");
            WriteSupplementalTree(writer, inner.Files);
            writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteEndElement();
        }
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, leaveOpen: true))
        {
            byte[] geometry = new byte[48];
            ulong dataRegion = checked((ulong)Math.Max(0, inner.MetadataOffset - BlockSize));
            BinaryPrimitives.WriteUInt64LittleEndian(geometry.AsSpan(0x10), dataRegion);
            BinaryPrimitives.WriteUInt64LittleEndian(geometry.AsSpan(0x18), 0x3E9);
            BinaryPrimitives.WriteUInt64LittleEndian(geometry.AsSpan(0x20), dataRegion);
            BinaryPrimitives.WriteUInt64LittleEndian(geometry.AsSpan(0x28), BlockSize);
            foreach (string id in new[] { "300", "301", "302", "308" })
                WriteStoredZipEntry(zip, $"common/etc/naps_meta_{id}.dat", geometry);
            byte[] xml = Encoding.UTF8.GetBytes(text.ToString());
            WriteStoredZipEntry(zip, "common/etc/pfsimage.xml", xml);
        }
        return memory.ToArray();
    }

    private static void WriteStoredZipEntry(ZipArchive zip, string path, byte[] data)
    {
        ZipArchiveEntry entry = zip.CreateEntry(path, CompressionLevel.NoCompression);
        using Stream stream = entry.Open();
        stream.Write(data);
    }

    private static void WriteSupplementalTree(XmlWriter writer, IReadOnlyList<SonyDataFirstPfsFile> files)
    {
        var root = new SupplementalDirectory(string.Empty);
        foreach (SonyDataFirstPfsFile file in files)
        {
            string[] parts = file.RelativePath.Split('/');
            SupplementalDirectory current = root;
            for (int index = 0; index < parts.Length - 1; index++)
            {
                if (!current.Directories.TryGetValue(parts[index], out SupplementalDirectory? child))
                {
                    child = new SupplementalDirectory(parts[index]);
                    current.Directories.Add(parts[index], child);
                }
                current = child;
            }
            current.Files.Add((parts[^1], file));
        }
        WriteDirectoryChildren(writer, root, 0);
    }

    private static void WriteDirectoryChildren(XmlWriter writer, SupplementalDirectory directory, int depth)
    {
        if (depth > 256) throw new InvalidDataException("The supplemental PFS tree exceeds the directory depth limit.");
        foreach (SupplementalDirectory child in directory.Directories.Values.OrderBy(item => item.Name,
                     StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteStartElement("dir"); writer.WriteAttributeString("name", child.Name);
            WriteDirectoryChildren(writer, child, depth + 1);
            writer.WriteEndElement();
        }
        foreach ((string name, SonyDataFirstPfsFile file) in directory.Files.OrderBy(item => item.Name,
                     StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteStartElement("file");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("size", file.Size.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteAttributeString("offset", file.Offset.ToString(System.Globalization.CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }
    }

    private static byte[] CreateCnt(string source, SonyDebugPackageCredentials credentials,
        string outerPath, byte[] supplemental)
    {
        var payloads = new List<CntPayload>();
        payloads.Add(new CntPayload(0x0001, "digests.bin", []));
        payloads.Add(new CntPayload(0x0020, "image_key.bin", credentials.ImageKey.ToArray()));
        byte[] generalDigests = new byte[0x1E0];
        BinaryPrimitives.WriteUInt16BigEndian(generalDigests, 0x10DE);
        payloads.Add(new CntPayload(0x0080, "general_digests.bin", generalDigests));
        AddSource(payloads, source, 0x2000, "sce_sys/param.json");
        AddSource(payloads, source, 0x1000, "sce_sys/param.sfo");
        AddValidatedSystemSource(payloads, source, 0x0400, "sce_sys/license.dat");
        AddValidatedSystemSource(payloads, source, 0x0401, "sce_sys/license.info");
        AddValidatedSystemSource(payloads, source, 0x0402, "sce_sys/nptitle.dat");
        AddValidatedSystemSource(payloads, source, 0x0403, "sce_sys/npbind.dat");
        AddValidatedSystemSource(payloads, source, 0x0404, "sce_sys/selfinfo.dat");
        AddValidatedSystemSource(payloads, source, 0x0407, "sce_sys/target-deltainfo.dat");
        AddValidatedSystemSource(payloads, source, 0x0408, "sce_sys/origin-deltainfo.dat");
        AddSource(payloads, source, 0x1001, "sce_sys/playgo-chunk.dat");
        AddSource(payloads, source, 0x1002, "sce_sys/playgo-chunk.sha");
        AddSource(payloads, source, 0x1004, "sce_sys/pronunciation.xml");
        AddSource(payloads, source, 0x1005, "sce_sys/pronunciation.sig");
        AddSource(payloads, source, 0x1007, "sce_sys/pubtoolinfo.dat");
        AddSource(payloads, source, 0x1200, "sce_sys/icon0.png");
        AddSource(payloads, source, 0x1220, "sce_sys/pic0.png");
        AddSource(payloads, source, 0x1280, "sce_sys/icon0.dds");
        AddSource(payloads, source, 0x12A0, "sce_sys/pic0.dds");
        AddSource(payloads, source, 0x12C0, "sce_sys/pic1.dds");
        AddSource(payloads, source, 0x2060, "sce_sys/pic2.dds");
        AddSource(payloads, source, 0x2010, "sce_sys/playgo-hash-table.dat");
        AddSource(payloads, source, 0x2011, "sce_sys/playgo-ficm.dat");
        payloads.Add(new CntPayload(0x040A, string.Empty,
            BuildBlockDigests(outerPath, 0, new FileInfo(outerPath).Length)));
        payloads[0].Data = new byte[checked((payloads.Count + 1) * 32)];

        var nameBuilder = new MemoryStream(); nameBuilder.WriteByte(0);
        var nameOffsets = new Dictionary<string, uint>(StringComparer.Ordinal);
        nameOffsets["entry_names.bin"] = checked((uint)nameBuilder.Position);
        WriteCString(nameBuilder, "entry_names.bin");
        foreach (CntPayload payload in payloads)
        {
            if (payload.Name.Length == 0) continue;
            nameOffsets[payload.Name] = checked((uint)nameBuilder.Position);
            WriteCString(nameBuilder, payload.Name);
        }
        byte[] names = nameBuilder.ToArray();
        payloads.Insert(0, new CntPayload(0x0200, "entry_names.bin", names));

        int entryTableOffset = 0x5A0;
        int cursor = Align(Math.Max(entryTableOffset + payloads.Count * 0x20, 0x1100), 0x100);
        foreach (CntPayload payload in payloads) { payload.Offset = cursor; cursor = Align(checked(cursor + payload.Data.Length), 0x10); }
        int zipOffset = Align(cursor, 0x10);
        byte[] cnt = new byte[checked(zipOffset + supplemental.Length)];
        cnt[0] = 0x7F; cnt[1] = (byte)'C'; cnt[2] = (byte)'N'; cnt[3] = (byte)'T';
        WriteU32Be(cnt, 0x10, checked((uint)payloads.Count));
        WriteU16Be(cnt, 0x14, checked((ushort)payloads.Count));
        WriteU32Be(cnt, 0x18, checked((uint)entryTableOffset));
        WriteU64Be(cnt, 0x20, checked((ulong)payloads.Min(payload => payload.Offset)));
        WriteU64Be(cnt, 0x28, checked((ulong)(zipOffset - payloads.Min(payload => payload.Offset))));
        Encoding.ASCII.GetBytes(credentials.ContentId).CopyTo(cnt, 0x40);
        WriteU32Be(cnt, 0x74, 0x20);
        WriteU32Be(cnt, 0x78, 0x3);
        for (int index = 0; index < payloads.Count; index++)
        {
            CntPayload payload = payloads[index];
            int record = entryTableOffset + index * 0x20;
            WriteU32Be(cnt, record, payload.Id);
            WriteU32Be(cnt, record + 4, payload.Name.Length == 0 ? 0 : nameOffsets[payload.Name]);
            WriteU32Be(cnt, record + 0x10, checked((uint)payload.Offset));
            WriteU32Be(cnt, record + 0x14, checked((uint)payload.Data.Length));
            payload.Data.CopyTo(cnt, payload.Offset);
        }
        CntPayload digestTable = payloads.Single(payload => payload.Id == 0x0001);
        for (int index = 0; index < payloads.Count; index++)
        {
            if (payloads[index].Id == 0x0001) continue;
            SHA3_256.HashData(payloads[index].Data).CopyTo(digestTable.Data, index * 32);
        }
        digestTable.Data.CopyTo(cnt, digestTable.Offset);
        SHA3_256.HashData(cnt.AsSpan(0, 0xFE0)).CopyTo(cnt, 0xFE0);
        supplemental.CopyTo(cnt, zipOffset);
        return cnt;
    }

    private static async Task AssemblePackageAsync(string packagePath, string outerPath, byte[] cnt,
        long superblockRelative, byte[] superblockDigest, byte[] innerDigest,
        IProgress<SonyDebugPackageProgress>? progress, long sourceBytes,
        CancellationToken cancellationToken)
    {
        long outerSize = new FileInfo(outerPath).Length;
        long cntOffset = AlignLong(checked(FihSize + outerSize), BlockSize);
        byte[] fih = new byte[FihSize];
        fih[0] = 0x7F; fih[1] = (byte)'F'; fih[2] = (byte)'I'; fih[3] = (byte)'H';
        fih[4] = 1;
        fih[5] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(fih.AsSpan(6), 3);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x10), FihSize);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x18), checked((ulong)outerSize));
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x20), checked((ulong)(FihSize + superblockRelative)));
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x28), FihSize);
        superblockDigest.CopyTo(fih, 0x30);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x50), 0x40);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x58), checked((ulong)cntOffset));
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x60), FihSize);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0x68), SonyPfsCrypto.SignedSectorFlag);
        superblockDigest.CopyTo(fih, 0x70);
        BinaryPrimitives.WriteUInt64LittleEndian(fih.AsSpan(0xA0), checked((ulong)cnt.Length));
        innerDigest.CopyTo(fih, 0xB0);
        superblockDigest.CopyTo(fih, 0xD0);
        await using var output = new FileStream(packagePath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await output.WriteAsync(fih, cancellationToken).ConfigureAwait(false);
        await using (var outer = new FileStream(outerPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                         1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await outer.CopyToAsync(output, 1024 * 1024, cancellationToken).ConfigureAwait(false);
        await WriteZerosAsync(output, cntOffset - output.Position, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(cnt, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        progress?.Report(new SonyDebugPackageProgress("Validating package", sourceBytes, sourceBytes, Path.GetFileName(packagePath)));
    }

    private static void AddSource(List<CntPayload> payloads, string root, uint id, string relative)
    {
        string path = FindPath(root, relative);
        if (path.Length == 0) return;
        long length = new FileInfo(path).Length;
        if (length > 64 * 1024 * 1024) throw new InvalidDataException("A CNT metadata asset is unexpectedly large: " + relative);
        payloads.Add(new CntPayload(id, relative, File.ReadAllBytes(path)));
    }

    private static void AddValidatedSystemSource(List<CntPayload> payloads, string root, uint id, string relative)
    {
        string path = FindPath(root, relative);
        if (path.Length == 0) return;
        Ps5SystemFileValidation validation = Ps5SystemFileValidator.Validate(path);
        if (!validation.IsValid) throw new InvalidDataException(relative + " is invalid: " + validation.Message);
        AddSource(payloads, root, id, relative);
    }

    private static void ValidateSourceIdentity(string source, string contentId)
    {
        string paramPath = FindPath(source, "sce_sys/param.json");
        if (paramPath.Length == 0) throw new InvalidDataException("The dump does not contain sce_sys/param.json.");
        if (new FileInfo(paramPath).Length > 16 * 1024 * 1024)
            throw new InvalidDataException("sce_sys/param.json exceeds the 16 MiB safety limit.");
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(paramPath),
            new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        JsonElement property = document.RootElement.EnumerateObject()
            .FirstOrDefault(item => item.Name.Equals("contentId", StringComparison.OrdinalIgnoreCase)).Value;
        if (property.ValueKind == JsonValueKind.String && property.GetString() is string declared &&
            !declared.Equals(contentId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"The selected content ID does not match sce_sys/param.json ({declared}).");
    }

    private static byte[] BuildBlockDigests(string path, long offset, long count)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            1024 * 1024, FileOptions.SequentialScan);
        stream.Position = offset;
        using var output = new MemoryStream(checked((int)(DivideRoundUp(count, BlockSize) * 32)));
        byte[] buffer = new byte[BlockSize];
        while (count > 0)
        {
            int wanted = checked((int)Math.Min(count, buffer.Length));
            Array.Clear(buffer);
            stream.ReadExactly(buffer.AsSpan(0, wanted));
            byte[] digest = SHA3_256.HashData(buffer);
            Array.Reverse(digest);
            output.Write(digest);
            count -= wanted;
        }
        return output.ToArray();
    }

    private static byte[] HashFileSha3(string path)
    {
        using var input = File.OpenRead(path);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA3_256);
        byte[] buffer = new byte[1024 * 1024];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0) hash.AppendData(buffer, 0, read);
        return hash.GetHashAndReset();
    }

    private static bool ValidateFihDigests(string path, SonyPkgSummary package, SonyPfsSummary outer,
        SonyPfsEntry image)
    {
        byte[] fih = ReadRange(path, 0, FihSize);
        if (BinaryPrimitives.ReadUInt64LittleEndian(fih.AsSpan(0xA0)) !=
            checked((ulong)(new FileInfo(path).Length - (long)package.EmbeddedCntOffset))) return false;
        byte[] superblock = ReadRange(path, checked((long)package.PfsSuperblockOffset), BlockSize);
        byte[] superblockDigest = SHA3_256.HashData(superblock);
        if (!fih.AsSpan(0x30, 32).SequenceEqual(superblockDigest) ||
            !fih.AsSpan(0x70, 32).SequenceEqual(superblockDigest) ||
            !fih.AsSpan(0xD0, 32).SequenceEqual(superblockDigest)) return false;
        using var innerHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA3_256);
        long offset = 0;
        while (offset < image.Size)
        {
            int count = checked((int)Math.Min(1024 * 1024, image.Size - offset));
            innerHash.AppendData(SonyPfsEntryDataReader.ReadRange(path, package, outer, image, offset, count));
            offset += count;
        }
        return fih.AsSpan(0xB0, 32).SequenceEqual(innerHash.GetHashAndReset());
    }

    private static bool ValidateCntDigests(string path, SonyPkgSummary package)
    {
        if (package.ContainerOffset < 0 || package.ContainerOffset > new FileInfo(path).Length - 0x1000) return false;
        byte[] prefix = ReadRange(path, package.ContainerOffset, 0x1000);
        if (!prefix.AsSpan(0xFE0, 32).SequenceEqual(SHA3_256.HashData(prefix.AsSpan(0, 0xFE0)))) return false;
        SonyPkgEntry? tableEntry = package.Entries.FirstOrDefault(entry => entry.Id == 0x0001);
        if (tableEntry is null || tableEntry.IsEncrypted || tableEntry.DataSize != package.Entries.Count * 32) return false;
        var reader = new SonyPkgReader();
        byte[] table = reader.ReadEntryBytes(path, package, tableEntry, 4 * 1024 * 1024);
        for (int index = 0; index < package.Entries.Count; index++)
        {
            SonyPkgEntry entry = package.Entries[index];
            ReadOnlySpan<byte> expected = table.AsSpan(index * 32, 32);
            if (entry.Id == 0x0001)
            {
                if (expected.IndexOfAnyExcept((byte)0) >= 0) return false;
                continue;
            }
            if (entry.IsEncrypted || entry.DataSize > 64 * 1024 * 1024) return false;
            byte[] data = reader.ReadEntryBytes(path, package, entry, 64 * 1024 * 1024);
            if (!expected.SequenceEqual(SHA3_256.HashData(data))) return false;
        }
        return true;
    }

    private static byte[] ReadRange(string path, long offset, int count)
    {
        byte[] data = new byte[count];
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.RandomAccess);
        stream.Position = offset;
        stream.ReadExactly(data);
        return data;
    }

    private static string FindPath(string root, string relative)
    {
        string direct = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(direct)) return direct;
        string name = Path.GetFileName(relative);
        return Directory.EnumerateFiles(root, name, SearchOption.AllDirectories).FirstOrDefault(path =>
            path.Replace('\\', '/').EndsWith(relative, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    private static void WriteCString(Stream output, string value) { output.Write(Encoding.UTF8.GetBytes(value)); output.WriteByte(0); }
    private static void WriteU16Be(byte[] target, int offset, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(target.AsSpan(offset), value);
    private static void WriteU32Be(byte[] target, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(target.AsSpan(offset), value);
    private static void WriteU64Be(byte[] target, int offset, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(target.AsSpan(offset), value);
    private static int Align(int value, int alignment) => checked((value + alignment - 1) & -alignment);
    private static long AlignLong(long value, long alignment) => checked((value + alignment - 1) / alignment * alignment);
    private static long DivideRoundUp(long value, long divisor) => checked((value + divisor - 1) / divisor);
    private static async Task WriteZerosAsync(Stream output, long count, CancellationToken cancellationToken)
    {
        byte[] zero = new byte[BlockSize];
        while (count > 0) { int take = checked((int)Math.Min(count, zero.Length)); await output.WriteAsync(zero.AsMemory(0, take), cancellationToken).ConfigureAwait(false); count -= take; }
    }
    private static void DeleteTemporary(string path) { try { if (File.Exists(path)) File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { } }

    private sealed class CntPayload(uint id, string name, byte[] data)
    {
        public uint Id { get; } = id;
        public string Name { get; } = name;
        public byte[] Data { get; set; } = data;
        public int Offset { get; set; }
    }
    private sealed class SupplementalDirectory(string name)
    {
        public string Name { get; } = name;
        public Dictionary<string, SupplementalDirectory> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<(string Name, SonyDataFirstPfsFile File)> Files { get; } = [];
    }
    private readonly record struct OuterPfsResult(long SuperblockRelativeOffset, long ImageSize,
        byte[] SuperblockDigest);
}
