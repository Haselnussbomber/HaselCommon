using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

public partial class Node
{
    // Given an offset to an edge, returns the offset to the opposite edge on the
    // same axis. This assumes that the width/height of both nodes is determined at
    // this point.
    private static float GetPositionOfOppositeEdge(float position, FlexDirection axis, Node containingNode, Node node)
    {
        return containingNode.Layout.GetMeasuredDimension(axis.Dimension()) -
            node.Layout.GetMeasuredDimension(axis.Dimension()) - position;
    }

    private static void SetChildTrailingPosition(Node node, Node child, FlexDirection axis)
    {
        child.Layout.SetPosition(
            GetPositionOfOppositeEdge(child.Layout.GetPosition(axis.FlexStartEdge()), axis, node, child),
            axis.FlexEndEdge());
    }

    private bool NeedsTrailingPosition(FlexDirection axis)
    {
        return axis == FlexDirection.RowReverse || axis == FlexDirection.ColumnReverse;
    }
}
