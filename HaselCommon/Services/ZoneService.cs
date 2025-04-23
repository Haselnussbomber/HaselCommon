using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class ZoneService
{
    private readonly ExcelService _excelService;

    public uint CurrentTerritoryTypeId => GameMain.Instance()->CurrentTerritoryTypeId;
    public uint CurrentTerritoryIntendedUseId => GameMain.Instance()->CurrentTerritoryIntendedUseId;

    public bool IsPvPZone(uint? territoryTypeId = null)
        => _excelService.TryGetRow<TerritoryType>(territoryTypeId ?? CurrentTerritoryTypeId, out var territoryType) && territoryType.IsPvpZone;
}
