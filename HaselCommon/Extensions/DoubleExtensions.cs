namespace HaselCommon.Extensions;

public static class DoubleExtensions
{
    public static bool IsApproximately(this double a, double b, double margin = 0.001f)
    {
        var bothNaN = double.IsNaN(a) && double.IsNaN(b);

        if (!bothNaN)
            return Math.Abs(a - b) < margin;

        return bothNaN;
    }
}
