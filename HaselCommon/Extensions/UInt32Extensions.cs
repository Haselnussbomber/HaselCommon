namespace HaselCommon.Extensions;

public static class UInt32Extensions
{
    public static uint Reverse(this uint value)
        => (value & 0x000000FFu) << 24 | (value & 0x0000FF00u) << 8 |
            (value & 0x00FF0000u) >> 8 | (value & 0xFF000000u) >> 24;

    public static bool IsNormalItem(this uint itemId)
        => itemId is < 500_000;
    public static bool IsCollectibleItem(this uint itemId)
        => itemId is > 500_000 and < 1_000_000;
    public static bool IsHighQualityItem(this uint itemId)
        => itemId is > 1_000_000 and < 2_000_000;
    public static bool IsEventItem(this uint itemId)
        => itemId is > 2_000_000;
}
