namespace HaselCommon.Extensions;

public static class DoubleExtensions
{
    public static bool IsApproximately(this double a, double b, double margin = 0.001f)
        => Math.Abs(a - b) < margin;
}
