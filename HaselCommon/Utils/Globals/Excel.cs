using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud;
using Lumina.Excel;

namespace HaselCommon.Utils.Globals;

public static class Excel
{
    private static readonly Dictionary<Type, Dictionary<(ClientLanguage, uint, uint), ExcelRow?>> ExcelCache = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : ExcelRow
        => Service.DataManager.GetExcelSheet<T>(language ?? Service.TranslationManager.ClientLanguage)!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetRowCount<T>() where T : ExcelRow
        => GetSheet<T>().RowCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetRow<T>(uint rowId, uint subRowId = uint.MaxValue, ClientLanguage? language = null) where T : ExcelRow
    {
        if (!ExcelCache.TryGetValue(typeof(T), out var dict))
            ExcelCache.Add(typeof(T), dict = []);

        var lang = language ?? Service.TranslationManager.ClientLanguage;
        if (!dict.TryGetValue((lang, rowId, subRowId), out var row))
            dict.Add((lang, rowId, subRowId), row = GetSheet<T>(language).GetRow(rowId, subRowId));

        return (T?)row;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FindRow<T>(Func<T?, bool> predicate) where T : ExcelRow
        => GetSheet<T>().FirstOrDefault(predicate, null);
}
