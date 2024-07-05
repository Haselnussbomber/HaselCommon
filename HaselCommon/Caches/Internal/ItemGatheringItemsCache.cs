using HaselCommon.Caching;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caches.Internal;

internal class ItemGatheringItemsCache(ExcelService ExcelService) : MemoryCache<uint, GatheringItem[]>
{
    public override GatheringItem[]? CreateEntry(uint itemId)
        => ExcelService.FindRows<GatheringItem>(row => row?.Item == itemId);
}
