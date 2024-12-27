using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using HaselCommon.Extensions.Strings;
using HaselCommon.Graphics;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Events;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

[DebuggerDisplay("Guid: {Guid.ToString()} | Text: {_text.ExtractText()}")]
public partial class TextNode : Node
{
    protected ReadOnlySeString _text;
    private bool _isTextOnly;
    protected bool _hovered;

    public override string TypeName => "#text";
    public override string DebugNodeOpenTag => $"{_text.ExtractText().Replace("\n", "")}";

    [NodeProp("Text", editable: true)]
    public ReadOnlySeString Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                _isTextOnly = _text.IsTextOnly();
                IsDirty = true;
            }
        }
    }

    [NodeProp("Text", editable: true)]
    public Color? TextColor { get; set; }
    public IFontHandle? Font { get; set; }

    public TextNode() : base()
    {
        NodeType = YGNodeType.Text;
        EnableMeasureFunc = true;
    }

    public override void ApplyGlobalScale(float globalFontScale)
    {
        IsDirty = true;
    }

    public override unsafe Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        if (_text.IsEmpty)
            return Vector2.Zero;

        using var font = Font?.Push();

        if (_isTextOnly)
            return ImGuiUtils.CalcTextSize(_text, wrapWidth: width);

        return ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = width, TargetDrawList = 0 }).Size;
    }

    public override unsafe void DrawContent()
    {
        if (_text.IsEmpty)
            return;

        var isLeftClicked = false;
        var isMiddleClicked = false;
        var isRightClicked = false;
        ReadOnlySeString? clickedPayload = null;

        using (Font?.Push())
        {
            if (_isTextOnly)
            {
                var pos = ImGui.GetCursorPos();

                using (TextColor?.Push(ImGuiCol.Text))
                    ImGuiUtils.TextUnformatted(_text);

                ImGui.SetCursorPos(pos);
                ImGui.InvisibleButton("##InvisibleButton", ComputedSize);
                isLeftClicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
                isMiddleClicked = ImGui.IsItemClicked(ImGuiMouseButton.Middle);
                isRightClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);

                if (isLeftClicked || isMiddleClicked || isRightClicked)
                    clickedPayload = Text;
            }
            else
            {
                var result = ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = ComputedWidth, Color = TextColor });
                isLeftClicked = result.Clicked;
                clickedPayload = (ReadOnlySeString)result.InteractedPayloadEnvelope;
            }
        }

        var hovered = ImGui.IsItemHovered();

        if (hovered != IsHovered)
        {
            IsHovered = hovered;

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

        if (isLeftClicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Left,
                Payload = clickedPayload
            });
        }
        else if (isMiddleClicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Middle,
                Payload = clickedPayload
            });
        }
        else if (isRightClicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Right,
                Payload = clickedPayload
            });
        }
    }
}
