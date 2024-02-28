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

            var GatheringPointSheet = GetSheet<ExtendedGatheringPoint>();

            //! https://github.com/Ottermandias/GatherBuddy/blob/56da5c9/GatherBuddy.GameData/Data/HiddenItems.cs
            var pointBases = Item switch
            {
                7758 => [203],  // Grade 1 La Noscean Topsoil
                7761 => [200],  // Grade 1 Shroud Topsoil
                7764 => [201],  // Grade 1 Thanalan Topsoil
                7759 => [150],  // Grade 2 La Noscean Topsoil
                7762 => [209],  // Grade 2 Shroud Topsoil
                7765 => [151],  // Grade 2 Thanalan Topsoil
                10092 => [210], // Black Limestone
                10094 => [177], // Little Worm
                10097 => [133], // Yafaemi Wildgrass
                12893 => [295], // Dark Chestnut
                15865 => [30],  // Firelight Seeds
                15866 => [39],  // Icelight Seeds
                15867 => [21],  // Windlight Seeds
                15868 => [31],  // Earthlight Seeds
                15869 => [25],  // Levinlight Seeds
                15870 => [14],  // Waterlight Seeds
                12534 => [285], // Mythrite Ore
                12535 => [353], // Hardsilver Ore
                12537 => [286], // Titanium Ore
                12579 => [356], // Birch Log
                12878 => [297], // Cyclops Onion
                12879 => [298], // Emerald Beans
                _ => new HashSet<uint>()
            };

            foreach (var point in GatheringPointSheet)
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
                .Select((baseId) => GatheringPointSheet.Where((row) => row.TerritoryType.Row > 1 && row.GatheringPointBase.Row == baseId))
                .SelectMany(e => e)
                .OfType<ExtendedGatheringPoint>().ToArray();
        }
    }
}
