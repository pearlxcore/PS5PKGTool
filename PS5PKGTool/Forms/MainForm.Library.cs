using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DarkUI.Controls;
using DarkUI.Forms;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private string _libraryGroupBy = string.Empty;
    private readonly List<(ToolStripMenuItem Item, string Key)> _groupItems = new();

    private void InitializeLibraryTools()
    {
        _groupItems.Add((menuLibraryGroupNone, string.Empty));
        _groupItems.Add((menuLibraryGroupTitleId, "titleid"));
        _groupItems.Add((menuLibraryGroupCategory, "category"));
        _groupItems.Add((menuLibraryGroupRegion, "region"));
        _groupItems.Add((menuLibraryGroupSource, "source"));
        _groupItems.Add((menuLibraryGroupFirmware, "firmware"));
        BuildRenamePresetMenu(menuLibraryRename, all: false);
        BuildRenamePresetMenu(menuLibraryRenameAll, all: true);
    }

    private void menuLibraryReveal_Click(object? sender, EventArgs e)
    {
        if (SelectedGame() is { } game) RevealGame(game);
    }

    private void menuLibraryCopyTitle_Click(object? sender, EventArgs e) => CopyText(SelectedGame()?.Title);

    private void menuLibraryCopyTitleId_Click(object? sender, EventArgs e) => CopyText(SelectedGame()?.TitleId);

    private void menuLibraryCopyContentId_Click(object? sender, EventArgs e) => CopyText(SelectedGame()?.ContentId);

    private void menuLibraryCopyPath_Click(object? sender, EventArgs e) => CopyText(SelectedGame()?.RootPath);

    private void menuLibraryGroupNone_Click(object? sender, EventArgs e) => SetGroupBy(string.Empty);

    private void menuLibraryGroupTitleId_Click(object? sender, EventArgs e) => SetGroupBy("titleid");

    private void menuLibraryGroupCategory_Click(object? sender, EventArgs e) => SetGroupBy("category");

    private void menuLibraryGroupRegion_Click(object? sender, EventArgs e) => SetGroupBy("region");

    private void menuLibraryGroupSource_Click(object? sender, EventArgs e) => SetGroupBy("source");

    private void menuLibraryGroupFirmware_Click(object? sender, EventArgs e) => SetGroupBy("firmware");

    private void menuLibraryDuplicates_Click(object? sender, EventArgs e) => FindDuplicates();

    private void menuLibraryExport_Click(object? sender, EventArgs e) => ExportLibraryCsv();

    private void contextLibrary_Opening(object? sender, CancelEventArgs e)
    {
        bool groupContext = _contextRowIndex >= 0 && gridLibrary.IsGroupRow(_contextRowIndex);
        bool hasGame = !groupContext && SelectedGame() is not null;
        // Single-row actions target one entry; disable them for a multi-selection rather than acting
        // on an arbitrary row.
        bool single = !groupContext && SelectedGames().Count() == 1;

        menuLibraryReveal.Enabled = single;
        menuLibraryCopy.Enabled = single;
        menuLibraryCopyTitle.Enabled = single;
        menuLibraryCopyTitleId.Enabled = single;
        menuLibraryCopyContentId.Enabled = single;
        menuLibraryCopyPath.Enabled = single;
        menuLibraryCopyFileName.Enabled = single;
        // Move and Find Duplicates are disabled for v1.0.0.
        menuLibraryRename.Enabled = hasGame;
        menuLibraryRenameAll.Enabled = _games.Count > 0;
        menuLibraryRenameByPriority.Enabled = hasGame;
        menuLibrarySaveArtwork.Enabled = single;
        menuLibraryMove.Enabled = hasGame;
        menuLibraryDelete.Enabled = hasGame;
        menuLibraryDuplicates.Enabled = false;

        menuLibraryGroupExport.Visible = groupContext;
        menuLibraryGroupArtwork.Visible = groupContext;
        menuLibrarySeparator5.Visible = groupContext;

        menuLibraryExport.Text = SelectedGames().Any() ? "Export selected (CSV)..." : "Export library (CSV)...";
    }

    private void gridLibrary_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;
        DataGridView.HitTestInfo hit = gridLibrary.HitTest(e.X, e.Y);
        _contextRowIndex = hit.RowIndex;
        if (hit.RowIndex < 0 || hit.RowIndex >= gridLibrary.Rows.Count) return;
        DataGridViewRow row = gridLibrary.Rows[hit.RowIndex];
        if (row.Tag is not Ps5GameInfo) return;
        if (!row.Selected)
        {
            gridLibrary.ClearSelection();
            row.Selected = true;
        }
    }

    private void RebuildRecentMenu()
    {
        menuRecent.DropDownItems.Clear();
        if (_settings.RecentFolders.Count == 0)
        {
            menuRecent.DropDownItems.Add(new ToolStripMenuItem("(none)") { Enabled = false });
            return;
        }
        foreach (string folder in _settings.RecentFolders.ToArray())
        {
            string captured = folder;
            var item = new ToolStripMenuItem(captured);
            item.Click += async (_, _) =>
            {
                try { await ScanAsync([captured], merge: true); }
                catch (OperationCanceledException) { }
            };
            menuRecent.DropDownItems.Add(item);
        }
        menuRecent.DropDownItems.Add(new ToolStripSeparator());
        var clear = new ToolStripMenuItem("Clear recent folders");
        clear.Click += (_, _) =>
        {
            _settings.RecentFolders.Clear();
            SaveSettingsQuietly();
            RebuildRecentMenu();
        };
        menuRecent.DropDownItems.Add(clear);
    }

    private void AddRecentFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        _settings.RecentFolders.RemoveAll(existing => string.Equals(existing, path, StringComparison.OrdinalIgnoreCase));
        _settings.RecentFolders.Insert(0, path);
        if (_settings.RecentFolders.Count > 10)
            _settings.RecentFolders.RemoveRange(10, _settings.RecentFolders.Count - 10);
        SaveSettingsQuietly();
        RebuildRecentMenu();
    }

    private IReadOnlyList<string> ScanRoots() => _settings.LibraryFolders
        .Concat(_settings.ManualSources)
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    private bool IsUnderLibrary(string path)
    {
        foreach (string folder in _settings.LibraryFolders)
        {
            if (string.IsNullOrWhiteSpace(folder)) continue;
            string prefix = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private bool IsManualSource(string path) =>
        _settings.ManualSources.Contains(path, StringComparer.OrdinalIgnoreCase);

    private void RememberManualSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!_settings.ManualSources.Contains(path, StringComparer.OrdinalIgnoreCase))
        {
            _settings.ManualSources.Add(path);
            SaveSettingsQuietly();
        }
    }

    private void SaveSettingsQuietly()
    {
        try
        {
            // Remember the task list/details split (only while the details panel is shown).
            if (!_settings.TaskDetailsCollapsed && splitTasks.PanelCount > 0)
            {
                int[] sizes = splitTasks.PanelSizes;
                if (sizes.Length > 0 && sizes[0] > 0) _settings.TaskSplitterDistance = sizes[0];
            }
            _stateStore.SaveSettings(_settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    private void menuSaveManifest_Click(object? sender, EventArgs e)
    {
        _stateStore.SaveManifest(_games);
        Logger.Info($"Manifest saved with {_games.Count:N0} game(s).");
        statusLabel.Text = $"Saved manifest with {_games.Count:N0} game(s).";
    }

    private void menuEmptyList_Click(object? sender, EventArgs e)
    {
        if (_games.Count == 0)
        {
            AppDialog.ShowInformation("The library is already empty.", "Empty list");
            return;
        }
        if (AppDialog.ShowWarning(
                "Clear the current library list?\n\nThe cached manifest is emptied, so the list stays empty until you refresh. " +
                "Files on disk are not deleted.",
                "Empty list", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        _scanCancellation?.Cancel();
        _detailCancellation?.Cancel();
        _games = [];
        _visibleGames = [];
        _detailsCache.Clear();
        _stateStore.SaveManifest(_games);
        SetSelectedGame(null);
        ClearDetails();
        ApplyFilter();
        statusCount.Text = "0 games";
        statusLabel.Text = "Library list emptied. Use Refresh to scan again.";
        Logger.Info("Library list emptied.");
    }

    private void menuRemoveMissing_Click(object? sender, EventArgs e)
    {
        int removed = _games.RemoveAll(game => !SourceExists(game));
        if (removed == 0)
        {
            AppDialog.ShowInformation("No cached library items are missing.", "Remove missing items");
            return;
        }
        _stateStore.SaveManifest(_games);
        ApplyFilter();
        statusLabel.Text = $"Removed {removed:N0} missing item(s).";
    }

    private void SetGroupBy(string key)
    {
        _libraryGroupBy = key;
        foreach ((ToolStripMenuItem item, string itemKey) in _groupItems)
            item.Checked = string.Equals(itemKey, key, StringComparison.Ordinal);
        SyncFilterGroupCombo();
        ApplyFilter();
        statusLabel.Text = key.Length == 0 ? "Grouping disabled." : $"Grouped by {GroupLabel(key)}.";
    }

    private static string GroupLabel(string key) => key switch
    {
        "titleid" => "Title ID",
        "category" => "Category",
        "region" => "Region",
        "source" => "Source format",
        "firmware" => "Required firmware",
        _ => "None"
    };

    internal string GroupKey(Ps5GameInfo game) => _libraryGroupBy switch
    {
        "titleid" => game.TitleId,
        "category" => CategoryOf(game),
        "region" => RegionOf(game),
        "source" => game.SourceDescription,
        "firmware" => game.RequiredSystemSoftware,
        _ => string.Empty
    };

    private static string RegionOf(Ps5GameInfo game)
    {
        string id = !string.IsNullOrWhiteSpace(game.ContentId) ? game.ContentId : game.TitleId;
        if (id.Length < 1) return "Unknown";
        return char.ToUpperInvariant(id[0]) switch
        {
            'U' => "Americas",
            'E' => "Europe",
            'J' => "Japan",
            'K' => "Korea",
            'A' => "Asia",
            'H' => "Hong Kong",
            _ => "Other"
        };
    }

    private static string CategoryOf(Ps5GameInfo game)
    {
        string raw = game.ApplicationCategory;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            string token = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? raw;
            if (int.TryParse(token, out int number))
            {
                // Only 1 selects additional content; publisher-specific encodings such as 0x01000000
                // are base applications, matching the engine's ProsperoParam normalization.
                return number switch
                {
                    1 => "DLC",
                    2 => "Patch",
                    3 => "App",
                    _ => "Game"
                };
            }
            return raw;
        }
        return game.Package?.ContentType switch
        {
            0x20 => "Game",
            // 0x21 is PS5 additional content (DLC), not a patch: real patches use the LIH patch-layer
            // envelope, and the engine classifies 0x21 as Dlc (ProsperoPackageCategories). One
            // presentation value ("DLC") is used everywhere so the filter matches exactly.
            0x21 => "DLC",
            0x22 => "DLC",
            _ => game.SourceKind == Ps5SourceKind.LooseDump ? "Game" : "Unknown"
        };
    }

    private Ps5GameInfo? SelectedGame()
    {
        // Prefer the focused row so single-row actions target what the user is looking at, not an
        // arbitrary member of a multi-selection.
        DataGridViewRow? current = gridLibrary.CurrentRow;
        if (current is { Selected: true } && current.Tag is Ps5GameInfo focused) return focused;
        foreach (DataGridViewRow row in gridLibrary.SelectedRows)
            if (row.Tag is Ps5GameInfo game) return game;
        return null;
    }

    private void RevealGame(Ps5GameInfo game)
    {
        try
        {
            if (Directory.Exists(game.RootPath))
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{game.RootPath}\"") { UseShellExecute = true });
            else if (File.Exists(game.RootPath))
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{game.RootPath}\"") { UseShellExecute = true });
            else
                AppDialog.ShowInformation("The source path no longer exists.", "PS5 PKG Tool");
        }
        catch (Exception ex) when (ex is Win32Exception or IOException or ArgumentException)
        {
            AppDialog.ShowError(ex.Message, "Reveal in Explorer");
        }
    }

    private void CopyText(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        try
        {
            Clipboard.SetText(value);
            statusLabel.Text = "Copied to clipboard.";
        }
        catch (ExternalException) { }
    }

    private void FindDuplicates()
    {
        List<IGrouping<string, Ps5GameInfo>> groups = _games
            .GroupBy(DuplicateKey, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .OrderByDescending(group => group.Count())
            .ToList();
        if (groups.Count == 0)
        {
            AppDialog.ShowInformation("No duplicate sources were found.", "Find Duplicates");
            return;
        }

        var lines = new List<string> { $"{groups.Count:N0} duplicate group(s):", string.Empty };
        foreach (IGrouping<string, Ps5GameInfo> group in groups)
        {
            lines.Add($"{group.Key}  ({group.Count()} copies)");
            foreach (Ps5GameInfo game in group)
                lines.Add("    " + game.RootPath);
            lines.Add(string.Empty);
        }
        ShowTextReport("Duplicate PS5 sources", string.Join(Environment.NewLine, lines));
    }

    private static string DuplicateKey(Ps5GameInfo game)
    {
        if (!string.IsNullOrWhiteSpace(game.ContentId)) return "content:" + game.ContentId;
        if (!string.IsNullOrWhiteSpace(game.TitleId)) return $"title:{game.TitleId}:{game.SourceSize}";
        return $"file:{Path.GetFileName(game.RootPath)}:{game.SourceSize}";
    }

    private string? PromptText(string title, string prompt, string initial)
    {
        using var form = new TextPromptForm(title, prompt, initial);
        return form.ShowDialog(this) == DialogResult.OK ? form.Value.Trim() : null;
    }

    private void ShowTextReport(string title, string body)
    {
        using var form = new TextReportForm(title, body);
        form.ShowDialog(this);
    }

    private void ExportLibraryCsv()
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0) games = [.. _visibleGames];
        ExportGamesCsv(games, "PS5-library.csv");
    }

    private void ExportGamesCsv(IReadOnlyList<Ps5GameInfo> games, string defaultFileName)
    {
        if (games.Count == 0)
        {
            AppDialog.ShowInformation("There is nothing to export.", "Export library");
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = defaultFileName,
            Title = "Export PS5 library"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var builder = new StringBuilder();
            builder.AppendLine("Title,TitleId,ContentId,Category,Region,Source,SizeBytes,Version,RequiredFirmware,DRM,FileName,Path");
            foreach (Ps5GameInfo game in games)
            {
                string[] fields =
                [
                    game.Title, game.TitleId, game.ContentId, CategoryOf(game), RegionOf(game),
                    game.SourceDescription, game.SourceSize.ToString(), game.DisplayVersion,
                    game.RequiredSystemSoftware, game.DrmType,
                    Path.GetFileName(game.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                    game.RootPath
                ];
                builder.AppendLine(string.Join(',', fields.Select(Csv)));
            }
            File.WriteAllText(dialog.FileName, builder.ToString(), new UTF8Encoding(true));
            statusLabel.Text = $"Exported {games.Count:N0} games.";
            AppDialog.ShowInformation($"Exported {games.Count:N0} games to:\n{dialog.FileName}",
                "Export complete");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError(ex.Message, "Export library");
        }
    }

    private static string Csv(string value)
    {
        value ??= string.Empty;
        return value.IndexOfAny([',', '"', '\n', '\r']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static void ReplaceSettingPath(List<string> paths, string source, string target)
    {
        for (int index = 0; index < paths.Count; index++)
            if (string.Equals(paths[index], source, StringComparison.OrdinalIgnoreCase))
                paths[index] = target;
    }
}
