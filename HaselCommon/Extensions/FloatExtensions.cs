namespace HaselCommon.Extensions;

public static class FloatExtensions
{
    public static bool IsApproximately(this float a, float b, float margin = 0.001f)
    {
        var bothNaN = float.IsNaN(a) && float.IsNaN(b);

        if (!bothNaN)
            return MathF.Abs(a - b) < margin;

        return bothNaN;
    }
}
