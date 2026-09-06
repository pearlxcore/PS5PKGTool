using System.Text;
using UFS2Tool;

namespace PS5PKGTool.Ffpfsc;

public enum FfpfscInnerFilesystemKind
{
    Exfat,
    Ufs2
}

public sealed record FfpfscVolumeEntry(string Path, bool IsDirectory, long Size);

/// <summary>Opens the inner exFAT or UFS2 filesystem of an FFPFSC image without extracting it.</summary>
public sealed class FfpfscVolume : IDisposable
{
    private readonly FileStream _container;
    private readonly PfscReadStream _decoded;
    private readonly ExfatVolume? _exfat;
    private readonly Ufs2Volume? _ufs2;
    private bool _disposed;

    private FfpfscVolume(string path)
    {
        Path = System.IO.Path.GetFullPath(path);
        _container = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read,
            1024 * 1024, FileOptions.RandomAccess);
        try
        {
            Info = FfpfscImage.Inspect(_container);
            _decoded = new PfscReadStream(_container, Info.Pfsc, Info.PayloadOffset, Info.LogicalLength, leaveOpen: true);
            if (System.IO.Path.GetExtension(Info.InnerFileName).Equals(".ffpkg", StringComparison.OrdinalIgnoreCase))
            {
                _ufs2 = new Ufs2Volume(_decoded, Path + "!" + Info.InnerFileName, leaveOpen: true);
                InnerFilesystemKind = FfpfscInnerFilesystemKind.Ufs2;
                Entries = _ufs2.Entries.Select(entry =>
                    new FfpfscVolumeEntry(entry.Path, entry.IsDirectory, entry.Size)).ToArray();
            }
            else
            {
                _exfat = new ExfatVolume(_decoded, leaveOpen: true);
                InnerFilesystemKind = FfpfscInnerFilesystemKind.Exfat;
                Entries = _exfat.Entries.Select(entry =>
                    new FfpfscVolumeEntry(entry.Path, entry.IsDirectory, entry.Size)).ToArray();
            }
        }
        catch
        {
            _ufs2?.Dispose();
            _exfat?.Dispose();
            _decoded?.Dispose();
            _container.Dispose();
            throw;
        }
    }

    public string Path { get; }
    public FfpfscInfo Info { get; }
    public FfpfscInnerFilesystemKind InnerFilesystemKind { get; }
    public IReadOnlyList<FfpfscVolumeEntry> Entries { get; } = [];
    public ExfatVolume FileSystem => _exfat ?? throw new InvalidOperationException("The inner filesystem is UFS2.");

    public static FfpfscVolume Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new FfpfscVolume(path);
    }

    public FfpfscVolumeEntry? Find(string path)
    {
        string normalized = path.Replace('\\', '/').Trim('/');
        return Entries.FirstOrDefault(entry => entry.Path.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }
    public Stream OpenFile(string path) => _ufs2 is not null ? _ufs2.OpenFile(path) : _exfat!.OpenFile(path);
    public byte[] ReadAllBytes(string path, int maximumBytes = 256 * 1024 * 1024)
    {
        FfpfscVolumeEntry entry = Find(path) ?? throw new FileNotFoundException("The file was not found in FFPFSC.", path);
        if (entry.IsDirectory) throw new IOException("The requested FFPFSC entry is a directory.");
        if (entry.Size < 0 || entry.Size > maximumBytes || entry.Size > int.MaxValue)
            throw new IOException($"The FFPFSC file is too large to load into memory: {entry.Path}");
        using Stream input = OpenFile(entry.Path);
        byte[] result = new byte[checked((int)entry.Size)];
        input.ReadExactly(result);
        return result;
    }
    public string ReadAllText(string path, Encoding? encoding = null, int maximumBytes = 16 * 1024 * 1024) =>
        (encoding ?? new UTF8Encoding(false, true)).GetString(ReadAllBytes(path, maximumBytes));

    public void Dispose()
    {
        if (_disposed) return;
        _exfat?.Dispose();
        _ufs2?.Dispose();
        _decoded.Dispose();
        _container.Dispose();
        _disposed = true;
    }
}
