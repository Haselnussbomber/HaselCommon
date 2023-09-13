using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Utils;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedFishingSpot : FishingSpot
{
    private uint? _icon { get; set; } = null;

    public uint Icon
        => _icon ??= !Rare ? 60465u : 60466u;

    public unsafe bool OpenMap(ExtendedItem? item = null, SeString? prefix = null)
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return false;

        var territoryType = TerritoryType.Value;
        if (territoryType == null)
            return false;

        var gatheringItemLevel = 0;
        if (item != null)
        {
            gatheringItemLevel = FindRow<FishParameter>(row => row?.Item == (item?.RowId ?? 0))
                ?.GatheringItemLevel.Value
                ?.GatheringItemLevel ?? 0;
        }

        static int convert(short pos, ushort scale) => (pos - 1024) / (scale / 100);

        var scale = territoryType!.Map.Value!.SizeFactor;
        var x = convert(X, scale);
        var y = convert(Z, scale);
        var radius = Radius / 7 / (scale / 100); // don't ask me why this works

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = gatheringItemLevel == 0
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText2(35, gatheringItemLevel, 0);

        using var tooltip = new DisposableUtf8String(levelText);

        var iconId = Rare ? 60466u : 60465u;

        agentMap->TempMapMarkerCount = 0;
        agentMap->AddGatheringTempMarker(
            4u,
            x,
            y,
            iconId,
            radius,
            tooltip
        );

        var titleBuilder = new SeStringBuilder();

        if (prefix != null)
        {
            titleBuilder.Append(prefix);
        }

        if (item != null)
        {
            if (prefix != null)
            {
                titleBuilder.AddText(" (");
            }

            titleBuilder
                .AddUiForeground(549)
                .AddUiGlow(550)
                .AddText(GetItemName(item.RowId))
                .AddUiGlowOff()
                .AddUiForegroundOff();

            if (prefix != null)
            {
                titleBuilder.AddText(")");
            }
        }

        using var title = new DisposableUtf8String(titleBuilder.BuiltString);

        var mapInfo = stackalloc OpenMapInfo[1];
        mapInfo->Type = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType.GatheringLog;
        mapInfo->MapId = territoryType.Map.Row;
        mapInfo->TerritoryId = territoryType.RowId;
        mapInfo->TitleString = *title.Ptr;
        agentMap->OpenMap(mapInfo);

        return true;
    }
}
