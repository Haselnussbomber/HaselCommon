using HaselCommon.Gui;

namespace HaselCommon.Tests.Gui;

public class RoundingFunctionTest
{
    [Fact]
    public void RoundingValue()
    {
        // Test that whole numbers are rounded to whole despite ceil/floor flags
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.000001, 2.0, false, true));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.999999, 2.0, false, true));
        // Same tests for negative numbers
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.000001, 2.0, false, true));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.999999, 2.0, false, true));

        // Test that numbers with fraction are rounded correctly accounting for
        // ceil/floor flags
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.01, 2.0, false, false));
        Assert.Equal(6.5, Node.RoundValueToPixelGrid(6.01, 2.0, true, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(6.01, 2.0, false, true));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.99, 2.0, false, false));
        Assert.Equal(6.0, Node.RoundValueToPixelGrid(5.99, 2.0, true, false));
        Assert.Equal(5.5, Node.RoundValueToPixelGrid(5.99, 2.0, false, true));
        // Same tests for negative numbers
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.01, 2.0, false, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-6.01, 2.0, true, false));
        Assert.Equal(-6.5, Node.RoundValueToPixelGrid(-6.01, 2.0, false, true));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.99, 2.0, false, false));
        Assert.Equal(-5.5, Node.RoundValueToPixelGrid(-5.99, 2.0, true, false));
        Assert.Equal(-6.0, Node.RoundValueToPixelGrid(-5.99, 2.0, false, true));
    }

    // consistent_rounding_during_repeated_layouts
    // per_node_point_scale_factor
}
