using HaselCommon.Records;

namespace HaselCommon.Utils;

public interface IImGuiContextMenuEntry
{
    public bool Visible { get; set; }
    public bool Enabled { get; set; }
    public string Label { get; set; }
    public bool LoseFocusOnClick { get; set; }
    public Action? ClickCallback { get; set; }
    public Action? HoverCallback { get; set; }
    public void Draw(IterationArgs args);
}
