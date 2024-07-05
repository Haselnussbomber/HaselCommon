using System.Numerics;
using System.Text;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Extensions;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services;

public class MapService(IClientState ClientState, IGameGui GameGui, TextService TextService, ExcelService ExcelService)
{
    public static Vector2 GetCoords(Level level)
    {
        var map = level.Map.Value;
        var c = map!.SizeFactor / 100.0f;
        var x = 41.0f / c * (((level.X + map.OffsetX) * c + 1024.0f) / 2048.0f) + 1f;
        var y = 41.0f / c * (((level.Z + map.OffsetY) * c + 1024.0f) / 2048.0f) + 1f;
        return new(x, y);
    }

    public float GetDistanceFromPlayer(Level level)
    {
        var localPlayer = ClientState.LocalPlayer;
        if (localPlayer == null || level.Territory.Row != ClientState.TerritoryType)
            return float.MaxValue; // far, far away

        return Vector2.Distance(
            new Vector2(localPlayer.Position.X, localPlayer.Position.Z),
            new Vector2(level.X, level.Z)
        );
    }

    public ReadOnlySeString? GetMapLink(IGameObject obj)
    {
        var territoryId = ClientState.TerritoryType;
        if (territoryId == 0)
            return null;

        var territoryType = ExcelService.GetRow<TerritoryType>(territoryId);
        if (territoryType == null)
            return null;

        // TODO: evaluate addon string?
        var marker = new SeStringBuilder()
            .PushColorType(500)
            .PushEdgeColorType(501)
            .Append($"{(char)SeIconChar.LinkMarker}{ClientState.ClientLanguage switch
            {
                ClientLanguage.German => " ",
                ClientLanguage.French => " ",
                _ => string.Empty,
            }}")
            .PopEdgeColorType()
            .PopColorType()
            .ToReadOnlySeString();

        // TODO: TEST
        var sb = new SeStringBuilder()
            .PushLinkMapPosition(territoryId, territoryType.Map.Row, (int)(obj.Position.X * 1_000f), (int)(obj.Position.Z * 1_000f))
            .Append(marker)
            .Append(" ")
            .PushColorType(1)
            .Append(obj.Name.Encode())
            .PopColorType()
            .PopLink();

        return sb.ToReadOnlySeString();
    }

    public void OpenMap(Level? level)
    {
        if (level == null)
            return;

        var map = level.Map.Value;
        if (map == null)
            return;

        var terr = map.TerritoryType.Value;
        if (terr == null)
            return;

        GameGui.OpenMapWithMapLink(new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(
            terr.RowId,
            map.RowId,
            (int)(level.X * 1_000f),
            (int)(level.Z * 1_000f)
        ));
    }

    public unsafe bool OpenMap(GatheringPoint point, Item? item = null, string? prefix = null)
        => OpenMap(point, item, prefix != null ? new ReadOnlySeString(Encoding.UTF8.GetBytes(prefix)) : (ReadOnlySeString?)null);

    public unsafe bool OpenMap(GatheringPoint point, Item? item = null, ReadOnlySeString? prefix = null)
    {
        var territoryType = point.TerritoryType.Value;
        if (territoryType == null)
            return false;

        var gatheringPointBase = point.GatheringPointBase.Value;
        if (gatheringPointBase == null)
            return false;

        var exportedPoint = ExcelService.GetRow<ExportedGatheringPoint>(gatheringPointBase.RowId);
        if (exportedPoint == null)
            return false;

        var gatheringType = exportedPoint.GatheringType.Value;
        if (gatheringType == null)
            return false;

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = gatheringPointBase.GatheringLevel == 1
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText1IntIntUInt(35, gatheringPointBase.GatheringLevel, 0, 0);
        var gatheringPointName = TextService.GetGatheringPointName(point.RowId);

        using var tooltip = new Utf8String(
            new SeStringBuilder()
                .Append(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(levelText))
                .Append(" " + gatheringPointName)
                .ToArray());

        var iconId = !IsGatheringTypeRare(exportedPoint.GatheringPointType)
            ? gatheringType.IconMain
            : gatheringType.IconOff;

        return AddGatheringMarkerAndOpenMap(
            territoryType,
            (int)Math.Round(exportedPoint.X),
            (int)Math.Round(exportedPoint.Y),
            exportedPoint.Radius,
            (uint)iconId,
            tooltip,
            item,
            prefix);
    }

    public unsafe bool OpenMap(FishingSpot fishingSpot, Item? item = null, ReadOnlySeString? prefix = null)
    {
        var territoryType = fishingSpot.TerritoryType.Value;
        if (territoryType == null)
            return false;

        var gatheringItemLevel = 0;
        if (item != null)
        {
            gatheringItemLevel = ExcelService.FindRow<FishParameter>(row => row?.Item == (item?.RowId ?? 0))
                ?.GatheringItemLevel.Value
                ?.GatheringItemLevel ?? 0;
        }

        static int convert(short pos, ushort scale) => (pos - 1024) / (scale / 100);

        var scale = territoryType!.Map.Value!.SizeFactor;
        var x = convert(fishingSpot.X, scale);
        var y = convert(fishingSpot.Z, scale);
        var radius = fishingSpot.Radius / 7 / (scale / 100); // don't ask me why this works

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = gatheringItemLevel == 0
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText1IntIntUInt(35, gatheringItemLevel, 0, 0);

        using var tooltip = new Utf8String(
            new SeStringBuilder()
                .Append(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(levelText))
                .ToArray());

        var iconId = fishingSpot.Rare ? 60466u : 60465u;

        return AddGatheringMarkerAndOpenMap(territoryType, x, y, radius, iconId, tooltip, item, prefix);
    }

    private unsafe bool AddGatheringMarkerAndOpenMap(TerritoryType territoryType, int x, int y, int radius, uint iconId, Utf8String tooltip, Item? item = null, ReadOnlySeString? prefix = null)
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return false;

        agentMap->TempMapMarkerCount = 0;
        agentMap->AddGatheringTempMarker(
            4u,
            x,
            y,
            iconId,
            radius,
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
                .Append(TextService.GetItemName(item.RowId))
                .PopEdgeColorType()
                .PopColorType();

            if (prefix != null)
            {
                titleBuilder.Append(")");
            }
        }

        using var title = new Utf8String(new ReadOnlySpan<byte>(titleBuilder.ToArray()).WithNullTerminator());

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
