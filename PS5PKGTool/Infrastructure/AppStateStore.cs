using System.Text.Json;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Infrastructure;

/// <summary>A named snapshot of the library layout: filters, grouping, sorting and columns.</summary>
public sealed class SavedLibraryView
{
    public string Name { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = [];
    public List<string> Regions { get; set; } = [];
    public List<string> Formats { get; set; } = [];
    public string GroupBy { get; set; } = string.Empty;
    public List<string> SortKeys { get; set; } = [];
    public List<string> HiddenColumns { get; set; } = [];
    public List<string> ColumnOrder { get; set; } = [];
}

public sealed class AppSettings
{
    public List<string> LibraryFolders { get; set; } = [];
    public bool RecursiveScan { get; set; } = true;
    public List<string> RecentFolders { get; set; } = [];
    public List<string> ManualSources { get; set; } = [];
    public int WindowWidth { get; set; }
    public int WindowHeight { get; set; }
    public bool WindowMaximized { get; set; }
    public string LibrarySortColumn { get; set; } = "Title";
    public bool LibrarySortAscending { get; set; } = true;
    /// <summary>Ordered sort keys ("Title:asc", "Size:desc") for multi-column sorting.</summary>
    public List<string> LibrarySortKeys { get; set; } = [];
    /// <summary>Named saved library views (filters, grouping, sorting and column layout).</summary>
    public List<SavedLibraryView> SavedViews { get; set; } = [];
    public List<string> LibraryColumnOrder { get; set; } = [];
    public List<string> LibraryHiddenColumns { get; set; } = [];
    /// <summary>Remembered column fill weights by column name so user-resized widths survive a restart.</summary>
    public Dictionary<string, float> LibraryColumnWeights { get; set; } = [];

    // Appearance
    public string Theme { get; set; } = "Default (Charcoal)";
    public int GridRowHeight { get; set; } = 22;
    public bool ShowThumbnails { get; set; } = true;
    public bool ShowGridLines { get; set; } = true;
    /// <summary>Show the inline file preview pane in the Files tab (list-only when disabled).</summary>
    public bool ShowFilePreview { get; set; } = true;
    public string DefaultGroupBy { get; set; } = string.Empty;

    // Library & scanning
    public bool RefreshOnStartup { get; set; }
    public string RenameFormat { get; set; } = "{TITLE} [{TITLE_ID}]";
    public bool LogAutoScroll { get; set; } = true;

    // Files & preview
    public int MaxPreviewMb { get; set; } = 16;
    public int HexPageKb { get; set; } = 16;

    // Performance
    public int ThumbnailCacheCount { get; set; } = 512;

    // Paths & outputs
    public string OutputDirectory { get; set; } = string.Empty;
    public bool OpenOutputAfterTask { get; set; }

    // Tasks workspace
    /// <summary>Remembered vertical split between the task list and the details panel.</summary>
    public int TaskSplitterDistance { get; set; }
    /// <summary>True when the details panel is collapsed to give the list more height.</summary>
    public bool TaskDetailsCollapsed { get; set; }

    // Build defaults
    public string DebugPasscode { get; set; } = string.Empty;

    /// <summary>Selected package backend id ("lpp" or "ppt"). Empty/unknown falls back to the default.</summary>
    public string BuildBackend { get; set; } = string.Empty;

    // Safety
    public bool ConfirmDelete { get; set; } = true;
    public bool ConfirmMove { get; set; } = true;
    public bool PermanentDelete { get; set; }
}

public sealed class LibraryManifest
{
    public DateTime CreatedUtc { get; set; }
    public List<Ps5GameInfo> Games { get; set; } = [];
}

/// <summary>Result of loading settings, including a non-fatal warning worth surfacing to the user.</summary>
public sealed record SettingsLoadResult(AppSettings Settings, string? Warning);

public sealed class AppStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppStateStore()
    {
        AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PS5PKGTool");
        Directory.CreateDirectory(AppDataDirectory);
    }

    public string AppDataDirectory { get; }
    public string SettingsPath => Path.Combine(AppDataDirectory, "settings.json");
    public string ManifestPath => Path.Combine(AppDataDirectory, "manifest.json");

    public AppSettings LoadSettings() => LoadSettingsWithDiagnostics().Settings;

    /// <summary>
    /// Loads and normalizes settings. A missing file simply yields defaults; an unreadable file is
    /// quarantined (renamed) rather than silently overwritten by a later automatic save, and a warning
    /// describing what happened is returned.
    /// </summary>
    public SettingsLoadResult LoadSettingsWithDiagnostics()
    {
        if (!File.Exists(SettingsPath)) return new SettingsLoadResult(new AppSettings(), null);
        try
        {
            AppSettings settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), JsonOptions)
                ?? new AppSettings();
            return new SettingsLoadResult(AppSettingsNormalizer.Normalize(settings), null);
        }
        catch (JsonException ex)
        {
            string quarantine = SettingsPath + ".invalid-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            try { File.Move(SettingsPath, quarantine, overwrite: true); }
            catch (Exception moveEx) when (moveEx is IOException or UnauthorizedAccessException) { }
            return new SettingsLoadResult(new AppSettings(),
                $"The settings file could not be read and was moved to {Path.GetFileName(quarantine)}. " +
                $"Defaults are in use. ({ex.Message})");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new SettingsLoadResult(new AppSettings(),
                $"Settings could not be read: {ex.Message}. Defaults are in use for this session.");
        }
    }

    public LibraryManifest LoadManifest()
    {
        LibraryManifest manifest = Load<LibraryManifest>(ManifestPath) ?? new LibraryManifest();
        manifest.Games ??= [];
        return manifest;
    }

    public void SaveSettings(AppSettings settings) => Save(SettingsPath, AppSettingsNormalizer.Normalize(settings));
    public void SaveManifest(IReadOnlyCollection<Ps5GameInfo> games) => Save(ManifestPath, new LibraryManifest
    {
        CreatedUtc = DateTime.UtcNow,
        Games = games.ToList()
    });

    private static T? Load<T>(string path)
    {
        try
        {
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions) : default;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return default;
        }
    }

    private static void Save<T>(string path, T value)
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(value, JsonOptions));
        File.Move(tempPath, path, true);
    }
}
