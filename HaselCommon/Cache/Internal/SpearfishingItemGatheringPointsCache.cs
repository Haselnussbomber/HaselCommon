using System.Linq;
using HaselCommon.Services;
using Lumina.Excel.Sheets;

namespace HaselCommon.Cache.Internal;

internal class SpearfishingItemGatheringPointsCache(ExcelService excelService) : MemoryCache<uint, GatheringPoint[]>
{
    public override GatheringPoint[] CreateEntry(uint spearfishingItemId)
    {
        var bases = excelService.FindRows<GatheringPointBase>(row => row.GatheringType.RowId == 5 && row.Item.Any(item => item.RowId == spearfishingItemId));
        return excelService.FindRows<GatheringPoint>(row => bases.Any(b => b.RowId == row.GatheringPointBase.RowId));
    }
}
