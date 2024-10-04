using System.Diagnostics.CodeAnalysis;
using HaselCommon.Gui.Yoga.Enums;

namespace HaselCommon.Gui.Yoga;

internal class CachedMeasurement
{
    public float AvailableWidth { get; set; } = -1;
    public float AvailableHeight { get; set; } = -1;
    public SizingMode WidthSizingMode { get; set; } = SizingMode.MaxContent;
    public SizingMode HeightSizingMode { get; set; } = SizingMode.MaxContent;
    public float ComputedWidth { get; set; } = -1;
    public float ComputedHeight { get; set; } = -1;

    public static bool operator ==(CachedMeasurement? a, CachedMeasurement? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(CachedMeasurement? a, CachedMeasurement? b)
        => !(a == b);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not CachedMeasurement measurement)
            return false;

        var isEqual = WidthSizingMode == measurement.WidthSizingMode &&
            HeightSizingMode == measurement.HeightSizingMode;

        if (!float.IsNaN(AvailableWidth) || !float.IsNaN(measurement.AvailableWidth))
            isEqual = isEqual && AvailableWidth == measurement.AvailableWidth;
        if (!float.IsNaN(AvailableHeight) || !float.IsNaN(measurement.AvailableHeight))
            isEqual = isEqual && AvailableHeight == measurement.AvailableHeight;
        if (!float.IsNaN(ComputedWidth) || !float.IsNaN(measurement.ComputedWidth))
            isEqual = isEqual && ComputedWidth == measurement.ComputedWidth;
        if (!float.IsNaN(ComputedHeight) || !float.IsNaN(measurement.ComputedHeight))
            isEqual = isEqual && ComputedHeight == measurement.ComputedHeight;

        return isEqual;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AvailableWidth, AvailableHeight, WidthSizingMode, HeightSizingMode, ComputedWidth, ComputedHeight);
    }
}
