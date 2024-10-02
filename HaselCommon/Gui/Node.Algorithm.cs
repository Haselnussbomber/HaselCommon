using HaselCommon.Gui.Enums;
using HaselCommon.Gui.Extensions;

namespace HaselCommon.Gui;

// Based on https://github.com/facebook/yoga/tree/dc4ab5ad571c91c2a29e35a66182315c01726f3c

public partial class Node
{
    private Config _config = new();
    private bool _isDirty = true;
    private int _lineIndex;
    private StyleLength _resolvedWidth = StyleLength.Undefined;
    private StyleLength _resolvedHeight = StyleLength.Undefined;
    private LayoutResults _layout = new();

    /// <summary>
    /// Whether a leaf node's layout results may be truncated during layout rounding.
    /// </summary>
    public NodeType NodeType { get; set; }

    /// <summary>
    /// Whether this node will always form a containing block for any descendant nodes.<br/>
    /// This is useful for when a node has a property outside of of Yoga that will form a containing block.<br/>
    /// For example, transforms or some of the others listed in <see href="https://developer.mozilla.org/en-US/docs/Web/CSS/Containing_block"/>
    /// </summary>
    public bool AlwaysFormsContainingBlock { get; set; }

    /// <summary>
    /// Whether this node should be considered the reference baseline among siblings.
    /// </summary>
    public bool IsReferenceBaseline { get; set; }

    /// <summary>
    /// Whether the <see cref="Baseline(float, float)"/> function is enabled.
    /// </summary>
    public bool HasBaselineFunc { get; set; }

    /// <summary>
    /// Whether the <see cref="Measure(float, MeasureMode, float, MeasureMode)"/> function is enabled.
    /// </summary>
    public bool HasMeasureFunc { get; set; }

    /// <summary>
    /// A config for the node.
    /// </summary>
    public Config Config
    {
        get => _config;
        set
        {
            // C# port: This is skipped when the layout hasn't been generated yet, to allow setting the property on construction.
            if (_layout.GenerationCount != 0 && _config.UseWebDefaults != value.UseWebDefaults)
                throw new Exception("UseWebDefaults may not be changed after constructing a Node");

            if (Config.UpdateInvalidatesLayout(_config, value))
            {
                MarkDirtyAndPropagate();
                _layout.ConfigVersion = 0;
            }
            else
            {
                // If the config is functionally the same, then align the configVersion so
                // that we can reuse the layout cache
                _layout.ConfigVersion = value.Version;
            }

            // C# port: This is processed here when the layout hasn't been generated yet, to allow setting the property on construction.
            if (_layout.GenerationCount == 0 && value.UseWebDefaults)
            {
                FlexDirection = FlexDirection.Row;
                AlignContent = Align.Stretch;
            }

            _config = value;
        }
    }

    /// <summary>
    /// Whether the given node may have new layout results. Must be reset by setting it to false.
    /// </summary>
    public bool HasNewLayout { get; set; } = true;

