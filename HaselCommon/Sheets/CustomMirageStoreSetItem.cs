using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace HaselCommon.Sheets;

/// <summary>
/// <see cref="MirageStoreSetItem"/>
/// </summary>
[Sheet("MirageStoreSetItem")]
public readonly unsafe struct CustomMirageStoreSetItem(ExcelPage page, uint offset, uint row) : IExcelRow<CustomMirageStoreSetItem>
{
    public ExcelPage ExcelPage => page;
    public uint RowOffset => offset;
    public uint RowId => row;

    public readonly RowRef<Item> Set => new(page.Module, RowId, page.Language);

    /* based on EquipSlotCategory sheet used in E8 ?? ?? ?? ?? 85 C0 74 56 48 8B 0D */
    public readonly Collection<RowRef<Item>> Items => new(page, parentOffset: offset, offset: offset, &ItemCtor, size: 11);
    public readonly RowRef<Item> MainHand => new(page.Module, page.ReadUInt32(offset), page.Language);
    public readonly RowRef<Item> OffHand => new(page.Module, page.ReadUInt32(offset + 4), page.Language);
    public readonly RowRef<Item> Head => new(page.Module, page.ReadUInt32(offset + 8), page.Language);
    public readonly RowRef<Item> Body => new(page.Module, page.ReadUInt32(offset + 12), page.Language);
    public readonly RowRef<Item> Hands => new(page.Module, page.ReadUInt32(offset + 16), page.Language);
    public readonly RowRef<Item> Legs => new(page.Module, page.ReadUInt32(offset + 20), page.Language);
    public readonly RowRef<Item> Feet => new(page.Module, page.ReadUInt32(offset + 24), page.Language);
    public readonly RowRef<Item> Earrings => new(page.Module, page.ReadUInt32(offset + 28), page.Language);
    public readonly RowRef<Item> Necklace => new(page.Module, page.ReadUInt32(offset + 32), page.Language);
    public readonly RowRef<Item> Bracelets => new(page.Module, page.ReadUInt32(offset + 36), page.Language);
    public readonly RowRef<Item> Ring => new(page.Module, page.ReadUInt32(offset + 40), page.Language);

    public unsafe bool TryGetSetItemBitArray(out BitArray bitArray, bool useCache = true)
    {
        var mirageManager = MirageManager.Instance();
        if (mirageManager->PrismBoxLoaded)
        {
            var prismBoxItemIndex = mirageManager->PrismBoxItemIds.IndexOf(RowId);
            if (prismBoxItemIndex != -1)
            {
                bitArray = new BitArray(mirageManager->PrismBoxStain0Ids.GetPointer(prismBoxItemIndex), Items.Count);
                return true;
            }
        }

        if (useCache)
        {
            var itemFinderModule = ItemFinderModule.Instance();
            var glamourDresserIndex = itemFinderModule->GlamourDresserItemIds.IndexOf(RowId);
            if (glamourDresserIndex != -1)
            {
                bitArray = new BitArray((byte*)itemFinderModule->GlamourDresserItemSetUnlockBits.GetPointer(glamourDresserIndex), Items.Count);
                return true;
            }
        }

        bitArray = default;
        return false;
    }

    private static RowRef<Item> ItemCtor(ExcelPage page, uint parentOffset, uint offset, uint i)
        => new(page.Module, page.ReadUInt32(offset + i * 4), page.Language);

    static CustomMirageStoreSetItem IExcelRow<CustomMirageStoreSetItem>.Create(ExcelPage page, uint offset, uint row)
        => new(page, offset, row);
}
