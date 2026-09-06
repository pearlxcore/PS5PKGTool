using System.Buffers.Binary;
using System.Text;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;

internal static class SonyPkgReaderSmoke
{
    private const string ContentId = "UP0000-PPSA12345_00-TESTPACKAGE00000";

    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PS5PKGTool-SonyPkg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            byte[] cnt = BuildCnt();
            string cntPath = Path.Combine(directory, "test-meta.pkg");
            File.WriteAllBytes(cntPath, cnt);
            var reader = new SonyPkgReader();
            SonyPkgSummary meta = reader.Read(cntPath);
            Require(meta.Kind == SonyPkgKind.MetadataContainer, "CNT type detection failed.");
            Require(meta.ContentId == ContentId, "CNT content ID was not decoded.");
            Require(meta.Entries.Count == 3, "CNT entry count is incorrect.");
            SonyPkgEntry param = meta.Entries.Single(entry => entry.Id == 0x2000);
            Require(param.DisplayName == "sce_sys/param.json", "CNT name-table resolution failed.");
            string paramJson = Encoding.UTF8.GetString(reader.ReadEntryBytes(cntPath, meta, param, 1024 * 1024));
            Require(paramJson.Contains("Synthetic PS5 Package", StringComparison.Ordinal), "Bounded CNT entry read failed.");

            Ps5GameInfo game = new SonyPkgGameReader().Read(cntPath);
            Require(game.SourceKind == Ps5SourceKind.SonyPackage, "Package source kind was not set.");
            Require(game.Title == "Synthetic PS5 Package", "Package param.json was not integrated.");
            Require(game.TitleId == "PPSA12345", "Package title ID was not integrated.");

            string fihPath = Path.Combine(directory, "test-final.pkg");
            File.WriteAllBytes(fihPath, BuildFih(cnt));
            SonyPkgSummary finalized = reader.Read(fihPath);
            Require(finalized.Kind == SonyPkgKind.FinalizedDebug, "FIH debug type detection failed.");
            Require(finalized.FormatVersion == 3 && finalized.EmbeddedCntOffset == 0x20000,
                "FIH header fields were not decoded.");
            Require(finalized.Entries.Count == 3, "Embedded CNT was not decoded.");

            string nestedPath = Path.Combine(directory, "test-nested.pkg");
            File.WriteAllBytes(nestedPath, BuildFihWithPlaintextPfs(cnt));
            Ps5GameInfo nestedGame = new SonyPkgGameReader().Read(nestedPath);
            Require(nestedGame.Package?.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed,
                "Plaintext nested PFS was not indexed.");
            Require(nestedGame.Package?.NestedPfs?.Files.Single().RelativePath == "hello.txt",
                "Nested PFS path reconstruction failed.");
            using (IReadOnlyGameFileSystem files = GameFileSystem.Open(nestedGame))
            using (Stream content = files.OpenRead("hello.txt"))
            using (var text = new StreamReader(content, Encoding.UTF8))
                Require(text.ReadToEnd() == "hello from pfs", "Nested PFS direct read failed.");

            byte[] corrupt = (byte[])cnt.Clone();
            BinaryPrimitives.WriteUInt32BigEndian(corrupt.AsSpan(0x5A0 + 0x14, 4), uint.MaxValue);
            string corruptPath = Path.Combine(directory, "corrupt.pkg");
            File.WriteAllBytes(corruptPath, corrupt);
            RequireThrows<InvalidDataException>(() => reader.Read(corruptPath),
                "An out-of-bounds CNT entry was accepted.");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static byte[] BuildCnt()
    {
        const int entryTableOffset = 0x5A0;
        const int nameTableOffset = 0x700;
        const int paramOffset = 0x800;
        const int iconOffset = 0x1000;
        byte[] names = Encoding.UTF8.GetBytes("\0entry_names.bin\0sce_sys/param.json\0sce_sys/icon0.png\0");
        int nameEntryOffset = 1;
        int paramNameOffset = nameEntryOffset + Encoding.UTF8.GetByteCount("entry_names.bin") + 1;
        int iconNameOffset = paramNameOffset + Encoding.UTF8.GetByteCount("sce_sys/param.json") + 1;
        byte[] param = Encoding.UTF8.GetBytes("""
            {
              "contentId": "UP0000-PPSA12345_00-TESTPACKAGE00000",
              "titleId": "PPSA12345",
              "contentVersion": "01.000.000",
              "localizedParameters": {
                "defaultLanguage": "en-US",
                "en-US": { "titleName": "Synthetic PS5 Package" }
              }
            }
            """);
        byte[] image = new byte[iconOffset + 8];
        image[0] = 0x7F;
        image[1] = (byte)'C';
        image[2] = (byte)'N';
        image[3] = (byte)'T';
        WriteU32Be(image, 0x10, 3);
        WriteU16Be(image, 0x14, 3);
        WriteU32Be(image, 0x18, entryTableOffset);
        WriteU64Be(image, 0x20, nameTableOffset);
        WriteU64Be(image, 0x28, (ulong)(image.Length - nameTableOffset));
        Encoding.ASCII.GetBytes(ContentId).CopyTo(image, 0x40);
        WriteU32Be(image, 0x74, 0x20);
        WriteU32Be(image, 0x78, 0x3);

        WriteEntry(image, entryTableOffset + 0x00, 0x0200, (uint)nameEntryOffset, nameTableOffset, names.Length);
        WriteEntry(image, entryTableOffset + 0x20, 0x2000, (uint)paramNameOffset, paramOffset, param.Length);
        WriteEntry(image, entryTableOffset + 0x40, 0x1200, (uint)iconNameOffset, iconOffset, 8);
        names.CopyTo(image, nameTableOffset);
        param.CopyTo(image, paramOffset);
        new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(image, iconOffset);
        return image;
    }

