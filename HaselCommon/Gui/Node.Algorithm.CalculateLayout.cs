using System.Linq;
using System.Numerics;
using HaselCommon.Extensions;
using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;
using HaselCommon.Utils;

namespace HaselCommon.Gui;

public partial class Node
{
    private static uint CurrentGenerationCount;

    private static void ConstrainMaxSizeForMode(Node node, FlexDirection axis, float ownerAxisSize, float ownerWidth, ref SizingMode mode, ref float size)
    {
        var maxSize = node.GetMaxDimension(axis.Dimension()).Resolve(ownerAxisSize) + node.ComputeMarginForAxis(axis, ownerWidth);
        switch (mode)
        {
            case SizingMode.StretchFit:
            case SizingMode.FitContent:
                size = (float.IsNaN(maxSize) || size < maxSize) ? size : maxSize;
                break;
            case SizingMode.MaxContent:
                if (!float.IsNaN(maxSize))
                {
                    mode = SizingMode.FitContent;
                    size = maxSize;
                }
                break;
        }
    }

    private static void ComputeFlexBasisForChild(
        Node node,
        Node child,
        float width,
        SizingMode widthMode,
        float height,
        float ownerWidth,
        float ownerHeight,
        SizingMode heightMode,
        Direction direction,
        uint depth,
        uint generationCount)
    {
        var mainAxis = node.FlexDirection.ResolveDirection(direction);
        var isMainAxisRow = mainAxis.IsRow();
        var mainAxisSize = isMainAxisRow ? width : height;
        var mainAxisownerSize = isMainAxisRow ? ownerWidth : ownerHeight;

        var childWidth = float.NaN;
        var childHeight = float.NaN;
        SizingMode childWidthSizingMode;
        SizingMode childHeightSizingMode;

        var resolvedFlexBasis = child.FlexBasis.Resolve(mainAxisownerSize);
        var isRowStyleDimDefined = child.HasDefiniteLength(Dimension.Width, ownerWidth);
        var isColumnStyleDimDefined = child.HasDefiniteLength(Dimension.Height, ownerHeight);

        if (!float.IsNaN(resolvedFlexBasis) && !float.IsNaN(mainAxisSize))
        {
            if (float.IsNaN(child.Layout.ComputedFlexBasis) || (child.Config.ExperimentalFeatures.HasFlag(ExperimentalFeature.WebFlexBasis) && child.Layout.ComputedFlexBasisGeneration != generationCount))
            {
                var paddingAndBorder = child.PaddingAndBorderForAxis(mainAxis, direction, ownerWidth);
                child.Layout.ComputedFlexBasis = MathUtils.MaxOrDefined(resolvedFlexBasis, paddingAndBorder);
            }
        }
        else if (isMainAxisRow && isRowStyleDimDefined)
        {
            // The width is definite, so use that as the flex basis.
            var paddingAndBorder = child.PaddingAndBorderForAxis(FlexDirection.Row, direction, ownerWidth);
            child.Layout.ComputedFlexBasis = MathUtils.MaxOrDefined(child.GetResolvedDimension(Dimension.Width).Resolve(ownerWidth), paddingAndBorder);
        }
        else if (!isMainAxisRow && isColumnStyleDimDefined)
        {
            // The height is definite, so use that as the flex basis.
            var paddingAndBorder = child.PaddingAndBorderForAxis(FlexDirection.Column, direction, ownerWidth);
            child.Layout.ComputedFlexBasis = MathUtils.MaxOrDefined(child.GetResolvedDimension(Dimension.Height).Resolve(ownerHeight), paddingAndBorder);
        }
        else
        {
            // Compute the flex basis and hypothetical main size (i.e. the clamped flex
            // basis).
            childWidthSizingMode = SizingMode.MaxContent;
            childHeightSizingMode = SizingMode.MaxContent;

            var marginRow = child.ComputeMarginForAxis(FlexDirection.Row, ownerWidth);
            var marginColumn = child.ComputeMarginForAxis(FlexDirection.Column, ownerWidth);

            if (isRowStyleDimDefined)
            {
                childWidth = child.GetResolvedDimension(Dimension.Width).Resolve(ownerWidth) + marginRow;
                childWidthSizingMode = SizingMode.StretchFit;
            }
            if (isColumnStyleDimDefined)
            {
                childHeight = child.GetResolvedDimension(Dimension.Height).Resolve(ownerHeight) + marginColumn;
                childHeightSizingMode = SizingMode.StretchFit;
            }

            // The W3C spec doesn't say anything about the 'overflow' property, but all
            // major browsers appear to implement the following logic.
            if ((!isMainAxisRow && node.Overflow == Overflow.Scroll) || node.Overflow != Overflow.Scroll)
            {
                if (float.IsNaN(childWidth) && !float.IsNaN(width))
                {
                    childWidth = width;
                    childWidthSizingMode = SizingMode.FitContent;
                }
            }

            if ((isMainAxisRow && node.Overflow == Overflow.Scroll) || node.Overflow != Overflow.Scroll)
            {
                if (float.IsNaN(childHeight) && !float.IsNaN(height))
                {
                    childHeight = height;
                    childHeightSizingMode = SizingMode.FitContent;
                }
            }

            if (child.AspectRatio.IsDefined)
            {
                if (!isMainAxisRow && childWidthSizingMode == SizingMode.StretchFit)
                {
                    childHeight = marginColumn + (childWidth - marginRow) / child.AspectRatio.Value;
                    childHeightSizingMode = SizingMode.StretchFit;
                }
                else if (isMainAxisRow && childHeightSizingMode == SizingMode.StretchFit)
                {
                    childWidth = marginRow + (childHeight - marginColumn) * child.AspectRatio.Value;
                    childWidthSizingMode = SizingMode.StretchFit;
                }
            }

            // If child has no defined size in the cross axis and is set to stretch, set
            // the cross axis to be measured exactly with the available inner width

            var hasExactWidth = !float.IsNaN(width) && widthMode == SizingMode.StretchFit;
            var childWidthStretch = ResolveChildAlignment(node, child) == Align.Stretch && childWidthSizingMode != SizingMode.StretchFit;
            if (!isMainAxisRow && !isRowStyleDimDefined && hasExactWidth && childWidthStretch)
            {
                childWidth = width;
                childWidthSizingMode = SizingMode.StretchFit;
                if (child.AspectRatio.IsDefined)
                {
                    childHeight = (childWidth - marginRow) / child.AspectRatio.Value;
                    childHeightSizingMode = SizingMode.StretchFit;
                }
            }

            var hasExactHeight = !float.IsNaN(height) && heightMode == SizingMode.StretchFit;
            var childHeightStretch = ResolveChildAlignment(node, child) == Align.Stretch && childHeightSizingMode != SizingMode.StretchFit;
            if (isMainAxisRow && !isColumnStyleDimDefined && hasExactHeight && childHeightStretch)
            {
                childHeight = height;
                childHeightSizingMode = SizingMode.StretchFit;

                if (child.AspectRatio.IsDefined)
                {
                    childWidth = (childHeight - marginColumn) * child.AspectRatio.Value;
                    childWidthSizingMode = SizingMode.StretchFit;
                }
            }

            ConstrainMaxSizeForMode(
                child,
                FlexDirection.Row,
                ownerWidth,
                ownerWidth,
                ref childWidthSizingMode,
                ref childWidth);
            ConstrainMaxSizeForMode(
                child,
                FlexDirection.Column,
                ownerHeight,
                ownerWidth,
                ref childHeightSizingMode,
                ref childHeight);

            // Measure the child
            CalculateLayoutInternal(
                child,
                childWidth,
                childHeight,
                direction,
                childWidthSizingMode,
                childHeightSizingMode,
                ownerWidth,
                ownerHeight,
                false,
                depth,
                generationCount);

            child.Layout.ComputedFlexBasis = MathUtils.MaxOrDefined(child.Layout.GetMeasuredDimension(mainAxis.Dimension()), child.PaddingAndBorderForAxis(mainAxis, direction, ownerWidth));
        }

        child.Layout.ComputedFlexBasisGeneration = generationCount;
    }

