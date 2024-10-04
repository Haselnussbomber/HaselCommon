using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Tests.Yoga;

public class ComputedMarginTest
{
    [Fact]
    public void ComputedLayoutMargin()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;
        root.MarginStart = StyleLength.Percent(10);

        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(10, root.ComputedMarginLeft);
        Assert.Equal(0, root.ComputedMarginRight);

        root.CalculateLayout(100, 100, Direction.RTL);

        Assert.Equal(0, root.ComputedMarginLeft);
        Assert.Equal(10, root.ComputedMarginRight);
    }

    [Fact]
    public void MarginSideOverridesHorizontalAndVertical()
    {
        var edges = new Edge[] {
            Edge.Top,
            Edge.Bottom,
            Edge.Start,
            Edge.End,
            Edge.Left,
            Edge.Right,
        };

        for (float edgeValue = 0; edgeValue < 2; ++edgeValue)
        {
            foreach (var edge in edges)
            {
                using var root = new Node();
                root.Width = 100;
                root.Height = 100;

                if (edge == Edge.Top || edge == Edge.Bottom)
                    root.MarginVertical = 10;
                else
                    root.MarginHorizontal = 10;

                switch (edge)
                {
                    case Edge.Top: root.MarginTop = edgeValue; break;
                    case Edge.Bottom: root.MarginBottom = edgeValue; break;
                    case Edge.Start: root.MarginStart = edgeValue; break;
                    case Edge.End: root.MarginEnd = edgeValue; break;
                    case Edge.Left: root.MarginLeft = edgeValue; break;
                    case Edge.Right: root.MarginRight = edgeValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                switch (edge)
                {
                    case Edge.Top: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Top)); break;
                    case Edge.Bottom: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Bottom)); break;
                    case Edge.Start: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Start)); break;
                    case Edge.End: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.End)); break;
                    case Edge.Left: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Left)); break;
                    case Edge.Right: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Right)); break;
                }
            }
        }
    }

    [Fact]
    public void MarginSideOverridesAll()
    {
        var edges = new Edge[] {
            Edge.Top,
            Edge.Bottom,
            Edge.Start,
            Edge.End,
            Edge.Left,
            Edge.Right,
        };

        for (float edgeValue = 0; edgeValue < 2; ++edgeValue)
        {
            foreach (var edge in edges)
            {
                using var root = new Node();
                root.Width = 100;
                root.Height = 100;
                root.MarginAll = 10;

                switch (edge)
                {
                    case Edge.Top: root.MarginTop = edgeValue; break;
                    case Edge.Bottom: root.MarginBottom = edgeValue; break;
                    case Edge.Start: root.MarginStart = edgeValue; break;
                    case Edge.End: root.MarginEnd = edgeValue; break;
                    case Edge.Left: root.MarginLeft = edgeValue; break;
                    case Edge.Right: root.MarginRight = edgeValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                switch (edge)
                {
                    case Edge.Top: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Top)); break;
                    case Edge.Bottom: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Bottom)); break;
                    case Edge.Start: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Start)); break;
                    case Edge.End: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.End)); break;
                    case Edge.Left: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Left)); break;
                    case Edge.Right: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Right)); break;
                }
            }
        }
    }

    [Fact]
    public void MarginHorizontalAndVerticalOverridesAll()
    {
        var directions = new Edge[] {
            Edge.Horizontal,
            Edge.Vertical,
        };

        for (float directionValue = 0; directionValue < 2; ++directionValue)
        {
            foreach (var direction in directions)
            {
                using var root = new Node();
                root.Width = 100;
                root.Height = 100;
                root.MarginAll = 10;

                switch (direction)
                {
                    case Edge.Horizontal: root.MarginHorizontal = directionValue; break;
                    case Edge.Vertical: root.MarginVertical = directionValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                if (direction == Edge.Vertical)
                {
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Top));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Bottom));
                }
                else
                {
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Start));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.End));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Left));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Margin, Edge.Right));
                }
            }
        }
    }
}
