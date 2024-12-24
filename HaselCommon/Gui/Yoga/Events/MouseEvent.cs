using System.Numerics;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga.Events;

public class MouseEvent : YogaEvent
{
    public required MouseEventType EventType { get; init; }
    public ImGuiMouseButton? Button { get; init; }
    public bool CtrlKey { get; init; } = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
    public bool ShiftKey { get; init; } = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
    public Vector2 Position { get; init; } = ImGui.GetMousePos();
    public object? Payload { get; init; }
}

public enum MouseEventType
{
    MouseOver,
    MouseOut,
    MouseClick,
    MouseMove,
    MouseHover,
    MouseDown,
    MouseUp,
}
