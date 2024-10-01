using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class DirtyMarkingTest
{
    [Fact]
    public void DirtyPropagation()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child0);

        var root_child1 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        root_child0.Width = 20;

        Assert.True(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.True(root.IsDirty);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root.IsDirty);
    }

    [Fact]
    public void DirtyPropagationOnlyIfPropChanged()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child0);

        var root_child1 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child1);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        root_child0.Width = 50;

        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root.IsDirty);
    }

    [Fact]
    public void DirtyPropagationChangingLayout/*Config*/()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child0);

        var root_child1 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child1);

        var root_child0_child0 = new Node
        {
            Width = 25,
            Height = 20
        };
        root.Add(root_child0_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.False(root.IsDirty);
        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root_child0_child0.IsDirty);
        /*
        var config = new Config()
        {
            Errata = Errata.StretchFlexBasis
        };
        root_child0.Config = config;

        Assert.True(root.IsDirty);
        Assert.True(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root_child0_child0.IsDirty);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.False(root.IsDirty);
        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root_child0_child0.IsDirty);
        */
    }

    [Fact]
    public void DirtyPropagationChangingBenignConfig()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 100;
        root.Height = 100;

        var root_child0 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child0);

        var root_child1 = new Node
        {
            Width = 50,
            Height = 20
        };
        root.Add(root_child1);

        var root_child0_child0 = new Node
        {
            Width = 25,
            Height = 20
        };
        root.Add(root_child0_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.False(root.IsDirty);
        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root_child0_child0.IsDirty);

        var config = new Config();
        /*
        YGConfigSetLogger(
            newConfig,
            [](YGConfigConstRef, Node, YGLogLevel, const char*, va_list) {
            return 0;
        });
        */
        root_child0.Config = config;

        Assert.False(root.IsDirty);
        Assert.False(root_child0.IsDirty);
        Assert.False(root_child1.IsDirty);
        Assert.False(root_child0_child0.IsDirty);
    }

    [Fact]
    public void DirtyMarkAllChildrenAsDirtyWhenDisplayChanges()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Height = 100;

        var child0 = new Node
        {
            FlexGrow = 1
        };
        var child1 = new Node
        {
            FlexGrow = 1
        };

        var child1_child0 = new Node();
        var child1_child0_child0 = new Node
        {
            Width = 8,
            Height = 16
        };

        child1_child0.Add(child1_child0_child0);

        child1.Add(child1_child0);
        root.Add(child0);
        root.Add(child1);

        child0.Display = Display.Flex;
        child1.Display = Display.None;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(0, child1_child0_child0.Layout.Width);
        Assert.Equal(0, child1_child0_child0.Layout.Height);

        child0.Display = Display.None;
        child1.Display = Display.Flex;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(8, child1_child0_child0.Layout.Width);
        Assert.Equal(16, child1_child0_child0.Layout.Height);

        child0.Display = Display.Flex;
        child1.Display = Display.None;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(0, child1_child0_child0.Layout.Width);
        Assert.Equal(0, child1_child0_child0.Layout.Height);

        child0.Display = Display.None;
        child1.Display = Display.Flex;
        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.Equal(8, child1_child0_child0.Layout.Width);
        Assert.Equal(16, child1_child0_child0.Layout.Height);
    }

    [Fact]
    public void DirtyNodeOnlyIfChildrenAreActuallyRemoved()
    {
        using var root = new Node();
        root.AlignItems = Align.FlexStart;
        root.Width = 50;
        root.Height = 50;

        using var child0 = new Node();
        child0.Width = 50;
        child0.Height = 25;
        root.Add(child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        using var child1 = new Node();
        root.Remove(child1);
        Assert.False(root.IsDirty);

        root.Remove(child0);
        Assert.True(root.IsDirty);
    }

    [Fact]
    public void DirtyNodeOnlyIfUndefinedValuesGetsSetToUndefined()
    {
        using var root = new Node();
        root.Width = 50;
        root.Height = 50;
        root.MinWidth = float.NaN;

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);
        Assert.False(root.IsDirty);

        root.MinWidth = float.NaN;

        Assert.False(root.IsDirty);
    }
}
