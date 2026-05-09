using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using CabinetSheet = Lumina.Excel.Sheets.Cabinet;
using CabinetState = FFXIVClientStructs.FFXIV.Client.Game.UI.Cabinet.CabinetState;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class CabinetService
{
    private readonly ExcelService _excelService;
    private readonly Dictionary<uint, uint> _items = [];

    [AutoPostConstruct]
    private void Initialize()
    {
        foreach (var row in _excelService.GetSheet<CabinetSheet>())
        {
            if (row.Item.RowId == 0 || !row.Item.IsValid)
                continue;

            _items[row.Item.RowId] = row.RowId;
        }
    }

    public bool TryGetCabinetId(ItemHandle item, out uint cabinetId)
    {
        return _items.TryGetValue(item, out cabinetId);
    }

    public bool IsItemCollected(ItemHandle item, bool useCache = true)
    {
        return _items.TryGetValue(item.ItemId, out var cabinetId) && IsCabinetItemCollected(cabinetId, useCache);
    }

    public bool IsCabinetItemCollected(uint cabinetId, bool useCache = true)
    {
        ref var cabinet = ref UIState.Instance()->Cabinet;

        if (cabinet.State == CabinetState.Loaded)
        {
            return cabinet.IsItemInCabinet(cabinetId);
        }

        if (useCache)
        {
            var itemFinderModule = ItemFinderModule.Instance();
            var bitArray = new BitArray((byte*)itemFinderModule->CabinetItemUnlockBits.GetPointer(0), _excelService.GetRowCount<CabinetSheet>());
            return bitArray.Get((int)cabinetId);
        }

        return false;
    }
}
