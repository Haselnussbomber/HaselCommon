using System.Collections.Generic;
using System.Reflection;
using Dalamud.Utility;
using Lumina.Excel;

namespace HaselCommon.Services.Internal;

public class SheetTextCache
{
    public readonly Dictionary<(string sheetName, uint rowId, string columnName), string> TextCache = [];

    public string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
    {
        var sheetType = typeof(T);
        var sheetName = sheetType.Name;
        var key = (sheetName, rowId, columnName);

        if (!TextCache.TryGetValue(key, out var value))
        {
            var prop = sheetType.GetProperty(columnName, BindingFlags.Instance | BindingFlags.Public);
            if (prop == null || prop.PropertyType != typeof(Lumina.Text.SeString))
                return string.Empty;

            var sheetRow = GetRow<T>(rowId);
            if (sheetRow == null)
                return string.Empty;

            var seStr = (Lumina.Text.SeString?)prop.GetValue(sheetRow);
            if (seStr == null)
                return string.Empty;

            value = seStr.ToDalamudString().TextValue;

            TextCache.Add(key, value);
        }

        return value;
    }
}
