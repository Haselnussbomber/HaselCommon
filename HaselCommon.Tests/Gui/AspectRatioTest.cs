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

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(300, root.Layout.Width);
        Assert.Equal(300, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(300, root_child0.Layout.Width);
        Assert.Equal(300, root_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0.Layout.PositionTop);
        Assert.Equal(300, root_child0_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child0.Layout.PositionTop);
        Assert.Equal(197, root_child0_child0_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0_child0.Layout.Height);

        Assert.Equal(197, root_child0_child0_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child1.Layout.PositionTop);
        Assert.Equal(5, root_child0_child0_child1.Layout.Width);
        Assert.Equal(197, root_child0_child0_child1.Layout.Height);

        Assert.Equal(202, root_child0_child0_child2.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.Layout.PositionTop);
        Assert.Equal(5, root_child0_child0_child2_child0_child0.Layout.Width);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2_child0_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child1.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2_child0_child1.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2_child0_child1.Layout.Height);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(300, root.Layout.Width);
        Assert.Equal(300, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(300, root_child0.Layout.Width);
        Assert.Equal(300, root_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0.Layout.PositionTop);
        Assert.Equal(300, root_child0_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0.Layout.Height);

        Assert.Equal(103, root_child0_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child0.Layout.PositionTop);
        Assert.Equal(197, root_child0_child0_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0_child0.Layout.Height);

        Assert.Equal(98, root_child0_child0_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child1.Layout.PositionTop);
        Assert.Equal(5, root_child0_child0_child1.Layout.Width);
        Assert.Equal(197, root_child0_child0_child1.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2_child0.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2_child0.Layout.Height);

        Assert.Equal(93, root_child0_child0_child2_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.Layout.PositionTop);
        Assert.Equal(5, root_child0_child0_child2_child0_child0.Layout.Width);
        Assert.Equal(0, root_child0_child0_child2_child0_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child2_child0_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child2_child0_child1.Layout.PositionTop);
        Assert.Equal(98, root_child0_child0_child2_child0_child1.Layout.Width);
        Assert.Equal(197, root_child0_child0_child2_child0_child1.Layout.Height);
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

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(300, root.Layout.Width);
        Assert.Equal(300, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(50, root_child0.Layout.Width);
        Assert.Equal(0, root_child0.Layout.Height);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(300, root.Layout.Width);
        Assert.Equal(300, root.Layout.Height);

        Assert.Equal(250, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(50, root_child0.Layout.Width);
        Assert.Equal(0, root_child0.Layout.Height);
    }

}
