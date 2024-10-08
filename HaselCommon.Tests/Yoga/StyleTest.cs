using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Tests.Yoga;

public class StyleTest
{
    [Fact]
    public void ComputedPaddingIsFloored()
    {
        using var node = new Node()
        {
            PaddingAll = -1.0f
        };
        var paddingStart = node.ComputeInlineStartPadding(FlexDirection.Row, Direction.LTR, 0.0f /*widthSize*/);
        Assert.Equal(0.0f, paddingStart);
    }

    [Fact]
    public void ComputedBorderIsFloored()
    {
        using var node = new Node()
        {
            BorderAll = -1.0f
        };
        var borderStart = node.ComputeInlineStartBorder(FlexDirection.Row, Direction.LTR);
        Assert.Equal(0.0f, borderStart);
    }

    [Fact]
    public void ComputedGapIsFloored()
    {
        using var node = new Node()
        {
            ColumnGap = -1.0f
        };
        var gapBetweenColumns = node.ComputeGapForAxis(FlexDirection.Row, 0.0f);
        Assert.Equal(0.0f, gapBetweenColumns);
    }

    [Fact]
    public void ComputedMarginIsNotFloored()
    {
        using var node = new Node()
        {
            MarginAll = -1.0f
        };
        var marginStart = node.ComputeInlineStartMargin(FlexDirection.Row, Direction.LTR, 0.0f /*widthSize*/);
        Assert.Equal(-1.0f, marginStart);
    }
}
