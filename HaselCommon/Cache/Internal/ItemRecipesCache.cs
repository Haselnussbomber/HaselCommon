using System.Collections.Generic;
using HaselCommon.Services;
using Lumina.Excel.Sheets;

namespace HaselCommon.Cache.Internal;

internal class ItemRecipesCache(ExcelService excelService) : MemoryCache<uint, Recipe[]>
{
    public override Recipe[]? CreateEntry(uint itemId)
    {
        if (!excelService.TryGetRow<RecipeLookup>(itemId, out var lookup))
            return [];

        var _recipes = new List<Recipe>();
        if (lookup.CRP.RowId != 0 && lookup.CRP.IsValid) _recipes.Add(lookup.CRP.Value);
        if (lookup.BSM.RowId != 0 && lookup.BSM.IsValid) _recipes.Add(lookup.BSM.Value);
        if (lookup.ARM.RowId != 0 && lookup.ARM.IsValid) _recipes.Add(lookup.ARM.Value);
        if (lookup.GSM.RowId != 0 && lookup.GSM.IsValid) _recipes.Add(lookup.GSM.Value);
        if (lookup.LTW.RowId != 0 && lookup.LTW.IsValid) _recipes.Add(lookup.LTW.Value);
        if (lookup.WVR.RowId != 0 && lookup.WVR.IsValid) _recipes.Add(lookup.WVR.Value);
        if (lookup.ALC.RowId != 0 && lookup.ALC.IsValid) _recipes.Add(lookup.ALC.Value);
        if (lookup.CUL.RowId != 0 && lookup.CUL.IsValid) _recipes.Add(lookup.CUL.Value);
        return [.. _recipes];
    }
}
