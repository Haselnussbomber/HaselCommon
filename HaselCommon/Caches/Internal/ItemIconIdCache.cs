using HaselCommon.Caching;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caches.Internal;

internal class ItemIconIdCache(ExcelService ExcelService) : MemoryCache<uint, uint>
{
    public override uint CreateEntry(uint itemId)
    {
        // EventItem
        if (itemId is > 2_000_000)
            return ExcelService.GetRow<EventItem>(itemId)?.Icon ?? 0;

        // HighQuality
        if (itemId is > 1_000_000 and < 2_000_000)
            itemId -= 1_000_000;

        // Collectible
        if (itemId is > 500_000 and < 1_000_000)
            itemId -= 500_000;

        return ExcelService.GetRow<Item>(itemId)?.Icon ?? 0;
    }
}
