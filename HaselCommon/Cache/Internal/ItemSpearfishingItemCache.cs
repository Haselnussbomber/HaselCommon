using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caching.Internal;

internal class ItemSpearfishingItemCache(ExcelService ExcelService) : MemoryCache<uint, SpearfishingItem>
{
    public override SpearfishingItem? CreateEntry(uint itemId)
        => ExcelService.FindRow<SpearfishingItem>(row => row?.Item.Row == itemId);
}
