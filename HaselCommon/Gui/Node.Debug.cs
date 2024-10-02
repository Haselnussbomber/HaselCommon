using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace HaselCommon.Gui;

[DebuggerDisplay("Guid: {Guid.ToString()} | Children: {Count}")]
public partial class Node : IDisposable
{
    internal bool _isDebugHovered;

    public bool EnableDebug { get; set; } = false;
    public virtual string DebugNodeOpenTag => $"<{TagName}{(Count == 0 ? " /" : string.Empty)}>";
    public virtual bool DebugHasClosingTag => Count != 0;

    private void DrawDebugBorder()
    {
        if (_isDebugHovered)
        {
            var pos = ImGui.GetWindowPos() + AbsolutePosition;
            var size = new Vector2(ComputedWidth, ComputedHeight);

            ImGui.GetForegroundDrawList().AddRect(pos, pos + size, 0xFF0000FF);

            _isDebugHovered = false;
        }
    }
}
