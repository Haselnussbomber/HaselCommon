using System.Diagnostics;
using System.Numerics;
using HaselCommon.Extensions.Graphics;
using HaselCommon.Graphics;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

[DebuggerDisplay("Guid: {Guid.ToString()} | Children: {Count}")]
public partial class Node
{
    internal bool _showDebugHighlight;

    public virtual string DebugNodeOpenTag => $"<{TypeName}{(Count == 0 ? " /" : string.Empty)}>";
    public virtual bool DebugHasClosingTag => Count != 0;

    [Conditional("DEBUG")]
    private void DrawDebugHighlight()
    {
        if (_showDebugHighlight)
        {
            var pos = ImGui.GetWindowPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + AbsolutePosition;

            var text = $"{ComputedWidth}x{ComputedHeight}";
            var textSize = ImGui.CalcTextSize(text);
            var textPadding = new Vector2(6, 4);
            var textPos = pos + new Vector2(ComputedSize.X / 2f - textSize.X / 2f + textPadding.X, -ImGui.GetTextLineHeightWithSpacing()) - textPadding;
            ImGui.GetForegroundDrawList().AddRectFilled(textPos - textPadding, textPos + textSize + textPadding, hsla(0, 0, 0, 0.5f), 5);
            ImGui.GetForegroundDrawList().AddText(textPos - new Vector2(0, 1), Color.White, text);
            ImGui.GetForegroundDrawList().AddRectFilled(pos, pos + ComputedSize - Vector2.One, hsla(200, 1, 0.7f, 0.3f));
            ImGui.GetForegroundDrawList().AddDashedBorder(pos, pos + ComputedSize - Vector2.One, hsla(200, 1, 0.4f, 1));

            _showDebugHighlight = false;
        }
    }
}
