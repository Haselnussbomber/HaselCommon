using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui.Extensions;

public static class MeasureModeExtensions
{
    public static SizingMode SizingMode(this MeasureMode mode)
    {
        return mode switch
        {
            MeasureMode.Exactly => Enums.SizingMode.StretchFit,
            MeasureMode.Undefined => Enums.SizingMode.MaxContent,
            MeasureMode.AtMost => Enums.SizingMode.FitContent,
            _ => throw new Exception("Invalid MeasureMode"),
        };
    }
}
