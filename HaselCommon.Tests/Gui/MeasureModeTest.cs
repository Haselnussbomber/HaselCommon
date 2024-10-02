using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class MeasureModeTest
{
    public record MeasureConstraint(float width, MeasureMode widthMode, float height, MeasureMode heightMode);

    public class MeasureNode : Node
    {
        private readonly Action<MeasureConstraint> _callback;

        public MeasureNode(Action<MeasureConstraint> callback)
        {
            _callback = callback;
            HasMeasureFunc = true;
        }

        public override Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            _callback(new MeasureConstraint(width, widthMode, height, heightMode));
            return new Vector2(
                widthMode == MeasureMode.Undefined ? 10 : width,
                heightMode == MeasureMode.Undefined ? 10 : width);
        }
    }

    [Fact]
    public void ExactlyMeasureStretchedChildColumn()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].width);
        Assert.Equal(MeasureMode.Exactly, measureConstraintList[0].widthMode);
    }

    [Fact]
    public void ExactlyMeasureStretchedChildRow()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.Exactly, measureConstraintList[0].heightMode);
    }

    [Fact]
    public void AtMostMainAxisColumn()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].heightMode);
    }

    [Fact]
    public void AtMostCrossAxisColumn()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].width);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].widthMode);
    }

    [Fact]
    public void AtMostMainAxisRow()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].width);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].widthMode);
    }

    [Fact]
    public void AtMostCrossAxisRow()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].heightMode);
    }

    [Fact]
    public void FlexChild()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.Height = 100;

        void callback(MeasureConstraint mc)
        {
            measureConstraintList.Add(mc);
        }

        var root_child0 = new MeasureNode(callback)
        {
            FlexGrow = 1
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(2, measureConstraintList.Count);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].heightMode);

        Assert.Equal(100, measureConstraintList[1].height);
        Assert.Equal(MeasureMode.Exactly, measureConstraintList[1].heightMode);
    }

    [Fact]
    public void FlexChildWithFlexBasis()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.Height = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add)
        {
            FlexGrow = 1,
            FlexBasis = 0
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.Exactly, measureConstraintList[0].heightMode);
    }

    [Fact]
    public void OverflowScrollColumn()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Overflow = Overflow.Scroll;
        root.Height = 100;
        root.Width = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.Equal(100, measureConstraintList[0].width);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].widthMode);

        Assert.True(float.IsNaN(measureConstraintList[0].height));
        Assert.Equal(MeasureMode.Undefined, measureConstraintList[0].heightMode);
    }

    [Fact]
    public void OverflowScrollRow()
    {
        var measureConstraintList = new List<MeasureConstraint>();

        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.FlexDirection = FlexDirection.Row;
        root.Overflow = Overflow.Scroll;
        root.Height = 100;
        root.Width = 100;

        var root_child0 = new MeasureNode(measureConstraintList.Add);
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Single(measureConstraintList);

        Assert.True(float.IsNaN(measureConstraintList[0].width));
        Assert.Equal(MeasureMode.Undefined, measureConstraintList[0].widthMode);

        Assert.Equal(100, measureConstraintList[0].height);
        Assert.Equal(MeasureMode.AtMost, measureConstraintList[0].heightMode);
    }
}
