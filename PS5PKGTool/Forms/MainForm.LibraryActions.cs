using System.Runtime.InteropServices;
using DarkUI.Forms;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Tasks;
using PS5PKGTool.Ffpfsc;
using PS5PKGTool.Infrastructure;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private int _contextRowIndex = -1;

    // ---------------------------------------------------------------- copy

    private void menuLibraryCopyFileName_Click(object? sender, EventArgs e) =>
        CopyText(SelectedGame() is { } game ? LibraryFileName(game) : null);

    // ---------------------------------------------------------------- placeholders

    private void menuLibraryConvertFfpfsc_Click(object? sender, EventArgs e) => ConvertSelectedImage(null);

    private void ConvertSelectedImage(Ps5ImageConversionTarget? preferred)
    {
        if (SelectedGame() is not { } game) return;
        string source = game.RootPath;
        bool isDirectory = Directory.Exists(source);
        if (!isDirectory && !File.Exists(source))
        {
            AppDialog.ShowInformation("The selected source no longer exists.", "Convert image");
            return;
        }

        List<Ps5ImageConversionTarget> targets = [];
        if (isDirectory)
        {
            targets.AddRange(Enum.GetValues<Ps5ImageConversionTarget>());
        }
        else
        {
            Ps5ImageFormat format = Ps5ImageConversionService.DetectSource(source);
            foreach (Ps5ImageConversionTarget target in Enum.GetValues<Ps5ImageConversionTarget>())
                if (Ps5ImageConversionService.IsSupported(format, target))
                    targets.Add(target);
        }

        if (targets.Count == 0)
        {
            AppDialog.ShowInformation("The selected source cannot be converted.", "Convert image");
            return;
        }

        using var form = new ConvertImageForm(source, targets, preferred);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        string output = form.OutputPath;
        Ps5ImageConversionTarget selected = form.Target;
        bool overwrite = form.Overwrite;
        string sourceRoute = isDirectory ? "dump" : Ps5ImageConversionService.DetectSource(source) switch
        {
            Ps5ImageFormat.Exfat => "exFAT",
            Ps5ImageFormat.Ufs2 => "FFPKG",
            Ps5ImageFormat.Pfs => "FFPFSC",
            _ => "image"
        };
        string targetRoute = selected switch
        {
            Ps5ImageConversionTarget.Exfat => "exFAT",
            Ps5ImageConversionTarget.Ffpkg => "FFPKG",
            _ => "FFPFSC"
        };

        EnqueueTask(PackageTaskTypes.ImageConvert, $"Convert {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                var bridge = new Progress<Ps5ImageConversionProgress>(value =>
                    progress.Report(new PackageTaskProgress(value.Stage, 0, 0, value.Completed, value.Total, 0, 0,
                        string.Empty)));
                await Ps5ImageConversionService.ConvertAsync(source, output, selected, overwrite, bridge, token)
                    .ConfigureAwait(false);
            },
            sourcePath: source, outputPath: output,
            operation: "Convert", sourceFormat: sourceRoute, targetFormat: targetRoute,
            stagePlan: PackageTaskPlans.ConvertImage,
            payload: Payload(("source", source), ("output", output), ("target", selected.ToString()),
                ("overwrite", overwrite.ToString())),
            onFinished: task =>
            {
                statusLabel.Text = task.Status == PackageTaskStatus.Completed
                    ? $"Converted to {Path.GetFileName(output)}."
                    : $"Image conversion {StatusText(task.Status).ToLowerInvariant()}.";
            });
        statusLabel.Text = "Queued: image conversion. See the Tasks tab.";
    }

    private void menuLibraryMergeBaseUpdate_Click(object? sender, EventArgs e) =>
        ShowPlaceholder("Merge base + update");

    private void ShowPlaceholder(string feature) =>
        AppDialog.ShowInformation($"{feature} is not available yet.", "PS5 PKG Tool");

    // ---------------------------------------------------------------- check patches missing base

    private void menuLibraryMissingBase_Click(object? sender, EventArgs e) => CheckPatchesMissingBase();

    private void CheckPatchesMissingBase()
    {
        var lines = new List<string>();
        int missing = 0;
        foreach (IGrouping<string, Ps5GameInfo> group in _games
            .Where(game => !string.IsNullOrWhiteSpace(game.TitleId))
            .GroupBy(game => game.TitleId, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            bool hasBase = group.Any(game => CategoryOf(game).Equals("Game", StringComparison.OrdinalIgnoreCase));
            List<Ps5GameInfo> patches = group
                .Where(game => CategoryOf(game).Equals("Patch", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (hasBase || patches.Count == 0) continue;

            missing++;
            lines.Add($"{group.Key}: {patches.Count} patch(es) without a base game");
            foreach (Ps5GameInfo patch in patches)
                lines.Add("    " + patch.RootPath);
        }

        if (missing == 0)
        {
            AppDialog.ShowInformation("Every patch has a matching base game.", "Patches missing base");
            return;
        }

        ShowTextReport("Patches missing base",
            $"{missing:N0} title(s) have patches without a base game:{Environment.NewLine}{Environment.NewLine}" +
            string.Join(Environment.NewLine, lines));
    }

    // ---------------------------------------------------------------- save artwork

    private void menuLibrarySaveArtwork_Click(object? sender, EventArgs e) =>
        SaveArtworkForGamesAsync(SelectedGames().ToList());

    private void menuLibraryGroupArtwork_Click(object? sender, EventArgs e) =>
        SaveArtworkForGamesAsync(GroupContextGames());

    private async void SaveArtworkForGamesAsync(IReadOnlyList<Ps5GameInfo> games)
    {
        if (games.Count == 0) return;
        folderBrowserDialog.Description = "Select a folder for the artwork images";
        if (!string.IsNullOrEmpty(_settings.OutputDirectory) && Directory.Exists(_settings.OutputDirectory))
            folderBrowserDialog.SelectedPath = _settings.OutputDirectory;
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string folder = folderBrowserDialog.SelectedPath;

        int saved = 0;
        try
        {
            foreach (Ps5GameInfo game in games)
            {
                Ps5GameDetails details = await GetDetailsAsync(game);
                string baseName = MakeSafeFileName(string.IsNullOrWhiteSpace(game.Title) ? LibraryFileName(game) : game.Title);
                if (baseName.Length == 0) baseName = "artwork";
                foreach ((Ps5ImageData? data, string name) in ArtworkData(details))
                {
                    if (data is null || data.IsEmpty) continue;
                    SaveArtworkImage(data, Path.Combine(folder, baseName + "-" + name));
                    saved++;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException)
        {
            AppDialog.ShowError(ex.Message, "Save artwork");
            return;
        }

        if (saved == 0) AppDialog.ShowInformation("No artwork was available to save.", "Artwork");
        else statusLabel.Text = $"Saved {saved:N0} artwork image(s) to {folder}.";
    }

    private static IEnumerable<(Ps5ImageData? Data, string Name)> ArtworkData(Ps5GameDetails details)
    {
        yield return (details.Icon, "icon0.png");
        yield return (details.Background, "pic0.png");
        yield return (details.Background1, "pic1.png");
        yield return (details.Background2, "pic2.png");
    }

    private async Task<Ps5GameDetails> GetDetailsAsync(Ps5GameInfo game)
    {
        if (_detailsCache.TryGetValue(game.RootPath, out Ps5GameDetails? cached)) return cached;
        Ps5GameDetails details = await _detailsLoader.LoadAsync(game, CancellationToken.None).ConfigureAwait(true);
        _detailsCache[game.RootPath] = details;
        return details;
    }

    // ---------------------------------------------------------------- extract package

    private void menuLibraryExtractPackage_Click(object? sender, EventArgs e) => ExtractPackage(SelectedGame());

    private void ExtractPackage(Ps5GameInfo? game)
    {
        if (game is null || game.SourceKind != Ps5SourceKind.SonyPackage || !File.Exists(game.RootPath))
        {
            AppDialog.ShowInformation("Select a Sony PKG file to extract.", "Extract package");
            return;
        }

        folderBrowserDialog.Description = "Select a folder for package extraction";
        if (!string.IsNullOrEmpty(_settings.OutputDirectory) && Directory.Exists(_settings.OutputDirectory))
            folderBrowserDialog.SelectedPath = _settings.OutputDirectory;
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;

        string source = game.RootPath;
        string destination = FindAvailableDirectory(Path.Combine(folderBrowserDialog.SelectedPath,
            MakeSafeFileName(Path.GetFileNameWithoutExtension(source))));
        string passcode = string.IsNullOrEmpty(_settings.DebugPasscode)
            ? SonyDebugPackageCredentials.DefaultPasscode
            : _settings.DebugPasscode;

        SonyPackageExtractResult? outcome = null;
        EnqueueTask(PackageTaskTypes.PackageExtract, $"Extract {Path.GetFileName(source)}",
            async (progress, token) =>
            {
                outcome = await SonyPackageExtraction.ExtractAsync(source, destination, passcode,
                    AdaptSonyExtractProgress(progress), token).ConfigureAwait(false);
            },
            sourcePath: source, outputPath: destination,
            operation: "Extract package", sourceFormat: "FPKG", targetFormat: "folder",
            stagePlan: PackageTaskPlans.Extract,
            payload: Payload(("source", source), ("output", destination), ("kind", "sony"), ("passcode", passcode)),
            onFinished: task =>
            {
                if (task.Status == PackageTaskStatus.Completed && outcome is not null)
                    statusLabel.Text = $"Extracted {outcome.FileCount:N0} file(s) to {outcome.Destination}.";
                else
                    statusLabel.Text = $"Package extraction {StatusText(task.Status).ToLowerInvariant()}.";
            });
        statusLabel.Text = "Queued: package extraction. See the Tasks tab.";
    }

    // ---------------------------------------------------------------- move to folder

    private enum MoveMode { Title, TitleId, Category, Region, Source, Flat }

    private sealed class MoveOutcome
    {
        public List<(Ps5GameInfo Game, string Source, string Target)> Moved { get; } = [];
        public List<string> Skipped { get; } = [];
    }

    private void menuLibraryMoveTitle_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.Title);
    private void menuLibraryMoveTitleId_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.TitleId);
    private void menuLibraryMoveCategory_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.Category);
    private void menuLibraryMoveRegion_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.Region);
    private void menuLibraryMoveSource_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.Source);
    private void menuLibraryMoveSingle_Click(object? sender, EventArgs e) => MoveGamesToFolder(MoveMode.Flat);

    private static string MoveModeLabel(MoveMode mode) => mode switch
    {
        MoveMode.TitleId => "Title ID",
        MoveMode.Category => "Category",
        MoveMode.Region => "Region",
        MoveMode.Source => "Source",
        MoveMode.Flat => "a single folder",
        _ => "Title"
    };

    private void MoveGamesToFolder(MoveMode mode)
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0)
        {
            AppDialog.ShowInformation("Select one or more items to move.", "Move to folder");
            return;
        }

        folderBrowserDialog.Description = "Select the destination folder";
        if (!string.IsNullOrEmpty(_settings.OutputDirectory) && Directory.Exists(_settings.OutputDirectory))
            folderBrowserDialog.SelectedPath = _settings.OutputDirectory;
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string destinationRoot = folderBrowserDialog.SelectedPath;

        bool addToLibrary = false;
        if (!IsUnderLibrary(destinationRoot) && !IsManualSource(destinationRoot))
            addToLibrary = AppDialog.ShowWarning(
                $"Add this folder to the library so the moved items stay listed?\n\n{destinationRoot}",
                "Move to folder", DarkDialogButton.YesNo) == DialogResult.Yes;

        string modeLabel = MoveModeLabel(mode);
        if (_settings.ConfirmMove && AppDialog.ShowWarning(
                $"Move {games.Count:N0} source(s) into:\n\n{destinationRoot}\n\nGrouped by: {modeLabel}\n\nProceed?",
                "Move to folder", DarkDialogButton.YesNo) != DialogResult.Yes)
            return;

        var snapshot = games.ToList();
        var outcome = new MoveOutcome();
        EnqueueTask(PackageTaskTypes.LibraryMove, $"Move {snapshot.Count:N0} item(s) by {modeLabel}",
            (progress, token) =>
            {
                MoveWorker(snapshot, destinationRoot, mode, outcome, progress, token);
                return Task.CompletedTask;
            },
            sourcePath: string.Empty, outputPath: destinationRoot,
            operation: "Move", sourceFormat: "library", targetFormat: modeLabel,
            stagePlan: PackageTaskPlans.Single,
            onFinished: _ => CompleteMove(outcome, destinationRoot, addToLibrary, modeLabel));
    }

    private void CompleteMove(MoveOutcome outcome, string destinationRoot, bool addToLibrary, string modeLabel)
    {
        foreach ((Ps5GameInfo game, string source, string target) in outcome.Moved)
            RewriteGamePath(game, source, target);
        if (addToLibrary &&
            !_settings.LibraryFolders.Contains(destinationRoot, StringComparer.OrdinalIgnoreCase))
            _settings.LibraryFolders.Add(destinationRoot);
        if (outcome.Moved.Count > 0) SaveSettingsQuietly();
        _stateStore.SaveManifest(_games);
        ApplyFilter();
        Logger.Info($"Move by {modeLabel}: {outcome.Moved.Count:N0} moved, {outcome.Skipped.Count:N0} skipped.");
        statusLabel.Text = $"Moved {outcome.Moved.Count:N0} item(s); skipped {outcome.Skipped.Count:N0}.";
        if (outcome.Moved.Count == 0 && outcome.Skipped.Count > 0)
            AppDialog.ShowWarning("No items were moved. Check the log for details.", "Move to folder");
    }

    private static void MoveWorker(IReadOnlyList<Ps5GameInfo> games, string destinationRoot, MoveMode mode,
        MoveOutcome outcome, IProgress<PackageTaskProgress> progress, CancellationToken token)
    {
        int total = games.Count;
        int index = 0;
        foreach (Ps5GameInfo game in games)
        {
            token.ThrowIfCancellationRequested();
            string source = game.RootPath;
            index++;
            progress.Report(new PackageTaskProgress("Moving", index - 1, total, 0, 0, index, total, game.Title));
            if (string.IsNullOrWhiteSpace(source) || (!Directory.Exists(source) && !File.Exists(source)))
            {
                outcome.Skipped.Add($"{game.Title}: source not found");
                continue;
            }

            string? group = GroupFolder(game, mode);
            if (group is null)
            {
                outcome.Skipped.Add($"{game.Title}: no {MoveModeLabel(mode)} value");
                continue;
            }

            string target = Path.Combine(destinationRoot, group, LibraryFileName(game));
            if (string.Equals(target, source, StringComparison.OrdinalIgnoreCase))
            {
                outcome.Skipped.Add($"{LibraryFileName(game)}: already in the destination");
                continue;
            }
            if (File.Exists(target) || Directory.Exists(target))
            {
                outcome.Skipped.Add($"{LibraryFileName(game)}: destination already exists");
                Logger.Warn($"Move skipped (exists): {target}");
                continue;
            }

            try
            {
                string? parent = Path.GetDirectoryName(target);
                if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
                LibraryFileMover.Move(source, target, token, progress);                outcome.Moved.Add((game, source, target));
                Logger.Info($"Moved {source} -> {target}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                outcome.Skipped.Add($"{LibraryFileName(game)}: {ex.Message}");
                Logger.Warn($"Move failed for '{source}': {ex.Message}");
            }
        }
        progress.Report(new PackageTaskProgress("Moving", total, total, 0, 0, total, total, string.Empty));
    }

    private static string? GroupFolder(Ps5GameInfo game, MoveMode mode)
    {
        switch (mode)
        {
            case MoveMode.Title:
            {
                string category = CategoryOf(game);
                if (category.Equals("DLC", StringComparison.OrdinalIgnoreCase) ||
                    category.Equals("Add-on", StringComparison.OrdinalIgnoreCase))
                    return Path.Combine("Addon", SafeFolder(game.TitleId, "UNKNOWN_TITLEID"));
                if (category.Equals("App", StringComparison.OrdinalIgnoreCase))
                    return Path.Combine("App", SafeFolder(game.Title, "UNKNOWN_TITLEID"));
                if (category.Equals("Game", StringComparison.OrdinalIgnoreCase) ||
                    category.Equals("Patch", StringComparison.OrdinalIgnoreCase))
                    return Path.Combine("Base + Update", SafeFolder(game.Title, "UNKNOWN_TITLEID"));
                return null;
            }
            case MoveMode.TitleId:
                return SafeFolder(game.TitleId, "UNKNOWN_TITLEID");
            case MoveMode.Category:
                return CategoryOf(game) switch
                {
                    "Game" => "Game",
                    "Patch" => "Patch",
                    "DLC" or "Add-on" => "Dlc",
                    "App" => "App",
                    _ => null
                };
            case MoveMode.Region:
                return SafeFolder(RegionOf(game), "Other");
            case MoveMode.Source:
                return SafeFolder(game.SourceDescription, "Other");
            case MoveMode.Flat:
                return string.Empty;
            default:
                return null;
        }
    }

    private static string SafeFolder(string value, string fallback)
    {
        string safe = Ps5RenameFormatter.Sanitize(value);
        return safe.Length > 0 ? safe : Ps5RenameFormatter.Sanitize(fallback);
    }

    private void RewriteGamePath(Ps5GameInfo game, string source, string target)
    {
        game.RootPath = target;
        game.ParamPath = ReplacePathPrefix(game.ParamPath, source, target);
        ReplaceSettingPath(_settings.LibraryFolders, source, target);
        ReplaceSettingPath(_settings.ManualSources, source, target);
        ReplaceSettingPath(_settings.RecentFolders, source, target);
        _detailsCache.Remove(source);
        _libraryThumbnails.TryRemove(source, out _);
        _libraryThumbnailAttempts.TryRemove(source, out _);
    }

    private static string ReplacePathPrefix(string value, string source, string target)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= source.Length) return value;
        if (!value.StartsWith(source, StringComparison.OrdinalIgnoreCase)) return value;
        char next = value[source.Length];
        return next is '\\' or '/' or ':' ? target + value[source.Length..] : value;
    }

    // ---------------------------------------------------------------- delete to recycle bin

    private void menuLibraryDelete_Click(object? sender, EventArgs e) => DeleteSelected();

    private void DeleteSelected()
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0) return;
        bool permanent = _settings.PermanentDelete;
        if (_settings.ConfirmDelete && !ConfirmDelete(games, permanent)) return;

        var recycle = permanent
            ? Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently
            : Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin;

        int deleted = 0;
        int skipped = 0;
        foreach (Ps5GameInfo game in games)
        {
            string path = game.RootPath;
            if (!IsUnderLibrary(path) && !IsManualSource(path)) { skipped++; continue; }

            try
            {
                if (Directory.Exists(path))
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, recycle);
                else if (File.Exists(path))
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                        Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, recycle);
                else { skipped++; continue; }

                RemoveSettingPath(_settings.LibraryFolders, path);
                RemoveSettingPath(_settings.ManualSources, path);
                RemoveSettingPath(_settings.RecentFolders, path);
                _detailsCache.Remove(path);
                _games.Remove(game);
                deleted++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
            {
                Logger.Warn($"Delete failed for '{path}': {ex.Message}");
                skipped++;
            }
        }

        SaveSettingsQuietly();
        _stateStore.SaveManifest(_games);
        ApplyFilter();
        string verb = permanent ? "Permanently deleted" : "Recycled";
        Logger.Info($"{verb} {deleted:N0} source(s); skipped {skipped:N0}.");
        statusLabel.Text = $"{verb} {deleted:N0} source(s); skipped {skipped:N0}.";
    }

    /// <summary>
    /// Confirms a destructive action with a cancellable Yes/No dialog (previously OK-only) and an
    /// accurate verb: a permanent delete is never presented as a Recycle Bin move.
    /// </summary>
    private static bool ConfirmDelete(IReadOnlyList<Ps5GameInfo> games, bool permanent)
    {
        const int maximumListed = 15;
        var listed = new System.Text.StringBuilder();
        foreach (string path in games.Take(maximumListed).Select(game => game.RootPath))
            listed.Append("  ").AppendLine(path);
        if (games.Count > maximumListed)
            listed.AppendLine($"  ... and {games.Count - maximumListed:N0} more");

        string caption = permanent ? "Permanently delete" : "Move to Recycle Bin";
        string question = permanent
            ? $"Permanently delete {games.Count:N0} source(s)? This cannot be undone.\n\n{listed}"
            : $"Send {games.Count:N0} source(s) to the Recycle Bin?\n\n{listed}";
        return AppDialog.ShowWarning(question, caption, DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes;
    }

    private static void RemoveSettingPath(List<string> paths, string value) =>
        paths.RemoveAll(path => string.Equals(path, value, StringComparison.OrdinalIgnoreCase));

    // ---------------------------------------------------------------- group scope

    private void menuLibraryGroupExport_Click(object? sender, EventArgs e)
    {
        IReadOnlyList<Ps5GameInfo> games = GroupContextGames();
        if (games.Count == 0) return;
        ExportGamesCsv(games, "PS5-group.csv");
    }

    private IReadOnlyList<Ps5GameInfo> GroupContextGames() =>
        _contextRowIndex >= 0 && gridLibrary.IsGroupRow(_contextRowIndex)
            ? gridLibrary.GetGroupItems<Ps5GameInfo>(_contextRowIndex).ToList()
            : [];
}
