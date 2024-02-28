using Dalamud.Interface.Internal;
using HaselCommon.Extensions;
using HaselCommon.Yoga;
using ImGuiNET;
using YGMeasureFuncDelegate = HaselCommon.Yoga.Interop.YGMeasureFuncDelegate;

namespace HaselCommon.Utils.ImGuiYoga;

public unsafe class ImageNode : Node
{
    private readonly IDalamudTextureWrap textureWrap;

    public ImageNode(YogaWindow window, IDalamudTextureWrap textureWrap) : base(window)
    {
        this.textureWrap = textureWrap;

        SetMeasureFunc(new YGMeasureFuncDelegate(Measure));
    }

    private YGSize Measure(YGNode* node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return textureWrap.Size;
    }

    public override void Draw()
    {
        PreDraw();

        if (textureWrap.ImGuiHandle != 0)
        {
            ImGui.Image(textureWrap.ImGuiHandle, textureWrap.Size.Contain(GetComputedSize()));
        }

        PostDraw();
    }
}
