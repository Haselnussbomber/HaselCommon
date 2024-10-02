using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;
using HaselCommon.Utils;

namespace HaselCommon.Gui;

public partial class Node
{
    private const float DefaultFlexGrow = 0.0f;
    private const float DefaultFlexShrink = 0.0f;
    private const float WebDefaultFlexShrink = 1.0f;

    private Direction _direction = Direction.Inherit;
    private FlexDirection _flexDirection = FlexDirection.Column;
    private Justify _justifyContent = Justify.FlexStart;
    private Align _alignContent = Align.FlexStart;
    private Align _alignItems = Align.Stretch;
    private Align _alignSelf = Align.Auto;
    private PositionType _positionType = PositionType.Relative;
    private Wrap _flexWrap = Wrap.NoWrap;
    private Overflow _overflow = Overflow.Visible;
    private Display _display = Display.Flex;
    private StyleLength _flex = StyleLength.Auto;
    private StyleLength _flexGrow = StyleLength.Undefined;
    private StyleLength _flexShrink = StyleLength.Undefined;
    private StyleLength _flexBasis = StyleLength.Auto;

    private readonly StyleLength[] _margin = new StyleLength[Enum.GetValues<Edge>().Length];
    private readonly StyleLength[] _position = new StyleLength[Enum.GetValues<Edge>().Length];
    private readonly StyleLength[] _padding = new StyleLength[Enum.GetValues<Edge>().Length];
    private readonly StyleLength[] _border = new StyleLength[Enum.GetValues<Edge>().Length];

    private StyleLength _gap = 0;
    private StyleLength _rowGap = StyleLength.Undefined;
    private StyleLength _columnGap = StyleLength.Undefined;

    private StyleLength _width = StyleLength.Auto;
    private StyleLength _height = StyleLength.Auto;

    private StyleLength _minWidth = StyleLength.Undefined;
    private StyleLength _minHeight = StyleLength.Undefined;

    private StyleLength _maxWidth = StyleLength.Undefined;
    private StyleLength _maxHeight = StyleLength.Undefined;

    private StyleLength _aspectRatio = StyleLength.Undefined;

