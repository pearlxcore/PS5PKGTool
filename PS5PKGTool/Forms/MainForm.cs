using System.Data;
using System.Diagnostics;
using System.Net.Http;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using DarkUI.Controls;
using DarkUI.Config;
using DarkUI.Forms;
using PS5PKGTool.Core.Assets;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;
using PS5PKGTool.Core.Tasks;
using PS5PKGTool.Ffpfsc;
using PS5PKGTool.Infrastructure;
using UFS2Tool;

namespace PS5PKGTool.Forms;

public partial class MainForm : DarkForm
{
    private readonly AppStateStore _stateStore = new();
    private readonly Ps5LibraryScanner _scanner = new();
    private readonly Ps5DetailsLoader _detailsLoader = new();
    private readonly Dictionary<string, Ps5GameDetails> _detailsCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TreeNode> _directoryNodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Ps5FileInfo>> _filesByDirectory = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, long> _directorySizes = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _populatedDetailTabs = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Image> _trophyImages = [];
    private Ps5GameDetails? _currentDetails;
    private Ps5Artwork? _currentArtwork;
    private Ps5UdsSummary? _udsSummary;
    private Ps5SelfInfo? _selfInfo;
    private DataView? _trophyView;
    private int _fileSortColumn;
    private bool _fileSortAscending = true;
    private string _currentDetailsRoot = string.Empty;
    private AppSettings _settings = new();
    private List<Ps5GameInfo> _games = [];
    private List<Ps5GameInfo> _visibleGames = [];
    private CancellationTokenSource? _scanCancellation;
    private CancellationTokenSource? _detailCancellation;
    private CancellationTokenSource? _fileCancellation;
    private CancellationTokenSource? _filePreviewCancellation;
    private Ps5GameInfo? _selectedGame;
    private string _currentGameRoot = string.Empty;
    private readonly string _previewDirectory = Path.Combine(Path.GetTempPath(), "PS5PKGTool", "Preview",
        Environment.ProcessId.ToString());
    private readonly string? _pendingExternalPath;
    private bool _currentSourceIsContainer;
    private bool _isScanning;
    private bool _isFileBusy;
    private int _detailVersion;
    private int _filePreviewVersion;
    private long _hexPreviewOffset;
    private FileBrowserEntry? _filePreviewEntry;
    private Ps5FileInfo? _filePreviewInfo;
    private int HexPreviewPageSizeBytes => Math.Max(1, _settings.HexPageKb) * 1024;

    public MainForm() : this(null)
    {
    }

    public MainForm(string? externalPath)
    {
        InitializeComponent();
        Text = $"PS5 PKG Tool v{AppVersion()}";
        // The WinForms designer cannot serialize the hosted WPF MediaElement, so re-saving the form
        // drops it. Recreate it here when that happens.
        if (mediaFileViewer is null)
        {
            mediaFileViewer = new System.Windows.Controls.MediaElement();
            mediaFileHost.Child = mediaFileViewer;
        }
        // MediaElement must be Manual to allow imperative Play/Pause/Stop.
        mediaFileViewer.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
        mediaFileViewer.UnloadedBehavior = System.Windows.Controls.MediaState.Close;
        FileIconProvider.Populate(imageListFiles);
        InitializeTaskQueue();
        InitializeLibraryTools();
        InitializeLog();
        RefreshImageTools();
        _pendingExternalPath = externalPath;
    }

    private static string AppVersion()
    {
        Version? version = typeof(MainForm).Assembly.GetName().Version;
        if (version is null) return "1.0.0";
        return version.Build >= 0
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : $"{version.Major}.{version.Minor}";
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _settings = _stateStore.LoadSettings();
        _settings.ManualSources.RemoveAll(path => !File.Exists(path) && !Directory.Exists(path));
        chkLogAutoScroll.Checked = _settings.LogAutoScroll;
        RestoreWindowBounds();
        ApplyRuntimeSettings();
        ApplyDefaultGrouping();
        SyncFilterControls();
        RebuildRecentMenu();

        var configured = new List<Ps5GameInfo>();
        foreach (Ps5GameInfo game in _stateStore.LoadManifest().Games)
        {
            if (string.IsNullOrWhiteSpace(game.RootPath)) continue;
            if (game.SourceKind == Ps5SourceKind.FilesystemImage &&
                !Path.GetExtension(game.RootPath).Equals(".exfat", StringComparison.OrdinalIgnoreCase)) continue;
            if (game.SourceKind == Ps5SourceKind.Ffpkg &&
                !Path.GetExtension(game.RootPath).Equals(".ffpkg", StringComparison.OrdinalIgnoreCase)) continue;
            if (IsUnderLibrary(game.RootPath) || IsManualSource(game.RootPath)) configured.Add(game);
        }
        _games = configured.OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase).ToList();
        _stateStore.SaveManifest(_games);
        Logger.Info($"Loaded cached library: {_games.Count:N0} game(s).");
        ApplyFilter();
        int missing = _games.Count(game => !SourceExists(game));
        if (_games.Count == 0)
            statusLabel.Text = "Add a PS5 dump, PKG, FFPFSC, FFPKG, or exFAT library folder, then choose Refresh.";
        else if (missing > 0)
            statusLabel.Text = $"Loaded cached library. {missing:N0} item(s) not found; choose Refresh to rescan.";
        else
            statusLabel.Text = "Loaded cached library. Choose Refresh to rescan folders.";

        if (!string.IsNullOrWhiteSpace(_pendingExternalPath))
        {
            string path = _pendingExternalPath;
            if (File.Exists(path) || Directory.Exists(path))
            {
                Logger.Info($"Opening external path: {path}");
                AddRecentFolder(path);
                RememberManualSource(path);
                await ScanAsync([path], merge: true);
            }
        }
        else if (_settings.RefreshOnStartup && ScanRoots().Count > 0)
        {
            await ScanAsync(ScanRoots(), merge: false);
        }

