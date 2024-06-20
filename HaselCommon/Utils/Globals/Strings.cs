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
    [Obsolete]
    public static string t(string key)
        => Service.Get<TranslationManager>().Translate(key);

    [Obsolete]
    public static string t(string key, params object?[] args)
        => Service.Get<TranslationManager>().Translate(key, args);

    [Obsolete]
    public static SeString tSe(string key, params SeString[] args)
        => Service.Get<TranslationManager>().TranslateSeString(key, args.Select(s => s.Payloads).ToArray());

    [Obsolete]
    public static string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
        => Service.Get<SheetTextCache<T>>().TryGetValue((rowId, columnName), out var value) ? value! : string.Empty;

    [Obsolete]
    private static string TitleCasedSingularNoun(string sheetName, uint id)
        => string.Empty;

    [Obsolete]
    public static string GetAddonText(uint id)
        => GetSheetText<AddonSheet>(id, "Text");

    [Obsolete]
    public static string GetItemName(uint id)
        => GetSheetText<Item>(id, "Name");

    [Obsolete]
    public static string GetBNpcName(uint id)
        => TitleCasedSingularNoun("BNpcName", id);

    [Obsolete]
    public static string GetENpcResidentName(uint id)
        => TitleCasedSingularNoun("ENpcResident", id);

    [Obsolete]
    public static string GetTreasureName(uint id)
        => TitleCasedSingularNoun("Treasure", id);

    [Obsolete]
    public static string GetGatheringPointName(uint id)
        => TitleCasedSingularNoun("GatheringPointName", id);

    [Obsolete]
    public static string GetEObjName(uint id)
        => TitleCasedSingularNoun("EObjName", id);

    [Obsolete]
    public static string GetCompanionName(uint id)
        => TitleCasedSingularNoun("Companion", id);

    [Obsolete]
    public static string GetTraitName(uint id)
        => GetSheetText<Trait>(id, "Name");

    [Obsolete]
    public static string GetActionName(uint id)
        => GetSheetText<ActionSheet>(id, "Name");

    [Obsolete]
    public static string GetEventActionName(uint id)
        => GetSheetText<EventAction>(id, "Name");

    [Obsolete]
    public static string GetGeneralActionName(uint id)
        => GetSheetText<GeneralAction>(id, "Name");

    [Obsolete]
    public static string GetBuddyActionName(uint id)
        => GetSheetText<BuddyAction>(id, "Name");

    [Obsolete]
    public static string GetMainCommandName(uint id)
        => GetSheetText<MainCommand>(id, "Name");

    [Obsolete]
    public static string GetCraftActionName(uint id)
        => GetSheetText<CraftAction>(id, "Name");

    [Obsolete]
    public static string GetPetActionName(uint id)
        => GetSheetText<PetAction>(id, "Name");

    [Obsolete]
    public static string GetCompanyActionName(uint id)
        => GetSheetText<CompanyAction>(id, "Name");

    [Obsolete]
    public static string GetMountName(uint id)
        => TitleCasedSingularNoun("Mount", id);

    [Obsolete]
    public static string GetOrnamentName(uint id)
        => TitleCasedSingularNoun("Ornament", id);
}
