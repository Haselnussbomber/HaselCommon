using FFXIVClientStructs.FFXIV.Client.UI;

namespace HaselCommon.Game;

public static class Misc
{
    // "80 F9 07 77 0D"
    public static bool IsGatheringTypeRare(byte id)
        => id > 7 || (0x8B & 1 << id) == 0;

    public static unsafe bool IsLightTheme
        => RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType == 1;
}
