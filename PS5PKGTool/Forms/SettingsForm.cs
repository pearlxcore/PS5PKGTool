using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using DarkUI.Config;
using DarkUI.Forms;
using PS5PKGTool.Core.Backends;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class SettingsForm : DarkUI.Forms.DarkForm
{
    /// <summary>Shared with AppStateStore so a file round-trips identically wherever it is read.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static readonly int[] DensityRowHeights = [18, 22, 28];

    private readonly string _originalTheme;
    private readonly Func<AppSettings, (bool Success, string? Error)>? _save;
    private string _initialSignature = string.Empty;
    private bool _dirtyHooked;
    private bool _loading;
    private bool _syncingDensity;
    private bool _syncingRename;

    public SettingsForm(AppSettings settings, Func<AppSettings, (bool Success, string? Error)>? save = null)
    {
        InitializeComponent();
        _save = save;
        Settings = AppSettingsNormalizer.Normalize(Clone(settings));
        _originalTheme = Settings.Theme;

        cboTheme.Items.AddRange(ThemeManager.Presets.Select(theme => theme.Name).ToArray());
        cboDensity.Items.AddRange(["Compact", "Normal", "Comfortable"]);
        cboDefaultGroup.Items.AddRange(["None", "Title ID", "Family (base + updates + DLC)", "Category", "Region", "Source format", "Required firmware"]);
        cboDefaultBackend.Items.AddRange(BackendRegistry.All.Select(backend => backend.DisplayName).ToArray());
        cboRenamePreset.Items.Add(Ps5RenameFormats.CustomLabel);
        cboRenamePreset.Items.AddRange(Ps5RenameFormats.Presets.Select(preset => preset.Label).ToArray());
        cboRenameToken.Items.AddRange(Ps5RenameFormatter.TokenNames.ToArray());

        cboDensity.SelectedIndexChanged += (_, _) => ApplyDensityPreset();
        nudRowHeight.ValueChanged += (_, _) => SyncDensityFromRowHeight();

        foreach (string folder in Settings.LibraryFolders) lstFolders.Items.Add(folder);
        foreach (string source in Settings.ManualSources) lstManualSources.Items.Add(source);

        LoadToControls();
        HookDirtyTracking();
        _initialSignature = Signature();
        UpdateSaveEnabled();
    }

    public AppSettings Settings { get; private set; }

    public bool ResetLayout { get; private set; }

    public bool ClearCaches { get; private set; }

    private static AppSettings Clone(AppSettings settings) =>
        JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(settings, JsonOptions)) ?? new AppSettings();

    // ---------------------------------------------------------------- load / capture

    private void LoadToControls()
    {
        _loading = true;
        try
        {
            chkRecursive.Checked = Settings.RecursiveScan;
            chkRefreshOnStartup.Checked = Settings.RefreshOnStartup;

            SelectCombo(cboTheme, Settings.Theme);
            nudRowHeight.Value = Clamp(Settings.GridRowHeight, nudRowHeight);
            SyncDensityFromRowHeight();
            chkShowThumbnails.Checked = Settings.ShowThumbnails;
            chkShowGridLines.Checked = Settings.ShowGridLines;
            SelectCombo(cboDefaultGroup, GroupLabelFor(Settings.DefaultGroupBy));

            txtRenameFormat.Text = Settings.RenameFormat;
            SyncRenamePreset();

            nudMaxPreviewMb.Value = Clamp(Settings.MaxPreviewMb, nudMaxPreviewMb);
            nudHexPageKb.Value = Clamp(Settings.HexPageKb, nudHexPageKb);
            nudThumbnailCache.Value = Clamp(Settings.ThumbnailCacheCount, nudThumbnailCache);

            txtOutputDirectory.Text = Settings.OutputDirectory;
            SelectBackend(Settings.BuildBackend);
            txtDebugPasscode.Text = Settings.DebugPasscode;
            chkOpenOutputAfterTask.Checked = Settings.OpenOutputAfterTask;

            chkConfirmMove.Checked = Settings.ConfirmMove;
            chkConfirmDelete.Checked = Settings.ConfirmDelete;
            chkPermanentDelete.Checked = Settings.PermanentDelete;
        }
        finally
        {
            _loading = false;
        }
        UpdateRenameFeedback();
    }

    private void ApplyControlsToDraft()
    {
        Settings.LibraryFolders = lstFolders.Items.Cast<string>().ToList();
        Settings.ManualSources = lstManualSources.Items.Cast<string>().ToList();
        Settings.RecursiveScan = chkRecursive.Checked;
        Settings.RefreshOnStartup = chkRefreshOnStartup.Checked;

        Settings.Theme = cboTheme.SelectedItem as string ?? Settings.Theme;
        Settings.GridRowHeight = (int)nudRowHeight.Value;
        Settings.ShowThumbnails = chkShowThumbnails.Checked;
        Settings.ShowGridLines = chkShowGridLines.Checked;
        Settings.DefaultGroupBy = GroupKeyFor(cboDefaultGroup.SelectedItem as string);

        Settings.RenameFormat = txtRenameFormat.Text.Trim();

        Settings.MaxPreviewMb = (int)nudMaxPreviewMb.Value;
        Settings.HexPageKb = (int)nudHexPageKb.Value;
        Settings.ThumbnailCacheCount = (int)nudThumbnailCache.Value;

        string output = txtOutputDirectory.Text.Trim();
        Settings.OutputDirectory = output.Length == 0 ? string.Empty : AppSettingsNormalizer.NormalizePath(output);
        Settings.BuildBackend = SelectedBackendId();
        Settings.DebugPasscode = txtDebugPasscode.Text;
        Settings.OpenOutputAfterTask = chkOpenOutputAfterTask.Checked;

        Settings.ConfirmMove = chkConfirmMove.Checked;
        Settings.ConfirmDelete = chkConfirmDelete.Checked;
        Settings.PermanentDelete = chkPermanentDelete.Checked;
    }

    /// <summary>Captures the controls and validates them, returning an inline message on failure.</summary>
    private bool TryApplyControls(out string? error)
    {
        if (txtDebugPasscode.Text.Length > 0 && !AppSettingsNormalizer.IsValidPasscode(txtDebugPasscode.Text))
        {
            error = "The debug passcode must be blank or exactly 32 printable ASCII characters.";
            tabsSettings.SelectedTab = tabPaths;
            txtDebugPasscode.Focus();
            return false;
        }

        ApplyControlsToDraft();
        error = null;
        return true;
    }

    // ---------------------------------------------------------------- theme / density

    private void ApplyDensityPreset()
    {
        if (_syncingDensity || _loading) return;
        int index = Math.Clamp(cboDensity.SelectedIndex, 0, DensityRowHeights.Length - 1);
        _syncingDensity = true;
        try { nudRowHeight.Value = Math.Clamp(DensityRowHeights[index], (int)nudRowHeight.Minimum, (int)nudRowHeight.Maximum); }
        finally { _syncingDensity = false; }
    }

    private void SyncDensityFromRowHeight()
    {
        if (_syncingDensity) return;
        int height = (int)nudRowHeight.Value;
        int index = height <= 19 ? 0 : height <= 24 ? 1 : 2;
        _syncingDensity = true;
        try { cboDensity.SelectedIndex = index; }
        finally { _syncingDensity = false; }
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

    // ---------------------------------------------------------------- dirty tracking

    private void HookDirtyTracking()
    {
        if (_dirtyHooked) return;
        _dirtyHooked = true;
        foreach (Control control in EnumerateControls(this))
        {
            switch (control)
            {
                case DarkUI.Controls.DarkNumericUpDown numeric: numeric.ValueChanged += OnDirtyChanged; break;
                case CheckBox check: check.CheckedChanged += OnDirtyChanged; break;
                case ComboBox combo: combo.SelectedIndexChanged += OnDirtyChanged; break;
                case TextBox text: text.TextChanged += OnDirtyChanged; break;
            }
        }
    }

    private static IEnumerable<Control> EnumerateControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (Control grandchild in EnumerateControls(child)) yield return grandchild;
        }
    }

    private void OnDirtyChanged(object? sender, EventArgs e) => UpdateSaveEnabled();

    private string Signature()
    {
        ApplyControlsToDraft();
        return JsonSerializer.Serialize(AppSettingsNormalizer.Normalize(Clone(Settings)), JsonOptions);
    }

    private void UpdateSaveEnabled()
    {
        if (_loading || btnSave is null) return;
        btnSave.Enabled = !string.Equals(Signature(), _initialSignature, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- library lists

    private void btnAdd_Click(object? sender, EventArgs e)
    {
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string path = AppSettingsNormalizer.NormalizePath(folderBrowserDialog.SelectedPath);
        if (lstFolders.Items.Cast<string>().Contains(path, StringComparer.OrdinalIgnoreCase)) return;
        lstFolders.Items.Add(path);
        UpdateSaveEnabled();
    }

    private void btnRemove_Click(object? sender, EventArgs e)
    {
        while (lstFolders.SelectedIndices.Count > 0)
            lstFolders.Items.RemoveAt(lstFolders.SelectedIndices[0]);
        UpdateSaveEnabled();
    }

    private void btnRemoveSource_Click(object? sender, EventArgs e)
    {
        while (lstManualSources.SelectedIndices.Count > 0)
            lstManualSources.Items.RemoveAt(lstManualSources.SelectedIndices[0]);
        UpdateSaveEnabled();
    }

    // ---------------------------------------------------------------- naming

    private void txtRenameFormat_TextChanged(object? sender, EventArgs e)
    {
        if (_syncingRename) return;
        UpdateRenameFeedback();
    }

    private void cboRenamePreset_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_syncingRename || cboRenamePreset.SelectedIndex <= 0) return;
        int presetIndex = cboRenamePreset.SelectedIndex - 1;
        if (presetIndex >= Ps5RenameFormats.Presets.Count) return;
        txtRenameFormat.Text = Ps5RenameFormats.Presets[presetIndex].Format;
    }

    private void btnInsertToken_Click(object? sender, EventArgs e)
    {
        if (cboRenameToken.SelectedItem is not string token) return;
        int start = txtRenameFormat.SelectionStart;
        txtRenameFormat.Text = txtRenameFormat.Text.Insert(start, token);
        txtRenameFormat.SelectionStart = start + token.Length;
        txtRenameFormat.Focus();
    }

    private void SyncRenamePreset()
    {
        _syncingRename = true;
        try
        {
            int index = -1;
            for (int i = 0; i < Ps5RenameFormats.Presets.Count; i++)
                if (string.Equals(Ps5RenameFormats.Presets[i].Format, Settings.RenameFormat, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            cboRenamePreset.SelectedIndex = index >= 0 ? index + 1 : 0;
        }
        finally
        {
            _syncingRename = false;
        }
    }

    private void UpdateRenameFeedback()
    {
        if (lblRenamePreview is null) return;
        string format = txtRenameFormat.Text;
        string preview = Ps5RenameFormatter.Expand(format, SampleTokens, "PS5_GAME");
        lblRenamePreview.Text = format.Trim().Length == 0
            ? "Example: (blank uses the default format)"
            : $"Example: {preview}";

        string[] unknown = UnknownTokens(format);
        lblRenameUnknown.Text = unknown.Length == 0
            ? string.Empty
            : "Unknown token(s): " + string.Join(", ", unknown) + " — they will be left as typed. Valid tokens: "
              + string.Join(" ", Ps5RenameFormatter.TokenNames);
    }

    private static string[] UnknownTokens(string format)
    {
        var known = new HashSet<string>(Ps5RenameFormatter.TokenNames, StringComparer.OrdinalIgnoreCase);
        return Regex.Matches(format, @"\{[A-Za-z0-9_]+\}")
            .Select(match => match.Value)
            .Where(token => !known.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static readonly Ps5RenameTokens SampleTokens = new(
        Title: "Example Game",
        TitleId: "PPSA12345",
        ContentId: "UP0000-PPSA12345_00-EXAMPLE0000000000",
        ConceptId: "100000",
        Version: "01.001.000",
        ContentVersion: "01.001",
        MasterVersion: "01.001.000",
        Category: "Game",
        Region: "Europe",
        Platform: "PS5",
        SystemVersion: "10.000.000",
        SdkVersion: "0.0.0",
        Source: "Dump Files",
        Size: "12.3 GiB",
        Language: "en-US",
        Drm: "None",
        Date: "2026-09-17",
        Tool: "PS5 PKG Tool");

    // ---------------------------------------------------------------- output / passcode

    private void btnBrowseOutput_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtOutputDirectory.Text) && Directory.Exists(txtOutputDirectory.Text))
            folderBrowserDialog.SelectedPath = txtOutputDirectory.Text;
        if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
    }

    private void txtDebugPasscode_TextChanged(object? sender, EventArgs e)
    {
        if (_loading) return;
        if (txtDebugPasscode.Text.Length == 0)
        {
            lblPasscodeHint.Text = "Blank uses the default all-zero passcode. Otherwise exactly 32 printable ASCII characters.";
            return;
        }
        lblPasscodeHint.Text = AppSettingsNormalizer.IsValidPasscode(txtDebugPasscode.Text)
            ? "Valid passcode (32 printable ASCII characters)."
            : $"Invalid: {txtDebugPasscode.Text.Length} character(s); needs exactly 32 printable ASCII characters.";
    }

    private void chkShowPasscode_CheckedChanged(object? sender, EventArgs e) =>
        txtDebugPasscode.UseSystemPasswordChar = !chkShowPasscode.Checked;

    private void SelectBackend(string? id)
    {
        for (int index = 0; index < BackendRegistry.All.Count; index++)
        {
            if (string.Equals(BackendRegistry.All[index].Id, id, StringComparison.OrdinalIgnoreCase))
            {
                cboDefaultBackend.SelectedIndex = index;
                return;
            }
        }
        cboDefaultBackend.SelectedIndex = 0;
    }

    private string SelectedBackendId() =>
        cboDefaultBackend.SelectedIndex >= 0 && cboDefaultBackend.SelectedIndex < BackendRegistry.All.Count
            ? BackendRegistry.All[cboDefaultBackend.SelectedIndex].Id
            : BackendRegistry.DefaultId;

    // ---------------------------------------------------------------- maintenance

    private void btnResetLayout_Click(object? sender, EventArgs e)
    {
        ResetLayout = true;
        AppDialog.ShowInformation(
            "The library columns will return to their default order, widths and visibility when you save.",
            "Reset column layout");
    }

    private void btnClearCaches_Click(object? sender, EventArgs e)
    {
        ClearCaches = true;
        AppDialog.ShowInformation(
            "Details and thumbnail caches will be cleared when you save. Cancel discards this.",
            "Clear caches");
    }

    private void btnOpenLogs_Click(object? sender, EventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Logger.LogDirectory);
            Process.Start(new ProcessStartInfo("explorer.exe", $"\"{Logger.LogDirectory}\"") { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError($"Could not open the log folder:\n\n{ex.Message}", "Open log folder");
        }
    }

    private void btnResetSettings_Click(object? sender, EventArgs e)
    {
        if (AppDialog.ShowWarning(
                "Reset all preferences to their defaults?\n\nLibrary folders, manual sources, recent folders, saved views and window/column layout are kept. This does not clear caches.",
                "Reset preferences", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        Settings = AppSettingsNormalizer.Normalize(new AppSettings
        {
            LibraryFolders = [.. Settings.LibraryFolders],
            ManualSources = [.. Settings.ManualSources],
            RecentFolders = [.. Settings.RecentFolders],
            SavedViews = [.. Settings.SavedViews],
            LibraryColumnOrder = [.. Settings.LibraryColumnOrder],
            LibraryHiddenColumns = [.. Settings.LibraryHiddenColumns],
            LibraryColumnWeights = new Dictionary<string, float>(Settings.LibraryColumnWeights, StringComparer.Ordinal),
            LibrarySortKeys = [.. Settings.LibrarySortKeys],
            LibrarySortColumn = Settings.LibrarySortColumn,
            LibrarySortAscending = Settings.LibrarySortAscending,
            WindowWidth = Settings.WindowWidth,
            WindowHeight = Settings.WindowHeight,
            WindowMaximized = Settings.WindowMaximized
        });
        LoadToControls();
        UpdateSaveEnabled();
    }

    private void btnExportSettings_Click(object? sender, EventArgs e)
    {
        if (!TryApplyControls(out string? error))
        {
            AppDialog.ShowWarning(error!, "Export preferences");
            return;
        }

        exportSettingsDialog.FileName = "PS5PKGTool-settings.json";
        if (exportSettingsDialog.ShowDialog(this) != DialogResult.OK) return;
        AppSettings export = Clone(Settings);
        if (!chkExportCredentials.Checked)
            export.DebugPasscode = string.Empty;
        try
        {
            File.WriteAllText(exportSettingsDialog.FileName, JsonSerializer.Serialize(export, JsonOptions));
            AppDialog.ShowInformation(
                chkExportCredentials.Checked
                    ? "Preferences exported, including the debug passcode."
                    : "Preferences exported. The debug passcode was excluded.",
                "Export preferences");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError(ex.Message, "Export preferences");
        }
    }

    private void btnImportSettings_Click(object? sender, EventArgs e)
    {
        if (importSettingsDialog.ShowDialog(this) != DialogResult.OK) return;

        AppSettings candidate;
        try
        {
            string json = File.ReadAllText(importSettingsDialog.FileName);
            candidate = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
                ?? throw new InvalidDataException("The file did not contain settings.");
            candidate = AppSettingsNormalizer.Normalize(candidate);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            AppDialog.ShowError($"The settings file could not be read:\n\n{ex.Message}", "Import preferences");
            return;
        }

        List<string> changes = DescribeChanges(Settings, candidate);
        string message = changes.Count == 0
            ? "The file matches the current preferences."
            : "Import these changes?\n\n" + string.Join(Environment.NewLine, changes.Take(20))
              + (changes.Count > 20 ? Environment.NewLine + $"… and {changes.Count - 20:N0} more" : string.Empty);
        if (AppDialog.ShowWarning(message, "Import preferences", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        Settings = candidate;
        lstFolders.Items.Clear();
        foreach (string folder in Settings.LibraryFolders) lstFolders.Items.Add(folder);
        lstManualSources.Items.Clear();
        foreach (string source in Settings.ManualSources) lstManualSources.Items.Add(source);
        LoadToControls();
        _initialSignature = Signature();
        UpdateSaveEnabled();
        AppDialog.ShowInformation("Preferences imported. Review them and choose Save to keep them.", "Import preferences");
    }

    private static List<string> DescribeChanges(AppSettings before, AppSettings after)
    {
        var changes = new List<string>();
        AppendPathChange(changes, "Library folders", before.LibraryFolders, after.LibraryFolders);
        AppendPathChange(changes, "Manual sources", before.ManualSources, after.ManualSources);
        AppendChange(changes, "Scan subfolders", before.RecursiveScan, after.RecursiveScan);
        AppendChange(changes, "Refresh on startup", before.RefreshOnStartup, after.RefreshOnStartup);
        AppendChange(changes, "Theme", before.Theme, after.Theme);
        AppendChange(changes, "Grid row height", before.GridRowHeight, after.GridRowHeight);
        AppendChange(changes, "Default grouping", before.DefaultGroupBy, after.DefaultGroupBy);
        AppendChange(changes, "Rename format", before.RenameFormat, after.RenameFormat);
        AppendChange(changes, "Max preview (MiB)", before.MaxPreviewMb, after.MaxPreviewMb);
        AppendChange(changes, "Hex page (KiB)", before.HexPageKb, after.HexPageKb);
        AppendChange(changes, "Thumbnail cache entries", before.ThumbnailCacheCount, after.ThumbnailCacheCount);
        AppendChange(changes, "Default output folder", before.OutputDirectory, after.OutputDirectory);
        AppendChange(changes, "Default builder", before.BuildBackend, after.BuildBackend);
        AppendChange(changes, "Debug passcode", Mask(before.DebugPasscode), Mask(after.DebugPasscode));
        AppendChange(changes, "Open output after success", before.OpenOutputAfterTask, after.OpenOutputAfterTask);
        AppendChange(changes, "Confirm moves", before.ConfirmMove, after.ConfirmMove);
        AppendChange(changes, "Confirm recycling", before.ConfirmDelete, after.ConfirmDelete);
        AppendChange(changes, "Permanent deletion", before.PermanentDelete, after.PermanentDelete);
        return changes;
    }

    private static void AppendChange(List<string> changes, string label, object? before, object? after)
    {
        string left = before?.ToString() ?? string.Empty;
        string right = after?.ToString() ?? string.Empty;
        if (!string.Equals(left, right, StringComparison.Ordinal))
            changes.Add($"• {label}: {(left.Length == 0 ? "(none)" : left)} -> {(right.Length == 0 ? "(none)" : right)}");
    }

    private static void AppendPathChange(List<string> changes, string label, List<string> before, List<string> after)
    {
        var removed = before.Except(after, StringComparer.OrdinalIgnoreCase).ToList();
        var added = after.Except(before, StringComparer.OrdinalIgnoreCase).ToList();
        if (removed.Count > 0) changes.Add($"• {label} removed: {string.Join(", ", removed.Take(5))}");
        if (added.Count > 0) changes.Add($"• {label} added: {string.Join(", ", added.Take(5))}");
    }

    private static string Mask(string? value) => string.IsNullOrEmpty(value) ? string.Empty : new string('*', value.Length);

    // ---------------------------------------------------------------- save

    private void btnSave_Click(object? sender, EventArgs e)
    {
        if (!TryApplyControls(out string? error))
        {
            AppDialog.ShowWarning(error!, "Settings");
            return;
        }

        Settings = AppSettingsNormalizer.Normalize(Settings);
        if (_save is not null)
        {
            while (true)
            {
                (bool success, string? saveError) = _save(Settings);
                if (success) break;
                if (AppDialog.ShowWarning(
                        $"The settings could not be saved:\n\n{saveError}\n\nYour changes are kept. Retry?",
                        "Settings", DarkDialogButton.YesNo) != DialogResult.Yes)
                    return;
            }
        }

        Logger.Info("Settings saved.");
        DialogResult = DialogResult.OK;
        Close();
    }

    // ---------------------------------------------------------------- helpers

    private static string GroupKeyFor(string? label) => label switch
    {
        "Title ID" => "titleid",
        "Family (base + updates + DLC)" => "family",
        "Category" => "category",
        "Region" => "region",
        "Source format" => "source",
        "Required firmware" => "firmware",
        _ => string.Empty
    };

    // The combo lists labels while the setting stores keys; map back so the saved choice round-trips.
    private static string GroupLabelFor(string? key) => key switch
    {
        "titleid" => "Title ID",
        "family" => "Family (base + updates + DLC)",
        "category" => "Category",
        "region" => "Region",
        "source" => "Source format",
        "firmware" => "Required firmware",
        _ => "None"
    };

    private static void SelectCombo(DarkUI.Controls.DarkComboBox combo, string value)
    {
        int index = combo.Items.IndexOf(value);
        combo.SelectedIndex = index >= 0 ? index : (combo.Items.Count > 0 ? 0 : -1);
    }

    private static decimal Clamp(int value, DarkUI.Controls.DarkNumericUpDown control) =>
        Math.Clamp(value, (int)control.Minimum, (int)control.Maximum);
}
