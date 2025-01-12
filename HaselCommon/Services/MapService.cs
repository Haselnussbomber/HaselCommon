using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Extensions.Sheets;
using HaselCommon.Game;
using HaselCommon.Services.SeStringEvaluation;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;
using Map = Lumina.Excel.Sheets.Map;

namespace HaselCommon.Services;

[RegisterSingleton]
public class MapService(IClientState clientState, IGameGui gameGui, TextService textService, ExcelService excelService, SeStringEvaluatorService seStringEvaluatorService)
{
    public static Vector2 GetCoords(Level level)
    {
        var map = level.Map.Value;
        var c = map!.SizeFactor / 100.0f;
        var x = 41.0f / c * (((level.X + map.OffsetX) * c + 1024.0f) / 2048.0f) + 1f;
        var y = 41.0f / c * (((level.Z + map.OffsetY) * c + 1024.0f) / 2048.0f) + 1f;
        return new(x, y);
    }

    public static Vector2 GetCoords(Map map, MapMarker mapMarker)
    {
        var scale = map.SizeFactor / 100f;
        var x = (int)(2 * mapMarker.X / scale + 100.9);
        var y = (int)(2 * mapMarker.Y / scale + 100.9);
        return new(x, y);
    }

    public float GetDistanceFromPlayer(Level level)
    {
        var localPlayer = clientState.LocalPlayer;
        if (localPlayer == null || level.Territory.RowId != clientState.TerritoryType)
            return float.MaxValue; // far, far away

        return Vector2.Distance(
            new Vector2(localPlayer.Position.X, localPlayer.Position.Z),
            new Vector2(level.X, level.Z)
        );
    }

    public unsafe ReadOnlySeString? GetMapLink(IGameObject obj, ClientLanguage? language = null)
    {
        var territoryId = GameMain.Instance()->CurrentTerritoryTypeId;
        if (territoryId == 0)
            return null;

        if (!excelService.TryGetRow<TerritoryType>(territoryId, out var territoryType))
            return null;

        var mapId = TerritoryInfo.Instance()->ChatLinkMapIdOverride;
        if (mapId == 0)
            mapId = GameMain.Instance()->CurrentMapId;
        if (mapId == 0)
            return null;

        if (!excelService.TryGetRow<Lumina.Excel.Sheets.Map>(mapId, out var map))
            return null;

        if (!excelService.TryGetRow<PlaceName>(territoryType.PlaceName.RowId, language, out var placeName))
            return null;

        var placeNameWithInstanceBuilder = SeStringBuilder.SharedPool.Get()
            .Append(placeName.Name);

        var instanceId = Framework.Instance()->GetNetworkModuleProxy()->GetCurrentInstance();
        if (instanceId > 0)
            placeNameWithInstanceBuilder.Append((char)(SeIconChar.Instance1 + (byte)(instanceId - 1)));

        var placeNameWithInstance = placeNameWithInstanceBuilder.ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(placeNameWithInstanceBuilder);

        var mapPosX = map.ConvertRawToMapPosX(obj.Position.X);
        var mapPosY = map.ConvertRawToMapPosY(obj.Position.Z);

        ReadOnlySeString linkText;
        if (!excelService.TryGetRow<TerritoryTypeTransient>(territoryId, out var territoryTransient) && territoryTransient.OffsetZ != -10000)
        {
            var zFloat = obj.Position.Y - territoryTransient.OffsetZ;
            var z = (uint)(int)zFloat;
            if (zFloat < 0.0 && zFloat != (int)z)
                z -= 10;
            z /= 10;

            linkText = seStringEvaluatorService.EvaluateFromAddon(1636, new SeStringContext()
            {
                Language = language,
                LocalParameters = [placeNameWithInstance, mapPosX, mapPosY, z]
            });
        }
        else
        {
            linkText = seStringEvaluatorService.EvaluateFromAddon(1635, new SeStringContext()
            {
                Language = language,
                LocalParameters = [placeNameWithInstance, mapPosX, mapPosY]
            });
        }

        var sb = SeStringBuilder.SharedPool.Get();
        var mapLink = sb
            .PushLinkMapPosition(territoryId, mapId, (int)(obj.Position.X * 1000f), (int)(obj.Position.Z * 1000f))
            .Append(linkText)
            .PopLink()
            .ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);

