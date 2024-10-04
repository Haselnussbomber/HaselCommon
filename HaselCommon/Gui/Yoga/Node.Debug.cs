using System.Diagnostics;
using System.Numerics;
using HaselCommon.Graphics;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

[DebuggerDisplay("Guid: {Guid.ToString()} | Children: {Count}")]
public partial class Node : IDisposable
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
            var pos = ImGui.GetWindowPos() + AbsolutePosition;

            ImGuiUtils.DrawDashedBorder(pos, pos + new Vector2(ComputedWidth, ComputedHeight), Color.Red);

            _isDebugHovered = false;
        }
    }
}
