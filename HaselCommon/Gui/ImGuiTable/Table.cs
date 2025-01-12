using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public class Table<T>(string id)
{
    public string Id { get; set; } = id;
    public List<T> Rows { get; set; } = [];
    public Column<T>[] Columns { get; set; } = [];
    public ImGuiTableFlags Flags { get; set; } = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
    public bool IsSearchDirty { get; set; }
    public float? LineHeight { get; set; } = null;

    protected IReadOnlyList<T>? FilteredRows { get; set; }

    protected virtual void PreDraw() { }
    protected virtual void PostDraw() { }
    protected virtual void PreDrawRows() { }
    protected virtual void PostDrawRows() { }

    public void Draw()
    {
        PreDraw();

        using var table = ImRaii.Table(Id, Columns.Length, Flags);
        if (!table) return;

        foreach (var column in Columns)
            ImGui.TableSetupColumn(column.Label, column.Flags, column.Width * ImGuiHelpers.GlobalScale);

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        var columnIndex = 0;
        foreach (var column in Columns)
        {
            if (!ImGui.TableSetColumnIndex(columnIndex++))
                continue;

            using var id = ImRaii.PushId($"ColumnHeader{columnIndex}");
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            ImGui.TableHeader(string.Empty);
            ImGui.SameLine();
            style.Pop();

            column.DrawColumnHeader(this);
        }

        if (Rows.Count == 0)
            return;

        PreDrawRows();

        if (IsSearchDirty || FilteredRows == null)
        {
            FilteredRows = Columns.Any(column => column.IsSearchable)
                ? Rows.Where(row => Columns.Where(column => column.IsSearchable).All(column => column.ShouldShow(row))).ToList()
                : Rows;

            IsSearchDirty = false;
        }

        ImGuiClip.ClippedDraw(FilteredRows, DrawRow, LineHeight ?? ImGui.GetTextLineHeightWithSpacing());
        PostDrawRows();

        table.Dispose();
        PostDraw();
    }

    protected virtual void DrawRow(T row, int rowIndex)
    {
        using var rowId = ImRaii.PushId($"Row{rowIndex}");
        ImGui.TableNextRow();

        for (var i = 0; i < Columns.Length; i++)
        {
            ImGui.TableNextColumn();
            using (ImRaii.PushId($"Column{i}"))
                Columns[i].DrawColumn(row);
        }
    }
}
