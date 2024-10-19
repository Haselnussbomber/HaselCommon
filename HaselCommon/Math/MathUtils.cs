using FFXIVClientStructs.FFXIV.Client.System.Framework;
using HaselCommon.Extensions.Math;

namespace HaselCommon.Math;

public static class MathUtils
{
    public static float Clamp01(float value)
        => value switch
        {
            < 0f => 0f,
            > 1f => 1f,
            _ => value
        };

    public static float Lerp(float startValue, float endValue, float amount)
    {
        var val = (1 - amount) * startValue + amount * endValue;
        if (val.IsApproximately(endValue))
            val = endValue;
        return val;
    }

    public static unsafe float DeltaLerp(float startValue, float endValue, float amount, float? timeSec = null)
    {
        timeSec ??= Framework.Instance()->FrameDeltaTime;
        return Lerp(startValue, endValue, Clamp01(amount * (float)timeSec * 60));
    }
}
