using System.Drawing;
using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class ScaleChangeTest
{
    [Fact]
    public void ScaleChangeInvalidatesLayout()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        config.PointScaleFactor = 1.0f;

        root.FlexDirection = FlexDirection.Row;
        root.Width = 50;
        root.Height = 50;

        var root_child0 = new Node
        {
            Config = config,
            FlexGrow = 1
        };
        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexGrow = 1
        };
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(25, root_child1.Layout.PositionLeft);

        config.PointScaleFactor = 1.5f;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(0, root_child0.Layout.PositionLeft);
        // Left should change due to pixel alignment of new scale factor
        Assert.Equal(25.333334f, root_child1.Layout.PositionLeft);
    }

    [Fact]
    public void ErrataConfigChangeRelayout()
    {
        var config = new Config
        {
            Errata = Errata.StretchFlexBasis
        };
        using var root = new Node() { Config = config };
        root.Width = 500;
        root.Height = 500;

        var root_child0 = new Node
        {
            Config = config,
            AlignItems = Align.FlexStart
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Node
        {
            Config = config,
            FlexGrow = 1,
            FlexShrink = 1
        };
        root_child0.Insert(0, root_child0_child0);

        var root_child0_child0_child0 = new Node
        {
            Config = config,
            FlexGrow = 1,
            FlexShrink = 1
        };
        root_child0_child0.Insert(0, root_child0_child0_child0);
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(500, root.Layout.Width);
        Assert.Equal(500, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(500, root_child0.Layout.Width);
        Assert.Equal(500, root_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0.Layout.PositionTop);
        Assert.Equal(0, root_child0_child0.Layout.Width);
        Assert.Equal(500, root_child0_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child0.Layout.PositionTop);
        Assert.Equal(0, root_child0_child0_child0.Layout.Width);
        Assert.Equal(500, root_child0_child0_child0.Layout.Height);

        config.Errata = Errata.None;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(500, root.Layout.Width);
        Assert.Equal(500, root.Layout.Height);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(500, root_child0.Layout.Width);
        // This should be modified by the lack of the errata
        Assert.Equal(0, root_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0.Layout.PositionTop);
        Assert.Equal(0, root_child0_child0.Layout.Width);
        // This should be modified by the lack of the errata
        Assert.Equal(0, root_child0_child0.Layout.Height);

        Assert.Equal(0, root_child0_child0_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0_child0_child0.Layout.PositionTop);
        Assert.Equal(0, root_child0_child0_child0.Layout.Width);
        // This should be modified by the lack of the errata
        Assert.Equal(0, root_child0_child0_child0.Layout.Height);
    }

    private class MeasureCallCountNode : Node
    {
        public static int MeasureCallCount = 0;

        public MeasureCallCountNode() : base()
        {
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            MeasureCallCount++;
            return new Vector2(25.0f, 25.0f);
        }
    }

    [Fact]
    public void SettingCompatibleConfigMaintainsLayoutCache()
    {
        var config = new Config();

        using var root = new Node() { Config = config };
        config.PointScaleFactor = 1.0f;

        root.FlexDirection = FlexDirection.Row;
        root.Width = 50;
        root.Height = 50;

        var root_child0 = new MeasureCallCountNode() { Config = config };
        Assert.Equal(0, MeasureCallCountNode.MeasureCallCount);

        root.Insert(0, root_child0);

        var root_child1 = new Node
        {
            Config = config,
            FlexGrow = 1
        };
        root.Insert(1, root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(1, MeasureCallCountNode.MeasureCallCount);
        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(25, root_child1.Layout.PositionLeft);

        // Calling YGConfigSetPointScaleFactor multiple times, ensures that config2
        // gets a different config version than config1
        var config2 = new Config
        {
            PointScaleFactor = 1.0f
        };
        config2.PointScaleFactor = 1.5f;
        config2.PointScaleFactor = 1.0f;

        root.Config = config2;
        root_child0.Config = config2;
        root_child1.Config = config2;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        // Measure should not be called again, as layout should have been cached since
        // config is functionally the same as before
        Assert.Equal(1, MeasureCallCountNode.MeasureCallCount);
        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(25, root_child1.Layout.PositionLeft);
    }
}
