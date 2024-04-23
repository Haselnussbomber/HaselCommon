using HaselCommon.Records;
using ImGuiNET;

namespace HaselCommon.Utils;

public struct ImGuiContextMenuEntry : IImGuiContextMenuEntry
{
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool LoseFocusOnClick { get; set; } = false;
    public string Label { get; set; } = string.Empty;
    public Action? ClickCallback { get; set; } = null;
    public Action? HoverCallback { get; set; } = null;

    public ImGuiContextMenuEntry() { }

    public void Draw(IterationArgs args)
    {
        if (ImGui.MenuItem(Label, Enabled))
        {
            ClickCallback?.Invoke();

            if (LoseFocusOnClick)
            {
                ImGui.SetWindowFocus(null);
            }
        }
        if (ImGui.IsItemHovered())
        {
            HoverCallback?.Invoke();
        }
    }
}
