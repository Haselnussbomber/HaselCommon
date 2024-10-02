using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

public partial class Node
{
    private static void SetFlexStartLayoutPosition(Node parent, Node child, Direction direction, FlexDirection axis, float containingBlockWidth)
    {
        child._layout.SetPosition(
              child.ComputeFlexStartMargin(axis, direction, containingBlockWidth) +
                parent._layout.GetBorder(axis.FlexStartEdge()) +
                parent._layout.GetPadding(axis.FlexStartEdge()),
            axis.FlexStartEdge());
    }

    private static void SetFlexEndLayoutPosition(Node parent, Node child, Direction direction, FlexDirection axis, float containingBlockWidth)
    {
        child._layout.SetPosition(
            GetPositionOfOppositeEdge(
                parent._layout.GetBorder(axis.FlexEndEdge()) +
                    parent._layout.GetPadding(axis.FlexEndEdge()) +
                    child.ComputeFlexEndMargin(axis, direction, containingBlockWidth),
                axis,
                parent,
                child),
            axis.FlexStartEdge());
    }

    private static void SetCenterLayoutPosition(Node parent, Node child, Direction direction, FlexDirection axis, float containingBlockWidth)
    {
        var parentContentBoxSize =
            parent._layout.GetMeasuredDimension(axis.Dimension()) -
            parent._layout.GetBorder(axis.FlexStartEdge()) -
            parent._layout.GetBorder(axis.FlexEndEdge()) -
            parent._layout.GetPadding(axis.FlexStartEdge()) -
            parent._layout.GetPadding(axis.FlexEndEdge());

        var childOuterSize =
            child._layout.GetMeasuredDimension(axis.Dimension()) +
            child.ComputeMarginForAxis(axis, containingBlockWidth);

        child._layout.SetPosition(
            (parentContentBoxSize - childOuterSize) / 2.0f +
              parent._layout.GetBorder(axis.FlexStartEdge()) +
              parent._layout.GetPadding(axis.FlexStartEdge()) +
              child.ComputeFlexStartMargin(
                  axis, direction, containingBlockWidth),
          axis.FlexStartEdge());
    }

    private static void JustifyAbsoluteChild(Node parent, Node child, Direction direction, FlexDirection mainAxis, float containingBlockWidth)
    {
        switch (parent.JustifyContent)
        {
            case Justify.FlexStart:
            case Justify.SpaceBetween:
                SetFlexStartLayoutPosition(parent, child, direction, mainAxis, containingBlockWidth);
                break;
            case Justify.FlexEnd:
                SetFlexEndLayoutPosition(parent, child, direction, mainAxis, containingBlockWidth);
                break;
            case Justify.Center:
            case Justify.SpaceAround:
            case Justify.SpaceEvenly:
                SetCenterLayoutPosition(parent, child, direction, mainAxis, containingBlockWidth);
                break;
        }
    }

    private static void AlignAbsoluteChild(Node parent, Node child, Direction direction, FlexDirection crossAxis, float containingBlockWidth)
    {
        var itemAlign = ResolveChildAlignment(parent, child);
        var parentWrap = parent.FlexWrap;

        if (parentWrap == Wrap.WrapReverse)
        {
            if (itemAlign == Align.FlexEnd)
            {
                itemAlign = Align.FlexStart;
            }
            else if (itemAlign != Align.Center)
            {
                itemAlign = Align.FlexEnd;
            }
        }

        switch (itemAlign)
        {
            case Align.Auto:
            case Align.FlexStart:
            case Align.Baseline:
            case Align.SpaceAround:
            case Align.SpaceBetween:
            case Align.Stretch:
            case Align.SpaceEvenly:
                SetFlexStartLayoutPosition(parent, child, direction, crossAxis, containingBlockWidth);
                break;
            case Align.FlexEnd:
                SetFlexEndLayoutPosition(parent, child, direction, crossAxis, containingBlockWidth);
                break;
            case Align.Center:
                SetCenterLayoutPosition(parent, child, direction, crossAxis, containingBlockWidth);
                break;
        }
    }

