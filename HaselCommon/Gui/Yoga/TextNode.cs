using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using HaselCommon.Graphics;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Enums;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Gui.Yoga;

#pragma warning disable SeStringRenderer

[DebuggerDisplay("Guid: {Guid.ToString()} | Text: {_text.ExtractText()}")]
public partial class TextNode : Node
{
    private ReadOnlySeString _text;

    public override string TagName => "#text";
    public override string DebugNodeOpenTag => $"{_text.ExtractText().Replace("\n", "")}";

    [NodeProp("Node", editable: true)]
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

    [NodeProp("Node", editable: true)]
    public Color? TextColor { get; set; }

    public Action<TextNode, ReadOnlySeString>? ClickCallback { get; set; }

    public TextNode()
    {
        NodeType = NodeType.Text;
        HasMeasureFunc = true;
    }

    public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = width, TargetDrawList = 0 }).Size;
    }

    public override void DrawContent()
    {
        var result = ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = ComputedWidth, Color = TextColor });
        if (result.Clicked)
            ClickCallback?.Invoke(this, (ReadOnlySeString)result.InteractedPayloadEnvelope);
    }
}
