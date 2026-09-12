using System.Collections.Concurrent;
using System.ComponentModel;
using PS5PKGTool.Core.Models;
using PS5PKGTool.Core.Services;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private static readonly (string Name, string Header, int Width, int Weight)[] LibraryColumnDefinitions =
    [
        ("Icon", "", 34, 0),
        ("Title", "Title", 240, 26),
        ("TitleId", "Title ID", 110, 12),
        ("ContentId", "Content ID", 190, 20),
        ("Category", "Category", 80, 9),
        ("Region", "Region", 80, 9),
        ("Source", "Source", 120, 13),
        ("Size", "Size", 80, 9),
        ("Version", "Version", 80, 9),
        ("Firmware", "Required Firmware", 110, 12),
        ("Features", "Features", 140, 14),
        ("Drm", "DRM", 90, 10),
        ("FileName", "File name", 200, 18),
        ("Location", "Location", 240, 26)
    ];

    private string _librarySortColumn = "Title";
    private bool _librarySortAscending = true;
    private bool _libraryColumnsReady;
    private bool _suppressLibrarySelection;
    private bool _groupApplied;
    private ToolStripMenuItem? _libraryColumnMenu;
    private readonly ConcurrentDictionary<string, Image> _libraryThumbnails = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte> _libraryThumbnailAttempts = new(StringComparer.OrdinalIgnoreCase);
    private int _libraryThumbnailVersion;

    private void EnsureLibraryColumns()
    {
        if (_libraryColumnsReady) return;
        _libraryColumnsReady = true;

        _librarySortColumn = string.IsNullOrWhiteSpace(_settings.LibrarySortColumn) ? "Title" : _settings.LibrarySortColumn;
        _librarySortAscending = _settings.LibrarySortAscending;
        _settings.LibraryColumnOrder ??= [];
        _settings.LibraryHiddenColumns ??= [];

        gridLibrary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLibrary.AutoSortGroups = false;
        gridLibrary.GroupLabelFormatter = (keyObject, count) =>
        {
            string key = keyObject as string ?? keyObject?.ToString() ?? string.Empty;
            string value = string.IsNullOrWhiteSpace(key) ? "(none)" : key;
            return $"{GroupLabel(_libraryGroupBy)}: {value}  ({count})";
        };
        gridLibrary.Columns.Clear();

        foreach ((string name, string header, int width, int weight) in LibraryColumnDefinitions)
        {
            if (name == "Icon")
            {
                gridLibrary.Columns.Add(new DataGridViewImageColumn
                {
                    Name = name,
                    HeaderText = header,
                    Width = width,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    Resizable = DataGridViewTriState.False,
                    ReadOnly = true
                });
                continue;
            }

            var column = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                FillWeight = weight,
                SortMode = DataGridViewColumnSortMode.Programmatic,
                ReadOnly = true
            };
            if (name == "Size")
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridLibrary.Columns.Add(column);
        }

        RestoreLibraryColumnLayout();
        ApplyLibraryColumnVisibility();
        BuildLibraryColumnMenu();
    }

    private void PopulateLibraryGrid()
    {
        EnsureLibraryColumns();

        string? previousRoot = SelectedGame()?.RootPath;
        _groupApplied = false;

        gridLibrary.SuspendLayout();
        _suppressLibrarySelection = true;
        try
        {
            if (_libraryGroupBy.Length > 0)
            {
                ConfigureGroupHeaderColumn();
                gridLibrary.SetGroups(_visibleGames, game => GroupKey(game), FillLibraryRow);
                _groupApplied = gridLibrary.IsGrouped;
            }
            else
            {
                gridLibrary.ClearGroups();
                gridLibrary.Rows.Clear();
                foreach (Ps5GameInfo game in OrderLibraryGames())
                    AddLibraryGameRow(game);
            }
        }
        finally
        {
            _suppressLibrarySelection = false;
            gridLibrary.ResumeLayout();
        }

        if (_groupApplied)
        {
            int sortIndex = gridLibrary.Columns.Contains(_librarySortColumn)
                ? gridLibrary.Columns[_librarySortColumn]!.Index
                : gridLibrary.Columns["Title"]!.Index;
            gridLibrary.SortGroups(sortIndex,
                _librarySortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
        }
        else
        {
            ApplyLibrarySortGlyph();
        }

        RestoreLibrarySelection(previousRoot);
        StartLibraryThumbnailLoad(_visibleGames);
    }

    private void RestoreLibrarySelection(string? rootPath)
    {
        if (rootPath is null) return;
        _suppressLibrarySelection = true;
        try
        {
            foreach (DataGridViewRow row in gridLibrary.Rows)
            {
                if (row.Tag is Ps5GameInfo game &&
                    string.Equals(game.RootPath, rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    break;
                }
            }
        }
        finally
        {
            _suppressLibrarySelection = false;
        }
    }

    private IReadOnlyList<Ps5GameInfo> OrderLibraryGames()
    {
        Comparison<Ps5GameInfo> comparison = LibrarySortComparison();
        var list = new List<Ps5GameInfo>(_visibleGames);
        list.Sort((a, b) =>
        {
            int result = comparison(a, b);
            return result != 0 ? result : string.Compare(a.Title, b.Title, StringComparison.CurrentCultureIgnoreCase);
        });
        return list;
    }

    private Comparison<Ps5GameInfo> LibrarySortComparison()
    {
        Func<Ps5GameInfo, object?> selector = _librarySortColumn switch
        {
            "TitleId" => game => game.TitleId,
            "ContentId" => game => game.ContentId,
            "Category" => game => CategoryOf(game),
            "Region" => game => RegionOf(game),
            "Source" => game => game.SourceDescription,
            "Size" => game => game.SourceSize,
            "Version" => game => game.DisplayVersion,
            "Firmware" => game => game.RequiredSystemSoftware,
            "Features" => game => string.Join(", ", game.DeclaredFeatures),
            "Drm" => game => game.DrmType,
            "FileName" => game => LibraryFileName(game),
            "Location" => game => game.RootPath,
            _ => game => game.Title
        };
        int sign = _librarySortAscending ? 1 : -1;
        return (a, b) =>
        {
            object? left = selector(a);
            object? right = selector(b);
            int result = left is long lx && right is long ly
                ? lx.CompareTo(ly)
                : string.Compare(left?.ToString(), right?.ToString(), StringComparison.CurrentCultureIgnoreCase);
            return result * sign;
        };
    }

    private void AddLibraryGameRow(Ps5GameInfo game)
    {
        DataGridViewRow row = gridLibrary.Rows[gridLibrary.Rows.Add()];
        FillLibraryRow(row, game);
    }

    private void FillLibraryRow(DataGridViewRow row, Ps5GameInfo game)
    {
        row.Tag = game;
        row.Cells["Icon"].Value = LibraryIcon(game);
        row.Cells["Title"].Value = game.Title;
        row.Cells["TitleId"].Value = game.TitleId;
        row.Cells["ContentId"].Value = game.ContentId;
        row.Cells["Category"].Value = CategoryOf(game);
        row.Cells["Region"].Value = RegionOf(game);
        row.Cells["Source"].Value = game.SourceDescription;
        row.Cells["Size"].Value = game.SourceSize > 0 ? FormatBytes(game.SourceSize) : string.Empty;
        row.Cells["Version"].Value = game.DisplayVersion;
        row.Cells["Firmware"].Value = game.RequiredSystemSoftware;
        row.Cells["Features"].Value = string.Join(", ", game.DeclaredFeatures);
        row.Cells["Drm"].Value = game.DrmType;
        row.Cells["FileName"].Value = LibraryFileName(game);
        row.Cells["Location"].Value = game.RootPath;
        row.Cells["Title"].ToolTipText = game.Title;
        row.Cells["ContentId"].ToolTipText = game.ContentId;
        row.Cells["Features"].ToolTipText = string.Join(", ", game.DeclaredFeatures);
        row.Cells["FileName"].ToolTipText = game.RootPath;
        row.Cells["Location"].ToolTipText = game.RootPath;
    }

    private void ConfigureGroupHeaderColumn()
    {
        if (gridLibrary.Columns.Contains("Title") && gridLibrary.Columns["Title"]!.Visible)
        {
            gridLibrary.GroupHeaderColumnName = "Title";
        }
        else
        {
            gridLibrary.GroupHeaderColumnName = null;
            gridLibrary.GroupHeaderColumnIndex = FirstVisibleLibraryColumnIndex();
        }
    }

    private int FirstVisibleLibraryColumnIndex()
    {
        int best = 0;
        int bestDisplay = int.MaxValue;
        foreach (DataGridViewColumn column in gridLibrary.Columns)
        {
            if (!column.Visible || column.DisplayIndex >= bestDisplay) continue;
            bestDisplay = column.DisplayIndex;
            best = column.Index;
        }
        return best;
    }

    private void gridLibrary_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.ColumnIndex < 0 || e.ColumnIndex >= gridLibrary.Columns.Count) return;
        string column = gridLibrary.Columns[e.ColumnIndex].Name;
        if (column == "Icon") return;

        if (string.Equals(_librarySortColumn, column, StringComparison.Ordinal))
            _librarySortAscending = !_librarySortAscending;
        else
        {
            _librarySortColumn = column;
            _librarySortAscending = true;
        }

        _settings.LibrarySortColumn = _librarySortColumn;
        _settings.LibrarySortAscending = _librarySortAscending;
        SaveSettingsQuietly();

        if (_groupApplied)
        {
            gridLibrary.SortGroups(e.ColumnIndex,
                _librarySortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending);
            return;
        }

        ApplyFilter();
    }

    private void ApplyLibrarySortGlyph()
    {
        foreach (DataGridViewColumn column in gridLibrary.Columns)
            column.HeaderCell.SortGlyphDirection = SortOrder.None;
        if (gridLibrary.Columns.Contains(_librarySortColumn))
            gridLibrary.Columns[_librarySortColumn]!.HeaderCell.SortGlyphDirection =
                _librarySortAscending ? SortOrder.Ascending : SortOrder.Descending;
    }

    private static Ps5GameInfo? GameOf(DataGridViewRow? row) => row?.Tag as Ps5GameInfo;

    private static string LibraryFileName(Ps5GameInfo game) =>
        Path.GetFileName(game.RootPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    private IEnumerable<Ps5GameInfo> SelectedGames()
    {
        foreach (DataGridViewRow row in gridLibrary.SelectedRows)
            if (row.Tag is Ps5GameInfo game) yield return game;
    }

    private Image LibraryIcon(Ps5GameInfo game)
    {
        if (_settings.ShowThumbnails &&
            _libraryThumbnails.TryGetValue(game.RootPath, out Image? thumbnail) && thumbnail is not null)
            return thumbnail;
        return SourceIcon(game.SourceKind);
    }

    private Image SourceIcon(Ps5SourceKind kind)
    {
        // Image containers (FFPFSC, exFAT, FFPKG) use the container icon. Debug packages keep the
        // package icon, and loose dumps keep the folder icon.
        string key = kind switch
        {
            Ps5SourceKind.LooseDump => "folder",
            Ps5SourceKind.SonyPackage => "package",
            _ => "container"
        };
        return imageListFiles.Images[key] ?? imageListFiles.Images[0] ?? new Bitmap(16, 16);
    }

    private void StartLibraryThumbnailLoad(IReadOnlyList<Ps5GameInfo> ordered)
    {
        if (!_settings.ShowThumbnails) return;
        if (_settings.ThumbnailCacheCount > 0 && _libraryThumbnails.Count >= _settings.ThumbnailCacheCount)
        {
            _libraryThumbnails.Clear();
            _libraryThumbnailAttempts.Clear();
        }

        int version = ++_libraryThumbnailVersion;
        var pending = new List<Ps5GameInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Ps5GameInfo game in ordered)
        {
            if (_libraryThumbnails.ContainsKey(game.RootPath)) continue;
            if (!seen.Add(game.RootPath)) continue;
            if (!_libraryThumbnailAttempts.TryAdd(game.RootPath, 0)) continue;
            pending.Add(game);
        }
        if (pending.Count == 0) return;

        _ = Task.Run(() =>
        {
            foreach (Ps5GameInfo game in pending)
            {
                if (version != _libraryThumbnailVersion) return;
                Image? thumbnail = LoadLibraryThumbnail(game);
                if (thumbnail is null) continue;
                _libraryThumbnails[game.RootPath] = thumbnail;
                try
                {
                    BeginInvoke(() => ApplyLibraryThumbnail(game, thumbnail));
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        });
    }

    private void ApplyLibraryThumbnail(Ps5GameInfo game, Image thumbnail)
    {
        foreach (DataGridViewRow row in gridLibrary.Rows)
        {
            if (ReferenceEquals(row.Tag, game))
            {
                row.Cells["Icon"].Value = thumbnail;
                return;
            }
        }
    }

    private static Image? LoadLibraryThumbnail(Ps5GameInfo game)
    {
        try
        {
            byte[] png = GameFileSystem.ReadFileChunk(game, "sce_sys/icon0.png", 0, 4 * 1024 * 1024).Data;
            if (png.Length == 0) return null;
            using var stream = new MemoryStream(png, writable: false);
            using var source = new Bitmap(stream);
            var target = new Bitmap(30, 30);
            using var graphics = Graphics.FromImage(target);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(source, 0, 0, 30, 30);
            return target;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or
                                   InvalidDataException or NotSupportedException or OutOfMemoryException)
        {
            return null;
        }
    }

    private void ApplyLibraryColumnVisibility()
    {
        foreach (DataGridViewColumn column in gridLibrary.Columns)
            column.Visible = column.Name == "Icon" ||
                !_settings.LibraryHiddenColumns.Contains(column.Name, StringComparer.OrdinalIgnoreCase);
    }

    private void BuildLibraryColumnMenu()
    {
        if (_libraryColumnMenu is not null) return;
        _libraryColumnMenu = new ToolStripMenuItem("Columns");
        foreach ((string name, string header, _, _) in LibraryColumnDefinitions)
        {
            if (name == "Icon") continue;
            string captured = name;
            var item = new ToolStripMenuItem(header)
            {
                CheckOnClick = true,
                Checked = !_settings.LibraryHiddenColumns.Contains(captured, StringComparer.OrdinalIgnoreCase)
            };
            item.CheckedChanged += (_, _) =>
            {
                if (item.Checked)
                    _settings.LibraryHiddenColumns.RemoveAll(entry => string.Equals(entry, captured, StringComparison.OrdinalIgnoreCase));
                else if (!_settings.LibraryHiddenColumns.Contains(captured, StringComparer.OrdinalIgnoreCase))
                    _settings.LibraryHiddenColumns.Add(captured);
                ApplyLibraryColumnVisibility();
                SaveSettingsQuietly();
            };
            _libraryColumnMenu.DropDownItems.Add(item);
        }
        contextLibrary.Items.Add(new ToolStripSeparator());
        contextLibrary.Items.Add(_libraryColumnMenu);
    }

    private void RestoreLibraryColumnLayout()
    {
        var order = new List<string>();
        foreach (string name in _settings.LibraryColumnOrder)
            if (gridLibrary.Columns.Contains(name) && !order.Contains(name, StringComparer.Ordinal))
                order.Add(name);
        foreach (DataGridViewColumn column in gridLibrary.Columns.Cast<DataGridViewColumn>().OrderBy(column => column.Index))
            if (!order.Contains(column.Name, StringComparer.Ordinal))
                order.Add(column.Name);

        for (int index = 0; index < order.Count; index++)
            gridLibrary.Columns[order[index]]!.DisplayIndex = index;
    }

    private void CaptureLibraryColumnLayout()
    {
        if (!_libraryColumnsReady) return;
        _settings.LibraryColumnOrder = gridLibrary.Columns
            .Cast<DataGridViewColumn>()
            .OrderBy(column => column.DisplayIndex)
            .Select(column => column.Name)
            .ToList();
    }
}
