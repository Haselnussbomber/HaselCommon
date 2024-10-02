using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class MeasureCacheTest
{
    public class MeasureMaxNode : Node
    {
        private readonly Action _callback;

        public MeasureMaxNode(Action callback)
        {
            _callback = callback;
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            _callback();
            return new Vector2(
                widthMode == MeasureMode.Undefined ? 10 : width,
                heightMode == MeasureMode.Undefined ? 10 : height);
        }
    }

    public class MeasureMinNode : Node
    {
        private readonly Action _callback;

        public MeasureMinNode(Action callback)
        {
            _callback = callback;
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            _callback();
            return new Vector2(
                widthMode == MeasureMode.Undefined || (widthMode == MeasureMode.AtMost && width > 10) ? 10 : width,
                heightMode == MeasureMode.Undefined || (heightMode == MeasureMode.AtMost && height > 10) ? 10 : height);
        }
    }

    public class Measure8449Node : Node
    {
        private readonly Action _callback;

        public Measure8449Node(Action callback)
        {
            _callback = callback;
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            _callback();
            return new Vector2(84, 49);
        }
    }

    [Fact]
    public void MeasureOnceSingleFlexibleChild()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var measureCount = 0;
        var root_child0 = new MeasureMaxNode(() => measureCount++)
        {
            FlexGrow = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(1, measureCount);
    }

    [Fact]
    public void RemeasureWithSameExactWidthLargerThanNeededHeight()
    {
        using var root = new Node();

        var measureCount = 0;
        var root_child0 = new MeasureMinNode(() => measureCount++);
        root.Insert(0, root_child0);

        root.CalculateLayout(100, 100, Direction.LTR);
        root.CalculateLayout(100, 50, Direction.LTR);

        Assert.Equal(1, measureCount);
    }

    [Fact]
    public void RemeasureWithSameAtmostWidthLargerThanNeededHeight()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;

        var measureCount = 0;
        var root_child0 = new MeasureMinNode(() => measureCount++);
        root.Insert(0, root_child0);

        root.CalculateLayout(100, 100, Direction.LTR);
        root.CalculateLayout(100, 50, Direction.LTR);

        Assert.Equal(1, measureCount);
    }

    [Fact]
    public void RemeasureWithComputedWidthLargerThanNeededHeight()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;

        var measureCount = 0;
        var root_child0 = new MeasureMinNode(() => measureCount++);
        root.Insert(0, root_child0);

        root.CalculateLayout(100, 100, Direction.LTR);
        root.AlignItems = Align.Stretch;
        root.CalculateLayout(10, 50, Direction.LTR);

        Assert.Equal(1, measureCount);
    }

    [Fact]
    public void RemeasureWithAtmostComputedWidthUndefinedHeight()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;

        var measureCount = 0;
        var root_child0 = new MeasureMinNode(() => measureCount++);
        root.Insert(0, root_child0);

        root.CalculateLayout(100, float.NaN, Direction.LTR);
        root.CalculateLayout(10, float.NaN, Direction.LTR);

        Assert.Equal(1, measureCount);
    }

    [Fact]
    public void RemeasureWithAlreadyMeasuredValueSmallerButStillFloatEqual()
    {
        var measureCount = 0;

        using var root = new Node();
        root.Width = 288;
        root.Height = 288;
        root.FlexDirection = FlexDirection.Row;

        var root_child0 = new Node
        {
            PaddingAll = 2.88f,
            FlexDirection = FlexDirection.Row
        };
        root.Insert(0, root_child0);

        var root_child0_child0 = new Measure8449Node(() => measureCount++);
        root_child0.Insert(0, root_child0_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(1, measureCount);
    }
}
