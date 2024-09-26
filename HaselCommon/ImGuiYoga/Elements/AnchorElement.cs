using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;
using YGMeasureFuncDelegate = YogaSharp.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.ImGuiYoga.Elements;

public unsafe class AnchorElement : Node
{
    public override string TagName => "a";

    public AnchorElement() : base()
    {
        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    public string Label
    {
        get => Attributes["label"] ?? string.Empty;
        set => Attributes["label"] = value;
    }

    public string Title
    {
        get => Attributes["title"] ?? string.Empty;
        set => Attributes["title"] = value;
    }

    public string Href
    {
        get => Attributes["href"] ?? string.Empty;
        set => Attributes["href"] = value;
    }

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
