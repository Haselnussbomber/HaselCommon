using System.Collections.Generic;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Caching.Internal;

internal class ItemRecipesCache(ExcelService ExcelService) : MemoryCache<uint, Recipe[]>
{
    public override Recipe[]? CreateEntry(uint itemId)
    {
        var lookup = ExcelService.GetRow<RecipeLookup>(itemId);
        if (lookup == null)
            return [];

        var _recipes = new List<Recipe>();
        if (lookup.CRP.Row != 0 && lookup.CRP.Value != null) _recipes.Add(lookup.CRP.Value);
        if (lookup.BSM.Row != 0 && lookup.BSM.Value != null) _recipes.Add(lookup.BSM.Value);
        if (lookup.ARM.Row != 0 && lookup.ARM.Value != null) _recipes.Add(lookup.ARM.Value);
        if (lookup.GSM.Row != 0 && lookup.GSM.Value != null) _recipes.Add(lookup.GSM.Value);
        if (lookup.LTW.Row != 0 && lookup.LTW.Value != null) _recipes.Add(lookup.LTW.Value);
        if (lookup.WVR.Row != 0 && lookup.WVR.Value != null) _recipes.Add(lookup.WVR.Value);
        if (lookup.ALC.Row != 0 && lookup.ALC.Value != null) _recipes.Add(lookup.ALC.Value);
        if (lookup.CUL.Row != 0 && lookup.CUL.Value != null) _recipes.Add(lookup.CUL.Value);
        return [.. _recipes];
    }
}