    // To ensure no breaking changes, we preserve the legacy way of positioning
    // absolute children and determine if we should use it using an errata.
    private static void PositionAbsoluteChildLegacy(
        Node containingNode,
        Node parent,
        Node child,
        Direction direction,
        FlexDirection axis,
        bool isMainAxis,
        float containingBlockWidth,
        float containingBlockHeight)
    {
        var isAxisRow = axis.IsRow();
        var shouldCenter = isMainAxis
            ? parent.JustifyContent == Justify.Center
            : ResolveChildAlignment(parent, child) == Align.Center;
        var shouldFlexEnd = isMainAxis
            ? parent.JustifyContent == Justify.FlexEnd
            : ((ResolveChildAlignment(parent, child) == Align.FlexEnd) ^
               (parent.FlexWrap == Wrap.WrapReverse));

        if (child.IsFlexEndPositionDefined(axis, direction) &&
            (!child.IsFlexStartPositionDefined(axis, direction) ||
             child.IsFlexStartPositionAuto(axis, direction)))
        {
            child._layout.SetPosition(
                containingNode._layout.GetMeasuredDimension(axis.Dimension()) -
                    child._layout.GetMeasuredDimension(axis.Dimension()) -
                    containingNode.ComputeFlexEndBorder(axis, direction) -
                    child.ComputeFlexEndMargin(
                        axis,
                        direction,
                        isAxisRow ? containingBlockWidth : containingBlockHeight) -
                    child.ComputeFlexEndPosition(
                        axis,
                        direction,
                        isAxisRow ? containingBlockWidth : containingBlockHeight),
                axis.FlexStartEdge());
        }
        else if (
            (!child.IsFlexStartPositionDefined(axis, direction) ||
             child.IsFlexStartPositionAuto(axis, direction)) &&
            shouldCenter)
        {
            child._layout.SetPosition(
                (parent._layout.GetMeasuredDimension(axis.Dimension()) -
                 child._layout.GetMeasuredDimension(axis.Dimension())) /
                    2.0f,
                axis.FlexStartEdge());
        }
        else if (
            (!child.IsFlexStartPositionDefined(axis, direction) ||
             child.IsFlexStartPositionAuto(axis, direction)) &&
            shouldFlexEnd)
        {
            child._layout.SetPosition(
                parent._layout.GetMeasuredDimension(axis.Dimension()) -
                 child._layout.GetMeasuredDimension(axis.Dimension()),
                axis.FlexStartEdge());
        }
    }

    /*
     * Absolutely positioned nodes do not participate in flex layout and thus their
     * positions can be determined independently from the rest of their siblings.
     * For each axis there are essentially two cases:
     *
     * 1) The node has insets defined. In this case we can just use these to
     *    determine the position of the node.
     * 2) The node does not have insets defined. In this case we look at the style
     *    of the parent to position the node. Things like justify content and
     *    align content will move absolute children around. If none of these
     *    special properties are defined, the child is positioned at the start
     *    (defined by flex direction) of the leading flex line.
     *
     * This function does that positioning for the given axis. The spec has more
     * information on this topic: https://www.w3.org/TR/css-flexbox-1/#abspos-items
     */
    private static void PositionAbsoluteChildImpl(
        Node containingNode,
        Node parent,
        Node child,
        Direction direction,
        FlexDirection axis,
        bool isMainAxis,
        float containingBlockWidth,
        float containingBlockHeight)
    {
        var isAxisRow = axis.IsRow();
        var containingBlockSize =
            isAxisRow ? containingBlockWidth : containingBlockHeight;

        // The inline-start position takes priority over the end position in the case
        // that they are both set and the node has a fixed width. Thus we only have 2
        // cases here: if inline-start is defined and if inline-end is defined.
        //
        // Despite checking inline-start to honor prioritization of insets, we write
        // to the flex-start edge because this algorithm works by positioning on the
        // flex-start edge and then filling in the flex-end direction at the end if
        // necessary.
        if (child.IsInlineStartPositionDefined(axis, direction) &&
            !child.IsInlineStartPositionAuto(axis, direction))
        {
            var positionRelativeToInlineStart =
                child.ComputeInlineStartPosition(
                    axis, direction, containingBlockSize) +
                containingNode.ComputeInlineStartBorder(axis, direction) +
                child.ComputeInlineStartMargin(
                    axis, direction, containingBlockSize);
            var positionRelativeToFlexStart =
                axis.InlineStartEdge(direction) != axis.FlexStartEdge()
                ? GetPositionOfOppositeEdge(positionRelativeToInlineStart, axis, containingNode, child)
                : positionRelativeToInlineStart;

            child._layout.SetPosition(positionRelativeToFlexStart, axis.FlexStartEdge());
        }
        else if (
            child.IsInlineEndPositionDefined(axis, direction) &&
            !child.IsInlineEndPositionAuto(axis, direction))
        {
            var positionRelativeToInlineStart =
                containingNode._layout.GetMeasuredDimension(axis.Dimension()) -
                child._layout.GetMeasuredDimension(axis.Dimension()) -
                containingNode.ComputeInlineEndBorder(axis, direction) -
                child.ComputeInlineEndMargin(
                    axis, direction, containingBlockSize) -
                child.ComputeInlineEndPosition(
                    axis, direction, containingBlockSize);
            var positionRelativeToFlexStart =
                axis.InlineStartEdge(direction) != axis.FlexStartEdge()
                ? GetPositionOfOppositeEdge(positionRelativeToInlineStart, axis, containingNode, child)
                : positionRelativeToInlineStart;

            child._layout.SetPosition(positionRelativeToFlexStart, axis.FlexStartEdge());
        }
        else
        {
            if (isMainAxis)
                JustifyAbsoluteChild(parent, child, direction, axis, containingBlockWidth);
            else
                AlignAbsoluteChild(parent, child, direction, axis, containingBlockWidth);
        }
    }