    public Direction Direction
    {
        get => _direction;
        set
        {
            if (_direction != value)
            {
                _direction = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public FlexDirection FlexDirection
    {
        get => _flexDirection;
        set
        {
            if (_flexDirection != value)
            {
                _flexDirection = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Justify JustifyContent
    {
        get => _justifyContent;
        set
        {
            if (_justifyContent != value)
            {
                _justifyContent = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Align AlignContent
    {
        get => _alignContent;
        set
        {
            if (_alignContent != value)
            {
                _alignContent = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Align AlignItems
    {
        get => _alignItems;
        set
        {
            if (_alignItems != value)
            {
                _alignItems = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Align AlignSelf
    {
        get => _alignSelf;
        set
        {
            if (_alignSelf != value)
            {
                _alignSelf = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public PositionType PositionType
    {
        get => _positionType;
        set
        {
            if (_positionType != value)
            {
                _positionType = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Wrap FlexWrap
    {
        get => _flexWrap;
        set
        {
            if (_flexWrap != value)
            {
                _flexWrap = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Overflow Overflow
    {
        get => _overflow;
        set
        {
            if (_overflow != value)
            {
                _overflow = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public Display Display
    {
        get => _display;
        set
        {
            if (_display != value)
            {
                _display = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength Flex
    {
        get => _flex;
        set
        {
            if (_flex != value)
            {
                _flex = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength FlexGrow
    {
        get => _flexGrow.IsUndefined ? DefaultFlexGrow : _flexGrow;
        set
        {
            if (_flexGrow != value)
            {
                _flexGrow = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength FlexShrink
    {
        get
        {
            if (_flexShrink.IsDefined)
                return _flexShrink.Value;

            return Config.UseWebDefaults ? WebDefaultFlexShrink : DefaultFlexShrink;
        }

        set
        {
            if (_flexShrink != value)
            {
                _flexShrink = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength FlexBasis
    {
        get => _flexBasis;
        set
        {
            if (_flexBasis != value)
            {
                _flexBasis = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginAll
    {
        get => _margin[(int)Edge.All];
        set
        {
            if (_margin[(int)Edge.All] != value)
            {
                _margin[(int)Edge.All] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginTop
    {
        get => _margin[(int)Edge.Top];
        set
        {
            if (_margin[(int)Edge.Top] != value)
            {
                _margin[(int)Edge.Top] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginBottom
    {
        get => _margin[(int)Edge.Bottom];
        set
        {
            if (_margin[(int)Edge.Bottom] != value)
            {
                _margin[(int)Edge.Bottom] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginLeft
    {
        get => _margin[(int)Edge.Left];
        set
        {
            if (_margin[(int)Edge.Left] != value)
            {
                _margin[(int)Edge.Left] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginRight
    {
        get => _margin[(int)Edge.Right];
        set
        {
            if (_margin[(int)Edge.Right] != value)
            {
                _margin[(int)Edge.Right] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginHorizontal
    {
        get => _margin[(int)Edge.Horizontal];
        set
        {
            if (_margin[(int)Edge.Horizontal] != value)
            {
                _margin[(int)Edge.Horizontal] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginVertical
    {
        get => _margin[(int)Edge.Vertical];
        set
        {
            if (_margin[(int)Edge.Vertical] != value)
            {
                _margin[(int)Edge.Vertical] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginStart
    {
        get => _margin[(int)Edge.Start];
        set
        {
            if (_margin[(int)Edge.Start] != value)
            {
                _margin[(int)Edge.Start] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MarginEnd
    {
        get => _margin[(int)Edge.End];
        set
        {
            if (_margin[(int)Edge.End] != value)
            {
                _margin[(int)Edge.End] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionAll
    {
        get => _position[(int)Edge.All];
        set
        {
            if (_position[(int)Edge.All] != value)
            {
                _position[(int)Edge.All] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionTop
    {
        get => _position[(int)Edge.Top];
        set
        {
            if (_position[(int)Edge.Top] != value)
            {
                _position[(int)Edge.Top] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionBottom
    {
        get => _position[(int)Edge.Bottom];
        set
        {
            if (_position[(int)Edge.Bottom] != value)
            {
                _position[(int)Edge.Bottom] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionLeft
    {
        get => _position[(int)Edge.Left];
        set
        {
            if (_position[(int)Edge.Left] != value)
            {
                _position[(int)Edge.Left] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionRight
    {
        get => _position[(int)Edge.Right];
        set
        {
            if (_position[(int)Edge.Right] != value)
            {
                _position[(int)Edge.Right] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionHorizontal
    {
        get => _position[(int)Edge.Horizontal];
        set
        {
            if (_position[(int)Edge.Horizontal] != value)
            {
                _position[(int)Edge.Horizontal] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionVertical
    {
        get => _position[(int)Edge.Vertical];
        set
        {
            if (_position[(int)Edge.Vertical] != value)
            {
                _position[(int)Edge.Vertical] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionStart
    {
        get => _position[(int)Edge.Start];
        set
        {
            if (_position[(int)Edge.Start] != value)
            {
                _position[(int)Edge.Start] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PositionEnd
    {
        get => _position[(int)Edge.End];
        set
        {
            if (_position[(int)Edge.End] != value)
            {
                _position[(int)Edge.End] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingAll
    {
        get => _padding[(int)Edge.All];
        set
        {
            if (_padding[(int)Edge.All] != value)
            {
                _padding[(int)Edge.All] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingTop
    {
        get => _padding[(int)Edge.Top];
        set
        {
            if (_padding[(int)Edge.Top] != value)
            {
                _padding[(int)Edge.Top] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingBottom
    {
        get => _padding[(int)Edge.Bottom];
        set
        {
            if (_padding[(int)Edge.Bottom] != value)
            {
                _padding[(int)Edge.Bottom] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingLeft
    {
        get => _padding[(int)Edge.Left];
        set
        {
            if (_padding[(int)Edge.Left] != value)
            {
                _padding[(int)Edge.Left] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingRight
    {
        get => _padding[(int)Edge.Right];
        set
        {
            if (_padding[(int)Edge.Right] != value)
            {
                _padding[(int)Edge.Right] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingHorizontal
    {
        get => _padding[(int)Edge.Horizontal];
        set
        {
            if (_padding[(int)Edge.Horizontal] != value)
            {
                _padding[(int)Edge.Horizontal] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingVertical
    {
        get => _padding[(int)Edge.Vertical];
        set
        {
            if (_padding[(int)Edge.Vertical] != value)
            {
                _padding[(int)Edge.Vertical] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingStart
    {
        get => _padding[(int)Edge.Start];
        set
        {
            if (_padding[(int)Edge.Start] != value)
            {
                _padding[(int)Edge.Start] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength PaddingEnd
    {
        get => _padding[(int)Edge.End];
        set
        {
            if (_padding[(int)Edge.End] != value)
            {
                _padding[(int)Edge.End] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderAll
    {
        get => _border[(int)Edge.All];
        set
        {
            if (_border[(int)Edge.All] != value)
            {
                _border[(int)Edge.All] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderTop
    {
        get => _border[(int)Edge.Top];
        set
        {
            if (_border[(int)Edge.Top] != value)
            {
                _border[(int)Edge.Top] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderBottom
    {
        get => _border[(int)Edge.Bottom];
        set
        {
            if (_border[(int)Edge.Bottom] != value)
            {
                _border[(int)Edge.Bottom] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderLeft
    {
        get => _border[(int)Edge.Left];
        set
        {
            if (_border[(int)Edge.Left] != value)
            {
                _border[(int)Edge.Left] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderRight
    {
        get => _border[(int)Edge.Right];
        set
        {
            if (_border[(int)Edge.Right] != value)
            {
                _border[(int)Edge.Right] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderHorizontal
    {
        get => _border[(int)Edge.Horizontal];
        set
        {
            if (_border[(int)Edge.Horizontal] != value)
            {
                _border[(int)Edge.Horizontal] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderVertical
    {
        get => _border[(int)Edge.Vertical];
        set
        {
            if (_border[(int)Edge.Vertical] != value)
            {
                _border[(int)Edge.Vertical] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderStart
    {
        get => _border[(int)Edge.Start];
        set
        {
            if (_border[(int)Edge.Start] != value)
            {
                _border[(int)Edge.Start] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength BorderEnd
    {
        get => _border[(int)Edge.End];
        set
        {
            if (_border[(int)Edge.End] != value)
            {
                _border[(int)Edge.End] = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength Gap
    {
        get => _gap;
        set
        {
            if (_gap != value)
            {
                _gap = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength RowGap
    {
        get => _rowGap;
        set
        {
            if (_rowGap != value)
            {
                _rowGap = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength ColumnGap
    {
        get => _columnGap;
        set
        {
            if (_columnGap != value)
            {
                _columnGap = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MinWidth
    {
        get => _minWidth;
        set
        {
            if (_minWidth != value)
            {
                _minWidth = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MinHeight
    {
        get => _minHeight;
        set
        {
            if (_minHeight != value)
            {
                _minHeight = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (_maxWidth != value)
            {
                _maxWidth = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength MaxHeight
    {
        get => _maxHeight;
        set
        {
            if (_maxHeight != value)
            {
                _maxHeight = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    public StyleLength AspectRatio
    {
        get => _aspectRatio;
        set
        {
            if (_aspectRatio != value)
            {
                _aspectRatio = value;
                MarkDirtyAndPropagate();
            }
        }
    }

    internal StyleLength ResolveFlexBasis()
    {
        var flexBasis = _flexBasis;

        if (flexBasis.Unit != Unit.Auto && flexBasis.Unit != Unit.Undefined)
            return flexBasis;

        if (_flex.IsDefined && _flex.Value > 0.0f)
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
        return _position[(int)Edge.Left].IsDefined ||
            _position[(int)Edge.Right].IsDefined ||
            _position[(int)Edge.Horizontal].IsDefined ||
            _position[(int)Edge.Start].IsDefined ||
            _position[(int)Edge.End].IsDefined;
    }

    private bool VerticalInsetsDefined()
    {
        return _position[(int)Edge.Top].IsDefined ||
            _position[(int)Edge.Bottom].IsDefined ||
            _position[(int)Edge.All].IsDefined ||
            _position[(int)Edge.Vertical].IsDefined;
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
        return MathUtils.MaxOrDefined(ComputeBorder(axis.FlexStartEdge(), direction).Resolve(0.0f), 0.0f);
    }

    internal float ComputeInlineStartBorder(FlexDirection axis, Direction direction)
    {
        return MathUtils.MaxOrDefined(ComputeBorder(axis.InlineStartEdge(direction), direction).Resolve(0.0f), 0.0f);
    }

    private float ComputeFlexEndBorder(FlexDirection axis, Direction direction)
    {
        return MathUtils.MaxOrDefined(ComputeBorder(axis.FlexEndEdge(), direction).Resolve(0.0f), 0.0f);
    }

    private float ComputeInlineEndBorder(FlexDirection axis, Direction direction)
    {
        return MathUtils.MaxOrDefined(ComputeBorder(axis.InlineEndEdge(direction), direction).Resolve(0.0f), 0.0f);
    }

    private float ComputeFlexStartPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return MathUtils.MaxOrDefined(ComputePadding(axis.FlexStartEdge(), direction).Resolve(widthSize), 0.0f);
    }

    internal float ComputeInlineStartPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return MathUtils.MaxOrDefined(ComputePadding(axis.InlineStartEdge(direction), direction).Resolve(widthSize), 0.0f);
    }

    private float ComputeFlexEndPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return MathUtils.MaxOrDefined(ComputePadding(axis.FlexEndEdge(), direction).Resolve(widthSize), 0.0f);
    }

    private float ComputeInlineEndPadding(FlexDirection axis, Direction direction, float widthSize)
    {
        return MathUtils.MaxOrDefined(ComputePadding(axis.InlineEndEdge(direction), direction).Resolve(widthSize), 0.0f);
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
        return MathUtils.MaxOrDefined(gap.Resolve(ownerSize), 0.0f);
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
        return _columnGap.IsDefined ? _columnGap : _gap;
    }

    private StyleLength ComputeRowGap()
    {
        return _rowGap.IsDefined ? _rowGap : _gap;
    }

    private static StyleLength ComputeLeftEdge(StyleLength[] edges, Direction layoutDirection)
    {
        if (layoutDirection == Direction.LTR && edges[(int)Edge.Start].IsDefined)
        {
            return edges[(int)Edge.Start];
        }
        else if (layoutDirection == Direction.RTL && edges[(int)Edge.End].IsDefined)
        {
            return edges[(int)Edge.End];
        }
        else if (edges[(int)Edge.Left].IsDefined)
        {
            return edges[(int)Edge.Left];
        }
        else if (edges[(int)Edge.Horizontal].IsDefined)
        {
            return edges[(int)Edge.Horizontal];
        }

        return edges[(int)Edge.All];
    }

    private static StyleLength ComputeTopEdge(StyleLength[] edges)
    {
        if (edges[(int)Edge.Top].IsDefined)
        {
            return edges[(int)Edge.Top];
        }
        else if (edges[(int)Edge.Vertical].IsDefined)
        {
            return edges[(int)Edge.Vertical];
        }
        else
        {
            return edges[(int)Edge.All];
        }
    }

    private static StyleLength ComputeRightEdge(StyleLength[] edges, Direction layoutDirection)
    {
        if (layoutDirection == Direction.LTR && edges[(int)Edge.End].IsDefined)
        {
            return edges[(int)Edge.End];
        }
        else if (layoutDirection == Direction.RTL && edges[(int)Edge.Start].IsDefined)
        {
            return edges[(int)Edge.Start];
        }
        else if (edges[(int)Edge.Right].IsDefined)
        {
            return edges[(int)Edge.Right];
        }
        else if (edges[(int)Edge.Horizontal].IsDefined)
        {
            return edges[(int)Edge.Horizontal];
        }
        else
        {
            return edges[(int)Edge.All];
        }
    }

    private static StyleLength ComputeBottomEdge(StyleLength[] edges)
    {
        if (edges[(int)Edge.Bottom].IsDefined)
        {
            return edges[(int)Edge.Bottom];
        }
        else if (edges[(int)Edge.Vertical].IsDefined)
        {
            return edges[(int)Edge.Vertical];
        }
        else
        {
            return edges[(int)Edge.All];
        }
    }

    private StyleLength ComputeMargin(PhysicalEdge edge, Direction direction)
    {
        return edge switch
        {
            PhysicalEdge.Left => ComputeLeftEdge(_margin, direction),
            PhysicalEdge.Top => ComputeTopEdge(_margin),
            PhysicalEdge.Right => ComputeRightEdge(_margin, direction),
            PhysicalEdge.Bottom => ComputeBottomEdge(_margin),
            _ => throw new Exception("Invalid physical edge"),
        };
    }

    private StyleLength ComputePosition(PhysicalEdge edge, Direction direction)
    {
        return edge switch
        {
            PhysicalEdge.Left => ComputeLeftEdge(_position, direction),
            PhysicalEdge.Top => ComputeTopEdge(_position),
            PhysicalEdge.Right => ComputeRightEdge(_position, direction),
            PhysicalEdge.Bottom => ComputeBottomEdge(_position),
            _ => throw new Exception("Invalid physical edge"),
        };
    }

    private StyleLength ComputePadding(PhysicalEdge edge, Direction direction)
    {
        return edge switch
        {
            PhysicalEdge.Left => ComputeLeftEdge(_padding, direction),
            PhysicalEdge.Top => ComputeTopEdge(_padding),
            PhysicalEdge.Right => ComputeRightEdge(_padding, direction),
            PhysicalEdge.Bottom => ComputeBottomEdge(_padding),
            _ => throw new Exception("Invalid physical edge"),
        };
    }

    private StyleLength ComputeBorder(PhysicalEdge edge, Direction direction)
    {
        return edge switch
        {
            PhysicalEdge.Left => ComputeLeftEdge(_border, direction),
            PhysicalEdge.Top => ComputeTopEdge(_border),
            PhysicalEdge.Right => ComputeRightEdge(_border, direction),
            PhysicalEdge.Bottom => ComputeBottomEdge(_border),
            _ => throw new Exception("Invalid physical edge"),
        };
    }

    private StyleLength GetMaxDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => _maxWidth,
            Dimension.Height => _maxHeight,
            _ => throw new Exception("Invalid Dimension")
        };
    }

    private StyleLength GetMinDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => _minWidth,
            Dimension.Height => _minHeight,
            _ => throw new Exception("Invalid Dimension")
        };
    }
}
