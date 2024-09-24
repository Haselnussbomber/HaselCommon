using System.Numerics;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga;

public partial class Node
{
    private void DrawBackground()
    {
        if (Style.BackgroundColor.A == 0)
            return;

        // gradients???? maybe with Skia?

        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetWindowPos() + CumulativePosition - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
        var size = ComputedSize;

        // Rectangular background without rounded corners
        if (Style.BorderRadiusTopLeft == 0 && Style.BorderRadiusTopRight == 0 && Style.BorderRadiusBottomRight == 0 && Style.BorderRadiusBottomLeft == 0)
        {
            drawList.AddRectFilled(pos, pos + size, Style.BackgroundColor);
            return;
        }

        drawList.PathClear();

        // Step 1: Draw each rounded corner
        Vector2 center;

        // Top-left corner
        if (Style.BorderRadiusTopLeft > 0)
        {
            center = pos + new Vector2(Style.BorderRadiusTopLeft);
            drawList.PathArcTo(center, Style.BorderRadiusTopLeft, MathF.PI, MathF.PI * 1.5f);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(Style.BackgroundColor);
        }

        // Top-right corner
        if (Style.BorderRadiusTopRight > 0)
        {
            center = pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight);
            drawList.PathArcTo(center, Style.BorderRadiusTopRight, -MathF.PI * 0.5f, 0);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(Style.BackgroundColor);
        }

        // Bottom-right corner
        if (Style.BorderRadiusBottomRight > 0)
        {
            center = pos + size - new Vector2(Style.BorderRadiusBottomRight);
            drawList.PathArcTo(center, Style.BorderRadiusBottomRight, 0, MathF.PI * 0.5f);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(Style.BackgroundColor);
        }

        // Bottom-left corner
        if (Style.BorderRadiusBottomLeft > 0)
        {
            center = pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft);
            drawList.PathArcTo(center, Style.BorderRadiusBottomLeft, MathF.PI * 0.5f, MathF.PI);
            drawList.PathLineTo(center);
            drawList.PathFillConvex(Style.BackgroundColor);
        }

        // Step 2: Draw Triangles to fill the middle portion

        var savedFlags = drawList.Flags;
        drawList.Flags = ImDrawListFlags.None;

        // TODO: clamp values (example case: border-radius: 100px; on a node that's 20x20)

        // Top edge
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft, 0));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft));
        drawList.PathFillConvex(Style.BackgroundColor);

        // Left edge
        drawList.PathLineTo(pos + new Vector2(0, Style.BorderRadiusTopLeft));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.PathLineTo(pos + new Vector2(0, Style.BorderRadiusTopLeft));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathLineTo(pos + new Vector2(0, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathFillConvex(Style.BackgroundColor);

        // Bottom edge
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusBottomRight, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusBottomRight, size.Y));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusBottomRight, size.Y));
        drawList.PathFillConvex(Style.BackgroundColor);

        // Right edge

        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight));
        drawList.PathLineTo(pos + new Vector2(size.X, Style.BorderRadiusTopRight));
        drawList.PathLineTo(pos + new Vector2(size.X, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathLineTo(pos + new Vector2(size.X, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathFillConvex(Style.BackgroundColor);

        // Center
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, Style.BorderRadiusTopRight));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusTopRight, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusTopLeft));
        drawList.PathLineTo(pos + new Vector2(Style.BorderRadiusBottomLeft, size.Y - Style.BorderRadiusBottomLeft));
        drawList.PathLineTo(pos + new Vector2(size.X - Style.BorderRadiusBottomRight, size.Y - Style.BorderRadiusBottomRight));
        drawList.PathFillConvex(Style.BackgroundColor);

        drawList.Flags = savedFlags;
    }
}
