using HaselCommon.Yoga;
using ImGuiNET;
using YGMeasureFuncDelegate = HaselCommon.Yoga.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.Utils.ImGuiYoga;

public unsafe class LinkNode : Node
{
    public string Label { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public LinkNode(YogaWindow window) : base(window)
    {
        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    public LinkNode(YogaWindow window, string label, string title, string url) : this(window)
    {
        Label = label;
        Title = title;
        Url = url;
    }

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return ImGui.CalcTextSize(Label);
    }

    public override void Draw()
    {
        PreDraw();

        ImGuiUtils.DrawLink(Label, Title, Url);

        PostDraw();
    }
}
