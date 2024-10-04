using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Tests.Yoga;

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

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.Equal(100, root_child0.ComputedWidth);
        Assert.Equal(100, root_child0.ComputedHeight);

        root.Remove(root_child0);

        Assert.Equal(0, root_child0.ComputedLeft);
        Assert.Equal(0, root_child0.ComputedTop);
        Assert.True(float.IsNaN(root_child0.ComputedWidth));
        Assert.True(float.IsNaN(root_child0.ComputedHeight));
    }
}
