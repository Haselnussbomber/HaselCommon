using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;
using PropertyChanged.SourceGenerator;

namespace HaselCommon.Gui;

public partial class Node
{
    private const float DefaultFlexGrow = 0.0f;
    private const float DefaultFlexShrink = 0.0f;
    private const float WebDefaultFlexShrink = 1.0f;

    [IsChanged]
    protected bool IsDirtyInternal
    {
        set => MarkDirtyAndPropagate();
    }

    [Notify] private Direction _direction = Direction.Inherit;
    [Notify] private FlexDirection _flexDirection = FlexDirection.Column;
    [Notify] private Justify _justifyContent = Justify.FlexStart;
    [Notify] private Align _alignContent = Align.FlexStart;
    [Notify] private Align _alignItems = Align.Stretch;
    [Notify] private Align _alignSelf = Align.Auto;
    [Notify] private PositionType _positionType = PositionType.Relative;
    [Notify] private Wrap _flexWrap = Wrap.NoWrap;
    [Notify] private Overflow _overflow = Overflow.Visible;
    [Notify] private Display _display = Display.Flex;
    [Notify] private StyleLength _flex = StyleLength.Auto;
    [Notify] private StyleLength _flexGrow = 0;
    [Notify] private StyleLength _flexShrink = 0;
    [Notify] private StyleLength _flexBasis = StyleLength.Auto;

    [Notify] private StyleLength _margin = StyleLength.Undefined;
    [Notify] private StyleLength _marginTop = StyleLength.Undefined;
    [Notify] private StyleLength _marginBottom = StyleLength.Undefined;
    [Notify] private StyleLength _marginLeft = StyleLength.Undefined;
    [Notify] private StyleLength _marginRight = StyleLength.Undefined;
    [Notify] private StyleLength _marginHorizontal = StyleLength.Undefined;
    [Notify] private StyleLength _marginVertical = StyleLength.Undefined;
    [Notify] private StyleLength _marginStart = StyleLength.Undefined;
    [Notify] private StyleLength _marginEnd = StyleLength.Undefined;

    [Notify] private StyleLength _position = StyleLength.Undefined;
    [Notify] private StyleLength _positionTop = StyleLength.Undefined;
    [Notify] private StyleLength _positionBottom = StyleLength.Undefined;
    [Notify] private StyleLength _positionLeft = StyleLength.Undefined;
    [Notify] private StyleLength _positionRight = StyleLength.Undefined;
    [Notify] private StyleLength _positionHorizontal = StyleLength.Undefined;
    [Notify] private StyleLength _positionVertical = StyleLength.Undefined;
    [Notify] private StyleLength _positionStart = StyleLength.Undefined;
    [Notify] private StyleLength _positionEnd = StyleLength.Undefined;

    [Notify] private StyleLength _padding = StyleLength.Undefined;
    [Notify] private StyleLength _paddingTop = StyleLength.Undefined;
    [Notify] private StyleLength _paddingBottom = StyleLength.Undefined;
    [Notify] private StyleLength _paddingLeft = StyleLength.Undefined;
    [Notify] private StyleLength _paddingRight = StyleLength.Undefined;
    [Notify] private StyleLength _paddingHorizontal = StyleLength.Undefined;
    [Notify] private StyleLength _paddingVertical = StyleLength.Undefined;
    [Notify] private StyleLength _paddingStart = StyleLength.Undefined;
    [Notify] private StyleLength _paddingEnd = StyleLength.Undefined;

    [Notify] private StyleLength _border = StyleLength.Undefined;
    [Notify] private StyleLength _borderTop = StyleLength.Undefined;
    [Notify] private StyleLength _borderBottom = StyleLength.Undefined;
    [Notify] private StyleLength _borderLeft = StyleLength.Undefined;
    [Notify] private StyleLength _borderRight = StyleLength.Undefined;
    [Notify] private StyleLength _borderHorizontal = StyleLength.Undefined;
    [Notify] private StyleLength _borderVertical = StyleLength.Undefined;
    [Notify] private StyleLength _borderStart = StyleLength.Undefined;
    [Notify] private StyleLength _borderEnd = StyleLength.Undefined;

    [Notify] private StyleLength _gap = 0;
    [Notify] private StyleLength _rowGap = StyleLength.Undefined;
    [Notify] private StyleLength _columnGap = StyleLength.Undefined;

    [Notify] private StyleLength _width = StyleLength.Auto;
    [Notify] private StyleLength _height = StyleLength.Auto;

    [Notify] private StyleLength _minWidth = StyleLength.Undefined;
    [Notify] private StyleLength _minHeight = StyleLength.Undefined;

    [Notify] private StyleLength _maxWidth = StyleLength.Undefined;
    [Notify] private StyleLength _maxHeight = StyleLength.Undefined;

