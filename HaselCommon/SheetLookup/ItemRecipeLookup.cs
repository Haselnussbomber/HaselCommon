using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.SheetLookup;

public static class ItemRecipeLookup
{
    private static readonly Dictionary<uint, HashSet<Recipe>> Cache = new();

    private static void Load()
    {
        foreach (var recipe in GetSheet<Recipe>())
        {
            if (!Cache.TryGetValue(recipe.ItemResult.Row, out var recipeIds))
            {
                Cache.Add(recipe.ItemResult.Row, recipeIds = new());
            }

            recipeIds.Add(GetRow<Recipe>(recipe.RowId)!);
        }
    }

    public static bool Any(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return Cache.ContainsKey(itemId);
    }

    public static Recipe? First(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var recipes)
                ? null
                : recipes.First();
    }

    public static Recipe[] All(uint itemId)
    {
        if (!Cache.Any())
            Load();

        return !Cache.TryGetValue(itemId, out var recipes)
                ? Array.Empty<Recipe>()
                : recipes.ToArray();
    }
}
