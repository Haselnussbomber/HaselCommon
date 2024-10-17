using System.Runtime.CompilerServices;
using HaselCommon.Graphics;

namespace HaselCommon.Globals;

public static class ColorHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color rgb(float red, float green, float blue)
        => Color.From(red, green, blue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color rgba(float red, float green, float blue, float alpha)
        => Color.From(red, green, blue, alpha);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color hsl(float hue, float saturation, float lightness)
        => Color.FromHSL(hue, saturation, lightness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color hsla(float hue, float saturation, float lightness, float alpha)
        => Color.FromHSL(hue, saturation, lightness, alpha);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color oklch(float lightness, float chroma, float hue, float alpha = 1f)
        => Color.FromOKLCH(lightness, chroma, hue, alpha);
}
