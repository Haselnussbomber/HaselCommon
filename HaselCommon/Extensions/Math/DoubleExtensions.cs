using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class DoubleExtensions
{
    extension(double value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsApproximately(double b, double margin = 0.0001f)
        {
            var bothNaN = double.IsNaN(value) && double.IsNaN(b);

            if (!bothNaN)
                return Math.Abs(value - b) < margin;

            return bothNaN;
        }
    }
}
