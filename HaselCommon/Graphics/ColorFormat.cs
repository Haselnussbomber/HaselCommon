namespace HaselCommon.Graphics;

public enum ColorFormat
{
    /// <summary>
    /// RGBA | 0xAABBGGRR<br/>
    /// Used by ImGui.
    /// </summary>
    RGBA,

    /// <summary>
    /// BGRA | 0xAARRGGBB<br/>
    /// Used by SeStrings.
    /// </summary>
    BGRA,

    /// <summary>
    /// ARGB | 0xBBGGRRAA
    /// </summary>
    ARGB,

    /// <summary>
    /// ABGR | 0xRRGGBBAA
    /// </summary>
    ABGR,
}
