using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class EdgeTest
{
    [Fact]
    public void StartOverrides()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginStart = 10,
            MarginLeft = 20,
            MarginRight = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(20, root_child0.ComputedRight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);
        Assert.Equal(20, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedRight);
    }

    [Fact]
    public void EndOverrides()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginEnd = 10,
            MarginLeft = 20,
            MarginRight = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(20, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedRight);

        root.CalculateLayout(float.NaN, float.NaN, Direction.RTL);
        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(20, root_child0.ComputedRight);
    }

    [Fact]
    public void HorizontalOverridden()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginHorizontal = 10,
            MarginLeft = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(20, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedRight);
    }

    [Fact]
    public void VerticalOverridden()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Column;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginVertical = 10,
            MarginTop = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(20, root_child0.ComputedTop);
        Assert.Equal(10, root_child0.ComputedBottom);
    }

    [Fact]
    public void HorizontalOverridesAll()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Column;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginHorizontal = 10,
            MarginAll = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(20, root_child0.ComputedTop);
        Assert.Equal(10, root_child0.ComputedRight);
        Assert.Equal(20, root_child0.ComputedBottom);
    }

    [Fact]
    public void VerticalOverridesAll()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Column;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginVertical = 10,
            MarginAll = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(20, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedTop);
        Assert.Equal(20, root_child0.ComputedRight);
        Assert.Equal(10, root_child0.ComputedBottom);
    }

    [Fact]
    public void AllOverridden()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Column;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            FlexGrow = 1,
            MarginLeft = 10,
            MarginTop = 10,
            MarginRight = 10,
            MarginBottom = 10,
            MarginAll = 20
        };
        root.Insert(0, root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(10, root_child0.ComputedLeft);
        Assert.Equal(10, root_child0.ComputedTop);
        Assert.Equal(10, root_child0.ComputedRight);
        Assert.Equal(10, root_child0.ComputedBottom);
    }

}