    /// <summary>
    /// Whether the node's layout results are dirty due to it or its children changing.
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty != value)
            {
                if (value)
                {
                    if (!HasMeasureFunc)
                        throw new InvalidOperationException("Only leaf nodes with custom measure functions should manually mark themselves as dirty");

                    MarkDirtyAndPropagate();
                }
                else
                {
                    _isDirty = false;
                }
            }
        }
    }

    private float DimensionWithMargin(FlexDirection axis, float widthSize)
    {
        return _layout.GetMeasuredDimension(axis.Dimension()) + ComputeMarginForAxis(axis, widthSize);
    }

    private bool IsLayoutDimensionDefined(FlexDirection axis)
    {
        var value = _layout.GetMeasuredDimension(axis.Dimension());
        return !float.IsNaN(value) && value >= 0.0f;
    }

    /**
     * Whether the node has a "definite length" along the given axis.
     * https://www.w3.org/TR/css-sizing-3/#definite
     */
    private bool HasDefiniteLength(Dimension dimension, float ownerSize)
    {
        var usedValue = GetResolvedDimension(dimension).Resolve(ownerSize);
        return !float.IsNaN(usedValue) && usedValue >= 0.0f;
    }

    private StyleLength GetResolvedDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => Width,
            Dimension.Height => Height,
            _ => throw new Exception("Invalid Dimension")
        };
    }

    // If both left and right are defined, then use left. Otherwise return +left or
    // -right depending on which is defined. Ignore statically positioned nodes as
    // insets do not apply to them.
    private float RelativePosition(FlexDirection axis, Direction direction, float axisSize)
    {
        if (PositionType == PositionType.Static)
            return 0;

        if (IsInlineStartPositionDefined(axis, direction) && !IsInlineStartPositionAuto(axis, direction))
            return ComputeInlineStartPosition(axis, direction, axisSize);

        return -1 * ComputeInlineEndPosition(axis, direction, axisSize);
    }

    private void SetPosition(Direction direction, float ownerWidth, float ownerHeight)
    {
        /* Root nodes should be always layouted as LTR, so we don't return negative
         * values. */
        var directionRespectingRoot = Parent != null ? direction : Direction.LTR;
        var mainAxis = FlexDirection.ResolveDirection(directionRespectingRoot);
        var crossAxis = mainAxis.ResolveCrossDirection(directionRespectingRoot);

        // In the case of position static these are just 0. See:
        // https://www.w3.org/TR/css-position-3/#valdef-position-static
        var relativePositionMain = RelativePosition(
            mainAxis,
            directionRespectingRoot,
            mainAxis.IsRow() ? ownerWidth : ownerHeight);
        var relativePositionCross = RelativePosition(
            crossAxis,
            directionRespectingRoot,
            mainAxis.IsRow() ? ownerHeight : ownerWidth);

        var mainAxisLeadingEdge = mainAxis.InlineStartEdge(direction);
        var mainAxisTrailingEdge = mainAxis.InlineEndEdge(direction);
        var crossAxisLeadingEdge = crossAxis.InlineStartEdge(direction);
        var crossAxisTrailingEdge = crossAxis.InlineEndEdge(direction);

        _layout.SetPosition(ComputeInlineStartMargin(mainAxis, direction, ownerWidth) + relativePositionMain, mainAxisLeadingEdge);
        _layout.SetPosition(ComputeInlineEndMargin(mainAxis, direction, ownerWidth) + relativePositionMain, mainAxisTrailingEdge);
        _layout.SetPosition(ComputeInlineStartMargin(crossAxis, direction, ownerWidth) + relativePositionCross, crossAxisLeadingEdge);
        _layout.SetPosition(ComputeInlineEndMargin(crossAxis, direction, ownerWidth) + relativePositionCross, crossAxisTrailingEdge);
    }

    private void ResolveDimension()
    {
        if (MaxWidth.IsDefined && MaxWidth.IsApproximately(MinWidth))
        {
            _resolvedWidth = MaxWidth;
        }
        else
        {
            _resolvedWidth = Width;
        }

        if (MaxHeight.IsDefined && MaxHeight.IsApproximately(MinHeight))
        {
            _resolvedHeight = MaxHeight;
        }
        else
        {
            _resolvedHeight = Height;
        }
    }

    private Direction ResolveDirection(Direction ownerDirection)
    {
        if (Direction == Direction.Inherit)
            return ownerDirection != Direction.Inherit ? ownerDirection : Direction.LTR;

        return Direction;
    }

    private void MarkDirtyAndPropagate()
    {
        if (!IsDirty)
        {
            _isDirty = true;
            _layout.ComputedFlexBasis = float.NaN;
            Parent?.MarkDirtyAndPropagate();
        }
    }

    private bool IsNodeFlexible()
    {
        return (PositionType != PositionType.Absolute) && (ResolveFlexGrow() != 0 || ResolveFlexShrink() != 0);
    }
}
