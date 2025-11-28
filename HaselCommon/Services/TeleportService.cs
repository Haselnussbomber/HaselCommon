using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using HaselCommon.Game.Enums;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class TeleportService : IDisposable
{
    private readonly ILogger<TeleportService> _logger;
    private readonly IFramework _framework;
    private readonly IClientState _clientState;
    private readonly ExcelService _excelService;
    private readonly IUnlockState _unlockState;
    private Debouncer _updateAetherytesDebouncer;

    [AutoPostConstruct]
    private void Initialize()
    {
        _updateAetherytesDebouncer = _framework.CreateDebouncer(TimeSpan.FromMilliseconds(200), UpdateAetherytes);

        _unlockState.Unlock += OnUnlock;

        if (_clientState.IsLoggedIn)
            _updateAetherytesDebouncer.Debounce();
    }

    public void Dispose()
    {
        _unlockState.Unlock -= OnUnlock;
        _updateAetherytesDebouncer.Dispose();
    }

    private void OnUnlock(RowRef rowRef)
    {
        _updateAetherytesDebouncer.Debounce();
    }

    private void UpdateAetherytes()
    {
        if (Control.GetLocalPlayer() != null)
        {
            _logger.LogTrace("Updating aetheryte list.");
            Telepo.Instance()->UpdateAetheryteList();
        }
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
        if (!level.Territory.IsValid)
        {
            aetheryte = default;
            return false;
        }

        if (level.Territory.Value.Aetheryte.RowId != 0 && level.Territory.Value.Aetheryte.IsValid)
        {
            aetheryte = level.Territory.Value.Aetheryte.Value;
            return true;
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

        return currentDistance != float.MaxValue;
    }
}
