using System.Numerics;
using HaselCommon.Gui.Yoga.Components.Events;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga.Components;

public class Checkbox : Node
{
    private bool _isChecked;

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
        if (ImGui.Checkbox($"###{Guid}Checkbox", ref _isChecked))
        {
            DispatchEvent(new CheckboxStateChangeEvent()
            {
                IsChecked = _isChecked
            });
        }
    }
}