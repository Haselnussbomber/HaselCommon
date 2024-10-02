using System.Collections.Generic;
using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

public partial class Node
{
    private struct FlexLineRunningLayout
    {
        // Total flex grow factors of flex items which are to be laid in the current
        // line. This is decremented as free space is distributed.
        public float TotalFlexGrowFactors;

        // Total flex shrink factors of flex items which are to be laid in the current
        // line. This is decremented as free space is distributed.
        public float TotalFlexShrinkScaledFactors;

        // The amount of available space within inner dimensions of the line which may
        // still be distributed.
        public float RemainingFreeSpace;

        // The size of the mainDim for the row after considering size, padding, margin
        // and border of flex items. This is used to calculate maxLineDim after going
        // through all the rows to decide on the main axis size of owner.
        public float MainDim;

        // The size of the crossDim for the row after considering size, padding,
        // margin and border of flex items. Used for calculating containers crossSize.
        public float CrossDim;
    }

    private struct FlexLine
    {
        // List of children which are part of the line flow. This means they are not
        // positioned absolutely, or with `display: "none"`, and do not overflow the
        // available dimensions.
        public List<Node> ItemsInFlow;

        // Accumulation of the dimensions and margin of all the children on the
        // current line. This will be used in order to either set the dimensions of
        // the node if none already exist or to compute the remaining space left for
        // the flexible children.
        public float SizeConsumed;

        // The index of the first item beyond the current line.
        public int EndOfLineIndex;

        // Number of edges along the line flow with an auto margin.
        public int NumberOfAutoMargins;

        // Layout information about the line computed in steps after line-breaking
        public FlexLineRunningLayout Layout;
    }

    private FlexLine CalculateFlexLine(Direction ownerDirection, float mainAxisownerSize, float availableInnerWidth, float availableInnerMainDim, int startOfLineIndex, int lineCount)
    {
        var itemsInFlow = new List<Node>(Children.Count);

        var sizeConsumed = 0.0f;
        var totalFlexGrowFactors = 0.0f;
        var totalFlexShrinkScaledFactors = 0.0f;
        var numberOfAutoMargins = 0;
        var endOfLineIndex = startOfLineIndex;
        var firstElementInLineIndex = startOfLineIndex;

        var sizeConsumedIncludingMinConstraint = 0f;
        var mainAxis = FlexDirection.ResolveDirection(ResolveDirection(ownerDirection));
        var isNodeFlexWrap = FlexWrap != Wrap.NoWrap;
        var gap = ComputeGapForAxis(mainAxis, availableInnerMainDim);

        // Add items to the current line until it's full or we run out of items.
        for (; endOfLineIndex < Count; endOfLineIndex++)
        {
            var child = this[endOfLineIndex];
            if (child.Display == Display.None || child.PositionType == PositionType.Absolute)
            {
                if (firstElementInLineIndex == endOfLineIndex)
                {
                    // We haven't found the first contributing element in the line yet.
                    firstElementInLineIndex++;
                }
                continue;
            }

            if (child.FlexStartMarginIsAuto(mainAxis, ownerDirection))
            {
                numberOfAutoMargins++;
            }
            if (child.FlexEndMarginIsAuto(mainAxis, ownerDirection))
            {
                numberOfAutoMargins++;
            }

            var isFirstElementInLine = endOfLineIndex - firstElementInLineIndex == 0;

            child._lineIndex = lineCount;
            var childMarginMainAxis = child.ComputeMarginForAxis(mainAxis, availableInnerWidth);
            var childLeadingGapMainAxis = isFirstElementInLine ? 0.0f : gap;
            var flexBasisWithMinAndMaxConstraints = BoundAxisWithinMinAndMax(
                child,
                mainAxis,
                child._layout.ComputedFlexBasis,
                mainAxisownerSize);

            // If this is a multi-line flow and this item pushes us over the available
            // size, we've hit the end of the current line. Break out of the loop and
            // lay out the current line.
            if (sizeConsumedIncludingMinConstraint + flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis > availableInnerMainDim &&
                isNodeFlexWrap && itemsInFlow.Count != 0)
            {
                break;
            }

            sizeConsumedIncludingMinConstraint += flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;
            sizeConsumed += flexBasisWithMinAndMaxConstraints + childMarginMainAxis + childLeadingGapMainAxis;

            if (child.IsNodeFlexible())
            {
                totalFlexGrowFactors += child.ResolveFlexGrow();

                // Unlike the grow factor, the shrink factor is scaled relative to the
                // child dimension.
                totalFlexShrinkScaledFactors += -child.ResolveFlexShrink() * child._layout.ComputedFlexBasis;
            }

            itemsInFlow.Add(child);
        }

        // The total flex factor needs to be floored to 1.
        if (totalFlexGrowFactors > 0 && totalFlexGrowFactors < 1)
        {
            totalFlexGrowFactors = 1;
        }

        // The total flex shrink factor needs to be floored to 1.
        if (totalFlexShrinkScaledFactors > 0 && totalFlexShrinkScaledFactors < 1)
        {
            totalFlexShrinkScaledFactors = 1;
        }

        return new FlexLine()
        {
            ItemsInFlow = itemsInFlow,
            SizeConsumed = sizeConsumed,
            EndOfLineIndex = endOfLineIndex,
            NumberOfAutoMargins = numberOfAutoMargins,
            Layout = new FlexLineRunningLayout()
            {
                TotalFlexGrowFactors = totalFlexGrowFactors,
                TotalFlexShrinkScaledFactors = totalFlexShrinkScaledFactors,
            }
        };
    }
}
