using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga.Extensions;

public static class SizingModeExtensions
{
    public static MeasureMode MeasureMode(this SizingMode mode)
    {
        return mode switch
        {
            SizingMode.StretchFit => Enums.MeasureMode.Exactly,
            SizingMode.MaxContent => Enums.MeasureMode.Undefined,
            SizingMode.FitContent => Enums.MeasureMode.AtMost,
            _ => throw new Exception("Invalid SizingMode"),
        };
    }
}
