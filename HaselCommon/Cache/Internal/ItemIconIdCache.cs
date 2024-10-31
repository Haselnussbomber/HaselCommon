using HaselCommon.Services;
using Lumina.Excel.Sheets;

namespace HaselCommon.Cache.Internal;

internal class ItemIconIdCache(ExcelService excelService) : MemoryCache<uint, uint>
{
    public override uint CreateEntry(uint itemId)
    {
        // EventItem
        if (itemId is > 2_000_000)
            return excelService.TryGetRow<EventItem>(itemId, out var eventItem) ? eventItem.Icon : 0u;

        // HighQuality
        if (itemId is > 1_000_000 and < 2_000_000)
            itemId -= 1_000_000;

        // Collectible
        if (itemId is > 500_000 and < 1_000_000)
            itemId -= 500_000;

        return excelService.TryGetRow<Item>(itemId, out var item) ? item.Icon : 0u;
    }
}
