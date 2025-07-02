namespace HaselCommon.Gui.ImGuiTable;

public class Column<T>
{
    public string Label { get; set; } = string.Empty;
    public string LabelKey { get; set; } = string.Empty;
    public bool AutoLabel { get; set; } = true;
    public float Width { get; set; } = -1;
    public ImGuiTableColumnFlags Flags { get; set; } = ImGuiTableColumnFlags.NoResize | ImGuiTableColumnFlags.WidthStretch;

    public Column()
    {
        var type = GetType();
        LabelKey = $"{type.Namespace}.{type.Name}.Label";
    }

    public void SetStretchWidth(float width = 1)
    {
        if (Flags.HasFlag(ImGuiTableColumnFlags.WidthFixed))
            Flags &= ~ImGuiTableColumnFlags.WidthFixed;

        Flags |= ImGuiTableColumnFlags.WidthStretch;
        Width = width;
    }

    public void SetFixedWidth(float width)
    {
        if (Flags.HasFlag(ImGuiTableColumnFlags.WidthStretch))
            Flags &= ~ImGuiTableColumnFlags.WidthStretch;

        Flags |= ImGuiTableColumnFlags.WidthFixed;
        Width = width;
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
