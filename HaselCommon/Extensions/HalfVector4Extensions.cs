using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions;

public static class HalfVector4Extensions
{
    public static bool IsApproximately(this HalfVector4 a, HalfVector4 b)
        => a.X.IsApproximately(b.X)
        && a.Y.IsApproximately(b.Y)
        && a.Z.IsApproximately(b.Z)
        && a.W.IsApproximately(b.W);
}
