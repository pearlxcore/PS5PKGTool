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

    // ---------------------------------------------------------------- move into folders

    private void menuLibraryMoveTitle_Click(object? sender, EventArgs e) => MoveSelected(game => game.Title);
    private void menuLibraryMoveTitleId_Click(object? sender, EventArgs e) => MoveSelected(game => game.TitleId);
    private void menuLibraryMoveCategory_Click(object? sender, EventArgs e) => MoveSelected(game => CategoryOf(game));
    private void menuLibraryMoveRegion_Click(object? sender, EventArgs e) => MoveSelected(game => RegionOf(game));

    private void menuLibraryMoveSingle_Click(object? sender, EventArgs e)
    {
        string? folder = PromptText("Move to folder", "Folder name (created next to each source):", "PS5");
        if (!string.IsNullOrWhiteSpace(folder)) MoveSelected(_ => folder);
    }

    private void MoveSelected(Func<Ps5GameInfo, string> folderSelector)
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0) return;
        if (_settings.ConfirmMove && AppDialog.ShowWarning(
                $"Move {games.Count:N0} source(s) into subfolders next to each source?\n\n" +
                string.Join(Environment.NewLine, games.Select(game => "  " + game.RootPath)),
                "Move to folder") != DialogResult.OK)
            return;

        int moved = 0;
        int skipped = 0;
        foreach (Ps5GameInfo game in games)
        {
            string source = game.RootPath;
            bool directory = Directory.Exists(source);
            bool file = File.Exists(source);
            if (!directory && !file) { skipped++; continue; }

            string? parent = Path.GetDirectoryName(source);
            if (string.IsNullOrEmpty(parent)) { skipped++; continue; }

            string sub = MakeSafeFileName(folderSelector(game));
            if (sub.Length == 0) sub = "PS5";
            string target = Path.Combine(Path.Combine(parent, sub), Path.GetFileName(source));
            if (string.Equals(target, source, StringComparison.OrdinalIgnoreCase)) { skipped++; continue; }
            if (File.Exists(target) || Directory.Exists(target)) { skipped++; continue; }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if (directory) Directory.Move(source, target);
                else File.Move(source, target);
                RewriteGamePath(game, source, target);
                moved++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                Logger.Warn($"Move failed for '{source}': {ex.Message}");
                skipped++;
            }
        }

        SaveSettingsQuietly();
        _stateStore.SaveManifest(_games);
        ApplyFilter();
        statusLabel.Text = $"Moved {moved:N0} source(s); skipped {skipped:N0}.";
        if (moved == 0 && skipped > 0)
            AppDialog.ShowWarning("No sources were moved. Check the log for details.", "Move to folder");
    }

    private void RewriteGamePath(Ps5GameInfo game, string source, string target)
    {
        game.RootPath = target;
        if (game.ParamPath.StartsWith(source, StringComparison.OrdinalIgnoreCase))
            game.ParamPath = target + game.ParamPath[source.Length..];
        ReplaceSettingPath(_settings.LibraryFolders, source, target);
        ReplaceSettingPath(_settings.ManualSources, source, target);
        ReplaceSettingPath(_settings.RecentFolders, source, target);
        _detailsCache.Remove(source);
    }

    // ---------------------------------------------------------------- delete to recycle bin

    private void menuLibraryDelete_Click(object? sender, EventArgs e) => DeleteSelected();

    private void DeleteSelected()
    {
        List<Ps5GameInfo> games = SelectedGames().ToList();
        if (games.Count == 0) return;
        if (_settings.ConfirmDelete && AppDialog.ShowWarning(
                $"Send {games.Count:N0} source(s) to the Recycle Bin?\n\n" +
                string.Join(Environment.NewLine, games.Select(game => "  " + game.RootPath)),
                "Delete package") != DialogResult.OK)
            return;

        var recycle = _settings.PermanentDelete
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
        statusLabel.Text = $"Deleted {deleted:N0} source(s); skipped {skipped:N0}.";
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
