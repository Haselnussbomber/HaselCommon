using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class RoundingFunctionTest
{
    [Fact]
    public void RoundingValue()
    {
        // Test that whole numbers are rounded to whole despite ceil/floor flags
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, false, true));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, false, true));
        // Same tests for negative numbers
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, false, true));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, false, true));

        // Test that numbers with fraction are rounded correctly accounting for
        // ceil/floor flags
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.01, 2.0, false, false));
        Assert.Equal(6.5, Node.RoundValueToPixelGrid(6.01, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.01, 2.0, false, true));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.99, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.99, 2.0, true, false));
        Assert.Equal(5.5, Node.RoundValueToPixelGrid(5.99, 2.0, false, true));
        // Same tests for negative numbers
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.01, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.01, 2.0, true, false));
        Assert.Equal(-6.5, Node.RoundValueToPixelGrid(-6.01, 2.0, false, true));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.99, 2.0, false, false));
        Assert.Equal(-5.5, Node.RoundValueToPixelGrid(-5.99, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.99, 2.0, false, true));
    }

    private class MeasureTextNode : Node
    {
        public MeasureTextNode()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(10, 10);
        }
    }

    // Regression test for https://github.com/facebook/yoga/issues/824
    [Fact]
    public void ConsistentRoundingDuringRepeatedLayouts()
    {
        var config = new Config
        {
            PointScaleFactor = 2
        };

        using var root = new Node() { Config = config };
        root.MarginTop = -1.49f;
        root.Width = 500;
        root.Height = 500;

        var node0 = new Node() { Config = config };
        root.Insert(0, node0);

        var node1 = new MeasureTextNode() { Config = config };
        node0.Insert(0, node1);

        for (var i = 0; i < 5; i++)
        {
            // Dirty the tree so YGRoundToPixelGrid runs again
            root.MarginLeft = i + 1;

            root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
            Assert.Equal(10, node1.ComputedHeight);
        }
    }

    [Fact]
    public void PerNodePointScaleFactor()
    {
        var config1 = new Config
        {
            PointScaleFactor = 2
        };

        var config2 = new Config
        {
            PointScaleFactor = 1
        };

        var config3 = new Config
        {
            PointScaleFactor = 0.5f
        };

        var root = new Node
        {
            Config = config1,
            Width = 11.5f,
            Height = 11.5f
        };

        var node0 = new Node
        {
            Config = config2,
            Width = 9.5f,
            Height = 9.5f
        };
        root.Insert(0, node0);

        var node1 = new Node
        {
            Config = config3,
            Width = 7,
            Height = 7
        };
        node0.Insert(0, node1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(11.5f, root.ComputedWidth);
        Assert.Equal(11.5f, root.ComputedHeight);

        Assert.Equal(10, node0.ComputedWidth);
        Assert.Equal(10, node0.ComputedHeight);

        Assert.Equal(8, node1.ComputedWidth);
        Assert.Equal(8, node1.ComputedHeight);
    }
}
