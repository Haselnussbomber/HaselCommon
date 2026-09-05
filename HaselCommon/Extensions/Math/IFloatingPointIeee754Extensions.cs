using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class IFloatingPointIeee754Extensions
{
    extension<T>(T value) where T : IFloatingPointIeee754<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsApproximately(T b, T? margin = default)
        {
            if (T.IsNaN(value) && T.IsNaN(b))
                return true;

            var actualMargin = margin ?? T.CreateChecked(0.0001f);
            return T.Abs(value - b) < actualMargin;
        }
    }
}
