using HaselCommon.Gui.Enums;
using PropertyChanged.SourceGenerator;

namespace HaselCommon.Gui;

public partial class Config
{
    internal uint Version { get; set; }

    [IsChanged]
    private bool IsChanged
    {
        set => Version++;
    }

    /// <summary>
    /// Yoga by default creates new nodes with style defaults different from flexbox on web (e.g. <see cref="FlexDirection.Column"/> and <see cref="PositionType.Relative"/>).<br/>
    /// <see cref="UseWebDefaults"/> instructs Yoga to instead use a default style consistent with the web.
    /// </summary>
    [Notify] public bool _useWebDefaults = GlobalConfig.UseWebDefaults;

    /// <summary>
    /// Yoga will by deafult round final layout positions and dimensions to the nearst point.<br/>
    /// <see cref="PointScaleFactor"/> controls the density of the grid used for layout rounding (e.g. to round to the closest display pixel).<br/>
    /// May be set to 0.0f to avoid rounding the layout results.
    /// </summary>
    [Notify] public float _pointScaleFactor = GlobalConfig.PointScaleFactor;

    /// <summary>
    /// Enable an experimental/unsupported feature in Yoga.
    /// </summary>
    [Notify] public ExperimentalFeature _experimentalFeatures = GlobalConfig.ExperimentalFeatures;

    /// <summary>
    /// Configures how Yoga balances W3C conformance vs compatibility with layouts created against earlier versions of Yoga.<br/>
    /// <br/>
    /// By deafult Yoga will prioritize W3C conformance. <see cref="Errata"/> may be set to ask Yoga to produce specific incorrect behaviors.<br/>
    /// <br/>
    /// Errata is a bitmask, and multiple errata may be set at once. Predfined constants exist for convenience:<br/>
    /// <list type="table">
    ///   <item>
    ///     <term><see cref="YGErrata.None"/></term>
    ///     <description>No errata</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="YGErrata.Classic"/></term>
    ///     <description>Match layout behaviors of Yoga 1.x</description>
    ///   </item>
    ///   <item>
    ///     <term><see cref="YGErrata.All"/></term>
    ///     <description>Match layout behaviors of Yoga 1.x, including `UseLegacyStretchBehaviour`</description>
    ///   </item>
    /// </list>
    /// </summary>
    [Notify] public Errata _errata = GlobalConfig.Errata;

    internal static bool UpdateInvalidatesLayout(Config oldConfig, Config newConfig)
    {
        return oldConfig.Errata != newConfig.Errata ||
            oldConfig.ExperimentalFeatures != newConfig.ExperimentalFeatures ||
            oldConfig.PointScaleFactor != newConfig.PointScaleFactor ||
            oldConfig.UseWebDefaults != newConfig.UseWebDefaults;
    }
}
