namespace HaselCommon.ImGuiYoga.Elements;

// TODO: not updated yet
/*
public unsafe class ImageNode : YogaNode
{
    private readonly IDalamudTextureWrap textureWrap;

    public ImageNode(string id, IDalamudTextureWrap textureWrap) : base(id)
    {
        this.textureWrap = textureWrap;

        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return textureWrap.Size;
    }

    protected override void DrawNode()
    {
        if (!ImGuiUtils.IsInViewport(ComputedSize) || textureWrap.ImGuiHandle == 0)
        {
            ImGui.Dummy(ComputedSize);
        }
        else
        {
            ImGui.Image(textureWrap.ImGuiHandle, textureWrap.Size.Contain(ComputedSize));
        }
    }
}
*/
