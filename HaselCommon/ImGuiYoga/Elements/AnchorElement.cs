using HaselCommon.ImGuiYoga.Attributes;
using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;
using YGMeasureFuncDelegate = YogaSharp.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.ImGuiYoga.Elements;

public unsafe class AnchorElement : Node
{
    public AnchorElement() : base()
    {
        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    // TODO: use {children}??
    [NodeProperty("label")]
    public string Label { get; set; } = string.Empty;

    [NodeProperty("title")]
    public string Title { get; set; } = string.Empty;

    [NodeProperty("href")]
    public string Href { get; set; } = string.Empty;

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return ImGui.CalcTextSize(Label);
    }

    protected override void DrawNode()
    {
        if (!ImGuiUtils.IsInViewport(ComputedSize))
        {
            ImGui.Dummy(ComputedSize);
        }
        else
        {
            ImGuiUtils.DrawLink(Label, Title, Href);
        }
    }
}
