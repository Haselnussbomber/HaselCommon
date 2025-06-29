using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions;

public static class HalfVector4Extensions
{
    extension(HalfVector4 value)
    {
        public bool IsApproximately(HalfVector4 other)
        {
            return value.X.IsApproximately(other.X)
                && value.Y.IsApproximately(other.Y)
                && value.Z.IsApproximately(other.Z)
                && value.W.IsApproximately(other.W);
        }
    }
}
