using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class FloatExtensions
{
    extension(float value)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsApproximately(float b, float margin = 0.0001f)
        {
            if (float.IsNaN(value) && float.IsNaN(b)) return true;
            return MathF.Abs(value - b) < margin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Clamp(float min, float max)
        {
            if (value < min) return min;
            if (value < max) return value;
            return max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float LerpTo(float to, float fraction)
        {
            return value + fraction * (to - value);
        }
    }
}