    [Notify] private StyleLength _aspectRatio = StyleLength.Undefined;

    internal StyleLength ResolveFlexBasis()
    {
        var flexBasis = _flexBasis;

        if (flexBasis.Unit != Unit.Auto && flexBasis.Unit != Unit.Undefined)
            return flexBasis;

        if (Flex.IsDefined && Flex.Value > 0.0f)
            return Config.UseWebDefaults ? StyleLength.Auto : 0;

        return StyleLength.Auto;
    }

    internal float ResolveFlexGrow()
    {
        // Root nodes flexGrow should always be 0
        if (Parent == null)
            return 0.0f;

        if (_flexGrow.IsDefined)
            return _flexGrow.Value;

        if (_flex.IsDefined && _flex.Value > 0.0f)
            return _flex.Value;

        return DefaultFlexGrow;
    }

    internal float ResolveFlexShrink()
    {
        if (Parent == null)
            return 0.0f;

        if (_flexShrink.IsDefined)
            return _flexShrink.Value;

        if (!Config.UseWebDefaults && _flex.IsDefined && _flex.Value < 0.0f)
            return -_flex.Value;

        return Config.UseWebDefaults ? WebDefaultFlexShrink : DefaultFlexShrink;
    }

    private bool HorizontalInsetsDefined()
    {
        return PositionLeft.IsDefined || PositionRight.IsDefined || PositionHorizontal.IsDefined || PositionStart.IsDefined || PositionEnd.IsDefined;
    }

    private bool VerticalInsetsDefined()
    {
        return PositionTop.IsDefined || PositionBottom.IsDefined || Position.IsDefined || PositionVertical.IsDefined;
    }

