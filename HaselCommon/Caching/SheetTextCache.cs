using System.Reflection;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HaselCommon.Services;
using Lumina.Excel;

namespace HaselCommon.Caching;

using SheetTextCacheKey = (uint RowId, string ColumnName);

// TODO: remove again (lol). add sheets to the service collection and make an extension for SeString to extract text via ReadOnlySeStringSpan
public class SheetTextCache<T> : MemoryCache<SheetTextCacheKey, string> where T : ExcelRow
{
    private readonly IDataManager DataManager;
    private readonly TranslationManager TranslationManager;

    public SheetTextCache(IDataManager dataManager, TranslationManager translationManager)
    {
        DataManager = dataManager;
        TranslationManager = translationManager;
        TranslationManager.LanguageChanged += OnLanguageChanged;
    }

    public override void Dispose()
    {
        TranslationManager.LanguageChanged -= OnLanguageChanged;
        Clear();
        GC.SuppressFinalize(this);
    }

    private void OnLanguageChanged(string langCode)
    {
        Clear();
    }

    public override string CreateEntry(SheetTextCacheKey key)
    {
        var prop = typeof(T).GetProperty(key.ColumnName, BindingFlags.Instance | BindingFlags.Public);
        if (prop == null || prop.PropertyType != typeof(Lumina.Text.SeString))
            return string.Empty;

        var sheetRow = DataManager.GetExcelSheet<T>(TranslationManager.ClientLanguage)?.GetRow(key.RowId);
        if (sheetRow == null)
            return string.Empty;

        var seStr = (Lumina.Text.SeString?)prop.GetValue(sheetRow);
        if (seStr == null)
            return string.Empty;

        return seStr.ToDalamudString().TextValue;
    }
}
