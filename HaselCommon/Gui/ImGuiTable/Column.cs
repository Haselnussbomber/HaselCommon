using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public class Column<T>
{
    public string Label { get; set; } = string.Empty;
    public string LabelKey { get; set; } = string.Empty;
    public bool AutoLabel { get; set; } = true;
    public float Width { get; set; } = -1;
    public ImGuiTableColumnFlags Flags
    {
        get;
        set
        {
            if (value.HasFlag(ImGuiTableColumnFlags.WidthFixed) && field.HasFlag(ImGuiTableColumnFlags.WidthStretch))
                field &= ~ImGuiTableColumnFlags.WidthStretch;
            else if (value.HasFlag(ImGuiTableColumnFlags.WidthStretch) && field.HasFlag(ImGuiTableColumnFlags.WidthFixed))
                field &= ~ImGuiTableColumnFlags.WidthFixed;

            field |= value;
        }
    } = ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthStretch;

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

    internal void UpdateLabel()
    {
        Label = Service.Get<TextService>().Translate(LabelKey);
    }
}
