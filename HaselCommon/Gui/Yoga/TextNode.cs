using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using HaselCommon.Graphics;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Events;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

#pragma warning disable SeStringRenderer

[DebuggerDisplay("Guid: {Guid.ToString()} | Text: {_text.ExtractText()}")]
public partial class TextNode : Node
{
    protected ReadOnlySeString _text;
    protected bool _hovered;
    protected Vector2 _mousePos;

    public override string TagName => "#text";
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
                IsDirty = true;
            }
        }
    }

    [NodeProp("Text", editable: true)]
    public Color? TextColor { get; set; }

    public TextNode() : base()
    {
        NodeType = YGNodeType.Text;
        EnableMeasureFunc = true;
    }

    public override Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        return ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = width, TargetDrawList = 0 }).Size;
    }

    public override void DrawContent()
    {
        var result = ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = ComputedWidth, Color = TextColor });

        var hovered = ImGui.IsItemHovered();
        if (hovered != _hovered)
        {
            _hovered = hovered;

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

        if (result.Clicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Left,
                Payload = (ReadOnlySeString)result.InteractedPayloadEnvelope
            });
        }
    }
}
