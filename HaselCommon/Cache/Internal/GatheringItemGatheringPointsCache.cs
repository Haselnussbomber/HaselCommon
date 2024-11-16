using System.Collections.Generic;
using System.Linq;
using HaselCommon.Services;
using Lumina.Excel.Sheets;

namespace HaselCommon.Cache.Internal;

internal class GatheringItemGatheringPointsCache(ExcelService excelService) : MemoryCache<uint, GatheringPoint[]>
{
    public override GatheringPoint[]? CreateEntry(uint gatheringItemId)
    {
        if (!excelService.TryGetRow<GatheringItem>(gatheringItemId, out var gatheringItem))
            return null;

        var pointBases = new HashSet<uint>();

        if (gatheringItem.IsHidden)
        {
            var gatheringItemPointSheet = excelService.GetSubrowSheet<GatheringItemPoint>();

            foreach (var row in gatheringItemPointSheet)
            {
                if (row.RowId < gatheringItem.RowId) continue;
                if (row.RowId > gatheringItem.RowId) break;

                foreach (var subrow in row)
                {
                    if (subrow.RowId != 0 && subrow.GatheringPoint.IsValid)
                        pointBases.Add(subrow.GatheringPoint.Value.GatheringPointBase.RowId);
                }
            }
        }

        var gatheringPointSheet = excelService.GetSheet<GatheringPoint>();

        foreach (var point in gatheringPointSheet)
        {
            if (point.TerritoryType.RowId <= 1)
                continue;

            if (!point.GatheringPointBase.IsValid)
                continue;

            var gatheringPointBase = point.GatheringPointBase.Value;

            // only accept Mining, Quarrying, Logging and Harvesting
            if (gatheringPointBase.GatheringType.RowId >= 5)
                continue;

            foreach (var id in gatheringPointBase.Item)
            {
                if (id.RowId == gatheringItem.RowId)
                    pointBases.Add(gatheringPointBase.RowId);
            }
        }

        return pointBases
            .Select((baseId) => gatheringPointSheet.Where((row) => row.TerritoryType.RowId > 1 && row.GatheringPointBase.RowId == baseId))
            .SelectMany(e => e)
            .OfType<GatheringPoint>()
            .ToArray();
    }
}
