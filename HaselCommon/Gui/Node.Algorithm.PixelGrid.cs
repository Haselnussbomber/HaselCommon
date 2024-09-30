using HaselCommon.Extensions;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

public partial class Node
{
    internal static float RoundValueToPixelGrid(
        double value,
        double pointScaleFactor,
        bool forceCeil,
        bool forceFloor)
    {
        var scaledValue = value * pointScaleFactor;
        // We want to calculate `fractial` such that `floor(scaledValue) = scaledValue
        // - fractial`.
        var fractial = scaledValue % 1.0;

        if (fractial < 0.0)
        {
            // This branch is for handling negative numbers for `value`.
            //
            // Regarding `floor` and `ceil`. Note that for a number x, `floor(x) <= x <=
            // ceil(x)` even for negative numbers. Here are a couple of examples:
            //   - x =  2.2: floor( 2.2) =  2, ceil( 2.2) =  3
            //   - x = -2.2: floor(-2.2) = -3, ceil(-2.2) = -2
            //
            // Regarding `fmodf`. For fractional negative numbers, `fmodf` returns a
            // negative number. For example, `fmodf(-2.2) = -0.2`. However, we want
            // `fractial` to be the number such that subtracting it from `value` will
            // give us `floor(value)`. In the case of negative numbers, adding 1 to
            // `fmodf(value)` gives us this. Let's continue the example from above:
            //   - fractial = fmodf(-2.2) = -0.2
            //   - Add 1 to the fraction: fractial2 = fractial + 1 = -0.2 + 1 = 0.8
            //   - Finding the `floor`: -2.2 - fractial2 = -2.2 - 0.8 = -3
            ++fractial;
        }

        if (fractial.IsApproximately(0.0))
        {
            // First we check if the value is already rounded
            scaledValue = scaledValue - fractial;
        }
        else if (fractial.IsApproximately(1.0))
        {
            scaledValue = scaledValue - fractial + 1.0;
        }
        else if (forceCeil)
        {
            // Next we check if we need to use forced rounding
            scaledValue = scaledValue - fractial + 1.0;
        }
        else if (forceFloor)
        {
            scaledValue = scaledValue - fractial;
        }
        else
        {
            // Finally we just round the value
            scaledValue = scaledValue - fractial + (!double.IsNaN(fractial) && (fractial > 0.5 || fractial.IsApproximately(0.5)) ? 1.0 : 0.0);
        }

        return (double.IsNaN(scaledValue) || double.IsNaN(pointScaleFactor))
            ? float.NaN
            : (float)(scaledValue / pointScaleFactor);
    }

    private void RoundLayoutResultsToPixelGrid(double absoluteLeft, double absoluteTop)
    {
        var pointScaleFactor = Config.PointScaleFactor;

        var nodeLeft = Layout.PositionLeft;
        var nodeTop = Layout.PositionTop;

        var nodeWidth = Layout.Width;
        var nodeHeight = Layout.Height;

        var absoluteNodeLeft = absoluteLeft + nodeLeft;
        var absoluteNodeTop = absoluteTop + nodeTop;

        var absoluteNodeRight = absoluteNodeLeft + nodeWidth;
        var absoluteNodeBottom = absoluteNodeTop + nodeHeight;

        if (pointScaleFactor != 0.0f)
        {
            // If a node has a custom measure function we never want to round down its
            // size as this could lead to unwanted text truncation.
            var textRounding = NodeType == NodeType.Text;

            Layout.SetPosition(RoundValueToPixelGrid(nodeLeft, pointScaleFactor, false, textRounding), PhysicalEdge.Left);
            Layout.SetPosition(RoundValueToPixelGrid(nodeTop, pointScaleFactor, false, textRounding), PhysicalEdge.Top);

            // We multiply dimension by scale factor and if the result is close to the
            // whole number, we don't have any fraction To verify if the result is close
            // to whole number we want to check both floor and ceil numbers
            var hasFractionalWidth =
                !(nodeWidth * pointScaleFactor % 1.0f).IsApproximately(0f) &&
                !(nodeWidth * pointScaleFactor % 1.0f).IsApproximately(1.0f);
            var hasFractionalHeight =
                !(nodeHeight * pointScaleFactor % 1.0f).IsApproximately(0f) &&
                !(nodeHeight * pointScaleFactor % 1.0f).IsApproximately(1.0f);

            Layout.SetDimension(
                RoundValueToPixelGrid(
                    absoluteNodeRight,
                    pointScaleFactor,
                    textRounding && hasFractionalWidth,
                    textRounding && !hasFractionalWidth) -
                    RoundValueToPixelGrid(absoluteNodeLeft, pointScaleFactor, false, textRounding),
                Dimension.Width);

            Layout.SetDimension(
                RoundValueToPixelGrid(
                    absoluteNodeBottom,
                    pointScaleFactor,
                    textRounding && hasFractionalHeight,
                    textRounding && !hasFractionalHeight) -
                    RoundValueToPixelGrid(absoluteNodeTop, pointScaleFactor, false, textRounding),
                Dimension.Height);
        }

        foreach (var child in this)
        {
            child.RoundLayoutResultsToPixelGrid(absoluteNodeLeft, absoluteNodeTop);
        }
    }
}
