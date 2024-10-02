using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class FlexGapTest
{
    [Fact]
    public void GapNegativeValue()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Gap = -20;
        root.Height = 200;

        var root_child0 = new Node
        {
            Width = 20
        };
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Width = 20
        };
        root.Insert(1, root_child1);

        var root_child2 = new Node
        {
            Width = 20
        };
        root.Insert(2, root_child2);

        var root_child3 = new Node
        {
            Width = 20
        };
        root.Insert(3, root_child3);
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(80, root.Layout.Width);
        Assert.Equal(200, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(20, root_child0.Layout.Width);
        Assert.Equal(200, root_child0.Layout.Height);

        Assert.Equal(20, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);
        Assert.Equal(20, root_child1.Layout.Width);
        Assert.Equal(200, root_child1.Layout.Height);

        Assert.Equal(40, root_child2.Layout.PositionLeft);
        Assert.Equal(0, root_child2.Layout.PositionTop);
        Assert.Equal(20, root_child2.Layout.Width);
        Assert.Equal(200, root_child2.Layout.Height);

        Assert.Equal(60, root_child3.Layout.PositionLeft);
        Assert.Equal(0, root_child3.Layout.PositionTop);
        Assert.Equal(20, root_child3.Layout.Width);
        Assert.Equal(200, root_child3.Layout.Height);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(80, root.Layout.Width);
        Assert.Equal(200, root.Layout.Height);

        Assert.Equal(60, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(20, root_child0.Layout.Width);
        Assert.Equal(200, root_child0.Layout.Height);

        Assert.Equal(40, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);
        Assert.Equal(20, root_child1.Layout.Width);
        Assert.Equal(200, root_child1.Layout.Height);

        Assert.Equal(20, root_child2.Layout.PositionLeft);
        Assert.Equal(0, root_child2.Layout.PositionTop);
        Assert.Equal(20, root_child2.Layout.Width);
        Assert.Equal(200, root_child2.Layout.Height);

        Assert.Equal(0, root_child3.Layout.PositionLeft);
        Assert.Equal(0, root_child3.Layout.PositionTop);
        Assert.Equal(20, root_child3.Layout.Width);
        Assert.Equal(200, root_child3.Layout.Height);
    }
}
