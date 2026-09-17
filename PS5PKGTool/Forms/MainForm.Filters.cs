using System.Globalization;
using System.Text;
using DarkUI.Controls;
using PS5PKGTool.Core.Models;

namespace PS5PKGTool.Forms;

public partial class MainForm
{
    private bool _suppressFilterEvents;

    private void cboFilter_CheckedItemsChanged(object? sender, EventArgs e)
    {
        if (!_suppressFilterEvents) ApplyFilter();
    }

    private void btnFilterClear_Click(object? sender, EventArgs e) => ClearFilters();

    private static HashSet<string> SelectedValues(DarkCheckedComboBox combo)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (object? item in combo.CheckedItems)
            if (item is not null) result.Add(item.ToString() ?? string.Empty);
        return result;
    }

    private static string SourceFilterValue(Ps5GameInfo game) => game.SourceKind switch
    {
        Ps5SourceKind.LooseDump => "Dump Files",
        Ps5SourceKind.SonyPackage => "PKG",
        Ps5SourceKind.Ffpfsc => "FFPFSC",
        Ps5SourceKind.FilesystemImage => "exFAT",
        Ps5SourceKind.Ffpkg => "FFPKG",
        _ => "Other"
    };

    private bool MatchesFilters(Ps5GameInfo game)
    {
        HashSet<string> categories = SelectedValues(cboFilterCategory);
        if (categories.Count > 0 && !categories.Contains(CategoryOf(game))) return false;

        HashSet<string> regions = SelectedValues(cboFilterRegion);
        if (regions.Count > 0 && !regions.Contains(RegionOf(game))) return false;

        HashSet<string> sources = SelectedValues(cboFilterFormat);
        if (sources.Count > 0 && !sources.Contains(SourceFilterValue(game))) return false;

        return true;
    }

    private static readonly string[] FilterGroupKeys = ["", "family", "titleid", "category", "region", "source", "firmware"];

    private void ClearFilters()
    {
        _suppressFilterEvents = true;
        try
        {
            cboFilterCategory.SetAllItemsChecked(false);
            cboFilterRegion.SetAllItemsChecked(false);
            cboFilterFormat.SetAllItemsChecked(false);
            searchLibrary.SearchText = string.Empty;
        }
        finally
        {
            _suppressFilterEvents = false;
        }
        ApplyFilter();
    }

    private void cboFilterGroup_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_suppressFilterEvents) return;
        int index = cboFilterGroup.SelectedIndex;
        SetGroupBy(index >= 0 && index < FilterGroupKeys.Length ? FilterGroupKeys[index] : string.Empty);
    }

    private void cboFilterPreset_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int preset = cboFilterPreset.SelectedIndex;
        if (_suppressFilterEvents || preset <= 0) return;
        _suppressFilterEvents = true;
        try
        {
            cboFilterCategory.SetAllItemsChecked(false);
            cboFilterRegion.SetAllItemsChecked(false);
            cboFilterFormat.SetAllItemsChecked(false);
            switch (preset)
            {
                // All means all: clear the query too, not just the checkbox filters.
                case 1: searchLibrary.SearchText = string.Empty; break;
                case 2: SetComboItem(cboFilterCategory, "Game"); break;
                case 3: SetComboItem(cboFilterCategory, "Patch"); break;
                case 4: SetComboItem(cboFilterCategory, "DLC"); break;
                case 5: SetComboItem(cboFilterFormat, "Dump Files"); break;
                case 6: SetComboItem(cboFilterFormat, "PKG"); break;
                case 7: SetComboItem(cboFilterFormat, "FFPKG"); break;
                case 8: SetComboItem(cboFilterFormat, "FFPFSC"); break;
                case 9: SetComboItem(cboFilterFormat, "exFAT"); break;
            }
        }
        finally
        {
            _suppressFilterEvents = false;
            cboFilterPreset.SelectedIndex = 0;
        }
        ApplyFilter();
    }

    /// <summary>Replaces a combo's checked set (used when restoring a saved view).</summary>
    private static void SetCheckedItems(DarkCheckedComboBox combo, IEnumerable<string> values)
    {
        combo.SetAllItemsChecked(false);
        foreach (string value in values) SetComboItem(combo, value);
    }

    private static void SetComboItem(DarkCheckedComboBox combo, string value)
    {
        for (int index = 0; index < combo.Items.Count; index++)
        {
            if (string.Equals(combo.Items[index]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                combo.SetItemChecked(index, true);
                return;
            }
        }
    }

    private void SyncFilterGroupCombo()
    {
        _suppressFilterEvents = true;
        try
        {
            int index = Array.IndexOf(FilterGroupKeys, _libraryGroupBy);
            cboFilterGroup.SelectedIndex = index >= 0 ? index : 0;
        }
        finally
        {
            _suppressFilterEvents = false;
        }
    }

    private void SyncFilterControls()
    {
        _suppressFilterEvents = true;
        try
        {
            int index = Array.IndexOf(FilterGroupKeys, _libraryGroupBy);
            cboFilterGroup.SelectedIndex = index >= 0 ? index : 0;
        }
        finally
        {
            _suppressFilterEvents = false;
        }
    }

    private void UpdateFilterBadges()
    {
        UpdateFilterCaption(lblFilterCategory, "Category", cboFilterCategory);
        UpdateFilterCaption(lblFilterRegion, "Region", cboFilterRegion);
        UpdateFilterCaption(lblFilterFormat, "Format", cboFilterFormat);
        // Grouping is a view setting, not a filter, so it does not keep the reset button active.
        bool active = cboFilterCategory.CheckedItems.Count > 0 || cboFilterRegion.CheckedItems.Count > 0 ||
                      cboFilterFormat.CheckedItems.Count > 0 || searchLibrary.SearchText.Length > 0;
        btnFilterClear.Text = "Reset filters";
        btnFilterClear.Visible = active;
    }

    private static void UpdateFilterCaption(DarkLabel label, string caption, DarkCheckedComboBox combo) =>
        label.Text = combo.CheckedItems.Count > 0 ? $"{caption} ({combo.CheckedItems.Count})" : caption;

    private void RebuildFilterChips()
    {
        chipsFilter.SuspendLayout();
        chipsFilter.Controls.Clear();
        string query = searchLibrary.SearchText.Trim();
        if (query.Length > 0)
            chipsFilter.AddChip("Search: " + query, (_, _) => searchLibrary.SearchText = string.Empty);
        if (ValidateQuery(query) is string warning)
            chipsFilter.AddChip("Check query: " + warning, (_, _) => { });
        AddChips(cboFilterCategory);
        AddChips(cboFilterRegion);
        AddChips(cboFilterFormat);
        chipsFilter.ResumeLayout();
    }

    private void AddChips(DarkCheckedComboBox combo)
    {
        foreach (object? item in combo.CheckedItems)
        {
            if (item is null) continue;
            string value = item.ToString() ?? string.Empty;
            chipsFilter.AddChip(value, (_, _) => Uncheck(combo, value));
        }
    }

    private static void Uncheck(DarkCheckedComboBox combo, string value)
    {
        for (int index = 0; index < combo.Items.Count; index++)
        {
            if (string.Equals(combo.Items[index]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                combo.SetItemChecked(index, false);
                return;
            }
        }
    }

    private CancellationTokenSource? _searchDebounce;

    private async void searchLibrary_SearchTextChanged(object? sender, EventArgs e)
    {
        _searchDebounce?.Cancel();
        var cts = new CancellationTokenSource();
        _searchDebounce = cts;
        try
        {
            await Task.Delay(200, cts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        if (!cts.IsCancellationRequested) ApplyFilter();
    }

    /// <summary>
    /// Matches a game against the search box. Supports space-separated AND tokens, quoted phrases,
    /// field prefixes (<c>title:</c>, <c>id:</c>, <c>content:</c>, <c>category:</c>, <c>region:</c>,
    /// <c>size:</c>, <c>version:</c>, <c>fw:</c>, <c>feature:</c>, <c>drm:</c>, <c>path:</c>),
    /// OR groups with <c>|</c>, numeric comparisons (<c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>,
    /// <c>&lt;=</c>, <c>=</c>) and <c>-</c> negation.
    /// </summary>
    private readonly List<(string Body, bool Negate)> _queryTokens = [];

    /// <summary>Tokenizes the query once per filter pass instead of once per record.</summary>
    private void ParseQueryTokens(string query)
    {
        _queryTokens.Clear();
        if (string.IsNullOrWhiteSpace(query)) return;
        foreach (string token in TokenizeQuery(query))
        {
            bool negate = token.Length > 1 && token[0] == '-';
            string body = negate ? token[1..] : token;
            if (body.Length == 0) continue;
            _queryTokens.Add((body, negate));
        }
    }

    private bool MatchesQueryTokens(Ps5GameInfo game)
    {
        foreach ((string body, bool negate) in _queryTokens)
        {
            int colon = body.IndexOf(':');
            bool match = colon > 0
                ? MatchField(game, body[..colon].ToLowerInvariant(), body[(colon + 1)..])
                : MatchFreeText(game, body);
            if (negate) match = !match;
            if (!match) return false;
        }
        return true;
    }

    private static IEnumerable<string> TokenizeQuery(string query)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool quoted = false;
        foreach (char c in query)
        {
            if (c == '"')
            {
                quoted = !quoted;
                continue;
            }
            if (!quoted && char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }
            current.Append(c);
        }
        if (current.Length > 0) tokens.Add(current.ToString());
        return tokens;
    }

    private static readonly string[] QueryFieldKeys =
    [
        "title", "id", "titleid", "title-id", "content", "contentid", "content-id",
        "category", "role", "region", "source", "format", "drm", "path", "location",
        "feature", "features", "size", "version", "fw", "firmware"
    ];

    /// <summary>
    /// Returns a human-readable hint when the query cannot be understood (unbalanced quotes, an unknown
    /// field prefix, a missing value, or a malformed size/version comparison), or null when it is fine.
    /// The query still runs — this only surfaces the likely mistake inline.
    /// </summary>
    private static string? ValidateQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return null;
        if (query.Count(c => c == '"') % 2 != 0)
            return "unbalanced quotation marks.";

        foreach (string token in TokenizeQuery(query))
        {
            string body = token.Length > 1 && token[0] == '-' ? token[1..] : token;
            if (body.Length == 0) continue;
            int colon = body.IndexOf(':');
            if (colon <= 0) continue;

            string field = body[..colon].ToLowerInvariant();
            string value = body[(colon + 1)..];
            if (Array.IndexOf(QueryFieldKeys, field) < 0)
                return $"unknown field '{field}:'. Valid fields: title, id, content, category, role, region, source, size, version, fw, feature, drm, path.";
            if (value.Length == 0)
                return $"'{field}:' needs a value.";

            if (field == "size" &&
                !(TryParseComparison(value, out _, out string sizeRest) && TryParseSize(sizeRest, out _)))
                return $"'{value}' is not a valid size (try size:>50GB).";
            if (field is "version" or "fw" or "firmware")
            {
                if (!TryParseComparison(value, out _, out string versionRest) || versionRest.Length == 0)
                    return $"'{field}:' is missing a value after the comparison.";
                if (CompareVersions(versionRest, versionRest) is null)
                    return $"'{versionRest}' is not a valid version.";
            }
        }
        return null;
    }

    private static bool MatchFreeText(Ps5GameInfo game, string value) =>
        MatchAny(value, part =>
            ContainsText(game.Title, part) ||
            ContainsText(game.TitleId, part) ||
            ContainsText(game.ContentId, part) ||
            ContainsText(game.RootPath, part) ||
            ContainsText(game.SourceDescription, part));

    private bool MatchField(Ps5GameInfo game, string field, string value) => field switch
    {
        "title" => MatchText(game.Title, value),
        "id" or "titleid" or "title-id" => MatchText(game.TitleId, value),
        "content" or "contentid" or "content-id" => MatchText(game.ContentId, value),
        "category" => MatchAny(value, part => ContainsText(CategoryOf(game), part)),
        "role" => MatchAny(value, part => ContainsText(RelationshipLabel(game), part)),
        "region" => MatchAny(value, part => ContainsText(RegionOf(game), part)),
        "source" or "format" => MatchAny(value, part =>
            ContainsText(game.SourceDescription, part) || ContainsText(SourceFilterValue(game), part)),
        "drm" => MatchAny(value, part => ContainsText(game.DrmType, part)),
        "path" or "location" => MatchAny(value, part => ContainsText(game.RootPath, part)),
        "feature" or "features" => MatchAny(value, part => game.DeclaredFeatures.Any(feature => ContainsText(feature, part))),
        "size" => MatchSize(value, game.SourceSize),
        "version" => MatchVersion(value, game.DisplayVersion),
        "fw" or "firmware" => MatchVersion(value, game.RequiredSystemSoftware),
        _ => MatchFreeText(game, value)
    };

    /// <summary>
    /// Text field match: a leading <c>=</c> means exact identity (<c>id:=PPSA12345</c>), otherwise a
    /// value-level <c>|</c> contains match.
    /// </summary>
    private static bool MatchText(string? source, string value)
    {
        if (value.StartsWith('='))
            return string.Equals(source, value[1..].Trim(), StringComparison.OrdinalIgnoreCase);
        return MatchAny(value, part => ContainsText(source, part));
    }

    private static bool MatchAny(string value, Func<string, bool> predicate)
    {
        foreach (string part in value.Split('|', StringSplitOptions.RemoveEmptyEntries))
            if (predicate(part)) return true;
        return false;
    }

    private static bool ContainsText(string? source, string value) =>
        !string.IsNullOrEmpty(source) && source.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static bool MatchSize(string value, long size)
    {
        if (TryParseComparison(value, out string op, out string rest) && TryParseSize(rest, out long target))
            return Satisfies(size, target, op);
        return TryParseSize(value, out long minimum) && size >= minimum;
    }

    private static bool MatchVersion(string value, string candidate)
    {
        if (TryParseComparison(value, out string op, out string rest))
            return CompareVersions(candidate, rest) is int comparison && Satisfies(comparison, op);
        return ContainsText(candidate, value);
    }

    private static bool TryParseComparison(string value, out string op, out string rest)
    {
        foreach (string candidate in new[] { ">=", "<=", ">", "<", "=" })
        {
            if (value.StartsWith(candidate, StringComparison.Ordinal))
            {
                op = candidate;
                rest = value[candidate.Length..].Trim();
                return true;
            }
        }
        op = "=";
        rest = value;
        return false;
    }

    private static bool Satisfies(long value, long target, string op) => op switch
    {
        ">" => value > target,
        "<" => value < target,
        ">=" => value >= target,
        "<=" => value <= target,
        _ => value == target
    };

    private static bool Satisfies(int comparison, string op) => op switch
    {
        ">" => comparison > 0,
        "<" => comparison < 0,
        ">=" => comparison >= 0,
        "<=" => comparison <= 0,
        _ => comparison == 0
    };

    private static int? CompareVersions(string? left, string? right)
    {
        long[]? a = ParseVersion(left);
        long[]? b = ParseVersion(right);
        if (a is null || b is null) return null;
        int length = Math.Max(a.Length, b.Length);
        for (int i = 0; i < length; i++)
        {
            long x = i < a.Length ? a[i] : 0;
            long y = i < b.Length ? b[i] : 0;
            if (x != y) return x < y ? -1 : 1;
        }
        return 0;
    }

    private static long[]? ParseVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts = new List<long>();
        foreach (string piece in value.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            int digits = 0;
            while (digits < piece.Length && char.IsDigit(piece[digits])) digits++;
            if (digits == 0) break;
            // Guard against numeric overflow from user input instead of throwing out of the search.
            if (!long.TryParse(piece[..digits], NumberStyles.None, CultureInfo.InvariantCulture, out long number))
                return null;
            parts.Add(number);
        }
        return parts.Count > 0 ? parts.ToArray() : null;
    }

    private static bool TryParseSize(string value, out long bytes)
    {
        bytes = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;
        string text = value.Trim().ToUpperInvariant().Replace(" ", string.Empty);
        int digits = 0;
        while (digits < text.Length && (char.IsDigit(text[digits]) || text[digits] == '.')) digits++;
        if (digits == 0 ||
            !double.TryParse(text[..digits], NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
            return false;
        double multiplier = text[digits..] switch
        {
            "" or "B" => 1d,
            "K" or "KB" => 1024d,
            "M" or "MB" => 1024d * 1024d,
            "G" or "GB" => 1024d * 1024d * 1024d,
            "T" or "TB" => 1024d * 1024d * 1024d * 1024d,
            _ => 0d
        };
        if (multiplier == 0d) return false;
        bytes = (long)(number * multiplier);
        return true;
    }
}
