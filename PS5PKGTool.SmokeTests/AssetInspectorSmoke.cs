using System.Buffers.Binary;
using System.Text;
using PS5PKGTool.Core.Assets;

internal static class AssetInspectorSmoke
{
    public static void Run()
    {
        byte[] png = new byte[32];
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(png, 0);
        "IHDR"u8.CopyTo(png.AsSpan(12));
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(16), 512);
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(20), 256);
        AssetInspection pngInfo = AssetInspector.Inspect("icon0.png", png);
        Require(pngInfo.Category == AssetCategory.Image && pngInfo.Format == "PNG",
            "PNG detection failed.");
        Require(pngInfo.Metadata.Any(entry => entry is { Name: "Width", Value: "512" }) &&
                pngInfo.Metadata.Any(entry => entry is { Name: "Height", Value: "256" }),
            "PNG dimensions were not read.");

        byte[] dds = new byte[96];
        "DDS "u8.CopyTo(dds);
        BinaryPrimitives.WriteUInt32LittleEndian(dds.AsSpan(12), 2160);
        BinaryPrimitives.WriteUInt32LittleEndian(dds.AsSpan(16), 3840);
        "DX10"u8.CopyTo(dds.AsSpan(84));
        AssetInspection ddsInfo = AssetInspector.Inspect("pic0.dds", dds);
        Require(ddsInfo.Category == AssetCategory.Image && ddsInfo.Format == "DDS",
            "DDS detection failed.");
        Require(ddsInfo.Metadata.Any(entry => entry is { Name: "Width", Value: "3840" }) &&
                ddsInfo.Metadata.Any(entry => entry is { Name: "Height", Value: "2160" }),
            "DDS dimensions were not read.");

        byte[] wav = new byte[48];
        "RIFF"u8.CopyTo(wav);
        "WAVE"u8.CopyTo(wav.AsSpan(8));
        "fmt "u8.CopyTo(wav.AsSpan(12));
        BinaryPrimitives.WriteUInt16LittleEndian(wav.AsSpan(22), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(wav.AsSpan(24), 48000);
        BinaryPrimitives.WriteUInt16LittleEndian(wav.AsSpan(34), 16);
        AssetInspection wavInfo = AssetInspector.Inspect("bgm.wav", wav);
        Require(wavInfo.Category == AssetCategory.Audio && wavInfo.Format == "WAV",
            "WAV detection failed.");
        Require(wavInfo.Metadata.Any(entry => entry is { Name: "Sample rate", Value: "48000" }),
            "WAV sample rate was not read.");

        byte[] json = Encoding.UTF8.GetBytes("{\"titleId\":\"PPSA00001\"}");
        AssetInspection jsonInfo = AssetInspector.Inspect("param.json", json);
        Require(jsonInfo.Category == AssetCategory.Text && jsonInfo.Format == "JSON",
            "JSON text detection failed.");

        AssetInspection at9 = AssetInspector.Inspect("snd0.at9", new byte[16]);
        Require(at9.Category == AssetCategory.Audio && at9.Format == "AT9",
            "AT9 extension detection failed.");

        byte[] binary = [0x00, 0x01, 0x02, 0x00, 0xFF, 0x00, 0x42, 0x00];
        AssetInspection binaryInfo = AssetInspector.Inspect("eboot.bin", binary);
        Require(binaryInfo.Category == AssetCategory.Unknown,
            "Binary data was misclassified.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
