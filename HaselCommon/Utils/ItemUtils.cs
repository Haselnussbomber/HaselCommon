using System.Collections.Frozen;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Utils;

public static class ItemUtils
{
    private static FrozenDictionary<short, (uint Min, uint Max)>? maxLevelRanges = null;

    public static FrozenDictionary<short, (uint Min, uint Max)> MaxLevelRanges
    {
        get
        {
            if (maxLevelRanges != null)
                return maxLevelRanges;

            var dict = new Dictionary<short, (uint Min, uint Max)>();

            short level = 50;
            foreach (var exVersion in GetSheet<ExVersion>())
            {
                var entry = (Min: uint.MaxValue, Max: 0u);

                foreach (var item in GetSheet<Item>())
                {
                    if (item.LevelEquip != level || item.LevelItem.Row <= 1)
                        continue;

                    if (entry.Min > item.LevelItem.Row)
                        entry.Min = item.LevelItem.Row;

                    if (entry.Max < item.LevelItem.Row)
                        entry.Max = item.LevelItem.Row;
                }

                dict.Add(level, entry);
                level += 10;
            }

            return maxLevelRanges = dict.ToFrozenDictionary();
        }
    }
}
