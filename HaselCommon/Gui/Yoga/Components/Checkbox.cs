using System.Numerics;
using HaselCommon.Gui.Yoga.Components.Events;
using HaselCommon.Gui.Yoga.Events;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga.Components;

public class Checkbox : Node
{
    private bool _isChecked;
    private bool _isHovered;
    private Vector2 _mousePos;

    public bool IsChecked
    {
        get => _isChecked;
        set => _isChecked = value;
    }

    public Checkbox() : base()
    {
        EnableMeasureFunc = true;
    }

    public override Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        return new Vector2(ImGui.GetFrameHeight());
    }

    public override void ApplyGlobalScale(float globalFontScale)
    {
        IsDirty = true;
    }

    public override void DrawContent()
    {
        var clicked = ImGui.Checkbox($"###{Guid}Checkbox", ref _isChecked);

        var hovered = ImGui.IsItemHovered();
        if (hovered != _isHovered)
        {
            _isHovered = hovered;

            if (hovered)
            {
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseOver,
                });
            }
            else
            {
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseOut
                });
            }
        }

        if (hovered)
        {
            var mousePos = ImGui.GetMousePos();
            if (mousePos != _mousePos)
            {
                _mousePos = mousePos;
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseMove
                });
            }

            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseHover
            });
        }

        if (clicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Left,
                Payload = _isChecked
            });

            DispatchEvent(new CheckboxStateChangeEvent()
            {
                IsChecked = _isChecked
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Middle
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Right
            });
        }
    }
}
