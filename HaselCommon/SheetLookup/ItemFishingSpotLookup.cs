using System.Collections.Generic;
using System.Linq;
using HaselCommon.Sheets;

namespace HaselCommon.SheetLookup;

public static class ItemFishingSpotLookup
{
    private static readonly Dictionary<uint, HashSet<ExtendedFishingSpot>> Cache = [];

    private static void Load()
    {
        foreach (var fishingSpot in GetSheet<ExtendedFishingSpot>())
        {
            if (fishingSpot.TerritoryType.Row == 0)
                continue;

            foreach (var item in fishingSpot.Item)
            {
                if (!Cache.TryGetValue(item.Row, out var fishingSpots))
                    Cache.Add(item.Row, fishingSpots = []);

                fishingSpots.Add(fishingSpot);
            }
        }
    }

    public static bool Any(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return Cache.ContainsKey(itemId);
    }

    public static ExtendedFishingSpot? First(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var fishingSpots)
                ? null
                : fishingSpots.First();
    }

    public static ExtendedFishingSpot[] All(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var fishingSpots)
                ? []
                : fishingSpots.ToArray();
    }
}
