using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class DefaultValuesTest
{
    [Fact]
    public void AssertDefaultValues()
    {
        using var root = new Node();

        Assert.Empty(root);
        //Assert.Equal(nullptr, YGNodeGetChild(root, 1));

        Assert.Equal(Direction.Inherit, root.Direction);
        Assert.Equal(FlexDirection.Column, root.FlexDirection);
        Assert.Equal(Justify.FlexStart, root.JustifyContent);
        Assert.Equal(Align.FlexStart, root.AlignContent);
        Assert.Equal(Align.Stretch, root.AlignItems);
        Assert.Equal(Align.Auto, root.AlignSelf);
        Assert.Equal(PositionType.Relative, root.PositionType);
        Assert.Equal(Wrap.NoWrap, root.FlexWrap);
        Assert.Equal(Overflow.Visible, root.Overflow);
        Assert.Equal(0, root.FlexGrow);
        Assert.Equal(0, root.FlexShrink);
        Assert.Equal(Unit.Auto, root.FlexBasis.Unit);

        Assert.Equal(Unit.Undefined, root.PositionLeft.Unit);
        Assert.Equal(Unit.Undefined, root.PositionTop.Unit);
        Assert.Equal(Unit.Undefined, root.PositionRight.Unit);
        Assert.Equal(Unit.Undefined, root.PositionBottom.Unit);
        Assert.Equal(Unit.Undefined, root.PositionStart.Unit);
        Assert.Equal(Unit.Undefined, root.PositionEnd.Unit);

        Assert.Equal(Unit.Undefined, root.MarginLeft.Unit);
        Assert.Equal(Unit.Undefined, root.MarginTop.Unit);
        Assert.Equal(Unit.Undefined, root.MarginRight.Unit);
        Assert.Equal(Unit.Undefined, root.MarginBottom.Unit);
        Assert.Equal(Unit.Undefined, root.MarginStart.Unit);
        Assert.Equal(Unit.Undefined, root.MarginEnd.Unit);

        Assert.Equal(Unit.Undefined, root.PaddingLeft.Unit);
        Assert.Equal(Unit.Undefined, root.PaddingTop.Unit);
        Assert.Equal(Unit.Undefined, root.PaddingRight.Unit);
        Assert.Equal(Unit.Undefined, root.PaddingBottom.Unit);
        Assert.Equal(Unit.Undefined, root.PaddingStart.Unit);
        Assert.Equal(Unit.Undefined, root.PaddingEnd.Unit);

        Assert.Equal(0, root.BorderLeft.Value);
        Assert.Equal(0, root.BorderTop.Value);
        Assert.Equal(0, root.BorderRight.Value);
        Assert.Equal(0, root.BorderBottom.Value);
        Assert.Equal(0, root.BorderStart.Value);
        Assert.Equal(0, root.BorderEnd.Value);

        Assert.Equal(Unit.Auto, root.Width.Unit);
        Assert.Equal(Unit.Auto, root.Height.Unit);
        Assert.Equal(Unit.Undefined, root.MinWidth.Unit);
        Assert.Equal(Unit.Undefined, root.MinHeight.Unit);
        Assert.Equal(Unit.Undefined, root.MaxWidth.Unit);
        Assert.Equal(Unit.Undefined, root.MaxHeight.Unit);

        Assert.Equal(0, root.Layout.PositionLeft);
        Assert.Equal(0, root.Layout.PositionTop);
        Assert.Equal(0, root.Layout.PositionRight);
        Assert.Equal(0, root.Layout.PositionBottom);

        Assert.Equal(0, root.MarginLeft.Value);
        Assert.Equal(0, root.MarginTop.Value);
        Assert.Equal(0, root.MarginRight.Value);
        Assert.Equal(0, root.MarginBottom.Value);

        Assert.Equal(0, root.PaddingLeft.Value);
        Assert.Equal(0, root.PaddingTop.Value);
        Assert.Equal(0, root.PaddingRight.Value);
        Assert.Equal(0, root.PaddingBottom.Value);

        Assert.Equal(0, root.BorderLeft.Value);
        Assert.Equal(0, root.BorderTop.Value);
        Assert.Equal(0, root.BorderRight.Value);
        Assert.Equal(0, root.BorderBottom.Value);

        Assert.Equal(0, root.Layout.Width);
        Assert.Equal(0, root.Layout.Height);
        Assert.Equal(Direction.Inherit, root.Layout.Direction);
    }

    [Fact]
    public void AssertWebDefaultValues()
    {
        var config = new Config() { UseWebDefaults = true };
        using var root = new Node() { Config = config };

        Assert.Equal(FlexDirection.Row, root.FlexDirection);
        Assert.Equal(Align.Stretch, root.AlignContent);

        // this is part of the public api YGNodeStyleGetFlexShrink, which doesn't exist in this c# port, so it is ignored
        // Assert.Equal(1.0f, root.FlexShrink);
    }

    // this c# port doesn't have a Node::reset() function, because it replaces the Node object with a new Node via pointer
    /*
    [Fact]
    public void AssertWebDefaultValuesReset()
    {
        var config = new Config() { UseWebDefaults = true };
        using var root = new Node() { Config = config };
        root.Reset();

        Assert.Equal(FlexDirection.Row, root.FlexDirection);
        Assert.Equal(Align.Stretch, root.AlignContent);
        Assert.Equal(1.0f, root.FlexShrink);
    }
    */

    [Fact]
    public void AssertLegacyStretchBehaviour()
    {
        var config = new Config() { Errata = Errata.StretchFlexBasis };
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
    }
}
