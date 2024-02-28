using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedEObj : EObj
{
    private static readonly Dictionary<uint, ExtendedEObj?> DataIdCache = [];

    public static ExtendedEObj? GetByDataId(uint dataId)
    {
        if (!DataIdCache.TryGetValue(dataId, out var value))
        {
            value = FindRow<ExtendedEObj>(row => row?.Data == dataId);
            DataIdCache.Add(dataId, value);
        }

        return value;
    }
}
