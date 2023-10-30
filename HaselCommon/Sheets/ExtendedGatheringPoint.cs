using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Utils;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedGatheringPoint : GatheringPoint
{
    private uint? _icon { get; set; } = null;

    public uint Icon
    {
        get
        {
            if (_icon != null)
                return _icon ??= 0;

            var gatheringType = GatheringPointBase.Value?.GatheringType.Value;
            if (gatheringType == null)
                return _icon ??= 0;

            var rare = !IsGatheringPointRare.Invoke(Type);
            return _icon ??= rare ? (uint)gatheringType.IconMain : (uint)gatheringType.IconOff;
        }
    }

    public unsafe bool OpenMap(ExtendedItem? item = null, SeString? prefix = null)
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return false;

        var territoryType = TerritoryType.Value;
        if (territoryType == null)
            return false;

        var gatheringPointBase = GatheringPointBase.Value;
        if (gatheringPointBase == null)
            return false;

        var exportedPoint = GetRow<ExportedGatheringPoint>(gatheringPointBase.RowId);
        if (exportedPoint == null)
            return false;

        var gatheringType = exportedPoint.GatheringType.Value;
        if (gatheringType == null)
            return false;

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = gatheringPointBase.GatheringLevel == 1
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText2(35, gatheringPointBase.GatheringLevel, 0);
        var gatheringPointName = GetGatheringPointName(RowId);

        using var tooltip = new DisposableUtf8String(levelText);
        tooltip.Append(" " + gatheringPointName);

        var iconId = !IsGatheringPointRare.Invoke(exportedPoint.GatheringPointType)
            ? gatheringType.IconMain
            : gatheringType.IconOff;

        agentMap->TempMapMarkerCount = 0;
        agentMap->AddGatheringTempMarker(
            4u,
            (int)Math.Round(exportedPoint.X),
            (int)Math.Round(exportedPoint.Y),
            (uint)iconId,
            exportedPoint.Radius,
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

    internal unsafe delegate bool IsGatheringPointRareDelegate(byte gatheringPointType);
    internal static IsGatheringPointRareDelegate IsGatheringPointRare { get; } = MemoryUtils.GetDelegateForSignature<IsGatheringPointRareDelegate>("80 F9 07 77 10");
}
