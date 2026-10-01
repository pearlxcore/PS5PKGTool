namespace PS5PKGTool.Infrastructure;

/// <summary>
/// Redirects the process temp folder. Path.GetTempPath (and the bundled builders, which use it for
/// extract/build/convert workspaces) reads TMP/TEMP, so pointing those at the chosen folder moves every
/// temporary workspace off the system drive without touching each call site.
/// </summary>
public static class TempFolder
{
    private static readonly string? OriginalTmp;
    private static readonly string? OriginalTemp;

    // An explicit static constructor captures the originals before the first Apply overwrites them;
    // plain field initializers may run lazily, after TMP/TEMP were already changed.
    static TempFolder()
    {
        OriginalTmp = Environment.GetEnvironmentVariable("TMP");
        OriginalTemp = Environment.GetEnvironmentVariable("TEMP");
    }

    /// <summary>Uses <paramref name="folder"/>, or the system default when blank. Returns an error when the folder is unusable.</summary>
    public static string? Apply(string? folder)
    {
        string? error = Validate(folder);
        bool custom = !string.IsNullOrWhiteSpace(folder) && error is null;
        Environment.SetEnvironmentVariable("TMP", custom ? folder : OriginalTmp);
        Environment.SetEnvironmentVariable("TEMP", custom ? folder : OriginalTemp);
        if (error is not null) Logger.Warn($"Temp folder \"{folder}\" is unusable, using the system temp folder. ({error})");
        return error;
    }

    /// <summary>Creates the folder if needed; returns null when it is blank or usable.</summary>
    public static string? Validate(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return null;
        try
        {
            Directory.CreateDirectory(folder);
            return null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return ex.Message;
        }
    }
}
