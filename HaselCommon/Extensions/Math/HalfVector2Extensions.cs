using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions.Math;

public static class HalfVector2Extensions
{
    public static bool IsApproximately(this HalfVector2 a, HalfVector2 b)
        => a.X.IsApproximately(b.X)
        && a.Y.IsApproximately(b.Y);
}
