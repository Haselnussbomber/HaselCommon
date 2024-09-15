using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel;

namespace HaselCommon.Services;

public class ExcelService(IDataManager DataManager, TextService TextService)
{
    public ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : ExcelRow
        => DataManager.GetExcelSheet<T>(language ?? TextService.ClientLanguage.Value)!;

    public uint GetRowCount<T>() where T : ExcelRow
        => GetSheet<T>().RowCount;

    public T? GetRow<T>(uint rowId, ClientLanguage? language = null) where T : ExcelRow
        => GetSheet<T>(language).GetRow(rowId);

    public T? GetRow<T>(uint rowId, uint subRowId, ClientLanguage? language = null) where T : ExcelRow
        => GetSheet<T>(language).GetRow(rowId, subRowId);

    public T? FindRow<T>(Func<T?, bool> predicate) where T : ExcelRow
        => GetSheet<T>().FirstOrDefault(predicate, null);

    public T[] FindRows<T>(Func<T?, bool> predicate) where T : ExcelRow
        => GetSheet<T>().Where(row => row != null && predicate(row)).ToArray();
}
