using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

public partial class Node
{
    public bool HadOverflow => _layout.HadOverflow;
    public Direction ComputedDirection => _layout.Direction;

    public float ComputedLeft => _layout.GetPosition(PhysicalEdge.Left);
    public float ComputedTop => _layout.GetPosition(PhysicalEdge.Top);
    public float ComputedRight => _layout.GetPosition(PhysicalEdge.Right);
    public float ComputedBottom => _layout.GetPosition(PhysicalEdge.Bottom);
    public float ComputedWidth => _layout.GetDimension(Dimension.Width);
    public float ComputedHeight => _layout.GetDimension(Dimension.Height);
    public float ComputedMarginTop => GetResolvedLayoutProperty(_layout.Margin, Edge.Top);
    public float ComputedMarginBottom => GetResolvedLayoutProperty(_layout.Margin, Edge.Bottom);
    public float ComputedMarginLeft => GetResolvedLayoutProperty(_layout.Margin, Edge.Left);
    public float ComputedMarginRight => GetResolvedLayoutProperty(_layout.Margin, Edge.Right);
    public float ComputedBorderTop => GetResolvedLayoutProperty(_layout.Border, Edge.Top);
    public float ComputedBorderBottom => GetResolvedLayoutProperty(_layout.Border, Edge.Bottom);
    public float ComputedBorderLeft => GetResolvedLayoutProperty(_layout.Border, Edge.Left);
    public float ComputedBorderRight => GetResolvedLayoutProperty(_layout.Border, Edge.Right);
    public float ComputedPaddingTop => GetResolvedLayoutProperty(_layout.Padding, Edge.Top);
    public float ComputedPaddingBottom => GetResolvedLayoutProperty(_layout.Padding, Edge.Bottom);
    public float ComputedPaddingLeft => GetResolvedLayoutProperty(_layout.Padding, Edge.Left);
    public float ComputedPaddingRight => GetResolvedLayoutProperty(_layout.Padding, Edge.Right);

    private float GetResolvedLayoutProperty(float[] values, Edge edge)
    {
        if (values.Length > (int)Edge.End)
            throw new Exception("Cannot get layout properties of multi-edge shorthands");

        if (edge == Edge.Start)
            return values[(int)(_layout.Direction == Direction.RTL ? PhysicalEdge.Right : PhysicalEdge.Left)];

        if (edge == Edge.End)
            return values[(int)(_layout.Direction == Direction.RTL ? PhysicalEdge.Left : PhysicalEdge.Right)];

        return values[(int)edge];
    }
}
