using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

public partial class Node
{
    private float CalculateBaseline()
    {
        float baseline;

        if (HasBaselineFunc)
        {
            baseline = Baseline(
                _layout.GetMeasuredDimension(Dimension.Width),
                _layout.GetMeasuredDimension(Dimension.Height));

            if (float.IsNaN(baseline))
                throw new Exception("Expect custom baseline function to not return NaN");

            return baseline;
        }

        Node? baselineChild = null;

        foreach (var child in this)
        {
            if (child._lineIndex > 0)
            {
                break;
            }

            if (child._positionType == PositionType.Absolute)
            {
                continue;
            }

            if (ResolveChildAlignment(this, child) == Align.Baseline || child.IsReferenceBaseline)
            {
                baselineChild = child;
                break;
            }

            baselineChild ??= child;
        }

        if (baselineChild == null)
        {
            return _layout.GetMeasuredDimension(Dimension.Height);
        }

        baseline = baselineChild.CalculateBaseline();
        return baseline + baselineChild._layout.GetPosition(PhysicalEdge.Top);
    }

    private bool IsBaselineLayout()
    {
        if (_flexDirection.IsColumn())
        {
            return false;
        }

        if (_alignItems == Align.Baseline)
        {
            return true;
        }

        foreach (var child in this)
        {
            if (child._positionType != PositionType.Absolute && child._alignSelf == Align.Baseline)
            {
                return true;
            }
        }

        return false;
    }
}
