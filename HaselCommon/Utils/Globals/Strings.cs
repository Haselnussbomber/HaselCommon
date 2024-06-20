using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using HaselCommon.Caching;
using HaselCommon.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using ActionSheet = Lumina.Excel.GeneratedSheets.Action;
using AddonSheet = Lumina.Excel.GeneratedSheets.Addon;

namespace HaselCommon.Utils.Globals;

public static unsafe class Strings
{
    public static string t(string key)
        => Service.Get<TranslationManager>().Translate(key);

    public static string t(string key, params object?[] args)
        => Service.Get<TranslationManager>().Translate(key, args);

    public static SeString tSe(string key, params SeString[] args)
        => Service.Get<TranslationManager>().TranslateSeString(key, args.Select(s => s.Payloads).ToArray());

    public static string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
        => Service.Get<SheetTextCache<T>>().TryGetValue((rowId, columnName), out var value) ? value! : string.Empty;

    private static string TitleCasedSingularNoun(string sheetName, uint id)
    {
        var tm = Service.Get<TranslationManager>();
        return tm.CultureInfo.TextInfo.ToTitleCase(TextDecoder.ProcessNoun(tm.ClientLanguage, sheetName, 5, (int)id));
    }

    public static string GetAddonText(uint id)
        => GetSheetText<AddonSheet>(id, "Text");

    public static string GetItemName(uint id)
        => GetSheetText<Item>(id, "Name");

    public static string GetBNpcName(uint id)
        => TitleCasedSingularNoun("BNpcName", id);

    public static string GetENpcResidentName(uint id)
        => TitleCasedSingularNoun("ENpcResident", id);

    public static string GetTreasureName(uint id)
        => TitleCasedSingularNoun("Treasure", id);

    public static string GetGatheringPointName(uint id)
        => TitleCasedSingularNoun("GatheringPointName", id);

    public static string GetEObjName(uint id)
        => TitleCasedSingularNoun("EObjName", id);

    public static string GetCompanionName(uint id)
        => TitleCasedSingularNoun("Companion", id);

    public static string GetTraitName(uint id)
        => GetSheetText<Trait>(id, "Name");

    public static string GetActionName(uint id)
        => GetSheetText<ActionSheet>(id, "Name");

    public static string GetEventActionName(uint id)
        => GetSheetText<EventAction>(id, "Name");

    public static string GetGeneralActionName(uint id)
        => GetSheetText<GeneralAction>(id, "Name");

    public static string GetBuddyActionName(uint id)
        => GetSheetText<BuddyAction>(id, "Name");

    public static string GetMainCommandName(uint id)
        => GetSheetText<MainCommand>(id, "Name");

    public static string GetCraftActionName(uint id)
        => GetSheetText<CraftAction>(id, "Name");

    public static string GetPetActionName(uint id)
        => GetSheetText<PetAction>(id, "Name");

    public static string GetCompanyActionName(uint id)
        => GetSheetText<CompanyAction>(id, "Name");

    public static string GetMountName(uint id)
        => TitleCasedSingularNoun("Mount", id);

    public static string GetOrnamentName(uint id)
        => TitleCasedSingularNoun("Ornament", id);
}
