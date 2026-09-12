using Microsoft.Win32;

namespace PS5PKGTool.Infrastructure;

/// <summary>
/// Per-user (HKCU, no admin) File Explorer context menu integration for PS5 package and image
/// files. Adds an "Open with PS5 PKG Tool" verb that launches the app with <c>--open "&lt;path&gt;"</c>.
/// </summary>
public static class ShellIntegration
{
    private static readonly string[] Extensions = [".pkg", ".ffpfsc", ".ffpkg", ".exfat"];
    private const string VerbName = "PS5PKGTool";

    private static string ExecutablePath =>
        Environment.ProcessPath ?? System.Windows.Forms.Application.ExecutablePath;

    private static string VerbKey(string extension) =>
        $@"Software\Classes\SystemFileAssociations\{extension}\shell\{VerbName}";

    public static bool IsInstalled()
    {
        foreach (string extension in Extensions)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(VerbKey(extension));
            if (key is not null) return true;
        }
        return false;
    }

    public static void Install()
    {
        string executable = ExecutablePath;
        foreach (string extension in Extensions)
        {
            using RegistryKey verb = Registry.CurrentUser.CreateSubKey(VerbKey(extension), true)
                ?? throw new InvalidOperationException($"Unable to create {VerbKey(extension)}.");
            verb.SetValue("MUIVerb", "Open with PS5 PKG Tool");
            verb.SetValue("Icon", $"\"{executable}\"");
            using RegistryKey command = verb.CreateSubKey("command", true)
                ?? throw new InvalidOperationException("Unable to create the shell command key.");
            command.SetValue(null, $"\"{executable}\" --open \"%1\"");
        }
    }

    public static void Uninstall()
    {
        foreach (string extension in Extensions)
            Registry.CurrentUser.DeleteSubKeyTree(VerbKey(extension), throwOnMissingSubKey: false);
    }
}
