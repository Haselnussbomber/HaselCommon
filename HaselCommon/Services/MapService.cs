using System.Globalization;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Map = Lumina.Excel.Sheets.Map;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class MapService
{
    private readonly IClientState _clientState;
    private readonly IGameGui _gameGui;
    private readonly TextService _textService;
    private readonly ExcelService _excelService;
    private readonly SeStringEvaluator _seStringEvaluator;

    private static readonly string[] CompassHeadings = ["E", "NE", "N", "NW", "W", "SW", "S", "SE"];

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

    public string GetHumanReadableCoords(Level level)
    {
        var coords = GetCoords(level);
        var x = coords.X.ToString("0.0", CultureInfo.InvariantCulture);
        var y = coords.Y.ToString("0.0", CultureInfo.InvariantCulture);
        return _textService.Translate("CoordsXY", x, y);
    }

    public unsafe float GetDistanceFromPlayer(Level level)
    {
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null || level.Territory.RowId != _clientState.TerritoryType)
            return float.MaxValue; // far, far away

        return Vector2.Distance(
            new Vector2(localPlayer->Position.X, localPlayer->Position.Z),
            new Vector2(level.X, level.Z)
        );
    }

    //! https://gamedev.stackexchange.com/a/49300
    public string GetCompassDirection(Vector2 a, Vector2 b)
    {
        var vector = a - b;
        var angle = MathF.Atan2(vector.Y, vector.X);
        var octant = (int)MathF.Round(8 * angle / (2 * MathF.PI) + 8) % 8;

        return _textService.Translate($"CompassHeadings.{CompassHeadings[octant]}");
    }

    public unsafe string GetCompassDirection(Level level)
    {
        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
            return string.Empty;

        return GetCompassDirection(
            new Vector2(-localPlayer->Position.X, localPlayer->Position.Z),
            new Vector2(-level.X, level.Z)
        );
    }

    public unsafe ReadOnlySeString? GetMapLink(IGameObject obj, ClientLanguage? language = null)
    {
        var territoryId = GameMain.Instance()->CurrentTerritoryTypeId;
        if (territoryId == 0)
            return null;

        if (!_excelService.TryGetRow<TerritoryType>(territoryId, out var territoryType))
            return null;

        var mapId = TerritoryInfo.Instance()->ChatLinkMapIdOverride;
        if (mapId == 0)
            mapId = GameMain.Instance()->CurrentMapId;
        if (mapId == 0)
            return null;

        if (!_excelService.TryGetRow<Lumina.Excel.Sheets.Map>(mapId, out var map))
            return null;

        if (!_excelService.TryGetRow<PlaceName>(territoryType.PlaceName.RowId, language, out var placeName))
            return null;

        ReadOnlySeString placeNameWithInstance;
        using (SeStringBuilderHelper.Rent(out var sb))
        {
            sb.Append(placeName.Name);

            var instanceId = Framework.Instance()->GetNetworkModuleProxy()->GetCurrentInstance();
            if (instanceId > 0)
                sb.Append((char)(SeIconChar.Instance1 + (byte)(instanceId - 1)));

            placeNameWithInstance = sb.ToReadOnlySeString();
        }

        var mapPosX = map.ConvertRawToMapPosX(obj.Position.X);
        var mapPosY = map.ConvertRawToMapPosY(obj.Position.Z);

        ReadOnlySeString linkText;
        if (!_excelService.TryGetRow<TerritoryTypeTransient>(territoryId, out var territoryTransient) && territoryTransient.OffsetZ != -10000)
        {
            var zFloat = obj.Position.Y - territoryTransient.OffsetZ;
            var z = (uint)(int)zFloat;
            if (zFloat < 0.0 && zFloat != (int)z)
                z -= 10;
            z /= 10;

            linkText = _seStringEvaluator.EvaluateFromAddon(1636, [placeNameWithInstance, mapPosX, mapPosY, z], language);
        }
        else
        {
            linkText = _seStringEvaluator.EvaluateFromAddon(1635, [placeNameWithInstance, mapPosX, mapPosY], language);
        }

        ReadOnlySeString mapLink;
        using (SeStringBuilderHelper.Rent(out var sb))
        {
            mapLink = sb
                .PushLinkMapPosition(territoryId, mapId, (int)(obj.Position.X * 1000f), (int)(obj.Position.Z * 1000f))
                .Append(linkText)
                .PopLink()
                .ToReadOnlySeString();
        }

        // Link Marker
        return _seStringEvaluator.EvaluateFromAddon(371, [mapLink], language);
    }

    public void OpenMap(Level level)
    {
        if (!level.Map.IsValid || !level.Map.Value.TerritoryType.IsValid)
            return;

        _gameGui.OpenMapWithMapLink(new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(
            level.Map.Value.TerritoryType.RowId,
            level.Map.RowId,
            (int)(level.X * 1_000f),
            (int)(level.Z * 1_000f)
        ));
    }

    private static readonly uint[,] GatheringPointNameMapping = new uint[4, 5]
    {
        {  1,  9, 13, 23,  5 },
        {  2, 10, 14, 24,  6 },
        {  3, 11, 15, 25,  7 },
        {  4, 12, 16, 26,  8 }
    };

    public unsafe bool OpenMap(GatheringPoint point, uint itemId, ReadOnlySeString? prefix = null)
    {
        if (!point.TerritoryType.IsValid)
            return false;

        if (!point.GatheringPointBase.IsValid)
            return false;

        if (!_excelService.TryGetRow<ExportedGatheringPoint>(point.GatheringPointBase.Value.RowId, out var exportedPoint))
            return false;

        if (!exportedPoint.GatheringType.IsValid)
            return false;

        var raptureTextModule = RaptureTextModule.Instance();

        var levelText = point.GatheringPointBase.Value.GatheringLevel == 1
            ? raptureTextModule->GetAddonText(242) // "Lv. ???"
            : raptureTextModule->FormatAddonText1IntIntUInt(35, point.GatheringPointBase.Value.GatheringLevel, 0, 0);

        var isTimed = UIGlobals.IsExportedGatheringPointTimed(exportedPoint.GatheringPointType);

        var offset = isTimed
            ? exportedPoint.GatheringPointType switch
            {
                2 => 4,
                4 => 1,
                5 => 2,
                8 => 3,
                _ => 4,
            }
            : 0;

        var gatheringPointName = _seStringEvaluator.EvaluateObjStr(
            ObjectKind.GatheringPoint,
            GatheringPointNameMapping[exportedPoint.GatheringType.RowId, offset]);

        using var tooltip = new Utf8String();
        using (SeStringBuilderHelper.Rent(out var sb))
        {
            tooltip.SetString(sb
                .Append(levelText.AsSpan())
                .Append(" ")
                .Append(gatheringPointName)
                .GetViewAsSpan());
        }

        var iconId = !isTimed
            ? exportedPoint.GatheringType.Value.IconMain
            : exportedPoint.GatheringType.Value.IconOff;

        return AddGatheringMarkerAndOpenMap(
            point.TerritoryType.Value,
            (int)MathF.Round(exportedPoint.X),
            (int)MathF.Round(exportedPoint.Y),
            exportedPoint.Radius,
            (uint)iconId,
            tooltip,
            itemId,
            prefix);
    }

    public unsafe bool OpenMap(FishingSpot fishingSpot, uint itemId, ReadOnlySeString? prefix = null)
    {
        if (!fishingSpot.TerritoryType.IsValid)
            return false;

        var territoryType = fishingSpot.TerritoryType.Value;

        var gatheringItemLevel = 0;
        if (itemId != 0
            && _excelService.TryFindRow<FishParameter>(row => row.Item.RowId == itemId, out var fishParameter)
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

        using var tooltip = new Utf8String(levelText.Value);
        var iconId = fishingSpot.Rare ? 60466u : 60465u;
        return AddGatheringMarkerAndOpenMap(territoryType, x, y, radius, iconId, tooltip, itemId, prefix);
    }

    private unsafe bool AddGatheringMarkerAndOpenMap(TerritoryType territoryType, int x, int y, int radius, uint iconId, Utf8String tooltip, uint itemId, ReadOnlySeString? prefix = null)
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

        using var title = new Utf8String();
        using (SeStringBuilderHelper.Rent(out var titleBuilder))
        {
            if (prefix != null)
            {
                titleBuilder.Append(prefix);
            }

            if (itemId != 0 && _excelService.GetSheet<Item>().HasRow(itemId))
            {
                if (prefix != null)
                {
                    titleBuilder.Append(" (");
                }

                titleBuilder
                    .PushColorType(549)
                    .PushEdgeColorType(550)
                    .Append(_textService.GetItemName(itemId))
                    .PopEdgeColorType()
                    .PopColorType();

                if (prefix != null)
                {
                    titleBuilder.Append(")");
                }
            }

            title.SetString(titleBuilder.GetViewAsSpan());
        }

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
