using System.Numerics;
using ImGuiNET;

namespace HaselCommon.Extensions.Graphics;

public static class ImDrawListPtrExtensions
{
    public static void AddDashedBorder(this ImDrawListPtr drawList, Vector2 topLeft, Vector2 bottomRight, uint color, float dashLength = 5.0f, float gapLength = 3.0f, float thickness = 1.0f)
    {
        var bottomLeft = new Vector2(topLeft.X, bottomRight.Y);
        var topRight = new Vector2(bottomRight.X, topLeft.Y);
        drawList.AddDashedLine(topLeft, topRight, color, dashLength, gapLength, thickness); // Top
        drawList.AddDashedLine(bottomLeft, bottomRight, color, dashLength, gapLength, thickness); // Bottom
        drawList.AddDashedLine(topLeft, bottomLeft, color, dashLength, gapLength, thickness); // Left
        drawList.AddDashedLine(topRight, bottomRight, color, dashLength, gapLength, thickness); // Right
    }

    public static void AddDashedLine(this ImDrawListPtr drawList, Vector2 start, Vector2 end, uint color, float dashLength, float gapLength, float thickness)
    {
        var direction = end - start;
        var totalLength = direction.Length();
        direction = Vector2.Normalize(direction);

        var currentLength = 0.0f;
        var draw = true;

        while (currentLength < totalLength)
        {
            var segmentLength = draw ? dashLength : gapLength;
            if (currentLength + segmentLength > totalLength)
                segmentLength = totalLength - currentLength;

            if (draw)
            {
                var segmentStart = start + direction * currentLength;
                var segmentEnd = start + direction * (currentLength + segmentLength);
                drawList.AddLine(segmentStart, segmentEnd, color, thickness);
            }

            currentLength += segmentLength;
            draw = !draw; // Toggle between drawing and not drawing (dash/gap)
        }
    }
}
