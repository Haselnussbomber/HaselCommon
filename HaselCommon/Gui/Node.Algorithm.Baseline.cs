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
                Layout.GetMeasuredDimension(Dimension.Width),
                Layout.GetMeasuredDimension(Dimension.Height));

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

            if (child.PositionType == PositionType.Absolute)
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
            return Layout.GetMeasuredDimension(Dimension.Height);
        }

        baseline = baselineChild.CalculateBaseline();
        return baseline + baselineChild.Layout.GetPosition(PhysicalEdge.Top);
    }

    private bool IsBaselineLayout()
    {
        if (FlexDirection.IsColumn())
        {
            return false;
        }

        if (AlignItems == Align.Baseline)
        {
            return true;
        }

        foreach (var child in this)
        {
            if (child.PositionType != PositionType.Absolute && child.AlignSelf == Align.Baseline)
            {
                return true;
            }
        }

        return false;
    }
}
