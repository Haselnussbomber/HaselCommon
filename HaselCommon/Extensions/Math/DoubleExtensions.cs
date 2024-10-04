using MathD = System.Math;

namespace HaselCommon.Extensions.Math;

public static class DoubleExtensions
{
    public static bool IsApproximately(this double a, double b, double margin = 0.0001f)
    {
        var bothNaN = double.IsNaN(a) && double.IsNaN(b);

        if (!bothNaN)
            return MathD.Abs(a - b) < margin;

        return bothNaN;
    }
}
