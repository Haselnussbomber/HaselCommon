using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;

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

            var rare = !IsGatheringTypeRare(Type);
            return _icon ??= rare ? (uint)gatheringType.IconMain : (uint)gatheringType.IconOff;
        }
    }

    public unsafe bool OpenMap(ExtendedItem? item = null, ReadOnlySeString? prefix = null)
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

        using var tooltip = new Utf8String(
            new SeStringBuilder()
                .Append(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(levelText))
                .Append(" " + gatheringPointName)
                .ToArray());

        var iconId = !IsGatheringTypeRare(exportedPoint.GatheringPointType)
            ? gatheringType.IconMain
            : gatheringType.IconOff;

        agentMap->TempMapMarkerCount = 0;
        agentMap->AddGatheringTempMarker(
            4u,
            (int)Math.Round(exportedPoint.X),
            (int)Math.Round(exportedPoint.Y),
            (uint)iconId,
            exportedPoint.Radius,
            &tooltip
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
                titleBuilder.Append(" (");
            }

            titleBuilder
                .PushColorType(549)
                .PushEdgeColorType(550)
                .Append(GetItemName(item.RowId))
                .PopEdgeColorType()
                .PopColorType();

            if (prefix != null)
            {
                titleBuilder.Append(")");
            }
        }

        using var title = new Utf8String(titleBuilder.ToArray());

        var mapInfo = new OpenMapInfo
        {
            Type = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType.GatheringLog,
            MapId = territoryType.Map.Row,
            TerritoryId = territoryType.RowId,
            TitleString = title
        };
        agentMap->OpenMap(&mapInfo);

        return true;
    }
}