        // Link Marker
        return seStringEvaluatorService.EvaluateFromAddon(371, new SeStringContext()
        {
            Language = language,
            LocalParameters = [mapLink]
        });
    }

    public void OpenMap(Level level)
    {
        if (!level.Map.IsValid || !level.Map.Value.TerritoryType.IsValid)
            return;

        gameGui.OpenMapWithMapLink(new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(
            level.Map.Value.TerritoryType.RowId,
            level.Map.RowId,
            (int)(level.X * 1_000f),
            (int)(level.Z * 1_000f)
        ));
    }

    public unsafe bool OpenMap(GatheringPoint point, RowRef<Item> itemRef, ReadOnlySeString? prefix = null)
    {
        if (!point.TerritoryType.IsValid)
            return false;

        if (!point.GatheringPointBase.IsValid)
            return false;

        if (!excelService.TryGetRow<ExportedGatheringPoint>(point.GatheringPointBase.Value.RowId, out var exportedPoint))
            return false;

        if (!exportedPoint.GatheringType.IsValid)
            return false;

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = point.GatheringPointBase.Value.GatheringLevel == 1
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText1IntIntUInt(35, point.GatheringPointBase.Value.GatheringLevel, 0, 0);
        var gatheringPointName = textService.GetGatheringPointName(point.RowId);

        var sb = SeStringBuilder.SharedPool.Get();
        using var tooltip = new Utf8String(sb
            .Append(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(levelText))
            .Append(" " + gatheringPointName)
            .GetViewAsSpan());
        SeStringBuilder.SharedPool.Return(sb);

        var iconId = !Misc.IsGatheringTypeRare(exportedPoint.GatheringPointType)
            ? exportedPoint.GatheringType.Value.IconMain
            : exportedPoint.GatheringType.Value.IconOff;

        return AddGatheringMarkerAndOpenMap(
            point.TerritoryType.Value,
            (int)MathF.Round(exportedPoint.X),
            (int)MathF.Round(exportedPoint.Y),
            exportedPoint.Radius,
            (uint)iconId,
            tooltip,
            itemRef,
            prefix);
    }

    public unsafe bool OpenMap(FishingSpot fishingSpot, RowRef<Item> itemRef, ReadOnlySeString? prefix = null)
    {
        if (!fishingSpot.TerritoryType.IsValid)
            return false;

        var territoryType = fishingSpot.TerritoryType.Value;

        var gatheringItemLevel = 0;
        if (itemRef.IsValid
            && excelService.TryFindRow<FishParameter>(row => row.Item.RowId == itemRef.RowId, out var fishParameter)
            && fishParameter.GatheringItemLevel.IsValid)
        {
            gatheringItemLevel = fishParameter.GatheringItemLevel.Value.GatheringItemLevel;
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

        var sb = SeStringBuilder.SharedPool.Get();
        using var tooltip = new Utf8String(sb
            .Append(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(levelText))
            .GetViewAsSpan());
        SeStringBuilder.SharedPool.Return(sb);

        var iconId = fishingSpot.Rare ? 60466u : 60465u;

        return AddGatheringMarkerAndOpenMap(territoryType, x, y, radius, iconId, tooltip, itemRef, prefix);
    }

    private unsafe bool AddGatheringMarkerAndOpenMap(TerritoryType territoryType, int x, int y, int radius, uint iconId, Utf8String tooltip, RowRef<Item> itemRef, ReadOnlySeString? prefix = null)
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

        var titleBuilder = SeStringBuilder.SharedPool.Get();

        if (prefix != null)
        {
            titleBuilder.Append(prefix);
        }

        if (itemRef.IsValid)
        {
            if (prefix != null)
            {
                titleBuilder.Append(" (");
            }

            titleBuilder
                .PushColorType(549)
                .PushEdgeColorType(550)
                .Append(textService.GetItemName(itemRef.RowId))
                .PopEdgeColorType()
                .PopColorType();

            if (prefix != null)
            {
                titleBuilder.Append(")");
            }
        }

        using var title = new Utf8String(titleBuilder.GetViewAsSpan());

        SeStringBuilder.SharedPool.Return(titleBuilder);

        var mapInfo = new OpenMapInfo
        {
            Type = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType.GatheringLog,
            MapId = territoryType.Map.RowId,
            TerritoryId = territoryType.RowId,
            TitleString = title
        };

        agentMap->OpenMap(&mapInfo);

        return true;
    }
}
