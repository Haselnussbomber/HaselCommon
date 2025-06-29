namespace HaselCommon.Extensions;

public static class FloatExtensions
{
    extension(float value)
    {
        public bool IsApproximately(float other, float margin = 0.0001f)
        {
            var bothNaN = float.IsNaN(value) && float.IsNaN(other);

            if (!bothNaN)
                return MathF.Abs(value - other) < margin;

            return bothNaN;
        }
    }
}