    private static void MeasureNodeWithMeasureFunc(
        Node node,
        Direction direction,
        float availableWidth,
        float availableHeight,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        float ownerWidth,
        float ownerHeight)
    {
        if (!node.HasMeasureFunc)
            throw new Exception("Expected node to have custom measure function");

        if (widthSizingMode == SizingMode.MaxContent)
        {
            availableWidth = float.NaN;
        }
        if (heightSizingMode == SizingMode.MaxContent)
        {
            availableHeight = float.NaN;
        }

        var paddingAndBorderAxisRow = node.Layout.GetPadding(PhysicalEdge.Left) +
            node.Layout.GetPadding(PhysicalEdge.Right) + node.Layout.GetBorder(PhysicalEdge.Left) +
            node.Layout.GetBorder(PhysicalEdge.Right);
        var paddingAndBorderAxisColumn = node.Layout.GetPadding(PhysicalEdge.Top) +
            node.Layout.GetPadding(PhysicalEdge.Bottom) + node.Layout.GetBorder(PhysicalEdge.Top) +
            node.Layout.GetBorder(PhysicalEdge.Bottom);

        // We want to make sure we don't call measure with negative size
        var innerWidth = float.IsNaN(availableWidth)
            ? availableWidth
            : MathUtils.MaxOrDefined(0.0f, availableWidth - paddingAndBorderAxisRow);
        var innerHeight = float.IsNaN(availableHeight)
            ? availableHeight
            : MathUtils.MaxOrDefined(0.0f, availableHeight - paddingAndBorderAxisColumn);

        if (widthSizingMode == SizingMode.StretchFit && heightSizingMode == SizingMode.StretchFit)
        {
            // Don't bother sizing the text if both dimensions are already defined.
            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Row,
                    direction,
                    availableWidth,
                    ownerWidth,
                    ownerWidth),
                Dimension.Width);
            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Column,
                    direction,
                    availableHeight,
                    ownerHeight,
                    ownerWidth),
                Dimension.Height);
        }
        else
        {
            // Measure the text under the current constraints.
            var measuredSize = node.MeasureWrapper(
                innerWidth,
                widthSizingMode.MeasureMode(),
                innerHeight,
                heightSizingMode.MeasureMode());

            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Row,
                    direction,
                    (widthSizingMode == SizingMode.MaxContent ||
                     widthSizingMode == SizingMode.FitContent)
                        ? measuredSize.X + paddingAndBorderAxisRow
                        : availableWidth,
                    ownerWidth,
                    ownerWidth),
                Dimension.Width);

            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Column,
                    direction,
                    (heightSizingMode == SizingMode.MaxContent ||
                     heightSizingMode == SizingMode.FitContent)
                        ? measuredSize.Y + paddingAndBorderAxisColumn
                        : availableHeight,
                    ownerHeight,
                    ownerWidth),
                Dimension.Height);
        }
    }

    // For nodes with no children, use the available values if they were provided,
    // or the minimum size as indicated by the padding and border sizes.
    private static void MeasureNodeWithoutChildren(
        Node node,
        Direction direction,
        float availableWidth,
        float availableHeight,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        float ownerWidth,
        float ownerHeight)
    {
        var width = availableWidth;
        if (widthSizingMode == SizingMode.MaxContent || widthSizingMode == SizingMode.FitContent)
        {
            width = node.Layout.GetPadding(PhysicalEdge.Left) + node.Layout.GetPadding(PhysicalEdge.Right) +
                node.Layout.GetBorder(PhysicalEdge.Left) + node.Layout.GetBorder(PhysicalEdge.Right);
        }

        node.Layout.SetMeasuredDimension(
            BoundAxis(node, FlexDirection.Row, direction, width, ownerWidth, ownerWidth),
            Dimension.Width);

        var height = availableHeight;
        if (heightSizingMode == SizingMode.MaxContent || heightSizingMode == SizingMode.FitContent)
        {
            height = node.Layout.GetPadding(PhysicalEdge.Top) + node.Layout.GetPadding(PhysicalEdge.Bottom) +
                node.Layout.GetBorder(PhysicalEdge.Top) + node.Layout.GetBorder(PhysicalEdge.Bottom);
        }

        node.Layout.SetMeasuredDimension(
            BoundAxis(
                node,
                FlexDirection.Column,
                direction,
                height,
                ownerHeight,
                ownerWidth),
            Dimension.Height);
    }

    private static bool MeasureNodeWithFixedSize(
        Node node,
        Direction direction,
        float availableWidth,
        float availableHeight,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        float ownerWidth,
        float ownerHeight)
    {
        if ((!float.IsNaN(availableWidth) && widthSizingMode == SizingMode.FitContent && availableWidth <= 0.0f) ||
            (!float.IsNaN(availableHeight) && heightSizingMode == SizingMode.FitContent && availableHeight <= 0.0f) ||
            (widthSizingMode == SizingMode.StretchFit && heightSizingMode == SizingMode.StretchFit))
        {
            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Row,
                    direction,
                    float.IsNaN(availableWidth) ||
                            (widthSizingMode == SizingMode.FitContent &&
                             availableWidth < 0.0f)
                        ? 0.0f
                        : availableWidth,
                    ownerWidth,
                    ownerWidth),
                Dimension.Width);

            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    FlexDirection.Column,
                    direction,
                    float.IsNaN(availableHeight) ||
                            (heightSizingMode == SizingMode.FitContent &&
                             availableHeight < 0.0f)
                        ? 0.0f
                        : availableHeight,
                    ownerHeight,
                    ownerWidth),
                Dimension.Height);
            return true;
        }

        return false;
    }

    private static void ZeroOutLayoutRecursively(Node node)
    {
        node.Layout = new();
        node.HasNewLayout = true;

        foreach (var child in node)
            ZeroOutLayoutRecursively(child);
    }

    private static float CalculateAvailableInnerDimension(
        Node node,
        Dimension dimension,
        float availableDim,
        float paddingAndBorder,
        float ownerDim)
    {
        var availableInnerDim = availableDim - paddingAndBorder;

        // Max dimension overrides predefined dimension value; Min dimension in turn
        // overrides both of the above
        if (!float.IsNaN(availableInnerDim))
        {
            // We want to make sure our available height does not violate min and max
            // constraints
            var minDimensionOptional = node.GetMinDimension(dimension).Resolve(ownerDim);
            var minInnerDim = float.IsNaN(minDimensionOptional)
                ? 0.0f
                : minDimensionOptional - paddingAndBorder;

            var maxDimensionOptional = node.GetMaxDimension(dimension).Resolve(ownerDim);
            var maxInnerDim = float.IsNaN(maxDimensionOptional)
                ? float.MaxValue
                : maxDimensionOptional - paddingAndBorder;

            availableInnerDim = MathUtils.MaxOrDefined(MathUtils.MinOrDefined(availableInnerDim, maxInnerDim), minInnerDim);
        }

        return availableInnerDim;
    }

    private static float ComputeFlexBasisForChildren(
        Node node,
        float availableInnerWidth,
        float availableInnerHeight,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        Direction direction,
        FlexDirection mainAxis,
        bool performLayout,
        uint depth,
        uint generationCount)
    {
        var totalOuterFlexBasis = 0.0f;
        Node? singleFlexChild = null;
        var sizingModeMainDim = mainAxis.IsRow() ? widthSizingMode : heightSizingMode;

        // If there is only one child with flexGrow + flexShrink it means we can set
        // the computedFlexBasis to 0 instead of measuring and shrinking / flexing the
        // child to exactly match the remaining space
        if (sizingModeMainDim == SizingMode.StretchFit)
        {
            foreach (var child in node)
            {
                if (child.IsNodeFlexible())
                {
                    if (singleFlexChild != null || child.ResolveFlexGrow().IsApproximately(0.0f) || child.ResolveFlexShrink().IsApproximately(0.0f))
                    {
                        // There is already a flexible child, or this flexible child doesn't
                        // have flexGrow and flexShrink, abort
                        singleFlexChild = null;
                        break;
                    }
                    else
                    {
                        singleFlexChild = child;
                    }
                }
            }
        }

        foreach (var child in node)
        {
            child.ResolveDimension();

            if (child.Display == Display.None)
            {
                ZeroOutLayoutRecursively(child);
                child.HasNewLayout = true;
                child.IsDirty = false;
                continue;
            }

            if (performLayout)
            {
                // Set the initial position (relative to the owner).
                var childDirection = child.ResolveDirection(direction);
                child.SetPosition(childDirection, availableInnerWidth, availableInnerHeight);
            }

            if (child.PositionType == PositionType.Absolute)
            {
                continue;
            }

            if (child == singleFlexChild)
            {
                child.Layout.ComputedFlexBasisGeneration = generationCount;
                child.Layout.ComputedFlexBasis = 0;
            }
            else
            {
                ComputeFlexBasisForChild(
                    node,
                    child,
                    availableInnerWidth,
                    widthSizingMode,
                    availableInnerHeight,
                    availableInnerWidth,
                    availableInnerHeight,
                    heightSizingMode,
                    direction,
                    depth,
                    generationCount);
            }

            totalOuterFlexBasis += child.Layout.ComputedFlexBasis + child.ComputeMarginForAxis(mainAxis, availableInnerWidth);
        }

        return totalOuterFlexBasis;
    }

    // It distributes the free space to the flexible items and ensures that the size
    // of the flex items abide the min and max constraints. At the end of this
    // function the child nodes would have proper size. Prior using this function
    // please ensure that distributeFreeSpaceFirstPass is called.
    private static float DistributeFreeSpaceSecondPass(
        ref FlexLine flexLine,
        Node node,
        FlexDirection mainAxis,
        FlexDirection crossAxis,
        Direction direction,
        float mainAxisownerSize,
        float availableInnerMainDim,
        float availableInnerCrossDim,
        float availableInnerWidth,
        float availableInnerHeight,
        bool mainAxisOverflows,
        SizingMode sizingModeCrossDim,
        bool performLayout,
        uint depth,
        uint generationCount)
    {
        float childFlexBasis;
        float flexShrinkScaledFactor;
        float flexGrowFactor;
        float deltaFreeSpace = 0;
        var isMainAxisRow = mainAxis.IsRow();
        var isNodeFlexWrap = node.FlexWrap != Wrap.NoWrap;

        foreach (var currentLineChild in flexLine.ItemsInFlow)
        {
            childFlexBasis = BoundAxisWithinMinAndMax(
                currentLineChild,
                mainAxis,
                currentLineChild.Layout.ComputedFlexBasis,
                mainAxisownerSize);

            var updatedMainSize = childFlexBasis;

            if (!float.IsNaN(flexLine.Layout.RemainingFreeSpace) && flexLine.Layout.RemainingFreeSpace < 0)
            {
                flexShrinkScaledFactor = -currentLineChild.ResolveFlexShrink() * childFlexBasis;

                // Is this child able to shrink?
                if (flexShrinkScaledFactor != 0)
                {
                    float childSize;

                    if (!float.IsNaN(flexLine.Layout.TotalFlexShrinkScaledFactors) && flexLine.Layout.TotalFlexShrinkScaledFactors == 0)
                    {
                        childSize = childFlexBasis + flexShrinkScaledFactor;
                    }
                    else
                    {
                        childSize = childFlexBasis + flexLine.Layout.RemainingFreeSpace / flexLine.Layout.TotalFlexShrinkScaledFactors * flexShrinkScaledFactor;
                    }

                    updatedMainSize = BoundAxis(
                        currentLineChild,
                        mainAxis,
                        direction,
                        childSize,
                        availableInnerMainDim,
                        availableInnerWidth);
                }
            }
            else if (!float.IsNaN(flexLine.Layout.RemainingFreeSpace) && flexLine.Layout.RemainingFreeSpace > 0)
            {
                flexGrowFactor = currentLineChild.ResolveFlexGrow();

                // Is this child able to grow?
                if (!float.IsNaN(flexGrowFactor) && flexGrowFactor != 0)
                {
                    updatedMainSize = BoundAxis(
                        currentLineChild,
                        mainAxis,
                        direction,
                        childFlexBasis + flexLine.Layout.RemainingFreeSpace / flexLine.Layout.TotalFlexGrowFactors * flexGrowFactor,
                        availableInnerMainDim,
                        availableInnerWidth);
                }
            }

            if (float.IsNaN(updatedMainSize))
                throw new Exception($"updatedMainSize is undefined. mainAxisownerSize: {mainAxisownerSize}");

            deltaFreeSpace += updatedMainSize - childFlexBasis;

            var marginMain = currentLineChild.ComputeMarginForAxis(mainAxis, availableInnerWidth);
            var marginCross = currentLineChild.ComputeMarginForAxis(crossAxis, availableInnerWidth);

            var childCrossSize = float.NaN;
            var childMainSize = updatedMainSize + marginMain;
            SizingMode childCrossSizingMode;
            var childMainSizingMode = SizingMode.StretchFit;

            if (currentLineChild.AspectRatio.IsDefined)
            {
                childCrossSize = isMainAxisRow
                    ? (childMainSize - marginMain) / currentLineChild.AspectRatio.Value
                    : (childMainSize - marginMain) * currentLineChild.AspectRatio.Value;
                childCrossSizingMode = SizingMode.StretchFit;

                childCrossSize += marginCross;
            }
            else if (
                !float.IsNaN(availableInnerCrossDim) &&
                !currentLineChild.HasDefiniteLength(crossAxis.Dimension(), availableInnerCrossDim) &&
                sizingModeCrossDim == SizingMode.StretchFit &&
                !(isNodeFlexWrap && mainAxisOverflows) &&
                ResolveChildAlignment(node, currentLineChild) == Align.Stretch &&
                !currentLineChild.FlexStartMarginIsAuto(crossAxis, direction) &&
                !currentLineChild.FlexEndMarginIsAuto(crossAxis, direction))
            {
                childCrossSize = availableInnerCrossDim;
                childCrossSizingMode = SizingMode.StretchFit;
            }
            else if (!currentLineChild.HasDefiniteLength(crossAxis.Dimension(), availableInnerCrossDim))
            {
                childCrossSize = availableInnerCrossDim;
                childCrossSizingMode = float.IsNaN(childCrossSize)
                    ? SizingMode.MaxContent
                    : SizingMode.FitContent;
            }
            else
            {
                childCrossSize = currentLineChild.GetResolvedDimension(crossAxis.Dimension()).Resolve(availableInnerCrossDim) + marginCross;
                var isLoosePercentageMeasurement = currentLineChild.GetResolvedDimension(crossAxis.Dimension()).Unit == Unit.Percent && sizingModeCrossDim != SizingMode.StretchFit;
                childCrossSizingMode = float.IsNaN(childCrossSize) || isLoosePercentageMeasurement
                    ? SizingMode.MaxContent
                    : SizingMode.StretchFit;
            }

            ConstrainMaxSizeForMode(
                currentLineChild,
                mainAxis,
                availableInnerMainDim,
                availableInnerWidth,
                ref childMainSizingMode,
                ref childMainSize);
            ConstrainMaxSizeForMode(
                currentLineChild,
                crossAxis,
                availableInnerCrossDim,
                availableInnerWidth,
                ref childCrossSizingMode,
                ref childCrossSize);

            var requiresStretchLayout =
                !currentLineChild.HasDefiniteLength(crossAxis.Dimension(), availableInnerCrossDim) &&
                ResolveChildAlignment(node, currentLineChild) == Align.Stretch &&
                !currentLineChild.FlexStartMarginIsAuto(crossAxis, direction) &&
                !currentLineChild.FlexEndMarginIsAuto(crossAxis, direction);

            var childWidth = isMainAxisRow ? childMainSize : childCrossSize;
            var childHeight = !isMainAxisRow ? childMainSize : childCrossSize;

            var childWidthSizingMode =
                isMainAxisRow ? childMainSizingMode : childCrossSizingMode;
            var childHeightSizingMode =
                !isMainAxisRow ? childMainSizingMode : childCrossSizingMode;

            var isLayoutPass = performLayout && !requiresStretchLayout;
            // Recursively call the layout algorithm for this child with the updated
            // main size.

            if (float.IsNaN(childMainSize) && childMainSizingMode != SizingMode.MaxContent)
                throw new Exception("childMainSize is undefined so childMainSizingMode must be MaxContent");

            if (float.IsNaN(childCrossSize) && childCrossSizingMode != SizingMode.MaxContent)
                throw new Exception("childCrossSize is undefined so childCrossSizingMode must be MaxContent");

            CalculateLayoutInternal(
                currentLineChild,
                childWidth,
                childHeight,
                node.Layout.Direction,
                childWidthSizingMode,
                childHeightSizingMode,
                availableInnerWidth,
                availableInnerHeight,
                isLayoutPass,
                depth,
                generationCount);

            node.Layout.HadOverflow |= currentLineChild.Layout.HadOverflow;
        }

        return deltaFreeSpace;
    }

    // It distributes the free space to the flexible items.For those flexible items
    // whose min and max constraints are triggered, those flex item's clamped size
    // is removed from the remaingfreespace.
    private static void DistributeFreeSpaceFirstPass(
        ref FlexLine flexLine,
        Direction direction,
        FlexDirection mainAxis,
        float mainAxisownerSize,
        float availableInnerMainDim,
        float availableInnerWidth)
    {
        float flexShrinkScaledFactor;
        float flexGrowFactor;
        float baseMainSize;
        float boundMainSize;
        float deltaFreeSpace = 0;

        foreach (var currentLineChild in flexLine.ItemsInFlow)
        {
            var childFlexBasis = BoundAxisWithinMinAndMax(
                currentLineChild,
                mainAxis,
                currentLineChild.Layout.ComputedFlexBasis,
                mainAxisownerSize);

            if (flexLine.Layout.RemainingFreeSpace < 0)
            {
                flexShrinkScaledFactor = -currentLineChild.ResolveFlexShrink() * childFlexBasis;

                // Is this child able to shrink?
                if (!float.IsNaN(flexShrinkScaledFactor) && flexShrinkScaledFactor != 0)
                {
                    baseMainSize = childFlexBasis +
                        flexLine.Layout.RemainingFreeSpace /
                            flexLine.Layout.TotalFlexShrinkScaledFactors *
                            flexShrinkScaledFactor;

                    boundMainSize = BoundAxis(
                        currentLineChild,
                        mainAxis,
                        direction,
                        baseMainSize,
                        availableInnerMainDim,
                        availableInnerWidth);

                    if (!float.IsNaN(baseMainSize) && !float.IsNaN(boundMainSize) && baseMainSize != boundMainSize)
                    {
                        // By excluding this item's size and flex factor from remaining, this
                        // item's min/max constraints should also trigger in the second pass
                        // resulting in the item's size calculation being identical in the
                        // first and second passes.
                        deltaFreeSpace += boundMainSize - childFlexBasis;
                        flexLine.Layout.TotalFlexShrinkScaledFactors -= -currentLineChild.ResolveFlexShrink() * currentLineChild.Layout.ComputedFlexBasis;
                    }
                }
            }
            else if (!float.IsNaN(flexLine.Layout.RemainingFreeSpace) && flexLine.Layout.RemainingFreeSpace > 0)
            {
                flexGrowFactor = currentLineChild.ResolveFlexGrow();

                // Is this child able to grow?
                if (!float.IsNaN(flexGrowFactor) && flexGrowFactor != 0)
                {
                    baseMainSize = childFlexBasis +
                        flexLine.Layout.RemainingFreeSpace /
                            flexLine.Layout.TotalFlexGrowFactors * flexGrowFactor;

                    boundMainSize = BoundAxis(
                        currentLineChild,
                        mainAxis,
                        direction,
                        baseMainSize,
                        availableInnerMainDim,
                        availableInnerWidth);

                    if (!float.IsNaN(baseMainSize) && !float.IsNaN(boundMainSize) && baseMainSize != boundMainSize)
                    {
                        // By excluding this item's size and flex factor from remaining, this
                        // item's min/max constraints should also trigger in the second pass
                        // resulting in the item's size calculation being identical in the
                        // first and second passes.
                        deltaFreeSpace += boundMainSize - childFlexBasis;
                        flexLine.Layout.TotalFlexGrowFactors -= flexGrowFactor;
                    }
                }
            }
        }

        flexLine.Layout.RemainingFreeSpace -= deltaFreeSpace;
    }

    // Do two passes over the flex items to figure out how to distribute the
    // remaining space.
    //
    // The first pass finds the items whose min/max constraints trigger, freezes
    // them at those sizes, and excludes those sizes from the remaining space.
    //
    // The second pass sets the size of each flexible item. It distributes the
    // remaining space amongst the items whose min/max constraints didn't trigger in
    // the first pass. For the other items, it sets their sizes by forcing their
    // min/max constraints to trigger again.
    //
    // This two pass approach for resolving min/max constraints deviates from the
    // spec. The spec
    // (https://www.w3.org/TR/CSS-flexbox-1/#resolve-flexible-lengths) describes a
    // process that needs to be repeated a variable number of times. The algorithm
    // implemented here won't handle all cases but it was simpler to implement and
    // it mitigates performance concerns because we know exactly how many passes
    // it'll do.
    //
    // At the end of this function the child nodes would have the proper size
    // assigned to them.
    //
    private static void ResolveFlexibleLength(
        Node node,
        ref FlexLine flexLine,
        FlexDirection mainAxis,
        FlexDirection crossAxis,
        Direction direction,
        float mainAxisownerSize,
        float availableInnerMainDim,
        float availableInnerCrossDim,
        float availableInnerWidth,
        float availableInnerHeight,
        bool mainAxisOverflows,
        SizingMode sizingModeCrossDim,
        bool performLayout,
        uint depth,
        uint generationCount)
    {
        var originalFreeSpace = flexLine.Layout.RemainingFreeSpace;

        // First pass: detect the flex items whose min/max constraints trigger
        DistributeFreeSpaceFirstPass(
            ref flexLine,
            direction,
            mainAxis,
            mainAxisownerSize,
            availableInnerMainDim,
            availableInnerWidth);

        // Second pass: resolve the sizes of the flexible items
        var distributedFreeSpace = DistributeFreeSpaceSecondPass(
            ref flexLine,
            node,
            mainAxis,
            crossAxis,
            direction,
            mainAxisownerSize,
            availableInnerMainDim,
            availableInnerCrossDim,
            availableInnerWidth,
            availableInnerHeight,
            mainAxisOverflows,
            sizingModeCrossDim,
            performLayout,
            depth,
            generationCount);

        flexLine.Layout.RemainingFreeSpace = originalFreeSpace - distributedFreeSpace;
    }

    private static void JustifyMainAxis(
        Node node,
        ref FlexLine flexLine,
        int startOfLineIndex,
        FlexDirection mainAxis,
        FlexDirection crossAxis,
        Direction direction,
        SizingMode sizingModeMainDim,
        SizingMode sizingModeCrossDim,
        float mainAxisownerSize,
        float ownerWidth,
        float availableInnerMainDim,
        float availableInnerCrossDim,
        float availableInnerWidth,
        bool performLayout)
    {

        var leadingPaddingAndBorderMain = node.ComputeFlexStartPaddingAndBorder(mainAxis, direction, ownerWidth);
        var trailingPaddingAndBorderMain = node.ComputeFlexEndPaddingAndBorder(mainAxis, direction, ownerWidth);

        var gap = node.ComputeGapForAxis(mainAxis, availableInnerMainDim);

        // If we are using "at most" rules in the main axis, make sure that
        // remainingFreeSpace is 0 when min main dimension is not given
        if (sizingModeMainDim == SizingMode.FitContent && flexLine.Layout.RemainingFreeSpace > 0)
        {
            if (node.GetMinDimension(mainAxis.Dimension()).IsDefined && !float.IsNaN(node.GetMinDimension(mainAxis.Dimension()).Resolve(mainAxisownerSize)))
            {
                // This condition makes sure that if the size of main dimension(after
                // considering child nodes main dim, leading and trailing padding etc)
                // falls below min dimension, then the remainingFreeSpace is reassigned
                // considering the min dimension

                // `minAvailableMainDim` denotes minimum available space in which child
                // can be laid out, it will exclude space consumed by padding and border.
                var minAvailableMainDim = node.GetMinDimension(mainAxis.Dimension()).Resolve(mainAxisownerSize) - leadingPaddingAndBorderMain - trailingPaddingAndBorderMain;
                var occupiedSpaceByChildNodes = availableInnerMainDim - flexLine.Layout.RemainingFreeSpace;
                flexLine.Layout.RemainingFreeSpace = MathUtils.MaxOrDefined(0.0f, minAvailableMainDim - occupiedSpaceByChildNodes);
            }
            else
            {
                flexLine.Layout.RemainingFreeSpace = 0;
            }
        }

        // In order to position the elements in the main axis, we have two controls.
        // The space between the beginning and the first element and the space between
        // each two elements.
        float leadingMainDim = 0;
        var betweenMainDim = gap;
        var justifyContent = flexLine.Layout.RemainingFreeSpace >= 0
            ? node.JustifyContent
            : FallbackAlignment(node.JustifyContent);

        if (flexLine.NumberOfAutoMargins == 0)
        {
            switch (justifyContent)
            {
                case Justify.Center:
                    leadingMainDim = flexLine.Layout.RemainingFreeSpace / 2;
                    break;
                case Justify.FlexEnd:
                    leadingMainDim = flexLine.Layout.RemainingFreeSpace;
                    break;
                case Justify.SpaceBetween:
                    if (flexLine.ItemsInFlow.Count > 1)
                    {
                        betweenMainDim += flexLine.Layout.RemainingFreeSpace / (flexLine.ItemsInFlow.Count - 1);
                    }
                    break;
                case Justify.SpaceEvenly:
                    // Space is distributed evenly across all elements
                    leadingMainDim = flexLine.Layout.RemainingFreeSpace / (flexLine.ItemsInFlow.Count + 1);
                    betweenMainDim += leadingMainDim;
                    break;
                case Justify.SpaceAround:
                    // Space on the edges is half of the space between elements
                    leadingMainDim = 0.5f * flexLine.Layout.RemainingFreeSpace / flexLine.ItemsInFlow.Count;
                    betweenMainDim += leadingMainDim * 2;
                    break;
                case Justify.FlexStart:
                    break;
            }
        }

        flexLine.Layout.MainDim = leadingPaddingAndBorderMain + leadingMainDim;
        flexLine.Layout.CrossDim = 0;

        float maxAscentForCurrentLine = 0;
        float maxDescentForCurrentLine = 0;
        var isNodeBaselineLayout = node.IsBaselineLayout();
        for (var i = startOfLineIndex; i < flexLine.EndOfLineIndex; i++)
        {
            var child = node[i];

            if (child.Display == Display.None)
            {
                continue;
            }

            if (child.PositionType == PositionType.Absolute &&
                child.IsFlexStartPositionDefined(mainAxis, direction) &&
                !child.IsFlexStartPositionAuto(mainAxis, direction))
            {
                if (performLayout)
                {
                    // In case the child is position absolute and has left/top being
                    // defined, we override the position to whatever the user said (and
                    // margin/border).
                    child.Layout.SetPosition(
                        child.ComputeFlexStartPosition(mainAxis, direction, availableInnerMainDim) +
                            node.ComputeFlexStartBorder(mainAxis, direction) +
                            child.ComputeFlexStartMargin(mainAxis, direction, availableInnerWidth),
                        mainAxis.FlexStartEdge());
                }
            }
            else
            {
                // Now that we placed the element, we need to update the variables.
                // We need to do that only for relative elements. Absolute elements do not
                // take part in that phase.
                if (child.PositionType != PositionType.Absolute)
                {
                    if (child.FlexStartMarginIsAuto(mainAxis, direction) && flexLine.Layout.RemainingFreeSpace > 0.0f)
                    {
                        flexLine.Layout.MainDim += flexLine.Layout.RemainingFreeSpace / flexLine.NumberOfAutoMargins;
                    }

                    if (performLayout)
                    {
                        child.Layout.SetPosition(child.Layout.GetPosition(mainAxis.FlexStartEdge()) + flexLine.Layout.MainDim, mainAxis.FlexStartEdge());
                    }

                    if (child != flexLine.ItemsInFlow.Last())
                    {
                        flexLine.Layout.MainDim += betweenMainDim;
                    }

                    if (child.FlexEndMarginIsAuto(mainAxis, direction) && flexLine.Layout.RemainingFreeSpace > 0.0f)
                    {
                        flexLine.Layout.MainDim += flexLine.Layout.RemainingFreeSpace / flexLine.NumberOfAutoMargins;
                    }

                    var canSkipFlex = !performLayout && sizingModeCrossDim == SizingMode.StretchFit;
                    if (canSkipFlex)
                    {
                        // If we skipped the flex step, then we can't rely on the measuredDims
                        // because they weren't computed. This means we can't call
                        // dimensionWithMargin.
                        flexLine.Layout.MainDim += child.ComputeMarginForAxis(mainAxis, availableInnerWidth) + child.Layout.ComputedFlexBasis;
                        flexLine.Layout.CrossDim = availableInnerCrossDim;
                    }
                    else
                    {
                        // The main dimension is the sum of all the elements dimension plus
                        // the spacing.
                        flexLine.Layout.MainDim += child.DimensionWithMargin(mainAxis, availableInnerWidth);

                        if (isNodeBaselineLayout)
                        {
                            // If the child is baseline aligned then the cross dimension is
                            // calculated by adding maxAscent and maxDescent from the baseline.
                            var ascent = child.CalculateBaseline() + child.ComputeFlexStartMargin(FlexDirection.Column, direction, availableInnerWidth);
                            var descent =
                                child.Layout.GetMeasuredDimension(Dimension.Height) +
                                child.ComputeMarginForAxis(FlexDirection.Column, availableInnerWidth) -
                                ascent;

                            maxAscentForCurrentLine = MathUtils.MaxOrDefined(maxAscentForCurrentLine, ascent);
                            maxDescentForCurrentLine = MathUtils.MaxOrDefined(maxDescentForCurrentLine, descent);
                        }
                        else
                        {
                            // The cross dimension is the max of the elements dimension since
                            // there can only be one element in that cross dimension in the case
                            // when the items are not baseline aligned
                            flexLine.Layout.CrossDim = MathUtils.MaxOrDefined(flexLine.Layout.CrossDim, child.DimensionWithMargin(crossAxis, availableInnerWidth));
                        }
                    }
                }
                else if (performLayout)
                {
                    child.Layout.SetPosition(
                        child.Layout.GetPosition(mainAxis.FlexStartEdge()) +
                            node.ComputeFlexStartBorder(mainAxis, direction) +
                            leadingMainDim,
                        mainAxis.FlexStartEdge());
                }
            }
        }

        flexLine.Layout.MainDim += trailingPaddingAndBorderMain;

        if (isNodeBaselineLayout)
        {
            flexLine.Layout.CrossDim = maxAscentForCurrentLine + maxDescentForCurrentLine;
        }
    }

    /// <inheritdoc cref="CalculateLayout(float, float, Direction)"/>
    public void CalculateLayout(Vector2 ownerSize, Direction ownerDirection = Direction.LTR)
        => CalculateLayout(ownerSize.X, ownerSize.Y, ownerDirection);

    /// <summary>
    /// Calculates the layout of the tree rooted at the given node.<br/>
    /// Layout results may be read after calling CalculateLayout using properties
    /// like <see cref="LayoutResults.PositionLeft"/>, <see cref="LayoutResults.PositionTop"/>, etc.
    /// from <see cref="Layout"/>.<br/>
    /// <see cref="HasNewLayout"/> may be read to know if the layout of the node or its
    /// subtrees may have changed since the last time CalculateLayout was called.
    /// </summary>
    public void CalculateLayout(float ownerWidth, float ownerHeight, Direction ownerDirection)
    {
        CurrentGenerationCount++;
        ResolveDimension();

        float width;
        float height;
        SizingMode widthSizingMode;
        SizingMode heightSizingMode;

        if (HasDefiniteLength(Dimension.Width, ownerWidth))
        {
            width = _resolvedWidth.Resolve(ownerWidth) + ComputeMarginForAxis(FlexDirection.Row, ownerWidth);
            widthSizingMode = SizingMode.StretchFit;
        }
        else if (!float.IsNaN(MaxWidth.Resolve(ownerWidth)))
        {
            width = MaxWidth.Resolve(ownerWidth);
            widthSizingMode = SizingMode.FitContent;
        }
        else
        {
            width = ownerWidth;
            widthSizingMode = float.IsNaN(width) ? SizingMode.MaxContent : SizingMode.StretchFit;
        }

        if (HasDefiniteLength(Dimension.Height, ownerHeight))
        {
            height = _resolvedHeight.Resolve(ownerHeight) + ComputeMarginForAxis(FlexDirection.Column, ownerHeight);
            heightSizingMode = SizingMode.StretchFit;
        }
        else if (!float.IsNaN(MaxHeight.Resolve(ownerHeight)))
        {
            height = MaxHeight.Resolve(ownerHeight);
            heightSizingMode = SizingMode.FitContent;
        }
        else
        {
            height = ownerHeight;
            heightSizingMode = float.IsNaN(height) ? SizingMode.MaxContent : SizingMode.StretchFit;
        }

        if (CalculateLayoutInternal(
                this,
                width,
                height,
                ownerDirection,
                widthSizingMode,
                heightSizingMode,
                ownerWidth,
                ownerHeight,
                true,
                0, // tree root
                CurrentGenerationCount))
        {
            SetPosition(Layout.Direction, ownerWidth, ownerHeight);
            RoundLayoutResultsToPixelGrid(0.0f, 0.0f);
        }
    }

    //
    // This is a wrapper around the calculateLayoutImpl function. It determines
    // whether the layout request is redundant and can be skipped.
    //
    // Parameters:
    //  Input parameters are the same as calculateLayoutImpl (see above)
    //  Return parameter is true if layout was performed, false if skipped
    //
    private static bool CalculateLayoutInternal(
        Node node,
        float availableWidth,
        float availableHeight,
        Direction ownerDirection,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        float ownerWidth,
        float ownerHeight,
        bool performLayout,
        uint depth,
        uint generationCount)
    {
        var layout = node.Layout;

        depth++;

        var needToVisitNode =
            (node.IsDirty && layout.GenerationCount != generationCount) ||
            layout.ConfigVersion != node.Config.Version ||
            layout.LastOwnerDirection != ownerDirection;

        if (needToVisitNode)
        {
            // Invalidate the cached results.
            layout.NextCachedMeasurementsIndex = 0;
            layout.CachedLayout = new CachedMeasurement()
            {
                AvailableWidth = -1,
                AvailableHeight = -1,
                WidthSizingMode = SizingMode.MaxContent,
                HeightSizingMode = SizingMode.MaxContent,
                ComputedWidth = -1,
                ComputedHeight = -1,
            };
        }

        CachedMeasurement? cachedResults = null;

        // Determine whether the results are already cached. We maintain a separate
        // cache for layouts and measurements. A layout operation modifies the
        // positions and dimensions for nodes in the subtree. The algorithm assumes
        // that each node gets laid out a maximum of one time per tree layout, but
        // multiple measurements may be required to resolve all of the flex
        // dimensions. We handle nodes with measure functions specially here because
        // they are the most expensive to measure, so it's worth avoiding redundant
        // measurements if at all possible.
        if (node.HasMeasureFunc)
        {
            var marginAxisRow = node.ComputeMarginForAxis(FlexDirection.Row, ownerWidth);
            var marginAxisColumn = node.ComputeMarginForAxis(FlexDirection.Column, ownerWidth);

            // First, try to use the layout cache.
            if (node.CanUseCachedMeasurement(
                    widthSizingMode,
                    availableWidth,
                    heightSizingMode,
                    availableHeight,
                    layout.CachedLayout.WidthSizingMode,
                    layout.CachedLayout.AvailableWidth,
                    layout.CachedLayout.HeightSizingMode,
                    layout.CachedLayout.AvailableHeight,
                    layout.CachedLayout.ComputedWidth,
                    layout.CachedLayout.ComputedHeight,
                    marginAxisRow,
                    marginAxisColumn))
            {
                cachedResults = layout.CachedLayout;
            }
            else
            {
                // Try to use the measurement cache.
                for (var i = 0u; i < layout.NextCachedMeasurementsIndex; i++)
                {
                    if (node.CanUseCachedMeasurement(
                            widthSizingMode,
                            availableWidth,
                            heightSizingMode,
                            availableHeight,
                            layout.CachedMeasurements[i].WidthSizingMode,
                            layout.CachedMeasurements[i].AvailableWidth,
                            layout.CachedMeasurements[i].HeightSizingMode,
                            layout.CachedMeasurements[i].AvailableHeight,
                            layout.CachedMeasurements[i].ComputedWidth,
                            layout.CachedMeasurements[i].ComputedHeight,
                            marginAxisRow,
                            marginAxisColumn))
                    {
                        cachedResults = layout.CachedMeasurements[i];
                        break;
                    }
                }
            }
        }
        else if (performLayout)
        {
            if (layout.CachedLayout.AvailableWidth.IsApproximately(availableWidth) &&
                layout.CachedLayout.AvailableHeight.IsApproximately(availableHeight) &&
                layout.CachedLayout.WidthSizingMode == widthSizingMode &&
                layout.CachedLayout.HeightSizingMode == heightSizingMode)
            {
                cachedResults = layout.CachedLayout;
            }
        }
        else
        {
            for (var i = 0u; i < layout.NextCachedMeasurementsIndex; i++)
            {
                if (layout.CachedMeasurements[i].AvailableWidth.IsApproximately(availableWidth) &&
                    layout.CachedMeasurements[i].AvailableHeight.IsApproximately(availableHeight) &&
                    layout.CachedMeasurements[i].WidthSizingMode == widthSizingMode &&
                    layout.CachedMeasurements[i].HeightSizingMode == heightSizingMode)
                {
                    cachedResults = layout.CachedMeasurements[i];
                    break;
                }
            }
        }

        if (!needToVisitNode && cachedResults != null)
        {
            layout.MeasuredWidth = cachedResults.ComputedWidth;
            layout.MeasuredHeight = cachedResults.ComputedHeight;
        }
        else
        {
            CalculateLayoutImpl(
                node,
                availableWidth,
                availableHeight,
                ownerDirection,
                widthSizingMode,
                heightSizingMode,
                ownerWidth,
                ownerHeight,
                performLayout,
                depth,
                generationCount);

            layout.LastOwnerDirection = ownerDirection;
            layout.ConfigVersion = node.Config.Version;

            if (cachedResults == null)
            {
                if (layout.NextCachedMeasurementsIndex == LayoutResults.MaxCachedMeasurements)
                {
                    layout.NextCachedMeasurementsIndex = 0;
                }

                CachedMeasurement? newCacheEntry;

                if (performLayout)
                {
                    // Use the single layout cache entry.
                    newCacheEntry = layout.CachedLayout;
                }
                else
                {
                    // Allocate a new measurement cache entry.
                    newCacheEntry = layout.CachedMeasurements[layout.NextCachedMeasurementsIndex] = new();
                    layout.NextCachedMeasurementsIndex++;
                }

                newCacheEntry.AvailableWidth = availableWidth;
                newCacheEntry.AvailableHeight = availableHeight;
                newCacheEntry.WidthSizingMode = widthSizingMode;
                newCacheEntry.HeightSizingMode = heightSizingMode;
                newCacheEntry.ComputedWidth = layout.MeasuredWidth;
                newCacheEntry.ComputedHeight = layout.MeasuredHeight;
            }
        }

        if (performLayout)
        {
            node.Layout.Width = node.Layout.MeasuredWidth;
            node.Layout.Height = node.Layout.MeasuredHeight;
            node.HasNewLayout = true;
            node.IsDirty = false;
        }

        layout.GenerationCount = generationCount;

        return needToVisitNode || cachedResults == null;
    }

    //
    // This is the main routine that implements a subset of the flexbox layout
    // algorithm described in the W3C CSS documentation:
    // https://www.w3.org/TR/CSS3-flexbox/.
    //
    // Limitations of this algorithm, compared to the full standard:
    //  * Display property is always assumed to be 'flex' except for Text nodes,
    //    which are assumed to be 'inline-flex'.
    //  * The 'zIndex' property (or any form of z ordering) is not supported. Nodes
    //    are stacked in document order.
    //  * The 'order' property is not supported. The order of flex items is always
    //    defined by document order.
    //  * The 'visibility' property is always assumed to be 'visible'. Values of
    //    'collapse' and 'hidden' are not supported.
    //  * There is no support for forced breaks.
    //  * It does not support vertical inline directions (top-to-bottom or
    //    bottom-to-top text).
    //
    // Deviations from standard:
    //  * Section 4.5 of the spec indicates that all flex items have a default
    //    minimum main size. For text blocks, for example, this is the width of the
    //    widest word. Calculating the minimum width is expensive, so we forego it
    //    and assume a default minimum main size of 0.
    //  * Min/Max sizes in the main axis are not honored when resolving flexible
    //    lengths.
    //  * The spec indicates that the default value for 'flexDirection' is 'row',
    //    but the algorithm below assumes a default of 'column'.
    //
    // Input parameters:
    //    - node: current node to be sized and laid out
    //    - availableWidth & availableHeight: available size to be used for sizing
    //      the node or YGUndefined if the size is not available; interpretation
    //      depends on layout flags
    //    - ownerDirection: the inline (text) direction within the owner
    //      (left-to-right or right-to-left)
    //    - widthSizingMode: indicates the sizing rules for the width (see below
    //      for explanation)
    //    - heightSizingMode: indicates the sizing rules for the height (see below
    //      for explanation)
    //    - performLayout: specifies whether the caller is interested in just the
    //      dimensions of the node or it requires the entire node and its subtree to
    //      be laid out (with final positions)
    //
    // Details:
    //    This routine is called recursively to lay out subtrees of flexbox
    //    elements. It uses the information in node.style, which is treated as a
    //    read-only input. It is responsible for setting the layout.direction and
    //    layout.measuredDimensions fields for the input node as well as the
    //    layout.position and layout.lineIndex fields for its child nodes. The
    //    layout.measuredDimensions field includes any border or padding for the
    //    node but does not include margins.
    //
    //    When calling calculateLayoutImpl and calculateLayoutInternal, if the
    //    caller passes an available size of undefined then it must also pass a
    //    measure mode of SizingMode.MaxContent in that dimension.
    //
    private static void CalculateLayoutImpl(
        Node node,
        float availableWidth,
        float availableHeight,
        Direction ownerDirection,
        SizingMode widthSizingMode,
        SizingMode heightSizingMode,
        float ownerWidth,
        float ownerHeight,
        bool performLayout,
        uint depth,
        uint generationCount)
    {
        if (float.IsNaN(availableWidth) && widthSizingMode != SizingMode.MaxContent)
            throw new Exception("availableWidth is indefinite so widthSizingMode must be SizingMode.MaxContent");

        if (float.IsNaN(availableHeight) && heightSizingMode != SizingMode.MaxContent)
            throw new Exception("availableHeight is indefinite so heightSizingMode must be SizingMode.MaxContent");

        // Set the resolved resolution in the node's layout.
        var direction = node.ResolveDirection(ownerDirection);
        node.Layout.Direction = direction;

        var flexRowDirection = FlexDirection.Row.ResolveDirection(direction);
        var flexColumnDirection = FlexDirection.Column.ResolveDirection(direction);

        var startEdge = direction == Direction.LTR ? PhysicalEdge.Left : PhysicalEdge.Right;
        var endEdge = direction == Direction.LTR ? PhysicalEdge.Right : PhysicalEdge.Left;

        var marginRowLeading = node.ComputeInlineStartMargin(flexRowDirection, direction, ownerWidth);
        node.Layout.SetMargin(marginRowLeading, startEdge);
        var marginRowTrailing = node.ComputeInlineEndMargin(flexRowDirection, direction, ownerWidth);
        node.Layout.SetMargin(marginRowTrailing, endEdge);
        var marginColumnLeading = node.ComputeInlineStartMargin(flexColumnDirection, direction, ownerWidth);
        node.Layout.SetMargin(marginColumnLeading, PhysicalEdge.Top);
        var marginColumnTrailing = node.ComputeInlineEndMargin(flexColumnDirection, direction, ownerWidth);
        node.Layout.SetMargin(marginColumnTrailing, PhysicalEdge.Bottom);

        var marginAxisRow = marginRowLeading + marginRowTrailing;
        var marginAxisColumn = marginColumnLeading + marginColumnTrailing;

        node.Layout.SetBorder(node.ComputeInlineStartBorder(flexRowDirection, direction), startEdge);
        node.Layout.SetBorder(node.ComputeInlineEndBorder(flexRowDirection, direction), endEdge);
        node.Layout.SetBorder(node.ComputeInlineStartBorder(flexColumnDirection, direction), PhysicalEdge.Top);
        node.Layout.SetBorder(node.ComputeInlineEndBorder(flexColumnDirection, direction), PhysicalEdge.Bottom);

        node.Layout.SetPadding(node.ComputeInlineStartPadding(flexRowDirection, direction, ownerWidth), startEdge);
        node.Layout.SetPadding(node.ComputeInlineEndPadding(flexRowDirection, direction, ownerWidth), endEdge);
        node.Layout.SetPadding(node.ComputeInlineStartPadding(flexColumnDirection, direction, ownerWidth), PhysicalEdge.Top);
        node.Layout.SetPadding(node.ComputeInlineEndPadding(flexColumnDirection, direction, ownerWidth), PhysicalEdge.Bottom);

        if (node.HasMeasureFunc)
        {
            MeasureNodeWithMeasureFunc(
                node,
                direction,
                availableWidth - marginAxisRow,
                availableHeight - marginAxisColumn,
                widthSizingMode,
                heightSizingMode,
                ownerWidth,
                ownerHeight);
            return;
        }

        var childCount = node.Count;
        if (childCount == 0)
        {
            MeasureNodeWithoutChildren(
                node,
                direction,
                availableWidth - marginAxisRow,
                availableHeight - marginAxisColumn,
                widthSizingMode,
                heightSizingMode,
                ownerWidth,
                ownerHeight);
            return;
        }

        // If we're not being asked to perform a full layout we can skip the algorithm
        // if we already know the size
        if (!performLayout &&
            MeasureNodeWithFixedSize(
                node,
                direction,
                availableWidth - marginAxisRow,
                availableHeight - marginAxisColumn,
                widthSizingMode,
                heightSizingMode,
                ownerWidth,
                ownerHeight))
        {
            return;
        }

        // At this point we know we're going to perform work.
        // Reset layout flags, as they could have changed.
        node.Layout.HadOverflow = false;

        // STEP 1: CALCULATE VALUES FOR REMAINDER OF ALGORITHM
        var mainAxis = node.FlexDirection.ResolveDirection(direction);
        var crossAxis = mainAxis.ResolveCrossDirection(direction);
        var isMainAxisRow = mainAxis.IsRow();
        var isNodeFlexWrap = node.FlexWrap != Wrap.NoWrap;

        var mainAxisownerSize = isMainAxisRow ? ownerWidth : ownerHeight;
        var crossAxisownerSize = isMainAxisRow ? ownerHeight : ownerWidth;

        var paddingAndBorderAxisMain = node.PaddingAndBorderForAxis(mainAxis, direction, ownerWidth);
        var paddingAndBorderAxisCross = node.PaddingAndBorderForAxis(crossAxis, direction, ownerWidth);
        var leadingPaddingAndBorderCross = node.ComputeFlexStartPaddingAndBorder(crossAxis, direction, ownerWidth);

        var sizingModeMainDim = isMainAxisRow ? widthSizingMode : heightSizingMode;
        var sizingModeCrossDim = isMainAxisRow ? heightSizingMode : widthSizingMode;

        var paddingAndBorderAxisRow =
            isMainAxisRow ? paddingAndBorderAxisMain : paddingAndBorderAxisCross;
        var paddingAndBorderAxisColumn =
            isMainAxisRow ? paddingAndBorderAxisCross : paddingAndBorderAxisMain;

        // STEP 2: DETERMINE AVAILABLE SIZE IN MAIN AND CROSS DIRECTIONS

        var availableInnerWidth = CalculateAvailableInnerDimension(
            node,
            Dimension.Width,
            availableWidth - marginAxisRow,
            paddingAndBorderAxisRow,
            ownerWidth);
        var availableInnerHeight = CalculateAvailableInnerDimension(
            node,
            Dimension.Height,
            availableHeight - marginAxisColumn,
            paddingAndBorderAxisColumn,
            ownerHeight);

        var availableInnerMainDim = isMainAxisRow ? availableInnerWidth : availableInnerHeight;
        var availableInnerCrossDim = isMainAxisRow ? availableInnerHeight : availableInnerWidth;

        // STEP 3: DETERMINE FLEX BASIS FOR EACH ITEM

        // Computed basis + margins + gap
        float totalMainDim = 0;
        totalMainDim += ComputeFlexBasisForChildren(
            node,
            availableInnerWidth,
            availableInnerHeight,
            widthSizingMode,
            heightSizingMode,
            direction,
            mainAxis,
            performLayout,
            depth,
            generationCount);

        if (childCount > 1)
        {
            totalMainDim += node.ComputeGapForAxis(mainAxis, availableInnerMainDim) * (childCount - 1);
        }

        var mainAxisOverflows = (sizingModeMainDim != SizingMode.MaxContent) && totalMainDim > availableInnerMainDim;

        if (isNodeFlexWrap && mainAxisOverflows && sizingModeMainDim == SizingMode.FitContent)
        {
            sizingModeMainDim = SizingMode.StretchFit;
        }

        // STEP 4: COLLECT FLEX ITEMS INTO FLEX LINES

        // Indexes of children that represent the first and last items in the line.
        var startOfLineIndex = 0;
        var endOfLineIndex = 0;

        // Number of lines.
        var lineCount = 0;

        // Accumulated cross dimensions of all lines so far.
        float totalLineCrossDim = 0;

        var crossAxisGap = node.ComputeGapForAxis(crossAxis, availableInnerCrossDim);

        // Max main dimension of all the lines.
        float maxLineMainDim = 0;
        for (; endOfLineIndex < childCount; lineCount++, startOfLineIndex = endOfLineIndex)
        {
            var flexLine = node.CalculateFlexLine(
                ownerDirection,
                mainAxisownerSize,
                availableInnerWidth,
                availableInnerMainDim,
                startOfLineIndex,
                lineCount);

            endOfLineIndex = flexLine.EndOfLineIndex;

            // If we don't need to measure the cross axis, we can skip the entire flex
            // step.
            var canSkipFlex =
                !performLayout && sizingModeCrossDim == SizingMode.StretchFit;

            // STEP 5: RESOLVING FLEXIBLE LENGTHS ON MAIN AXIS
            // Calculate the remaining available space that needs to be allocated. If
            // the main dimension size isn't known, it is computed based on the line
            // length, so there's no more space left to distribute.

            var sizeBasedOnContent = false;
            // If we don't measure with exact main dimension we want to ensure we don't
            // violate min and max
            if (sizingModeMainDim != SizingMode.StretchFit)
            {
                var minInnerWidth = node.GetMinDimension(Dimension.Width).Resolve(ownerWidth) - paddingAndBorderAxisRow;
                var maxInnerWidth = node.GetMaxDimension(Dimension.Width).Resolve(ownerWidth) - paddingAndBorderAxisRow;
                var minInnerHeight = node.GetMinDimension(Dimension.Height).Resolve(ownerHeight) - paddingAndBorderAxisColumn;
                var maxInnerHeight = node.GetMaxDimension(Dimension.Height).Resolve(ownerHeight) - paddingAndBorderAxisColumn;

                var minInnerMainDim = isMainAxisRow ? minInnerWidth : minInnerHeight;
                var maxInnerMainDim = isMainAxisRow ? maxInnerWidth : maxInnerHeight;

                if (!float.IsNaN(minInnerMainDim) && flexLine.SizeConsumed < minInnerMainDim)
                {
                    availableInnerMainDim = minInnerMainDim;
                }
                else if (!float.IsNaN(maxInnerMainDim) && flexLine.SizeConsumed > maxInnerMainDim)
                {
                    availableInnerMainDim = maxInnerMainDim;
                }
                else
                {
                    var useLegacyStretchBehaviour = node.Config.Errata.HasFlag(Errata.StretchFlexBasis);

                    if (!useLegacyStretchBehaviour && (
                        (!float.IsNaN(flexLine.Layout.TotalFlexGrowFactors) && flexLine.Layout.TotalFlexGrowFactors == 0) ||
                        (!float.IsNaN(node.ResolveFlexGrow()) && node.ResolveFlexGrow() == 0)))
                    {
                        // If we don't have any children to flex or we can't flex the node
                        // itself, space we've used is all space we need. Root node also
                        // should be shrunk to minimum
                        availableInnerMainDim = flexLine.SizeConsumed;
                    }

                    sizeBasedOnContent = !useLegacyStretchBehaviour;
                }
            }

            if (!sizeBasedOnContent && !float.IsNaN(availableInnerMainDim))
            {
                flexLine.Layout.RemainingFreeSpace = availableInnerMainDim - flexLine.SizeConsumed;
            }
            else if (flexLine.SizeConsumed < 0)
            {
                // availableInnerMainDim is indefinite which means the node is being sized
                // based on its content. sizeConsumed is negative which means
                // the node will allocate 0 points for its content. Consequently,
                // remainingFreeSpace is 0 - sizeConsumed.
                flexLine.Layout.RemainingFreeSpace = -flexLine.SizeConsumed;
            }

            if (!canSkipFlex)
            {
                ResolveFlexibleLength(
                    node,
                    ref flexLine,
                    mainAxis,
                    crossAxis,
                    direction,
                    mainAxisownerSize,
                    availableInnerMainDim,
                    availableInnerCrossDim,
                    availableInnerWidth,
                    availableInnerHeight,
                    mainAxisOverflows,
                    sizingModeCrossDim,
                    performLayout,
                    depth,
                    generationCount);
            }

            node.Layout.HadOverflow |= flexLine.Layout.RemainingFreeSpace < 0;

            // STEP 6: MAIN-AXIS JUSTIFICATION & CROSS-AXIS SIZE DETERMINATION

            // At this point, all the children have their dimensions set in the main
            // axis. Their dimensions are also set in the cross axis with the exception
            // of items that are aligned "stretch". We need to compute these stretch
            // values and set the final positions.

            JustifyMainAxis(
                node,
                ref flexLine,
                startOfLineIndex,
                mainAxis,
                crossAxis,
                direction,
                sizingModeMainDim,
                sizingModeCrossDim,
                mainAxisownerSize,
                ownerWidth,
                availableInnerMainDim,
                availableInnerCrossDim,
                availableInnerWidth,
                performLayout);

            var containerCrossAxis = availableInnerCrossDim;
            if (sizingModeCrossDim == SizingMode.MaxContent ||
                sizingModeCrossDim == SizingMode.FitContent)
            {
                // Compute the cross axis from the max cross dimension of the children.
                containerCrossAxis =
                    BoundAxis(
                        node,
                        crossAxis,
                        direction,
                        flexLine.Layout.CrossDim + paddingAndBorderAxisCross,
                        crossAxisownerSize,
                        ownerWidth) -
                    paddingAndBorderAxisCross;
            }

            // If there's no flex wrap, the cross dimension is defined by the container.
            if (!isNodeFlexWrap && sizingModeCrossDim == SizingMode.StretchFit)
            {
                flexLine.Layout.CrossDim = availableInnerCrossDim;
            }

            // As-per https://www.w3.org/TR/css-flexbox-1/#cross-sizing, the
            // cross-size of the line within a single-line container should be bound to
            // min/max constraints before alignment within the line. In a multi-line
            // container, affecting alignment between the lines.
            if (!isNodeFlexWrap)
            {
                flexLine.Layout.CrossDim =
                    BoundAxis(
                        node,
                        crossAxis,
                        direction,
                        flexLine.Layout.CrossDim + paddingAndBorderAxisCross,
                        crossAxisownerSize,
                        ownerWidth) -
                    paddingAndBorderAxisCross;
            }

            // STEP 7: CROSS-AXIS ALIGNMENT
            // We can skip child alignment if we're just measuring the container.
            if (performLayout)
            {
                for (var i = startOfLineIndex; i < endOfLineIndex; i++)
                {
                    var child = node[i];
                    if (child.Display == Display.None)
                    {
                        continue;
                    }
                    if (child.PositionType == PositionType.Absolute)
                    {
                        // If the child is absolutely positioned and has a
                        // top/left/bottom/right set, override all the previously computed
                        // positions to set it correctly.
                        var isChildLeadingPosDefined =
                            child.IsFlexStartPositionDefined(crossAxis, direction) &&
                            !child.IsFlexStartPositionAuto(crossAxis, direction);
                        if (isChildLeadingPosDefined)
                        {
                            child.Layout.SetPosition(
                                child.ComputeFlexStartPosition(crossAxis, direction, availableInnerCrossDim) +
                                node.ComputeFlexStartBorder(crossAxis, direction) +
                                child.ComputeFlexStartMargin(crossAxis, direction, availableInnerWidth),
                                crossAxis.FlexStartEdge());
                        }
                        // If leading position is not defined or calculations result in Nan,
                        // default to border + margin
                        if (!isChildLeadingPosDefined || float.IsNaN(child.Layout.GetPosition(crossAxis.FlexStartEdge())))
                        {
                            child.Layout.SetPosition(
                                node.ComputeFlexStartBorder(crossAxis, direction) +
                                child.ComputeFlexStartMargin(crossAxis, direction, availableInnerWidth),
                                crossAxis.FlexStartEdge());
                        }
                    }
                    else
                    {
                        var leadingCrossDim = leadingPaddingAndBorderCross;

                        // For a relative children, we're either using alignItems (owner) or
                        // alignSelf (child) in order to determine the position in the cross
                        // axis
                        var alignItem = ResolveChildAlignment(node, child);

                        // If the child uses align stretch, we need to lay it out one more
                        // time, this time forcing the cross-axis size to be the computed
                        // cross size for the current line.
                        if (alignItem == Align.Stretch &&
                            !child.FlexStartMarginIsAuto(crossAxis, direction) &&
                            !child.FlexEndMarginIsAuto(crossAxis, direction))
                        {
                            // If the child defines a definite size for its cross axis, there's
                            // no need to stretch.
                            if (!child.HasDefiniteLength(crossAxis.Dimension(), availableInnerCrossDim))
                            {
                                var childMainSize = child.Layout.GetMeasuredDimension(mainAxis.Dimension());
                                var childCrossSize = child.AspectRatio.IsDefined
                                    ? child.ComputeMarginForAxis(crossAxis, availableInnerWidth) +
                                      (isMainAxisRow ? childMainSize / child.AspectRatio.Value : childMainSize * child.AspectRatio.Value)
                                    : flexLine.Layout.CrossDim;

                                childMainSize += child.ComputeMarginForAxis(mainAxis, availableInnerWidth);

                                var childMainSizingMode = SizingMode.StretchFit;
                                var childCrossSizingMode = SizingMode.StretchFit;
                                ConstrainMaxSizeForMode(
                                    child,
                                    mainAxis,
                                    availableInnerMainDim,
                                    availableInnerWidth,
                                    ref childMainSizingMode,
                                    ref childMainSize);
                                ConstrainMaxSizeForMode(
                                    child,
                                    crossAxis,
                                    availableInnerCrossDim,
                                    availableInnerWidth,
                                    ref childCrossSizingMode,
                                    ref childCrossSize);

                                var childWidth = isMainAxisRow ? childMainSize : childCrossSize;
                                var childHeight = !isMainAxisRow ? childMainSize : childCrossSize;

                                var alignContent = node.AlignContent;
                                var crossAxisDoesNotGrow = alignContent != Align.Stretch && isNodeFlexWrap;
                                var childWidthSizingMode =
                                    float.IsNaN(childWidth) ||
                                        (!isMainAxisRow && crossAxisDoesNotGrow)
                                    ? SizingMode.MaxContent
                                    : SizingMode.StretchFit;
                                var childHeightSizingMode =
                                    float.IsNaN(childHeight) ||
                                        (isMainAxisRow && crossAxisDoesNotGrow)
                                    ? SizingMode.MaxContent
                                    : SizingMode.StretchFit;

                                CalculateLayoutInternal(
                                    child,
                                    childWidth,
                                    childHeight,
                                    direction,
                                    childWidthSizingMode,
                                    childHeightSizingMode,
                                    availableInnerWidth,
                                    availableInnerHeight,
                                    true,
                                    depth,
                                    generationCount);
                            }
                        }
                        else
                        {
                            var remainingCrossDim = containerCrossAxis - child.DimensionWithMargin(crossAxis, availableInnerWidth);

                            if (child.FlexStartMarginIsAuto(crossAxis, direction) &&
                                child.FlexEndMarginIsAuto(crossAxis, direction))
                            {
                                leadingCrossDim += MathUtils.MaxOrDefined(0.0f, remainingCrossDim / 2);
                            }
                            else if (child.FlexEndMarginIsAuto(crossAxis, direction))
                            {
                                // No-Op
                            }
                            else if (child.FlexStartMarginIsAuto(crossAxis, direction))
                            {
                                leadingCrossDim += MathUtils.MaxOrDefined(0.0f, remainingCrossDim);
                            }
                            else if (alignItem == Align.FlexStart)
                            {
                                // No-Op
                            }
                            else if (alignItem == Align.Center)
                            {
                                leadingCrossDim += remainingCrossDim / 2;
                            }
                            else
                            {
                                leadingCrossDim += remainingCrossDim;
                            }
                        }
                        // And we apply the position
                        child.Layout.SetPosition(child.Layout.GetPosition(crossAxis.FlexStartEdge()) + totalLineCrossDim + leadingCrossDim, crossAxis.FlexStartEdge());
                    }
                }
            }

            var appliedCrossGap = lineCount != 0 ? crossAxisGap : 0.0f;
            totalLineCrossDim += flexLine.Layout.CrossDim + appliedCrossGap;
            maxLineMainDim = MathUtils.MaxOrDefined(maxLineMainDim, flexLine.Layout.MainDim);
        }

        // STEP 8: MULTI-LINE CONTENT ALIGNMENT
        // currentLead stores the size of the cross dim
        if (performLayout && (isNodeFlexWrap || node.IsBaselineLayout()))
        {
            var leadPerLine = 0f;
            var currentLead = leadingPaddingAndBorderCross;

            var unclampedCrossDim = sizingModeCrossDim == SizingMode.StretchFit
                ? availableInnerCrossDim + paddingAndBorderAxisCross
                : node.HasDefiniteLength(crossAxis.Dimension(), crossAxisownerSize)
                ? node.GetResolvedDimension(crossAxis.Dimension()).Resolve(crossAxisownerSize)
                : totalLineCrossDim + paddingAndBorderAxisCross;

            var innerCrossDim = BoundAxis(
                node,
                crossAxis,
                direction,
                unclampedCrossDim,
                ownerHeight,
                ownerWidth) - paddingAndBorderAxisCross;

            var remainingAlignContentDim = innerCrossDim - totalLineCrossDim;

            var alignContent = remainingAlignContentDim >= 0
                ? node.AlignContent
                : FallbackAlignment(node.AlignContent);

            switch (alignContent)
            {
                case Align.FlexEnd:
                    currentLead += remainingAlignContentDim;
                    break;
                case Align.Center:
                    currentLead += remainingAlignContentDim / 2;
                    break;
                case Align.Stretch:
                    leadPerLine = remainingAlignContentDim / lineCount;
                    break;
                case Align.SpaceAround:
                    currentLead +=
                        remainingAlignContentDim / (2 * (float)lineCount);
                    leadPerLine = remainingAlignContentDim / lineCount;
                    break;
                case Align.SpaceEvenly:
                    currentLead +=
                        remainingAlignContentDim / (lineCount + 1);
                    leadPerLine =
                        remainingAlignContentDim / (lineCount + 1);
                    break;
                case Align.SpaceBetween:
                    if (lineCount > 1)
                    {
                        leadPerLine =
                            remainingAlignContentDim / (lineCount + 1);
                    }
                    break;
                case Align.Auto:
                case Align.FlexStart:
                case Align.Baseline:
                    break;
            }

            var endIndex = 0;
            for (var i = 0; i < lineCount; i++)
            {
                var startIndex = endIndex;
                var ii = startIndex;

                // compute the line's height and find the endIndex
                float lineHeight = 0;
                float maxAscentForCurrentLine = 0;
                float maxDescentForCurrentLine = 0;
                for (; ii < childCount; ii++)
                {
                    var child = node[ii];
                    if (child.Display == Display.None)
                    {
                        continue;
                    }
                    if (child.PositionType != PositionType.Absolute)
                    {
                        if (child._lineIndex != i)
                        {
                            break;
                        }
                        if (child.IsLayoutDimensionDefined(crossAxis))
                        {
                            lineHeight = MathUtils.MaxOrDefined(lineHeight, child.Layout.GetMeasuredDimension(crossAxis.Dimension()) + child.ComputeMarginForAxis(crossAxis, availableInnerWidth));
                        }
                        if (ResolveChildAlignment(node, child) == Align.Baseline)
                        {
                            var ascent = child.CalculateBaseline() + child.ComputeFlexStartMargin(FlexDirection.Column, direction, availableInnerWidth);
                            var descent = child.Layout.GetMeasuredDimension(Dimension.Height) + child.ComputeMarginForAxis(FlexDirection.Column, availableInnerWidth) - ascent;
                            maxAscentForCurrentLine = MathUtils.MaxOrDefined(maxAscentForCurrentLine, ascent);
                            maxDescentForCurrentLine = MathUtils.MaxOrDefined(maxDescentForCurrentLine, descent);
                            lineHeight = MathUtils.MaxOrDefined(lineHeight, maxAscentForCurrentLine + maxDescentForCurrentLine);
                        }
                    }
                }
                endIndex = ii;
                currentLead += i != 0 ? crossAxisGap : 0;

                for (ii = startIndex; ii < endIndex; ii++)
                {
                    var child = node[ii];
                    if (child.Display == Display.None)
                    {
                        continue;
                    }
                    if (child.PositionType != PositionType.Absolute)
                    {
                        switch (ResolveChildAlignment(node, child))
                        {
                            case Align.FlexStart:
                                child.Layout.SetPosition(
                                    currentLead + child.ComputeFlexStartPosition(crossAxis, direction, availableInnerWidth),
                                    crossAxis.FlexStartEdge());
                                break;

                            case Align.FlexEnd:
                                child.Layout.SetPosition(
                                    currentLead +
                                    lineHeight -
                                    child.ComputeFlexEndMargin(crossAxis, direction, availableInnerWidth) -
                                    child.Layout.GetMeasuredDimension(crossAxis.Dimension()),
                                    crossAxis.FlexStartEdge());
                                break;

                            case Align.Center:
                                {
                                    var childHeight = child.Layout.GetMeasuredDimension(crossAxis.Dimension());
                                    child.Layout.SetPosition(currentLead + (lineHeight - childHeight) / 2, crossAxis.FlexStartEdge());
                                    break;
                                }

                            case Align.Stretch:
                                {
                                    child.Layout.SetPosition(
                                        currentLead + child.ComputeFlexStartMargin(crossAxis, direction, availableInnerWidth),
                                        crossAxis.FlexStartEdge());

                                    // Remeasure child with the line height as it as been only
                                    // measured with the owners height yet.
                                    if (!child.HasDefiniteLength(crossAxis.Dimension(), availableInnerCrossDim))
                                    {
                                        var childWidth = isMainAxisRow
                                            ? (child.Layout.GetMeasuredDimension(Dimension.Width) +
                                               child.ComputeMarginForAxis(mainAxis, availableInnerWidth))
                                            : leadPerLine + lineHeight;

                                        var childHeight = !isMainAxisRow
                                            ? (child.Layout.GetMeasuredDimension(Dimension.Height) +
                                               child.ComputeMarginForAxis(crossAxis, availableInnerWidth))
                                            : leadPerLine + lineHeight;

                                        if (!(childWidth.IsApproximately(child.Layout.GetMeasuredDimension(Dimension.Width)) && childHeight.IsApproximately(child.Layout.GetMeasuredDimension(Dimension.Height))))
                                        {
                                            CalculateLayoutInternal(
                                                child,
                                                childWidth,
                                                childHeight,
                                                direction,
                                                SizingMode.StretchFit,
                                                SizingMode.StretchFit,
                                                availableInnerWidth,
                                                availableInnerHeight,
                                                true,
                                                depth,
                                                generationCount);
                                        }
                                    }
                                    break;
                                }
                            case Align.Baseline:
                                child.Layout.SetPosition(
                                    currentLead + maxAscentForCurrentLine - child.CalculateBaseline() + child.ComputeFlexStartPosition(FlexDirection.Column, direction, availableInnerCrossDim),
                                    PhysicalEdge.Top);
                                break;
                            case Align.Auto:
                            case Align.SpaceBetween:
                            case Align.SpaceAround:
                            case Align.SpaceEvenly:
                                break;
                        }
                    }
                }

                currentLead = currentLead + leadPerLine + lineHeight;
            }
        }

        // STEP 9: COMPUTING FINAL DIMENSIONS

        node.Layout.SetMeasuredDimension(
            BoundAxis(
                node,
                FlexDirection.Row,
                direction,
                availableWidth - marginAxisRow,
                ownerWidth,
                ownerWidth),
            Dimension.Width);

        node.Layout.SetMeasuredDimension(
            BoundAxis(
                node,
                FlexDirection.Column,
                direction,
                availableHeight - marginAxisColumn,
                ownerHeight,
                ownerWidth),
            Dimension.Height);

        // If the user didn't specify a width or height for the node, set the
        // dimensions based on the children.
        if (sizingModeMainDim == SizingMode.MaxContent || (node.Overflow != Overflow.Scroll && sizingModeMainDim == SizingMode.FitContent))
        {
            // Clamp the size to the min/max size, if specified, and make sure it
            // doesn't go below the padding and border amount.
            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    mainAxis,
                    direction,
                    maxLineMainDim,
                    mainAxisownerSize,
                    ownerWidth),
                mainAxis.Dimension());

        }
        else if (sizingModeMainDim == SizingMode.FitContent && node.Overflow == Overflow.Scroll)
        {
            node.Layout.SetMeasuredDimension(
                MathUtils.MaxOrDefined(
                    MathUtils.MinOrDefined(
                        availableInnerMainDim + paddingAndBorderAxisMain,
                        BoundAxisWithinMinAndMax(node, mainAxis, maxLineMainDim, mainAxisownerSize)),
                    paddingAndBorderAxisMain),
                mainAxis.Dimension());
        }

        if (sizingModeCrossDim == SizingMode.MaxContent || (node.Overflow != Overflow.Scroll && sizingModeCrossDim == SizingMode.FitContent))
        {
            // Clamp the size to the min/max size, if specified, and make sure it
            // doesn't go below the padding and border amount.
            node.Layout.SetMeasuredDimension(
                BoundAxis(
                    node,
                    crossAxis,
                    direction,
                    totalLineCrossDim + paddingAndBorderAxisCross,
                    crossAxisownerSize,
                    ownerWidth),
                crossAxis.Dimension());

        }
        else if (sizingModeCrossDim == SizingMode.FitContent && node.Overflow == Overflow.Scroll)
        {
            node.Layout.SetMeasuredDimension(
                MathUtils.MaxOrDefined(
                    MathUtils.MinOrDefined(
                        availableInnerCrossDim + paddingAndBorderAxisCross,
                        BoundAxisWithinMinAndMax(node, crossAxis, totalLineCrossDim + paddingAndBorderAxisCross, crossAxisownerSize)),
                    paddingAndBorderAxisCross),
                crossAxis.Dimension());
        }

        // As we only wrapped in normal direction yet, we need to reverse the
        // positions on wrap-reverse.
        if (performLayout && node.FlexWrap == Wrap.WrapReverse)
        {
            for (var i = 0; i < childCount; i++)
            {
                var child = node[i];
                if (child.PositionType != PositionType.Absolute)
                {
                    child.Layout.SetPosition(
                        node.Layout.GetMeasuredDimension(crossAxis.Dimension()) - child.Layout.GetPosition(crossAxis.FlexStartEdge()) - child.Layout.GetMeasuredDimension(crossAxis.Dimension()),
                        crossAxis.FlexStartEdge());
                }
            }
        }

        if (performLayout)
        {
            // STEP 10: SETTING TRAILING POSITIONS FOR CHILDREN
            var needsMainTrailingPos = mainAxis.NeedsTrailingPosition();
            var needsCrossTrailingPos = crossAxis.NeedsTrailingPosition();

            if (needsMainTrailingPos || needsCrossTrailingPos)
            {
                for (var i = 0; i < childCount; i++)
                {
                    var child = node[i];
                    // Absolute children will be handled by their containing block since we
                    // cannot guarantee that their positions are set when their parents are
                    // done with layout.
                    if (child.Display == Display.None ||
                        child.PositionType == PositionType.Absolute)
                    {
                        continue;
                    }
                    if (needsMainTrailingPos)
                    {
                        SetChildTrailingPosition(node, child, mainAxis);
                    }

                    if (needsCrossTrailingPos)
                    {
                        SetChildTrailingPosition(node, child, crossAxis);
                    }
                }
            }

            // STEP 11: SIZING AND POSITIONING ABSOLUTE CHILDREN
            // Let the containing block layout its absolute descendants.
            if (node.PositionType != PositionType.Static || node.AlwaysFormsContainingBlock || depth == 1)
            {
                LayoutAbsoluteDescendants(
                    node,
                    node,
                    isMainAxisRow ? sizingModeMainDim : sizingModeCrossDim,
                    direction,
                    depth,
                    generationCount,
                    0.0f,
                    0.0f,
                    availableInnerWidth,
                    availableInnerHeight);
            }
        }
    }

    private Vector2 MeasureWrapper(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        var size = Measure(width, widthMode, height, heightMode);

        if (float.IsNaN(size.X) || size.X < 0 || float.IsNaN(size.Y) || size.Y < 0)
            throw new Exception($"Measure function returned invalid dimensions to Yoga: {size}");

        return new Vector2(MathUtils.MaxOrDefined(0f, size.X), MathUtils.MaxOrDefined(0f, size.Y));
    }
}
