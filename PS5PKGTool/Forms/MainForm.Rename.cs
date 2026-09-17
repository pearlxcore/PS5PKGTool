using DarkUI.Controls;
using DarkUI.Forms;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    // ---------------------------------------------------------------- menu entry points

    private void menuLibraryRenameByPriority_Click(object? sender, EventArgs e) => RenameGamesByInstallOrder();

    /// <summary>Builds the preset dropdown for Rename (selected) or Rename All.</summary>
    private void BuildRenamePresetMenu(ToolStripMenuItem parent, bool all)
    {
        parent.DropDownItems.Clear();
        foreach (Ps5RenameFormat preset in Ps5RenameFormats.Presets)
        {
            string format = preset.Format;
            var item = new ToolStripMenuItem(preset.Label);
            item.Click += (_, _) => RunRenameWithFormat(format, all);
            parent.DropDownItems.Add(item);
        }
        parent.DropDownItems.Add(new DarkToolStripSeparator());
        var custom = new ToolStripMenuItem(Ps5RenameFormats.CustomLabel);
        custom.Click += (_, _) => RunRenameWithFormat(RenameFormatOrDefault(_settings.RenameFormat), all);
        parent.DropDownItems.Add(custom);
    }

    private void RunRenameWithFormat(string format, bool all)
    {
        if (all) RenameAllGames(format);
        else RenameSelectedGames(format);
    }

    // ---------------------------------------------------------------- name building

    private static string RenameFormatOrDefault(string? format) =>
        string.IsNullOrWhiteSpace(format) ? "{TITLE} [{TITLE_ID}]" : format.Trim();

    private static Ps5RenameTokens BuildRenameTokens(Ps5GameInfo game) => new(
        Title: game.Title,
        TitleId: game.TitleId,
        ContentId: game.ContentId,
        ConceptId: game.ConceptId,
        Version: game.DisplayVersion,
        ContentVersion: game.ContentVersion,
        MasterVersion: game.MasterVersion,
        Category: CategoryOf(game),
        Region: RegionOf(game),
        Platform: game.Platform,
        SystemVersion: game.RequiredSystemSoftware,
        SdkVersion: game.SdkVersion,
        Source: game.SourceDescription,
        Size: game.SourceSize > 0 ? FormatBytes(game.SourceSize) : string.Empty,
        Language: game.DefaultLanguage,
        Drm: game.DrmType,
        Date: game.CreationDate,
        Tool: game.ToolVersion);

    private static string BuildBaseName(Ps5GameInfo game, string format) =>
        Ps5RenameFormatter.Expand(format, BuildRenameTokens(game), RenameFallbackName(game));

    private static string RenameFallbackName(Ps5GameInfo game)
    {
        string name = Path.GetFileNameWithoutExtension(game.RootPath);
        if (string.IsNullOrWhiteSpace(name)) name = LibraryFileName(game);
        return string.IsNullOrWhiteSpace(name) ? "PS5_GAME" : name;
    }

    // ---------------------------------------------------------------- single / batch

    private void RenameSelectedGames(string format)
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0)
        {
            AppDialog.ShowInformation("Select one or more items to rename.", "Rename");
            return;
        }
        RenameGames(games, $"Rename {games.Count:N0} selected item(s)?", format);
    }

    private void RenameAllGames(string format)
    {
        if (_games.Count == 0)
        {
            AppDialog.ShowInformation("The library is empty.", "Rename");
            return;
        }
        if (IsLibraryFilterActive() && AppDialog.ShowWarning(
                "A filter is active. Rename All will rename every item in the library, including the ones that are hidden.\n\nContinue?",
                "Rename All", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;
        RenameGames(_games.ToList(), $"Rename all {_games.Count:N0} item(s)?", format);
    }

    private void RenameGames(IReadOnlyList<Ps5GameInfo> games, string question, string format)
    {
        if (RefuseIfBusy(games)) return;
        List<(Ps5GameInfo Game, string BaseName)> plans = BuildRenamePlans(games, format);
        if (!ConfirmRenamePreview(plans, question)) return;
        ApplyRenames(plans);
    }

    private void RenameGamesByInstallOrder()
    {
        List<Ps5GameInfo> packages = SelectedGames()
            .Where(game => game.SourceKind == Ps5SourceKind.SonyPackage)
            .ToList();
        if (packages.Count == 0)
        {
            AppDialog.ShowInformation("Select one or more package (.pkg) files.", "Rename by install order");
            return;
        }
        if (RefuseIfBusy(packages)) return;
        string format = RenameFormatOrDefault(_settings.RenameFormat);
        var plans = new List<(Ps5GameInfo Game, string BaseName)>(BuildInstallOrderPlans(packages, format));
        if (!ConfirmRenamePreview(plans,
                $"Rename {packages.Count:N0} package(s) by install order?\n\n" +
                "Each Title ID group is numbered base game first, then updates, then add-ons."))
            return;
        ApplyRenames(plans);
    }

    /// <summary>Builds the base name for each selected item in selection order.</summary>
    private static List<(Ps5GameInfo Game, string BaseName)> BuildRenamePlans(
        IReadOnlyList<Ps5GameInfo> games, string format)
    {
        var plans = new List<(Ps5GameInfo Game, string BaseName)>(games.Count);
        foreach (Ps5GameInfo game in games)
            plans.Add((game, BuildBaseName(game, format)));
        return plans;
    }

    /// <summary>
    /// Shows every planned rename (old name, new name, conflicts and no-ops) and returns true when the
    /// user confirms. Conflicts are shown with the numbered name they will actually receive.
    /// </summary>
    private bool ConfirmRenamePreview(IReadOnlyList<(Ps5GameInfo Game, string BaseName)> plans, string question)
    {
        var lines = new List<string> { question, string.Empty };
        int moves = 0, conflicts = 0, unchanged = 0, errors = 0;
        foreach ((Ps5GameInfo game, string baseName) in plans)
        {
            if (!TryResolveRenameTarget(game, baseName, out string source, out string target, out bool conflict, out string? error))
            {
                errors++;
                lines.Add($"  x {LibraryFileName(game)} — {error}");
                continue;
            }
            if (target.Length == 0)
            {
                unchanged++;
                lines.Add($"  = {LibraryFileName(game)} (unchanged)");
                continue;
            }
            moves++;
            if (conflict) conflicts++;
            lines.Add($"  {LibraryFileName(game)}");
            lines.Add($"    -> {Path.GetFileName(target)}{(conflict ? "   [name already taken]" : string.Empty)}");
        }

        lines.Insert(1,
            $"{moves:N0} to rename, {conflicts:N0} renamed to avoid a clash, {unchanged:N0} unchanged" +
            (errors > 0 ? $", {errors:N0} cannot be renamed" : string.Empty) + ".");
        if (moves == 0)
        {
            AppDialog.ShowInformation(string.Join(Environment.NewLine, lines), "Rename");
            return false;
        }
        return AppDialog.ShowWarning(string.Join(Environment.NewLine, lines), "Rename",
            DarkDialogButton.YesNo) == DialogResult.Yes;
    }

    private void ApplyRenames(IReadOnlyList<(Ps5GameInfo Game, string BaseName)> plans)
    {
        int renamed = 0;
        int skipped = 0;
        foreach ((Ps5GameInfo game, string baseName) in plans)
        {
            if (TryRenamePath(game, baseName, out string? error)) renamed++;
            else
            {
                skipped++;
                if (error is not null) Logger.Warn($"Rename skipped for '{game.RootPath}': {error}");
            }
        }

        if (renamed > 0) SaveSettingsQuietly();
        _stateStore.SaveManifest(_games);
        ApplyFilter();
        Logger.Info($"Renamed {renamed:N0} item(s); skipped {skipped:N0}.");
        statusLabel.Text = $"Renamed {renamed:N0} item(s); skipped {skipped:N0}.";
        if (renamed == 0 && skipped > 0)
            AppDialog.ShowWarning("No items were renamed. Check the log for details.", "Rename");
    }

    private IEnumerable<(Ps5GameInfo Game, string BaseName)> BuildInstallOrderPlans(
        IReadOnlyList<Ps5GameInfo> games, string format)
    {
        IEnumerable<IGrouping<string, Ps5GameInfo>> groups = games
            .Where(game => game.SourceKind == Ps5SourceKind.SonyPackage)
            .GroupBy(game => game.TitleId, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key))
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<string, Ps5GameInfo> group in groups)
        {
            List<Ps5GameInfo> ordered = group
                .OrderBy(game => CategoryPriority(CategoryOf(game)))
                .ThenBy(game => game.DisplayVersion, StringComparer.OrdinalIgnoreCase)
                .ThenBy(game => game.RootPath, StringComparer.OrdinalIgnoreCase)
                .ToList();
            for (int index = 0; index < ordered.Count; index++)
            {
                Ps5GameInfo game = ordered[index];
                string baseName = BuildBaseName(game, format);
                yield return (game, $"{index:D2} - {baseName}");
            }
        }
    }

    private static int CategoryPriority(string category) => category switch
    {
        "Game" => 0,
        "Patch" => 1,
        "DLC" or "Add-on" => 2,
        "App" => 3,
        _ => 4
    };

    private bool IsLibraryFilterActive() =>
        _visibleGames.Count != _games.Count || searchLibrary.SearchText.Trim().Length > 0;

    // ---------------------------------------------------------------- the actual move

    private bool TryRenamePath(Ps5GameInfo game, string baseName, out string? error)
    {
        if (!TryResolveRenameTarget(game, baseName, out string source, out string target, out _, out error))
            return false;
        if (target.Length == 0) return false; // already has the requested name

        try
        {
            if (Directory.Exists(source)) Directory.Move(source, target);
            else File.Move(source, target);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            error = ex.Message;
            return false;
        }

        RewriteGamePath(game, source, target);
        return true;
    }

    /// <summary>
    /// Computes where a rename would land without touching disk. Returns false only on an error; a
    /// successful call with an empty <paramref name="target"/> means the name is already correct.
    /// <paramref name="conflict"/> is true when the plain target existed and a numbered name was chosen.
    /// </summary>
    private static bool TryResolveRenameTarget(Ps5GameInfo game, string baseName,
        out string source, out string target, out bool conflict, out string? error)
    {
        source = game.RootPath;
        target = string.Empty;
        conflict = false;
        error = null;

        if (string.IsNullOrWhiteSpace(source))
        {
            error = "empty path";
            return false;
        }

        bool isDirectory = Directory.Exists(source);
        bool isFile = File.Exists(source);
        if (!isDirectory && !isFile)
        {
            error = "source path no longer exists";
            return false;
        }

        string safeBase = Ps5RenameFormatter.Sanitize(baseName);
        if (safeBase.Length == 0)
        {
            error = "empty name";
            return false;
        }

        string? parent = Path.GetDirectoryName(source);
        if (string.IsNullOrEmpty(parent))
        {
            error = "no parent folder";
            return false;
        }

        string extension = isDirectory ? string.Empty : Path.GetExtension(source);
        string raw = Path.Combine(parent, safeBase + extension);
        if (string.Equals(raw, source, StringComparison.OrdinalIgnoreCase)) return true;

        conflict = File.Exists(raw) || Directory.Exists(raw);
        target = conflict ? MakeUniquePath(raw, isDirectory) : raw;
        return true;
    }

    private static string MakeUniquePath(string target, bool isDirectory)
    {
        if (!File.Exists(target) && !Directory.Exists(target)) return target;
        string? parent = Path.GetDirectoryName(target);
        string name = isDirectory ? Path.GetFileName(target) : Path.GetFileNameWithoutExtension(target);
        string extension = isDirectory ? string.Empty : Path.GetExtension(target);
        for (int index = 2; index < 10000; index++)
        {
            string candidate = Path.Combine(parent ?? string.Empty, $"{name} ({index}){extension}");
            if (!File.Exists(candidate) && !Directory.Exists(candidate)) return candidate;
        }
        return target;
    }
}
