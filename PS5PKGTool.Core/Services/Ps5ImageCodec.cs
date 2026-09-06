using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PS5PKGTool.Core.Services;

public static class Ps5ImageCodec
{
    public static byte[] DecodeDdsToPng(Stream ddsStream)
    {
        ArgumentNullException.ThrowIfNull(ddsStream);
        if (!ddsStream.CanRead) throw new ArgumentException("The DDS stream must be readable.", nameof(ddsStream));
        var decoder = new BcDecoder();
        using Image<Rgba32> image = decoder.DecodeToImageRgba32(ddsStream);
        using var output = new MemoryStream();
        image.SaveAsPng(output);
        return output.ToArray();
    }
}
