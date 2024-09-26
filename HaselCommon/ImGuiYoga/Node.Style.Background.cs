using System.Numerics;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    private void DrawBackground()
    {
        var backgroundColor = ComputedStyle.BackgroundColor;
        if (backgroundColor.A == 0)
            return;

        var borderTopLeftRadius = ComputedStyle.BorderTopLeftRadius;
        var borderTopRightRadius = ComputedStyle.BorderTopRightRadius;
        var borderBottomRightRadius = ComputedStyle.BorderBottomRightRadius;
        var borderBottomLeftRadius = ComputedStyle.BorderBottomLeftRadius;

        // gradients???? maybe with Skia?

        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetWindowPos() + CumulativePosition - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
        var size = ComputedSize;

        // Rectangular background without rounded corners
        if (borderTopLeftRadius == 0 && borderTopRightRadius == 0 && borderBottomRightRadius == 0 && borderBottomLeftRadius == 0)
        {
            drawList.AddRectFilled(pos, pos + size, backgroundColor);
            return;
        }

        drawList.PathClear();

        // Step 1: Draw each rounded corner
        Vector2 center;

        // Top-left corner
        if (borderTopLeftRadius > 0)
        {
            center = pos + new Vector2(borderTopLeftRadius);
            drawList.PathArcTo(center, borderTopLeftRadius, MathF.PI, MathF.PI * 1.5f);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(backgroundColor);
        }

        // Top-right corner
        if (borderTopRightRadius > 0)
        {
            center = pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius);
            drawList.PathArcTo(center, borderTopRightRadius, -MathF.PI * 0.5f, 0);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(backgroundColor);
        }

        // Bottom-right corner
        if (borderBottomRightRadius > 0)
        {
            center = pos + size - new Vector2(borderBottomRightRadius);
            drawList.PathArcTo(center, borderBottomRightRadius, 0, MathF.PI * 0.5f);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(backgroundColor);
        }

        // Bottom-left corner
        if (borderBottomLeftRadius > 0)
        {
            center = pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius);
            drawList.PathArcTo(center, borderBottomLeftRadius, MathF.PI * 0.5f, MathF.PI);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(backgroundColor);
        }

        // Step 2: Draw Triangles to fill the middle portion

        var savedFlags = drawList.Flags;
        drawList.Flags = ImDrawListFlags.None;

        // TODO: clamp values (example case: border-radius: 100px; on a node that's 20x20)

        // Top edge
        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius));
        drawList.PathFillConvex(backgroundColor);

        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius));
        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius));
        drawList.PathFillConvex(backgroundColor);

        // Left edge
        drawList.PathLineTo(pos + new Vector2(0, borderTopLeftRadius));
        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius));
        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius));
        drawList.PathFillConvex(backgroundColor);

        drawList.PathLineTo(pos + new Vector2(0, borderTopLeftRadius));
        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius));
        drawList.PathLineTo(pos + new Vector2(0, size.Y - borderBottomLeftRadius));
        drawList.PathFillConvex(backgroundColor);

        // Bottom edge
        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderBottomRightRadius, size.Y - borderBottomRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderBottomRightRadius, size.Y));
        drawList.PathFillConvex(backgroundColor);

        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius));
        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y));
        drawList.PathLineTo(pos + new Vector2(size.X - borderBottomRightRadius, size.Y));
        drawList.PathFillConvex(backgroundColor);

        // Right edge

        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X, borderTopRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X, size.Y - borderBottomRightRadius));
        drawList.PathFillConvex(backgroundColor);

        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, size.Y - borderBottomRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X, size.Y - borderBottomRightRadius));
        drawList.PathFillConvex(backgroundColor);

        // Center
        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, borderTopRightRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderTopRightRadius, size.Y - borderBottomRightRadius));
        drawList.PathFillConvex(backgroundColor);

        drawList.PathLineTo(pos + new Vector2(borderTopLeftRadius));
        drawList.PathLineTo(pos + new Vector2(borderBottomLeftRadius, size.Y - borderBottomLeftRadius));
        drawList.PathLineTo(pos + new Vector2(size.X - borderBottomRightRadius, size.Y - borderBottomRightRadius));
        drawList.PathFillConvex(backgroundColor);

        drawList.Flags = savedFlags;
    }
}
