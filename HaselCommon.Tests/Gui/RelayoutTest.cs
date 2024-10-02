using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class RelayoutTest
{
    [Fact]
    public void DontCacheComputedFlexBasisBetweenLayouts()
    {
        var config = new Config
        {
            ExperimentalFeatures = ExperimentalFeature.WebFlexBasis
        };

        using var root = new Node() { Config = config };
        root.Height = StyleLength.Percent(100);
        root.Width = StyleLength.Percent(100);

        var root_child0 = new Node
        {
            Config = config,
            FlexBasis = StyleLength.Percent(100)
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(100, float.NaN, Direction.LTR);
        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void RecalculateResolvedDimonsionOnchange()
    {
        using var root = new Node();

        var root_child0 = new Node
        {
            MinHeight = 10,
            MaxHeight = 10
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(10, root_child0.ComputedHeight);

        root_child0.MinHeight = float.NaN;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedHeight);
    }

    [Fact]
    public void RelayoutContainingBlockSizeChanges()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.PositionType = PositionType.Absolute;

        var root_child0 = new Node
        {
            Config = config,
            PositionType = PositionType.Relative,
            MarginLeft = 4,
            MarginTop = 5,
            MarginRight = 9,
            MarginBottom = 1,
            PaddingLeft = 2,
            PaddingTop = 9,
            PaddingRight = 11,
            PaddingBottom = 13,
            BorderLeft = 5,
            BorderTop = 6,
            BorderRight = 7,
            BorderBottom = 8,
            Width = 500,
            Height = 500
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            PositionType = PositionType.Static,
            MarginLeft = 8,
            MarginTop = 6,
            MarginRight = 3,
            MarginBottom = 9,
            PaddingLeft = 1,
            PaddingTop = 7,
            PaddingRight = 9,
            PaddingBottom = 4,
            BorderLeft = 8,
            BorderTop = 10,
            BorderRight = 2,
            BorderBottom = 1,
            Width = 200,
            Height = 200
        };
        root_child0.Insert(0, root_child0_child0);

        var root_child0_child0_child0 = new Node
        {
            Config = config,
            PositionType = PositionType.Absolute,
            PositionLeft = 2,
            PositionRight = 12,
            MarginLeft = 9,
            MarginTop = 12,
            MarginRight = 4,
            MarginBottom = 7,
            PaddingLeft = 5,
            PaddingTop = 3,
            PaddingRight = 8,
            PaddingBottom = 10,
            BorderLeft = 2,
            BorderTop = 1,
            BorderRight = 5,
            BorderBottom = 9,
            Width = StyleLength.Percent(41),
            Height = StyleLength.Percent(63)
        };
        root_child0_child0.Insert(0, root_child0_child0_child0);
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(513, root.ComputedWidth);
        Assert.Equal(506, root.ComputedHeight);

        Assert.Equal(4, root_child0.ComputedLeft);
        Assert.Equal(5, root_child0.ComputedTop);
        Assert.Equal(500, root_child0.ComputedWidth);
        Assert.Equal(500, root_child0.ComputedHeight);

        Assert.Equal(15, root_child0_child0.ComputedLeft);
        Assert.Equal(21, root_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(200, root_child0_child0.ComputedHeight);

        Assert.Equal(1, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(29, root_child0_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(306, root_child0_child0_child0.ComputedHeight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(513, root.ComputedWidth);
        Assert.Equal(506, root.ComputedHeight);

        Assert.Equal(4, root_child0.ComputedLeft);
        Assert.Equal(5, root_child0.ComputedTop);
        Assert.Equal(500, root_child0.ComputedWidth);
        Assert.Equal(500, root_child0.ComputedHeight);

        Assert.Equal(279, root_child0_child0.ComputedLeft);
        Assert.Equal(21, root_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(200, root_child0_child0.ComputedHeight);

        Assert.Equal(-2, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(29, root_child0_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(306, root_child0_child0_child0.ComputedHeight);

        // Relayout starts here
        root_child0.Width = 456;
        root_child0.Height = 432;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(469, root.ComputedWidth);
        Assert.Equal(438, root.ComputedHeight);

        Assert.Equal(4, root_child0.ComputedLeft);
        Assert.Equal(5, root_child0.ComputedTop);
        Assert.Equal(456, root_child0.ComputedWidth);
        Assert.Equal(432, root_child0.ComputedHeight);

        Assert.Equal(15, root_child0_child0.ComputedLeft);
        Assert.Equal(21, root_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(200, root_child0_child0.ComputedHeight);

        Assert.Equal(1, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(29, root_child0_child0_child0.ComputedTop);
        Assert.Equal(182, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(263, root_child0_child0_child0.ComputedHeight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(469, root.ComputedWidth);
        Assert.Equal(438, root.ComputedHeight);

        Assert.Equal(4, root_child0.ComputedLeft);
        Assert.Equal(5, root_child0.ComputedTop);
        Assert.Equal(456, root_child0.ComputedWidth);
        Assert.Equal(432, root_child0.ComputedHeight);

        Assert.Equal(235, root_child0_child0.ComputedLeft);
        Assert.Equal(21, root_child0_child0.ComputedTop);
        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(200, root_child0_child0.ComputedHeight);

        Assert.Equal(16, root_child0_child0_child0.ComputedLeft);
        Assert.Equal(29, root_child0_child0_child0.ComputedTop);
        Assert.Equal(182, root_child0_child0_child0.ComputedWidth);
        Assert.Equal(263, root_child0_child0_child0.ComputedHeight);
    }

    [Fact]
    public void HasNewLayoutFlagSetStatic()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            PositionType = PositionType.Static,
            Width = 10,
            Height = 10
        };
        root.Insert(0, root_child0);

        var root_child0_child1 = new Node
        {
            PositionType = PositionType.Absolute,
            Width = 5,
            Height = 5
        };
        root_child0.Insert(0, root_child0_child1);

        var root_child0_child0 = new Node
        {
            PositionType = PositionType.Static,
            Width = 5,
            Height = 5
        };
        root_child0.Insert(1, root_child0_child0);

        var root_child0_child0_child0 = new Node
        {
            PositionType = PositionType.Absolute,
            Width = StyleLength.Percent(1),
            Height = 1
        };
        root_child0_child0.Insert(0, root_child0_child0_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        root.HasNewLayout = false;
        root_child0.HasNewLayout = false;
        root_child0_child0.HasNewLayout = false;
        root_child0_child0_child0.HasNewLayout = false;

        root.Width = 110;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.True(root.HasNewLayout);
        Assert.True(root_child0.HasNewLayout);
        Assert.True(root_child0_child0.HasNewLayout);
        Assert.True(root_child0_child0_child0.HasNewLayout);
    }
}
