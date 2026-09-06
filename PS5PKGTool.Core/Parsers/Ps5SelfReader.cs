using System.Buffers.Binary;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;

namespace PS5PKGTool.Core.Parsers;

public sealed class Ps5SelfReader
{
    private static readonly byte[] ElfMagic = [0x7F, 0x45, 0x4C, 0x46];

    public Ps5SelfInfo? Read(string gameRoot, CancellationToken cancellationToken = default)
    {
        string ebootPath = Path.Combine(gameRoot, "eboot.bin");
        if (!File.Exists(ebootPath)) return null;
        using FileStream stream = new(ebootPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        IReadOnlyList<Ps5ModuleInfo> modules = ReadModules(gameRoot, cancellationToken);
        return ReadStream(stream, modules);
    }

    public Ps5SelfInfo? Read(IReadOnlyGameFileSystem files, CancellationToken cancellationToken = default)
    {
        const string path = "eboot.bin";
        if (!files.FileExists(path)) return null;
        using Stream stream = files.OpenRead(path);
        IReadOnlyList<Ps5ModuleInfo> modules = files.Files
            .Where(file => Path.GetExtension(file.RelativePath) is string extension &&
                           (extension.Equals(".prx", StringComparison.OrdinalIgnoreCase) ||
                            extension.Equals(".sprx", StringComparison.OrdinalIgnoreCase)))
            .Select(file => new Ps5ModuleInfo
            {
                Name = Path.GetFileName(file.RelativePath),
                RelativePath = file.RelativePath.Replace('/', Path.DirectorySeparatorChar),
                Size = file.Size,
                Kind = Path.GetExtension(file.RelativePath).TrimStart('.').ToUpperInvariant()
            }).OrderBy(module => module.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        cancellationToken.ThrowIfCancellationRequested();
        return ReadStream(stream, modules);
    }

    private static Ps5SelfInfo ReadStream(Stream stream, IReadOnlyList<Ps5ModuleInfo> modules)
    {
        byte[] prefix = new byte[Math.Min(4096, checked((int)Math.Min(stream.Length, int.MaxValue)))];
        stream.ReadExactly(prefix);
        int elfOffset = Find(prefix, ElfMagic);
        if (elfOffset < 0 || elfOffset + 64 > prefix.Length)
            throw new InvalidDataException("No embedded ELF header was found in eboot.bin.");
        ReadOnlySpan<byte> elf = prefix.AsSpan(elfOffset, 64);
        if (elf[5] != 1) throw new InvalidDataException("Only little-endian PS5 ELF files are currently supported.");

        return new Ps5SelfInfo
        {
            SelfMagic = Convert.ToHexString(prefix.AsSpan(0, 4)),
            FileSize = stream.Length,
            ElfOffset = elfOffset,
            ElfClass = elf[4],
            Endianness = elf[5],
            ElfType = BinaryPrimitives.ReadUInt16LittleEndian(elf.Slice(16, 2)),
            Machine = BinaryPrimitives.ReadUInt16LittleEndian(elf.Slice(18, 2)),
            EntryPoint = BinaryPrimitives.ReadUInt64LittleEndian(elf.Slice(24, 8)),
            ProgramHeaderCount = BinaryPrimitives.ReadUInt16LittleEndian(elf.Slice(56, 2)),
            SectionHeaderCount = BinaryPrimitives.ReadUInt16LittleEndian(elf.Slice(60, 2)),
            Modules = modules
        };
    }

    private static IReadOnlyList<Ps5ModuleInfo> ReadModules(string gameRoot, CancellationToken cancellationToken)
    {
        var modules = new List<Ps5ModuleInfo>();
        foreach (string file in SafeFiles(gameRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string extension = Path.GetExtension(file);
            if (!extension.Equals(".prx", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".sprx", StringComparison.OrdinalIgnoreCase)) continue;
            var info = new FileInfo(file);
            modules.Add(new Ps5ModuleInfo
            {
                Name = info.Name,
                RelativePath = Path.GetRelativePath(gameRoot, file),
                Size = info.Length,
                Kind = extension.TrimStart('.').ToUpperInvariant()
            });
        }
        return modules.OrderBy(module => module.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IEnumerable<string> SafeFiles(string root)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.ReparsePoint
        };
        return Directory.EnumerateFiles(root, "*", options);
    }

    private static int Find(ReadOnlySpan<byte> data, ReadOnlySpan<byte> pattern)
    {
        for (int index = 0; index <= data.Length - pattern.Length; index++)
            if (data.Slice(index, pattern.Length).SequenceEqual(pattern)) return index;
        return -1;
    }
}
