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
        _settings.LibraryColumnWeights ??= [];

        gridLibrary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridLibrary.AutoSortGroups = false;
        // Grouped rows sort on cell values; compare byte-size text numerically so 10 GB sorts after 2 GB.
        gridLibrary.GroupCellValueComparer = CompareLibraryCellValues;
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
                FillWeight = ResolveLibraryColumnWeight(name, weight),
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

        // Capture the whole selection and the focused row by stable root path, not just one.
        string[] selectedRoots = SelectedGames().Select(game => game.RootPath).ToArray();
        string? focusedRoot = SelectedGame()?.RootPath;
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

        RestoreLibrarySelection(selectedRoots, focusedRoot);

        // On the first load there is no previous selection. The grid highlights the first row while
        // populating (selection suppressed), so promote it to a real selection to load the details
        // pane and the Tools source immediately.
        if (selectedRoots.Length == 0 && _visibleGames.Count > 0)
        {
            DataGridViewRow? first = null;
            foreach (DataGridViewRow row in gridLibrary.Rows)
                if (row.Tag is Ps5GameInfo) { first = row; break; }
            if (first is not null)
            {
                gridLibrary.ClearSelection();
                first.Selected = true;
            }
        }

        StartLibraryThumbnailLoad(_visibleGames);
    }

    /// <summary>
    /// Restores the full multi-selection and the focused row by stable root path after a rebuild.
    /// Identities that are no longer visible are dropped, so a narrowed view never keeps an
    /// invisible destructive scope.
    /// </summary>
    private void RestoreLibrarySelection(string[] roots, string? focusedRoot)
    {
        if (roots.Length == 0 && focusedRoot is null) return;
        var wanted = new HashSet<string>(roots, StringComparer.OrdinalIgnoreCase);
        _suppressLibrarySelection = true;
        try
        {
            gridLibrary.ClearSelection();
            DataGridViewRow? focused = null;
            foreach (DataGridViewRow row in gridLibrary.Rows)
            {
                if (row.Tag is not Ps5GameInfo game) continue;
                if (wanted.Contains(game.RootPath))
                {
                    row.Selected = true;
                    focused ??= row;
                }
                if (focusedRoot is not null &&
                    string.Equals(game.RootPath, focusedRoot, StringComparison.OrdinalIgnoreCase))
                    focused = row;
            }
            if (focused is { Index: >= 0 })
            {
                gridLibrary.CurrentCell = focused.Cells[0];
                gridLibrary.FirstDisplayedScrollingRowIndex = focused.Index;
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
            // Typed keys so 1.10 sorts after 1.9 instead of lexically.
            "Version" => game => VersionKey.Parse(game.DisplayVersion),
            "Firmware" => game => VersionKey.Parse(game.RequiredSystemSoftware),
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
                : left is VersionKey vx && right is VersionKey vy
                    ? vx.CompareTo(vy)
                    : string.Compare(left?.ToString(), right?.ToString(), StringComparison.CurrentCultureIgnoreCase);
            return result * sign;
        };
    }

    /// <summary>
    /// Orders dotted numeric version/system strings by value (1.10 after 1.9) with a text tie-break.
    /// Non-numeric or unknown values stay comparable and sort before/after per their text.
    /// </summary>
    private readonly record struct VersionKey(long Packed, string Raw) : IComparable<VersionKey>
    {
        public static VersionKey Parse(string? value)
        {
            string raw = value ?? string.Empty;
            long packed = 0;
            long current = 0;
            bool any = false;
            int groups = 0;
            foreach (char c in raw)
            {
                if (char.IsDigit(c))
                {
                    current = Math.Min(current * 10 + (c - '0'), 9_999);
                    any = true;
                }
                else if (c == '.')
                {
                    packed = packed * 10_000 + current;
                    current = 0;
                    any = false;
                    if (++groups >= 3) break;
                }
            }
            if (any) packed = packed * 10_000 + current;
            return new VersionKey(packed, raw);
        }

        public int CompareTo(VersionKey other)
        {
            int byValue = Packed.CompareTo(other.Packed);
            return byValue != 0 ? byValue : string.Compare(Raw, other.Raw, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Numeric-aware comparison for grouped cell values (formatted byte sizes).</summary>
    private static int CompareLibraryCellValues(object? left, object? right)
    {
        if (TryParseSize(left, out double a) && TryParseSize(right, out double b)) return a.CompareTo(b);
        return string.Compare(left?.ToString(), right?.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>Parses a "3.20 GiB" style size string (as produced by FormatBytes) back to bytes.</summary>
    private static bool TryParseSize(object? value, out double bytes)
    {
        bytes = 0;
        if (value is not string text || text.Length == 0) return false;
        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !double.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.CurrentCulture, out double amount))
            return false;
        double scale = parts[1] switch
        {
            "B" => 1d,
            "KiB" => 1024d,
            "MiB" => 1024d * 1024,
            "GiB" => 1024d * 1024 * 1024,
            "TiB" => 1024d * 1024 * 1024 * 1024,
            _ => 0d
        };
        if (scale == 0d) return false;
        bytes = amount * scale;
        return true;
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
        row.Cells["Size"].ToolTipText = game.SourceSize > 0
            ? FormatBytes(game.SourceSize)
            : game.SourceKind == Ps5SourceKind.LooseDump
                ? "Open this dump to calculate its size."
                : string.Empty;

        if (SourceExists(game))
        {
            row.Cells["Source"].ToolTipText = string.Empty;
            row.DefaultCellStyle.ForeColor = Color.Empty;
            row.DefaultCellStyle.SelectionForeColor = Color.Empty;
        }
        else
        {
            row.Cells["Source"].Value = "Missing";
            row.Cells["Source"].ToolTipText = "Source not found: " + game.RootPath;
            row.DefaultCellStyle.ForeColor = Color.FromArgb(208, 128, 128);
            row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(255, 190, 190);
        }
    }

    private void RefreshLibrarySizeCell(Ps5GameInfo game)
    {
        foreach (DataGridViewRow row in gridLibrary.Rows)
        {
            if (row.Tag is Ps5GameInfo candidate &&
                string.Equals(candidate.RootPath, game.RootPath, StringComparison.OrdinalIgnoreCase))
            {
                row.Cells["Size"].Value = game.SourceSize > 0 ? FormatBytes(game.SourceSize) : string.Empty;
                row.Cells["Size"].ToolTipText = game.SourceSize > 0 ? FormatBytes(game.SourceSize) : string.Empty;
                return;
            }
        }
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
            ClearLibraryThumbnails();

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
                // A newer rebuild may have started while this image decoded; drop the stale result.
                if (version != _libraryThumbnailVersion) { thumbnail.Dispose(); return; }
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

    /// <summary>
    /// Drops every cached thumbnail. Images still shown by a grid cell are left for the GC rather than
    /// disposed, so a rebuild can never paint a disposed image; the rest are released immediately.
    /// </summary>
    private void ClearLibraryThumbnails()
    {
        var referenced = new HashSet<Image>();
        foreach (DataGridViewRow row in gridLibrary.Rows)
            if (row.Cells["Icon"].Value is Image image) referenced.Add(image);
        foreach (Image image in _libraryThumbnails.Values)
            if (image is not null && !referenced.Contains(image)) image.Dispose();
        _libraryThumbnails.Clear();
        _libraryThumbnailAttempts.Clear();
    }

    /// <summary>
    /// Drops the cached thumbnail for a single (moved or removed) source. The image is not disposed
    /// here because the grid may still be painting its row until the next rebuild.
    /// </summary>
    private void ForgetLibraryThumbnail(string rootPath)
    {
        _libraryThumbnails.TryRemove(rootPath, out _);
        _libraryThumbnailAttempts.TryRemove(rootPath, out _);
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
        var resetView = new ToolStripMenuItem("Reset view");
        resetView.Click += (_, _) => ResetLibraryView();
        contextLibrary.Items.Add(new ToolStripSeparator());
        contextLibrary.Items.Add(resetView);
        contextLibrary.Items.Add(_libraryColumnMenu);
    }

    /// <summary>
    /// Restores the default layout — Title ascending sort, no grouping, all columns shown in their
    /// declared order — without touching the filter query or selections.
    /// </summary>
    private void ResetLibraryView()
    {
        _librarySortColumn = "Title";
        _librarySortAscending = true;
        _settings.LibrarySortColumn = _librarySortColumn;
        _settings.LibrarySortAscending = _librarySortAscending;

        _settings.LibraryHiddenColumns.Clear();
        _settings.LibraryColumnWeights.Clear();
        _settings.LibraryColumnOrder = LibraryColumnDefinitions.Select(definition => definition.Name).ToList();
        ApplyLibraryColumnVisibility();
        ApplyLibraryColumnWeights();
        RestoreLibraryColumnLayout();
        foreach (DataGridViewColumn column in gridLibrary.Columns)
            column.HeaderCell.SortGlyphDirection = SortOrder.None;

        SetGroupBy(string.Empty);
        ApplyFilter();
        SaveSettingsQuietly();
        statusLabel.Text = "View reset to defaults.";
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
        CaptureLibraryColumnWeights();
    }

    /// <summary>Remembers the current column fill weights (proportional widths) for the next session.</summary>
    private void CaptureLibraryColumnWeights()
    {
        if (!_libraryColumnsReady) return;
        _settings.LibraryColumnWeights ??= [];
        foreach (DataGridViewColumn column in gridLibrary.Columns)
        {
            if (column.Name == "Icon" || !column.Visible) continue;
            _settings.LibraryColumnWeights[column.Name] = column.FillWeight;
        }
    }

    /// <summary>The saved width for a column, falling back to the declared default weight.</summary>
    private float ResolveLibraryColumnWeight(string name, float fallback)
    {
        if (_settings.LibraryColumnWeights is { } weights &&
            weights.TryGetValue(name, out float saved) && saved > 0)
            return saved;
        return fallback;
    }

    /// <summary>Re-applies the resolved weights to every column (used after resetting the view).</summary>
    private void ApplyLibraryColumnWeights()
    {
        foreach (DataGridViewColumn column in gridLibrary.Columns)
        {
            if (column.Name == "Icon") continue;
            float fallback = LibraryColumnDefinitions
                .FirstOrDefault(definition => string.Equals(definition.Name, column.Name, StringComparison.Ordinal)).Weight;
            column.FillWeight = ResolveLibraryColumnWeight(column.Name, fallback <= 0 ? 1 : fallback);
        }
    }
}
