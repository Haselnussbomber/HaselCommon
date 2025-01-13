using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public class Column<T>
{
    private ImGuiTableColumnFlags _flags = ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthStretch;

    public string Label { get; set; } = string.Empty;
    public float Width { get; set; } = -1;
    public ImGuiTableColumnFlags Flags
    {
        get => _flags;
        set
        {
            if (value.HasFlag(ImGuiTableColumnFlags.WidthFixed) && _flags.HasFlag(ImGuiTableColumnFlags.WidthStretch))
                _flags &= ~ImGuiTableColumnFlags.WidthStretch;
            else if (value.HasFlag(ImGuiTableColumnFlags.WidthStretch) && _flags.HasFlag(ImGuiTableColumnFlags.WidthFixed))
                _flags &= ~ImGuiTableColumnFlags.WidthFixed;

            _flags |= value;
        }
    }

    public virtual bool DrawFilter()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(Label);
        return false;
    }

    public virtual bool ShouldShow(T row)
        => true;

    public virtual int Compare(T lhs, T rhs)
        => 0;

    public virtual void DrawColumn(T row)
    {
    }

    public virtual void OnLanguageChanged(string langCode)
    {
    }
}
