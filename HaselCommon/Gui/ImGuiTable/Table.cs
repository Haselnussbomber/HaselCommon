using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public static class Table
{
    public const float ArrowWidth = 10;
}

public class Table<T> : IDisposable
{
    protected readonly LanguageProvider _languageProvider;
    protected List<T> _rows = [];
    protected List<T>? _filteredRows;

    public string Id { get; set; }
    public List<T> Rows
    {
        get => _rows;
        set { _rows = value; IsSortDirty |= true; }
    }
    public Column<T>[] Columns { get; set; } = [];
    public bool IsFilterDirty { get; set; } = true;
    public bool IsSortDirty { get; set; } = true;
    public float? LineHeight { get; set; } = null;

    protected bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
      | ImGuiTableFlags.Sortable
      | ImGuiTableFlags.BordersOuter
      | ImGuiTableFlags.ScrollY
      | ImGuiTableFlags.BordersInnerV
      | ImGuiTableFlags.NoBordersInBodyUntilResize;

    public Table(string id, LanguageProvider languageProvider)
    {
        Id = id;
        _languageProvider = languageProvider;
        _languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }

    protected virtual void OnLanguageChanged(string langCode)
        => IsSortDirty |= true;

    public void Draw()
    {
        using var id = ImRaii.PushId(Id);
        DrawTableInternal();
    }

    protected virtual void DrawFilters()
        => throw new NotImplementedException();

    private void SortInternal()
    {
        if (!Sortable)
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        IsSortDirty |= sortSpecs.SpecsDirty;

        if (!IsSortDirty)
            return;

        var sortIdx = sortSpecs.Specs.ColumnIndex;
        if (Columns.Length <= sortIdx)
            sortIdx = 0;

        if (sortSpecs.SpecsCount == 0)
        {
            if (Flags.HasFlag(ImGuiTableFlags.SortTristate))
                SortTristate();
        }
        else
        {
            var column = Columns[sortIdx];
            _rows.Sort((a, b) => (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending ? -1 : 1) * column.Compare(a, b));
            _filteredRows = null;
        }

        sortSpecs.SpecsDirty = IsSortDirty = false;
    }

    protected virtual void SortTristate()
    {
        throw new NotImplementedException();
    }

    private void UpdateFilter()
    {
        if (_filteredRows != null && !IsFilterDirty)
            return;

        _filteredRows?.Clear();
        _filteredRows ??= [];
        _filteredRows.AddRange(_rows.Where(row => Columns.All(column => column.ShouldShow(row))));

        IsFilterDirty = false;
    }

    private void DrawTableInternal()
    {
        using var table = ImRaii.Table("Table", Columns.Length, Flags, ImGui.GetContentRegionAvail());
        if (!table)
            return;

        ImGui.TableSetupScrollFreeze(1, 1);

        foreach (var column in Columns)
            ImGui.TableSetupColumn(column.Label, column.Flags, column.Width * ImGuiHelpers.GlobalScale);

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        for (var columnIndex = 0; columnIndex < Columns.Length; columnIndex++)
        {
            var column = Columns[columnIndex];

            if (!ImGui.TableSetColumnIndex(columnIndex))
                continue;

            using var id = ImRaii.PushId($"ColumnHeader{columnIndex}");

            using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero))
            {
                ImGui.TableHeader(string.Empty);
                ImGui.SameLine();
            }

            IsFilterDirty |= column.DrawFilter();
        }

        SortInternal();
        UpdateFilter();

        ImGuiClip.ClippedDraw(_filteredRows!, DrawItem, LineHeight ?? ImGui.GetTextLineHeightWithSpacing());
    }

    private void DrawItem(T row, int rowIndex)
    {
        var column = 0;
        using var id = ImRaii.PushId(rowIndex);
        foreach (var header in Columns)
        {
            id.Push(column++);
            if (ImGui.TableNextColumn())
                header.DrawColumn(row);
            id.Pop();
        }
    }
}
