using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class NodeChildTest
{
    [Fact]
    public void ResetLayoutWhenChildRemoved()
    {
        using var root = new Node();

        using var root_child0 = new Node();
        root_child0.Width = 100;
        root_child0.Height = 100;
        root.Add(root_child0);

        root.CalculateLayout(float.NaN, float.NaN, Direction.LTR);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(100, root_child0.Layout.Width);
        Assert.Equal(100, root_child0.Layout.Height);

        root.Remove(root_child0);

        Assert.Equal(0, root_child0.Layout.PositionLeft);
        Assert.Equal(0, root_child0.Layout.PositionTop);
        Assert.Equal(0, root_child0.Layout.Width);
        Assert.Equal(0, root_child0.Layout.Height);
    }
}
