using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class AspectRatioTest
{
    [Fact]
    public void AspectRatioDoesNotStretchCrossAxisDim()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.PositionType = PositionType.Absolute;
        root.Width = 300;
        root.Height = 300;

        var root_child0 = new Node
        {
            Config = config,
            Overflow = Overflow.Scroll,
            FlexGrow = 1,
            FlexShrink = 1,
            FlexBasis = StyleLength.Percent(0)
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Row
        };
        root_child0.Insert(0, root_child0_child0);

        var root_child0_child0_child0 = new Node
        {
            Config = config,
            FlexGrow = 2,
            FlexShrink = 1,
            FlexBasis = StyleLength.Percent(0),
            AspectRatio = 1 / 1
        };
        root_child0_child0.Insert(0, root_child0_child0_child0);

        var root_child0_child0_child1 = new Node
        {
            Config = config,
            Width = 5
        };
        root_child0_child0.Insert(1, root_child0_child0_child1);

        var root_child0_child0_child2 = new Node
        {
            Config = config,
            FlexGrow = 1,
            FlexShrink = 1,
            FlexBasis = StyleLength.Percent(0)
        };
        root_child0_child0.Insert(2, root_child0_child0_child2);

        var root_child0_child0_child2_child0 = new Node
        {
            Config = config,
            FlexGrow = 1,
            FlexShrink = 1,
            FlexBasis = StyleLength.Percent(0),
            AspectRatio = 1 / 1
        };
        root_child0_child0_child2.Insert(0, root_child0_child0_child2_child0);

        var root_child0_child0_child2_child0_child0 = new Node
        {
            Config = config,
            Width = 5
        };
        root_child0_child0_child2_child0.Insert(0, root_child0_child0_child2_child0_child0);

        var root_child0_child0_child2_child0_child1 = new Node
        {
            Config = config,
            FlexGrow = 1,
            FlexShrink = 1,
            FlexBasis = StyleLength.Percent(0),
            AspectRatio = 1 / 1
        };
        root_child0_child0_child2_child0.Insert(1, root_child0_child0_child2_child0_child1);
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(300, root.ComputedWidth);
        Assert.Equal(300, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(300, root_child0.ComputedWidth);
        Assert.Equal(300, root_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0.ComputedTop);
        Assert.Equal(300, root_child0_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child0.ComputedTop);
        Assert.Equal(197, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child0.ComputedHeight);

        Assert.Equal(197, root_child0_child0_child1.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child1.ComputedTop);
        Assert.Equal(5, root_child0_child0_child1.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child1.ComputedHeight);

        Assert.Equal(202, root_child0_child0_child2.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.ComputedTop);
        Assert.Equal(5, root_child0_child0_child2_child0_child0.ComputedWidth);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2_child0_child1.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child1.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2_child0_child1.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2_child0_child1.ComputedHeight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(300, root.ComputedWidth);
        Assert.Equal(300, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(300, root_child0.ComputedWidth);
        Assert.Equal(300, root_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0.ComputedTop);
        Assert.Equal(300, root_child0_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0.ComputedHeight);

        Assert.Equal(103, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child0.ComputedTop);
        Assert.Equal(197, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child0.ComputedHeight);

        Assert.Equal(98, root_child0_child0_child1.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child1.ComputedTop);
        Assert.Equal(5, root_child0_child0_child1.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child1.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2_child0.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2_child0.ComputedHeight);

        Assert.Equal(93, root_child0_child0_child2_child0_child0.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.ComputedTop);
        Assert.Equal(5, root_child0_child0_child2_child0_child0.ComputedWidth);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.ComputedHeight);

        Assert.Equal(0, root_child0_child0_child2_child0_child1.ComputedLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child1.ComputedTop);
        Assert.Equal(98, root_child0_child0_child2_child0_child1.ComputedWidth);
        Assert.Equal(197, root_child0_child0_child2_child0_child1.ComputedHeight);
    }

    [Fact]
    public void ZeroAspectRatioBehavesLikeAuto()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.PositionType = PositionType.Absolute;
        root.Width = 300;
        root.Height = 300;

        var root_child0 = new Node
        {
            Config = config,
            Width = 50,
            AspectRatio = 0 / 1
        };
        root.Insert(0, root_child0);
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(300, root.ComputedWidth);
        Assert.Equal(300, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(0, root_child0.ComputedHeight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(300, root.ComputedWidth);
        Assert.Equal(300, root.ComputedHeight);

        Assert.Equal(250, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(0, root_child0.ComputedHeight);
    }

}
