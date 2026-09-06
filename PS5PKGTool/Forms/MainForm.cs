using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DarkUI.Forms;
using PS5PKGTool.Core.Builders;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Parsers;
using PS5PKGTool.Core.Services;
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
    private readonly List<Image> _trophyImages = [];
    private AppSettings _settings = new();
    private List<Ps5GameInfo> _games = [];
    private List<Ps5GameInfo> _visibleGames = [];
    private CancellationTokenSource? _scanCancellation;
    private CancellationTokenSource? _detailCancellation;
    private CancellationTokenSource? _ffpfscCancellation;
    private CancellationTokenSource? _exfatCancellation;
    private CancellationTokenSource? _ffpkgCancellation;
    private CancellationTokenSource? _sonyPkgCancellation;
    private CancellationTokenSource? _fileCancellation;
    private CancellationTokenSource? _filePreviewCancellation;
    private Ps5GameInfo? _selectedGame;
    private string _lastFfpfscPath = string.Empty;
    private string _lastExfatPath = string.Empty;
    private string _lastFfpkgPath = string.Empty;
    private string _lastSonyPkgPath = string.Empty;
    private string _currentGameRoot = string.Empty;
    private readonly string _previewDirectory = Path.Combine(Path.GetTempPath(), "PS5PKGTool", "Preview",
        Environment.ProcessId.ToString());
    private bool _currentSourceIsContainer;
    private bool _isScanning;
    private bool _isFfpfscBusy;
    private bool _isExfatBusy;
    private bool _isFfpkgBusy;
    private bool _isSonyPkgBusy;
    private bool _isFileBusy;
    private int _detailVersion;
    private int _filePreviewVersion;
    private long _hexPreviewOffset;
    private FileBrowserEntry? _filePreviewEntry;
    private Ps5FileInfo? _filePreviewInfo;
    private const int HexPreviewPageSize = 16 * 1024;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        _settings = _stateStore.LoadSettings();
        _games = _stateStore.LoadManifest().Games
            .Where(game => !string.IsNullOrWhiteSpace(game.RootPath))
            .Where(game => game.SourceKind != Ps5SourceKind.FilesystemImage ||
                           Path.GetExtension(game.RootPath).Equals(".exfat", StringComparison.OrdinalIgnoreCase))
            .Where(game => game.SourceKind != Ps5SourceKind.Ffpkg ||
                           Path.GetExtension(game.RootPath).Equals(".ffpkg", StringComparison.OrdinalIgnoreCase))
            .OrderBy(game => game.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        ApplyFilter();
        statusLabel.Text = _games.Count == 0
            ? "Add a PS5 dump, PKG, FFPFSC, FFPKG, or exFAT library folder, then choose Refresh."
            : "Loaded cached library. Choose Refresh to rescan folders.";
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
        await ScanAsync(_settings.LibraryFolders, merge: false);
    }

    private async void OpenDump_Click(object? sender, EventArgs e)
    {
        folderBrowserDialog.Description = "Select the root of an unpacked PS5 game dump";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        await ScanAsync([folderBrowserDialog.SelectedPath], merge: true);
    }

    private async void OpenPackage_Click(object? sender, EventArgs e)
    {
        if (packageOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        await ScanAsync([packageOpenDialog.FileName], merge: true);
    }

    private async void Refresh_Click(object? sender, EventArgs e)
    {
        if (_settings.LibraryFolders.Count == 0)
        {
            DarkMessageBox.ShowInformation("Add at least one PS5 dump library folder in Settings.", "PS5 PKG Tool");
            return;
        }
        await ScanAsync(_settings.LibraryFolders, merge: false);
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
            Ps5ScanResult result = await _scanner.ScanAsync(folders, _settings.RecursiveScan, progress, _scanCancellation.Token);
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
                DarkMessageBox.ShowWarning(string.Join(Environment.NewLine, result.Errors.Take(12)), "PS5 scan warnings");
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Library refresh cancelled.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            statusLabel.Text = "Library refresh failed.";
            DarkMessageBox.ShowError(ex.Message, "PS5 library refresh");
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

    private void Cancel_Click(object? sender, EventArgs e)
    {
        _scanCancellation?.Cancel();
        _ffpfscCancellation?.Cancel();
        _exfatCancellation?.Cancel();
        _ffpkgCancellation?.Cancel();
        _sonyPkgCancellation?.Cancel();
        _ffpkgCancellation?.Cancel();
        _fileCancellation?.Cancel();
    }

    private async void BuildFfpfsc_Click(object? sender, EventArgs e)
    {
        if (sourceImageOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        ffpfscSaveDialog.FileName = Path.GetFileName(sourceImageOpenDialog.FileName) + ".ffpfsc";
        ffpfscSaveDialog.InitialDirectory = Path.GetDirectoryName(sourceImageOpenDialog.FileName);
        if (ffpfscSaveDialog.ShowDialog(this) != DialogResult.OK) return;

        string sourcePath = sourceImageOpenDialog.FileName;
        string outputPath = ffpfscSaveDialog.FileName;
        _ffpfscCancellation?.Dispose();
        _ffpfscCancellation = new CancellationTokenSource();
        SetFfpfscBusy(true);
        var progress = new Progress<FfpfscProgress>(value =>
        {
            double percent = value.TotalBytes == 0 ? 100 : value.BytesProcessed * 100.0 / value.TotalBytes;
            statusLabel.Text = $"{value.Stage} FFPFSC: {percent:N1}% ({FormatBytes(value.BytesProcessed)} / {FormatBytes(value.TotalBytes)})";
        });

        try
        {
            FfpfscBuildResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.CreateFromImageAsync(sourcePath, outputPath,
                    new FfpfscBuildOptions { OverwriteExisting = true }, progress, _ffpfscCancellation.Token),
                _ffpfscCancellation.Token);
            FfpfscVerificationResult verification = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(outputPath, sourcePath, progress, _ffpfscCancellation.Token),
                _ffpfscCancellation.Token);
            if (verification.SourceMatches != true)
                throw new InvalidDataException("The completed FFPFSC image did not match the source image.");

            statusLabel.Text = $"Created and verified {Path.GetFileName(outputPath)}.";
            DarkMessageBox.ShowInformation(
                $"Native FFPFSC image created and verified.\n\n" +
                $"Inner image: {result.InnerFileName}\n" +
                $"Source: {FormatBytes(result.SourceLength)}\n" +
                $"PFSC payload: {FormatBytes(result.PfscStoredLength)}\n" +
                $"Container: {FormatBytes(result.ContainerLength)}\n" +
                $"Payload saving: {result.PayloadSavingsPercent:N2}%\n" +
                $"SHA-256: {verification.DecodedSha256}",
                "FFPFSC conversion complete");
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "FFPFSC conversion cancelled; partial output was removed.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
        {
            statusLabel.Text = "FFPFSC conversion failed.";
            DarkMessageBox.ShowError(ex.Message, "FFPFSC conversion");
        }
        finally
        {
            SetFfpfscBusy(false);
        }
    }

    private async void BuildDumpFfpfsc_Click(object? sender, EventArgs e)
    {
        folderBrowserDialog.Description = "Select the root of an unpacked PS5 game dump";
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK) return;
        string sourceDirectory = folderBrowserDialog.SelectedPath;
        ffpfscSaveDialog.FileName = new DirectoryInfo(sourceDirectory).Name + ".ffpfsc";
        ffpfscSaveDialog.InitialDirectory = new DirectoryInfo(sourceDirectory).Parent?.FullName;
        if (ffpfscSaveDialog.ShowDialog(this) != DialogResult.OK) return;

        _ffpfscCancellation?.Dispose();
        _ffpfscCancellation = new CancellationTokenSource();
        SetFfpfscBusy(true);
        var progress = new Progress<FfpfscProgress>(value =>
        {
            double percent = value.TotalBytes == 0 ? 100 : value.BytesProcessed * 100.0 / value.TotalBytes;
            statusLabel.Text = $"{value.Stage}: {percent:N1}% ({FormatBytes(value.BytesProcessed)} / {FormatBytes(value.TotalBytes)})";
        });
        try
        {
            FfpfscBuildResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.CreateFromDirectoryAsync(sourceDirectory, ffpfscSaveDialog.FileName,
                    new FfpfscBuildOptions { OverwriteExisting = true }, progress: progress,
                    cancellationToken: _ffpfscCancellation.Token), _ffpfscCancellation.Token);
            FfpfscVerificationResult verification = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(ffpfscSaveDialog.FileName, progress: progress,
                    cancellationToken: _ffpfscCancellation.Token), _ffpfscCancellation.Token);
            statusLabel.Text = $"Created and verified {Path.GetFileName(ffpfscSaveDialog.FileName)}.";
            DarkMessageBox.ShowInformation(
                $"PS5 dump converted with the native single-pass writer.\n\n" +
                $"Inner image: {result.InnerFileName}\n" +
                $"Generated exFAT: {FormatBytes(result.SourceLength)}\n" +
                $"PFSC payload: {FormatBytes(result.PfscStoredLength)}\n" +
                $"Container: {FormatBytes(result.ContainerLength)}\n" +
                $"Payload saving: {result.PayloadSavingsPercent:N2}%\n" +
                $"Decoded SHA-256: {verification.DecodedSha256}",
                "Dump conversion complete");
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "Dump conversion cancelled; partial output was removed.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
        {
            statusLabel.Text = "Dump conversion failed.";
            DarkMessageBox.ShowError(ex.Message, "Dump to FFPFSC conversion");
        }
        finally
        {
            SetFfpfscBusy(false);
        }
    }

    private async void VerifyFfpfsc_Click(object? sender, EventArgs e)
    {
        if (ffpfscOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        _ffpfscCancellation?.Dispose();
        _ffpfscCancellation = new CancellationTokenSource();
        SetFfpfscBusy(true);
        var progress = new Progress<FfpfscProgress>(value =>
        {
            double percent = value.TotalBytes == 0 ? 100 : value.BytesProcessed * 100.0 / value.TotalBytes;
            statusLabel.Text = $"{value.Stage} FFPFSC: {percent:N1}%";
        });
        try
        {
            FfpfscVerificationResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(ffpfscOpenDialog.FileName, progress: progress,
                    cancellationToken: _ffpfscCancellation.Token), _ffpfscCancellation.Token);
            FfpfscInfo info = result.Info;
            statusLabel.Text = $"Verified {Path.GetFileName(ffpfscOpenDialog.FileName)}.";
            DarkMessageBox.ShowInformation(
                $"FFPFSC structure and every PFSC block are valid.\n\n" +
                $"Inner image: {info.InnerFileName}\n" +
                $"Logical size: {FormatBytes(info.LogicalLength)}\n" +
                $"Stored PFSC: {FormatBytes(info.StoredLength)}\n" +
                $"PFS blocks: {info.PfsBlockCount:N0} × {FormatBytes(info.PfsBlockSize)}\n" +
                $"PFSC blocks: {info.Pfsc.BlockCount:N0}\n" +
                $"Decoded SHA-256: {result.DecodedSha256}",
                "FFPFSC verification");
        }
        catch (OperationCanceledException)
        {
            statusLabel.Text = "FFPFSC verification cancelled.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
        {
            statusLabel.Text = "FFPFSC verification failed.";
            DarkMessageBox.ShowError(ex.Message, "FFPFSC verification");
        }
        finally
        {
            SetFfpfscBusy(false);
        }
    }

    private void SetFfpfscBusy(bool busy)
    {
        _isFfpfscBusy = busy;
        UpdateOperationState();
    }

    private void UpdateOperationState()
    {
        bool idle = !_isScanning && !_isFfpfscBusy && !_isExfatBusy && !_isFfpkgBusy && !_isSonyPkgBusy && !_isFileBusy;
        btnAddFolder.Enabled = idle;
        btnRefresh.Enabled = idle;
        btnSettings.Enabled = idle;
        btnCancel.Enabled = !idle;
        menuAddFolder.Enabled = idle;
        menuOpenDump.Enabled = idle;
        menuOpenPackage.Enabled = idle;
        menuRefresh.Enabled = idle;
        menuSettings.Enabled = idle;
        menuBuildDumpFfpfsc.Enabled = idle;
        menuBuildFfpfsc.Enabled = idle;
        menuVerifyFfpfsc.Enabled = idle;

        bool ffpfscIdle = idle;
        btnFfpfscBrowseOutput.Enabled = ffpfscIdle;
        txtFfpfscOutput.Enabled = ffpfscIdle;
        nudFfpfscLevel.Enabled = ffpfscIdle;
        nudFfpfscGain.Enabled = ffpfscIdle;
        cboFfpfscCluster.Enabled = ffpfscIdle;
        chkFfpfscAmpr.Enabled = ffpfscIdle;
        btnFfpfscCreateSelected.Enabled = ffpfscIdle && _selectedGame?.SourceKind == Ps5SourceKind.LooseDump;
        btnFfpfscWrapImage.Enabled = ffpfscIdle;
        btnFfpfscVerify.Enabled = ffpfscIdle;
        btnFfpfscExtract.Enabled = ffpfscIdle;
        btnFfpfscCancel.Enabled = _isFfpfscBusy;

        bool ffpkgIdle = idle;
        txtFfpkgOutput.Enabled = ffpkgIdle;
        btnFfpkgBrowseOutput.Enabled = ffpkgIdle;
        btnFfpkgCreate.Enabled = ffpkgIdle && _selectedGame?.SourceKind == Ps5SourceKind.LooseDump;
        btnFfpkgVerify.Enabled = ffpkgIdle;
        btnFfpkgExtract.Enabled = ffpkgIdle;
        btnFfpkgEdit.Enabled = ffpkgIdle;
        btnFfpkgRebuild.Enabled = ffpkgIdle;
        btnFfpkgCancel.Enabled = _isFfpkgBusy;

        bool exfatIdle = idle;
        txtExfatOutput.Enabled = exfatIdle;
        btnExfatBrowseOutput.Enabled = exfatIdle;
        cboExfatCluster.Enabled = exfatIdle;
        chkExfatAmpr.Enabled = exfatIdle;
        btnExfatCreate.Enabled = exfatIdle && _selectedGame?.SourceKind == Ps5SourceKind.LooseDump;
        btnExfatVerify.Enabled = exfatIdle;
        btnExfatExtract.Enabled = exfatIdle;
        btnExfatRefreshAmpr.Enabled = exfatIdle;
        btnExfatEdit.Enabled = exfatIdle;
        btnExfatRepair.Enabled = exfatIdle;
        btnExfatCancel.Enabled = _isExfatBusy;

        bool sonyPkgIdle = idle;
        txtSonyPkgOutput.Enabled = sonyPkgIdle;
        btnSonyPkgBrowseOutput.Enabled = sonyPkgIdle;
        txtSonyPkgContentId.Enabled = sonyPkgIdle;
        chkSonyPkgCustomPasscode.Enabled = sonyPkgIdle;
        txtSonyPkgPasscode.Enabled = sonyPkgIdle && chkSonyPkgCustomPasscode.Checked;
        btnSonyPkgCreate.Enabled = sonyPkgIdle && _selectedGame?.SourceKind == Ps5SourceKind.LooseDump;
        btnSonyPkgVerify.Enabled = sonyPkgIdle;
        btnSonyPkgExtract.Enabled = sonyPkgIdle;
        btnSonyPkgAcceptance.Enabled = sonyPkgIdle;
        btnSonyPkgSplit.Enabled = sonyPkgIdle;
        btnSonyPkgMerge.Enabled = sonyPkgIdle;
        btnSonyPkgCancel.Enabled = _isSonyPkgBusy;
    }

    private void SetSelectedGame(Ps5GameInfo? game)
    {
        _selectedGame = game;
        if (game is null)
        {
            lblFfpfscSelectedGame.Text = "Select an unpacked game in the library above.";
            txtFfpfscOutput.Clear();
        }
        else if (game.SourceKind == Ps5SourceKind.LooseDump)
        {
            lblFfpfscSelectedGame.Text = $"Selected dump: {game.Title}  |  {game.TitleId}  |  {game.RootPath}";
            string outputName = MakeSafeFileName(string.IsNullOrWhiteSpace(game.TitleId)
                ? new DirectoryInfo(game.RootPath).Name
                : game.TitleId) + ".ffpfsc";
            string? parent = Directory.GetParent(game.RootPath)?.FullName;
            txtFfpfscOutput.Text = Path.Combine(parent ?? game.RootPath, outputName);
        }
        else if (game.SourceKind == Ps5SourceKind.SonyPackage)
        {
            lblFfpfscSelectedGame.Text =
                $"Selected Sony PKG: {game.Title}  |  Dump conversion is unavailable; retail PKG is not an FFPKG/exFAT image.";
            txtFfpfscOutput.Clear();
        }
        else if (game.SourceKind == Ps5SourceKind.FilesystemImage)
        {
            lblFfpfscSelectedGame.Text =
                $"Selected filesystem image: {game.Title}  |  Direct reading is available; use Wrap Image to create FFPFSC.";
            txtFfpfscOutput.Clear();
        }
        else if (game.SourceKind == Ps5SourceKind.Ffpkg)
        {
            lblFfpfscSelectedGame.Text =
                $"Selected FFPKG: {game.Title}  |  Direct UFS2 reading is available; use Wrap Image to create FFPFSC.";
            txtFfpfscOutput.Clear();
        }
        else
        {
            lblFfpfscSelectedGame.Text =
                $"Selected FFPFSC: {game.Title}  |  Direct metadata and filesystem reading is available below.";
            txtFfpfscOutput.Clear();
        }
        UpdateExfatSelection(game);
        UpdateFfpkgSelection(game);
        UpdateSonyPkgSelection(game);
        UpdateOperationState();
    }

    private void UpdateSonyPkgSelection(Ps5GameInfo? game)
    {
        if (game?.SourceKind != Ps5SourceKind.LooseDump)
        {
            lblSonyPkgSelectedGame.Text = game is null
                ? "Select an unpacked game in the library above."
                : $"Selected source: {game.Title}  |  Debug package creation requires an unpacked dump folder.";
            txtSonyPkgOutput.Clear();
            txtSonyPkgContentId.Clear();
            return;
        }
        lblSonyPkgSelectedGame.Text = $"Selected dump: {game.Title}  |  {game.TitleId}  |  {game.RootPath}";
        string outputName = MakeSafeFileName(string.IsNullOrWhiteSpace(game.TitleId)
            ? new DirectoryInfo(game.RootPath).Name : game.TitleId) + ".pkg";
        string? parent = Directory.GetParent(game.RootPath)?.FullName;
        txtSonyPkgOutput.Text = Path.Combine(parent ?? game.RootPath, outputName);
        txtSonyPkgContentId.Text = game.ContentId;
    }

    private void UpdateFfpkgSelection(Ps5GameInfo? game)
    {
        if (game?.SourceKind != Ps5SourceKind.LooseDump)
        {
            lblFfpkgSelectedGame.Text = game is null
                ? "Select an unpacked game in the library above."
                : game.SourceKind == Ps5SourceKind.Ffpkg
                    ? $"Selected FFPKG: {game.Title}  |  Direct metadata and filesystem reading is available."
                    : $"Selected source: {game.Title}  |  FFPKG creation requires an unpacked dump folder.";
            txtFfpkgOutput.Clear();
            return;
        }

        lblFfpkgSelectedGame.Text = $"Selected dump: {game.Title}  |  {game.TitleId}  |  {game.RootPath}";
        string outputName = MakeSafeFileName(string.IsNullOrWhiteSpace(game.TitleId)
            ? new DirectoryInfo(game.RootPath).Name
            : game.TitleId) + ".ffpkg";
        string? parent = Directory.GetParent(game.RootPath)?.FullName;
        txtFfpkgOutput.Text = Path.Combine(parent ?? game.RootPath, outputName);
    }

    private void UpdateExfatSelection(Ps5GameInfo? game)
    {
        if (game?.SourceKind != Ps5SourceKind.LooseDump)
        {
            lblExfatSelectedGame.Text = game is null
                ? "Select an unpacked game in the library above."
                : $"Selected source: {game.Title}  |  exFAT creation requires an unpacked dump folder.";
            txtExfatOutput.Clear();
            return;
        }

        lblExfatSelectedGame.Text = $"Selected dump: {game.Title}  |  {game.TitleId}  |  {game.RootPath}";
        string outputName = MakeSafeFileName(string.IsNullOrWhiteSpace(game.TitleId)
            ? new DirectoryInfo(game.RootPath).Name
            : game.TitleId) + ".exfat";
        string? parent = Directory.GetParent(game.RootPath)?.FullName;
        txtExfatOutput.Text = Path.Combine(parent ?? game.RootPath, outputName);
    }

    private static string MakeSafeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string result = new(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        result = result.Trim(' ', '.');
        return result.Length == 0 ? "PS5_GAME" : result;
    }

    private void btnFfpfscBrowseOutput_Click(object? sender, EventArgs e)
    {
        string current = txtFfpfscOutput.Text.Trim();
        if (current.Length > 0)
        {
            ffpfscSaveDialog.FileName = Path.GetFileName(current);
            ffpfscSaveDialog.InitialDirectory = Path.GetDirectoryName(current);
        }
        if (ffpfscSaveDialog.ShowDialog(this) == DialogResult.OK)
            txtFfpfscOutput.Text = ffpfscSaveDialog.FileName;
    }

    private async void btnFfpfscCreateSelected_Click(object? sender, EventArgs e)
    {
        Ps5GameInfo? game = _selectedGame;
        if (game?.SourceKind != Ps5SourceKind.LooseDump || !Directory.Exists(game.RootPath))
        {
            DarkMessageBox.ShowWarning("Select an unpacked PS5 game dump in the library first.", "Create FFPFSC");
            return;
        }
        string outputPath = txtFfpfscOutput.Text.Trim();
        if (outputPath.Length == 0)
        {
            btnFfpfscBrowseOutput_Click(sender, e);
            outputPath = txtFfpfscOutput.Text.Trim();
            if (outputPath.Length == 0) return;
        }
        if (!ConfirmFfpfscOverwrite(outputPath)) return;

        BeginFfpfscOperation("Preparing selected dump...");
        try
        {
            FfpfscBuildOptions buildOptions = CreateFfpfscBuildOptions();
            ExfatBuildOptions exfatOptions = CreateExfatBuildOptions();
            IProgress<FfpfscProgress> progress = CreateFfpfscProgress();
            FfpfscBuildResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.CreateFromDirectoryAsync(game.RootPath, outputPath, buildOptions, exfatOptions,
                    progress, _ffpfscCancellation!.Token), _ffpfscCancellation!.Token);
            FfpfscVerificationResult verification = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(outputPath, progress: progress,
                    cancellationToken: _ffpfscCancellation.Token), _ffpfscCancellation.Token);
            _lastFfpfscPath = Path.GetFullPath(outputPath);
            ShowFfpfscBuildResult(result, verification, "Selected dump conversion complete");
        }
        catch (OperationCanceledException)
        {
            SetFfpfscResult("Dump conversion cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpfscResult("Dump conversion failed.");
            DarkMessageBox.ShowError(ex.Message, "Dump to FFPFSC conversion");
        }
        finally
        {
            EndFfpfscOperation();
        }
    }

    private async void btnFfpfscWrapImage_Click(object? sender, EventArgs e)
    {
        if (sourceImageOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        ffpfscSaveDialog.FileName = Path.GetFileName(sourceImageOpenDialog.FileName) + ".ffpfsc";
        ffpfscSaveDialog.InitialDirectory = Path.GetDirectoryName(sourceImageOpenDialog.FileName);
        if (ffpfscSaveDialog.ShowDialog(this) != DialogResult.OK) return;

        string sourcePath = sourceImageOpenDialog.FileName;
        string outputPath = ffpfscSaveDialog.FileName;
        BeginFfpfscOperation("Preparing filesystem image...");
        try
        {
            FfpfscBuildOptions buildOptions = CreateFfpfscBuildOptions();
            IProgress<FfpfscProgress> progress = CreateFfpfscProgress();
            FfpfscBuildResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.CreateFromImageAsync(sourcePath, outputPath, buildOptions, progress,
                    _ffpfscCancellation!.Token), _ffpfscCancellation!.Token);
            FfpfscVerificationResult verification = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(outputPath, sourcePath, progress, _ffpfscCancellation.Token),
                _ffpfscCancellation.Token);
            if (verification.SourceMatches != true)
                throw new InvalidDataException("The completed FFPFSC image did not match the source image.");
            _lastFfpfscPath = Path.GetFullPath(outputPath);
            txtFfpfscOutput.Text = _lastFfpfscPath;
            ShowFfpfscBuildResult(result, verification, "Filesystem image conversion complete");
        }
        catch (OperationCanceledException)
        {
            SetFfpfscResult("Image conversion cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpfscResult("Image conversion failed.");
            DarkMessageBox.ShowError(ex.Message, "Image to FFPFSC conversion");
        }
        finally
        {
            EndFfpfscOperation();
        }
    }

    private async void btnFfpfscVerify_Click(object? sender, EventArgs e)
    {
        PrepareFfpfscOpenDialog();
        if (ffpfscOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = ffpfscOpenDialog.FileName;
        BeginFfpfscOperation("Inspecting FFPFSC structure...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateFfpfscProgress();
            FfpfscVerificationResult result = await RunFfpfscWorkerAsync(() =>
                FfpfscImage.VerifyAsync(inputPath, progress: progress,
                    cancellationToken: _ffpfscCancellation!.Token), _ffpfscCancellation!.Token);
            _lastFfpfscPath = Path.GetFullPath(inputPath);
            FfpfscInfo info = result.Info;
            string summary = $"Valid | {info.InnerFileName} | Logical {FormatBytes(info.LogicalLength)} | " +
                             $"Stored {FormatBytes(info.StoredLength)} | {info.Pfsc.BlockCount:N0} PFSC blocks";
            SetFfpfscResult(summary);
            DarkMessageBox.ShowInformation(
                $"FFPFSC structure and every PFSC block are valid.\n\n" +
                $"Inner image: {info.InnerFileName}\n" +
                $"Logical size: {FormatBytes(info.LogicalLength)}\n" +
                $"Stored PFSC: {FormatBytes(info.StoredLength)}\n" +
                $"PFS blocks: {info.PfsBlockCount:N0} x {FormatBytes(info.PfsBlockSize)}\n" +
                $"PFSC blocks: {info.Pfsc.BlockCount:N0}\n" +
                $"Decoded SHA-256: {result.DecodedSha256}",
                "FFPFSC verification");
        }
        catch (OperationCanceledException)
        {
            SetFfpfscResult("FFPFSC verification cancelled.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpfscResult("FFPFSC verification failed.");
            DarkMessageBox.ShowError(ex.Message, "FFPFSC verification");
        }
        finally
        {
            EndFfpfscOperation();
        }
    }

    private async void btnFfpfscExtract_Click(object? sender, EventArgs e)
    {
        PrepareFfpfscOpenDialog();
        if (ffpfscOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = ffpfscOpenDialog.FileName;
        FfpfscInfo info;
        try
        {
            using FileStream input = File.OpenRead(inputPath);
            info = FfpfscImage.Inspect(input);
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            DarkMessageBox.ShowError(ex.Message, "FFPFSC inspection");
            return;
        }

        innerImageSaveDialog.FileName = info.InnerFileName;
        innerImageSaveDialog.InitialDirectory = Path.GetDirectoryName(inputPath);
        if (innerImageSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        string destination = innerImageSaveDialog.FileName;
        string extractionPath = File.Exists(destination)
            ? destination + "." + Guid.NewGuid().ToString("N") + ".replacement"
            : destination;

        BeginFfpfscOperation("Extracting inner filesystem image...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateFfpfscProgress();
            await RunFfpfscWorkerActionAsync(() =>
                FfpfscImage.ExtractAsync(inputPath, extractionPath, progress, _ffpfscCancellation!.Token),
                _ffpfscCancellation!.Token);
            if (!string.Equals(extractionPath, destination, StringComparison.OrdinalIgnoreCase))
                File.Move(extractionPath, destination, true);
            _lastFfpfscPath = Path.GetFullPath(inputPath);
            SetFfpfscResult($"Extracted {info.InnerFileName} to {destination}");
            DarkMessageBox.ShowInformation(
                $"Inner image extracted successfully.\n\nFile: {destination}\nSize: {FormatBytes(info.LogicalLength)}",
                "FFPFSC extraction");
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(extractionPath);
            SetFfpfscResult("FFPFSC extraction cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            TryDeleteFile(extractionPath);
            SetFfpfscResult("FFPFSC extraction failed.");
            DarkMessageBox.ShowError(ex.Message, "FFPFSC extraction");
        }
        finally
        {
            EndFfpfscOperation();
        }
    }

    private void btnFfpfscCancel_Click(object? sender, EventArgs e) => _ffpfscCancellation?.Cancel();

    private static Task<TResult> RunFfpfscWorkerAsync<TResult>(Func<Task<TResult>> operation,
        CancellationToken cancellationToken) => Task.Run(operation, cancellationToken);

    private static Task RunFfpfscWorkerActionAsync(Func<Task> operation, CancellationToken cancellationToken) =>
        Task.Run(operation, cancellationToken);

    private bool ConfirmFfpfscOverwrite(string outputPath) =>
        !File.Exists(outputPath) || DarkMessageBox.ShowWarning(
            $"The output file already exists and will be replaced after the new image is complete.\n\n{outputPath}",
            "Replace FFPFSC image?", DarkDialogButton.YesNo) == DialogResult.Yes;

    private FfpfscBuildOptions CreateFfpfscBuildOptions() => new()
    {
        OverwriteExisting = true,
        Compression = new PfscCompressionOptions
        {
            CompressionLevel = decimal.ToInt32(nudFfpfscLevel.Value),
            MinimumGainPercent = decimal.ToInt32(nudFfpfscGain.Value)
        }
    };

    private ExfatBuildOptions CreateExfatBuildOptions() => new()
    {
        ClusterSize = cboFfpfscCluster.SelectedIndex switch
        {
            1 => 32 * 1024,
            2 => 64 * 1024,
            _ => null
        },
        GenerateAmprIndex = chkFfpfscAmpr.Checked
    };

    private void BeginFfpfscOperation(string message)
    {
        _ffpfscCancellation?.Cancel();
        _ffpfscCancellation?.Dispose();
        _ffpfscCancellation = new CancellationTokenSource();
        progressFfpfsc.Value = 0;
        lblFfpfscProgress.Text = message;
        statusLabel.Text = message;
        SetFfpfscBusy(true);
    }

    private void EndFfpfscOperation()
    {
        SetFfpfscBusy(false);
        _ffpfscCancellation?.Dispose();
        _ffpfscCancellation = null;
    }

    private IProgress<FfpfscProgress> CreateFfpfscProgress() => new Progress<FfpfscProgress>(value =>
    {
        double percentage = value.TotalBytes <= 0 ? 0 : value.BytesProcessed * 100.0 / value.TotalBytes;
        progressFfpfsc.Value = Math.Clamp((int)Math.Round(percentage), progressFfpfsc.Minimum, progressFfpfsc.Maximum);
        lblFfpfscProgress.Text = $"{value.Stage}: {percentage:N1}%  ({FormatBytes(value.BytesProcessed)} / {FormatBytes(value.TotalBytes)})";
        statusLabel.Text = lblFfpfscProgress.Text;
    });

    private void SetFfpfscResult(string message)
    {
        lblFfpfscProgress.Text = message;
        lblFfpfscOperationsInfo.Text = message;
        statusLabel.Text = message;
    }

    private void ShowFfpfscBuildResult(FfpfscBuildResult result, FfpfscVerificationResult verification, string title)
    {
        progressFfpfsc.Value = progressFfpfsc.Maximum;
        string summary = $"Created {Path.GetFileName(result.OutputPath)} | {result.PayloadSavingsPercent:N2}% payload saving | " +
                         $"{result.CompressedBlockCount:N0}/{result.PfscBlockCount:N0} blocks compressed";
        SetFfpfscResult(summary);
        DarkMessageBox.ShowInformation(
            $"Native FFPFSC image created and fully verified.\n\n" +
            $"Inner image: {result.InnerFileName}\n" +
            $"Logical image: {FormatBytes(result.SourceLength)}\n" +
            $"PFSC payload: {FormatBytes(result.PfscStoredLength)}\n" +
            $"Container: {FormatBytes(result.ContainerLength)}\n" +
            $"Compressed blocks: {result.CompressedBlockCount:N0} / {result.PfscBlockCount:N0}\n" +
            $"Payload saving: {result.PayloadSavingsPercent:N2}%\n" +
            $"Decoded SHA-256: {verification.DecodedSha256}", title);
    }

    private void PrepareFfpfscOpenDialog()
    {
        string candidate = File.Exists(_lastFfpfscPath) ? _lastFfpfscPath : txtFfpfscOutput.Text.Trim();
        if (!File.Exists(candidate)) return;
        ffpfscOpenDialog.FileName = Path.GetFileName(candidate);
        ffpfscOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate);
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

    private void btnFfpkgBrowseOutput_Click(object? sender, EventArgs e)
    {
        string current = txtFfpkgOutput.Text.Trim();
        if (current.Length > 0)
        {
            ffpkgSaveDialog.FileName = Path.GetFileName(current);
            ffpkgSaveDialog.InitialDirectory = Path.GetDirectoryName(current);
        }
        if (ffpkgSaveDialog.ShowDialog(this) == DialogResult.OK)
            txtFfpkgOutput.Text = ffpkgSaveDialog.FileName;
    }

    private async void btnFfpkgCreate_Click(object? sender, EventArgs e)
    {
        Ps5GameInfo? game = _selectedGame;
        if (game?.SourceKind != Ps5SourceKind.LooseDump || !Directory.Exists(game.RootPath))
        {
            DarkMessageBox.ShowWarning("Select an unpacked PS5 game dump in the library first.", "Create FFPKG");
            return;
        }
        string destination = txtFfpkgOutput.Text.Trim();
        if (destination.Length == 0)
        {
            btnFfpkgBrowseOutput_Click(sender, e);
            destination = txtFfpkgOutput.Text.Trim();
            if (destination.Length == 0) return;
        }
        destination = Path.GetFullPath(destination);
        if (IsPathInsideDirectory(destination, game.RootPath))
        {
            DarkMessageBox.ShowWarning("The output image must be outside the source game folder.",
                "Invalid FFPKG output");
            return;
        }
        bool replacing = File.Exists(destination);
        if (replacing && DarkMessageBox.ShowWarning(
                $"The existing output will be replaced only after the new FFPKG passes full verification.\n\n{destination}",
                "Replace FFPKG image?", DarkDialogButton.YesNo) != DialogResult.Yes) return;
        string workingPath = replacing
            ? destination + "." + Guid.NewGuid().ToString("N") + ".replacement"
            : destination;

        BeginFfpkgOperation("Creating PS5 UFS2 filesystem...");
        try
        {
            Ufs2VerificationResult result = await Ufs2Operations.CreateFromDirectoryAsync(game.RootPath,
                workingPath, game.TitleId, CreateFfpkgProgress(), _ffpkgCancellation!.Token);
            if (replacing) File.Move(workingPath, destination, true);
            _lastFfpkgPath = destination;
            txtFfpkgOutput.Text = destination;
            progressFfpkg.Value = progressFfpkg.Maximum;
            SetFfpkgResult($"Created, source-matched, and verified {result.FileCount:N0} files | {FormatBytes(result.ImageSize)}");
            ShowFfpkgVerification(result with { ImagePath = destination }, "FFPKG creation complete",
                sourceMatched: true);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(workingPath);
            SetFfpkgResult("FFPKG creation cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            TryDeleteFile(workingPath);
            SetFfpkgResult("FFPKG creation failed.");
            DarkMessageBox.ShowError(ex.Message, "FFPKG creation");
        }
        finally
        {
            EndFfpkgOperation();
        }
    }

    private async void btnFfpkgVerify_Click(object? sender, EventArgs e)
    {
        PrepareFfpkgOpenDialog();
        if (ffpkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = ffpkgOpenDialog.FileName;
        BeginFfpkgOperation("Inspecting UFS2 filesystem...");
        try
        {
            Ufs2VerificationResult result = await Ufs2Operations.VerifyAsync(inputPath, CreateFfpkgProgress(),
                _ffpkgCancellation!.Token);
            _lastFfpkgPath = Path.GetFullPath(inputPath);
            progressFfpkg.Value = progressFfpkg.Maximum;
            SetFfpkgResult($"Valid UFS2 | {result.FileCount:N0} files | {FormatBytes(result.ImageSize)}");
            ShowFfpkgVerification(result, "FFPKG verification");
        }
        catch (OperationCanceledException)
        {
            SetFfpkgResult("FFPKG verification cancelled.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpkgResult("FFPKG verification failed.");
            DarkMessageBox.ShowError(ex.Message, "FFPKG verification");
        }
        finally
        {
            EndFfpkgOperation();
        }
    }

    private async void btnFfpkgExtract_Click(object? sender, EventArgs e)
    {
        PrepareFfpkgOpenDialog();
        if (ffpkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        if (ffpkgExtractFolderDialog.ShowDialog(this) != DialogResult.OK) return;
        string baseName = MakeSafeFileName(Path.GetFileNameWithoutExtension(ffpkgOpenDialog.FileName));
        string destination = FindAvailableDirectory(Path.Combine(ffpkgExtractFolderDialog.SelectedPath, baseName));
        BeginFfpkgOperation("Extracting FFPKG files...");
        try
        {
            await Ufs2Operations.ExtractAsync(ffpkgOpenDialog.FileName, destination, CreateFfpkgProgress(),
                _ffpkgCancellation!.Token);
            _lastFfpkgPath = Path.GetFullPath(ffpkgOpenDialog.FileName);
            progressFfpkg.Value = progressFfpkg.Maximum;
            SetFfpkgResult($"Extracted files to {destination}");
            DarkMessageBox.ShowInformation($"The FFPKG filesystem was extracted successfully.\n\n{destination}",
                "FFPKG extraction");
        }
        catch (OperationCanceledException)
        {
            SetFfpkgResult("FFPKG extraction cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpkgResult("FFPKG extraction failed.");
            DarkMessageBox.ShowError(ex.Message, "FFPKG extraction");
        }
        finally
        {
            EndFfpkgOperation();
        }
    }

    private void btnFfpkgEdit_Click(object? sender, EventArgs e)
    {
        PrepareFfpkgOpenDialog();
        ffpkgOpenDialog.Title = "Select an FFPKG image to edit";
        if (ffpkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var editor = new FfpkgEditorForm(ffpkgOpenDialog.FileName);
            editor.ShowDialog(this);
            _lastFfpkgPath = Path.GetFullPath(ffpkgOpenDialog.FileName);
            statusLabel.Text = $"Closed FFPKG editor for {Path.GetFileName(_lastFfpkgPath)}.";
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            DarkMessageBox.ShowError(ex.Message, "Open FFPKG editor");
        }
    }

    private async void btnFfpkgRebuild_Click(object? sender, EventArgs e)
    {
        PrepareFfpkgOpenDialog();
        ffpkgOpenDialog.Title = "Select a readable FFPKG image to rebuild";
        if (ffpkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = ffpkgOpenDialog.FileName;
        if (DarkMessageBox.ShowWarning(
                "This extracts every readable file, rebuilds all UFS2 metadata beside the original, fully verifies " +
                "the replacement, and only then swaps it into place. It requires substantial free disk space and " +
                "cannot recover unreadable file data or a filesystem whose directory tree cannot be opened.\n\n" + inputPath,
                "Rebuild FFPKG image?", DarkDialogButton.YesNo) != DialogResult.Yes) return;
        BeginFfpkgOperation("Extracting FFPKG for verified rebuild...");
        try
        {
            Ufs2VerificationResult result = await Ufs2Operations.RebuildWithEditsAsync(inputPath,
                static _ => { }, CreateFfpkgProgress(), _ffpkgCancellation!.Token);
            _lastFfpkgPath = Path.GetFullPath(inputPath);
            progressFfpkg.Value = progressFfpkg.Maximum;
            SetFfpkgResult($"Rebuilt and verified {result.FileCount:N0} files.");
            ShowFfpkgVerification(result, "FFPKG rebuild complete");
        }
        catch (OperationCanceledException)
        {
            SetFfpkgResult("FFPKG rebuild cancelled; the original image was preserved.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetFfpkgResult("FFPKG rebuild failed; the original image was preserved.");
            DarkMessageBox.ShowError(ex.Message, "FFPKG rebuild");
        }
        finally
        {
            EndFfpkgOperation();
        }
    }

    private void btnFfpkgCancel_Click(object? sender, EventArgs e) => _ffpkgCancellation?.Cancel();

    private void BeginFfpkgOperation(string message)
    {
        _ffpkgCancellation?.Cancel();
        _ffpkgCancellation?.Dispose();
        _ffpkgCancellation = new CancellationTokenSource();
        progressFfpkg.Value = 0;
        lblFfpkgProgress.Text = message;
        statusLabel.Text = message;
        _isFfpkgBusy = true;
        UpdateOperationState();
    }

    private void EndFfpkgOperation()
    {
        _isFfpkgBusy = false;
        UpdateOperationState();
        _ffpkgCancellation?.Dispose();
        _ffpkgCancellation = null;
    }

    private IProgress<Ufs2Progress> CreateFfpkgProgress() => new Progress<Ufs2Progress>(value =>
    {
        if (value.TotalBytes <= 0)
        {
            progressFfpkg.Value = progressFfpkg.Minimum;
            lblFfpkgProgress.Text = value.Stage + "...";
            statusLabel.Text = lblFfpkgProgress.Text;
            return;
        }
        double percentage = value.BytesProcessed * 100.0 / value.TotalBytes;
        progressFfpkg.Value = Math.Clamp((int)Math.Round(percentage), progressFfpkg.Minimum, progressFfpkg.Maximum);
        string amounts = value.Unit.Equals("bytes", StringComparison.OrdinalIgnoreCase)
            ? $"{FormatBytes(value.Completed)} / {FormatBytes(value.Total)}"
            : $"{value.Completed:N0} / {value.Total:N0} {value.Unit}";
        lblFfpkgProgress.Text = $"{value.Stage}: {percentage:N1}%  ({amounts})";
        statusLabel.Text = lblFfpkgProgress.Text;
    });

    private void SetFfpkgResult(string message)
    {
        lblFfpkgProgress.Text = message;
        lblFfpkgOperationsInfo.Text = message;
        statusLabel.Text = message;
    }

    private void PrepareFfpkgOpenDialog()
    {
        ffpkgOpenDialog.Title = "Select an FFPKG image";
        string candidate = File.Exists(_lastFfpkgPath) ? _lastFfpkgPath : txtFfpkgOutput.Text.Trim();
        if (_selectedGame?.SourceKind == Ps5SourceKind.Ffpkg && File.Exists(_selectedGame.RootPath))
            candidate = _selectedGame.RootPath;
        if (!File.Exists(candidate)) return;
        ffpkgOpenDialog.FileName = Path.GetFileName(candidate);
        ffpkgOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate);
    }

    private void ShowFfpkgVerification(Ufs2VerificationResult result, string title, bool sourceMatched = false) =>
        DarkMessageBox.ShowInformation(
            $"The UFS2 structure is valid and every file was read successfully.\n\n" +
            (sourceMatched ? "Every source file and directory matches the finished FFPKG.\n\n" : string.Empty) +
            $"Image: {result.ImagePath}\n" +
            $"Image size: {FormatBytes(result.ImageSize)}\n" +
            $"Volume: {(string.IsNullOrWhiteSpace(result.VolumeName) ? "(none)" : result.VolumeName)}\n" +
            $"Block / fragment: {FormatBytes(result.BlockSize)} / {FormatBytes(result.FragmentSize)}\n" +
            $"Files: {result.FileCount:N0}\n" +
            $"Directories: {result.DirectoryCount:N0}\n" +
            $"Logical file data: {FormatBytes(result.LogicalFileBytes)}\n" +
            $"Filesystem warnings: {result.FsckWarnings:N0}\n" +
            $"Manifest SHA-256: {result.ManifestSha256}", title);

    private void btnSonyPkgBrowseOutput_Click(object? sender, EventArgs e)
    {
        string current = txtSonyPkgOutput.Text.Trim();
        if (current.Length > 0)
        {
            sonyPkgSaveDialog.FileName = Path.GetFileName(current);
            sonyPkgSaveDialog.InitialDirectory = Path.GetDirectoryName(current);
        }
        if (sonyPkgSaveDialog.ShowDialog(this) == DialogResult.OK)
            txtSonyPkgOutput.Text = sonyPkgSaveDialog.FileName;
    }

    private void chkSonyPkgCustomPasscode_CheckedChanged(object? sender, EventArgs e)
    {
        if (!chkSonyPkgCustomPasscode.Checked)
            txtSonyPkgPasscode.Text = SonyDebugPackageCredentials.DefaultPasscode;
        txtSonyPkgPasscode.Enabled = chkSonyPkgCustomPasscode.Checked && !_isSonyPkgBusy;
    }

    private async void btnSonyPkgCreate_Click(object? sender, EventArgs e)
    {
        Ps5GameInfo? game = _selectedGame;
        if (game?.SourceKind != Ps5SourceKind.LooseDump || !Directory.Exists(game.RootPath))
        {
            DarkMessageBox.ShowWarning("Select an unpacked PS5 game dump in the library first.", "Create PS5 debug PKG");
            return;
        }
        string destination = txtSonyPkgOutput.Text.Trim();
        if (destination.Length == 0)
        {
            btnSonyPkgBrowseOutput_Click(sender, e);
            destination = txtSonyPkgOutput.Text.Trim();
            if (destination.Length == 0) return;
        }
        destination = Path.GetFullPath(destination);
        if (IsPathInsideDirectory(destination, game.RootPath))
        {
            DarkMessageBox.ShowWarning("The output package must be outside the source game folder.",
                "Invalid package output");
            return;
        }
        if (File.Exists(destination) && DarkMessageBox.ShowWarning(
                "The existing package will be replaced only after the new package passes validation.\n\n" + destination,
                "Replace PS5 debug package?", DarkDialogButton.YesNo) != DialogResult.Yes) return;

        string passcode = chkSonyPkgCustomPasscode.Checked
            ? txtSonyPkgPasscode.Text : SonyDebugPackageCredentials.DefaultPasscode;
        BeginSonyPkgOperation("Creating encrypted PS5 debug package...");
        try
        {
            SonyDebugPackageBuildResult result = await SonyDebugPackageBuilder.CreateFromDirectoryAsync(
                game.RootPath, destination, new SonyDebugPackageBuildOptions
                {
                    ContentId = txtSonyPkgContentId.Text.Trim(),
                    Passcode = passcode
                }, CreateSonyPkgProgress(), _sonyPkgCancellation!.Token);
            _lastSonyPkgPath = destination;
            progressSonyPkg.Value = progressSonyPkg.Maximum;
            SetSonyPkgResult($"Created and verified {result.SourceFiles:N0} files | {FormatBytes(result.PackageSize)}");
            DarkMessageBox.ShowInformation(
                $"The debug package was created and reopened successfully.\n\n" +
                $"Package: {result.OutputPath}\n" +
                $"Content ID: {result.ContentId}\n" +
                $"Files: {result.SourceFiles:N0}\n" +
                $"Source data: {FormatBytes(result.SourceBytes)}\n" +
                $"Package size: {FormatBytes(result.PackageSize)}\n" +
                $"Passcode: {(result.UsesDefaultPasscode ? "32 zero default" : "custom")}\n" +
                $"Image key fingerprint: {result.KeyFingerprint}", "PS5 debug package complete");
        }
        catch (OperationCanceledException)
        {
            SetSonyPkgResult("Package creation cancelled. Partial temporary files were removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetSonyPkgResult("Package creation failed.");
            DarkMessageBox.ShowError(ex.Message, "PS5 debug package creation");
        }
        finally { EndSonyPkgOperation(); }
    }

    private void btnSonyPkgVerify_Click(object? sender, EventArgs e)
    {
        string candidate = File.Exists(_lastSonyPkgPath) ? _lastSonyPkgPath : txtSonyPkgOutput.Text.Trim();
        if (File.Exists(candidate))
        {
            sonyPkgOpenDialog.FileName = Path.GetFileName(candidate);
            sonyPkgOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate);
        }
        if (sonyPkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string passcode = chkSonyPkgCustomPasscode.Checked
            ? txtSonyPkgPasscode.Text : SonyDebugPackageCredentials.DefaultPasscode;
        BeginSonyPkgOperation("Validating encrypted PS5 debug package...");
        try
        {
            SonyDebugPackageValidationResult result = SonyDebugPackageBuilder.Validate(sonyPkgOpenDialog.FileName, passcode);
            _lastSonyPkgPath = sonyPkgOpenDialog.FileName;
            progressSonyPkg.Value = result.IsValid ? progressSonyPkg.Maximum : progressSonyPkg.Minimum;
            SetSonyPkgResult(result.IsValid
                ? $"Valid debug package | {result.IndexedFiles:N0} indexed files | {FormatBytes(result.PackageSize)}"
                : "Package validation failed: " + result.Message);
            if (result.IsValid)
                DarkMessageBox.ShowInformation(result.Message + $"\n\nIndexed files: {result.IndexedFiles:N0}\nPackage size: {FormatBytes(result.PackageSize)}",
                    "PS5 debug package verification");
            else DarkMessageBox.ShowError(result.Message, "PS5 debug package verification");
        }
        finally { EndSonyPkgOperation(); }
    }

    private async void btnSonyPkgExtract_Click(object? sender, EventArgs e)
    {
        string candidate = File.Exists(_lastSonyPkgPath) ? _lastSonyPkgPath : txtSonyPkgOutput.Text.Trim();
        if (File.Exists(candidate)) { sonyPkgOpenDialog.FileName = Path.GetFileName(candidate); sonyPkgOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate); }
        if (sonyPkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        sonyPkgExtractFolderDialog.Description = "Select a folder for PS5 debug package extraction";
        if (sonyPkgExtractFolderDialog.ShowDialog(this) != DialogResult.OK) return;
        string destination = FindAvailableDirectory(Path.Combine(sonyPkgExtractFolderDialog.SelectedPath,
            MakeSafeFileName(Path.GetFileNameWithoutExtension(sonyPkgOpenDialog.FileName))));
        string passcode = chkSonyPkgCustomPasscode.Checked ? txtSonyPkgPasscode.Text : SonyDebugPackageCredentials.DefaultPasscode;
        BeginSonyPkgOperation("Extracting PS5 debug package...");
        try
        {
            SonyPackageExtractResult result = await SonyPackageExtraction.ExtractAsync(sonyPkgOpenDialog.FileName,
                destination, passcode, new Progress<SonyPackageExtractProgress>(value =>
                {
                    double percent = value.TotalBytes == 0 ? 0 : value.CompletedBytes * 100d / value.TotalBytes;
                    progressSonyPkg.Value = Math.Clamp((int)Math.Round(percent), progressSonyPkg.Minimum, progressSonyPkg.Maximum);
                    lblSonyPkgProgress.Text = $"Extracting: {percent:N1}%  ({FormatBytes(value.CompletedBytes)} / {FormatBytes(value.TotalBytes)})";
                }), _sonyPkgCancellation!.Token);
            SetSonyPkgResult($"Extracted {result.FileCount:N0} files to {result.Destination}");
        }
        catch (OperationCanceledException) { SetSonyPkgResult("Package extraction cancelled; partial output was removed."); }
        catch (Exception ex) when (IsFfpfscOperationException(ex)) { SetSonyPkgResult("Package extraction failed."); DarkMessageBox.ShowError(ex.Message, "PS5 debug package extraction"); }
        finally { EndSonyPkgOperation(); }
    }

    private void btnSonyPkgAcceptance_Click(object? sender, EventArgs e)
    {
        string candidate = File.Exists(_lastSonyPkgPath) ? _lastSonyPkgPath : txtSonyPkgOutput.Text.Trim();
        if (File.Exists(candidate)) { sonyPkgOpenDialog.FileName = Path.GetFileName(candidate); sonyPkgOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate); }
        if (sonyPkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string passcode = chkSonyPkgCustomPasscode.Checked ? txtSonyPkgPasscode.Text : SonyDebugPackageCredentials.DefaultPasscode;
        SonyPackageAcceptanceReport report = SonyPackageAcceptanceValidator.Validate(sonyPkgOpenDialog.FileName, passcode);
        string details = string.Join(Environment.NewLine, report.Checks.Select(check => $"[{check.State}] {check.Name}: {check.Message}"));
        SetSonyPkgResult(report.IsStructurallyReady ? "Structural package acceptance checks passed." : "Structural package acceptance checks found failures.");
        DarkMessageBox.ShowInformation(details, "PS5 debug package acceptance check");
    }

    private async void btnSonyPkgSplit_Click(object? sender, EventArgs e)
    {
        string candidate = File.Exists(_lastSonyPkgPath) ? _lastSonyPkgPath : txtSonyPkgOutput.Text.Trim();
        if (File.Exists(candidate))
        {
            sonyPkgOpenDialog.FileName = Path.GetFileName(candidate);
            sonyPkgOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate);
        }
        if (sonyPkgOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        if (sonyPkgSplitFolderDialog.ShowDialog(this) != DialogResult.OK) return;
        if (Directory.EnumerateFileSystemEntries(sonyPkgSplitFolderDialog.SelectedPath).Any())
        {
            DarkMessageBox.ShowWarning("Select an empty output folder. This keeps a split set and its manifest together.",
                "Split package");
            return;
        }
        BeginSonyPkgOperation("Splitting package and calculating SHA-256 and CRC-32C checksums...");
        try
        {
            SonyPackageSplitResult result = await SonyPackageSplit.CreateAsync(sonyPkgOpenDialog.FileName,
                sonyPkgSplitFolderDialog.SelectedPath, progress: new Progress<SonyPackageSplitProgress>(value =>
                {
                    double percent = value.TotalBytes == 0 ? 0 : value.CompletedBytes * 100d / value.TotalBytes;
                    progressSonyPkg.Value = Math.Clamp((int)Math.Round(percent), progressSonyPkg.Minimum, progressSonyPkg.Maximum);
                    lblSonyPkgProgress.Text = $"Splitting: {percent:N1}%  ({FormatBytes(value.CompletedBytes)} / {FormatBytes(value.TotalBytes)})";
                }), cancellationToken: _sonyPkgCancellation!.Token);
            _lastSonyPkgPath = sonyPkgOpenDialog.FileName;
            progressSonyPkg.Value = progressSonyPkg.Maximum;
            SetSonyPkgResult($"Created verified split set: {result.Manifest.Pieces.Count:N0} pieces.");
            DarkMessageBox.ShowInformation(
                $"Created {result.Manifest.Pieces.Count:N0} verified package pieces.\n\nManifest: {result.ManifestPath}\n" +
                $"Original size: {FormatBytes(result.Manifest.SourceBytes)}\nSHA-256: {result.Manifest.SourceSha256}",
                "PS5 package split complete");
        }
        catch (OperationCanceledException) { SetSonyPkgResult("Package split cancelled; partial pieces were removed."); }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetSonyPkgResult("Package split failed.");
            DarkMessageBox.ShowError(ex.Message, "PS5 package split");
        }
        finally { EndSonyPkgOperation(); }
    }

    private async void btnSonyPkgMerge_Click(object? sender, EventArgs e)
    {
        if (sonyPkgSplitManifestOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string manifest = sonyPkgSplitManifestOpenDialog.FileName;
        string baseName = Path.GetFileNameWithoutExtension(manifest);
        if (baseName.EndsWith(".ps5split", StringComparison.OrdinalIgnoreCase))
            baseName = baseName[..^".ps5split".Length];
        sonyPkgSaveDialog.FileName = baseName + ".pkg";
        sonyPkgSaveDialog.InitialDirectory = Path.GetDirectoryName(manifest);
        sonyPkgSaveDialog.Title = "Save merged PS5 package";
        if (sonyPkgSaveDialog.ShowDialog(this) != DialogResult.OK) return;
        string destination = sonyPkgSaveDialog.FileName;
        if (File.Exists(destination) && DarkMessageBox.ShowWarning(
                "The merged package will replace this existing file after the split set has been fully verified.\n\n" + destination,
                "Replace merged package?", DarkDialogButton.YesNo) != DialogResult.Yes) return;
        BeginSonyPkgOperation("Verifying split manifest and package pieces...");
        try
        {
            await SonyPackageMerge.MergeManifestAtomicAsync(manifest, destination, _sonyPkgCancellation!.Token);
            _lastSonyPkgPath = destination;
            progressSonyPkg.Value = progressSonyPkg.Maximum;
            SetSonyPkgResult("Verified split set merged successfully.");
            DarkMessageBox.ShowInformation("Every split piece passed SHA-256 and CRC-32C verification before merging.\n\n" + destination,
                "PS5 package merge complete");
        }
        catch (OperationCanceledException) { SetSonyPkgResult("Package merge cancelled; partial output was removed."); }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetSonyPkgResult("Package merge failed; the original split set is unchanged.");
            DarkMessageBox.ShowError(ex.Message, "PS5 package merge");
        }
        finally
        {
            sonyPkgSaveDialog.Title = "Save PS5 debug package";
            EndSonyPkgOperation();
        }
    }

    private void btnSonyPkgCancel_Click(object? sender, EventArgs e) => _sonyPkgCancellation?.Cancel();

    private void BeginSonyPkgOperation(string message)
    {
        _sonyPkgCancellation?.Cancel();
        _sonyPkgCancellation?.Dispose();
        _sonyPkgCancellation = new CancellationTokenSource();
        progressSonyPkg.Value = 0;
        lblSonyPkgProgress.Text = message;
        statusLabel.Text = message;
        _isSonyPkgBusy = true;
        UpdateOperationState();
    }

    private void EndSonyPkgOperation()
    {
        _isSonyPkgBusy = false;
        UpdateOperationState();
        _sonyPkgCancellation?.Dispose();
        _sonyPkgCancellation = null;
    }

    private IProgress<SonyDebugPackageProgress> CreateSonyPkgProgress() =>
        new Progress<SonyDebugPackageProgress>(value =>
        {
            double percentage = value.TotalBytes <= 0 ? 0 : value.CompletedBytes * 100.0 / value.TotalBytes;
            progressSonyPkg.Value = Math.Clamp((int)Math.Round(percentage), progressSonyPkg.Minimum, progressSonyPkg.Maximum);
            lblSonyPkgProgress.Text = value.TotalBytes <= 0
                ? value.Stage
                : $"{value.Stage}: {percentage:N1}%  ({FormatBytes(value.CompletedBytes)} / {FormatBytes(value.TotalBytes)})";
            statusLabel.Text = lblSonyPkgProgress.Text;
        });

    private void SetSonyPkgResult(string message)
    {
        lblSonyPkgProgress.Text = message;
        lblSonyPkgOperationsInfo.Text = message;
        statusLabel.Text = message;
    }

    private void btnExfatBrowseOutput_Click(object? sender, EventArgs e)
    {
        string current = txtExfatOutput.Text.Trim();
        if (current.Length > 0)
        {
            exfatSaveDialog.FileName = Path.GetFileName(current);
            exfatSaveDialog.InitialDirectory = Path.GetDirectoryName(current);
        }
        if (exfatSaveDialog.ShowDialog(this) == DialogResult.OK)
            txtExfatOutput.Text = exfatSaveDialog.FileName;
    }

    private async void btnExfatCreate_Click(object? sender, EventArgs e)
    {
        Ps5GameInfo? game = _selectedGame;
        if (game?.SourceKind != Ps5SourceKind.LooseDump || !Directory.Exists(game.RootPath))
        {
            DarkMessageBox.ShowWarning("Select an unpacked PS5 game dump in the library first.", "Create exFAT");
            return;
        }
        string destination = txtExfatOutput.Text.Trim();
        if (destination.Length == 0)
        {
            btnExfatBrowseOutput_Click(sender, e);
            destination = txtExfatOutput.Text.Trim();
            if (destination.Length == 0) return;
        }
        destination = Path.GetFullPath(destination);
        if (IsPathInsideDirectory(destination, game.RootPath))
        {
            DarkMessageBox.ShowWarning(
                "The output image must be outside the source game folder so it cannot be included in itself.",
                "Invalid exFAT output");
            return;
        }
        bool replacing = File.Exists(destination);
        if (replacing && DarkMessageBox.ShowWarning(
                $"The output file already exists and will be replaced only after the new image passes verification.\n\n{destination}",
                "Replace exFAT image?", DarkDialogButton.YesNo) != DialogResult.Yes) return;
        string workingPath = replacing
            ? destination + "." + Guid.NewGuid().ToString("N") + ".replacement"
            : destination;

        BeginExfatOperation("Preparing selected dump...");
        try
        {
            var options = new ExfatBuildOptions
            {
                ClusterSize = cboExfatCluster.SelectedIndex switch
                {
                    1 => 32 * 1024,
                    2 => 64 * 1024,
                    _ => null
                },
                GenerateAmprIndex = chkExfatAmpr.Checked
            };
            IProgress<FfpfscProgress> progress = CreateExfatProgress();
            await RunFfpfscWorkerActionAsync(() => ExfatImage.WriteDirectoryAsync(game.RootPath, workingPath,
                options, progress, _exfatCancellation!.Token), _exfatCancellation!.Token);
            ExfatVerificationResult verification = await RunFfpfscWorkerAsync(() =>
                ExfatImage.VerifyAsync(workingPath, progress, _exfatCancellation.Token), _exfatCancellation.Token);
            if (replacing) File.Move(workingPath, destination, true);
            _lastExfatPath = destination;
            txtExfatOutput.Text = destination;
            progressExfat.Value = progressExfat.Maximum;
            string summary = $"Created {Path.GetFileName(destination)} | {verification.FileCount:N0} files | " +
                             $"{FormatBytes(verification.ImageSize)} | {FormatBytes(verification.ClusterSize)} clusters";
            SetExfatResult(summary);
            DarkMessageBox.ShowInformation(
                $"Native exFAT image created and fully verified.\n\n" +
                $"Image: {destination}\n" +
                $"Image size: {FormatBytes(verification.ImageSize)}\n" +
                $"Cluster size: {FormatBytes(verification.ClusterSize)}\n" +
                $"Files: {verification.FileCount:N0}\n" +
                $"Directories: {verification.DirectoryCount:N0}\n" +
                $"Logical file data: {FormatBytes(verification.LogicalFileBytes)}\n" +
                $"Manifest SHA-256: {verification.ManifestSha256}",
                "exFAT creation complete");
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(workingPath);
            SetExfatResult("exFAT creation cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            TryDeleteFile(workingPath);
            SetExfatResult("exFAT creation failed.");
            DarkMessageBox.ShowError(ex.Message, "exFAT creation");
        }
        finally
        {
            EndExfatOperation();
        }
    }

    private async void btnExfatVerify_Click(object? sender, EventArgs e)
    {
        PrepareExfatOpenDialog();
        if (exfatOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = exfatOpenDialog.FileName;
        BeginExfatOperation("Inspecting exFAT filesystem...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateExfatProgress();
            ExfatVerificationResult result = await RunFfpfscWorkerAsync(() =>
                ExfatImage.VerifyAsync(inputPath, progress, _exfatCancellation!.Token),
                _exfatCancellation!.Token);
            _lastExfatPath = Path.GetFullPath(inputPath);
            progressExfat.Value = progressExfat.Maximum;
            SetExfatResult($"Valid | {result.FileCount:N0} files | {FormatBytes(result.ImageSize)} | " +
                           $"{FormatBytes(result.ClusterSize)} clusters");
            DarkMessageBox.ShowInformation(
                $"The exFAT structure is valid and every file was read successfully.\n\n" +
                $"Image size: {FormatBytes(result.ImageSize)}\n" +
                $"Cluster size: {FormatBytes(result.ClusterSize)}\n" +
                $"Files: {result.FileCount:N0}\n" +
                $"Directories: {result.DirectoryCount:N0}\n" +
                $"Logical file data: {FormatBytes(result.LogicalFileBytes)}\n" +
                $"Manifest SHA-256: {result.ManifestSha256}",
                "exFAT verification");
        }
        catch (OperationCanceledException)
        {
            SetExfatResult("exFAT verification cancelled.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetExfatResult("exFAT verification failed.");
            DarkMessageBox.ShowError(ex.Message, "exFAT verification");
        }
        finally
        {
            EndExfatOperation();
        }
    }

    private async void btnExfatExtract_Click(object? sender, EventArgs e)
    {
        PrepareExfatOpenDialog();
        if (exfatOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        if (exfatExtractFolderDialog.ShowDialog(this) != DialogResult.OK) return;
        string baseName = MakeSafeFileName(Path.GetFileNameWithoutExtension(exfatOpenDialog.FileName));
        string destination = FindAvailableDirectory(Path.Combine(exfatExtractFolderDialog.SelectedPath, baseName));
        BeginExfatOperation("Extracting exFAT files...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateExfatProgress();
            await RunFfpfscWorkerActionAsync(() => ExfatImage.ExtractDirectoryAsync(exfatOpenDialog.FileName,
                destination, progress, _exfatCancellation!.Token), _exfatCancellation!.Token);
            _lastExfatPath = Path.GetFullPath(exfatOpenDialog.FileName);
            progressExfat.Value = progressExfat.Maximum;
            SetExfatResult($"Extracted files to {destination}");
            DarkMessageBox.ShowInformation($"The exFAT filesystem was extracted successfully.\n\n{destination}",
                "exFAT extraction");
        }
        catch (OperationCanceledException)
        {
            SetExfatResult("exFAT extraction cancelled; partial output was removed.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetExfatResult("exFAT extraction failed.");
            DarkMessageBox.ShowError(ex.Message, "exFAT extraction");
        }
        finally
        {
            EndExfatOperation();
        }
    }

    private async void btnExfatRefreshAmpr_Click(object? sender, EventArgs e)
    {
        PrepareExfatOpenDialog();
        if (exfatOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = exfatOpenDialog.FileName;
        if (DarkMessageBox.ShowWarning(
                "This operation creates or refreshes the root ampr_emu.index. If necessary, it can relocate the " +
                "index, extend the root directory, and safely grow the exFAT image tail. All changed byte ranges " +
                "and the original image length are restored automatically if cancellation or verification fails.\n\n" +
                inputPath,
                "Refresh AMPR index?", DarkDialogButton.YesNo) != DialogResult.Yes) return;

        BeginExfatOperation("Building AMPR index from exFAT entries...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateExfatProgress();
            ExfatAmprRefreshResult result = await RunFfpfscWorkerAsync(() =>
                ExfatAmprPatcher.RefreshAsync(inputPath, progress, _exfatCancellation!.Token),
                _exfatCancellation!.Token);
            _lastExfatPath = Path.GetFullPath(inputPath);
            progressExfat.Value = progressExfat.Maximum;
            string state = result.Created ? "Created" : result.Changed ? "Refreshed" : "Already current";
            SetExfatResult($"{state} AMPR index | {result.RecordCount:N0} records | " +
                           $"{FormatBytes(result.IndexBytes)}");
            DarkMessageBox.ShowInformation(
                $"AMPR index status: {state}.\n\n" +
                $"Records: {result.RecordCount:N0}\n" +
                $"Index size: {FormatBytes(result.IndexBytes)}\n" +
                $"Allocation: cluster {result.FirstCluster:N0}, {result.ClusterCount:N0} cluster(s)\n" +
                $"Image grew: {(result.ImageGrew ? "Yes" : "No")}\n" +
                $"SHA-256: {result.Sha256}",
                "exFAT AMPR index");
        }
        catch (OperationCanceledException)
        {
            SetExfatResult("AMPR refresh cancelled; original index bytes were preserved.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetExfatResult("AMPR refresh was not applied.");
            DarkMessageBox.ShowError(ex.Message, "exFAT AMPR refresh");
        }
        finally
        {
            EndExfatOperation();
        }
    }

    private void btnExfatEdit_Click(object? sender, EventArgs e)
    {
        PrepareExfatOpenDialog();
        exfatOpenDialog.Title = "Select an exFAT image to edit";
        if (exfatOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var editor = new ExfatEditorForm(exfatOpenDialog.FileName);
            editor.ShowDialog(this);
            _lastExfatPath = Path.GetFullPath(exfatOpenDialog.FileName);
            statusLabel.Text = $"Closed exFAT editor for {Path.GetFileName(_lastExfatPath)}.";
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            DarkMessageBox.ShowError(ex.Message, "Open exFAT editor");
        }
    }

    private async void btnExfatRepair_Click(object? sender, EventArgs e)
    {
        PrepareExfatOpenDialog();
        exfatOpenDialog.Title = "Select a recoverable exFAT image to repair";
        if (exfatOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string inputPath = exfatOpenDialog.FileName;
        if (DarkMessageBox.ShowWarning(
                "Repair first restores a damaged main or backup boot region when the other copy is valid. It then " +
                "extracts every readable file, rebuilds all filesystem metadata beside the original, fully verifies " +
                "the result, and only then replaces the original image.\n\n" +
                "This can require substantial free space and time. Damage affecting both boot copies or unreadable " +
                "file data cannot be repaired automatically.\n\n" + inputPath,
                "Repair exFAT image?", DarkDialogButton.YesNo) != DialogResult.Yes) return;

        BeginExfatOperation("Checking exFAT boot regions...");
        try
        {
            IProgress<FfpfscProgress> progress = CreateExfatProgress();
            ExfatRepairResult result = await RunFfpfscWorkerAsync(() =>
                ExfatImageMaintenance.RepairAsync(inputPath, progress, _exfatCancellation!.Token),
                _exfatCancellation!.Token);
            _lastExfatPath = Path.GetFullPath(inputPath);
            progressExfat.Value = progressExfat.Maximum;
            SetExfatResult($"Repaired and verified {result.Verification.FileCount:N0} files.");
            DarkMessageBox.ShowInformation(
                $"The exFAT image was rebuilt and fully verified.\n\n" +
                $"Boot mirror recovered: {(result.BootRegionRecovered ? "Yes" : "Not required")}\n" +
                $"Files: {result.Verification.FileCount:N0}\n" +
                $"Directories: {result.Verification.DirectoryCount:N0}\n" +
                $"Manifest SHA-256: {result.Verification.ManifestSha256}",
                "exFAT repair complete");
        }
        catch (OperationCanceledException)
        {
            SetExfatResult("exFAT repair cancelled; the original image was preserved.");
        }
        catch (Exception ex) when (IsFfpfscOperationException(ex))
        {
            SetExfatResult("exFAT repair failed; the original image was preserved.");
            DarkMessageBox.ShowError(ex.Message, "exFAT repair");
        }
        finally
        {
            EndExfatOperation();
        }
    }

    private void btnExfatCancel_Click(object? sender, EventArgs e) => _exfatCancellation?.Cancel();

    private void BeginExfatOperation(string message)
    {
        _exfatCancellation?.Cancel();
        _exfatCancellation?.Dispose();
        _exfatCancellation = new CancellationTokenSource();
        progressExfat.Value = 0;
        lblExfatProgress.Text = message;
        statusLabel.Text = message;
        _isExfatBusy = true;
        UpdateOperationState();
    }

    private void EndExfatOperation()
    {
        _isExfatBusy = false;
        UpdateOperationState();
        _exfatCancellation?.Dispose();
        _exfatCancellation = null;
    }

    private IProgress<FfpfscProgress> CreateExfatProgress() => new Progress<FfpfscProgress>(value =>
    {
        double percentage = value.TotalBytes <= 0 ? 0 : value.BytesProcessed * 100.0 / value.TotalBytes;
        progressExfat.Value = Math.Clamp((int)Math.Round(percentage), progressExfat.Minimum, progressExfat.Maximum);
        lblExfatProgress.Text = $"{value.Stage}: {percentage:N1}%  " +
                                $"({FormatBytes(value.BytesProcessed)} / {FormatBytes(value.TotalBytes)})";
        statusLabel.Text = lblExfatProgress.Text;
    });

    private void SetExfatResult(string message)
    {
        lblExfatProgress.Text = message;
        lblExfatOperationsInfo.Text = message;
        statusLabel.Text = message;
    }

    private void PrepareExfatOpenDialog()
    {
        exfatOpenDialog.Title = "Select an exFAT image";
        string candidate = File.Exists(_lastExfatPath) ? _lastExfatPath : txtExfatOutput.Text.Trim();
        if (_selectedGame?.SourceKind == Ps5SourceKind.FilesystemImage && File.Exists(_selectedGame.RootPath))
            candidate = _selectedGame.RootPath;
        if (!File.Exists(candidate)) return;
        exfatOpenDialog.FileName = Path.GetFileName(candidate);
        exfatOpenDialog.InitialDirectory = Path.GetDirectoryName(candidate);
    }

    private static bool IsPathInsideDirectory(string path, string directory)
    {
        string fullPath = Path.GetFullPath(path);
        string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase);
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

    private void Settings_Click(object? sender, EventArgs e)
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog(this) != DialogResult.OK) return;
        _settings = form.Settings;
        _stateStore.SaveSettings(_settings);
        statusLabel.Text = "Settings saved. Choose Refresh to rescan the configured folders.";
    }

    private void Exit_Click(object? sender, EventArgs e) => Close();

    private void About_Click(object? sender, EventArgs e) =>
        DarkMessageBox.ShowInformation(
            "PS5 PKG Tool\n\nA DarkUI library manager for unpacked PS5 game dumps, filesystem images, and Sony CNT/FIH packages.\n" +
            "Includes native exFAT, UFS2/FFPKG, PFSC and PFS frameworks for creating, verifying, extracting, editing, rebuilding, and directly browsing images without Python or mounted drives.",
            "About PS5 PKG Tool");

    private void txtSearch_TextChanged(object? sender, EventArgs e) => ApplyFilter();

    private void ApplyFilter()
    {
        string query = txtSearch.Text.Trim();
        _visibleGames = string.IsNullOrWhiteSpace(query)
            ? _games.ToList()
            : _games.Where(game =>
                game.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                game.TitleId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                game.ContentId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                game.RootPath.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToList();

        var table = new DataTable();
        table.Columns.Add("Title");
        table.Columns.Add("Title ID");
        table.Columns.Add("Source");
        table.Columns.Add("Size", typeof(long));
        table.Columns.Add("Version");
        table.Columns.Add("Required Firmware");
        table.Columns.Add("Features");
        table.Columns.Add("DRM");
        table.Columns.Add("Location");
        foreach (Ps5GameInfo game in _visibleGames)
            table.Rows.Add(game.Title, game.TitleId, game.SourceDescription,
                game.SourceSize > 0 ? game.SourceSize : DBNull.Value, game.DisplayVersion, game.RequiredSystemSoftware,
                string.Join(", ", game.DeclaredFeatures), game.DrmType, game.RootPath);
        gridLibrary.DataSource = table;
        statusCount.Text = $"{_visibleGames.Count:N0} of {_games.Count:N0} games";
        if (_visibleGames.Count == 0)
        {
            SetSelectedGame(null);
            ClearDetails();
        }
    }

    private async void gridLibrary_SelectionChanged(object? sender, EventArgs e)
    {
        if (gridLibrary.SelectedRows.Count == 0) return;
        int rowIndex = gridLibrary.SelectedRows[0].Index;
        if (rowIndex < 0 || rowIndex >= _visibleGames.Count) return;
        Ps5GameInfo game = _visibleGames[rowIndex];
        SetSelectedGame(game);
        await ShowGameAsync(game);
    }

    private void gridLibrary_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (gridLibrary.Columns[e.ColumnIndex].Name != "Size") return;
        e.Value = e.Value is long size && size > 0 ? FormatBytes(size) : string.Empty;
        e.FormattingApplied = true;
    }

    private async Task ShowGameAsync(Ps5GameInfo game)
    {
        PopulateOverview(game);
        txtRawMetadata.Text = PrettyJson(game.RawParamJson);
        ClearDeepDetails();
        statusLabel.Text = $"Loading details for {game.Title}...";

        _detailCancellation?.Cancel();
        _detailCancellation?.Dispose();
        _detailCancellation = new CancellationTokenSource();
        int version = Interlocked.Increment(ref _detailVersion);
        try
        {
            if (!_detailsCache.TryGetValue(game.RootPath, out Ps5GameDetails? details))
            {
                details = await _detailsLoader.LoadAsync(game, _detailCancellation.Token);
                _detailsCache[game.RootPath] = details;
            }
            if (version != _detailVersion || _detailCancellation.IsCancellationRequested) return;
            PopulateDetails(game, details);
            statusLabel.Text = details.Errors.Count == 0
                ? $"Loaded {game.Title}."
                : $"Loaded {game.Title} with {details.Errors.Count} detail warning(s).";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (version != _detailVersion) return;
            statusLabel.Text = "Unable to load game details.";
            DarkMessageBox.ShowError(ex.Message, "PS5 game details");
        }
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
            Add("CNT Offset", $"0x{package.ContainerOffset:X}");
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
    }

    private void PopulateDetails(Ps5GameInfo game, Ps5GameDetails details)
    {
        ReplaceImage(pictureIcon, details.IconPng);
        ReplaceImage(pictureBackground0, details.BackgroundPng);
        ReplaceImage(pictureBackground1, details.Background1Png);
        ReplaceImage(pictureBackground2, details.Background2Png);
        PopulateTrophies(details.TrophySet);
        PopulateActivities(details.Uds);
        PopulateFiles(game, details.Files);
        PopulateExecutable(details.Executable);
        if (details.Errors.Count > 0)
            lblExecutableSummary.Text += "  Warnings: " + string.Join(" | ", details.Errors);
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
                Image? icon = CreateGridImage(trophy.IconPng);
                if (icon is not null) _trophyImages.Add(icon);
                table.Rows.Add(trophy.Id, icon!, trophy.Grade, trophy.Hidden, trophy.Name, trophy.Description, trophy.UnlockCondition);
            }
            string grades = string.Join(", ", set.Trophies.GroupBy(trophy => trophy.Grade)
                .Select(group => $"{group.Key}: {group.Count()}"));
            lblTrophySummary.Text = $"{set.Title} - {set.Trophies.Count} trophies ({grades}) - {set.NpCommunicationId} - " +
                                    $"Language: {set.SelectedLanguage} - UCP integrity: {(set.IntegrityValid ? "Valid" : "Failed")}";
        }
        else lblTrophySummary.Text = "No PS5 trophy archive was found.";
        gridTrophies.DataSource = table;
    }

    private void PopulateActivities(Ps5UdsSummary? uds)
    {
        var table = new DataTable();
        table.Columns.Add("Event");
        table.Columns.Add("Type");
        table.Columns.Add("Definition Group");
        table.Columns.Add("Properties", typeof(int));
        if (uds is not null)
        {
            foreach (Ps5UdsEvent item in uds.Events)
                table.Rows.Add(item.Name, item.Type, item.DefinitionGroup, item.PropertyCount);
            lblActivitiesSummary.Text = $"Events: {uds.EventCount:N0}  |  Stats: {uds.StatCount:N0}  |  Enum groups: {uds.EnumGroupCount:N0}  |  " +
                                        $"Extraction rules: {uds.ExtractionRuleCount:N0}  |  {uds.NpCommunicationId}  |  " +
                                        $"UCP integrity: {(uds.IntegrityValid ? "Valid" : "Failed")}";
        }
        else lblActivitiesSummary.Text = "No UDS activity archive was found.";
        gridActivities.DataSource = table;
    }

    private void PopulateFiles(Ps5GameInfo game, Ps5FileInventory inventory)
    {
        _currentGameRoot = game.RootPath;
        _currentSourceIsContainer = game.SourceKind != Ps5SourceKind.LooseDump;
        _directoryNodes.Clear();
        _filesByDirectory.Clear();
        treeFiles.BeginUpdate();
        treeFiles.Nodes.Clear();

        var root = new TreeNode(game.Title) { Tag = string.Empty };
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
                    node = new TreeNode(segment) { Tag = current };
                    parent.Nodes.Add(node);
                    _directoryNodes[current] = node;
                }
                parent = node;
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
    }

    private void RefreshFileList()
    {
        string directory = treeFiles.SelectedNode?.Tag as string ?? string.Empty;
        string filter = txtFileFilter.Text.Trim();
        listFiles.BeginUpdate();
        listFiles.Items.Clear();

        if (directory.Length > 0)
        {
            string parent = NormalizeDirectory(Path.GetDirectoryName(directory));
            AddFileListItem("..", "Folder", parent, string.Empty, new FileBrowserEntry(parent, true));
        }

        IEnumerable<string> childDirectories = _directoryNodes.Keys
            .Where(path => path.Length > 0 && NormalizeDirectory(Path.GetDirectoryName(path)).Equals(directory, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => Path.GetFileName(path), StringComparer.CurrentCultureIgnoreCase);
        foreach (string child in childDirectories)
        {
            string name = Path.GetFileName(child);
            if (!MatchesFileFilter(name, child, filter)) continue;
            AddFileListItem(name, "Folder", child, FormatBytes(GetDirectorySize(child)), new FileBrowserEntry(child, true));
        }

        if (_filesByDirectory.TryGetValue(directory, out List<Ps5FileInfo>? files))
        {
            foreach (Ps5FileInfo file in files.OrderBy(item => Path.GetFileName(item.RelativePath), StringComparer.CurrentCultureIgnoreCase))
            {
                string name = Path.GetFileName(file.RelativePath);
                if (!MatchesFileFilter(name, file.RelativePath, filter)) continue;
                string type = string.IsNullOrWhiteSpace(file.Extension) ? "File" : file.Extension.TrimStart('.').ToUpperInvariant() + " File";
                if (file.IsEncrypted) type += " (Encrypted)";
                AddFileListItem(name, type, file.RelativePath, FormatBytes(file.Size), new FileBrowserEntry(file.RelativePath, false));
            }
        }

        listFiles.EndUpdate();
        listFiles.RefreshLayout();
    }

    private void AddFileListItem(string name, string type, string path, string size, FileBrowserEntry entry)
    {
        var item = new ListViewItem(name) { Tag = entry };
        item.SubItems.Add(type);
        item.SubItems.Add(path);
        item.SubItems.Add(size);
        listFiles.Items.Add(item);
    }

    private long GetDirectorySize(string directory) => _filesByDirectory
        .Where(pair => pair.Key.Equals(directory, StringComparison.OrdinalIgnoreCase) ||
                       pair.Key.StartsWith(directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        .SelectMany(pair => pair.Value)
        .Sum(file => file.Size);

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

    private void txtFileFilter_TextChanged(object? sender, EventArgs e) => RefreshFileList();

    private void btnClearFileFilter_Click(object? sender, EventArgs e) => txtFileFilter.Clear();

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
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  {FormatBytes(file.Size)}";
        try
        {
            if (IsMediaExtension(extension))
            {
                ShowMediaPreviewReady(file);
                return;
            }
            if (IsImageExtension(extension) && file.Size <= 256L * 1024 * 1024 && file.Size <= int.MaxValue)
            {
                await LoadImageViewerAsync(_selectedGame, entry, file, extension, version,
                    _filePreviewCancellation.Token);
                return;
            }
            if (IsTextExtension(extension))
            {
                bool shown = await LoadTextViewerAsync(_selectedGame, entry, file, version,
                    _filePreviewCancellation.Token);
                if (shown) return;
            }
            await LoadHexViewerAsync(0, version, _filePreviewCancellation.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or
                                   NotSupportedException or ArgumentException)
        {
            if (version == _filePreviewVersion)
                lblFileViewerInfo.Text = $"Preview unavailable: {ex.Message}";
        }
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
        fileViewerCommands.Visible = false;
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
        fileViewerCommands.Visible = false;
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Text  •  {FormatBytes(file.Size)}";
        return true;
    }

    private async Task LoadHexViewerAsync(long offset, int version, CancellationToken cancellationToken)
    {
        Ps5GameInfo? game = _selectedGame;
        FileBrowserEntry? entry = _filePreviewEntry;
        if (game is null || entry is null) return;
        GameFileChunk chunk = await Task.Run(() => GameFileSystem.ReadFileChunk(game, entry.RelativePath, offset,
            HexPreviewPageSize, cancellationToken), cancellationToken);
        if (version != _filePreviewVersion || cancellationToken.IsCancellationRequested) return;
        _hexPreviewOffset = chunk.Offset;
        txtHexViewer.Text = FormatHexPage(chunk);
        txtHexViewer.SelectionStart = 0;
        txtHexViewer.SelectionLength = 0;
        txtHexViewer.Visible = true;
        txtHexViewer.BringToFront();
        fileViewerCommands.Visible = true;
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
        fileViewerCommands.Visible = true;
        btnMediaLoad.Visible = true;
        lblFileViewerInfo.Text = $"{Path.GetFileName(file.RelativePath)}  •  Media  •  {FormatBytes(file.Size)}  •  Click Load & Play";
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
        fileViewerCommands.Visible = false;
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
        long offset = Math.Max(0, _hexPreviewOffset - HexPreviewPageSize);
        await NavigateHexViewerAsync(offset);
    }

    private async void btnHexNext_Click(object? sender, EventArgs e)
    {
        long offset = checked(_hexPreviewOffset + HexPreviewPageSize);
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
        mediaFileViewer.Play();
        lblFileViewerInfo.Text = $"{Path.GetFileName(entry.RelativePath)}  •  Playing  •  {FormatBytes(file.Size)}";
    }

    private void btnMediaPlay_Click(object? sender, EventArgs e) => mediaFileViewer.Play();
    private void btnMediaPause_Click(object? sender, EventArgs e) => mediaFileViewer.Pause();
    private void btnMediaStop_Click(object? sender, EventArgs e) => mediaFileViewer.Stop();

    private void mediaFileViewer_MediaFailed(object? sender, System.Windows.ExceptionRoutedEventArgs e)
    {
        statusLabel.Text = "The selected media format is not supported by the installed Windows codecs.";
        DarkMessageBox.ShowWarning(e.ErrorException?.Message ??
            "The selected media format is not supported by the installed Windows codecs.", "Media preview");
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
            DarkMessageBox.ShowError(ex.Message, "Filesystem image browser");
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
            const long maximumImageBytes = 256L * 1024 * 1024;
            if (new FileInfo(path).Length <= maximumImageBytes)
            {
                byte[] image = await File.ReadAllBytesAsync(path);
                FilePreviewForm.ShowImage(this, title, image);
                return;
            }
        }

        if (extension is ".json" or ".txt" or ".xml" or ".ini" or ".cfg" or ".log" or ".csv" or ".yaml" or ".yml")
        {
            const long maximumTextBytes = 16L * 1024 * 1024;
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
            DarkMessageBox.ShowWarning(
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
        var table = new DataTable();
        table.Columns.Add("Module");
        table.Columns.Add("Kind");
        table.Columns.Add("Size");
        table.Columns.Add("Path");
        if (executable is not null)
        {
            foreach (Ps5ModuleInfo module in executable.Modules)
                table.Rows.Add(module.Name, module.Kind, FormatBytes(module.Size), module.RelativePath);
            lblExecutableSummary.Text = $"SELF {executable.SelfMagic} - {FormatBytes(executable.FileSize)} - embedded ELF at 0x{executable.ElfOffset:X} - " +
                                        $"x86-64 machine 0x{executable.Machine:X4} - entry 0x{executable.EntryPoint:X} - " +
                                        $"{executable.ProgramHeaderCount} program headers - {executable.Modules.Count} modules";
        }
        else lblExecutableSummary.Text = "eboot.bin was not found.";
        gridModules.DataSource = table;
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
        DisposeTrophyImages();
        gridActivities.DataSource = null;
        _currentGameRoot = string.Empty;
        _currentSourceIsContainer = false;
        _directoryNodes.Clear();
        _filesByDirectory.Clear();
        treeFiles.Nodes.Clear();
        listFiles.Items.Clear();
        txtFileFilter.Clear();
        gridModules.DataSource = null;
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

    private static Image? CreateGridImage(byte[]? bytes)
    {
        using Image? source = ImageFromBytes(bytes);
        return source is null ? null : new Bitmap(source, new Size(40, 40));
    }

    private static void ReplaceImage(PictureBox target, byte[]? bytes)
    {
        Image? previous = target.Image;
        target.Image = ImageFromBytes(bytes);
        previous?.Dispose();
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
        if (e.RowIndex < 0 || e.RowIndex >= _visibleGames.Count) return;
        Ps5GameInfo game = _visibleGames[e.RowIndex];
        if (game.SourceKind != Ps5SourceKind.LooseDump) RevealInExplorer(game.RootPath);
        else Process.Start(new ProcessStartInfo { FileName = game.RootPath, UseShellExecute = true });
    }

    private static void RevealInExplorer(string path) => Process.Start(new ProcessStartInfo
    {
        FileName = "explorer.exe",
        Arguments = $"/select,\"{path}\"",
        UseShellExecute = true
    });

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _scanCancellation?.Cancel();
        _detailCancellation?.Cancel();
        _ffpfscCancellation?.Cancel();
        _exfatCancellation?.Cancel();
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
        _ffpfscCancellation?.Dispose();
        _exfatCancellation?.Dispose();
        _ffpkgCancellation?.Dispose();
        _sonyPkgCancellation?.Dispose();
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

    private sealed record FileBrowserEntry(string RelativePath, bool IsDirectory);
}
