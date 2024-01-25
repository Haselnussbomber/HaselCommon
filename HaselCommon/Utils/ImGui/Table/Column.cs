using ImGuiNET;

namespace HaselCommon.Utils;

public class Column<TItem>
{
    public string                LabelKey = string.Empty;
    public ImGuiTableColumnFlags Flags = ImGuiTableColumnFlags.NoResize;

    public virtual float Width
        => -1f;

    public string FilterLabel
        => $"##{LabelKey}Filter";

    public virtual bool DrawFilter()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(t(LabelKey));
        return false;
    }

    public virtual bool FilterFunc(TItem item)
        => true;

    public virtual int Compare(TItem lhs, TItem rhs)
        => 0;

    public virtual void DrawColumn(TItem item, int idx)
    {
    }

    public int CompareInv(TItem lhs, TItem rhs)
        => Compare(rhs, lhs);
}
