using System.Collections.Generic;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public class Column<T> : IComparer<T>
{
    protected const float ArrowWidth = 10;

    public string Label { get; set; } = string.Empty;
    public ImGuiTableColumnFlags Flags { get; set; }
    public float Width { get; set; }
    public bool IsSearchable { get; set; }
    public string SearchQuery { get; set; } = string.Empty;

    public virtual void DrawColumnHeader(Table<T> table)
    {
        if (!IsSearchable)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(Label);
            return;
        }

        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);

        var text = SearchQuery;
        ImGui.SetNextItemWidth(-ArrowWidth * ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextWithHint($"##Filter", $"{Label}...", ref text, 255, ImGuiInputTextFlags.AutoSelectAll))
        {
            SearchQuery = text;
            table.IsSearchDirty |= true;
        }
    }

    public virtual void DrawColumn(T row) { }

    public virtual int Compare(T? x, T? y)
    {
        return 0;
    }

    public virtual bool ShouldShow(T row)
    {
        return true;
    }
}
