using System.Collections.Generic;
using System.Linq;
using HaselCommon.Sheets;

namespace HaselCommon.SheetLookup;

public static class ItemGatheringItemLookup
{
    private static readonly Dictionary<uint, HashSet<ExtendedGatheringItem>> Cache = [];

    private static void Load()
    {
        foreach (var gatheringItem in GetSheet<ExtendedGatheringItem>())
        {
            if (!Cache.TryGetValue(gatheringItem.ItemRow.Row, out var gatheringItemIds))
            {
                Cache.Add(gatheringItem.ItemRow.Row, gatheringItemIds = []);
            }

            gatheringItemIds.Add(GetRow<ExtendedGatheringItem>(gatheringItem.RowId)!);
        }
    }

    public static bool Any(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return Cache.ContainsKey(itemId);
    }

    public static ExtendedGatheringItem? First(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var gatheringItems)
                ? null
                : gatheringItems.First();
    }

    public static ExtendedGatheringItem[] All(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var gatheringItems)
                ? []
                : gatheringItems.ToArray();
    }
}
