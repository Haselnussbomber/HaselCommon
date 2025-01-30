using System.Numerics;
using ImGuiNET;

namespace HaselCommon.Yoga.Events;

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
    /// <summary>
    /// Is fired when the cursor moves into the node.
    /// </summary>
    MouseOver,

    /// <summary>
    /// Is fired when the cursor moves off of the node.
    /// </summary>
    MouseOut,

    /// <summary>
    /// Is fired when the mouse is clicked on the node.
    /// </summary>
    MouseClick,

    /// <summary>
    /// Is only fired when the cursor is moved over the node.
    /// </summary>
    MouseMove,

    /// <summary>
    /// Is fired every frame the cursor is hovering over the node.
    /// </summary>
    MouseHover,

    /// <summary>
    /// Is fired when a mouse button is pressed down.
    /// </summary>
    MouseDown,

    /// <summary>
    /// Is fired when a mouse button is released.
    /// </summary>
    MouseUp,
}
