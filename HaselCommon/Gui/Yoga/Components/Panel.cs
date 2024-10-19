using System.Numerics;
using HaselCommon.Graphics;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga.Components;

public class Panel : Node
{
    public Panel() : base()
    {
        Border = 1;
    }

    public override void DrawContent()
    {
        var pos = ImGui.GetWindowPos() + AbsolutePosition - Vector2.One;
        var size = pos + ComputedSize + Vector2.One * 2f;
        ImGui.GetForegroundDrawList().AddRect(pos, size, Color.From(ImGuiCol.Border));
    }
}