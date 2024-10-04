using System.Linq;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caching.Internal;

internal class SpearfishingItemGatheringPointsCache(ExcelService ExcelService) : MemoryCache<uint, GatheringPoint[]>
{
    public override GatheringPoint[]? CreateEntry(uint spearfishingItemId)
    {
        var bases = ExcelService.FindRows<GatheringPointBase>(
            row => row != null && row.GatheringType.Row == 5 && row.Item.Any(item => item == spearfishingItemId));

        return ExcelService.FindRows<GatheringPoint>(
            row => row != null && bases.Any(b => b.RowId == row.GatheringPointBase.Row));
    }
}
