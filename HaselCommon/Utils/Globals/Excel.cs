using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud;
using Lumina.Excel;

namespace HaselCommon.Utils.Globals;

public static class Excel
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : ExcelRow
        => Service.DataManager.GetExcelSheet<T>(language ?? HaselCommonBase.TranslationManager.ClientLanguage)!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetRowCount<T>() where T : ExcelRow
        => GetSheet<T>().RowCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetRow<T>(uint rowId, uint subRowId = uint.MaxValue) where T : ExcelRow
        => GetSheet<T>().GetRow(rowId, subRowId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FindRow<T>(Func<T, bool> predicate) where T : ExcelRow
        => GetSheet<T>().FirstOrDefault(predicate);
}
