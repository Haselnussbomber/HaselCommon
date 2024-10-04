using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Tests.Yoga;

public class ComputedPaddingTest
{
    [Fact]
    public void ComputedLayoutPadding()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;
        root.PaddingStart = StyleLength.Percent(10);

        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(10, root.ComputedPaddingLeft);
        Assert.Equal(0, root.ComputedPaddingRight);

        root.CalculateLayout(100, 100, Direction.RTL);

        Assert.Equal(0, root.ComputedPaddingLeft);
        Assert.Equal(10, root.ComputedPaddingRight);
    }

    [Fact]
    public void PaddingSideOverridesHorizontalAndVertical()
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
                    root.PaddingVertical = 10;
                else
                    root.PaddingHorizontal = 10;

                switch (edge)
                {
                    case Edge.Top: root.PaddingTop = edgeValue; break;
                    case Edge.Bottom: root.PaddingBottom = edgeValue; break;
                    case Edge.Start: root.PaddingStart = edgeValue; break;
                    case Edge.End: root.PaddingEnd = edgeValue; break;
                    case Edge.Left: root.PaddingLeft = edgeValue; break;
                    case Edge.Right: root.PaddingRight = edgeValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                switch (edge)
                {
                    case Edge.Top: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Top)); break;
                    case Edge.Bottom: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Bottom)); break;
                    case Edge.Start: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Start)); break;
                    case Edge.End: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.End)); break;
                    case Edge.Left: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Left)); break;
                    case Edge.Right: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Right)); break;
                }
            }
        }
    }

    [Fact]
    public void PaddingSideOverridesAll()
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
                root.PaddingAll = 10;

                switch (edge)
                {
                    case Edge.Top: root.PaddingTop = edgeValue; break;
                    case Edge.Bottom: root.PaddingBottom = edgeValue; break;
                    case Edge.Start: root.PaddingStart = edgeValue; break;
                    case Edge.End: root.PaddingEnd = edgeValue; break;
                    case Edge.Left: root.PaddingLeft = edgeValue; break;
                    case Edge.Right: root.PaddingRight = edgeValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                switch (edge)
                {
                    case Edge.Top: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Top)); break;
                    case Edge.Bottom: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Bottom)); break;
                    case Edge.Start: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Start)); break;
                    case Edge.End: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.End)); break;
                    case Edge.Left: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Left)); break;
                    case Edge.Right: Assert.Equal(edgeValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Right)); break;
                }
            }
        }
    }

    [Fact]
    public void PaddingHorizontalAndVerticalOverridesAll()
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
                root.PaddingAll = 10;

                switch (direction)
                {
                    case Edge.Horizontal: root.PaddingHorizontal = directionValue; break;
                    case Edge.Vertical: root.PaddingVertical = directionValue; break;
                }

                root.CalculateLayout(100, 100, Direction.LTR);

                if (direction == Edge.Vertical)
                {
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Top));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Bottom));
                }
                else
                {
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Start));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.End));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Left));
                    Assert.Equal(directionValue, root.GetResolvedLayoutProperty(root._layout.Padding, Edge.Right));
                }
            }
        }
    }
}
