namespace HaselCommon.Utils.Globals;

public static class Statics
{
    // "80 F9 07 77 10"
    public static bool IsGatheringTypeRare(byte id)
        => id > 7 || (0x8B & (1 << id)) == 0;
}
