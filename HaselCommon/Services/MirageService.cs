using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class MirageService
{
    private readonly ExcelService _excelService;

    public bool IsItemCollected(ItemHandle item, bool useCache = true)
    {
        // check if item is collected as part of set first
        if (_excelService.TryGetRow<MirageStoreSetItemLookup>(item, out var lookupRow))
        {
            foreach (var setItem in lookupRow.Item)
            {
                if (setItem.RowId == 0 || !setItem.IsValid)
                    continue;

                if (!_excelService.TryGetRow<MirageStoreSetItem>(setItem.RowId, out var setRow))
                    continue;

                foreach (var (slotIndex, slot) in setRow.Items.Index())
                {
                    if (slot.RowId != item.ItemId)
                        continue;

                    if (IsSetSlotCollected(setRow.RowId, slotIndex, useCache))
                        return true;
                }
            }
        }

        var mirageManager = MirageManager.Instance();
        if (mirageManager->PrismBoxLoaded)
        {
            return mirageManager->PrismBoxItemIds.IndexOf(item) != -1;
        }

        if (useCache)
        {
            var itemFinderModule = ItemFinderModule.Instance();
            return itemFinderModule->GlamourDresserItemIds.IndexOf(item) != -1;
        }

        return false;
    }

    public bool IsSetSlotCollected(ItemHandle setItem, int slotIndex, bool useCache = true)
    {
        var mirageManager = MirageManager.Instance();
        if (mirageManager->PrismBoxLoaded)
        {
            var itemIndex = mirageManager->PrismBoxItemIds.IndexOf(setItem);
            if (itemIndex != -1)
            {
                return mirageManager->IsSetSlotUnlocked((uint)itemIndex, slotIndex);
            }
        }

        if (useCache)
        {
            var itemFinderModule = ItemFinderModule.Instance();
            var itemIndex = itemFinderModule->GlamourDresserItemIds.IndexOf(setItem);
            if (itemIndex != -1)
            {
                var bitArray = new BitArray((byte*)itemFinderModule->GlamourDresserItemSetUnlockBits.GetPointer(itemIndex), MirageStoreSetItemExtensions.ItemCount);
                return !bitArray.Get(slotIndex);
            }
        }

        return false;
    }

    public bool IsFullSetCollected(ItemHandle setItem, bool useCache = true)
    {
        if (!_excelService.TryGetRow<MirageStoreSetItem>(setItem, out var setRow))
            return false;

        var collected = true;

        foreach (var (slotIndex, slot) in setRow.Items.Index())
        {
            if (slot.RowId == 0 || !slot.IsValid)
                continue;

            collected &= IsSetSlotCollected(setItem, slotIndex, useCache);
        }

        return collected;
    }
}
