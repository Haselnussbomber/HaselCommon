using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace HaselCommon.Extensions;

public static class INumberExtensions
{
    extension<T>(T value) where T : INumber<T>
    {
        /// <summary>
        /// Clamps the specified value within the range of 0 to 1.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Clamp01()
        {
            if (value < T.Zero) return T.Zero;
            if (value > T.One) return T.One;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Lerp(T to, T fraction)
        {
            return T.MultiplyAddEstimate(value, T.One - fraction, to * fraction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T DeltaLerp(T endValue, float amount)
        {
            var framework = Framework.Instance();
            var delta = framework != null ? framework->FrameDeltaTime : 0.016f;
            var valF = float.CreateChecked(value);
            var endF = float.CreateChecked(endValue);
            var lerpedF = valF.Lerp(endF, (amount * (float)delta * 60f).Clamp01());
            return T.CreateSaturating(lerpedF);
        }
    }
}
