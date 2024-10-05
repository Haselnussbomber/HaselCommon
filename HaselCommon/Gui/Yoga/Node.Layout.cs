using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga;

public partial class Node
{
    [NodeProp("Layout")]
    public bool HadOverflow => _layout.HadOverflow;

    [NodeProp("Layout")]
    public Direction ComputedDirection => _layout.Direction;

    [NodeProp("Layout")]
    public float ComputedLeft => _layout.GetPosition(PhysicalEdge.Left);

    [NodeProp("Layout")]
    public float ComputedTop => _layout.GetPosition(PhysicalEdge.Top);
    
    [NodeProp("Layout")]
    public float ComputedRight => _layout.GetPosition(PhysicalEdge.Right);
    
    [NodeProp("Layout")]
    public float ComputedBottom => _layout.GetPosition(PhysicalEdge.Bottom);
    
    [NodeProp("Layout")]
    public float ComputedWidth => _layout.GetDimension(Dimension.Width);
    
    [NodeProp("Layout")]
    public float ComputedHeight => _layout.GetDimension(Dimension.Height);
    
    [NodeProp("Layout")]
    public float ComputedMarginTop => GetResolvedLayoutProperty(_layout.Margin, Edge.Top);
    
    [NodeProp("Layout")]
    public float ComputedMarginBottom => GetResolvedLayoutProperty(_layout.Margin, Edge.Bottom);
    
    [NodeProp("Layout")]
    public float ComputedMarginLeft => GetResolvedLayoutProperty(_layout.Margin, Edge.Left);
    
    [NodeProp("Layout")]
    public float ComputedMarginRight => GetResolvedLayoutProperty(_layout.Margin, Edge.Right);
    
    [NodeProp("Layout")]
    public float ComputedBorderTop => GetResolvedLayoutProperty(_layout.Border, Edge.Top);
    
    [NodeProp("Layout")]
    public float ComputedBorderBottom => GetResolvedLayoutProperty(_layout.Border, Edge.Bottom);
    
    [NodeProp("Layout")]
    public float ComputedBorderLeft => GetResolvedLayoutProperty(_layout.Border, Edge.Left);
    
    [NodeProp("Layout")]
    public float ComputedBorderRight => GetResolvedLayoutProperty(_layout.Border, Edge.Right);
    
    [NodeProp("Layout")]
    public float ComputedPaddingTop => GetResolvedLayoutProperty(_layout.Padding, Edge.Top);
    
    [NodeProp("Layout")]
    public float ComputedPaddingBottom => GetResolvedLayoutProperty(_layout.Padding, Edge.Bottom);
    
    [NodeProp("Layout")]
    public float ComputedPaddingLeft => GetResolvedLayoutProperty(_layout.Padding, Edge.Left);
    
    [NodeProp("Layout")]
    public float ComputedPaddingRight => GetResolvedLayoutProperty(_layout.Padding, Edge.Right);

    internal float GetResolvedLayoutProperty(float[] values, Edge edge)
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
