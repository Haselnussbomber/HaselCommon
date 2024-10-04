using System.Linq;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caching.Internal;

internal class ItemFishingSpotsCache(ExcelService ExcelService) : MemoryCache<uint, FishingSpot[]>
{
    public override FishingSpot[]? CreateEntry(uint itemId)
        => ExcelService.FindRows<FishingSpot>(row => row?.Item.Any(item => item.Row == itemId) ?? false);
}
