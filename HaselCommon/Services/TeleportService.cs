using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using HaselCommon.Game.Enums;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class TeleportService : IDisposable
{
    private readonly IClientState _clientState;
    private readonly ExcelService _excelService;
    private readonly UnlocksObserver _unlocksObserver;

    [AutoPostConstruct]
    private void Initialize()
    {
        _clientState.Login += UpdateAetherytes;
        _unlocksObserver.Update += UpdateAetherytes;

        if (_clientState.IsLoggedIn)
            UpdateAetherytes();
    }

    public void Dispose()
    {
        _unlocksObserver.Update -= UpdateAetherytes;
        _clientState.Login -= UpdateAetherytes;
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
