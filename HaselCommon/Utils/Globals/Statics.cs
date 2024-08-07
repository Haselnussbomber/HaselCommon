using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Utils.Globals;

public static class Statics
{
    // "80 F9 07 77 0D"
    public static bool IsGatheringTypeRare(byte id)
        => id > 7 || (0x8B & (1 << id)) == 0;

    public static uint GetFishingSpotIcon(FishingSpot fishingSpot)
        => !fishingSpot.Rare ? 60465u : 60466u;

    public static unsafe void ExecuteCommand(string command)
    {
        using var cmd = new Utf8String(command);
        RaptureShellModule.Instance()->ExecuteCommandInner(&cmd, UIModule.Instance());
    }
}
