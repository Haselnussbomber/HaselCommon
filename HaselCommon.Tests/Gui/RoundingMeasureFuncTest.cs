using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class RoundingMeasureFuncTest
{
    private class MeasureFloorNode : Node
    {
        public MeasureFloorNode() : base()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(10.2f, 10.2f);
        }
    }

    private class MeasureCeilNode : Node
    {
        public MeasureCeilNode() : base()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(10.5f, 10.5f);
        }
    }

    private class MeasureFractialNode : Node
    {
        public MeasureFractialNode() : base()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            return new Vector2(0.5f, 0.5f);
        }
    }

    [Fact]
    public void RoundingFeatureWithCustomMeasureFuncFloor()
    {
        var config = new Config();
        using var root = new Node() { Config = config };

        var root_child0 = new MeasureFloorNode() { Config = config };
        root.Insert(0, root_child0);

        config.PointScaleFactor = 0.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(10.2f, root_child0.Layout.Width);
        Assert.Equal(10.2f, root_child0.Layout.Height);

        config.PointScaleFactor = 1.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(11, root_child0.Layout.Width);
        Assert.Equal(11, root_child0.Layout.Height);

        config.PointScaleFactor = 2.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(10.5, root_child0.Layout.Width);
        Assert.Equal(10.5, root_child0.Layout.Height);

        config.PointScaleFactor = 4.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(10.25, root_child0.Layout.Width);
        Assert.Equal(10.25, root_child0.Layout.Height);

        config.PointScaleFactor = 1.0f / 3.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);

        Assert.Equal(12.0, root_child0.Layout.Width);
        Assert.Equal(12.0, root_child0.Layout.Height);
    }

    [Fact]
    public void RoundingFeatureWithCustomMeasureFuncCeil()
    {
        var config = new Config();
        using var root = new Node() { Config = config };

        var root_child0 = new MeasureCeilNode() { Config = config };
        root.Insert(0, root_child0);

        config.PointScaleFactor = 1.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(11, root_child0.Layout.Width);
        Assert.Equal(11, root_child0.Layout.Height);
    }

    [Fact]
    public void RoundingFeatureWithCustomMeasureAndFractialMatchingScale()
    {
        var config = new Config();
        using var root = new Node() { Config = config };
        root.PositionType = PositionType.Absolute;

        var root_child0 = new MeasureFractialNode() { Config = config };
        root_child0.PositionLeft = 73.625f;
        root_child0.PositionType = PositionType.Relative;
        root.Insert(0, root_child0);

        config.PointScaleFactor = 2.0f;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0.5, root_child0.Layout.Width);
        Assert.Equal(0.5, root_child0.Layout.Height);
        Assert.Equal(73.5, root_child0.Layout.PositionLeft);
    }
}
