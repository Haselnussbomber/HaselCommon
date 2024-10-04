using System.Collections.Generic;
using System.Linq;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caching.Internal;

internal class GatheringItemGatheringPointsCache(ExcelService ExcelService) : MemoryCache<uint, GatheringPoint[]>
{
    public override GatheringPoint[]? CreateEntry(uint gatheringItemId)
    {
        var gatheringItem = ExcelService.GetRow<GatheringItem>(gatheringItemId);
        if (gatheringItem == null)
            return null;

        var pointBases = new HashSet<uint>();

        if (gatheringItem.IsHidden)
        {
            var gatheringItemPointSheet = ExcelService.GetSheet<GatheringItemPoint>();

            foreach (var row in gatheringItemPointSheet)
            {
                if (row.RowId < gatheringItem.RowId) continue;
                if (row.RowId > gatheringItem.RowId) break;
                if (row.GatheringPoint.Row == 0) continue;

                pointBases.Add(row.GatheringPoint.Value!.GatheringPointBase.Row);
            }
        }

        var gatheringPointSheet = ExcelService.GetSheet<GatheringPoint>();

        foreach (var point in gatheringPointSheet)
        {
            if (point.TerritoryType.Row <= 1)
                continue;

            if (point.GatheringPointBase.Value == null)
                continue;

            var gatheringPointBase = point.GatheringPointBase.Value;

            // only accept Mining, Quarrying, Logging and Harvesting
            if (gatheringPointBase.GatheringType.Row >= 5)
                continue;

            foreach (var id in gatheringPointBase.Item)
            {
                if (id == gatheringItem.RowId)
                    pointBases.Add(gatheringPointBase.RowId);
            }
        }

        return pointBases
            .Select((baseId) => gatheringPointSheet.Where((row) => row.TerritoryType.Row > 1 && row.GatheringPointBase.Row == baseId))
            .SelectMany(e => e)
            .OfType<GatheringPoint>()
            .ToArray();
    }
}
