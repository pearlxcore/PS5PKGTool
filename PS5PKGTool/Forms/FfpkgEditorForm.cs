using System.Data;
using DarkUI.Forms;
using UFS2Tool;

namespace PS5PKGTool.Forms;

public partial class FfpkgEditorForm : DarkForm
{
    private readonly string _imagePath;
    private readonly List<Ufs2EditOperation> _operations = [];
    private CancellationTokenSource? _cancellation;
    private bool _busy;

    public FfpkgEditorForm(string imagePath)
    {
        InitializeComponent();
        _imagePath = Path.GetFullPath(imagePath);
        lblImage.Text = _imagePath;
        LoadEntries();
        UpdateState();
    }

    private void LoadEntries()
    {
        var table = new DataTable();
        table.Columns.Add("Type");
        table.Columns.Add("Path");
        table.Columns.Add("Size", typeof(long));
        using var volume = new Ufs2Volume(_imagePath);
        foreach (Ufs2VolumeEntry entry in volume.Entries.OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase))
            table.Rows.Add(entry.IsDirectory ? "Directory" : "File", entry.Path,
                entry.IsDirectory ? DBNull.Value : entry.Size);
        gridEntries.DataSource = table;
        if (gridEntries.Columns[0] is DataGridViewColumn type) type.FillWeight = 18;
        if (gridEntries.Columns[1] is DataGridViewColumn path) path.FillWeight = 62;
        if (gridEntries.Columns[2] is DataGridViewColumn size)
        {
            size.FillWeight = 20;
            size.DefaultCellStyle.Format = "N0";
        }
    }

    private (string Path, bool IsDirectory)? SelectedEntry()
    {
        if (gridEntries.SelectedRows.Count != 1) return null;
        DataGridViewRow row = gridEntries.SelectedRows[0];
        string? path = row.Cells["Path"].Value?.ToString();
        string? type = row.Cells["Type"].Value?.ToString();
        return string.IsNullOrWhiteSpace(path) ? null : (path, type == "Directory");
    }

    private void gridEntries_SelectionChanged(object? sender, EventArgs e)
    {
        (string Path, bool IsDirectory)? selected = SelectedEntry();
        if (selected is null) return;
        txtTarget.Text = selected.Value.IsDirectory
            ? selected.Value.Path
            : ParentPath(selected.Value.Path);
        UpdateState();
    }

    private void btnReplace_Click(object? sender, EventArgs e)
    {
        (string Path, bool IsDirectory)? selected = SelectedEntry();
        if (selected is null || selected.Value.IsDirectory)
        {
            AppDialog.ShowWarning("Select one file to replace.", "FFPKG editor");
            return;
        }
        if (replacementOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        Queue(Ufs2EditOperation.Replace(selected.Value.Path, replacementOpenDialog.FileName),
            $"Replace  {selected.Value.Path}  ←  {replacementOpenDialog.FileName}");
    }

    private void btnAddFiles_Click(object? sender, EventArgs e)
    {
        if (addFilesOpenDialog.ShowDialog(this) != DialogResult.OK) return;
        string parent = NormalizeOptionalDirectory(txtTarget.Text);
        foreach (string source in addFilesOpenDialog.FileNames)
        {
            string target = CombineImagePath(parent, Path.GetFileName(source));
            Queue(Ufs2EditOperation.AddFile(target, source), $"Add file  {target}  ←  {source}");
        }
    }

    private void btnAddFolder_Click(object? sender, EventArgs e)
    {
        if (addFolderDialog.ShowDialog(this) != DialogResult.OK) return;
        string parent = NormalizeOptionalDirectory(txtTarget.Text);
        string source = addFolderDialog.SelectedPath;
        string name = new DirectoryInfo(source).Name;
        string target = CombineImagePath(parent, name);
        Queue(Ufs2EditOperation.AddDirectoryTree(target, source), $"Add tree  {target}  ←  {source}");
    }

    private void btnNewDirectory_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTarget.Text))
        {
            AppDialog.ShowWarning("Enter the full internal path for the new directory.", "FFPKG editor");
            return;
        }
        try
        {
            string target = Ufs2Operations.NormalizeImagePath(txtTarget.Text);
            Queue(Ufs2EditOperation.AddDirectory(target), $"New directory  {target}");
        }
        catch (Exception ex) when (ex is IOException or ArgumentException)
        {
            AppDialog.ShowError(ex.Message, "Invalid internal path");
        }
    }

    private void btnDelete_Click(object? sender, EventArgs e)
    {
        if (gridEntries.SelectedRows.Count == 0)
        {
            AppDialog.ShowWarning("Select one or more files or directories to delete.", "FFPKG editor");
            return;
        }
        string[] paths = gridEntries.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.Cells["Path"].Value?.ToString())
            .Where(path => !string.IsNullOrWhiteSpace(path)).Cast<string>()
            .OrderBy(path => path.Count(character => character == '/')).ToArray();
        paths = paths.Where(path => !paths.Any(parent => !parent.Equals(path, StringComparison.OrdinalIgnoreCase) &&
                         path.StartsWith(parent.TrimEnd('/') + '/', StringComparison.OrdinalIgnoreCase))).ToArray();
        if (AppDialog.ShowWarning($"Queue deletion of {paths.Length:N0} selected item(s)?\n\n" +
                                       string.Join("\n", paths.Take(12)) +
                                       (paths.Length > 12 ? "\n..." : string.Empty),
                "Queue FFPKG deletion?", DarkDialogButton.YesNo) != DialogResult.Yes) return;
        foreach (string path in paths)
            Queue(Ufs2EditOperation.Delete(path), $"Delete  {path}");
    }

    private void btnUndo_Click(object? sender, EventArgs e)
    {
        if (_operations.Count == 0) return;
        _operations.RemoveAt(_operations.Count - 1);
        lstChanges.Items.RemoveAt(lstChanges.Items.Count - 1);
        UpdateState();
    }

    private async void btnApply_Click(object? sender, EventArgs e)
    {
        if (_operations.Count == 0) return;
        if (AppDialog.ShowWarning(
                $"Apply {_operations.Count:N0} queued change(s)?\n\n" +
                "All changes extract and rebuild the FFPKG image beside the original. The original is replaced only after " +
                "full verification. Large game images require substantial free space and time.",
                "Apply FFPKG changes?", DarkDialogButton.YesNo) != DialogResult.Yes) return;

        _cancellation = new CancellationTokenSource();
        SetBusy(true);
        try
        {
            var progress = new Progress<Ufs2Progress>(value =>
            {
                int percentage = value.TotalBytes <= 0 ? 0 :
                    (int)Math.Round(value.BytesProcessed * 100.0 / value.TotalBytes);
                progressEdit.Value = Math.Clamp(percentage, progressEdit.Minimum, progressEdit.Maximum);
                string amounts = value.Unit.Equals("bytes", StringComparison.OrdinalIgnoreCase)
                    ? $"{FormatBytes(value.Completed)} / {FormatBytes(value.Total)}"
                    : $"{value.Completed:N0} / {value.Total:N0} {value.Unit}";
                lblStatus.Text = $"{value.Stage} - {amounts}";
            });
            Ufs2EditResult result = await Ufs2Operations.ApplyEditsAsync(_imagePath, _operations, progress,
                _cancellation.Token);
            progressEdit.Value = progressEdit.Maximum;
            lblStatus.Text = $"Completed and verified {result.Verification.FileCount:N0} files.";
            AppDialog.ShowInformation(
                $"All FFPKG changes were applied successfully.\n\n" +
                $"Mode: Verified transactional FFPKG rebuild\n" +
                $"Files: {result.Verification.FileCount:N0}\n" +
                $"Directories: {result.Verification.DirectoryCount:N0}\n" +
                $"Manifest SHA-256: {result.Verification.ManifestSha256}",
                "FFPKG edit complete");
            _operations.Clear();
            lstChanges.Items.Clear();
            LoadEntries();
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Operation cancelled; the original image was preserved.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
        {
            lblStatus.Text = "No unverified changes were retained.";
            AppDialog.ShowError(ex.Message, "FFPKG edit failed");
        }
        finally
        {
            _cancellation?.Dispose();
            _cancellation = null;
            SetBusy(false);
        }
    }

    private void btnCancelOperation_Click(object? sender, EventArgs e) => _cancellation?.Cancel();

    private void FfpkgEditorForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_busy)
        {
            _cancellation?.Cancel();
            e.Cancel = true;
            return;
        }
        if (_operations.Count > 0 && AppDialog.ShowWarning(
                $"Discard {_operations.Count:N0} queued FFPKG change(s)?", "Close FFPKG editor?",
                DarkDialogButton.YesNo) != DialogResult.Yes)
            e.Cancel = true;
    }

    private void Queue(Ufs2EditOperation operation, string description)
    {
        _operations.Add(operation);
        lstChanges.Items.Add(description);
        UpdateState();
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        UpdateState();
    }

    private void UpdateState()
    {
        bool idle = !_busy;
        (string Path, bool IsDirectory)? selected = SelectedEntry();
        gridEntries.Enabled = idle;
        txtTarget.Enabled = idle;
        btnReplace.Enabled = idle && selected is { IsDirectory: false };
        btnAddFiles.Enabled = idle;
        btnAddFolder.Enabled = idle;
        btnNewDirectory.Enabled = idle;
        btnDelete.Enabled = idle && gridEntries.SelectedRows.Count > 0;
        btnUndo.Enabled = idle && _operations.Count > 0;
        btnApply.Enabled = idle && _operations.Count > 0;
        btnCancelOperation.Enabled = _busy;
        btnClose.Enabled = idle;
    }

    private static string NormalizeOptionalDirectory(string path) =>
        string.IsNullOrWhiteSpace(path) ? string.Empty : Ufs2Operations.NormalizeImagePath(path);
    private static string CombineImagePath(string parent, string name) =>
        Ufs2Operations.NormalizeImagePath(parent.Length == 0 ? name : parent + "/" + name);
    private static string ParentPath(string path)
    {
        int slash = path.LastIndexOf('/');
        return slash < 0 ? string.Empty : path[..slash];
    }
    private static string FormatBytes(long value)
    {
        string[] suffixes = ["B", "KiB", "MiB", "GiB", "TiB"];
        double amount = Math.Max(0, value);
        int suffix = 0;
        while (amount >= 1024 && suffix < suffixes.Length - 1) { amount /= 1024; suffix++; }
        return suffix == 0 ? $"{amount:N0} {suffixes[suffix]}" : $"{amount:N2} {suffixes[suffix]}";
    }
}