    private static void PositionAbsoluteChild(
        Node containingNode,
        Node parent,
        Node child,
        Direction direction,
        FlexDirection axis,
        bool isMainAxis,
        float containingBlockWidth,
        float containingBlockHeight)
    {
        if (child.Config.Errata.HasFlag(Errata.AbsolutePositioningIncorrect))
        {
            PositionAbsoluteChildLegacy(
                containingNode,
                parent,
                child,
                direction,
                axis,
                isMainAxis,
                containingBlockWidth,
                containingBlockHeight);
        }
        else
        {
            PositionAbsoluteChildImpl(
                containingNode,
                parent,
                child,
                direction,
                axis,
                isMainAxis,
                containingBlockWidth,
                containingBlockHeight);
        }
    }

    private static void LayoutAbsoluteChild(
        Node containingNode,
        Node node,
        Node child,
        float containingBlockWidth,
        float containingBlockHeight,
        SizingMode widthMode,
        Direction direction,
        uint depth,
        uint generationCount)
    {
        var mainAxis =
            node.FlexDirection.ResolveDirection(direction);
        var crossAxis = mainAxis.ResolveCrossDirection(direction);
        var isMainAxisRow = mainAxis.IsRow();

        var childWidth = float.NaN;
        var childHeight = float.NaN;
        SizingMode childWidthSizingMode;
        SizingMode childHeightSizingMode;

        var marginRow = child.ComputeMarginForAxis(
            FlexDirection.Row, containingBlockWidth);
        var marginColumn = child.ComputeMarginForAxis(
            FlexDirection.Column, containingBlockWidth);

        if (child.HasDefiniteLength(Dimension.Width, containingBlockWidth))
        {
            childWidth = child.GetResolvedDimension(Dimension.Width).Resolve(containingBlockWidth) + marginRow;
        }
        else
        {
            // If the child doesn't have a specified width, compute the width based on
            // the left/right offsets if they're defined.
            if (child.IsFlexStartPositionDefined(FlexDirection.Row, direction) &&
                child.IsFlexEndPositionDefined(FlexDirection.Row, direction) &&
                !child.IsFlexStartPositionAuto(FlexDirection.Row, direction) &&
                !child.IsFlexEndPositionAuto(FlexDirection.Row, direction))
            {
                childWidth =
                    containingNode._layout.GetMeasuredDimension(Dimension.Width) -
                    (containingNode.ComputeFlexStartBorder(FlexDirection.Row, direction) +
                     containingNode.ComputeFlexEndBorder(FlexDirection.Row, direction)) -
                    (child.ComputeFlexStartPosition(FlexDirection.Row, direction, containingBlockWidth) +
                     child.ComputeFlexEndPosition(FlexDirection.Row, direction, containingBlockWidth));
                childWidth = BoundAxis(
                    child,
                    FlexDirection.Row,
                    direction,
                    childWidth,
                    containingBlockWidth,
                    containingBlockWidth);
            }
        }

        if (child.HasDefiniteLength(Dimension.Height, containingBlockHeight))
        {
            childHeight = child.GetResolvedDimension(Dimension.Height).Resolve(containingBlockHeight) + marginColumn;
        }
        else
        {
            // If the child doesn't have a specified height, compute the height based
            // on the top/bottom offsets if they're defined.
            if (child.IsFlexStartPositionDefined(FlexDirection.Column, direction) &&
                child.IsFlexEndPositionDefined(FlexDirection.Column, direction) &&
                !child.IsFlexStartPositionAuto(FlexDirection.Column, direction) &&
                !child.IsFlexEndPositionAuto(FlexDirection.Column, direction))
            {
                childHeight =
                    containingNode._layout.GetMeasuredDimension(Dimension.Height) -
                    (containingNode.ComputeFlexStartBorder(FlexDirection.Column, direction) +
                     containingNode.ComputeFlexEndBorder(FlexDirection.Column, direction)) -
                    (child.ComputeFlexStartPosition(FlexDirection.Column, direction, containingBlockHeight) +
                     child.ComputeFlexEndPosition(FlexDirection.Column, direction, containingBlockHeight));
                childHeight = BoundAxis(
                    child,
                    FlexDirection.Column,
                    direction,
                    childHeight,
                    containingBlockHeight,
                    containingBlockWidth);
            }
        }

        // Exactly one dimension needs to be defined for us to be able to do aspect
        // ratio calculation. One dimension being the anchor and the other being
        // flexible.
        if (float.IsNaN(childWidth) ^ float.IsNaN(childHeight))
        {
            if (child.AspectRatio.IsDefined)
            {
                if (float.IsNaN(childWidth))
                {
                    childWidth = marginRow + (childHeight - marginColumn) * child.AspectRatio.Value;
                }
                else if (float.IsNaN(childHeight))
                {
                    childHeight = marginColumn + (childWidth - marginRow) / child.AspectRatio.Value;
                }
            }
        }

        // If we're still missing one or the other dimension, measure the content.
        if (float.IsNaN(childWidth) || float.IsNaN(childHeight))
        {
            childWidthSizingMode = float.IsNaN(childWidth)
                ? SizingMode.MaxContent
                : SizingMode.StretchFit;
            childHeightSizingMode = float.IsNaN(childHeight)
                ? SizingMode.MaxContent
                : SizingMode.StretchFit;

            // If the size of the owner is defined then try to constrain the absolute
            // child to that size as well. This allows text within the absolute child
            // to wrap to the size of its owner. This is the same behavior as many
            // browsers implement.
            if (!isMainAxisRow && float.IsNaN(childWidth) &&
                widthMode != SizingMode.MaxContent &&
                !float.IsNaN(containingBlockWidth) && containingBlockWidth > 0)
            {
                childWidth = containingBlockWidth;
                childWidthSizingMode = SizingMode.FitContent;
            }

            CalculateLayoutInternal(
                child,
                childWidth,
                childHeight,
                direction,
                childWidthSizingMode,
                childHeightSizingMode,
                containingBlockWidth,
                containingBlockHeight,
                false,
                depth,
                generationCount);

            childWidth = child._layout.GetMeasuredDimension(Dimension.Width) +
                child.ComputeMarginForAxis(FlexDirection.Row, containingBlockWidth);

            childHeight = child._layout.GetMeasuredDimension(Dimension.Height) +
                child.ComputeMarginForAxis(FlexDirection.Column, containingBlockWidth);
        }

        CalculateLayoutInternal(
            child,
            childWidth,
            childHeight,
            direction,
            SizingMode.StretchFit,
            SizingMode.StretchFit,
            containingBlockWidth,
            containingBlockHeight,
            true,
            depth,
            generationCount);

        PositionAbsoluteChild(
            containingNode,
            node,
            child,
            direction,
            mainAxis,
            true /*isMainAxis*/,
            containingBlockWidth,
            containingBlockHeight);

        PositionAbsoluteChild(
            containingNode,
            node,
            child,
            direction,
            crossAxis,
            false /*isMainAxis*/,
            containingBlockWidth,
            containingBlockHeight);
    }

