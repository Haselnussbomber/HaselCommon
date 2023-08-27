namespace HaselCommon.Extensions;

public static class HalfExtensions
{
    public static bool IsApproximately(this Half a, Half b)
        => ((float)a).IsApproximately((float)b, 0.001f);
}
