using System.Numerics;
using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Tests.Yoga;

public class AlignBaselineTest
{
    private class BaselineNode : Node
    {
        public override float Baseline(float width, float height)
        {
            return height / 2;
        }
    }

    private class Measure1Node : Node
    {
        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(42, 50);
        }
    }

    private class Measure2Node : Node
    {
        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(279, 126);
        }
    }

    private static Node createNode(
        Config config,
        FlexDirection direction,
        int width,
        int height,
        bool alignBaseline)
    {
        var node = new Node
        {
            Config = config,
            FlexDirection = direction
        };
        if (alignBaseline)
        {
            node.AlignItems = Align.Baseline;
        }
        node.Width = width;
        node.Height = height;
        return node;
    }

    private static BaselineNode createBaselineNode(
        Config config,
        FlexDirection direction,
        int width,
        int height,
        bool alignBaseline)
    {
        var node = new BaselineNode
        {
            Config = config,
            FlexDirection = direction
        };
        if (alignBaseline)
        {
            node.AlignItems = Align.Baseline;
        }
        node.Width = width;
        node.Height = height;
        return node;
    }

    // Test case for bug in T32999822
    [Fact]
    public void AlignBaselineParentHtNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignContent = Align.Stretch;
        root.AlignItems = Align.Baseline;
        root.Width = 340;
        root.MaxHeight = 170;
        root.MinHeight = 0;

        var root_child0 = new Measure1Node
        {
            Config = config,
            FlexGrow = 0,
            FlexShrink = 1,
            HasMeasureFunc = true
        };
        root.Add(root_child0);

        var root_child1 = new Measure2Node
        {
            Config = config,
            FlexGrow = 0,
            FlexShrink = 1,
            HasMeasureFunc = true
        };
        root.Add(root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(340, root.ComputedWidth);
        Assert.Equal(126, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(42, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
        Assert.Equal(76, root_child0.ComputedTop);

        Assert.Equal(42, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);
        Assert.Equal(279, root_child1.ComputedWidth);
        Assert.Equal(126, root_child1.ComputedHeight);
    }

    [Fact]
    public void AlignBaselineWithNoParentHt()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 150;

        using var root_child0 = new Node() { Config = config };
        root_child0.Width = 50;
        root_child0.Height = 50;
        root.Insert(0, root_child0);

        using var root_child1 = new BaselineNode() { Config = config };
        root_child1.Width = 50;
        root_child1.Height = 40;
        root_child1.HasBaselineFunc = true;
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(150, root.ComputedWidth);
        Assert.Equal(70, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);

        Assert.Equal(50, root_child1.ComputedLeft);
        Assert.Equal(30, root_child1.ComputedTop);
        Assert.Equal(50, root_child1.ComputedWidth);
        Assert.Equal(40, root_child1.ComputedHeight);
    }

    [Fact]
    public void AlignBaselineWithNoBaselineFuncAndNoParentHt()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 150;

        using var root_child0 = new Node() { Config = config };
        root_child0.Width = 50;
        root_child0.Height = 80;
        root.Insert(0, root_child0);

        using var root_child1 = new Node() { Config = config };
        root_child1.Width = 50;
        root_child1.Height = 50;
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.ComputedLeft);
        Assert.Equal(0, root.ComputedTop);
        Assert.Equal(150, root.ComputedWidth);
        Assert.Equal(80, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(80, root_child0.ComputedHeight);

        Assert.Equal(50, root_child1.ComputedLeft);
        Assert.Equal(30, root_child1.ComputedTop);
        Assert.Equal(50, root_child1.ComputedWidth);
        Assert.Equal(50, root_child1.ComputedHeight);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithPaddingInColumnAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.PaddingLeft = 100;
        root_child1_child1.PaddingRight = 100;
        root_child1_child1.PaddingTop = 100;
        root_child1_child1.PaddingBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentWithPaddingUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root_child1.PaddingLeft = 100;
        root_child1.PaddingRight = 100;
        root_child1.PaddingTop = 100;
        root_child1.PaddingBottom = 100;
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);

        Assert.Equal(100, root_child1_child0.ComputedLeft);
        Assert.Equal(100, root_child1_child0.ComputedTop);

        Assert.Equal(100, root_child1_child1.ComputedLeft);
        Assert.Equal(400, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentWithMarginUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root_child1.MarginLeft = 100;
        root_child1.MarginRight = 100;
        root_child1.MarginTop = 100;
        root_child1.MarginBottom = 100;
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(600, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithMarginInColumnAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.MarginLeft = 100;
        root_child1_child1.MarginRight = 100;
        root_child1_child1.MarginTop = 100;
        root_child1_child1.MarginBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(100, root_child1_child1.ComputedLeft);
        Assert.Equal(400, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(500, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithPaddingInRowAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.PaddingLeft = 100;
        root_child1_child1.PaddingRight = 100;
        root_child1_child1.PaddingTop = 100;
        root_child1_child1.PaddingBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(500, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithMarginInRowAsReference()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.MarginLeft = 100;
        root_child1_child1.MarginRight = 100;
        root_child1_child1.MarginTop = 100;
        root_child1_child1.MarginBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(600, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithNoBaselineFunc()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(100, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithNoBaselineFunc()
    {
        var config = new Config();

        using var root = createNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(500, root_child1_child1.ComputedLeft);
        Assert.Equal(100, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Column,
            Width = 500
        };
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(800, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(100, root_child1.ComputedTop);
        Assert.Equal(700, root_child1.ComputedHeight);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Row,
            Width = 500
        };
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createBaselineNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(900, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(400, root_child1.ComputedTop);
        Assert.Equal(500, root_child1.ComputedHeight);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(500, root_child1_child1.ComputedLeft);
        Assert.Equal(0, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithNoBaselineFuncAndHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Column,
            Width = 500
        };
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(700, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(100, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);
        Assert.Equal(700, root_child1.ComputedHeight);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(0, root_child1_child1.ComputedLeft);
        Assert.Equal(300, root_child1_child1.ComputedTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithNoBaselineFuncAndHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Row,
            Width = 500
        };
        root.Insert(1, root_child1);

        var root_child1_child0 = createNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(700, root.ComputedHeight);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);

        Assert.Equal(500, root_child1.ComputedLeft);
        Assert.Equal(200, root_child1.ComputedTop);
        Assert.Equal(500, root_child1.ComputedHeight);

        Assert.Equal(0, root_child1_child0.ComputedLeft);
        Assert.Equal(0, root_child1_child0.ComputedTop);

        Assert.Equal(500, root_child1_child1.ComputedLeft);
        Assert.Equal(0, root_child1_child1.ComputedTop);
    }
}
