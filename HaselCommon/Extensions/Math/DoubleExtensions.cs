using MathD = System.Math;

namespace HaselCommon.Extensions;

public static class DoubleExtensions
{
    extension(double value)
    {
        public bool IsApproximately(double other, double margin = 0.0001f)
        {
            var bothNaN = double.IsNaN(value) && double.IsNaN(other);

            if (!bothNaN)
                return MathD.Abs(value - other) < margin;

            return bothNaN;
        }
    }
}
