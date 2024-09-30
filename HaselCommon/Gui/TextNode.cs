using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using HaselCommon.Gui.Enums;
using Lumina.Text.ReadOnly;
using PropertyChanged.SourceGenerator;

namespace HaselCommon.Gui;

#pragma warning disable SeStringRenderer

[DebuggerDisplay("Guid: {Guid.ToString()} | Text: {_text.ExtractText()}")]
public partial class TextNode : Node
{
    [Notify] private ReadOnlySeString _text;

    public override string TagName => "#text";
    public override string DebugNodeOpenTag => $"{_text.ExtractText().Replace("\n", "")}";

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
        ImGuiHelpers.SeStringWrapped(_text, new SeStringDrawParams() { WrapWidth = Layout.Width });
    }
}
