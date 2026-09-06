using System.Text.Json;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Infrastructure;

public sealed class AppSettings
{
    public List<string> LibraryFolders { get; set; } = [];
    public bool RecursiveScan { get; set; } = true;
}

public sealed class LibraryManifest
{
    public DateTime CreatedUtc { get; set; }
    public List<Ps5GameInfo> Games { get; set; } = [];
}

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

    public AppSettings LoadSettings() => Load<AppSettings>(SettingsPath) ?? new AppSettings();
    public LibraryManifest LoadManifest() => Load<LibraryManifest>(ManifestPath) ?? new LibraryManifest();
    public void SaveSettings(AppSettings settings) => Save(SettingsPath, settings);
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
