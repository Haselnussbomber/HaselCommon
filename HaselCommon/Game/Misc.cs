using FFXIVClientStructs.FFXIV.Client.UI;

namespace HaselCommon.Game;

public static class Misc
{
    public static unsafe bool IsLightTheme
        => RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType == 1;
}
