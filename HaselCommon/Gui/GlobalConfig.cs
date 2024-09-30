using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

public static class GlobalConfig
{
    public static bool UseWebDefaults { get; set; }
    public static float PointScaleFactor { get; set; } = 1.0f;
    public static ExperimentalFeature ExperimentalFeatures { get; set; }
    public static Errata Errata { get; set; }
}
