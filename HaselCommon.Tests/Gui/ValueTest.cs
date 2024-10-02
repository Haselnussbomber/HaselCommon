using HaselCommon.Gui;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Tests.Gui;

public class ValueTest
{
    [Fact]
    public void SupportsEquality()
    {
        Assert.True(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }.Equals(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }));
        Assert.False(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }.Equals(new StyleLength() { Value = 56.7f, Unit = Unit.Percent }));
        Assert.False(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }.Equals(new StyleLength() { Value = 12.5f, Unit = Unit.Point }));
        Assert.False(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }.Equals(new StyleLength() { Value = 12.5f, Unit = Unit.Auto }));
        Assert.False(new StyleLength() { Value = 12.5f, Unit = Unit.Percent }.Equals(new StyleLength() { Value = 12.5f, Unit = Unit.Undefined }));
        Assert.True(new StyleLength() { Value = 12.5f, Unit = Unit.Undefined }.Equals(new StyleLength() { Value = float.NaN, Unit = Unit.Undefined }));
        Assert.True(new StyleLength() { Value = 0, Unit = Unit.Auto }.Equals(new StyleLength() { Value = -1, Unit = Unit.Auto }));
    }
}
