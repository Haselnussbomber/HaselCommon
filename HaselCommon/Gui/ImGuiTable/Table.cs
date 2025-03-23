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
    protected bool _closePopupNextFrame;

    public string? Id { get; set; }
    public List<T> Rows
    {
        get => _rows;
        set { _rows = value; RowsLoaded = true; IsSortDirty |= true; }
    }
    public List<T>? FilteredRows => _filteredRows;
    public bool RowsLoaded { get; set; }
    public List<Column<T>> Columns { get; set; } = [];
    public bool IsFilterDirty { get; set; } = true;
    public bool IsSortDirty { get; set; } = true;
    public float? LineHeight { get; set; } = null;
    public int ScrollFreezeCols { get; set; } = 1;
    public bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }
    public bool ClosePopup { get; set; }

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
      | ImGuiTableFlags.Sortable
      | ImGuiTableFlags.BordersOuter
      | ImGuiTableFlags.ScrollY
      | ImGuiTableFlags.BordersInnerV
      | ImGuiTableFlags.NoBordersInBodyUntilResize;

    public Table(LanguageProvider languageProvider)
    {
        _languageProvider = languageProvider;
        _languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public virtual void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
    }

    public virtual void OnLanguageChanged(string langCode)
    {
        if (RowsLoaded)
        {
            Rows.Clear();
            LoadRows();

            foreach (var column in Columns)
            {
                column.OnLanguageChanged(langCode);
                column.UpdateLabel();
            }
        }

        IsSortDirty |= true;
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Id!, !string.IsNullOrEmpty(Id));
        DrawTableInternal();
    }

    public virtual void LoadRows()
    {
    }

    private void SortInternal()
    {
        if (!Sortable)
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        IsSortDirty |= sortSpecs.SpecsDirty;

        if (!IsSortDirty)
            return;

        if (sortSpecs.SpecsCount == 0)
        {
            if (Flags.HasFlag(ImGuiTableFlags.SortTristate))
                SortTristate();
        }
        else if (Columns.Count > 0)
        {
            var column = Columns[Columns.Count <= sortSpecs.Specs.ColumnIndex ? 0 : sortSpecs.Specs.ColumnIndex];
            _rows.Sort((a, b) => (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending ? -1 : 1) * column.Compare(a, b));
        }

        _filteredRows = null;
        sortSpecs.SpecsDirty = IsSortDirty = false;
    }

    public virtual void SortTristate()
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
        if (Columns.Count == 0)
            return;

        using var table = ImRaii.Table("Table", Columns.Count, Flags, ImGui.GetContentRegionAvail());
        if (!table)
            return;

        ImGui.TableSetupScrollFreeze(ScrollFreezeCols, 1);

        foreach (var column in Columns)
            ImGui.TableSetupColumn(column.Label, column.Flags, column.Width * ImGuiHelpers.GlobalScale);

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        if (_closePopupNextFrame)
        {
            ImGuiNativeAdditions.igClosePopupsExceptModals();
            _closePopupNextFrame = false;
        }

        if (ClosePopup && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
        {
            ClosePopup = false;
            _closePopupNextFrame = true;
        }

        for (var columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
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

            if (column.AutoLabel && string.IsNullOrEmpty(column.Label))
                column.UpdateLabel();

            IsFilterDirty |= column.DrawFilter();
        }

        if (!RowsLoaded)
        {
            LoadRows();
            RowsLoaded = true;
        }

        SortInternal();
        UpdateFilter();

        if (LineHeight == 0)
        {
            for (var i = 0; i < _filteredRows!.Count; i++)
                DrawRow(_filteredRows[i], i);
        }
        else
        {
            ImGuiClip.ClippedDraw(_filteredRows!, DrawRow, LineHeight ?? ImGui.GetTextLineHeightWithSpacing());
        }
    }

    private void DrawRow(T row, int rowIndex)
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

internal static class ImGuiNativeAdditions
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void igClosePopupsExceptModals();
}
