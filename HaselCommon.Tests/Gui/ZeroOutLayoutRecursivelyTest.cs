using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class ZeroOutLayoutRecursivelyTest
{
    [Fact]
    public void ZeroOutLayout()
    {
        using var root = new Node();
        root.FlexDirection = FlexDirection.Row;
        root.Width = 200;
        root.Height = 200;

        var child = new Node();
        root.Add(child);
        child.Width = 100;
        child.Height = 100;
        child.MarginTop = 10;
        child.PaddingTop = 10;

        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(10, child.ComputedMarginTop);
        Assert.Equal(10, child.ComputedPaddingTop);

        child.Display = Display.None;

        root.CalculateLayout(100, 100, Direction.LTR);

        Assert.Equal(0, child.ComputedMarginTop);
        Assert.Equal(0, child.ComputedPaddingTop);
    }
}
