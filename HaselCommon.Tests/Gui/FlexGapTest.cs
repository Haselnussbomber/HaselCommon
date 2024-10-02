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

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(80, root.ComputedWidth);
        Assert.Equal(200, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(20, root_child0.ComputedWidth);
        Assert.Equal(200, root_child0.ComputedHeight);

        Assert.Equal(20, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);
        Assert.Equal(20, root_child1.ComputedWidth);
        Assert.Equal(200, root_child1.ComputedHeight);

        Assert.Equal(40, root_child2.ComputedLeft);
        Assert.Equal(0, root_child2.ComputedTop);
        Assert.Equal(20, root_child2.ComputedWidth);
        Assert.Equal(200, root_child2.ComputedHeight);

        Assert.Equal(60, root_child3.ComputedLeft);
        Assert.Equal(0, root_child3.ComputedTop);
        Assert.Equal(20, root_child3.ComputedWidth);
        Assert.Equal(200, root_child3.ComputedHeight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(80, root.ComputedWidth);
        Assert.Equal(200, root.ComputedHeight);

        Assert.Equal(60, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(20, root_child0.ComputedWidth);
        Assert.Equal(200, root_child0.ComputedHeight);

        Assert.Equal(40, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);
        Assert.Equal(20, root_child1.ComputedWidth);
        Assert.Equal(200, root_child1.ComputedHeight);

        Assert.Equal(20, root_child2.ComputedLeft);
        Assert.Equal(0, root_child2.ComputedTop);
        Assert.Equal(20, root_child2.ComputedWidth);
        Assert.Equal(200, root_child2.ComputedHeight);

        Assert.Equal(0, root_child3.ComputedLeft);
        Assert.Equal(0, root_child3.ComputedTop);
        Assert.Equal(20, root_child3.ComputedWidth);
        Assert.Equal(200, root_child3.ComputedHeight);
    }
}
