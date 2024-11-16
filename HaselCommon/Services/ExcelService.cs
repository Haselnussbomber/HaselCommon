using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HaselCommon.Extensions;
using Lumina.Excel;

namespace HaselCommon.Services;

public class ExcelService(IDataManager dataManager, TextService textService)
{
    public bool HasSheet(string name)
        => dataManager.Excel.SheetNames.Contains(name);

    public ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => dataManager.GetExcelSheet<T>(language ?? textService.ClientLanguage)!;

    public ExcelSheet<T> GetSheet<T>(ClientLanguage? language, string name) where T : struct, IExcelRow<T>
        => dataManager.GetExcelSheet<T>(language ?? textService.ClientLanguage, name)!;

    public SubrowExcelSheet<T> GetSubrowSheet<T>(ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
        => dataManager.GetSubrowExcelSheet<T>(language ?? textService.ClientLanguage)!;

    public int GetRowCount<T>() where T : struct, IExcelRow<T>
        => GetSheet<T>().Count;

    public RowRef<T> CreateRef<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => new(dataManager.Excel, rowId, (language ?? textService.ClientLanguage).ToLumina());

    public bool TryGetRow<T>(uint rowId, [NotNullWhen(returnValue: true)] out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(textService.ClientLanguage).TryGetRow(rowId, out row);

    public bool TryGetRow<T>(uint rowId, ClientLanguage? language, [NotNullWhen(returnValue: true)] out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(language).TryGetRow(rowId, out row);

    public bool TryFindRow<T>(Predicate<T> predicate, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(textService.ClientLanguage).TryGetFirst(predicate, out row);

    public bool TryFindRow<T>(Predicate<T> predicate, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(language).TryGetFirst(predicate, out row);

    public T[] FindRows<T>(Predicate<T> predicate) where T : struct, IExcelRow<T>
        => GetSheet<T>().Where(row => predicate(row)).ToArray();

    public T[] FindRows<T>(Predicate<T> predicate, ClientLanguage? language) where T : struct, IExcelRow<T>
        => GetSheet<T>(language).Where(row => predicate(row)).ToArray();
}
