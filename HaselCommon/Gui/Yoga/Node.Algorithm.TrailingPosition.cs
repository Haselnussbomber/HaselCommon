using HaselCommon.Gui.Yoga.Enums;
using HaselCommon.Gui.Yoga.Extensions;

namespace HaselCommon.Gui.Yoga;

public partial class Node
{
    // Given an offset to an edge, returns the offset to the opposite edge on the
    // same axis. This assumes that the width/height of both nodes is determined at
    // this point.
    private static float GetPositionOfOppositeEdge(float position, FlexDirection axis, Node containingNode, Node node)
    {
        return containingNode._layout.GetMeasuredDimension(axis.Dimension()) -
            node._layout.GetMeasuredDimension(axis.Dimension()) - position;
    }

    private static void SetChildTrailingPosition(Node node, Node child, FlexDirection axis)
    {
        child._layout.SetPosition(
            GetPositionOfOppositeEdge(child._layout.GetPosition(axis.FlexStartEdge()), axis, node, child),
            axis.FlexEndEdge());
    }
}
