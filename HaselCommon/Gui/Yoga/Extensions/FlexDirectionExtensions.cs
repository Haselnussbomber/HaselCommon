using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga.Extensions;

public static class FlexDirectionExtensions
{
    public static bool IsRow(this FlexDirection flexDirection)
    {
        return flexDirection == FlexDirection.Row || flexDirection == FlexDirection.RowReverse;
    }

    public static bool IsColumn(this FlexDirection flexDirection)
    {
        return flexDirection == FlexDirection.Column || flexDirection == FlexDirection.ColumnReverse;
    }

    public static FlexDirection ResolveDirection(this FlexDirection flexDirection, Direction direction)
    {
        if (direction == Direction.RTL)
        {
            if (flexDirection == FlexDirection.Row)
                return FlexDirection.RowReverse;
            else if (flexDirection == FlexDirection.RowReverse)
            {
                return FlexDirection.Row;
            }
        }

        return flexDirection;
    }

    public static FlexDirection ResolveCrossDirection(this FlexDirection flexDirection, Direction direction)
    {
        return flexDirection.IsColumn()
            ? FlexDirection.Row.ResolveDirection(direction)
            : FlexDirection.Column;
    }

    public static PhysicalEdge FlexStartEdge(this FlexDirection flexDirection)
    {
        return flexDirection switch
        {
            FlexDirection.Column => PhysicalEdge.Top,
            FlexDirection.ColumnReverse => PhysicalEdge.Bottom,
            FlexDirection.Row => PhysicalEdge.Left,
            FlexDirection.RowReverse => PhysicalEdge.Right,
            _ => throw new Exception("Invalid FlexDirection"),
        };
    }

    public static PhysicalEdge FlexEndEdge(this FlexDirection flexDirection)
    {
        return flexDirection switch
        {
            FlexDirection.Column => PhysicalEdge.Bottom,
            FlexDirection.ColumnReverse => PhysicalEdge.Top,
            FlexDirection.Row => PhysicalEdge.Right,
            FlexDirection.RowReverse => PhysicalEdge.Left,
            _ => throw new Exception("Invalid FlexDirection"),
        };
    }

    public static PhysicalEdge InlineStartEdge(this FlexDirection flexDirection, Direction direction)
    {
        if (flexDirection.IsRow())
            return direction == Direction.RTL ? PhysicalEdge.Right : PhysicalEdge.Left;

        return PhysicalEdge.Top;
    }

    public static PhysicalEdge InlineEndEdge(this FlexDirection flexDirection, Direction direction)
    {
        if (flexDirection.IsRow())
            return direction == Direction.RTL ? PhysicalEdge.Left : PhysicalEdge.Right;

        return PhysicalEdge.Bottom;
    }

    public static Dimension Dimension(this FlexDirection flexDirection)
    {
        return flexDirection switch
        {
            FlexDirection.Column => Enums.Dimension.Height,
            FlexDirection.ColumnReverse => Enums.Dimension.Height,
            FlexDirection.Row => Enums.Dimension.Width,
            FlexDirection.RowReverse => Enums.Dimension.Width,
            _ => throw new Exception("Invalid FlexDirection"),
        };
    }

    public static bool NeedsTrailingPosition(this FlexDirection axis)
    {
        return axis == FlexDirection.RowReverse || axis == FlexDirection.ColumnReverse;
    }
}
