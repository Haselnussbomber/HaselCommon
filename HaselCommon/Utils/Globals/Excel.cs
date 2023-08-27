using System.Linq;
using Dalamud;
using Lumina.Excel;

namespace HaselCommon.Utils.Globals;

public static class Excel
{
    public static ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : ExcelRow
        => Service.DataManager.GetExcelSheet<T>(language ?? HaselCommonBase.TranslationManager.ClientLanguage)!;

    public static uint GetRowCount<T>() where T : ExcelRow
        => GetSheet<T>().RowCount;

    public static T? GetRow<T>(uint rowId, uint subRowId = uint.MaxValue) where T : ExcelRow
        => GetSheet<T>().GetRow(rowId, subRowId);

    public static T? FindRow<T>(Func<T?, bool> predicate) where T : ExcelRow
        => GetSheet<T>().FirstOrDefault(predicate);
}
