using HaselCommon.ImGuiYoga.Events;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    public override void DispatchEvent(Event evt)
    {
        base.DispatchEvent(evt);

        if (evt.Bubbles) // can be stopped by the nodes implementation of OnEvent through evt.StopPropagation()
        {
            Parent?.DispatchEvent(evt); // calls this function of the parent
        }
    }

    private void HandleMouse()
    {
        if (!EnableMouse)
            return;

        var pos = ImGui.GetCursorPos();
        ImGui.InvisibleButton($"##__InteractableButton", ComputedSize);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(Style.Cursor);

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
            if (InteractableLastMousePos != mousePos)
            {
                InteractableLastMousePos = mousePos;
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
