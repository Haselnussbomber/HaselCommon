using System.Collections.Generic;
using System.Linq;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedGatheringItem : GatheringItem
{
    private ExtendedGatheringPoint[]? _gatheringPoints = null;

    public LazyRow<ExtendedItem> ItemRow { get; set; } = null!;

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        ItemRow = new LazyRow<ExtendedItem>(gameData, Item, language);
    }

    public ExtendedGatheringPoint[] GatheringPoints
    {
        get
        {
            if (_gatheringPoints != null)
                return _gatheringPoints;

            var pointBases = new HashSet<uint>();

            if (IsHidden)
            {
                var gatheringItemPointSheet = GetSheet<GatheringItemPoint>();

                foreach (var row in gatheringItemPointSheet)
                {
                    if (row.RowId < RowId) continue;
                    if (row.RowId > RowId) break;
                    if (row.GatheringPoint.Row == 0) continue;

                    pointBases.Add(row.GatheringPoint.Value!.GatheringPointBase.Row);
                }
            }

            var gatheringPointSheet = GetSheet<ExtendedGatheringPoint>();

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

                foreach (var gatheringItemId in gatheringPointBase.Item)
                {
                    if (gatheringItemId == RowId)
                    {
                        pointBases.Add(gatheringPointBase.RowId);
                    }
                }
            }

            return _gatheringPoints ??= pointBases
                .Select((baseId) => gatheringPointSheet.Where((row) => row.TerritoryType.Row > 1 && row.GatheringPointBase.Row == baseId))
                .SelectMany(e => e)
                .OfType<ExtendedGatheringPoint>()
                .ToArray();
        }
    }
}
