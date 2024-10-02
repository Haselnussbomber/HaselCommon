using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class AspectRatioTest
{
    [Fact]
    public void AspectRatioCrossDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioMainDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioBothDimensionsDefinedRow()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 100,
            Height = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioBothDimensionsDefinedColumn()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 100,
            Height = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioAlignStretch()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioFlexGrow()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioFlexShrink()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 150,
            FlexShrink = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioFlexShrink_2()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = StyleLength.Percent(100),
            FlexShrink = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Height = StyleLength.Percent(100),
            FlexShrink = 1,
            AspectRatio = 1
        };
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);

        Assert.Equal(0, root_child1.ComputedLeft);
        Assert.Equal(50, root_child1.ComputedTop);
        Assert.Equal(50, root_child1.ComputedWidth);
        Assert.Equal(50, root_child1.ComputedHeight);
    }

    [Fact]
    public void AspectRatioBasis()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexBasis = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioAbsoluteLayoutWidthDefined()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            PositionType = PositionType.Absolute,
            PositionLeft = 0,
            PositionTop = 0,
            Width = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioAbsoluteLayoutHeightDefined()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            PositionType = PositionType.Absolute,
            PositionLeft = 0,
            PositionTop = 0,
            Height = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWithMaxCrossDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            MaxWidth = 40,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(40, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWithMaxMainDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            MaxHeight = 40,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(40, root_child0.ComputedWidth);
        Assert.Equal(40, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWithMinCrossDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 30,
            MinWidth = 40,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(40, root_child0.ComputedWidth);
        Assert.Equal(30, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWithMinMainDefined()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 30,
            MinHeight = 40,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(40, root_child0.ComputedWidth);
        Assert.Equal(40, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioDoubleCross()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            AspectRatio = 2
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioHalfCross()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 100,
            AspectRatio = 0.5f
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioDoubleMain()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            AspectRatio = 0.5f
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioHalfMain()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 100,
            AspectRatio = 2
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    public class MeasureFuncNode : Node
    {
        public MeasureFuncNode()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(
                widthMode == MeasureMode.Exactly ? width : 50,
                heightMode == MeasureMode.Exactly ? height : 50
            );
        }
    }

    [Fact]
    public void AspectRatioWithMeasureFunc()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureFuncNode
        {
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWidthHeightFlexGrowRow()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 200;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 50,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWidthHeightFlexGrowColumn()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 200;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 50,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioHeightAsFlexBasis()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.FlexDirection = FlexDirection.Row;
        root.Width = 200;
        root.Height = 200;

        var root_child0 = new Node
        {
            Height = 50,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Height = 100,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(75, root_child0.ComputedWidth);
        Assert.Equal(75, root_child0.ComputedHeight);

        Assert.Equal(75, root_child1.ComputedLeft);
        Assert.Equal(0, root_child1.ComputedTop);
        Assert.Equal(125, root_child1.ComputedWidth);
        Assert.Equal(125, root_child1.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWidthAsFlexBasis()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 200;
        root.Height = 200;

        var root_child0 = new Node
        {
            Width = 50,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Width = 100,
            FlexGrow = 1,
            AspectRatio = 1
        };
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(75, root_child0.ComputedWidth);
        Assert.Equal(75, root_child0.ComputedHeight);

        Assert.Equal(0, root_child1.ComputedLeft);
        Assert.Equal(75, root_child1.ComputedTop);
        Assert.Equal(125, root_child1.ComputedWidth);
        Assert.Equal(125, root_child1.ComputedHeight);
    }

    [Fact]
    public void AspectRatioOverridesFlexGrowRow()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            FlexGrow = 1,
            AspectRatio = 0.5f
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(200, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioOverridesFlexGrowColumn()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            FlexGrow = 1,
            AspectRatio = 2
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(200, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioLeftRightAbsolute()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            PositionType = PositionType.Absolute,
            PositionLeft = 10,
            PositionTop = 10,
            PositionRight = 10,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedTop);
        Assert.Equal(80, root_child0.ComputedWidth);
        Assert.Equal(80, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioTopBottomAbsolute()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            PositionType = PositionType.Absolute,
            PositionLeft = 10,
            PositionTop = 10,
            PositionBottom = 10,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedTop);
        Assert.Equal(80, root_child0.ComputedWidth);
        Assert.Equal(80, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioWidthOverridesAlignStretchRow()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioHeightOverridesAlignStretchColumn()
    {
        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            AspectRatio = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioAllowChildOverflowParentSize()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;

        var root_child0 = new Node
        {
            Height = 50,
            AspectRatio = 4
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(50, root.ComputedHeight);

        Assert.Equal(200, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioDefinedMainWithMargin()
    {
        using var root = new Node();
        root.AlignItems = Align.Center;
        root.JustifyContent = Justify.Center;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Height = 50,
            AspectRatio = 1,
            MarginLeft = 10,
            MarginRight = 10
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(100, root.ComputedHeight);

        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioDefinedCrossWithMargin()
    {
        using var root = new Node();
        root.AlignItems = Align.Center;
        root.JustifyContent = Justify.Center;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            AspectRatio = 1,
            MarginLeft = 10,
            MarginRight = 10
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(100, root.ComputedHeight);

        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioDefinedCrossWithMainMargin()
    {
        using var root = new Node();
        root.AlignItems = Align.Center;
        root.JustifyContent = Justify.Center;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            AspectRatio = 1,
            MarginTop = 10,
            MarginBottom = 10
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(100, root.ComputedHeight);

        Assert.Equal(50, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioShouldPreferExplicitHeight()
    {
        var config = new Config() { UseWebDefaults = true };

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Column;

        var root_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Column
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Column,
            Height = 100,
            AspectRatio = 2
        };
        root_child0.Insert(0, root_child0_child0);

        root.CalculateLayout(100, 200, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(200, root.ComputedHeight);

        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);

        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(100, root_child0_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioShouldPreferExplicitWidth()
    {
        var config = new Config() { UseWebDefaults = true };

        using var root = new Node() { Config = config };
        root.FlexDirection = FlexDirection.Row;

        var root_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Row
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Row,
            Width = 100,
            AspectRatio = 0.5f
        };
        root_child0.Insert(0, root_child0_child0);

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.Equal(200, root.ComputedWidth);
        Assert.Equal(100, root.ComputedHeight);

        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);

        Assert.Equal(100, root_child0_child0.ComputedWidth);
        Assert.Equal(200, root_child0_child0.ComputedHeight);
    }

    [Fact]
    public void AspectRatioShouldPreferFlexedDimension()
    {
        var config = new Config() { UseWebDefaults = true };

        using var root = new Node() { Config = config };

        var root_child0 = new Node
        {
            Config = config,
            FlexDirection = FlexDirection.Column,
            AspectRatio = 2,
            FlexGrow = 1
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            AspectRatio = 4,
            FlexGrow = 1
        };
        root_child0.Insert(0, root_child0_child0);

        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(100, root.ComputedWidth);
        Assert.Equal(100, root.ComputedHeight);

        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(50, root_child0.ComputedHeight);

        Assert.Equal(200, root_child0_child0.ComputedWidth);
        Assert.Equal(50, root_child0_child0.ComputedHeight);
    }
}
