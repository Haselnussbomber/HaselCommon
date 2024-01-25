using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using HaselCommon.Extensions;
using ImGuiNET;

namespace HaselCommon.Utils;

public static class Table
{
    public const float ArrowWidth = 10;
}

public class Table<T>
{
    protected readonly ICollection<T> Items;
    internal readonly List<(T, int)> FilteredItems;

    protected readonly string Label;
    protected readonly Column<T>[] Headers;

    protected bool filterDirty = true;
    protected bool sortDirty = true;

    protected float ItemHeight { get; set; }

    public float ExtraHeight { get; set; } = 0;

    private int currentIdx = 0;

    protected bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }

    protected int sortIdx = -1;

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
      | ImGuiTableFlags.Sortable
      | ImGuiTableFlags.BordersOuter
      | ImGuiTableFlags.ScrollY
      | ImGuiTableFlags.ScrollX
      | ImGuiTableFlags.PreciseWidths
      | ImGuiTableFlags.BordersInnerV
      | ImGuiTableFlags.NoBordersInBodyUntilResize;

    public int TotalItems
        => Items.Count;

    public int CurrentItems
        => FilteredItems.Count;

    public int TotalColumns
        => Headers.Length;

    public int VisibleColumns { get; private set; }

    public Table(string label, ICollection<T> items, params Column<T>[] headers)
    {
        Label = label;
        Items = items;
        Headers = headers;
        FilteredItems = new List<(T, int)>(Items.Count);
        VisibleColumns = Headers.Length;
    }

    public void Draw(float itemHeight)
    {
        ItemHeight = itemHeight;
        using var id = ImRaii.PushId(Label);
        UpdateFilter();
        DrawTableInternal();
    }

    protected virtual void DrawFilters()
        => throw new NotImplementedException();

    protected virtual void PreDraw()
    {
    }

    private void SortInternal()
    {
        if (!Sortable)
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        sortDirty |= sortSpecs.SpecsDirty;

        if (!sortDirty)
            return;

        sortIdx = sortSpecs.Specs.ColumnIndex;

        if (Headers.Length <= sortIdx)
            sortIdx = 0;

        if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
            FilteredItems.StableSort((a, b) => Headers[sortIdx].Compare(a.Item1, b.Item1));
        else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
            FilteredItems.StableSort((a, b) => Headers[sortIdx].CompareInv(a.Item1, b.Item1));
        else
            sortIdx = -1;

        sortDirty = false;
        sortSpecs.SpecsDirty = false;
    }

    private void UpdateFilter()
    {
        if (!filterDirty)
            return;

        FilteredItems.Clear();
        var idx = 0;
        foreach (var item in Items)
        {
            if (Headers.All(header => header.FilterFunc(item)))
                FilteredItems.Add((item, idx));
            idx++;
        }

        filterDirty = false;
        sortDirty = true;
    }

    private void DrawItem((T Item, int Index) pair)
    {
        var column = 0;
        using var id = ImRaii.PushId(currentIdx);
        currentIdx = pair.Index;
        foreach (var header in Headers)
        {
            id.Push(column++);
            if (ImGui.TableNextColumn())
                header.DrawColumn(pair.Item, pair.Index);
            id.Pop();
        }
    }

    private void DrawTableInternal()
    {
        using var table = ImRaii.Table("Table", Headers.Length, Flags,
            ImGui.GetContentRegionAvail() - ExtraHeight * Vector2.UnitY * ImGuiHelpers.GlobalScale);
        if (!table)
            return;

        PreDraw();
        ImGui.TableSetupScrollFreeze(1, 1);

        foreach (var header in Headers)
            ImGui.TableSetupColumn(t(header.LabelKey), header.Flags, header.Width);

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        var i = 0;
        VisibleColumns = 0;
        foreach (var header in Headers)
        {
            using var id = ImRaii.PushId(i);
            if (ImGui.TableGetColumnFlags(i).HasFlag(ImGuiTableColumnFlags.IsEnabled))
                ++VisibleColumns;
            if (!ImGui.TableSetColumnIndex(i++))
                continue;

            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            ImGui.TableHeader(string.Empty);
            ImGui.SameLine();
            style.Pop();
            if (header.DrawFilter())
                filterDirty = true;
        }

        SortInternal();
        currentIdx = 0;
        ImGuiClip.ClippedDraw(FilteredItems, DrawItem, ItemHeight);
    }
}