    private static bool LayoutAbsoluteDescendants(
        Node containingNode,
        Node currentNode,
        SizingMode widthSizingMode,
        Direction currentNodeDirection,
        uint currentDepth,
        uint generationCount,
        float currentNodeLeftOffsetFromContainingBlock,
        float currentNodeTopOffsetFromContainingBlock,
        float containingNodeAvailableInnerWidth,
        float containingNodeAvailableInnerHeight)
    {
        var hasNewLayout = false;

        foreach (var child in currentNode)
        {
            if (child.Display == Display.None)
            {
                continue;
            }
            else if (child.PositionType == PositionType.Absolute)
            {
                var absoluteErrata = currentNode.Config.Errata.HasFlag(Errata.AbsolutePercentAgainstInnerSize);
                var containingBlockWidth = absoluteErrata
                    ? containingNodeAvailableInnerWidth
                    : containingNode._layout.GetMeasuredDimension(Dimension.Width) -
                        containingNode.ComputeBorderForAxis(FlexDirection.Row);
                var containingBlockHeight = absoluteErrata
                    ? containingNodeAvailableInnerHeight
                    : containingNode._layout.GetMeasuredDimension(Dimension.Height) -
                        containingNode.ComputeBorderForAxis(FlexDirection.Column);

                LayoutAbsoluteChild(
                    containingNode,
                    currentNode,
                    child,
                    containingBlockWidth,
                    containingBlockHeight,
                    widthSizingMode,
                    currentNodeDirection,
                    currentDepth,
                    generationCount);

                hasNewLayout |= child.HasNewLayout;

                /*
                 * At this point the child has its position set but only on its the
                 * parent's flexStart edge. Additionally, this position should be
                 * interpreted relative to the containing block of the child if it had
                 * insets defined. So we need to adjust the position by subtracting the
                 * the parents offset from the containing block. However, getting that
                 * offset is complicated since the two nodes can have different main/cross
                 * axes.
                 */
                var parentMainAxis = currentNode.FlexDirection.ResolveDirection(currentNodeDirection);
                var parentCrossAxis = parentMainAxis.ResolveCrossDirection(currentNodeDirection);

                if (parentMainAxis.NeedsTrailingPosition())
                {
                    var mainInsetsDefined = parentMainAxis.IsRow()
                        ? child.HorizontalInsetsDefined()
                        : child.VerticalInsetsDefined();

                    SetChildTrailingPosition(
                        mainInsetsDefined ? containingNode : currentNode,
                        child,
                        parentMainAxis);
                }
                if (parentCrossAxis.NeedsTrailingPosition())
                {
                    var crossInsetsDefined = parentCrossAxis.IsRow()
                        ? child.HorizontalInsetsDefined()
                        : child.VerticalInsetsDefined();

                    SetChildTrailingPosition(
                        crossInsetsDefined ? containingNode : currentNode,
                        child,
                        parentCrossAxis);
                }

                /*
                 * At this point we know the left and top physical edges of the child are
                 * set with positions that are relative to the containing block if insets
                 * are defined
                 */
                var childLeftPosition =
                    child._layout.GetPosition(PhysicalEdge.Left);
                var childTopPosition =
                    child._layout.GetPosition(PhysicalEdge.Top);

                var childLeftOffsetFromParent =
                    child.HorizontalInsetsDefined()
                    ? (childLeftPosition - currentNodeLeftOffsetFromContainingBlock)
                    : childLeftPosition;
                var childTopOffsetFromParent =
                    child.VerticalInsetsDefined()
                    ? (childTopPosition - currentNodeTopOffsetFromContainingBlock)
                    : childTopPosition;

                child._layout.SetPosition(childLeftOffsetFromParent, PhysicalEdge.Left);
                child._layout.SetPosition(childTopOffsetFromParent, PhysicalEdge.Top);
            }
            else if (
                child.PositionType == PositionType.Static &&
                !child.AlwaysFormsContainingBlock)
            {
                // We may write new layout results for absolute descendants of "child"
                // which are positioned relative to the current containing block instead
                // of their parent. "child" may not be dirty, or have new constraints, so
                // absolute positioning may be the first time during this layout pass that
                // we need to mutate these descendents. Make sure the path of
                // nodes to them is mutable before positioning.
                // child->cloneChildrenIfNeeded();
                var childDirection = child.ResolveDirection(currentNodeDirection);
                // By now all descendants of the containing block that are not absolute
                // will have their positions set for left and top.
                var childLeftOffsetFromContainingBlock =
                    currentNodeLeftOffsetFromContainingBlock +
                    child._layout.GetPosition(PhysicalEdge.Left);
                var childTopOffsetFromContainingBlock =
                    currentNodeTopOffsetFromContainingBlock +
                    child._layout.GetPosition(PhysicalEdge.Top);

                hasNewLayout = LayoutAbsoluteDescendants(
                    containingNode,
                    child,
                    widthSizingMode,
                    childDirection,
                    currentDepth + 1,
                    generationCount,
                    childLeftOffsetFromContainingBlock,
                    childTopOffsetFromContainingBlock,
                    containingNodeAvailableInnerWidth,
                    containingNodeAvailableInnerHeight) ||
                    hasNewLayout;

                if (hasNewLayout)
                {
                    child.HasNewLayout = hasNewLayout;
                }
            }
        }
        return hasNewLayout;
    }
}
