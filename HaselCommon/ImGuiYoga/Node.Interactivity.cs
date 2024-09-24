using System.Numerics;
using HaselCommon.ImGuiYoga.Events;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga;

public unsafe partial class Node
{
    public bool Interactive
    {
        get => Attributes["interactive"] == "true";
        set => Attributes["interactive"] = value == true ? "true" : "false";
    }

    public bool IsHovered { get; private set; } = false;
    public bool IsDebugHovered { get; set; }

    private Vector2 InteractiveLastMousePos = Vector2.Zero;

    private void HandleMouse()
    {
        if (!Interactive)
            return;

        var pos = ImGui.GetCursorPos();
        ImGui.InvisibleButton($"##__InteractableButton", ComputedSize);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(Style.Cursor.ResolveImGuiMouseCursor());

            if (IsHovered != true)
            {
                IsHovered = true;
                GetDocument()?.SetStyleDirty();
                DispatchEvent(new MouseEvent()
                {
                    Sender = this,
                    EventType = MouseEventType.MouseOver,
                });
            }

            var mousePos = ImGui.GetMousePos();
            if (InteractiveLastMousePos != mousePos)
            {
                InteractiveLastMousePos = mousePos;
                DispatchEvent(new MouseEvent()
                {
                    Sender = this,
                    EventType = MouseEventType.MouseMove
                });
            }
        }
        else if (IsHovered == true)
        {
            IsHovered = false;
            GetDocument()?.SetStyleDirty();
            DispatchEvent(new MouseEvent()
            {
                Sender = this,
                EventType = MouseEventType.MouseOut
            });
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            DispatchEvent(new MouseEvent()
            {
                Sender = this,
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Left,
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
        {
            DispatchEvent(new MouseEvent()
            {
                Sender = this,
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Middle,
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            DispatchEvent(new MouseEvent()
            {
                Sender = this,
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Right,
            });
        }

        ImGui.SetCursorPos(pos);
    }
}
