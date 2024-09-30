using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

internal class CachedMeasurement()
{
    public float AvailableWidth { get; set; } = -1;
    public float AvailableHeight { get; set; } = -1;
    public SizingMode WidthSizingMode { get; set; } = SizingMode.MaxContent;
    public SizingMode HeightSizingMode { get; set; } = SizingMode.MaxContent;
    public float ComputedWidth { get; set; } = -1;
    public float ComputedHeight { get; set; } = -1;
}
