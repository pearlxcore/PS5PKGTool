namespace PS5PKGTool.Core.Models;

public enum Ps5SourceKind
{
    LooseDump,
    SonyPackage,
    Ffpfsc,
    FilesystemImage,
    Ffpkg
}

public sealed class Ps5GameInfo
{
    public Ps5SourceKind SourceKind { get; set; }
    public string RootPath { get; set; } = string.Empty;
    public string ParamPath { get; set; } = string.Empty;
    public string VirtualRoot { get; set; } = string.Empty;
    public long SourceSize { get; set; }
    public string ContainerInnerFileName { get; set; } = string.Empty;
    public long ContainerLogicalSize { get; set; }
    public long ContainerStoredSize { get; set; }
    public int ContainerBlockCount { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleId { get; set; } = string.Empty;
    public string ContentId { get; set; } = string.Empty;
    public string ConceptId { get; set; } = string.Empty;
    public string ContentVersion { get; set; } = string.Empty;
    public string MasterVersion { get; set; } = string.Empty;
    public string TargetContentVersion { get; set; } = string.Empty;
    public string OriginContentVersion { get; set; } = string.Empty;
    public string RequiredSystemSoftware { get; set; } = string.Empty;
    public string SdkVersion { get; set; } = string.Empty;
    public string ApplicationCategory { get; set; } = string.Empty;
    public string DrmType { get; set; } = string.Empty;
    public string ContentBadgeType { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public Dictionary<string, string> LocalizedTitles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> AgeLevels { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> DeclaredFeatures { get; set; } = [];
    public List<string> GameIntents { get; set; } = [];
    public List<string> SharedAddOnServiceIds { get; set; } = [];
    public uint Attribute { get; set; }
    public uint Attribute2 { get; set; }
    public uint Attribute3 { get; set; }
    public uint Attribute4 { get; set; }
    public long DownloadDataSize { get; set; }
    public long? FlexibleMemorySize { get; set; }
    public string CreationDate { get; set; } = string.Empty;
    public string ToolVersion { get; set; } = string.Empty;
    public string VersionFileUri { get; set; } = string.Empty;
    public string RawParamJson { get; set; } = string.Empty;
    public List<string> DataWarnings { get; set; } = [];
    public DateTime LastWriteTimeUtc { get; set; }
    public SonyPkgSummary? Package { get; set; }

    public string DisplayVersion => string.IsNullOrWhiteSpace(ContentVersion) ? MasterVersion : ContentVersion;
    public string SourceDescription => SourceKind switch
    {
        Ps5SourceKind.SonyPackage => Package?.KindDisplayName ?? "Sony PKG",
        Ps5SourceKind.Ffpfsc => "FFPFSC image",
        Ps5SourceKind.FilesystemImage => "exFAT filesystem image",
        Ps5SourceKind.Ffpkg => "FFPKG (UFS2) image",
        _ => "Unpacked dump"
    };
}
