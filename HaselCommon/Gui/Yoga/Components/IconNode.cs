using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using HaselCommon.Graphics;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga.Components;

public class IconNode : Node
{
    private readonly ITextureProvider _textureProvider;

    public required GameIconLookup Icon { get; set; }
    public float Scale { get; set; } = 1.0f;
    public Color BorderColor { get; set; } = Color.Transparent;
    public Vector4 TintColor { get; set; } = Vector4.One;
    public Vector2 Uv0 { get; set; } = Vector2.Zero;
    public Vector2 Uv1 { get; set; } = Vector2.One;
    public Vector2? TransformUv { get; set; }

    public IconNode() : base()
    {
        _textureProvider = Service.Get<ITextureProvider>();
        Overflow = YGOverflow.Hidden;
        EnableMeasureFunc = true;
    }

    public override Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        if (!float.IsNaN(width) && !float.IsNaN(height))
        {
            return new Vector2(width, height) * Scale * ImGuiHelpers.GlobalScale;
        }

        if (_textureProvider.TryGetFromGameIcon(Icon, out var texture) && texture.TryGetWrap(out var textureWrap, out _))
        {
            return new Vector2(
                !float.IsNaN(width) ? width : textureWrap.Width,
                !float.IsNaN(height) ? height : textureWrap.Height) * Scale * ImGuiHelpers.GlobalScale;
        }

        return Vector2.Zero;
    }

    public override void DrawContent()
    {
        var size = ComputedSize;

        if (Icon.IconId == 0)
            return;

        if (!_textureProvider.TryGetFromGameIcon(Icon, out var texture))
            return;

        if (!texture.TryGetWrap(out var textureWrap, out _))
            return;

        var uv0 = Uv0;
        var uv1 = Uv1;

        if (TransformUv != null)
        {
            uv0 /= textureWrap.Size;
            uv1 /= textureWrap.Size;
        }

        ImGui.Image(
            textureWrap.ImGuiHandle,
            size,
            uv0,
            uv1,
            TintColor,
            BorderColor);

        HandleInputs();
    }
}
