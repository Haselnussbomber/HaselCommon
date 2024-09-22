using System.Numerics;
using HaselCommon.Utils;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public unsafe partial class Node
{
    private void DrawBorder()
    {
        var borderTop = IsDebugHovered ? 1 : YGNode->GetComputedBorder(YGEdge.Top);
        var borderRight = IsDebugHovered ? 1 : YGNode->GetComputedBorder(YGEdge.Right);
        var borderBottom = IsDebugHovered ? 1 : YGNode->GetComputedBorder(YGEdge.Bottom);
        var borderLeft = IsDebugHovered ? 1 : YGNode->GetComputedBorder(YGEdge.Left);

        if (borderTop == 0 && borderRight == 0 && borderBottom == 0 && borderLeft == 0)
        {
            if (IsDebugHovered)
                IsDebugHovered = false;

            return;
        }

        var borderRadiusTopLeft = Style.BorderRadiusTopLeft;
        var borderRadiusTopRight = Style.BorderRadiusTopRight;
        var borderRadiusBottomLeft = Style.BorderRadiusBottomLeft;
        var borderRadiusBottomRight = Style.BorderRadiusBottomRight;

        var cursorScreenPos = ImGui.GetWindowPos() + CumulativePosition - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
        var drawList = ImGui.GetWindowDrawList();
        var width = YGNode->GetComputedWidth();
        var height = YGNode->GetComputedHeight();

        var borderColorTop = IsDebugHovered ? Colors.Red : Style.BorderColorTop;
        var borderColorRight = IsDebugHovered ? Colors.Red : Style.BorderColorRight;
        var borderColorLeft = IsDebugHovered ? Colors.Red : Style.BorderColorLeft;
        var borderColorBottom = IsDebugHovered ? Colors.Red : Style.BorderColorBottom;

        if (IsDebugHovered)
            IsDebugHovered = false;

        drawList.Flags ^= ImDrawListFlags.AntiAliasedLines;

        // TODO: rounded borders don't have a color gradient
        // TODO: https://drafts.csswg.org/css-backgrounds-3/#corner-shaping

        if ((borderTop > 0 || borderLeft > 0) && borderRadiusTopLeft > 0)
        {
            drawList.AddBezierQuadratic(
                cursorScreenPos + new Vector2(0, borderRadiusTopLeft),
                cursorScreenPos,
                cursorScreenPos + new Vector2(borderRadiusTopLeft, 0),
                borderColorTop,
                borderTop);
        }

        if ((borderTop > 0 || borderRight > 0) && borderRadiusTopRight > 0)
        {
            drawList.AddBezierQuadratic(
                cursorScreenPos + new Vector2(width - borderRadiusTopRight, 0),
                cursorScreenPos + new Vector2(width, 0),
                cursorScreenPos + new Vector2(width, borderRadiusTopRight),
                borderColorTop,
                borderTop);
        }

        if ((borderBottom > 0 || borderRight > 0) && borderRadiusBottomRight > 0)
        {
            drawList.AddBezierQuadratic(
                cursorScreenPos + new Vector2(width - borderRadiusBottomRight, height),
                cursorScreenPos + new Vector2(width, height),
                cursorScreenPos + new Vector2(width, height - borderRadiusBottomRight),
                borderColorBottom,
                borderBottom);
        }

        if ((borderBottom > 0 || borderLeft > 0) && borderRadiusBottomLeft > 0)
        {
            drawList.AddBezierQuadratic(
                cursorScreenPos + new Vector2(0, height - borderRadiusBottomLeft),
                cursorScreenPos + new Vector2(0, height),
                cursorScreenPos + new Vector2(0 + borderRadiusBottomLeft, height),
                borderColorBottom,
                borderBottom);
        }

        if (borderTop > 0)
        {
            drawList.AddLine(
                cursorScreenPos + new Vector2(borderRadiusTopLeft, 0),
                cursorScreenPos + new Vector2(width - borderRadiusTopRight, 0),
                borderColorTop,
                borderTop);
        }

        if (borderRight > 0)
        {
            drawList.AddLine(
                cursorScreenPos + new Vector2(width - borderRight, borderRadiusTopRight),
                cursorScreenPos + new Vector2(width - borderRight, height - borderRadiusBottomRight),
                borderColorRight,
                borderRight);
        }

        if (borderBottom > 0)
        {
            drawList.AddLine(
                cursorScreenPos + new Vector2(0 + borderRadiusBottomLeft, height - borderBottom),
                cursorScreenPos + new Vector2(width - borderRadiusBottomRight, height - borderBottom),
                borderColorBottom,
                borderBottom);
        }

        if (borderLeft > 0)
        {
            drawList.AddLine(
                cursorScreenPos + new Vector2(0, borderRadiusTopLeft),
                cursorScreenPos + new Vector2(0, height - borderRadiusBottomLeft),
                borderColorLeft,
                borderLeft);
        }

        drawList.Flags ^= ImDrawListFlags.AntiAliasedLines;
    }
}
