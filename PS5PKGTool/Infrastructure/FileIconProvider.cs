using System.Drawing.Drawing2D;
using System.Reflection;

namespace PS5PKGTool.Infrastructure;

/// <summary>
/// Loads the packaged file-browser icon set (folders and file kinds) and maps package entries to
/// an icon index. The icon assets are embedded resources under <c>Resources\*.png</c> and are added
/// to the image list in the same fixed order as the PS4-style browser.
/// </summary>
public static class FileIconProvider
{
    public const int Folder = 0;
    public const int Document = 1;
    public const int Image = 2;
    public const int Config = 3;
    public const int Binary = 4;
    public const int FolderOpen = 5;
    public const int Audio = 6;
    public const int FileUnknown = 7;
    public const int Package = 8;
    public const int Video = 9;
    public const int Code = 10;
    public const int Container = 11;

    private const int IconSize = 16;

    private static readonly (string Key, string File)[] IconOrder =
    [
        ("folder", "folder.png"),
        ("document", "document.png"),
        ("image", "image.png"),
        ("config", "config.png"),
        ("binary", "binary.png"),
        ("folder-open", "folder-open.png"),
        ("audio", "audio.png"),
        ("file-unknown", "file-unknown.png"),
        ("package", "package.png"),
        ("video", "video.png"),
        ("code", "code.png"),
        ("container", "container.png"),
    ];

    /// <summary>Rebuilds <paramref name="imageList"/> from the embedded icons in a stable index order.</summary>
    public static void Populate(ImageList imageList)
    {
        ArgumentNullException.ThrowIfNull(imageList);
        imageList.Images.Clear();
        imageList.ImageSize = new Size(IconSize, IconSize);
        imageList.ColorDepth = ColorDepth.Depth32Bit;
        imageList.TransparentColor = Color.Transparent;

        Assembly assembly = typeof(FileIconProvider).Assembly;
        string[] resources = assembly.GetManifestResourceNames();
        foreach ((string key, string file) in IconOrder)
        {
            string? resource = Array.Find(resources,
                name => name.EndsWith("." + file, StringComparison.OrdinalIgnoreCase));
            if (resource is null)
            {
                imageList.Images.Add(key, new Bitmap(IconSize, IconSize));
                continue;
            }

            using Stream stream = assembly.GetManifestResourceStream(resource)!;
            using var source = new Bitmap(stream);
            imageList.Images.Add(key, Downscale(source, IconSize));
        }
    }

    /// <summary>Maps a file or directory to its icon index.</summary>
    public static int ForEntry(string name, bool isDirectory)
    {
        if (isDirectory) return Folder;
        string extension = Path.GetExtension(name).ToLowerInvariant();
        if (extension.Length == 0) return Binary;
        return extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" or ".dds" or ".tga" or ".webp" => Image,
            ".txt" or ".log" or ".md" => Document,
            ".xml" or ".json" or ".sfo" or ".ini" or ".cfg" => Config,
            ".at9" or ".ogg" or ".mp3" or ".wav" or ".aac" or ".flac" => Audio,
            ".mp4" or ".avi" or ".mkv" or ".webm" or ".mov" => Video,
            ".pkg" or ".ffpfsc" or ".ffpkg" or ".exfat" => Package,
            ".cs" or ".js" or ".lua" or ".py" or ".sh" or ".bat" => Code,
            _ => Binary
        };
    }

    private static Bitmap Downscale(Image source, int size)
    {
        var target = new Bitmap(size, size);
        target.MakeTransparent();
        using var graphics = Graphics.FromImage(target);
        graphics.CompositingMode = CompositingMode.SourceOver;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.DrawImage(source, new Rectangle(0, 0, size, size));
        return target;
    }
}
