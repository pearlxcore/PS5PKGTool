namespace PS5PKGTool.Core.Models;

public sealed class Ps5GameDetails
{
    public Ps5TrophySet? TrophySet { get; init; }
    public Ps5UdsSummary? Uds { get; init; }
    public Ps5SelfInfo? Executable { get; init; }
    public Ps5FileInventory Files { get; init; } = new();
    public byte[]? IconPng { get; init; }
    public byte[]? BackgroundPng { get; init; }
    public byte[]? Background1Png { get; init; }
    public byte[]? Background2Png { get; init; }
    public List<string> Errors { get; init; } = [];
}

public sealed class Ps5TrophySet
{
    public string NpCommunicationId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string TrophySetVersion { get; init; } = string.Empty;
    public string SelectedLanguage { get; init; } = string.Empty;
    public IReadOnlyList<string> Languages { get; init; } = [];
    public IReadOnlyList<Ps5Trophy> Trophies { get; init; } = [];
    public bool IntegrityValid { get; init; }
}

public sealed class Ps5Trophy
{
    public int Id { get; init; }
    public string Grade { get; init; } = string.Empty;
    public bool Hidden { get; init; }
    public bool HasReward { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PlatinumTrophyId { get; init; } = string.Empty;
    public string UnlockCondition { get; init; } = string.Empty;
    public byte[]? IconPng { get; init; }
}

public sealed class Ps5UdsSummary
{
    public string NpCommunicationId { get; init; } = string.Empty;
    public int EnumGroupCount { get; init; }
    public int EventCount { get; init; }
    public int StatCount { get; init; }
    public int ExtractionRuleCount { get; init; }
    public IReadOnlyList<Ps5UdsEvent> Events { get; init; } = [];
    public bool IntegrityValid { get; init; }
}

public sealed class Ps5UdsEvent
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string DefinitionGroup { get; init; } = string.Empty;
    public int PropertyCount { get; init; }
}

public sealed class Ps5SelfInfo
{
    public string SelfMagic { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public long ElfOffset { get; init; }
    public byte ElfClass { get; init; }
    public byte Endianness { get; init; }
    public ushort ElfType { get; init; }
    public ushort Machine { get; init; }
    public ulong EntryPoint { get; init; }
    public ushort ProgramHeaderCount { get; init; }
    public ushort SectionHeaderCount { get; init; }
    public IReadOnlyList<Ps5ModuleInfo> Modules { get; init; } = [];
}

public sealed class Ps5ModuleInfo
{
    public string Name { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Kind { get; init; } = string.Empty;
}

public sealed class Ps5FileInventory
{
    public long TotalSize { get; init; }
    public int FileCount { get; init; }
    public IReadOnlyList<Ps5FileInfo> Files { get; init; } = [];
    public IReadOnlyList<Ps5FileInfo> LargestFiles { get; init; } = [];
}

public sealed class Ps5FileInfo
{
    public string RelativePath { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public long Size { get; init; }
    public long Offset { get; init; }
    public uint? PackageEntryId { get; init; }
    public bool IsEncrypted { get; init; }
}
