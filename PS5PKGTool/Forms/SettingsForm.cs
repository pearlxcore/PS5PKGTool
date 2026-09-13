using System.Diagnostics;
using System.Text.Json;
using DarkUI.Config;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class SettingsForm : DarkUI.Forms.DarkForm
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _originalTheme;

    public SettingsForm(AppSettings settings)
    {
        InitializeComponent();
        Settings = Clone(settings);
        _originalTheme = Settings.Theme;

        cboTheme.Items.AddRange(ThemeManager.Presets.Select(theme => theme.Name).ToArray());
        cboDefaultGroup.Items.AddRange(["None", "Title ID", "Category", "Region", "Source format", "Required firmware"]);

        foreach (string folder in Settings.LibraryFolders) lstFolders.Items.Add(folder);
        LoadToControls();
    }

    public AppSettings Settings { get; private set; }

    public bool ResetLayout { get; private set; }

    public bool ClearCaches { get; private set; }

    private static AppSettings Clone(AppSettings settings) =>
        JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(settings)) ?? new AppSettings();

    private void LoadToControls()
    {
        chkRecursive.Checked = Settings.RecursiveScan;
        chkRefreshOnStartup.Checked = Settings.RefreshOnStartup;
        txtRenameFormat.Text = Settings.RenameFormat;

        SelectCombo(cboTheme, Settings.Theme);
        nudRowHeight.Value = Clamp(Settings.GridRowHeight, nudRowHeight);
        chkShowThumbnails.Checked = Settings.ShowThumbnails;
        chkShowGridLines.Checked = Settings.ShowGridLines;
        SelectCombo(cboDefaultGroup, string.IsNullOrEmpty(Settings.DefaultGroupBy) ? "None" : Settings.DefaultGroupBy);

        nudMaxPreviewMb.Value = Clamp(Settings.MaxPreviewMb, nudMaxPreviewMb);
        nudHexPageKb.Value = Clamp(Settings.HexPageKb, nudHexPageKb);

        nudThumbnailCache.Value = Clamp(Settings.ThumbnailCacheCount, nudThumbnailCache);

        txtOutputDirectory.Text = Settings.OutputDirectory;
        chkOpenOutputAfterTask.Checked = Settings.OpenOutputAfterTask;

        txtDebugPasscode.Text = Settings.DebugPasscode;

        chkConfirmDelete.Checked = Settings.ConfirmDelete;
        chkConfirmMove.Checked = Settings.ConfirmMove;
        chkPermanentDelete.Checked = Settings.PermanentDelete;
    }

    private void SaveFromControls()
    {
        Settings.LibraryFolders = lstFolders.Items.Cast<string>().ToList();
        Settings.RecursiveScan = chkRecursive.Checked;
        Settings.RefreshOnStartup = chkRefreshOnStartup.Checked;
        Settings.RenameFormat = txtRenameFormat.Text.Trim();

        Settings.Theme = cboTheme.SelectedItem as string ?? Settings.Theme;
        Settings.GridRowHeight = (int)nudRowHeight.Value;
        Settings.ShowThumbnails = chkShowThumbnails.Checked;
        Settings.ShowGridLines = chkShowGridLines.Checked;
        Settings.DefaultGroupBy = GroupKeyFor(cboDefaultGroup.SelectedItem as string);

        Settings.MaxPreviewMb = (int)nudMaxPreviewMb.Value;
        Settings.HexPageKb = (int)nudHexPageKb.Value;

        Settings.ThumbnailCacheCount = (int)nudThumbnailCache.Value;

        Settings.OutputDirectory = txtOutputDirectory.Text.Trim();
        Settings.OpenOutputAfterTask = chkOpenOutputAfterTask.Checked;

        Settings.DebugPasscode = txtDebugPasscode.Text.Trim();

        Settings.ConfirmDelete = chkConfirmDelete.Checked;
        Settings.ConfirmMove = chkConfirmMove.Checked;
        Settings.PermanentDelete = chkPermanentDelete.Checked;
    }

    private static string GroupKeyFor(string? label) => label switch
    {
        "Title ID" => "titleid",
        "Category" => "category",
        "Region" => "region",
        "Source format" => "source",
        "Required firmware" => "firmware",
        _ => string.Empty
    };

    private static void SelectCombo(DarkUI.Controls.DarkComboBox combo, string value)
    {
        int index = combo.Items.IndexOf(value);
        combo.SelectedIndex = index >= 0 ? index : (combo.Items.Count > 0 ? 0 : -1);
    }

    private static decimal Clamp(int value, DarkUI.Controls.DarkNumericUpDown control) =>
        Math.Clamp(value, (int)control.Minimum, (int)control.Maximum);

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        if (!lstFolders.Items.Cast<string>().Contains(folderBrowserDialog.SelectedPath, StringComparer.OrdinalIgnoreCase))
            lstFolders.Items.Add(folderBrowserDialog.SelectedPath);
    }

    private void btnRemove_Click(object? sender, EventArgs e)
    {
        while (lstFolders.SelectedIndices.Count > 0)
            lstFolders.Items.RemoveAt(lstFolders.SelectedIndices[0]);
    }

    private void btnBrowseOutput_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtOutputDirectory.Text) && Directory.Exists(txtOutputDirectory.Text))
            folderBrowserDialog.SelectedPath = txtOutputDirectory.Text;
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
    }

    private void chkShowPasscode_CheckedChanged(object? sender, EventArgs e) =>
        txtDebugPasscode.UseSystemPasswordChar = !chkShowPasscode.Checked;

    private void btnResetLayout_Click(object? sender, EventArgs e) => ResetLayout = true;

    private void btnClearCaches_Click(object? sender, EventArgs e) => ClearCaches = true;

    private void btnOpenLogs_Click(object? sender, EventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Logger.LogDirectory);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Logger.LogDirectory}\"") { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
        }
    }

    private void btnResetSettings_Click(object? sender, EventArgs e)
    {
        if (AppDialog.ShowWarning(
                "Reset all preferences to their defaults? Library folders, recent folders, and manual sources are kept.",
                "Reset settings") != DialogResult.OK)
            return;

        var defaults = new AppSettings
        {
            LibraryFolders = Settings.LibraryFolders,
            RecentFolders = Settings.RecentFolders,
            ManualSources = Settings.ManualSources
        };
        Settings = defaults;
        LoadToControls();
    }

    private void btnExportSettings_Click(object? sender, EventArgs e)
    {
        SaveFromControls();
        exportSettingsDialog.FileName = "PS5PKGTool-settings.json";
        if (exportSettingsDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.WriteAllText(exportSettingsDialog.FileName, JsonSerializer.Serialize(Settings, JsonOptions));
            AppDialog.ShowInformation("Settings exported.", "Export settings");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError(ex.Message, "Export settings");
        }
    }

    private void btnImportSettings_Click(object? sender, EventArgs e)
    {
        if (importSettingsDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            AppSettings imported = JsonSerializer.Deserialize<AppSettings>(
                File.ReadAllText(importSettingsDialog.FileName)) ?? throw new InvalidDataException("Empty settings file.");
            Settings = imported;
            lstFolders.Items.Clear();
            foreach (string folder in Settings.LibraryFolders) lstFolders.Items.Add(folder);
            LoadToControls();
            AppDialog.ShowInformation("Settings imported.", "Import settings");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            AppDialog.ShowError(ex.Message, "Import settings");
        }
    }

    private void btnSave_Click(object? sender, EventArgs e)
    {
        SaveFromControls();
        Logger.Info("Settings saved.");
        DialogResult = DialogResult.OK;
        Close();
    }

    private void cboTheme_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cboTheme.SelectedItem is string name)
            ThemeManager.Apply(ResolveTheme(name));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Revert the live preview if the user did not confirm.
        if (DialogResult != DialogResult.OK)
            ThemeManager.Apply(ResolveTheme(_originalTheme));
        base.OnFormClosing(e);
    }

    private static Theme ResolveTheme(string name) =>
        ThemeManager.Presets.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) ?? ThemeManager.BuiltIn.Default;
}
