using System.Diagnostics;
using System.Numerics;
using HaselCommon.Extensions.Graphics;
using HaselCommon.Graphics;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

[DebuggerDisplay("Guid: {Guid.ToString()} | Children: {Count}")]
public partial class Node
{
    internal bool _isDebugHovered;

    public virtual string DebugNodeOpenTag => $"<{TagName}{(Count == 0 ? " /" : string.Empty)}>";
    public virtual bool DebugHasClosingTag => Count != 0;

    private void DrawDebugBefore()
    {
        // not sure yet, maybe background of selection
    }

    private void DrawDebugAfter()
    {
        if (_isDebugHovered)
        {
            var pos = ImGui.GetWindowPos() + AbsolutePosition - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

            ImGui.GetForegroundDrawList().AddText(pos - new Vector2(0, ImGui.GetTextLineHeightWithSpacing()), Color.Gold, $"{ComputedWidth}x{ComputedHeight}");
            ImGui.GetForegroundDrawList().AddDashedBorder(pos, pos + ComputedSize - Vector2.One, Color.Red);

            _isDebugHovered = false;
        }
    }
}
