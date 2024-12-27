using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using HaselCommon.Game.Enums;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

public class TeleportService : IDisposable
{
    private readonly FrozenDictionary<uint, MapMarker> _aetheryteMapMarkers;
    private readonly IAetheryteList _aetheryteList;
    private readonly IClientState _clientState;
    private readonly ExcelService _excelService;
    private readonly UnlocksObserver _unlocksObserver;
    private readonly List<(Aetheryte Aetheryte, MapMarker Marker)> _aetherytes = [];

    public TeleportService(IAetheryteList aetheryteList, IClientState clientState, ExcelService excelService, UnlocksObserver unlocksObserver)
    {
        _aetheryteList = aetheryteList;
        _clientState = clientState;
        _excelService = excelService;
        _unlocksObserver = unlocksObserver;

        _aetheryteMapMarkers = excelService.GetSubrowSheet<MapMarker>()
            .SelectMany(subrows => subrows)
            .Cast<MapMarker?>()
            .Where(mapMarker => mapMarker != null && (MapMarkerDataType)mapMarker.Value.DataType == MapMarkerDataType.Aetheryte)
            .Cast<MapMarker>()
            .DistinctBy(m => m.DataKey.RowId)
            .ToFrozenDictionary(m => m.DataKey.RowId, m => m);

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
        _aetherytes.Clear();

        foreach (var entry in _aetheryteList)
        {
            if (!entry.AetheryteData.IsValid || !_aetheryteMapMarkers.TryGetValue(entry.AetheryteData.Value.RowId, out var marker))
                continue;

            _aetherytes.Add((entry.AetheryteData.Value, marker));
        }
    }

    public uint GetTeleportCost(uint destinationTerritoryTypeId)
    {
        var cost = 0u;

        foreach (var aetheryte in _aetheryteList)
        {
            if (aetheryte.TerritoryId == destinationTerritoryTypeId && (cost == 0 || aetheryte.GilCost < cost))
            {
                cost = aetheryte.GilCost;
            }
        }

        return cost;
    }

    public bool TryGetClosestAetheryte(Level level, out RowRef<Aetheryte> aetheryte)
    {
        var levelCoords = new Vector2(level.X, level.Z);

        var aetheryteId = level.RowId switch
        {
            // Ehll Tou or Charlemend are in the Firmament, so use Foundation instead
            8370121 or 8658159 => 70u,

            // Find Aetheryte in the same Map and TerritoryType
            _ => _aetherytes
                .Where(entry => entry.Aetheryte.Map.RowId == level.Map.RowId && entry.Aetheryte.Territory.RowId == level.Territory.RowId)
                .Select(entry => new
                {
                    entry.Aetheryte,
                    LengthSquared = (MapService.GetCoords(entry.Aetheryte.Map.Value, entry.Marker) - levelCoords).LengthSquared()
                })
                .OrderBy(entry => entry.LengthSquared)
                .FirstOrDefault()?
                .Aetheryte.RowId ?? 0u
        };

        // TODO: if no aetheryte was found in the same territory, find closest one in adjacent territories

        aetheryte = _excelService.CreateRef<Aetheryte>(aetheryteId);
        return aetheryteId != 0;
    }
}