    private bool IsFlexStartPositionDefined(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.FlexStartEdge(), direction).IsDefined;
    }

    private bool IsFlexStartPositionAuto(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.FlexStartEdge(), direction).IsAuto;
    }

    private bool IsInlineStartPositionDefined(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.InlineStartEdge(direction), direction).IsDefined;
    }

    private bool IsInlineStartPositionAuto(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.InlineStartEdge(direction), direction).IsAuto;
    }

    private bool IsFlexEndPositionDefined(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.FlexEndEdge(), direction).IsDefined;
    }

    private bool IsFlexEndPositionAuto(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.FlexEndEdge(), direction).IsAuto;
    }

    private bool IsInlineEndPositionDefined(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.InlineEndEdge(direction), direction).IsDefined;
    }

    private bool IsInlineEndPositionAuto(FlexDirection axis, Direction direction)
    {
        return ComputePosition(axis.InlineEndEdge(direction), direction).IsAuto;
    }

    private float ComputeFlexStartPosition(FlexDirection axis, Direction direction, float axisSize)
    {
        return ComputePosition(axis.FlexStartEdge(), direction).ResolveOrDefault(axisSize, 0.0f);
    }

    private float ComputeInlineStartPosition(FlexDirection axis, Direction direction, float axisSize)
    {
        return ComputePosition(axis.InlineStartEdge(direction), direction).ResolveOrDefault(axisSize, 0.0f);
    }

    private float ComputeFlexEndPosition(FlexDirection axis, Direction direction, float axisSize)
    {
        return ComputePosition(axis.FlexEndEdge(), direction).ResolveOrDefault(axisSize, 0.0f);
    }

    private float ComputeInlineEndPosition(FlexDirection axis, Direction direction, float axisSize)
    {
        return ComputePosition(axis.InlineEndEdge(direction), direction).ResolveOrDefault(axisSize, 0.0f);
    }

    private float ComputeFlexStartMargin(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeMargin(axis.FlexStartEdge(), direction).ResolveOrDefault(widthSize, 0.0f);
    }

    internal float ComputeInlineStartMargin(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeMargin(axis.InlineStartEdge(direction), direction).ResolveOrDefault(widthSize, 0.0f);
    }

    private float ComputeFlexEndMargin(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeMargin(axis.FlexEndEdge(), direction).ResolveOrDefault(widthSize, 0.0f);
    }

    private float ComputeInlineEndMargin(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeMargin(axis.InlineEndEdge(direction), direction).ResolveOrDefault(widthSize, 0.0f);
    }

    private float ComputeFlexStartBorder(FlexDirection axis, Direction direction)
    {
        return ComputeBorder(axis.FlexStartEdge(), direction).ResolveOrMax(0.0f, 0.0f);
    }

    internal float ComputeInlineStartBorder(FlexDirection axis, Direction direction)
    {
        return ComputeBorder(axis.InlineStartEdge(direction), direction).ResolveOrMax(0.0f, 0.0f);
    }

    private float ComputeFlexEndBorder(FlexDirection axis, Direction direction)
    {
        return ComputeBorder(axis.FlexEndEdge(), direction).ResolveOrMax(0.0f, 0.0f);
    }

    private float ComputeInlineEndBorder(FlexDirection axis, Direction direction)
    {
        return ComputeBorder(axis.InlineEndEdge(direction), direction).ResolveOrMax(0.0f, 0.0f);
    }

    private float ComputeFlexStartPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputePadding(axis.FlexStartEdge(), direction).ResolveOrMax(widthSize, 0.0f);
    }

    internal float ComputeInlineStartPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputePadding(axis.InlineStartEdge(direction), direction).ResolveOrMax(widthSize, 0.0f);
    }

    private float ComputeFlexEndPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputePadding(axis.FlexEndEdge(), direction).ResolveOrMax(widthSize, 0.0f);
    }

    private float ComputeInlineEndPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputePadding(axis.InlineEndEdge(direction), direction).ResolveOrMax(widthSize, 0.0f);
    }

    private float ComputeInlineStartPaddingAndBorder(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeInlineStartPadding(axis, direction, widthSize) + ComputeInlineStartBorder(axis, direction);
    }

    private float ComputeFlexStartPaddingAndBorder(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeFlexStartPadding(axis, direction, widthSize) + ComputeFlexStartBorder(axis, direction);
    }

    private float ComputeInlineEndPaddingAndBorder(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeInlineEndPadding(axis, direction, widthSize) + ComputeInlineEndBorder(axis, direction);
    }

    private float ComputeFlexEndPaddingAndBorder(FlexDirection axis, Direction direction, float widthSize)
    {
        return ComputeFlexEndPadding(axis, direction, widthSize) + ComputeFlexEndBorder(axis, direction);
    }

    private float ComputeBorderForAxis(FlexDirection axis)
    {
        return ComputeInlineStartBorder(axis, Direction.LTR) + ComputeInlineEndBorder(axis, Direction.LTR);
    }

    private float ComputeMarginForAxis(FlexDirection axis, float widthSize)
    {
        // The total margin for a given axis does not depend on the direction
        // so hardcoding LTR here to avoid piping direction to this function
        return ComputeInlineStartMargin(axis, Direction.LTR, widthSize) + ComputeInlineEndMargin(axis, Direction.LTR, widthSize);
    }

    internal float ComputeGapForAxis(FlexDirection axis, float ownerSize)
    {
        var gap = axis.IsRow() ? ComputeColumnGap() : ComputeRowGap();
        return gap.ResolveOrMax(ownerSize, 0.0f);
    }

    private bool FlexStartMarginIsAuto(FlexDirection axis, Direction direction)
    {
        return ComputeMargin(axis.FlexStartEdge(), direction).IsAuto;
    }

    private bool FlexEndMarginIsAuto(FlexDirection axis, Direction direction)
    {
        return ComputeMargin(axis.FlexEndEdge(), direction).IsAuto;
    }

    private StyleLength ComputeColumnGap()
    {
        return ColumnGap.IsDefined ? ColumnGap : Gap;
    }

    private StyleLength ComputeRowGap()
    {
        return RowGap.IsDefined ? RowGap : Gap;
    }

    private StyleLength ComputePosition(PhysicalEdge edge, Direction direction)
    {
        switch (edge)
        {
            case PhysicalEdge.Left:

                if (direction == Direction.LTR && PositionStart.IsDefined)
                {
                    return PositionStart;
                }
                else if (direction == Direction.RTL && PositionEnd.IsDefined)
                {
                    return PositionEnd;
                }
                else if (PositionLeft.IsDefined)
                {
                    return PositionLeft;
                }
                else if (PositionHorizontal.IsDefined)
                {
                    return PositionHorizontal;
                }

                return Position;

            case PhysicalEdge.Top:

                if (PositionTop.IsDefined)
                {
                    return PositionTop;
                }
                else if (PositionVertical.IsDefined)
                {
                    return PositionVertical;
                }

                return Position;

            case PhysicalEdge.Right:

                if (direction == Direction.LTR && PositionEnd.IsDefined)
                {
                    return PositionEnd;
                }
                else if (direction == Direction.RTL && PositionStart.IsDefined)
                {
                    return PositionStart;
                }
                else if (PositionRight.IsDefined)
                {
                    return PositionRight;
                }
                else if (PositionHorizontal.IsDefined)
                {
                    return PositionHorizontal;
                }

                return Position;

            case PhysicalEdge.Bottom:

                if (PositionBottom.IsDefined)
                {
                    return PositionBottom;
                }
                else if (PositionVertical.IsDefined)
                {
                    return PositionVertical;
                }

                return Position;
        }

        throw new Exception("Invalid physical edge");
    }

    private StyleLength ComputeMargin(PhysicalEdge edge, Direction direction)
    {
        switch (edge)
        {
            case PhysicalEdge.Left:

                if (direction == Direction.LTR && MarginStart.IsDefined)
                {
                    return MarginStart;
                }
                else if (direction == Direction.RTL && MarginEnd.IsDefined)
                {
                    return MarginEnd;
                }
                else if (MarginLeft.IsDefined)
                {
                    return MarginLeft;
                }
                else if (MarginHorizontal.IsDefined)
                {
                    return MarginHorizontal;
                }

                return Margin;

            case PhysicalEdge.Top:

                if (MarginTop.IsDefined)
                {
                    return MarginTop;
                }
                else if (MarginVertical.IsDefined)
                {
                    return MarginVertical;
                }

                return Margin;

            case PhysicalEdge.Right:

                if (direction == Direction.LTR && MarginEnd.IsDefined)
                {
                    return MarginEnd;
                }
                else if (direction == Direction.RTL && MarginStart.IsDefined)
                {
                    return MarginStart;
                }
                else if (MarginRight.IsDefined)
                {
                    return MarginRight;
                }
                else if (MarginHorizontal.IsDefined)
                {
                    return MarginHorizontal;
                }

                return Margin;

            case PhysicalEdge.Bottom:

                if (MarginBottom.IsDefined)
                {
                    return MarginBottom;
                }
                else if (MarginVertical.IsDefined)
                {
                    return MarginVertical;
                }

                return Margin;
        }

        throw new Exception("Invalid physical edge");
    }

    private StyleLength ComputePadding(PhysicalEdge edge, Direction direction)
    {
        switch (edge)
        {
            case PhysicalEdge.Left:

                if (direction == Direction.LTR && PaddingStart.IsDefined)
                {
                    return PaddingStart;
                }
                else if (direction == Direction.RTL && PaddingEnd.IsDefined)
                {
                    return PaddingEnd;
                }
                else if (PaddingLeft.IsDefined)
                {
                    return PaddingLeft;
                }
                else if (PaddingHorizontal.IsDefined)
                {
                    return PaddingHorizontal;
                }

                return Padding;

            case PhysicalEdge.Top:

                if (PaddingTop.IsDefined)
                {
                    return PaddingTop;
                }
                else if (PaddingVertical.IsDefined)
                {
                    return PaddingVertical;
                }

                return Padding;

            case PhysicalEdge.Right:

                if (direction == Direction.LTR && PaddingEnd.IsDefined)
                {
                    return PaddingEnd;
                }
                else if (direction == Direction.RTL && PaddingStart.IsDefined)
                {
                    return PaddingStart;
                }
                else if (PaddingRight.IsDefined)
                {
                    return PaddingRight;
                }
                else if (PaddingHorizontal.IsDefined)
                {
                    return PaddingHorizontal;
                }

                return Padding;

            case PhysicalEdge.Bottom:

                if (PaddingBottom.IsDefined)
                {
                    return PaddingBottom;
                }
                else if (PaddingVertical.IsDefined)
                {
                    return PaddingVertical;
                }

                return Padding;
        }

        throw new Exception("Invalid physical edge");
    }

    private StyleLength ComputeBorder(PhysicalEdge edge, Direction direction)
    {
        switch (edge)
        {
            case PhysicalEdge.Left:

                if (direction == Direction.LTR && BorderStart.IsDefined)
                {
                    return BorderStart;
                }
                else if (direction == Direction.RTL && BorderEnd.IsDefined)
                {
                    return BorderEnd;
                }
                else if (BorderLeft.IsDefined)
                {
                    return BorderLeft;
                }
                else if (BorderHorizontal.IsDefined)
                {
                    return BorderHorizontal;
                }

                return Border;

            case PhysicalEdge.Top:

                if (BorderTop.IsDefined)
                {
                    return BorderTop;
                }
                else if (BorderVertical.IsDefined)
                {
                    return BorderVertical;
                }

                return Border;

            case PhysicalEdge.Right:

                if (direction == Direction.LTR && BorderEnd.IsDefined)
                {
                    return BorderEnd;
                }
                else if (direction == Direction.RTL && BorderStart.IsDefined)
                {
                    return BorderStart;
                }
                else if (BorderRight.IsDefined)
                {
                    return BorderRight;
                }
                else if (BorderHorizontal.IsDefined)
                {
                    return BorderHorizontal;
                }

                return Border;

            case PhysicalEdge.Bottom:

                if (BorderBottom.IsDefined)
                {
                    return BorderBottom;
                }
                else if (BorderVertical.IsDefined)
                {
                    return BorderVertical;
                }

                return Border;
        }

        throw new Exception("Invalid physical edge");
    }

    private StyleLength GetMaxDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => MaxWidth,
            Dimension.Height => MaxHeight,
            _ => throw new Exception("Invalid Dimension")
        };
    }

    private StyleLength GetMinDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => MinWidth,
            Dimension.Height => MinHeight,
            _ => throw new Exception("Invalid Dimension")
        };
    }
}
