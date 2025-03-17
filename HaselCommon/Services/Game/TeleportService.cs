using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Events;
using HaselCommon.Game.Enums;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class TeleportService
{
    private readonly ExcelService _excelService;
    private readonly EventDispatcher _eventDispatcher;

    [AutoPostConstruct]
    private void Initialize()
    {
        _eventDispatcher.Subscribe(PlayerEvents.Login, OnLogin);
        _eventDispatcher.Subscribe(PlayerEvents.UnlocksChanged, OnUnlocksChanged);

        if (AgentLobby.Instance()->IsLoggedIn)
            UpdateAetherytes();
    }

    private void OnLogin()
    {
        UpdateAetherytes();
    }

    private void OnUnlocksChanged()
    {
        UpdateAetherytes();
    }

    private void UpdateAetherytes()
    {
        if (Control.GetLocalPlayer() != null)
            Telepo.Instance()->UpdateAetheryteList();
    }

    public uint GetTeleportCost(uint destinationTerritoryTypeId)
    {
        var cost = 0u;

        foreach (var teleportInfo in Telepo.Instance()->TeleportList)
        {
            if (teleportInfo.TerritoryId == destinationTerritoryTypeId && (cost == 0 || teleportInfo.GilCost < cost))
            {
                cost = teleportInfo.GilCost;
            }
        }

        return cost;
    }

    public bool TryGetClosestAetheryte(Level level, out Aetheryte aetheryte)
    {
        // Ehll Tou or Charlemend are in the Firmament, so use Foundation instead
        if (level.RowId is 8370121 or 8658159)
        {
            return _excelService.TryGetRow(70, out aetheryte);
        }

        var levelCoords = new Vector2(level.X, level.Z);
        var aetherytes = _excelService.FindRows<Aetheryte>(row => row.Map.RowId == level.Map.RowId && row.Territory.RowId == level.Territory.RowId);

        aetheryte = default;
        var currentDistance = float.MaxValue;

        foreach (var aetheryteRow in aetherytes)
        {
            if (_excelService.TryFindSubrow<MapMarker>(row => row.DataType == (byte)MapMarkerDataType.Aetheryte && row.DataKey.RowId == aetheryteRow.RowId, out var mapMarker))
            {
                var distance = (MapService.GetCoords(aetheryteRow.Map.Value, mapMarker) - levelCoords).LengthSquared();
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    aetheryte = aetheryteRow;
                }
            }
        }

        // TODO: if no aetheryte was found in the same territory, find closest one in adjacent territories

        return currentDistance != float.MaxValue;
    }
}
