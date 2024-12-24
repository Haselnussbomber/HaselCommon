using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using HaselCommon.Gui.Yoga.Events;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga.Components;

public class IconNode : Panel
{
    private readonly ITextureProvider _textureProvider;
    private bool _isHovered;
    private Vector2 _mousePos;

    public required GameIconLookup Icon { get; set; }
    public float Scale { get; set; } = 1.0f;
    public Vector4 TintColor { get; set; } = Vector4.One;
    public Vector2 Uv0 { get; set; } = Vector2.Zero;
    public Vector2 Uv1 { get; set; } = Vector2.One;
    public Vector2? TransformUv { get; set; }

    public IconNode() : base()
    {
        _textureProvider = Service.Get<ITextureProvider>();
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
            return textureWrap.Size * Scale * ImGuiHelpers.GlobalScale;
        }

        return Vector2.Zero;
    }

    public override void DrawContent()
    {
        if (!ImGuiUtils.IsInViewport(ComputedSize))
        {
            ImGui.Dummy(ComputedSize);
            return;
        }

        if (!_textureProvider.TryGetFromGameIcon(Icon, out var texture))
        {
            ImGui.Dummy(ComputedSize);
            return;
        }

        if (!texture.TryGetWrap(out var textureWrap, out _))
        {
            ImGui.Dummy(ComputedSize);
            return;
        }

        var size = ComputedSize;
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
            BorderColor ?? Vector4.Zero);

        HandleInputs();
    }

    private void HandleInputs()
    {
        var clicked = ImGui.IsItemClicked();
        var hovered = ImGui.IsItemHovered();
        if (hovered != _isHovered)
        {
            _isHovered = hovered;

            if (hovered)
            {
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseOver,
                });
            }
            else
            {
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseOut
                });
            }
        }

        if (hovered)
        {
            var mousePos = ImGui.GetMousePos();
            if (mousePos != _mousePos)
            {
                _mousePos = mousePos;
                DispatchEvent(new MouseEvent()
                {
                    EventType = MouseEventType.MouseMove
                });
            }

            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseHover
            });
        }

        if (clicked)
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Left
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Middle
            });
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            DispatchEvent(new MouseEvent()
            {
                EventType = MouseEventType.MouseClick,
                Button = ImGuiMouseButton.Right
            });
        }
    }
}
