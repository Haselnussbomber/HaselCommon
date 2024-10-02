using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;
using HaselCommon.Utils;

namespace HaselCommon.Gui;

public partial class Node
{
    private float PaddingAndBorderForAxis(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeInlineStartPaddingAndBorder(axis, direction, widthSize) +
            ComputeInlineEndPaddingAndBorder(axis, direction, widthSize);
    }

    private static float BoundAxisWithinMinAndMax(Node node, FlexDirection axis, float value, float axisSize)
    {
        var min = float.NaN;
        var max = float.NaN;

        if (axis.IsColumn())
        {
            min = node.GetMinDimension(Dimension.Height).Resolve(axisSize);
            max = node.GetMaxDimension(Dimension.Height).Resolve(axisSize);
        }
        else if (axis.IsRow())
        {
            min = node.GetMinDimension(Dimension.Width).Resolve(axisSize);
            max = node.GetMaxDimension(Dimension.Width).Resolve(axisSize);
        }

        if (max >= 0 && value > max)
        {
            return max;
        }

        if (min >= 0 && value < min)
        {
            return min;
        }

        return value;
    }

    // Like boundAxisWithinMinAndMax but also ensures that the value doesn't
    // go below the padding and border amount.
    private static float BoundAxis(Node node, FlexDirection axis, Direction direction, float value, float axisSize, float widthSize)
    {
        return MathUtils.MaxOrDefined(
            BoundAxisWithinMinAndMax(node, axis, value, axisSize),
            node.PaddingAndBorderForAxis(axis, direction, widthSize));
    }
}
