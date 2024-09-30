/*
using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class AlignBaselineTest
{
    private static float _baselineFunc(Node node, float width, float height)
    {
        return height / 2;
    }

    private static Vector2 _measure1(Node node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return new Vector2(42, 50);
    }

    private static Vector2 _measure2(Node node, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
    {
        return new Vector2(279, 126);
    }

    private static Node createYGNode(
        Config config,
        FlexDirection direction,
        int width,
        int height,
        bool alignBaseline)
    {
        var node = new Node() { Config = config };
        node.FlexDirection = direction;
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

        var root_child0 = new Node() { Config = config };
        root_child0.FlexGrow = 0;
        root_child0.FlexShrink = 1;
        root_child0.MeasureFunc = _measure1;
        root_child0.HasMeasureFunc = true;
        root.Add(root_child0);

        var root_child1 = new Node() { Config = config };
        root_child1.FlexGrow = 0;
        root_child1.FlexShrink = 1;
        root_child1.MeasureFunc = _measure2;
        root_child1.HasMeasureFunc = true;
        root.Add(root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(340, root.Layout.Width);
        Assert.Equal(126, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(42, root_child0.Layout.Width);
        Assert.Equal(50, root_child0.Layout.Height);
        Assert.Equal(76, root_child0.Layout.PositionTop);

        Assert.Equal(42, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);
        Assert.Equal(279, root_child1.Layout.Width);
        Assert.Equal(126, root_child1.Layout.Height);
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

        using var root_child1 = new Node() { Config = config };
        root_child1.Width = 50;
        root_child1.Height = 40;
        root_child1.BaselineFunc = _baselineFunc;
        root_child1.HasBaselineFunc = true;
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(150, root.Layout.Width);
        Assert.Equal(70, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(50, root_child0.Layout.Width);
        Assert.Equal(50, root_child0.Layout.Height);

        Assert.Equal(50, root_child1.Layout.PositionLeft);
        Assert.Equal(30, root_child1.Layout.PositionTop);
        Assert.Equal(50, root_child1.Layout.Width);
        Assert.Equal(40, root_child1.Layout.Height);
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

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(150, root.Layout.Width);
        Assert.Equal(80, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(50, root_child0.Layout.Width);
        Assert.Equal(80, root_child0.Layout.Height);

        Assert.Equal(50, root_child1.Layout.PositionLeft);
        Assert.Equal(30, root_child1.Layout.PositionTop);
        Assert.Equal(50, root_child1.Layout.Width);
        Assert.Equal(50, root_child1.Layout.Height);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithPaddingInColumnAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.PaddingLeft = 100;
        root_child1_child1.PaddingRight = 100;
        root_child1_child1.PaddingTop = 100;
        root_child1_child1.PaddingBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentWithPaddingUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root_child1.PaddingLeft = 100;
        root_child1.PaddingRight = 100;
        root_child1.PaddingTop = 100;
        root_child1.PaddingBottom = 100;
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);

        Assert.Equal(100, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(100, root_child1_child0.Layout.PositionTop);

        Assert.Equal(100, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(400, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentWithMarginUsingChildInColumnAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root_child1.MarginLeft = 100;
        root_child1.MarginRight = 100;
        root_child1.MarginTop = 100;
        root_child1.MarginBottom = 100;
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(600, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithMarginInColumnAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.MarginLeft = 100;
        root_child1_child1.MarginRight = 100;
        root_child1_child1.MarginTop = 100;
        root_child1_child1.MarginBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(100, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(400, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        using var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        using var root_child1 = createYGNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        using var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        using var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithPaddingInRowAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createYGNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.PaddingLeft = 100;
        root_child1_child1.PaddingRight = 100;
        root_child1_child1.PaddingTop = 100;
        root_child1_child1.PaddingBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildWithMarginInRowAsReference()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createYGNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1_child1.MarginLeft = 100;
        root_child1_child1.MarginRight = 100;
        root_child1_child1.MarginTop = 100;
        root_child1_child1.MarginBottom = 100;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(600, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithNoBaselineFunc()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createYGNode(config, FlexDirection.Column, 500, 800, false);
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(100, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithNoBaselineFunc()
    {
        var config = new Config();

        using var root = createYGNode(config, FlexDirection.Row, 1000, 1000, true);

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = createYGNode(config, FlexDirection.Row, 500, 800, true);
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node() { Config = config };
        root_child1.FlexDirection = FlexDirection.Column;
        root_child1.Width = 500;
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(800, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(100, root_child1.Layout.PositionTop);
        Assert.Equal(700, root_child1.Layout.Height);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node() { Config = config };
        root_child1.FlexDirection = FlexDirection.Row;
        root_child1.Width = 500;
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.BaselineFunc = _baselineFunc;
        root_child1_child1.HasBaselineFunc = true;
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(900, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(400, root_child1.Layout.PositionTop);
        Assert.Equal(500, root_child1.Layout.Height);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInColumnAsReferenceWithNoBaselineFuncAndHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node() { Config = config };
        root_child1.FlexDirection = FlexDirection.Column;
        root_child1.Width = 500;
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 300, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(700, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(100, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1.Layout.PositionTop);
        Assert.Equal(700, root_child1.Layout.Height);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(0, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(300, root_child1_child1.Layout.PositionTop);
    }

    [Fact]
    public void AlignBaselineParentUsingChildInRowAsReferenceWithNoBaselineFuncAndHeightNotSpecified()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.Baseline;
        root.Width = 1000;

        var root_child0 = createYGNode(config, FlexDirection.Column, 500, 600, false);
        root.Insert(0, root_child0);

        var root_child1 = new Node() { Config = config };
        root_child1.FlexDirection = FlexDirection.Row;
        root_child1.Width = 500;
        root.Insert(1, root_child1);

        var root_child1_child0 = createYGNode(config, FlexDirection.Column, 500, 500, false);
        root_child1.Insert(0, root_child1_child0);

        var root_child1_child1 = createYGNode(config, FlexDirection.Column, 500, 400, false);
        root_child1_child1.IsReferenceBaseline = true;
        root_child1.Insert(1, root_child1_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(700, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1.Layout.PositionLeft);
        Assert.Equal(200, root_child1.Layout.PositionTop);
        Assert.Equal(500, root_child1.Layout.Height);

        Assert.Equal(0, root_child1_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child0.Layout.PositionTop);

        Assert.Equal(500, root_child1_child1.Layout.PositionLeft);
        Assert.Equal(0, root_child1_child1.Layout.PositionTop);
    }
}
*/
