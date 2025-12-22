using FFXIVClientStructs.FFXIV.Common.Math;

namespace HaselCommon.Extensions;

public static class HalfVector2Extensions
{
    extension(HalfVector2 value)
    {
        public bool IsApproximately(HalfVector2 other)
        {
            return value.X.IsApproximately(other.X)
                && value.Y.IsApproximately(other.Y);
        }
    }
}