    private static byte[] BuildFih(byte[] cnt)
    {
        const int cntOffset = 0x20000;
        byte[] image = new byte[cntOffset + cnt.Length];
        image[0] = 0x7F;
        image[1] = (byte)'F';
        image[2] = (byte)'I';
        image[3] = (byte)'H';
        image[0x05] = 0x00;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(0x06, 2), 3);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x10, 8), 0x10000);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x18, 8), 0x10000);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x58, 8), cntOffset);
        cnt.CopyTo(image, cntOffset);
        return image;
    }

    private static byte[] BuildFihWithPlaintextPfs(byte[] cnt)
    {
        const int blockSize = 0x10000;
        const int pfsOffset = blockSize;
        const int pfsBlocks = 5;
        const int cntOffset = pfsOffset + pfsBlocks * blockSize;
        byte[] image = new byte[cntOffset + cnt.Length];
        image[0] = 0x7F;
        image[1] = (byte)'F';
        image[2] = (byte)'I';
        image[3] = (byte)'H';
        image[0x05] = 0x00;
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(0x06, 2), 3);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x10, 8), pfsOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x18, 8), pfsBlocks * blockSize);
        BinaryPrimitives.WriteUInt64LittleEndian(image.AsSpan(0x58, 8), cntOffset);

        Span<byte> pfs = image.AsSpan(pfsOffset, pfsBlocks * blockSize);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x00..], 2);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x08..], 20130315);
        BinaryPrimitives.WriteUInt16LittleEndian(pfs[0x1C..], 0x0008);
        BinaryPrimitives.WriteUInt32LittleEndian(pfs[0x20..], blockSize);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x28..], 1);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x30..], 3);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x38..], pfsBlocks);
        BinaryPrimitives.WriteInt64LittleEndian(pfs[0x40..], 1);

        WriteUnsignedInode(pfs.Slice(blockSize + 0x000, 0xA8), 0x4000, 24, 2);
        WriteUnsignedInode(pfs.Slice(blockSize + 0x0A8, 0xA8), 0x4000, 32, 3);
        WriteUnsignedInode(pfs.Slice(blockSize + 0x150, 0xA8), 0x8000, 14, 4);
        WriteDirent(pfs.Slice(blockSize * 2), 1, 3, "uroot");
        WriteDirent(pfs.Slice(blockSize * 3), 2, 2, "hello.txt");
        Encoding.UTF8.GetBytes("hello from pfs").CopyTo(pfs.Slice(blockSize * 4));
        cnt.CopyTo(image, cntOffset);
        return image;
    }

    private static void WriteUnsignedInode(Span<byte> inode, ushort mode, long size, int block)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(inode[0x00..], mode);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[0x02..], 1);
        BinaryPrimitives.WriteInt64LittleEndian(inode[0x08..], size);
        BinaryPrimitives.WriteInt64LittleEndian(inode[0x10..], size);
        BinaryPrimitives.WriteUInt32LittleEndian(inode[0x60..], 1);
        BinaryPrimitives.WriteInt32LittleEndian(inode[0x64..], block);
        for (int index = 1; index < 12; index++) BinaryPrimitives.WriteInt32LittleEndian(inode[(0x64 + index * 4)..], -1);
        for (int index = 0; index < 5; index++) BinaryPrimitives.WriteInt32LittleEndian(inode[(0x94 + index * 4)..], -1);
    }

    private static void WriteDirent(Span<byte> output, uint inode, int type, string name)
    {
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        int entrySize = (nameBytes.Length + 17 + 7) & ~7;
        BinaryPrimitives.WriteUInt32LittleEndian(output[0x00..], inode);
        BinaryPrimitives.WriteInt32LittleEndian(output[0x04..], type);
        BinaryPrimitives.WriteInt32LittleEndian(output[0x08..], nameBytes.Length);
        BinaryPrimitives.WriteInt32LittleEndian(output[0x0C..], entrySize);
        nameBytes.CopyTo(output[0x10..]);
    }

    private static void WriteEntry(byte[] image, int offset, uint id, uint nameOffset, int dataOffset, int dataSize)
    {
        WriteU32Be(image, offset, id);
        WriteU32Be(image, offset + 4, nameOffset);
        WriteU32Be(image, offset + 0x10, checked((uint)dataOffset));
        WriteU32Be(image, offset + 0x14, checked((uint)dataSize));
    }

    private static void WriteU16Be(byte[] target, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16BigEndian(target.AsSpan(offset, 2), value);

    private static void WriteU32Be(byte[] target, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(target.AsSpan(offset, 4), value);

    private static void WriteU64Be(byte[] target, int offset, ulong value) =>
        BinaryPrimitives.WriteUInt64BigEndian(target.AsSpan(offset, 8), value);

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
