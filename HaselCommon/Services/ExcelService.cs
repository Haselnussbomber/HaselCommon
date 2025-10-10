using Dalamud.Utility;
using Lumina.Excel;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ExcelService
{
    private readonly IDataManager _dataManager;
    private readonly LanguageProvider _languageProvider;

    public static ExcelService? Instance => ServiceLocator.GetService<ExcelService>();

    public bool HasSheet(string name)
        => _dataManager.Excel.SheetNames.Contains(name);

    public int GetRowCount<T>() where T : struct, IExcelRow<T>
        => GetSheet<T>().Count;

    // Normal Sheets

    public RowRef<T> CreateRowRef<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => new(_dataManager.Excel, rowId, (language ?? _languageProvider.ClientLanguage).ToLumina());

    public ExcelSheet<T> GetSheet<T>(ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => _dataManager.GetExcelSheet<T>(language ?? _languageProvider.ClientLanguage)!;

    public ExcelSheet<T> GetSheet<T>(string sheetName, ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => _dataManager.GetExcelSheet<T>(language ?? _languageProvider.ClientLanguage, sheetName)!;

    public bool TryGetRow<T>(uint rowId, out T row) where T : struct, IExcelRow<T>
        => TryGetRow(rowId, null, out row);

    public bool TryGetRow<T>(uint rowId, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(language ?? _languageProvider.ClientLanguage).TryGetRow(rowId, out row);

    public bool TryGetRow<T>(string sheetName, uint rowId, out T row) where T : struct, IExcelRow<T>
        => TryGetRow(sheetName, rowId, null, out row);

    public bool TryGetRow<T>(string sheetName, uint rowId, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(sheetName, language ?? _languageProvider.ClientLanguage).TryGetRow(rowId, out row);

    public bool TryFindRow<T>(string sheetName, Predicate<T> predicate, out T row) where T : struct, IExcelRow<T>
        => TryFindRow(sheetName, predicate, null, out row);

    public bool TryFindRow<T>(string sheetName, Predicate<T> predicate, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(sheetName, language ?? _languageProvider.ClientLanguage).TryGetFirst(predicate, out row);

    public bool TryFindRow<T>(Predicate<T> predicate, out T row) where T : struct, IExcelRow<T>
        => TryFindRow(predicate, null, out row);

    public bool TryFindRow<T>(Predicate<T> predicate, ClientLanguage? language, out T row) where T : struct, IExcelRow<T>
        => GetSheet<T>(language ?? _languageProvider.ClientLanguage).TryGetFirst(predicate, out row);

    public IReadOnlyList<T> FindRows<T>(Predicate<T> predicate, ClientLanguage? language = null) where T : struct, IExcelRow<T>
        => [.. GetSheet<T>(language ?? _languageProvider.ClientLanguage).Where(row => predicate(row))];

    public bool TryFindRows<T>(Predicate<T> predicate, out IReadOnlyList<T> rows) where T : struct, IExcelRow<T>
        => TryFindRows(predicate, null, out rows);

    public bool TryFindRows<T>(Predicate<T> predicate, ClientLanguage? language, out IReadOnlyList<T> rows) where T : struct, IExcelRow<T>
    {
        rows = [.. GetSheet<T>(language).Where(row => predicate(row))];
        return rows.Count != 0;
    }

    // Subrow Sheets

    public SubrowRef<T> CreateSubrowRef<T>(uint rowId, ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
        => new(_dataManager.Excel, rowId, (language ?? _languageProvider.ClientLanguage).ToLumina());

    public SubrowExcelSheet<T> GetSubrowSheet<T>(ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
        => _dataManager.GetSubrowExcelSheet<T>(language ?? _languageProvider.ClientLanguage)!;

    public SubrowExcelSheet<T> GetSubrowSheet<T>(string sheetName, ClientLanguage? language = null) where T : struct, IExcelSubrow<T>
        => _dataManager.GetSubrowExcelSheet<T>(language ?? _languageProvider.ClientLanguage, sheetName)!;

    public bool TryGetSubrows<T>(uint rowId, out SubrowCollection<T> rows) where T : struct, IExcelSubrow<T>
        => TryGetSubrows(rowId, null, out rows);

    public bool TryGetSubrows<T>(uint rowId, ClientLanguage? language, out SubrowCollection<T> rows) where T : struct, IExcelSubrow<T>
        => GetSubrowSheet<T>(language ?? _languageProvider.ClientLanguage).TryGetRow(rowId, out rows);

    public bool TryGetSubrow<T>(uint rowId, int subRowIndex, out T row) where T : struct, IExcelSubrow<T>
        => TryGetSubrow(rowId, subRowIndex, null, out row);

    public bool TryGetSubrow<T>(uint rowId, int subRowIndex, ClientLanguage? language, out T row) where T : struct, IExcelSubrow<T>
    {
        if (!GetSubrowSheet<T>(language ?? _languageProvider.ClientLanguage).TryGetRow(rowId, out var rows) || subRowIndex < rows.Count)
        {
            row = default;
            return false;
        }

        row = rows[subRowIndex];
        return true;
    }

    public bool TryFindSubrow<T>(Predicate<T> predicate, out T subrow) where T : struct, IExcelSubrow<T>
        => TryFindSubrow(predicate, null, out subrow);

    public bool TryFindSubrow<T>(Predicate<T> predicate, ClientLanguage? language, out T subrow) where T : struct, IExcelSubrow<T>
    {
        foreach (var irow in GetSubrowSheet<T>(language ?? _languageProvider.ClientLanguage))
        {
            foreach (var isubrow in irow)
            {
                if (predicate(isubrow))
                {
                    subrow = isubrow;
                    return true;
                }
            }
        }

        subrow = default;
        return false;
    }

    // RawRow

    public bool TryGetRawRow(string sheetName, uint rowId, out RawRow rawRow)
        => TryGetRow(sheetName, rowId, out rawRow);

    public bool TryGetRawRow(string sheetName, uint rowId, ClientLanguage? language, out RawRow rawRow)
        => TryGetRow(sheetName, rowId, language, out rawRow);
}
