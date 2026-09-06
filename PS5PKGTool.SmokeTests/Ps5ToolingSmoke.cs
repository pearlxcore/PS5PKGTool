using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;
using System.Buffers.Binary;

internal static class Ps5ToolingSmoke
{
    public static void Run()
    {
        string root = Path.Combine(Path.GetTempPath(), "PS5PKGTool-Tools-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "sce_sys"));
        try
        {
            Require(Ps5Crc32C.Compute("123456789"u8) == 0xE3069283, "CRC-32C test vector failed.");
            Ps5ParamDocument param = Ps5ParamDocument.Create("UP0000-PPSA12345_00-TESTPACKAGE00000", "PPSA12345", "Tooling Smoke");
            param.SetLocalizedTitle("ja-JP", "ツール");
            string paramPath = Path.Combine(root, "sce_sys", "param.json"); param.SaveAtomic(paramPath);
            Require(Ps5ParamDocument.Load(paramPath).TitleId == "PPSA12345", "param.json document round trip failed.");
            Ps5ManifestDocument.Create("UP0000-PPSA12345_00-TESTPACKAGE00000", "PPSA12345")
                .SaveAtomic(Path.Combine(root, "sce_sys", "manifest.json"));

            string ucp = Path.Combine(root, "trophy.ucp");
            UcpWriter.WriteAtomic(ucp, [new UcpWriteEntry("tropconf.json", "{}"u8.ToArray()), new UcpWriteEntry("trop001.png", [1, 2, 3])]);
            UcpArchive archive = new UcpReader().Read(ucp);
            Require(archive.Entries.Count == 2 && new UcpReader().ValidateIntegrity(archive), "UCP write or digest validation failed.");
            using (var stream = new FileStream(ucp, FileMode.Open, FileAccess.ReadWrite)) { stream.Position = 0x1C; stream.WriteByte(0); }
            UcpWriter.RepairDigestAtomic(ucp);
            Require(new UcpReader().ValidateIntegrity(new UcpReader().Read(ucp)), "UCP digest repair failed.");

            byte[] npbind = new byte[64]; npbind[0] = 0x18; npbind[1] = 0xA0; npbind[2] = 0x94; npbind[3] = 0xD2;
            "NPWR12345_00"u8.CopyTo(npbind.AsSpan(16)); string bind = Path.Combine(root, "sce_sys", "npbind.dat"); File.WriteAllBytes(bind, npbind);
            Require(Ps5SystemFileValidator.Validate(bind).IsValid, "npbind validation failed.");

            byte[] elf = new byte[64]; elf[0] = 0x7F; elf[1] = 0x45; elf[2] = 0x4C; elf[3] = 0x46; elf[4] = 2; elf[5] = 1;
            BinaryPrimitives.WriteUInt16LittleEndian(elf.AsSpan(16), 2); BinaryPrimitives.WriteUInt16LittleEndian(elf.AsSpan(18), 0x3E);
            string elfPath = Path.Combine(root, "eboot.bin"); File.WriteAllBytes(elfPath, elf);
            Ps5ElfHeaderEditor.EditAtomic(elfPath, osAbi: 9, type: 3);
            Require(Ps5ElfHeaderEditor.Read(elfPath).OsAbi == 9 && Ps5ElfHeaderEditor.Read(elfPath).Type == 3, "ELF header editing failed.");
            Require(!Ps5LaunchReadiness.Inspect(root).IsReady, "Plain ELF unexpectedly passed launch readiness.");
        }
        finally { Directory.Delete(root, true); }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
