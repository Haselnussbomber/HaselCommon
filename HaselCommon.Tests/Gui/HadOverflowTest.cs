using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class HadOverflowTest
{
    public Node CreateRoot()
    {
        var root = new Node
        {
            Width = 200,
            Height = 100,
            FlexDirection = FlexDirection.Column,
            FlexWrap = Wrap.NoWrap
        };
        return root;
    }

    [Fact]
    public void ChildrenOverflowNoWrapAndNoFlexChildren()
    {
        using var root = CreateRoot();

        var child0 = new Node
        {
            Width = 80,
            Height = 40,
            MarginTop = 10,
            MarginBottom = 15
        };
        root.Insert(0, child0);
        var child1 = new Node
        {
            Width = 80,
            Height = 40,
            MarginBottom = 5
        };
        root.Insert(1, child1);

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.True(root.HadOverflow);
    }

    [Fact]
    public void SpacingOverflowNoWrapAndNoFlexChildren()
    {
        using var root = CreateRoot();

        var child0 = new Node
        {
            Width = 80,
            Height = 40,
            MarginTop = 10,
            MarginBottom = 10
        };
        root.Insert(0, child0);
        var child1 = new Node
        {
            Width = 80,
            Height = 40,
            MarginBottom = 5
        };
        root.Insert(1, child1);

        root.CalculateLayout(200, 100, Direction.LTR);
        
        Assert.True(root.HadOverflow);
    }

    [Fact]
    public void NoOverflowNoWrapAndFlexChildren()
    {
        using var root = CreateRoot();

        var child0 = new Node
        {
            Width = 80,
            Height = 40,
            MarginTop = 10,
            MarginBottom = 10
        };
        root.Insert(0, child0);
        var child1 = new Node
        {
            Width = 80,
            Height = 40,
            MarginBottom = 5,
            FlexShrink = 1
        };
        root.Insert(1, child1);

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.False(root.HadOverflow);
    }

    [Fact]
    public void HadOverflowGetsResetIfNotLoggerValid()
    {
        using var root = CreateRoot();

        var child0 = new Node
        {
            Width = 80,
            Height = 40,
            MarginTop = 10,
            MarginBottom = 10
        };
        root.Insert(0, child0);
        var child1 = new Node
        {
            Width = 80,
            Height = 40,
            MarginBottom = 5
        };
        root.Insert(1, child1);

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.True(root.HadOverflow);

        child1.FlexShrink = 1;

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.False(root.HadOverflow);
    }

    [Fact]
    public void SpacingOverflowInNestedNodes()
    {
        using var root = CreateRoot();

        var child0 = new Node
        {
            Width = 80,
            Height = 40,
            MarginTop = 10,
            MarginBottom = 10
        };
        root.Insert(0, child0);
        var child1 = new Node
        {
            Width = 80,
            Height = 40
        };
        root.Insert(1, child1);
        var child1_1 = new Node
        {
            Width = 80,
            Height = 40,
            MarginBottom = 5
        };
        child1.Insert(0, child1_1);

        root.CalculateLayout(200, 100, Direction.LTR);

        Assert.True(root.HadOverflow);
    }
}
