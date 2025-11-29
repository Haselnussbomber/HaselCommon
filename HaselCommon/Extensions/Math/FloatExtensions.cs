using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class FloatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsApproximately(this float a, float b, float margin = 0.0001f)
    {
        if (float.IsNaN(a) && float.IsNaN(b)) return true;
        return MathF.Abs(a - b) < margin;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(this float v, float min, float max)
    {
        if (v < min) return min;
        if (v < max) return v;
        return max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpTo(this float from, float to, float fraction)
    {
        return from + fraction * (to - from);
    }
}
