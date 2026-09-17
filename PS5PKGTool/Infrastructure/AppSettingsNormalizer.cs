namespace PS5PKGTool.Infrastructure;

/// <summary>
/// The single normalization/validation contract for settings loaded from disk, imported from a file or
/// edited in the dialog. It never throws and never leaves null collections or out-of-range numbers, so
/// every consumer can assume a consistent shape regardless of where the settings came from.
/// </summary>
public static class AppSettingsNormalizer
{
    public const int MinRowHeight = 16;
    public const int MaxRowHeight = 60;
    public const int MinPreviewMb = 1;
    public const int MaxPreviewMb = 1024;
    public const int MinHexPageKb = 1;
    public const int MaxHexPageKb = 256;
    public const int MinThumbnailCache = 1;
    public const int MaxThumbnailCache = 100000;

    /// <summary>Grouping keys the library understands; anything else normalizes to "None".</summary>
    public static readonly string[] GroupKeys = ["", "family", "titleid", "category", "region", "source", "firmware"];

    public const string DefaultRenameFormat = "{TITLE} [{TITLE_ID}]";
    public const string DefaultTheme = "Default (Charcoal)";

    public static AppSettings Normalize(AppSettings? settings)
    {
        settings ??= new AppSettings();

        settings.LibraryFolders = CleanPaths(settings.LibraryFolders);
        settings.RecentFolders = CleanList(settings.RecentFolders, 10);
        settings.ManualSources = CleanPaths(settings.ManualSources);

        settings.LibraryColumnOrder = CleanList(settings.LibraryColumnOrder);
        settings.LibraryHiddenColumns = CleanList(settings.LibraryHiddenColumns);
        settings.LibraryColumnWeights = CleanWeights(settings.LibraryColumnWeights);
        settings.LibrarySortKeys = CleanSortKeys(settings.LibrarySortKeys);
        settings.SavedViews = CleanViews(settings.SavedViews);
        settings.LibrarySortColumn = string.IsNullOrWhiteSpace(settings.LibrarySortColumn)
            ? "Title"
            : settings.LibrarySortColumn.Trim();

        settings.Theme = string.IsNullOrWhiteSpace(settings.Theme) ? DefaultTheme : settings.Theme.Trim();
        settings.GridRowHeight = Math.Clamp(settings.GridRowHeight, MinRowHeight, MaxRowHeight);
        settings.DefaultGroupBy = Array.IndexOf(GroupKeys, settings.DefaultGroupBy) >= 0 ? settings.DefaultGroupBy : string.Empty;

        settings.RenameFormat = string.IsNullOrWhiteSpace(settings.RenameFormat) ? DefaultRenameFormat : settings.RenameFormat;

        settings.MaxPreviewMb = Math.Clamp(settings.MaxPreviewMb, MinPreviewMb, MaxPreviewMb);
        settings.HexPageKb = Math.Clamp(settings.HexPageKb, MinHexPageKb, MaxHexPageKb);
        settings.ThumbnailCacheCount = Math.Clamp(settings.ThumbnailCacheCount, MinThumbnailCache, MaxThumbnailCache);

        settings.OutputDirectory = (settings.OutputDirectory ?? string.Empty).Trim();

        // A passcode is either blank (meaning the built-in all-zero default) or exactly 32 printable
        // ASCII characters. Anything else is discarded rather than carried into a build.
        settings.DebugPasscode ??= string.Empty;
        if (settings.DebugPasscode.Length > 0 && !IsValidPasscode(settings.DebugPasscode))
            settings.DebugPasscode = string.Empty;

        settings.BuildBackend = (settings.BuildBackend ?? string.Empty).Trim();

        settings.WindowWidth = ClampNonNegative(settings.WindowWidth);
        settings.WindowHeight = ClampNonNegative(settings.WindowHeight);
        settings.TaskSplitterDistance = ClampNonNegative(settings.TaskSplitterDistance);

        return settings;
    }

    public static bool IsValidPasscode(string value) =>
        value.Length == 32 && value.All(character => character is >= (char)0x20 and <= (char)0x7E);

    private static int ClampNonNegative(int value) => Math.Clamp(value, 0, 32767);

    private static List<string> CleanList(List<string>? values, int max = int.MaxValue)
    {
        var result = new List<string>();
        if (values is null) return result;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string? value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            if (!seen.Add(value.Trim())) continue;
            result.Add(value.Trim());
            if (result.Count >= max) break;
        }
        return result;
    }

    private static List<string> CleanPaths(List<string>? values)
    {
        var result = new List<string>();
        if (values is null) return result;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string? value in values)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            string path = NormalizePath(value);
            if (!seen.Add(path)) continue;
            result.Add(path);
        }
        return result;
    }

    /// <summary>Returns a full path when possible, otherwise the trimmed original string.</summary>
    public static string NormalizePath(string value)
    {
        string trimmed = value.Trim();
        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return trimmed;
        }
    }

    private static Dictionary<string, float> CleanWeights(Dictionary<string, float>? weights)
    {
        var result = new Dictionary<string, float>(StringComparer.Ordinal);
        if (weights is null) return result;
        foreach ((string key, float value) in weights)
            if (!string.IsNullOrWhiteSpace(key) && value > 0 && float.IsFinite(value))
                result[key] = value;
        return result;
    }

    private static List<string> CleanSortKeys(List<string>? values) => CleanList(values);

    private static List<SavedLibraryView> CleanViews(List<SavedLibraryView>? views)
    {
        var result = new List<SavedLibraryView>();
        if (views is null) return result;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SavedLibraryView? view in views)
        {
            if (view is null || string.IsNullOrWhiteSpace(view.Name)) continue;
            string name = view.Name.Trim();
            if (!seen.Add(name)) continue;
            view.Name = name;
            view.Query ??= string.Empty;
            view.GroupBy = Array.IndexOf(GroupKeys, view.GroupBy) >= 0 ? view.GroupBy : string.Empty;
            view.Categories = CleanList(view.Categories);
            view.Regions = CleanList(view.Regions);
            view.Formats = CleanList(view.Formats);
            view.SortKeys = CleanList(view.SortKeys);
            view.HiddenColumns = CleanList(view.HiddenColumns);
            view.ColumnOrder = CleanList(view.ColumnOrder);
            result.Add(view);
        }
        return result;
    }
}
