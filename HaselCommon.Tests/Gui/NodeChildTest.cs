using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class NodeChildTest
{
    [Fact]
    public void ResetLayoutWhenChildRemoved()
    {
        using var root = new Node();

        var root_child0 = new Node
        {
            Width = 100,
            Height = 100
        };
        root.Add(root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(100, root_child0.Layout.Width);
        Assert.Equal(100, root_child0.Layout.Height);

        root.Remove(root_child0);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.True(float.IsNaN(root_child0.Layout.Width));
        Assert.True(float.IsNaN(root_child0.Layout.Height));
    }
}
