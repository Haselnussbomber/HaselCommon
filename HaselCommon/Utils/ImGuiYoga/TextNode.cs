using System.Numerics;
using HaselCommon.Yoga;
using ImGuiNET;
using YGMeasureFuncDelegate = HaselCommon.Yoga.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.Utils.ImGuiYoga;

//TODO: custom font and font size support
public unsafe class TextNode : Node
{
    private string text = string.Empty;

    public TextNode(YogaWindow window) : base(window)
    {
        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    public TextNode(YogaWindow window, string text) : this(window)
    {
        Text = text;
    }

    public string Text
    {
        get => text;
        set
        {
            if (!string.Equals(text, value))
            {
                text = value;
                MarkDirty();
            }
        }
    }

    public uint? TextColor { get; set; } = null;

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return ImGui.CalcTextSize(Text, width);
    }

    public override void Draw()
    {
        PreDraw();

        var drawList = ImGui.GetWindowDrawList();
        var font = ImGui.GetFont();
        var fontSize = ImGui.GetFontSize();
        var pos = Window.ViewportPosition + GetAbsolutePosition() - new Vector2(GetParent()?.ScrollLeft ?? 0, GetParent()?.ScrollTop ?? 0);
        var textColor = TextColor ?? ImGui.GetColorU32(ImGuiCol.Text);
        var wrapWidth = GetComputedWidth();

        ImGuiNativeAdditions.AddText(drawList, font, fontSize, pos, textColor, Text, wrapWidth);

        PostDraw();
    }
}
