using System.Collections.Generic;
using System.Reflection;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;

namespace HaselCommon.Services;

internal unsafe class StringManager
{
    internal static readonly Dictionary<(NameFormatterPlaceholder placeholder, NameFormatterIdConverter idConverter, uint id), string> NameCache = [];
    internal static readonly Dictionary<(string sheetName, uint rowId, string columnName), string> TextCache = [];

    internal string? FormatName(NameFormatterPlaceholder placeholder, NameFormatterIdConverter idConverter, uint id)
    {
        var key = (placeholder, idConverter, id);

        if (!NameCache.TryGetValue(key, out var value))
        {
            var ptr = (nint)RaptureTextModule.FormatName(placeholder, (int)id, idConverter, 1);
            if (ptr != nint.Zero)
            {
                value = MemoryHelper.ReadSeStringNullTerminated(ptr).ToString();

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                NameCache.Add(key, value);
            }
        }

        return value;
    }

    internal string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
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
