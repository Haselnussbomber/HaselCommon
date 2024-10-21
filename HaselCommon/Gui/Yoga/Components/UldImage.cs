using System.Numerics;
using Dalamud.Interface.Utility;
using HaselCommon.Services;
using YogaSharp;

namespace HaselCommon.Gui.Yoga.Components;

public class UldImage : Panel
{
    private readonly TextureService _textureService;

    public required string UldName { get; set; }
    public required uint PartListId { get; set; }
    public required uint PartIndex { get; set; }
    public float Scale { get; set; } = 1.0f;

    public UldImage() : base()
    {
        _textureService = Service.Get<TextureService>();
        EnableMeasureFunc = true;
    }

    public override Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        if (_textureService.TryGetUldPartSize(UldName, PartListId, PartIndex, out var size))
        {
            return size * Scale * ImGuiHelpers.GlobalScale;
        }

        if (!float.IsNaN(width) && !float.IsNaN(height))
        {
            return new Vector2(width, height) * Scale * ImGuiHelpers.GlobalScale;
        }

        return Vector2.Zero;
    }

    public override void DrawContent()
    {
        _textureService.DrawPart(UldName, PartListId, PartIndex, ComputedSize);
    }
}