        int missingNow = _games.Count(game => !SourceExists(game));
        if (missingNow > 0)
            BeginInvoke(new Action(() => NotifyMissingSources(missingNow)));
    }

    private void NotifyMissingSources(int missing)
    {
        string[] names = _games
            .Where(game => !SourceExists(game))
            .Select(game => string.IsNullOrWhiteSpace(game.Title) ? Path.GetFileName(game.RootPath) : game.Title)
            .Take(8)
            .ToArray();
        var message = new StringBuilder();
        message.Append(missing == 1
            ? "1 library item could not be found (deleted, moved, or its drive is offline):"
            : $"{missing:N0} library items could not be found (deleted, moved, or their drive is offline):");
        message.AppendLine();
        message.AppendLine();
        foreach (string name in names) message.AppendLine("  - " + name);
        if (missing > names.Length)
            message.AppendLine($"  ... and {missing - names.Length:N0} more");
        message.AppendLine();
        message.Append("They are marked as Missing until the next Refresh. Use File > Remove Missing Items to drop them now.");
        AppMessageBox.Show(this, "Missing library items", message.ToString(), AppMessageType.Warning, AppMessageButtons.OK);
    }

    private void ApplyRuntimeSettings()
    {
        Theme theme = ThemeManager.Presets.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, _settings.Theme, StringComparison.OrdinalIgnoreCase)) ?? ThemeManager.BuiltIn.Default;
        ThemeManager.Apply(theme);

        gridLibrary.RowTemplate.Height = Math.Max(16, _settings.GridRowHeight);
        gridLibrary.CellBorderStyle = _settings.ShowGridLines
            ? DataGridViewCellBorderStyle.Single
            : DataGridViewCellBorderStyle.None;
    }

    private void ApplyDefaultGrouping()
    {
        _libraryGroupBy = _settings.DefaultGroupBy;
        foreach ((ToolStripMenuItem item, string key) in _groupItems)
            item.Checked = string.Equals(key, _libraryGroupBy, StringComparison.Ordinal);
    }

    private void RestoreWindowBounds()
    {
        if (_settings.WindowWidth < 800 || _settings.WindowHeight < 600) return;
        var bounds = new Rectangle(0, 0, _settings.WindowWidth, _settings.WindowHeight);
        bool visible = Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(bounds));
        if (!visible) return;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(_settings.WindowWidth, _settings.WindowHeight);
        if (_settings.WindowMaximized) WindowState = FormWindowState.Maximized;
    }

    private void SaveWindowBounds()
    {
        Rectangle bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        if (bounds.Width >= 800 && bounds.Height >= 600)
        {
            _settings.WindowWidth = bounds.Width;
            _settings.WindowHeight = bounds.Height;
        }
        _settings.WindowMaximized = WindowState == FormWindowState.Maximized;
        SaveSettingsQuietly();
    }

    private async void AddFolder_Click(object? sender, EventArgs e)
    {
        folderBrowserDialog.Description = "Select a PS5 library folder";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string selected = folderBrowserDialog.SelectedPath;
        if (!_settings.LibraryFolders.Contains(selected, StringComparer.OrdinalIgnoreCase))
        {
            _settings.LibraryFolders.Add(selected);
            _stateStore.SaveSettings(_settings);
        }
        AddRecentFolder(selected);
        await ScanAsync(ScanRoots(), merge: false);
    }

    private async void OpenDump_Click(object? sender, EventArgs e)
    {
        folderBrowserDialog.Description = "Select the root of an unpacked PS5 game dump";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        AddRecentFolder(folderBrowserDialog.SelectedPath);
        RememberManualSource(folderBrowserDialog.SelectedPath);
        await ScanAsync([folderBrowserDialog.SelectedPath], merge: true);
    }

    private async void OpenPackage_Click(object? sender, EventArgs e)
    {
        if (packageOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        AddRecentFolder(packageOpenDialog.FileName);
        RememberManualSource(packageOpenDialog.FileName);
        await ScanAsync([packageOpenDialog.FileName], merge: true);
    }

    private async void Refresh_Click(object? sender, EventArgs e)
    {
        IReadOnlyList<string> roots = ScanRoots();
        Logger.Info($"Refresh requested for {roots.Count} root(s): {string.Join("; ", roots)}");
        if (roots.Count == 0)
        {
            AppDialog.ShowInformation("Add at least one PS5 dump library folder in Settings.", "PS5 PKG Tool");
            return;
        }
        await ScanAsync(roots, merge: false);
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            e.Handled = true;
            Refresh_Click(this, EventArgs.Empty);
        }
        else if (e.Control && e.KeyCode == Keys.F)
        {
            e.Handled = true;
            searchLibrary.Focus();
        }
        else if (e.KeyCode == Keys.Escape && searchLibrary.ContainsFocus)
        {
            e.Handled = true;
            searchLibrary.SearchText = string.Empty;
        }
        else if (e.KeyCode == Keys.Escape && _isFileBusy)
        {
            e.Handled = true;
            _fileCancellation?.Cancel();
            statusLabel.Text = "Cancelling extraction...";
        }
    }

    private async Task ScanAsync(IEnumerable<string> folders, bool merge)
    {
        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
        _scanCancellation = new CancellationTokenSource();
        SetScanning(true);
        var progress = new Progress<Ps5ScanProgress>(value =>
        {
            statusLabel.Text = value.Total == 0
                ? "Searching for PS5 dumps, packages, and filesystem images..."
                : $"Reading {value.Processed + 1:N0} of {value.Total:N0}: {Path.GetFileName(value.CurrentPath)}";
        });

        try
        {
            Ps5ScanResult result = await _scanner.ScanAsync(folders, _settings.RecursiveScan, _games, progress, _scanCancellation.Token);
            Logger.Info($"Scan finished: {result.Games.Count} game(s), {result.Errors.Count} warning(s).");
            if (merge)
            {
                var combined = _games.Concat(result.Games)
                    .GroupBy(game => game.RootPath, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.Last())
                    .OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase);
                _games = combined.ToList();
            }
            else
            {
                _games = result.Games;
                _detailsCache.Clear();
            }
            _stateStore.SaveManifest(_games);
            ApplyFilter();
            statusLabel.Text = result.Errors.Count == 0
                ? $"Library refresh complete: {_games.Count:N0} game(s)."
                : $"Refresh complete with {result.Errors.Count:N0} warning(s).";
            if (result.Errors.Count > 0)
                AppDialog.ShowWarning(string.Join(Environment.NewLine, result.Errors.Take(12)), "PS5 scan warnings");
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Library refresh cancelled.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            statusLabel.Text = "Library refresh failed.";
            AppDialog.ShowError(ex.Message, "PS5 library refresh");
        }
        finally
        {
            SetScanning(false);
        }
    }

    private void SetScanning(bool scanning)
    {
        _isScanning = scanning;
        UpdateOperationState();
    }

    private void UpdateOperationState()
    {
        bool idle = !_isScanning && !_isFileBusy;
        menuAddFolder.Enabled = idle;
        menuOpenDump.Enabled = idle;
        menuOpenPackage.Enabled = idle;
        menuRefresh.Enabled = idle;
        menuSettings.Enabled = idle;
        btnImageRun.Enabled = idle && cboImageAction.SelectedItem is not null;
        btnImageCancel.Enabled = _taskQueue.Tasks.Any(task => task.Status is PackageTaskStatus.Running or PackageTaskStatus.Queued);

        btnFileCancel.Enabled = _isFileBusy;
    }

    private void btnFileCancel_Click(object? sender, EventArgs e)
    {
        if (!_isFileBusy) return;
        _fileCancellation?.Cancel();
        statusLabel.Text = "Cancelling extraction...";
    }

    private void SetSelectedGame(Ps5GameInfo? game)
    {
        _selectedGame = game;
        UpdateOperationState();
    }


    private static string MakeSafeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string result = new(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        result = result.Trim(' ', '.');
        return result.Length == 0 ? "PS5_GAME" : result;
    }

    private static bool IsFfpfscOperationException(Exception ex) =>
        ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or
            InvalidOperationException or NotSupportedException or OverflowException;

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }


    private static string FindAvailableDirectory(string preferredPath)
    {
        string fullPath = Path.GetFullPath(preferredPath);
        if (!Directory.Exists(fullPath) && !File.Exists(fullPath)) return fullPath;
        for (int index = 2; index < 10_000; index++)
        {
            string candidate = fullPath + $" ({index})";
            if (!Directory.Exists(candidate) && !File.Exists(candidate)) return candidate;
        }
        throw new IOException("Unable to find an available extraction directory name.");
    }

    private async void Settings_Click(object? sender, EventArgs e)
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog(this) != DialogResult.OK) return;

        bool libraryChanged = !_settings.LibraryFolders.SequenceEqual(form.Settings.LibraryFolders, StringComparer.OrdinalIgnoreCase)
                              || _settings.RecursiveScan != form.Settings.RecursiveScan;
        _settings = form.Settings;
        SaveSettingsQuietly();
        ApplyRuntimeSettings();
        SyncFilterControls();
        if (form.ResetLayout) ResetLibraryColumnLayout();
        if (form.ClearCaches) ClearRuntimeCaches();

        if (!libraryChanged)
        {
            statusLabel.Text = "Settings saved.";
            return;
        }
        if (ScanRoots().Count == 0)
        {
            _games = [];
            _stateStore.SaveManifest(_games);
            _detailsCache.Clear();
            ApplyFilter();
            statusLabel.Text = "All library folders were removed.";
            return;
        }
        statusLabel.Text = "Settings saved. Rescanning configured folders...";
        await ScanAsync(ScanRoots(), merge: false);
    }

    private void ResetLibraryColumnLayout()
    {
        _settings.LibraryColumnOrder = [];
        _settings.LibraryHiddenColumns = [];
        if (_libraryColumnsReady)
        {
            _libraryColumnsReady = false;
            EnsureLibraryColumns();
            ApplyFilter();
        }
        statusLabel.Text = "Column layout reset.";
    }

    private void ClearRuntimeCaches()
    {
        _detailsCache.Clear();
        _libraryThumbnails.Clear();
        _libraryThumbnailAttempts.Clear();
        statusLabel.Text = "Caches cleared.";
    }

    private void Exit_Click(object? sender, EventArgs e) => Close();

    private void About_Click(object? sender, EventArgs e)
    {
        using var form = new AboutForm(AppVersion());
        form.ShowDialog(this);
    }

    private const string RepositoryUrl = "https://github.com/pearlxcore/PS5PkgTool";
    private const string LatestReleaseApiUrl = "https://api.github.com/repos/pearlxcore/PS5PkgTool/releases/latest";
    private const string KoFiUrl = "https://ko-fi.com/R6R524N7X";
    private const string PayPalUrl = "https://www.paypal.com/paypalme/pearlxcoree";

    private static void OpenExternalUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException or InvalidOperationException)
        {
            AppDialog.ShowError(ex.Message, "Open link");
        }
    }

    private void menuHelpKofi_Click(object? sender, EventArgs e) => OpenExternalUrl(KoFiUrl);

    private void menuHelpPayPal_Click(object? sender, EventArgs e) => OpenExternalUrl(PayPalUrl);

    private async void menuHelpCheckUpdate_Click(object? sender, EventArgs e)
    {
        try
        {
            statusLabel.Text = "Checking for updates...";
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("PS5PKGTool");
            string json = await client.GetStringAsync(LatestReleaseApiUrl);
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            string tag = document.RootElement.TryGetProperty("tag_name", out System.Text.Json.JsonElement tagElement)
                ? tagElement.GetString() ?? string.Empty
                : string.Empty;
            string page = document.RootElement.TryGetProperty("html_url", out System.Text.Json.JsonElement urlElement)
                ? urlElement.GetString() ?? RepositoryUrl
                : RepositoryUrl;
            string current = AppVersion();
            string latest = tag.TrimStart('v', 'V').Trim();
            if (latest.Length == 0)
            {
                AppDialog.ShowInformation("The latest release has no version tag.", "Check for updates");
                return;
            }
            int comparison = CompareVersions(current, latest) ?? 0;
            if (comparison < 0)
            {
                if (AppDialog.ShowInformation(
                        $"A newer version is available.\n\nInstalled: {current}\nLatest: {latest}\n\nOpen the download page?",
                        "Check for updates", DarkDialogButton.YesNo) == DialogResult.Yes)
                    OpenExternalUrl(page);
            }
            else
            {
                AppDialog.ShowInformation("PS5 PKG Tool is up to date.", "Check for updates");
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            AppDialog.ShowInformation("PS5 PKG Tool is up to date.", "Check for updates");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            AppDialog.ShowError("Could not check for updates.\n\n" + ex.Message, "Check for updates");
        }
        finally
        {
            statusLabel.Text = "Ready";
        }
    }


    private void ApplyFilter()
    {
        string query = searchLibrary.SearchText.Trim();
        _visibleGames = _games
            .Where(MatchesFilters)
            .Where(game => MatchesQuery(game, query))
            .ToList();

        PopulateLibraryGrid();

        statusCount.Text = $"{_visibleGames.Count:N0} of {_games.Count:N0} games";
        if (lblFilterEmpty is not null)
        {
            lblFilterEmpty.Visible = _visibleGames.Count == 0 && _games.Count > 0;
            if (lblFilterEmpty.Visible) lblFilterEmpty.BringToFront();
        }
        UpdateFilterBadges();
        RebuildFilterChips();
        if (_visibleGames.Count == 0)
        {
            SetSelectedGame(null);
            ClearDetails();
        }
    }

    private async void gridLibrary_SelectionChanged(object? sender, EventArgs e)
    {
        if (_suppressLibrarySelection) return;
        if (SelectedGame() is not { } game) return;
        try
        {
            SetSelectedGame(game);
            RefreshImageTools();
            await ShowGameAsync(game);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Unable to open '{game.RootPath}': {ex.Message}");
        }
    }

    private async Task ShowGameAsync(Ps5GameInfo game)
    {
        PopulateOverview(game);
        txtRawMetadata.Text = PrettyJson(game.RawParamJson);
        ClearDeepDetails();
        statusLabel.Text = $"Loading details for {game.Title}...";
        long sizeBefore = game.SourceSize;

        _detailCancellation?.Cancel();
        _detailCancellation?.Dispose();
        _detailCancellation = new CancellationTokenSource();
        int version = Interlocked.Increment(ref _detailVersion);
        try
        {
            if (!_detailsCache.TryGetValue(game.RootPath, out Ps5GameDetails? details))
            {
                // Artwork is reported progressively: the icon first (cheap PNG), then the DDS
                // backgrounds. Show each snapshot as it arrives so the Artwork tab fills in fast.
                var artworkProgress = new Progress<Ps5Artwork>(snapshot =>
                {
                    if (version != _detailVersion) return;
                    _currentArtwork = snapshot;
                    if (tabsDetails.SelectedTab == tabArtwork) ApplyArtwork(snapshot);
                });
                details = await _detailsLoader.LoadAsync(game, _detailCancellation.Token, artworkProgress);
                _detailsCache[game.RootPath] = details;
                if (_currentArtwork is null)
                    _currentArtwork = new Ps5Artwork(details.Icon, details.Background, details.Background1, details.Background2);
            }
            if (version != _detailVersion || _detailCancellation.IsCancellationRequested) return;
            _currentDetails = details;
            _currentDetailsRoot = game.RootPath;
            if (_currentArtwork is null)
                _currentArtwork = new Ps5Artwork(details.Icon, details.Background, details.Background1, details.Background2);
            _populatedDetailTabs.Clear();
            PopulateActiveDetailTab();
            // A loose dump's size is filled in during the details walk; refresh the overview, the
            // library row and persist it so the next start shows it without re-walking.
            if (game.SourceSize != sizeBefore)
            {
                PopulateOverview(game);
                RefreshLibrarySizeCell(game);
                _stateStore.SaveManifest(_games);
            }
            statusLabel.Text = details.Errors.Count == 0
                ? $"Loaded {game.Title}."
                : $"Loaded {game.Title} with {details.Errors.Count} detail warning(s).";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (version != _detailVersion) return;
            statusLabel.Text = "Unable to load game details.";
            Logger.Warn($"Unable to read '{game.RootPath}': {ex.Message}");
        }
    }

    /// <summary>Runs a detail-load task and logs any failure instead of surfacing a dialog.</summary>
    private static async Task RunSilentlyAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.Warn($"Suppressed read error: {ex.Message}");
        }
    }

    private void btnCopyRawJson_Click(object? sender, EventArgs e)
    {
        if (txtRawMetadata.Text.Length == 0)
        {
            AppDialog.ShowInformation("There is no param.json to copy.", "Raw param.json");
            return;
        }
        CopyText(txtRawMetadata.Text);
    }

    private void PopulateOverview(Ps5GameInfo game)
    {
        var table = new DataTable();
        table.Columns.Add("Property");
        table.Columns.Add("Value");
        void Add(string property, object? value)
        {
            string text = value?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(text)) table.Rows.Add(property, text);
        }

        Add("Title", game.Title);
        Add("Source", game.SourceDescription);
        if (game.SourceSize > 0) Add("Source Size", FormatBytes(game.SourceSize));
        Add("Title ID", game.TitleId);
        Add("Content ID", game.ContentId);
        Add("Concept ID", game.ConceptId);
        Add("Content Version", game.ContentVersion);
        Add("Master Version", game.MasterVersion);
        Add("Target Content Version", game.TargetContentVersion);
        Add("Origin Content Version", game.OriginContentVersion);
        Add("Required System Software", game.RequiredSystemSoftware);
        Add("SDK Version", game.SdkVersion);
        Add("Category", game.ApplicationCategory);
        Add("DRM Type", game.DrmType);
        Add("Content Badge", game.ContentBadgeType);
        Add("Default Language", game.DefaultLanguage);
        Add("Localized Titles", string.Join(" | ", game.LocalizedTitles.Select(pair => $"{pair.Key}: {pair.Value}")));
        Add("Declared Features", string.Join(", ", game.DeclaredFeatures));
        Add("Game Intents", string.Join(", ", game.GameIntents));
        Add("Shared Add-on IDs", string.Join(", ", game.SharedAddOnServiceIds));
        Add("Feature Attributes", $"0x{game.Attribute:X8} / 0x{game.Attribute2:X8} / 0x{game.Attribute3:X8} / 0x{game.Attribute4:X8}");
        Add("Download Data Size", FormatBytes(game.DownloadDataSize));
        if (game.FlexibleMemorySize.HasValue) Add("Flexible Memory", FormatBytes(game.FlexibleMemorySize.Value));
        Add("Creation Date", game.CreationDate);
        Add("Publishing Tool", game.ToolVersion);
        Add("Version URI", game.VersionFileUri);
        Add("Age Levels", string.Join(", ", game.AgeLevels.Select(pair => $"{pair.Key}: {pair.Value}")));
        Add("Location", game.RootPath);
        if (game.SourceKind is Ps5SourceKind.Ffpfsc or Ps5SourceKind.FilesystemImage or Ps5SourceKind.Ffpkg)
        {
            Add(game.SourceKind == Ps5SourceKind.Ffpfsc ? "Inner Filesystem Image" : "Filesystem Image",
                game.ContainerInnerFileName);
            bool isUfs2 = game.SourceKind == Ps5SourceKind.Ffpkg ||
                          Path.GetExtension(game.ContainerInnerFileName).Equals(".ffpkg", StringComparison.OrdinalIgnoreCase);
            Add(isUfs2 ? "Logical UFS2 Size" : "Logical exFAT Size",
                FormatBytes(game.ContainerLogicalSize));
            if (game.SourceKind == Ps5SourceKind.Ffpfsc)
            {
                Add("Stored PFSC Size", FormatBytes(game.ContainerStoredSize));
                Add("PFSC Blocks", game.ContainerBlockCount.ToString("N0"));
            }
            if (!string.IsNullOrWhiteSpace(game.VirtualRoot)) Add("Game Root in Image", game.VirtualRoot);
        }
        if (game.Package is SonyPkgSummary package)
        {
            Add("Package Type", package.KindDisplayName);
            Add("Package Size", FormatBytes(package.FileSize));
            Add("CNT Offset", $"0x{package.EmbeddedCntOffset:X}");
            Add("CNT Entries", $"{package.Entries.Count:N0} ({package.EncryptedEntryCount:N0} encrypted)");
            Add("Embedded CNT", package.Entries.Count > 0
                ? "Present"
                : package.Kind == SonyPkgKind.FinalizedRetail
                    ? "Not present in this retail FIH"
                    : "Empty");
            Add("CNT Header Flags", $"0x{package.HeaderFlags:X8}");
            Add("CNT Content Type", $"0x{package.ContentType:X8}");
            Add("CNT Content Flags", $"0x{package.ContentFlags:X8}");
            Add("CNT DRM Type", $"0x{package.DrmType:X8}");
            if (package.SignedByte.HasValue) Add("FIH Signed Byte", $"0x{package.SignedByte:X2}");
            if (package.FormatVersion.HasValue) Add("FIH Format Version", package.FormatVersion.Value);
            if (package.PfsImageSize > 0)
            {
                Add("PFS Image Offset", $"0x{package.PfsImageOffset:X}");
                Add("PFS Image Size", package.PfsImageSize <= long.MaxValue
                    ? FormatBytes((long)package.PfsImageSize)
                    : $"0x{package.PfsImageSize:X} bytes");
                if (package.NestedPfs is { } nested)
                {
                    Add("PFS Access", nested.AccessState == SonyPfsAccessState.EncryptedKeyRequired
                        ? "Retail image key required"
                        : nested.AccessState.ToString());
                    if (nested.BlockSize > 0) Add("PFS Block Size", FormatBytes(nested.BlockSize));
                    if (nested.InodeCount > 0) Add("PFS Inodes", nested.InodeCount.ToString("N0"));
                    if (nested.Files.Count > 0) Add("PFS Files", nested.Files.Count.ToString("N0"));
                    Add("PFS Status", nested.StatusMessage);
                }
            }
        }
        foreach (string warning in game.DataWarnings) Add("Metadata Warning", warning);
        gridOverview.DataSource = table;
        // The "Property" column keeps a fixed width; "Value" takes the rest.
        if (gridOverview.Columns.Count >= 2)
        {
            gridOverview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            gridOverview.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            gridOverview.Columns[0].Width = 220;
            gridOverview.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
        btnOverviewCopySelected.Enabled = false;
    }

    private void gridOverview_SelectionChanged(object? sender, EventArgs e) =>
        btnOverviewCopySelected.Enabled = gridOverview.SelectedRows.Count > 0;

    private void btnOverviewCopyAll_Click(object? sender, EventArgs e)
    {
        if (gridOverview.DataSource is not DataTable table || table.Rows.Count == 0)
        {
            AppDialog.ShowInformation("There is no overview information to copy.", "Overview");
            return;
        }

        var builder = new StringBuilder();
        foreach (DataRow row in table.Rows)
        {
            string property = row[0]?.ToString() ?? string.Empty;
            if (property.Length == 0) continue;
            builder.Append(property).Append(": ").Append(row[1]?.ToString() ?? string.Empty).AppendLine();
        }
        CopyText(builder.ToString().TrimEnd());
    }

    private void btnOverviewCopySelected_Click(object? sender, EventArgs e)
    {
        if (gridOverview.SelectedRows.Count == 0 ||
            gridOverview.SelectedRows[0].DataBoundItem is not DataRowView view) return;
        string property = view.Row[0]?.ToString() ?? string.Empty;
        string value = view.Row[1]?.ToString() ?? string.Empty;
        CopyText($"{property}: {value}");
    }

    private void tabsDetails_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            if (tabsDetails.SelectedTab == tabFiles) BalanceFilePanes();
            PopulateActiveDetailTab();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Unable to populate detail tab: {ex.Message}");
        }
    }

    private void splitFileBrowser_SizeChanged(object? sender, EventArgs e) => BalanceFilePanes();

    /// <summary>
    /// Divides the Files tab into three equal-width panes: the folder tree, the file list, and the
    /// file viewer. Re-applied whenever the split is resized so the panes stay balanced.
    /// </summary>
    private void BalanceFilePanes()
    {
        // Three equal panes: the outer split gives 1:2, the inner split halves the 2.
        // DarkSplitContainer.SetPanelSize clamps each weight to its 40px minimum, so the
        // weights must stay above 40 or the outer 1:2 collapses to 1:1.
        splitFileBrowser.SetPanelSize(0, 100);
        splitFileBrowser.SetPanelSize(1, 200);
        splitFileContentPreview.SetPanelSize(0, 100);
        splitFileContentPreview.SetPanelSize(1, 100);
    }

    private void PopulateActiveDetailTab()
    {
        if (_currentDetails is null || _selectedGame is null) return;
        Ps5GameDetails details = _currentDetails;
        string key = _currentDetailsRoot;

        if (tabsDetails.SelectedTab == tabArtwork)
        {
            ApplyArtwork(_currentArtwork ??
                new Ps5Artwork(details.Icon, details.Background, details.Background1, details.Background2));
        }
        else if (tabsDetails.SelectedTab == tabTrophies)
        {
            if (!_populatedDetailTabs.Add("trophies|" + key)) return;
            PopulateTrophies(details.TrophySet);
        }
        else if (tabsDetails.SelectedTab == tabActivities)
        {
            if (!_populatedDetailTabs.Add("activities|" + key)) return;
            PopulateActivities(details.Uds);
        }
        else if (tabsDetails.SelectedTab == tabFiles)
        {
            if (_populatedDetailTabs.Add("files|" + key))
                PopulateFiles(_selectedGame, details.Files);
            else
                RefreshFileList();
        }
        else if (tabsDetails.SelectedTab == tabExecutable)
        {
            if (!_populatedDetailTabs.Add("executable|" + key)) return;
            PopulateExecutable(details.Executable);
            if (details.Errors.Count > 0)
                lblExecutableSummary.Text += "  Warnings: " + string.Join(" | ", details.Errors);
        }
        else if (tabsDetails.SelectedTab == tabPackage)
        {
            if (!_populatedDetailTabs.Add("package|" + key)) return;
            _ = RunSilentlyAsync(() => PopulatePackageAsync(_selectedGame, details));
        }
    }

    private async Task PopulatePackageAsync(Ps5GameInfo game, Ps5GameDetails details)
    {
        if (!SourceExists(game))
        {
            gridPkgHeader.DataSource = null;
            gridPkgSegments.DataSource = null;
            gridPkgEntries.DataSource = null;
            gridParamSfo.DataSource = null;
            gridKeystone.DataSource = null;
            gridSi.DataSource = null;
            gridPlayGoChunks.DataSource = null;
            gridPlayGoScenarios.DataSource = null;
            gridPlayGoFiles.DataSource = null;
            lblPlayGoSummary.Text = "The selected source was not found.";
            Logger.Warn($"Package tab skipped: source not found '{game.RootPath}'.");
            return;
        }

        SonyPkgSummary? package = game.Package;

        var header = new DataTable();
        header.Columns.Add("Property");
        header.Columns.Add("Value");
        void Add(string property, string value)
        {
            if (!string.IsNullOrEmpty(value)) header.Rows.Add(property, value);
        }
        Add("Kind", package?.KindDisplayName ?? "PS5 source");
        Add("Location", game.RootPath);
        if (package is not null)
        {
            Add("File size", FormatBytes(package.FileSize));
            Add("Signed byte", package.SignedByte.HasValue ? $"0x{package.SignedByte:X2}" : string.Empty);
            Add("Format version", package.FormatVersion?.ToString() ?? string.Empty);
            Add("PFS offset", $"0x{package.PfsImageOffset:X}");
            Add("PFS size", FormatBytes((long)package.PfsImageSize));
            Add("PFS superblock offset", $"0x{package.PfsSuperblockOffset:X}");
            Add("Embedded CNT offset", $"0x{package.EmbeddedCntOffset:X}");
            Add("Header flags", $"0x{package.HeaderFlags:X8}");
            Add("System entry count", package.SystemEntryCount.ToString());
            Add("Body offset", $"0x{package.BodyOffset:X}");
            Add("Body size", FormatBytes((long)package.BodySize));
            Add("Content ID", package.ContentId);
            Add("DRM type", $"0x{package.DrmType:X8}");
            Add("Content type", ContentTypeText(package.ContentType));
            Add("Content flags", $"0x{package.ContentFlags:X8}");
            Add("CNT entries", package.Entries.Count.ToString("N0"));
            Add("Encrypted entries", package.EncryptedEntryCount.ToString("N0"));
            if (package.NestedPfs is { } pfs)
            {
                Add("PFS access", pfs.AccessState.ToString());
                if (pfs.BlockSize > 0) Add("PFS block size", FormatBytes(pfs.BlockSize));
                if (pfs.InodeCount > 0) Add("PFS inodes", pfs.InodeCount.ToString("N0"));
                Add("PFS status", pfs.StatusMessage);
            }
        }
        gridPkgHeader.DataSource = header;

        var segments = new DataTable();
        segments.Columns.Add("Name");
        segments.Columns.Add("Offset");
        segments.Columns.Add("Size");
        segments.Columns.Add("Bytes", typeof(long));
        segments.Columns.Add("Note");
        if (package is not null)
            foreach (SonyPkgSegment segment in package.Segments)
                segments.Rows.Add(SegmentName(segment.Name), $"0x{segment.Offset:X}", FormatBytes(segment.Size),
                    segment.Size, SegmentNote(segment.Name));
        gridPkgSegments.DataSource = segments;

        var entries = new DataTable();
        entries.Columns.Add("Id");
        entries.Columns.Add("Name");
        entries.Columns.Add("Offset (CNT)");
        entries.Columns.Add("Size");
        entries.Columns.Add("Stored");
        entries.Columns.Add("Encrypted", typeof(bool));
        entries.Columns.Add("Key", typeof(int));
        if (package is not null)
            foreach (SonyPkgEntry entry in package.Entries.OrderBy(item => item.Id))
                entries.Rows.Add($"0x{entry.Id:X4}", entry.DisplayName, $"0x{entry.DataOffset:X}",
                    FormatBytes(entry.DataSize), FormatBytes(entry.StoredSize), entry.IsEncrypted, entry.KeyIndex);
        gridPkgEntries.DataSource = entries;

        byte[] sfoBytes = await ReadParamSfoAsync(game, package);
        var sfo = new DataTable();
        sfo.Columns.Add("Key");
        sfo.Columns.Add("Format");
        sfo.Columns.Add("Value");
        foreach (Ps5SfoEntry entry in Ps5SfoReader.Read(sfoBytes))
            sfo.Rows.Add(entry.Key, entry.Format, entry.Value);
        gridParamSfo.DataSource = sfo;

        var keystone = new DataTable();
        keystone.Columns.Add("File");
        keystone.Columns.Add("Status");
        keystone.Columns.Add("Size");
        keystone.Columns.Add("Leading bytes");
        await AddExtraFileRowAsync(game, keystone, "sce_sys/keystone");
        await AddExtraFileRowAsync(game, keystone, "sce_sys/nptitle.dat");
        await AddExtraFileRowAsync(game, keystone, "sce_sys/about/right.sprx");
        gridKeystone.DataSource = keystone;

        var si = new DataTable();
        si.Columns.Add("Member");
        si.Columns.Add("Size");
        si.Columns.Add("Bytes", typeof(long));
        SonyPkgSegment? siSegment = package?.Segments.FirstOrDefault(segment =>
            segment.Name.Equals("SI", StringComparison.OrdinalIgnoreCase));
        if (siSegment is not null)
            foreach (Ps5SiMember member in Ps5SiReader.List(game.RootPath, siSegment.Offset, siSegment.Size))
                si.Rows.Add(member.Name, FormatBytes(member.Size), member.Size);
        gridSi.DataSource = si;

        await PopulatePlayGoAsync(game, details, siSegment);
    }

    private async Task PopulatePlayGoAsync(Ps5GameInfo game, Ps5GameDetails details, SonyPkgSegment? siSegment)
    {
        var chunks = new DataTable();
        chunks.Columns.Add("Chunk", typeof(int));
        chunks.Columns.Add("Label");
        chunks.Columns.Add("Extents", typeof(int));
        chunks.Columns.Add("Language mask");
        chunks.Columns.Add("Size");
        chunks.Columns.Add("Bytes", typeof(long));

        var scenarios = new DataTable();
        scenarios.Columns.Add("Scenario", typeof(int));
        scenarios.Columns.Add("Label");
        scenarios.Columns.Add("Initial", typeof(int));
        scenarios.Columns.Add("Chunks", typeof(int));
        scenarios.Columns.Add("Sequence");

        var files = new DataTable();
        files.Columns.Add("Path");
        files.Columns.Add("Chunk", typeof(int));
        files.Columns.Add("Path hash");

        byte[]? plgx = null;
        if (siSegment is not null)
            plgx = await Task.Run(() => Ps5SiReader.ReadMember(
                game.RootPath, siSegment.Offset, siSegment.Size, "playgo-chunk.dat"));
        byte[] hashTable = await ReadGameFileAsync(game, "sce_sys/playgo-hash-table.dat");
        byte[] ficm = await ReadGameFileAsync(game, "sce_sys/playgo-ficm.dat");
        string[] relativePaths = details.Files.Files.Select(file => file.RelativePath).ToArray();
        Ps5PlayGoSummary playGo = await Task.Run(() =>
        {
            IReadOnlyDictionary<ulong, string> pathMap = Ps5PlayGoReader.BuildPathMap(relativePaths);
            return Ps5PlayGoReader.Read(plgx, hashTable, ficm, pathMap);
        });

        foreach (Ps5PlayGoChunk chunk in playGo.Chunks)
            chunks.Rows.Add(chunk.Id, chunk.Label, chunk.ExtentCount, $"0x{chunk.LanguageMask:X16}",
                FormatBytes(chunk.TotalBytes), chunk.TotalBytes);
        foreach (Ps5PlayGoScenario scenario in playGo.Scenarios)
            scenarios.Rows.Add(scenario.Id, scenario.Label, scenario.InitialChunkCount, scenario.Chunks.Count,
                string.Join(" ", scenario.Chunks));
        foreach (Ps5PlayGoFileChunk file in playGo.Files)
            files.Rows.Add(file.Path, file.ChunkId, $"0x{file.PathHash:X16}");

        gridPlayGoChunks.DataSource = chunks;
        gridPlayGoScenarios.DataSource = scenarios;
        gridPlayGoFiles.DataSource = files;

        int resolved = playGo.Files.Count(file => !string.IsNullOrEmpty(file.Path));
        if (playGo.Chunks.Count == 0 && playGo.Files.Count == 0)
        {
            lblPlayGoSummary.Text = string.IsNullOrEmpty(playGo.Notice)
                ? "No PlayGo data found for this package."
                : "No PlayGo data found for this package.   Note: " + playGo.Notice;
            return;
        }

        StringBuilder summary = new();
        summary.Append($"PlayGo v{playGo.VersionMajor}.{playGo.VersionMinor}   ");
        summary.Append($"Header flags: 0x{playGo.HeaderFlags:X8}   ");
        summary.Append($"Chunks: {playGo.Chunks.Count:N0}   Scenarios: {playGo.Scenarios.Count:N0}   ");
        summary.Append($"Default scenario: {playGo.DefaultScenarioId}   Files mapped: {resolved:N0} / {playGo.Files.Count:N0}");
        if (!string.IsNullOrEmpty(playGo.ContentId))
            summary.Append($"   Content ID: {playGo.ContentId}");
        if (!string.IsNullOrEmpty(playGo.Notice))
            summary.Append($"   Note: {playGo.Notice}");
        lblPlayGoSummary.Text = summary.ToString();
    }

    private static string ContentTypeText(uint contentType) =>
        contentType == 0x20 ? "0x00000020  GD (game data)" : $"0x{contentType:X8}";

    private static string SegmentName(string name) => name switch
    {
        "SC" => "SC (embedded CNT)",
        "CNT" => "CNT (metadata container)",
        "LIH" => "LIH (patch layer)",
        _ => name
    };

    private static string SegmentNote(string name) => name switch
    {
        "SC" or "CNT" => "Block-aligned (64 KiB); not the logical CNT length",
        "LIH" => "Patch layer header",
        _ => string.Empty
    };

    private async Task<byte[]> ReadParamSfoAsync(Ps5GameInfo game, SonyPkgSummary? package)
    {
        // param.sfo is an outer CNT entry (id 0x1000) in Sony packages; the inner-PFS
        // path is only a fallback for layouts where that entry is not available.
        const uint ParamSfoEntryId = 0x1000;
        const int MaximumSfoBytes = 1 << 20;
        SonyPkgEntry? entry = package?.Entries.FirstOrDefault(candidate => candidate.Id == ParamSfoEntryId);
        if (entry is not null && !entry.IsEncrypted && entry.DataSize > 0 && entry.DataSize <= MaximumSfoBytes)
        {
            try
            {
                return new SonyPkgReader().ReadEntryBytes(game.RootPath, package!, entry, MaximumSfoBytes);
            }
            catch (InvalidDataException)
            {
                // Fall back to the inner-PFS copy below.
            }
        }
        return await ReadGameFileAsync(game, "sce_sys/param.sfo");
    }

    private static bool SourceExists(Ps5GameInfo game) =>
        game.SourceKind == Ps5SourceKind.LooseDump ? Directory.Exists(game.RootPath) : File.Exists(game.RootPath);

    private static async Task<byte[]> ReadGameFileAsync(Ps5GameInfo game, string path)
    {
        if (!SourceExists(game)) return [];
        try
        {
            GameFileChunk chunk = await Task.Run(() =>
                GameFileSystem.ReadFileChunk(game, path, 0, 4 * 1024 * 1024));
            return chunk.Data;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or ArgumentException)
        {
            return [];
        }
    }

    private static async Task AddExtraFileRowAsync(Ps5GameInfo game, DataTable table, string path)
    {
        byte[] bytes = await ReadGameFileAsync(game, path);
        if (bytes.Length == 0)
        {
            table.Rows.Add(path, "not present", string.Empty, string.Empty);
            return;
        }
        int leading = Math.Min(16, bytes.Length);
        table.Rows.Add(path, "present", FormatBytes(bytes.Length), Convert.ToHexString(bytes.AsSpan(0, leading)));
    }

    private void ApplyArtwork(Ps5Artwork artwork)
    {
        ReplaceImage(pictureIcon, artwork.Icon);
        ReplaceImage(pictureBackground0, artwork.Background);
        ReplaceImage(pictureBackground1, artwork.Background1);
        ReplaceImage(pictureBackground2, artwork.Background2);
    }

    private void btnArtworkSaveAll_Click(object? sender, EventArgs e)
    {
        if (_currentArtwork is null)
        {
            AppDialog.ShowInformation("Select a game first to save its artwork.", "Artwork");
            return;
        }

        folderBrowserDialog.Description = "Select a folder for the artwork images";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string folder = folderBrowserDialog.SelectedPath;
        int saved = 0;
        foreach ((PictureBox control, string name) in ArtworkSlots())
        {
            Ps5ImageData? data = ArtworkForControl(control);
            if (data is null || data.IsEmpty) continue;
            try
            {
                SaveArtworkImage(data, Path.Combine(folder, name));
                saved++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException)
            {
                AppDialog.ShowError(ex.Message, "Save artwork");
                return;
            }
        }

        if (saved == 0) AppDialog.ShowInformation("No artwork was available to save.", "Artwork");
        else statusLabel.Text = $"Saved {saved} artwork image(s) to {folder}.";
    }

    private void menuArtworkSaveThis_Click(object? sender, EventArgs e)
    {
        if (contextArtwork.SourceControl is not PictureBox control) return;
        Ps5ImageData? data = ArtworkForControl(control);
        if (data is null || data.IsEmpty)
        {
            AppDialog.ShowInformation("This image is not available for this game.", "Artwork");
            return;
        }

        artworkSaveDialog.FileName = ArtworkName(control);
        if (artworkSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            SaveArtworkImage(data, artworkSaveDialog.FileName);
            statusLabel.Text = $"Saved {Path.GetFileName(artworkSaveDialog.FileName)}.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ExternalException)
        {
            AppDialog.ShowError(ex.Message, "Save artwork");
        }
    }

    private IEnumerable<(PictureBox Control, string Name)> ArtworkSlots()
    {
        yield return (pictureIcon, "icon0.png");
        yield return (pictureBackground0, "pic0.png");
        yield return (pictureBackground1, "pic1.png");
        yield return (pictureBackground2, "pic2.png");
    }

    private Ps5ImageData? ArtworkForControl(Control? control) =>
        control == pictureIcon ? _currentArtwork?.Icon
        : control == pictureBackground0 ? _currentArtwork?.Background
        : control == pictureBackground1 ? _currentArtwork?.Background1
        : control == pictureBackground2 ? _currentArtwork?.Background2
        : null;

    private string ArtworkName(Control? control) =>
        control == pictureIcon ? "icon0.png"
        : control == pictureBackground0 ? "pic0.png"
        : control == pictureBackground1 ? "pic1.png"
        : control == pictureBackground2 ? "pic2.png"
        : "artwork.png";

    private static void SaveArtworkImage(Ps5ImageData data, string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        string extension = Path.GetExtension(path).ToLowerInvariant();
        if (!data.IsRgba && extension == ".png")
        {
            // PNG artwork is written exactly as stored.
            File.WriteAllBytes(path, data.Bytes);
            return;
        }

        ImageFormat format = extension switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
        using Image image = data.IsRgba
            ? BitmapFromRgba(data.Bytes, data.Width, data.Height)
            : ImageFromBytes(data.Bytes) ?? throw new InvalidDataException("The artwork image could not be decoded.");
        image.Save(path, format);
    }

    private void PopulateTrophies(Ps5TrophySet? set)
    {
        gridTrophies.DataSource = null;
        DisposeTrophyImages();
        var table = new DataTable();
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("Icon", typeof(Image));
        table.Columns.Add("Grade");
        table.Columns.Add("Hidden", typeof(bool));
        table.Columns.Add("Name");
        table.Columns.Add("Description");
        table.Columns.Add("Unlock Condition");
        if (set is not null)
        {
            foreach (Ps5Trophy trophy in set.Trophies)
            {
                Image? icon = ToImage(trophy.Icon);
                if (icon is not null) _trophyImages.Add(icon);
                table.Rows.Add(trophy.Id, icon!, trophy.Grade, trophy.Hidden, trophy.Name, trophy.Description, trophy.UnlockCondition);
            }
            string grades = string.Join(", ", set.Trophies.GroupBy(trophy => trophy.Grade)
                .Select(group => $"{group.Key}: {group.Count()}"));
            lblTrophySummary.Text = $"{set.Title} - {set.Trophies.Count} trophies ({grades}) - {set.NpCommunicationId} - " +
                                    $"Language: {set.SelectedLanguage} - UCP integrity: {(set.IntegrityValid ? "Valid" : "Failed")}";
        }
        else lblTrophySummary.Text = "No PS5 trophy archive was found.";
        _trophyView = table.DefaultView;
        gridTrophies.DataSource = _trophyView;
        ApplyTrophyFilter();
    }

    private void trophyFilter_Changed(object? sender, EventArgs e) => ApplyTrophyFilter();

    private void ApplyTrophyFilter()
    {
        if (_trophyView is null) return;
        var clauses = new List<string>();
        var grades = new List<string>();
        if (chkTrophyPlatinum.Checked) grades.Add("'Platinum'");
        if (chkTrophyGold.Checked) grades.Add("'Gold'");
        if (chkTrophySilver.Checked) grades.Add("'Silver'");
        if (chkTrophyBronze.Checked) grades.Add("'Bronze'");
        if (grades.Count > 0) clauses.Add($"Grade IN ({string.Join(",", grades)})");

        string search = searchTrophy.SearchText.Trim();
        if (search.Length > 0)
        {
            string escaped = search.Replace("'", "''").Replace("[", "[[]").Replace("*", "[*]").Replace("%", "[%]");
            clauses.Add($"(Name LIKE '%{escaped}%' OR Description LIKE '%{escaped}%')");
        }
        if (!chkTrophyShowHidden.Checked) clauses.Add("Hidden = False");

        try { _trophyView.RowFilter = string.Join(" AND ", clauses); }
        catch (SyntaxErrorException) { _trophyView.RowFilter = string.Empty; }
    }

    private IReadOnlyList<Ps5Trophy> VisibleTrophies()
    {
        if (_currentDetails?.TrophySet is not { } set || _trophyView is null) return [];
        var ids = new HashSet<int>();
        foreach (DataRowView row in _trophyView)
            if (row["ID"] is int id) ids.Add(id);
        return set.Trophies.Where(trophy => ids.Contains(trophy.Id)).ToList();
    }

    private Ps5Trophy? SelectedTrophy()
    {
        if (_currentDetails?.TrophySet is not { } set || gridTrophies.SelectedRows.Count == 0) return null;
        if (gridTrophies.SelectedRows[0].DataBoundItem is not DataRowView view) return null;
        if (view["ID"] is not int id) return null;
        return set.Trophies.FirstOrDefault(trophy => trophy.Id == id);
    }

    private void btnTrophySaveIcons_Click(object? sender, EventArgs e)
    {
        IReadOnlyList<Ps5Trophy> trophies = VisibleTrophies();
        if (trophies.Count == 0)
        {
            AppDialog.ShowInformation("No trophies are available to save.", "Trophies");
            return;
        }

        folderBrowserDialog.Description = "Select a folder for the trophy icons";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string folder = folderBrowserDialog.SelectedPath;
        int saved = 0;
        foreach (Ps5Trophy trophy in trophies)
        {
            if (trophy.IconPng is not { Length: > 0 }) continue;
            try
            {
                File.WriteAllBytes(Path.Combine(folder, $"trop{trophy.Id:000}.png"), trophy.IconPng);
                saved++;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                AppDialog.ShowError(ex.Message, "Save trophy icons");
                return;
            }
        }
        statusLabel.Text = saved == 0
            ? "No trophy icons were available to save."
            : $"Saved {saved} trophy icon(s) to {folder}.";
    }

    private void menuTrophySaveIcon_Click(object? sender, EventArgs e)
    {
        Ps5Trophy? trophy = SelectedTrophy();
        if (trophy?.IconPng is not { Length: > 0 })
        {
            AppDialog.ShowInformation("This trophy has no icon to save.", "Trophies");
            return;
        }

        artworkSaveDialog.FileName = $"trop{trophy.Id:000}.png";
        if (artworkSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.WriteAllBytes(artworkSaveDialog.FileName, trophy.IconPng);
            statusLabel.Text = $"Saved {Path.GetFileName(artworkSaveDialog.FileName)}.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError(ex.Message, "Save trophy icon");
        }
    }

    private void btnTrophyExportCsv_Click(object? sender, EventArgs e)
    {
        if (_trophyView is null || _trophyView.Count == 0)
        {
            AppDialog.ShowInformation("There are no trophies to export.", "Trophies");
            return;
        }
        if (trophyCsvSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var builder = new StringBuilder();
            builder.AppendLine("ID,Grade,Hidden,Name,Description,Unlock Condition");
            foreach (DataRowView row in _trophyView)
            {
                string[] fields =
                [
                    Convert.ToString(row["ID"]) ?? string.Empty,
                    Convert.ToString(row["Grade"]) ?? string.Empty,
                    Convert.ToString(row["Hidden"]) ?? string.Empty,
                    Convert.ToString(row["Name"]) ?? string.Empty,
                    Convert.ToString(row["Description"]) ?? string.Empty,
                    Convert.ToString(row["Unlock Condition"]) ?? string.Empty
                ];
                builder.AppendLine(string.Join(',', fields.Select(Csv)));
            }
            File.WriteAllText(trophyCsvSaveDialog.FileName, builder.ToString(), new UTF8Encoding(true));
            statusLabel.Text = $"Exported {_trophyView.Count:N0} trophies.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDialog.ShowError(ex.Message, "Export trophies");
        }
    }

    private void PopulateActivities(Ps5UdsSummary? uds)
    {
        _udsSummary = uds;
        searchUds.SearchText = string.Empty;
        if (uds is null)
        {
            lblActivitiesSummary.Text = "No UDS activity archive was found.";
            gridUdsEvents.DataSource = null;
            gridUdsEventProperties.DataSource = null;
            gridUdsStats.DataSource = null;
            gridUdsEnums.DataSource = null;
            gridUdsRules.DataSource = null;
            return;
        }

        lblActivitiesSummary.Text = $"Events: {uds.EventCount:N0}  |  Stats: {uds.StatCount:N0}  |  Enum groups: {uds.EnumGroupCount:N0}  |  " +
                                    $"Extraction rules: {uds.ExtractionRuleCount:N0}  |  {uds.NpCommunicationId}  |  " +
                                    $"UCP integrity: {(uds.IntegrityValid ? "Valid" : "Failed")}";
        RebuildUdsTables();
    }

    private void searchUds_SearchTextChanged(object? sender, EventArgs e) => RebuildUdsTables();

    private void tabsUds_SelectedIndexChanged(object? sender, EventArgs e) => RebuildUdsTables();

    private void gridUdsEvents_SelectionChanged(object? sender, EventArgs e) => PopulateEventProperties();

    private void btnUdsCopyAll_Click(object? sender, EventArgs e) => CopyActiveUdsGrid(copyAll: true);

    private void btnUdsCopySelected_Click(object? sender, EventArgs e) => CopyActiveUdsGrid(copyAll: false);

    private void RebuildUdsTables()
    {
        if (_udsSummary is not { } uds) return;
        string filter = searchUds.SearchText.Trim();

        var events = new DataTable();
        events.Columns.Add("Event");
        events.Columns.Add("Type");
        events.Columns.Add("Group");
        events.Columns.Add("Props", typeof(int));
        foreach (Ps5UdsEvent item in uds.Events)
            if (Matches(filter, item.Name, item.Type, item.DefinitionGroup))
                events.Rows.Add(item.Name, item.Type, item.DefinitionGroup, item.PropertyCount);
        gridUdsEvents.DataSource = events;

        var stats = new DataTable();
        stats.Columns.Add("ID", typeof(int));
        stats.Columns.Add("Name");
        stats.Columns.Add("Group");
        stats.Columns.Add("Type");
        stats.Columns.Add("Aggregation");
        stats.Columns.Add("Origin");
        stats.Columns.Add("Enum", typeof(int));
        stats.Columns.Add("Min");
        stats.Columns.Add("Max");
        stats.Columns.Add("Initial");
        stats.Columns.Add("Trophy refs");
        foreach (Ps5UdsStat stat in uds.Stats)
            if (Matches(filter, stat.Name, stat.DefinitionGroup, stat.DataType, stat.Aggregation, stat.Origin,
                    stat.StatId.ToString(), stat.SourceId))
                stats.Rows.Add(stat.StatId, stat.Name, stat.DefinitionGroup, stat.DataType, stat.Aggregation,
                    stat.Origin, stat.EnumId, stat.MinValue, stat.MaxValue, stat.InitialValue, TrophyRefs(stat.StatId));
        gridUdsStats.DataSource = stats;

        var enums = new DataTable();
        enums.Columns.Add("Enum", typeof(int));
        enums.Columns.Add("Group");
        enums.Columns.Add("Source");
        enums.Columns.Add("Values", typeof(int));
        enums.Columns.Add("Sample");
        foreach (Ps5UdsEnumGroup group in uds.EnumGroups)
            if (Matches(filter, group.SourceId, group.DefinitionGroup, group.EnumId.ToString(),
                    string.Join(' ', group.Values)))
                enums.Rows.Add(group.EnumId, group.DefinitionGroup, group.SourceId, group.ValueCount,
                    string.Join(", ", group.Values.Take(4)));
        gridUdsEnums.DataSource = enums;

        var rules = new DataTable();
        rules.Columns.Add("Rule", typeof(int));
        rules.Columns.Add("Group");
        rules.Columns.Add("Event");
        rules.Columns.Add("Condition");
        rules.Columns.Add("Input");
        rules.Columns.Add("Convert");
        rules.Columns.Add("Output stat");
        foreach (Ps5UdsRule rule in uds.Rules)
            if (Matches(filter, rule.EventName, rule.DefinitionGroup, rule.OutputStatName, rule.SourceId, rule.Input, rule.Condition))
                rules.Rows.Add(rule.RuleId, rule.DefinitionGroup, rule.EventName, rule.Condition, rule.Input, rule.Convert,
                    $"{rule.OutputStatName} (#{rule.OutputStatId})");
        gridUdsRules.DataSource = rules;

        PopulateEventProperties();
    }

    private void PopulateEventProperties()
    {
        var table = new DataTable();
        table.Columns.Add("Property");
        table.Columns.Add("Type");
        table.Columns.Add("Item type");
        table.Columns.Add("Mapped");
        if (_udsSummary is { } uds && gridUdsEvents.SelectedRows.Count > 0 &&
            gridUdsEvents.SelectedRows[0].DataBoundItem is DataRowView view && view["Event"] is string name)
        {
            Ps5UdsEvent? selected = uds.Events.FirstOrDefault(item =>
                string.Equals(item.Name, name, StringComparison.Ordinal));
            if (selected is not null)
                foreach (Ps5UdsProperty property in selected.Properties)
                    table.Rows.Add(property.Path, property.DataType, property.ItemType, property.MappedProperty);
        }
        gridUdsEventProperties.DataSource = table;
    }

    private string TrophyRefs(int statId)
    {
        if (_currentDetails?.TrophySet is not { } set) return string.Empty;
        List<int> ids = set.Trophies.Where(trophy => trophy.UdsStatId == statId).Select(trophy => trophy.Id).ToList();
        return ids.Count == 0 ? string.Empty : string.Join(", ", ids);
    }

    private static bool Matches(string filter, params string?[] values) =>
        filter.Length == 0 ||
        values.Any(value => value is not null && value.Contains(filter, StringComparison.CurrentCultureIgnoreCase));

    private DarkDataGridView ActiveUdsGrid() =>
        tabsUds.SelectedTab == tabUdsStats ? gridUdsStats
        : tabsUds.SelectedTab == tabUdsEnums ? gridUdsEnums
        : tabsUds.SelectedTab == tabUdsRules ? gridUdsRules
        : gridUdsEvents;

    private void CopyActiveUdsGrid(bool copyAll)
    {
        DarkDataGridView grid = ActiveUdsGrid();
        DataGridViewRow[] rows = (copyAll ? grid.Rows.Cast<DataGridViewRow>() : grid.SelectedRows.Cast<DataGridViewRow>()).ToArray();
        var builder = new StringBuilder();
        foreach (DataGridViewRow row in rows)
        {
            if (row.DataBoundItem is not DataRowView view) continue;
            foreach (DataColumn column in view.DataView.Table.Columns)
                builder.Append(column.ColumnName).Append(": ").Append(view.Row[column]?.ToString() ?? string.Empty).AppendLine();
            builder.AppendLine();
        }
        if (builder.Length == 0)
        {
            AppDialog.ShowInformation("Nothing to copy.", "Activities & UDS");
            return;
        }
        CopyText(builder.ToString().TrimEnd());
    }

    private void PopulateFiles(Ps5GameInfo game, Ps5FileInventory inventory)
    {
        _currentGameRoot = game.RootPath;
        _currentSourceIsContainer = game.SourceKind != Ps5SourceKind.LooseDump;
        _directoryNodes.Clear();
        _filesByDirectory.Clear();
        treeFiles.BeginUpdate();
        treeFiles.Nodes.Clear();

        var root = new TreeNode(game.Title) { Tag = string.Empty, ImageIndex = FileIconProvider.Folder, SelectedImageIndex = FileIconProvider.Folder };
        treeFiles.Nodes.Add(root);
        _directoryNodes[string.Empty] = root;

        foreach (Ps5FileInfo file in inventory.Files)
        {
            string directory = NormalizeDirectory(Path.GetDirectoryName(file.RelativePath));
            if (!_filesByDirectory.TryGetValue(directory, out List<Ps5FileInfo>? directoryFiles))
            {
                directoryFiles = [];
                _filesByDirectory[directory] = directoryFiles;
            }
            directoryFiles.Add(file);

            if (directory.Length == 0) continue;
            string current = string.Empty;
            TreeNode parent = root;
            foreach (string segment in directory.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                current = current.Length == 0 ? segment : Path.Combine(current, segment);
                if (!_directoryNodes.TryGetValue(current, out TreeNode? node))
                {
                    node = new TreeNode(segment) { Tag = current, ImageIndex = FileIconProvider.Folder, SelectedImageIndex = FileIconProvider.Folder };
                    parent.Nodes.Add(node);
                    _directoryNodes[current] = node;
                }
                parent = node;
            }
        }

        _directorySizes.Clear();
        foreach (KeyValuePair<string, List<Ps5FileInfo>> pair in _filesByDirectory)
        {
            long directSize = 0;
            foreach (Ps5FileInfo file in pair.Value) directSize += file.Size;
            string current = pair.Key;
            while (true)
            {
                _directorySizes[current] = _directorySizes.GetValueOrDefault(current) + directSize;
                if (current.Length == 0) break;
                current = NormalizeDirectory(Path.GetDirectoryName(current));
            }
        }

        SortTree(root);
        root.Expand();
        treeFiles.EndUpdate();
        treeFiles.SelectedNode = root;
        lblFilesSummary.Text = game.SourceKind switch
        {
            Ps5SourceKind.SonyPackage =>
                game.Package?.NestedPfs?.AccessState == SonyPfsAccessState.PlaintextIndexed
                    ? $"{inventory.FileCount:N0} files - {FormatBytes(inventory.TotalSize)} content - read directly from nested PFS"
                    : $"{inventory.FileCount:N0} readable CNT entries - {FormatBytes(inventory.TotalSize)} package",
            Ps5SourceKind.Ffpfsc =>
                $"{inventory.FileCount:N0} files - {FormatBytes(inventory.TotalSize)} logical " +
                $"{(Path.GetExtension(game.ContainerInnerFileName).Equals(".ffpkg", StringComparison.OrdinalIgnoreCase) ? "UFS2" : "exFAT")} content - read directly from FFPFSC",
            Ps5SourceKind.FilesystemImage =>
                $"{inventory.FileCount:N0} files - {FormatBytes(inventory.TotalSize)} content - read directly from {Path.GetExtension(game.RootPath).TrimStart('.').ToUpperInvariant()}",
            Ps5SourceKind.Ffpkg =>
                $"{inventory.FileCount:N0} files - {FormatBytes(inventory.TotalSize)} logical content - read directly from UFS2 FFPKG",
            _ => $"{inventory.FileCount:N0} files - {FormatBytes(inventory.TotalSize)} total - double-click folders to browse or files to reveal them"
        };
        RefreshFileList();
    }

    private void RefreshFileList()
    {
        string directory = treeFiles.SelectedNode?.Tag as string ?? string.Empty;
        string filter = searchFileFilter.SearchText.Trim();
        var rows = new List<FileListRow>();

        if (filter.Length == 0)
        {
            if (directory.Length > 0)
            {
                string parent = NormalizeDirectory(Path.GetDirectoryName(directory));
                rows.Add(new FileListRow("..", "Folder", parent, string.Empty, 0,
                    new FileBrowserEntry(parent, true), FileIconProvider.FolderOpen, true));
            }

            foreach (string child in _directoryNodes.Keys.Where(path =>
                path.Length > 0 && NormalizeDirectory(Path.GetDirectoryName(path)).Equals(directory, StringComparison.OrdinalIgnoreCase)))
            {
                long size = GetDirectorySize(child);
                rows.Add(new FileListRow(Path.GetFileName(child), "Folder", child, FormatBytes(size), size,
                    new FileBrowserEntry(child, true), FileIconProvider.Folder, true));
            }

            if (_filesByDirectory.TryGetValue(directory, out List<Ps5FileInfo>? files))
                foreach (Ps5FileInfo file in files)
                    rows.Add(FileListRowFor(file));
        }
        else
        {
            // Recursive search: match folders and files anywhere in the tree.
            foreach (string child in _directoryNodes.Keys.Where(path => path.Length > 0))
            {
                string name = Path.GetFileName(child);
                if (!MatchesFileFilter(name, child, filter)) continue;
                long size = GetDirectorySize(child);
                rows.Add(new FileListRow(name, "Folder", child, FormatBytes(size), size,
                    new FileBrowserEntry(child, true), FileIconProvider.Folder, true));
            }

            foreach (List<Ps5FileInfo> list in _filesByDirectory.Values)
                foreach (Ps5FileInfo file in list)
                    if (MatchesFileFilter(Path.GetFileName(file.RelativePath), file.RelativePath, filter))
                        rows.Add(FileListRowFor(file));
        }

        rows.Sort(CompareFileRows);

        listFiles.BeginUpdate();
        listFiles.Items.Clear();
        foreach (FileListRow row in rows)
            AddFileListItem(row.Name, row.Type, row.Path, row.SizeText, row.Entry, row.Icon);
        listFiles.EndUpdate();
        listFiles.RefreshLayout();
    }

    private static FileListRow FileListRowFor(Ps5FileInfo file)
    {
        string name = Path.GetFileName(file.RelativePath);
        string type = string.IsNullOrWhiteSpace(file.Extension) ? "File" : file.Extension.TrimStart('.').ToUpperInvariant() + " File";
        if (file.IsEncrypted) type += " (Encrypted)";
        return new FileListRow(name, type, file.RelativePath, FormatBytes(file.Size), file.Size,
            new FileBrowserEntry(file.RelativePath, false, file.Size), FileIconProvider.ForEntry(name, false), false);
    }

    private int CompareFileRows(FileListRow left, FileListRow right)
    {
        int group = right.IsDirectory.CompareTo(left.IsDirectory); // folders first
        if (group != 0) return group;
        int result = _fileSortColumn switch
        {
            1 => string.Compare(left.Type, right.Type, StringComparison.CurrentCultureIgnoreCase),
            2 => string.Compare(left.Path, right.Path, StringComparison.CurrentCultureIgnoreCase),
            3 => left.Size.CompareTo(right.Size),
            _ => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase)
        };
        if (!_fileSortAscending) result = -result;
        return result != 0 ? result : string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase);
    }

    private void listFiles_ColumnClick(object? sender, ColumnClickEventArgs e)
    {
        if (e.Column == _fileSortColumn) _fileSortAscending = !_fileSortAscending;
        else { _fileSortColumn = e.Column; _fileSortAscending = true; }
        RefreshFileList();
    }

    private List<(string RelativePath, long Size)> SelectedFileTargets()
    {
        var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (ListViewItem item in listFiles.SelectedItems)
        {
            if (item.Tag is not FileBrowserEntry entry) continue;
            if (entry.IsDirectory)
            {
                foreach ((string relativePath, long size) in FilesUnder(entry.RelativePath))
                    result[relativePath] = size;
            }
            else
            {
                result[entry.RelativePath] = entry.Size;
            }
        }
        return result.Select(pair => (pair.Key, pair.Value)).ToList();
    }

    private List<(string RelativePath, long Size)> FilesUnder(string folder)
    {
        var result = new List<(string, long)>();
        string prefix = folder.Length == 0 ? string.Empty : folder + Path.DirectorySeparatorChar;
        foreach (List<Ps5FileInfo> list in _filesByDirectory.Values)
            foreach (Ps5FileInfo file in list)
                if (prefix.Length == 0 || file.RelativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    result.Add((file.RelativePath, file.Size));
        return result;
    }

    private async void menuFileExtractSelected_Click(object? sender, EventArgs e)
    {
        List<(string RelativePath, long Size)> files = SelectedFileTargets();
        if (files.Count == 0)
        {
            AppDialog.ShowInformation("Select one or more files to extract.", "Files");
            return;
        }
        folderBrowserDialog.Description = "Select a folder for the extracted files";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        await ExtractFilesAsync(files, folderBrowserDialog.SelectedPath);
    }

    private async void menuTreeExtract_Click(object? sender, EventArgs e)
    {
        if (treeFiles.SelectedNode?.Tag is not string folder) return;
        List<(string RelativePath, long Size)> files = FilesUnder(folder);
        if (files.Count == 0)
        {
            AppDialog.ShowInformation("There are no files under this folder.", "Files");
            return;
        }
        folderBrowserDialog.Description = "Select a folder for the extracted files";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        await ExtractFilesAsync(files, folderBrowserDialog.SelectedPath);
    }

    private async void btnFileExtractAll_Click(object? sender, EventArgs e)
    {
        List<(string RelativePath, long Size)> files = FilesUnder(string.Empty);
        if (files.Count == 0)
        {
            AppDialog.ShowInformation("There are no files to extract.", "Files");
            return;
        }
        folderBrowserDialog.Description = "Select a folder for the extracted package";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        await ExtractFilesAsync(files, folderBrowserDialog.SelectedPath);
    }

    private async Task ExtractFilesAsync(IReadOnlyList<(string RelativePath, long Size)> files, string destination)
    {
        if (_selectedGame is not { } game || !IsBrowsableFilesystem(game.SourceKind) || _isFileBusy) return;
        _fileCancellation?.Cancel();
        _fileCancellation?.Dispose();
        _fileCancellation = new CancellationTokenSource();
        _isFileBusy = true;
        UpdateOperationState();
        string root = Path.GetFullPath(destination);
        string rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        long total = files.Sum(file => file.Size);
        long completed = 0;
        int extracted = 0;
        try
        {
            foreach ((string relativePath, long size) in files)
            {
                _fileCancellation.Token.ThrowIfCancellationRequested();
                string target = Path.GetFullPath(Path.Combine(root, relativePath));
                if (!target.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("The extraction path left the destination folder.");
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                long baseline = completed;
                var progress = new Progress<long>(copied =>
                    statusLabel.Text = $"Extracting {Path.GetFileName(relativePath)}: {FormatBytes(baseline + copied)} / {FormatBytes(total)}");
                await Task.Run(() => GameFileSystem.ExtractFileAsync(game, relativePath, target, progress,
                    _fileCancellation.Token), _fileCancellation.Token);
                completed += size;
                extracted++;
            }
            statusLabel.Text = $"Extracted {extracted:N0} file(s) to {destination}.";
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = $"Extraction cancelled after {extracted:N0} file(s).";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or System.ComponentModel.Win32Exception)
        {
            statusLabel.Text = "Extraction failed.";
            AppDialog.ShowError(ex.Message, "Extract files");
        }
        finally
        {
            _isFileBusy = false;
            UpdateOperationState();
        }
    }

    private async void listFiles_ItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (_selectedGame is not { } game || !IsBrowsableFilesystem(game.SourceKind) || _isFileBusy) return;
        List<(string RelativePath, long Size)> files = SelectedFileTargets();
        if (files.Count == 0) return;

        Directory.CreateDirectory(_previewDirectory);
        string dragRoot = Path.Combine(_previewDirectory, "drag-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dragRoot);
        var paths = new List<string>();
        try
        {
            foreach ((string relativePath, long size) in files)
            {
                string target = Path.Combine(dragRoot, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                try
                {
                    await Task.Run(() => GameFileSystem.ExtractFileAsync(game, relativePath, target));
                    paths.Add(target);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or NotSupportedException)
                {
                }
            }
        }
        catch (Exception) { }

        if (paths.Count > 0)
            listFiles.DoDragDrop(new DataObject(DataFormats.FileDrop, paths.ToArray()), DragDropEffects.Copy);
    }

    private void AddFileListItem(string name, string type, string path, string size, FileBrowserEntry entry, int iconIndex)
    {
        var item = new ListViewItem(name) { Tag = entry, ImageIndex = iconIndex };
        item.SubItems.Add(type);
        item.SubItems.Add(path);
        item.SubItems.Add(size);
        listFiles.Items.Add(item);
    }

    private long GetDirectorySize(string directory) => _directorySizes.GetValueOrDefault(directory);

    private static bool MatchesFileFilter(string name, string path, string filter) =>
        filter.Length == 0 ||
        name.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ||
        path.Contains(filter, StringComparison.CurrentCultureIgnoreCase);

    private static string NormalizeDirectory(string? directory) =>
        string.IsNullOrWhiteSpace(directory) || directory == "."
            ? string.Empty
            : directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar);

    private static void SortTree(TreeNode parent)
    {
        TreeNode[] children = parent.Nodes.Cast<TreeNode>()
            .OrderBy(node => node.Text, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        parent.Nodes.Clear();
        parent.Nodes.AddRange(children);
        foreach (TreeNode child in children) SortTree(child);
    }

    private void treeFiles_AfterSelect(object? sender, TreeViewEventArgs e) => RefreshFileList();

    private void searchFileFilter_SearchTextChanged(object? sender, EventArgs e) => RefreshFileList();

    private void menuTreeExpand_Click(object? sender, EventArgs e) => treeFiles.SelectedNode?.Expand();

    private void menuTreeCollapse_Click(object? sender, EventArgs e) => treeFiles.SelectedNode?.Collapse();

    private void menuTreeExpandAll_Click(object? sender, EventArgs e) => treeFiles.ExpandAll();

    private void menuTreeCollapseAll_Click(object? sender, EventArgs e) => treeFiles.CollapseAll();

    private void menuTreeCopyPath_Click(object? sender, EventArgs e)
    {
        if (treeFiles.SelectedNode is not { } node) return;
        string path = node.Tag as string ?? string.Empty;
        CopyText(path.Length > 0 ? path : _currentGameRoot);
    }

    private void menuFileCopyPath_Click(object? sender, EventArgs e)
    {
        if (TryGetSelectedFile(out FileBrowserEntry entry, out _)) CopyText(entry.RelativePath);
    }

    private void menuFileCopyName_Click(object? sender, EventArgs e)
    {
        if (TryGetSelectedFile(out FileBrowserEntry entry, out _)) CopyText(Path.GetFileName(entry.RelativePath));
    }

    private async void listFiles_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _filePreviewCancellation?.Cancel();
        _filePreviewCancellation?.Dispose();
        _filePreviewCancellation = new CancellationTokenSource();
        int version = Interlocked.Increment(ref _filePreviewVersion);
        ResetFileViewer();
        if (!TryGetSelectedFile(out FileBrowserEntry entry, out Ps5FileInfo file) || _selectedGame is null) return;

        _filePreviewEntry = entry;
        _filePreviewInfo = file;
        string extension = file.Extension.ToLowerInvariant();
        AssetInspection inspection = await InspectAssetAsync(_selectedGame, entry, file, _filePreviewCancellation.Token);
        string summary = BuildAssetSummary(entry, file, inspection);
        lblFileViewerInfo.Text = summary;
        try
        {
            if (inspection.Category is AssetCategory.Audio or AssetCategory.Video || IsMediaExtension(extension))
            {
                ShowMediaPreviewReady(file);
                lblFileViewerInfo.Text = summary;
                return;
            }
            bool image = inspection.Category == AssetCategory.Image || IsImageExtension(extension);
            if (image && file.Size <= 256L * 1024 * 1024 && file.Size <= int.MaxValue)
            {
                await LoadImageViewerAsync(_selectedGame, entry, file,
                    inspection.Format == "DDS" ? ".dds" : extension, version, _filePreviewCancellation.Token);
                lblFileViewerInfo.Text = summary;
                return;
            }
            if (inspection.Category == AssetCategory.Text || IsTextExtension(extension))
            {
                bool shown = await LoadTextViewerAsync(_selectedGame, entry, file, version,
                    _filePreviewCancellation.Token);
                if (shown)
                {
                    lblFileViewerInfo.Text = summary;
                    return;
                }
            }
            await LoadHexViewerAsync(0, version, _filePreviewCancellation.Token);
            lblFileViewerInfo.Text = summary;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or ArgumentException)
        {
            if (version == _filePreviewVersion)
                lblFileViewerInfo.Text = $"Preview unavailable: {ex.Message}";
        }
    }

    private async Task<AssetInspection> InspectAssetAsync(Ps5GameInfo game, FileBrowserEntry entry,
        Ps5FileInfo file, CancellationToken cancellationToken)
    {
        int count = checked((int)Math.Min(64, Math.Max(0, file.Size)));
        if (count == 0) return AssetInspection.Unknown("Empty file");
        try
        {
            GameFileChunk chunk = await Task.Run(() => GameFileSystem.ReadFileChunk(game, entry.RelativePath, 0,
                count, cancellationToken), cancellationToken);
            return AssetInspector.Inspect(entry.RelativePath, chunk.Data);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or ArgumentException)
        {
            return AssetInspection.Unknown(file.Extension);
        }
    }

    private static string BuildAssetSummary(FileBrowserEntry entry, Ps5FileInfo file, AssetInspection inspection)
    {
        string name = Path.GetFileName(entry.RelativePath);
        string metadata = inspection.Metadata.Count == 0
            ? string.Empty
            : "  |  " + string.Join(", ", inspection.Metadata.Select(item => $"{item.Name} {item.Value}"));
        return $"{name}  |  {inspection.Format}  |  {FormatBytes(file.Size)}{metadata}";
    }

    private async Task LoadImageViewerAsync(Ps5GameInfo game, FileBrowserEntry entry, Ps5FileInfo file,
        string extension, int version, CancellationToken cancellationToken)
    {
        byte[] imageBytes = await Task.Run(() =>
        {
            GameFileChunk chunk = GameFileSystem.ReadFileChunk(game, entry.RelativePath, 0,
                checked((int)file.Size), cancellationToken);
            if (extension != ".dds") return chunk.Data;
            using var input = new MemoryStream(chunk.Data, writable: false);
            return Ps5ImageCodec.DecodeDdsToPng(input);
        }, cancellationToken);
        if (version != _filePreviewVersion || cancellationToken.IsCancellationRequested) return;
        Image? replacement = ImageFromBytes(imageBytes);
        Image? previous = pictureFileViewer.Image;
        pictureFileViewer.Image = replacement;
        previous?.Dispose();
        pictureFileViewer.Visible = true;
        pictureFileViewer.BringToFront();
        HideViewerCommandButtons();
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Image  •  {FormatBytes(file.Size)}";
    }

    private async Task<bool> LoadTextViewerAsync(Ps5GameInfo game, FileBrowserEntry entry, Ps5FileInfo file,
        int version, CancellationToken cancellationToken)
    {
        const int maximumTextPreview = 1024 * 1024;
        GameFileChunk chunk = await Task.Run(() => GameFileSystem.ReadFileChunk(game, entry.RelativePath, 0,
            checked((int)Math.Min(file.Size, maximumTextPreview)), cancellationToken), cancellationToken);
        string text;
        try
        {
            using var input = new MemoryStream(chunk.Data, writable: false);
            using var reader = new StreamReader(input, new UTF8Encoding(false, true), true);
            text = reader.ReadToEnd();
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
        if (!LooksLikeText(text)) return false;
        if (version != _filePreviewVersion || cancellationToken.IsCancellationRequested) return true;
        if (Path.GetExtension(entry.RelativePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            text = PrettyJson(text);
        if (chunk.HasNext) text += $"{Environment.NewLine}{Environment.NewLine}… preview truncated at {FormatBytes(chunk.Data.Length)} …";
        txtFileViewer.Text = text;
        txtFileViewer.SelectionStart = 0;
        txtFileViewer.SelectionLength = 0;
        txtFileViewer.Visible = true;
        txtFileViewer.BringToFront();
        HideViewerCommandButtons();
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Text  •  {FormatBytes(file.Size)}";
        return true;
    }

    private async Task LoadHexViewerAsync(long offset, int version, CancellationToken cancellationToken)
    {
        Ps5GameInfo? game = _selectedGame;
        FileBrowserEntry? entry = _filePreviewEntry;
        if (game is null || entry is null) return;
        GameFileChunk chunk = await Task.Run(() => GameFileSystem.ReadFileChunk(game, entry.RelativePath, offset,
            HexPreviewPageSizeBytes, cancellationToken), cancellationToken);
        if (version != _filePreviewVersion || cancellationToken.IsCancellationRequested) return;
        _hexPreviewOffset = chunk.Offset;
        txtHexViewer.Text = FormatHexPage(chunk);
        txtHexViewer.SelectionStart = 0;
        txtHexViewer.SelectionLength = 0;
        txtHexViewer.Visible = true;
        txtHexViewer.BringToFront();
        HideViewerCommandButtons();
        btnHexPrevious.Visible = true;
        btnHexPrevious.Enabled = chunk.HasPrevious;
        btnHexNext.Visible = true;
        btnHexNext.Enabled = chunk.HasNext;
        lblHexPage.Visible = true;
        lblHexPage.Text = chunk.FileSize == 0
            ? "Empty file"
            : $"0x{chunk.Offset:X} - 0x{chunk.Offset + Math.Max(0, chunk.Data.Length - 1):X} / {FormatBytes(chunk.FileSize)}";
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Hex  •  {FormatBytes(chunk.FileSize)}";
    }

    private static string FormatHexPage(GameFileChunk chunk)
    {
        var output = new StringBuilder(Math.Max(128, chunk.Data.Length * 5));
        for (int row = 0; row < chunk.Data.Length; row += 16)
        {
            int count = Math.Min(16, chunk.Data.Length - row);
            output.Append((chunk.Offset + row).ToString("X12")).Append("  ");
            for (int column = 0; column < 16; column++)
            {
                if (column < count) output.Append(chunk.Data[row + column].ToString("X2"));
                else output.Append("  ");
                output.Append(column == 7 ? "  " : " ");
            }
            output.Append(" | ");
            for (int column = 0; column < count; column++)
            {
                byte value = chunk.Data[row + column];
                output.Append(value is >= 0x20 and <= 0x7E ? (char)value : '.');
            }
            output.AppendLine();
        }
        return output.ToString();
    }

    private void ShowMediaPreviewReady(Ps5FileInfo file)
    {
        mediaFileHost.Visible = true;
        mediaFileHost.BringToFront();
        HideViewerCommandButtons();
        btnMediaLoad.Visible = true;
        lblFileViewerInfo.Text = $"{Path.GetFileName(file.RelativePath)}  •  Media  •  {FormatBytes(file.Size)}  •  Click Load & Play";
    }

    private void HideViewerCommandButtons()
    {
        btnMediaLoad.Visible = false;
        btnMediaPlay.Visible = false;
        btnMediaPause.Visible = false;
        btnMediaStop.Visible = false;
        btnHexPrevious.Visible = false;
        btnHexNext.Visible = false;
        lblHexPage.Visible = false;
    }

    private void ResetFileViewer()
    {
        StopMediaViewer(clearSource: true);
        Image? previous = pictureFileViewer.Image;
        pictureFileViewer.Image = null;
        previous?.Dispose();
        pictureFileViewer.Visible = false;
        txtFileViewer.Clear();
        txtFileViewer.Visible = false;
        txtHexViewer.Clear();
        txtHexViewer.Visible = false;
        mediaFileHost.Visible = false;
        HideViewerCommandButtons();
        btnMediaLoad.Visible = false;
        btnMediaPlay.Visible = false;
        btnMediaPause.Visible = false;
        btnMediaStop.Visible = false;
        btnHexPrevious.Visible = false;
        btnHexNext.Visible = false;
        lblHexPage.Visible = false;
        lblFileViewerInfo.Text = "Select a file to preview it.";
        _filePreviewEntry = null;
        _filePreviewInfo = null;
        _hexPreviewOffset = 0;
    }

    private static bool IsImageExtension(string extension) => extension is
        ".dds" or ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff" or ".ico";

    private static bool IsTextExtension(string extension) => extension is
        ".json" or ".txt" or ".xml" or ".ini" or ".cfg" or ".conf" or ".log" or ".csv" or ".yaml" or ".yml" or
        ".md" or ".toml" or ".manifest" or ".html" or ".htm" or ".css" or ".js" or ".cs" or ".cpp" or
        ".c" or ".h" or ".hpp" or ".lua" or ".py" or ".ps1" or ".bat" or ".cmd" or ".sh" or ".sql";

    private static bool LooksLikeText(string text)
    {
        if (text.Length == 0) return true;
        int invalidControls = text.Count(character => char.IsControl(character) &&
            character is not '\r' and not '\n' and not '\t' and not '\f' and not '\b');
        return invalidControls <= Math.Max(1, text.Length / 100);
    }

    private static bool IsMediaExtension(string extension) => extension is
        ".mp3" or ".wav" or ".wma" or ".aac" or ".m4a" or ".flac" or ".mp4" or ".m4v" or ".mov" or ".avi" or
        ".wmv" or ".mpeg" or ".mpg" or ".webm" or ".mkv" or ".ogg" or ".oga" or ".opus" or ".3gp" or
        ".ts" or ".m2ts" or ".vob";

    private static bool IsBrowsableFilesystem(Ps5SourceKind sourceKind) =>
        sourceKind is Ps5SourceKind.Ffpfsc or Ps5SourceKind.FilesystemImage or Ps5SourceKind.Ffpkg or Ps5SourceKind.SonyPackage;

    private async void btnHexPrevious_Click(object? sender, EventArgs e)
    {
        long offset = Math.Max(0, _hexPreviewOffset - HexPreviewPageSizeBytes);
        await NavigateHexViewerAsync(offset);
    }

    private async void btnHexNext_Click(object? sender, EventArgs e)
    {
        long offset = checked(_hexPreviewOffset + HexPreviewPageSizeBytes);
        await NavigateHexViewerAsync(offset);
    }

    private async Task NavigateHexViewerAsync(long offset)
    {
        _filePreviewCancellation?.Cancel();
        _filePreviewCancellation?.Dispose();
        _filePreviewCancellation = new CancellationTokenSource();
        int version = _filePreviewVersion;
        try
        {
            await LoadHexViewerAsync(offset, version, _filePreviewCancellation.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or ArgumentException)
        {
            if (version == _filePreviewVersion)
                lblFileViewerInfo.Text = $"Hex preview unavailable: {ex.Message}";
        }
    }

    private async void btnMediaLoad_Click(object? sender, EventArgs e)
    {
        Ps5GameInfo? game = _selectedGame;
        FileBrowserEntry? entry = _filePreviewEntry;
        Ps5FileInfo? file = _filePreviewInfo;
        if (game is null || entry is null || file is null) return;
        int version = _filePreviewVersion;
        string mediaPath;
        if (game.SourceKind == Ps5SourceKind.LooseDump)
        {
            mediaPath = Path.GetFullPath(Path.Combine(game.RootPath,
                entry.RelativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)));
            string rootPrefix = Path.GetFullPath(game.RootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!mediaPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) || !File.Exists(mediaPath)) return;
        }
        else if (IsBrowsableFilesystem(game.SourceKind))
        {
            Directory.CreateDirectory(_previewDirectory);
            mediaPath = Path.Combine(_previewDirectory,
                Guid.NewGuid().ToString("N") + "_" + MakeSafeFileName(Path.GetFileName(entry.RelativePath)));
            if (!await ExtractContainedFileAsync(entry.RelativePath, file.Size, mediaPath, ExtractedFileAction.None)) return;
        }
        else return;

        if (version != _filePreviewVersion || !ReferenceEquals(game, _selectedGame) ||
            !string.Equals(_filePreviewEntry?.RelativePath, entry.RelativePath, StringComparison.OrdinalIgnoreCase))
        {
            if (IsBrowsableFilesystem(game.SourceKind))
            {
                try { File.Delete(mediaPath); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            return;
        }

        mediaFileViewer.Source = new Uri(mediaPath, UriKind.Absolute);
        mediaFileHost.Visible = true;
        mediaFileHost.BringToFront();
        btnMediaLoad.Visible = false;
        btnMediaPlay.Visible = true;
        btnMediaPause.Visible = true;
        btnMediaStop.Visible = true;
        MediaControl(viewer => viewer.Play());
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Playing  •  {FormatBytes(file.Size)}";
    }

    private void btnMediaPlay_Click(object? sender, EventArgs e) => MediaControl(viewer => viewer.Play());
    private void btnMediaPause_Click(object? sender, EventArgs e) => MediaControl(viewer => viewer.Pause());
    private void btnMediaStop_Click(object? sender, EventArgs e) => MediaControl(viewer => viewer.Stop());

    private void MediaControl(Action<System.Windows.Controls.MediaElement> action)
    {
        try
        {
            action(mediaFileViewer);
        }
        catch (InvalidOperationException) { }
    }

    private void mediaFileViewer_MediaFailed(object? sender, System.Windows.ExceptionRoutedEventArgs e)
    {
        statusLabel.Text = "The selected media format is not supported by the installed Windows codecs.";
        Logger.Warn($"Media preview failed: {e.ErrorException?.Message}");
    }

    private void StopMediaViewer(bool clearSource)
    {
        try
        {
            mediaFileViewer.Stop();
            if (clearSource) mediaFileViewer.Source = null;
        }
        catch (InvalidOperationException) { }
    }

    private async void listFiles_ItemActivate(object? sender, EventArgs e)
    {
        if (listFiles.SelectedItems.Count == 0 || listFiles.SelectedItems[0].Tag is not FileBrowserEntry entry) return;
        if (entry.IsDirectory)
        {
            if (_directoryNodes.TryGetValue(entry.RelativePath, out TreeNode? node))
            {
                treeFiles.SelectedNode = node;
                node.Expand();
            }
            return;
        }

        if (_selectedGame is { } selectedGame && IsBrowsableFilesystem(selectedGame.SourceKind))
        {
            await PreviewSelectedFileAsync();
            return;
        }

        if (_currentSourceIsContainer)
        {
            RevealInExplorer(_currentGameRoot);
            return;
        }

        string fullPath = Path.GetFullPath(Path.Combine(_currentGameRoot, entry.RelativePath));
        if (!File.Exists(fullPath)) return;
        RevealInExplorer(fullPath);
    }

    private void listFiles_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;
        ListViewItem? item = listFiles.GetItemAt(e.X, e.Y);
        if (item is null || item.Selected) return;
        listFiles.SelectedItems.Clear();
        item.Selected = true;
    }

    private void contextFiles_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        bool hasFile = TryGetSelectedFile(out _, out _);
        bool isBrowsableFilesystem = _selectedGame is { } game && IsBrowsableFilesystem(game.SourceKind);
        bool isContainer = _selectedGame?.SourceKind is Ps5SourceKind.Ffpfsc or Ps5SourceKind.FilesystemImage or
            Ps5SourceKind.Ffpkg or Ps5SourceKind.SonyPackage;
        menuFileOpenContained.Enabled = hasFile && isBrowsableFilesystem && !_isFileBusy;
        menuFileExtractContained.Enabled = hasFile && isBrowsableFilesystem && !_isFileBusy;
        menuFileCopyPath.Enabled = hasFile;
        menuFileCopyName.Enabled = hasFile;
        menuFileCopySeparator.Visible = hasFile;
        menuFileRevealContainer.Enabled = isContainer;
        menuFileContainerSeparator.Visible = isContainer;
        e.Cancel = !hasFile && !isContainer;
    }

    private async void menuFileOpenContained_Click(object? sender, EventArgs e) =>
        await PreviewSelectedFileAsync();

    private async void menuFileExtractContained_Click(object? sender, EventArgs e)
    {
        if (!TryGetSelectedFile(out FileBrowserEntry entry, out Ps5FileInfo file) ||
            _selectedGame is not { } selectedGame || !IsBrowsableFilesystem(selectedGame.SourceKind)) return;
        containedFileSaveDialog.FileName = Path.GetFileName(entry.RelativePath);
        containedFileSaveDialog.DefaultExt = Path.GetExtension(entry.RelativePath).TrimStart('.');
        if (containedFileSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        await ExtractContainedFileAsync(entry.RelativePath, file.Size, containedFileSaveDialog.FileName,
            ExtractedFileAction.Reveal);
    }

    private void menuFileRevealContainer_Click(object? sender, EventArgs e)
    {
        if (_selectedGame?.SourceKind is Ps5SourceKind.Ffpfsc or Ps5SourceKind.FilesystemImage or
            Ps5SourceKind.Ffpkg or Ps5SourceKind.SonyPackage)
            RevealInExplorer(_selectedGame.RootPath);
    }

    private async Task PreviewSelectedFileAsync()
    {
        if (!TryGetSelectedFile(out FileBrowserEntry entry, out Ps5FileInfo file) ||
            _selectedGame is not { } selectedGame || !IsBrowsableFilesystem(selectedGame.SourceKind)) return;
        Directory.CreateDirectory(_previewDirectory);
        string safeName = MakeSafeFileName(Path.GetFileName(entry.RelativePath));
        string previewPath = Path.Combine(_previewDirectory, Guid.NewGuid().ToString("N") + "_" + safeName);
        await ExtractContainedFileAsync(entry.RelativePath, file.Size, previewPath, ExtractedFileAction.Open);
    }

    private async Task<bool> ExtractContainedFileAsync(string relativePath, long totalBytes, string outputPath,
        ExtractedFileAction action)
    {
        Ps5GameInfo? game = _selectedGame;
        if (game is null || !IsBrowsableFilesystem(game.SourceKind) || _isFileBusy) return false;
        _fileCancellation?.Cancel();
        _fileCancellation?.Dispose();
        _fileCancellation = new CancellationTokenSource();
        _isFileBusy = true;
        UpdateOperationState();
        var progress = new Progress<long>(copied =>
        {
            double percentage = totalBytes == 0 ? 100 : copied * 100.0 / totalBytes;
            statusLabel.Text = $"Extracting {Path.GetFileName(relativePath)}: {percentage:N1}% " +
                               $"({FormatBytes(copied)} / {FormatBytes(totalBytes)})";
        });
        try
        {
            await Task.Run(() => GameFileSystem.ExtractFileAsync(game, relativePath, outputPath, progress,
                _fileCancellation.Token), _fileCancellation.Token);
            statusLabel.Text = $"Extracted {relativePath}.";
            if (action == ExtractedFileAction.Open)
                await OpenExtractedFileAsync(outputPath, Path.GetFileName(relativePath));
            else if (action == ExtractedFileAction.Reveal)
                RevealInExplorer(outputPath);
            return true;
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "File extraction cancelled; partial output was removed.";
            return false;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or System.ComponentModel.Win32Exception)
        {
            statusLabel.Text = "Unable to extract or open the selected file.";
            AppDialog.ShowError(ex.Message, "Filesystem image browser");
            return false;
        }
        finally
        {
            _isFileBusy = false;
            UpdateOperationState();
        }
    }

    private async Task OpenExtractedFileAsync(string path, string displayName)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        string title = string.IsNullOrWhiteSpace(displayName) ? Path.GetFileName(path) : displayName;
        if (extension == ".dds")
        {
            byte[] png = await Task.Run(() =>
            {
                using FileStream input = File.OpenRead(path);
                return Ps5ImageCodec.DecodeDdsToPng(input);
            });
            FilePreviewForm.ShowImage(this, title, png);
            return;
        }

        if (extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tif" or ".tiff" or ".ico")
        {
            long maximumImageBytes = Math.Max(1, _settings.MaxPreviewMb) * 1024L * 1024L;
            if (new FileInfo(path).Length <= maximumImageBytes)
            {
                byte[] image = await File.ReadAllBytesAsync(path);
                FilePreviewForm.ShowImage(this, title, image);
                return;
            }
        }

        if (extension is ".json" or ".txt" or ".xml" or ".ini" or ".cfg" or ".log" or ".csv" or ".yaml" or ".yml")
        {
            long maximumTextBytes = Math.Max(1, _settings.MaxPreviewMb) * 1024L * 1024L;
            if (new FileInfo(path).Length <= maximumTextBytes)
            {
                string text = await File.ReadAllTextAsync(path);
                if (extension == ".json") text = PrettyJson(text);
                FilePreviewForm.ShowText(this, title, text);
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            RevealInExplorer(path);
            AppDialog.ShowWarning(
                $"Windows has no application associated with {extension.ToUpperInvariant()} files.\n\n" +
                "The file was extracted successfully and selected in File Explorer.",
                "No preview application");
        }
    }

    private bool TryGetSelectedFile(out FileBrowserEntry entry, out Ps5FileInfo file)
    {
        FileBrowserEntry? selected = listFiles.SelectedItems.Count > 0
            ? listFiles.SelectedItems[0].Tag as FileBrowserEntry
            : null;
        if (selected is null || selected.IsDirectory)
        {
            entry = null!;
            file = null!;
            return false;
        }
        string relativePath = selected.RelativePath;
        Ps5FileInfo? found = _filesByDirectory.Values.SelectMany(files => files).FirstOrDefault(candidate =>
            candidate.RelativePath.Equals(relativePath, StringComparison.OrdinalIgnoreCase));
        if (found is null)
        {
            entry = null!;
            file = null!;
            return false;
        }
        entry = selected;
        file = found;
        return true;
    }

    private void PopulateExecutable(Ps5SelfInfo? executable)
    {
        _selfInfo = executable;
        searchExecutable.SearchText = string.Empty;
        if (executable is null)
        {
            lblExecutableSummary.Text = "eboot.bin was not found.";
            gridModules.DataSource = null;
            gridElfPrograms.DataSource = null;
            gridElfSections.DataSource = null;
            gridSelfHeader.DataSource = null;
            gridSelfSegments.DataSource = null;
            return;
        }

        var summary = new StringBuilder();
        summary.Append($"SELF {executable.SelfMagic} - {FormatBytes(executable.FileSize)} - embedded ELF at 0x{executable.ElfOffset:X} - ");
        summary.Append($"x86-64 machine 0x{executable.Machine:X4} - entry 0x{executable.EntryPoint:X} - ");
        summary.Append($"{executable.ProgramHeaderCount} program headers - {executable.SectionHeaderCount} sections - {executable.Modules.Count} modules");
        if (executable.SelfHeaderSize > 0)
            summary.Append($"  |  SELF v{executable.SelfVersion}, type 0x{executable.SelfProgramType:X8}, header 0x{executable.SelfHeaderSize:X}, " +
                           $"meta 0x{executable.SelfMetadataSize:X}, segments {executable.SelfSegmentCount}, flags 0x{executable.SelfFlags:X}");
        lblExecutableSummary.Text = summary.ToString();

        RebuildExecutableTables();
    }

    private void searchExecutable_SearchTextChanged(object? sender, EventArgs e) => RebuildExecutableTables();

    private void tabsExecutable_SelectedIndexChanged(object? sender, EventArgs e) => RebuildExecutableTables();

    private void RebuildExecutableTables()
    {
        if (_selfInfo is not { } executable) return;
        string filter = searchExecutable.SearchText.Trim();

        var modules = new DataTable();
        modules.Columns.Add("Module");
        modules.Columns.Add("Kind");
        modules.Columns.Add("Size");
        modules.Columns.Add("Path");
        foreach (Ps5ModuleInfo module in executable.Modules)
            if (Matches(filter, module.Name, module.Kind, module.RelativePath))
                modules.Rows.Add(module.Name, module.Kind, FormatBytes(module.Size), module.RelativePath);
        gridModules.DataSource = modules;

        var programs = new DataTable();
        programs.Columns.Add("Idx", typeof(int));
        programs.Columns.Add("Type");
        programs.Columns.Add("Flags");
        programs.Columns.Add("Offset");
        programs.Columns.Add("VAddr");
        programs.Columns.Add("PAddr");
        programs.Columns.Add("File size");
        programs.Columns.Add("Mem size");
        programs.Columns.Add("Align");
        foreach (Ps5ElfProgramHeader program in executable.ProgramHeaders)
            programs.Rows.Add(program.Index, ProgramTypeName(program.Type), $"0x{program.Flags:X}", $"0x{program.Offset:X}",
                $"0x{program.VirtualAddress:X}", $"0x{program.PhysicalAddress:X}", FormatBytes(program.FileSize),
                FormatBytes(program.MemorySize), $"0x{program.Align:X}");
        gridElfPrograms.DataSource = programs;

        var sections = new DataTable();
        sections.Columns.Add("Idx", typeof(int));
        sections.Columns.Add("Name");
        sections.Columns.Add("Type");
        sections.Columns.Add("Flags");
        sections.Columns.Add("Address");
        sections.Columns.Add("Offset");
        sections.Columns.Add("Size");
        foreach (Ps5ElfSectionHeader section in executable.SectionHeaders)
            sections.Rows.Add(section.Index, $"0x{section.Name:X8}", $"0x{section.Type:X8}", $"0x{section.Flags:X}",
                $"0x{section.Address:X}", $"0x{section.Offset:X}", FormatBytes(section.Size));
        gridElfSections.DataSource = sections;

        var header = new DataTable();
        header.Columns.Add("Property");
        header.Columns.Add("Value");
        void Add(string property, string value)
        {
            if (!string.IsNullOrEmpty(value)) header.Rows.Add(property, value);
        }
        Add("SELF magic", executable.SelfMagic);
        Add("SELF version", executable.SelfVersion.ToString());
        Add("Program type", $"0x{executable.SelfProgramType:X8}");
        Add("Header size", $"0x{executable.SelfHeaderSize:X}");
        Add("Metadata size", $"0x{executable.SelfMetadataSize:X}");
        Add("Declared file size", executable.SelfDeclaredFileSize > 0 ? FormatBytes((long)executable.SelfDeclaredFileSize) : string.Empty);
        Add("Segment count", executable.SelfSegmentCount.ToString());
        Add("Flags", $"0x{executable.SelfFlags:X}");
        Add("ELF offset", $"0x{executable.ElfOffset:X}");
        Add("ELF class", executable.ElfClass == 2 ? "ELF64" : executable.ElfClass.ToString());
        Add("Machine", $"0x{executable.Machine:X4}");
        Add("Entry point", $"0x{executable.EntryPoint:X}");
        Add("Program headers", executable.ProgramHeaderCount.ToString());
        Add("Sections", executable.SectionHeaderCount.ToString());
        Add("File size", FormatBytes(executable.FileSize));
        gridSelfHeader.DataSource = header;

        var segments = new DataTable();
        segments.Columns.Add("Idx", typeof(int));
        segments.Columns.Add("Flags");
        segments.Columns.Add("File offset");
        segments.Columns.Add("File size");
        segments.Columns.Add("Mem size");
        foreach (Ps5SelfSegment segment in executable.SelfSegments)
            segments.Rows.Add(segment.Index, $"0x{segment.Flags:X}", $"0x{segment.FileOffset:X}",
                FormatBytes(segment.FileSize), FormatBytes(segment.MemorySize));
        gridSelfSegments.DataSource = segments;
    }

    private static string ProgramTypeName(uint type) => type switch
    {
        0 => "PT_NULL",
        1 => "PT_LOAD",
        2 => "PT_DYNAMIC",
        3 => "PT_INTERP",
        4 => "PT_NOTE",
        5 => "PT_SHLIB",
        6 => "PT_PHDR",
        7 => "PT_TLS",
        _ => $"0x{type:X}"
    };

    private void btnExecExtract_Click(object? sender, EventArgs e)
    {
        if (_selfInfo is not { } executable)
        {
            AppDialog.ShowInformation("No executable information is loaded.", "Executable");
            return;
        }

        var files = new List<(string RelativePath, long Size)>
        {
            ("eboot.bin", executable.FileSize)
        };
        foreach (Ps5ModuleInfo module in executable.Modules)
            if (!string.IsNullOrWhiteSpace(module.RelativePath))
                files.Add((module.RelativePath, module.Size));

        folderBrowserDialog.Description = "Select a folder for eboot.bin and the modules";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        _ = ExtractFilesAsync(files, folderBrowserDialog.SelectedPath);
    }

    private void btnExecCopyAll_Click(object? sender, EventArgs e) => CopyActiveExecGrid(copyAll: true);

    private void btnExecCopySelected_Click(object? sender, EventArgs e) => CopyActiveExecGrid(copyAll: false);

    private void CopyActiveExecGrid(bool copyAll)
    {
        IReadOnlyList<DarkDataGridView> grids = tabsExecutable.SelectedTab == tabExecElf
            ? [gridElfPrograms, gridElfSections]
            : tabsExecutable.SelectedTab == tabExecSelf
                ? [gridSelfHeader, gridSelfSegments]
                : [gridModules];

        var builder = new StringBuilder();
        foreach (DarkDataGridView grid in grids)
        {
            IEnumerable<DataGridViewRow> source = copyAll
                ? grid.Rows.Cast<DataGridViewRow>()
                : grid.SelectedRows.Cast<DataGridViewRow>();
            foreach (DataGridViewRow row in source)
            {
                if (row.DataBoundItem is not DataRowView view) continue;
                foreach (DataColumn column in view.DataView.Table.Columns)
                    builder.Append(column.ColumnName).Append(": ").Append(view.Row[column]?.ToString() ?? string.Empty).AppendLine();
                builder.AppendLine();
            }
        }

        if (builder.Length == 0)
        {
            AppDialog.ShowInformation("Nothing to copy.", "Executable");
            return;
        }
        CopyText(builder.ToString().TrimEnd());
    }

    private void ClearDetails()
    {
        Interlocked.Increment(ref _detailVersion);
        _detailCancellation?.Cancel();
        gridOverview.DataSource = null;
        txtRawMetadata.Clear();
        ClearDeepDetails();
    }

    private void ClearDeepDetails()
    {
        _filePreviewCancellation?.Cancel();
        _filePreviewCancellation?.Dispose();
        _filePreviewCancellation = null;
        Interlocked.Increment(ref _filePreviewVersion);
        ResetFileViewer();
        ReplaceImage(pictureIcon, null);
        ReplaceImage(pictureBackground0, null);
        ReplaceImage(pictureBackground1, null);
        ReplaceImage(pictureBackground2, null);
        gridTrophies.DataSource = null;
        _trophyView = null;
        DisposeTrophyImages();
        _udsSummary = null;
        gridUdsEvents.DataSource = null;
        gridUdsEventProperties.DataSource = null;
        gridUdsStats.DataSource = null;
        gridUdsEnums.DataSource = null;
        gridUdsRules.DataSource = null;
        _currentGameRoot = string.Empty;
        _currentSourceIsContainer = false;
        _currentDetails = null;
        _currentArtwork = null;
        _currentDetailsRoot = string.Empty;
        _populatedDetailTabs.Clear();
        _directoryNodes.Clear();
        _filesByDirectory.Clear();
        _directorySizes.Clear();
        treeFiles.Nodes.Clear();
        listFiles.Items.Clear();
        searchFileFilter.SearchText = string.Empty;
        _selfInfo = null;
        gridModules.DataSource = null;
        gridElfPrograms.DataSource = null;
        gridElfSections.DataSource = null;
        gridSelfHeader.DataSource = null;
        gridSelfSegments.DataSource = null;
        gridPkgHeader.DataSource = null;
        gridPkgSegments.DataSource = null;
        gridPkgEntries.DataSource = null;
        gridParamSfo.DataSource = null;
        gridKeystone.DataSource = null;
        gridSi.DataSource = null;
        gridPlayGoChunks.DataSource = null;
        gridPlayGoScenarios.DataSource = null;
        gridPlayGoFiles.DataSource = null;
        lblPlayGoSummary.Text = "Select a game to inspect the PlayGo chunk map.";
        lblTrophySummary.Text = "Loading trophies...";
        lblActivitiesSummary.Text = "Loading activity definitions...";
        lblFilesSummary.Text = "Building file inventory...";
        lblExecutableSummary.Text = "Inspecting eboot.bin and modules...";
    }

    private static string PrettyJson(string raw)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException) { return raw; }
    }

    private static Image? ImageFromBytes(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0) return null;
        using var stream = new MemoryStream(bytes, writable: false);
        using Image source = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: true);
        return new Bitmap(source);
    }

    private static void ReplaceImage(PictureBox target, Ps5ImageData? data)
    {
        Image? previous = target.Image;
        target.Image = ToImage(data);
        previous?.Dispose();
    }

    private static Image? ToImage(Ps5ImageData? data)
    {
        if (data is null || data.Bytes.Length == 0) return null;
        return data.IsRgba ? BitmapFromRgba(data.Bytes, data.Width, data.Height) : ImageFromBytes(data.Bytes);
    }

    // Builds a GDI+ bitmap straight from raw RGBA pixels (GDI+ 32bppArgb expects BGRA byte order),
    // so a decoded DDS never has to be re-encoded to PNG and decoded again.
    private static Bitmap BitmapFromRgba(byte[] rgba, int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        BitmapData bits = bitmap.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            int rowBytes = width * 4;
            byte[] row = new byte[rowBytes];
            for (int y = 0; y < height; y++)
            {
                int source = y * rowBytes;
                for (int x = 0; x < rowBytes; x += 4)
                {
                    row[x] = rgba[source + x + 2];
                    row[x + 1] = rgba[source + x + 1];
                    row[x + 2] = rgba[source + x];
                    row[x + 3] = rgba[source + x + 3];
                }
                Marshal.Copy(row, 0, bits.Scan0 + y * bits.Stride, rowBytes);
            }
        }
        finally
        {
            bitmap.UnlockBits(bits);
        }
        return bitmap;
    }

    private void DisposeTrophyImages()
    {
        foreach (Image image in _trophyImages) image.Dispose();
        _trophyImages.Clear();
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        double value = Math.Max(0, bytes);
        int unit = 0;
        while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
        return $"{value:0.##} {units[unit]}";
    }

    private void gridLibrary_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= gridLibrary.Rows.Count) return;
        if (gridLibrary.Rows[e.RowIndex].Tag is not Ps5GameInfo game) return;
        if (game.SourceKind != Ps5SourceKind.LooseDump) RevealInExplorer(game.RootPath);
        else Process.Start(new ProcessStartInfo { FileName = game.RootPath, UseShellExecute = true });
    }

    private static void RevealInExplorer(string path) => Process.Start(new ProcessStartInfo
    {
        FileName = "explorer.exe",
        Arguments = $"/select,\"{path}\"",
        UseShellExecute = true
    });

    private static void OpenOutputFolder(string path)
    {
        try
        {
            string target = Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? path;
            if (Directory.Exists(target))
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{target}\"") { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException) { }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        ShutdownTaskQueue();
        ShutdownLog();
        CaptureLibraryColumnLayout();
        SaveSettingsQuietly();
        SaveWindowBounds();
        _scanCancellation?.Cancel();
        _detailCancellation?.Cancel();
        _fileCancellation?.Cancel();
        _filePreviewCancellation?.Cancel();
        StopMediaViewer(clearSource: true);
        ReplaceImage(pictureIcon, null);
        ReplaceImage(pictureBackground0, null);
        ReplaceImage(pictureBackground1, null);
        ReplaceImage(pictureBackground2, null);
        DisposeTrophyImages();
        _scanCancellation?.Dispose();
        _detailCancellation?.Dispose();
        _fileCancellation?.Dispose();
        _filePreviewCancellation?.Dispose();
        contextFiles.Dispose();
        try { if (Directory.Exists(_previewDirectory)) Directory.Delete(_previewDirectory, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private enum ExtractedFileAction
    {
        None,
        Open,
        Reveal
    }

    private sealed record FileBrowserEntry(string RelativePath, bool IsDirectory, long Size = 0);
    private sealed record FileListRow(string Name, string Type, string Path, string SizeText, long Size,
        FileBrowserEntry Entry, int Icon, bool IsDirectory);
}
