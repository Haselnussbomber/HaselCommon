using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

public class LayoutResults
{
    internal const int MaxCachedMeasurements = 8;

    internal uint ComputedFlexBasisGeneration { get; set; } = 0;
    internal float ComputedFlexBasis { get; set; } = float.NaN;

    // Instead of recomputing the entire layout every single time, we cache some
    // information to break early when nothing changed
    internal uint GenerationCount { get; set; } = 0;
    internal uint ConfigVersion { get; set; } = 0;
    internal Direction LastOwnerDirection { get; set; } = Direction.Inherit;
    internal uint NextCachedMeasurementsIndex { get; set; } = 0;
    internal CachedMeasurement[] CachedMeasurements { get; set; } = new CachedMeasurement[MaxCachedMeasurements];
    internal CachedMeasurement CachedLayout { get; set; } = new();

    public Direction Direction { get; internal set; } = Direction.Inherit;
    public bool HadOverflow { get; internal set; } = false;

    // Dimensions
    public float Width { get; internal set; }
    public float Height { get; internal set; }

    // MeasuredDimensions
    public float MeasuredWidth { get; internal set; }
    public float MeasuredHeight { get; internal set; }

    // Position
    public float PositionTop { get; internal set; }
    public float PositionBottom { get; internal set; }
    public float PositionLeft { get; internal set; }
    public float PositionRight { get; internal set; }

    // Margin
    public float MarginTop { get; internal set; }
    public float MarginBottom { get; internal set; }
    public float MarginLeft { get; internal set; }
    public float MarginRight { get; internal set; }

    // Border
    public float BorderTop { get; internal set; }
    public float BorderBottom { get; internal set; }
    public float BorderLeft { get; internal set; }
    public float BorderRight { get; internal set; }

    // Padding
    public float PaddingTop { get; internal set; }
    public float PaddingBottom { get; internal set; }
    public float PaddingLeft { get; internal set; }
    public float PaddingRight { get; internal set; }

    internal float GetDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => Width,
            Dimension.Height => Height,
            _ => throw new Exception("Invalid Dimension"),
        };
    }

    internal void SetDimension(float value, Dimension dimension)
    {
        switch (dimension)
        {
            case Dimension.Width: Width = value; break;
            case Dimension.Height: Height = value; break;
            default: throw new Exception("Invalid Dimension");
        };
    }

    internal float GetMeasuredDimension(Dimension dimension)
    {
        return dimension switch
        {
            Dimension.Width => MeasuredWidth,
            Dimension.Height => MeasuredHeight,
            _ => throw new Exception("Invalid Dimension"),
        };
    }

    internal void SetMeasuredDimension(float value, Dimension dimension)
    {
        switch (dimension)
        {
            case Dimension.Width: MeasuredWidth = value; break;
            case Dimension.Height: MeasuredHeight = value; break;
            default: throw new Exception("Invalid Dimension");
        };
    }

    internal float GetPosition(PhysicalEdge edge)
    {
        return edge switch
        {
            PhysicalEdge.Left => PositionLeft,
            PhysicalEdge.Top => PositionTop,
            PhysicalEdge.Right => PositionRight,
            PhysicalEdge.Bottom => PositionBottom,
            _ => throw new Exception("Invalid PhysicalEdge"),
        };
    }

    internal void SetPosition(float position, PhysicalEdge edge)
    {
        switch (edge)
        {
            case PhysicalEdge.Left: PositionLeft = position; break;
            case PhysicalEdge.Top: PositionTop = position; break;
            case PhysicalEdge.Right: PositionRight = position; break;
            case PhysicalEdge.Bottom: PositionBottom = position; break;
        }
    }

    internal float GetMargin(PhysicalEdge edge)
    {
        return edge switch
        {
            PhysicalEdge.Left => MarginLeft,
            PhysicalEdge.Top => MarginTop,
            PhysicalEdge.Right => MarginRight,
            PhysicalEdge.Bottom => MarginBottom,
            _ => throw new Exception("Invalid PhysicalEdge"),
        };
    }

    internal void SetMargin(float margin, PhysicalEdge edge)
    {
        switch (edge)
        {
            case PhysicalEdge.Left: MarginLeft = margin; break;
            case PhysicalEdge.Top: MarginTop = margin; break;
            case PhysicalEdge.Right: MarginRight = margin; break;
            case PhysicalEdge.Bottom: MarginBottom = margin; break;
        }
    }

    internal float GetBorder(PhysicalEdge edge)
    {
        return edge switch
        {
            PhysicalEdge.Left => BorderLeft,
            PhysicalEdge.Top => BorderTop,
            PhysicalEdge.Right => BorderRight,
            PhysicalEdge.Bottom => BorderBottom,
            _ => throw new Exception("Invalid PhysicalEdge"),
        };
    }

    internal void SetBorder(float border, PhysicalEdge edge)
    {
        switch (edge)
        {
            case PhysicalEdge.Left: BorderLeft = border; break;
            case PhysicalEdge.Top: BorderTop = border; break;
            case PhysicalEdge.Right: BorderRight = border; break;
            case PhysicalEdge.Bottom: BorderBottom = border; break;
        }
    }

    internal float GetPadding(PhysicalEdge edge)
    {
        return edge switch
        {
            PhysicalEdge.Left => PaddingLeft,
            PhysicalEdge.Top => PaddingTop,
            PhysicalEdge.Right => PaddingRight,
            PhysicalEdge.Bottom => PaddingBottom,
            _ => throw new Exception("Invalid PhysicalEdge"),
        };
    }

    internal void SetPadding(float padding, PhysicalEdge edge)
    {
        switch (edge)
        {
            case PhysicalEdge.Left: PaddingLeft = padding; break;
            case PhysicalEdge.Top: PaddingTop = padding; break;
            case PhysicalEdge.Right: PaddingRight = padding; break;
            case PhysicalEdge.Bottom: PaddingBottom = padding; break;
        }
    }
}
