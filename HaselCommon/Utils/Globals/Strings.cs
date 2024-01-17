using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using AddonSheet = Lumina.Excel.GeneratedSheets.Addon;

namespace HaselCommon.Utils.Globals;

public static unsafe class Strings
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string t(string key)
        => Service.TranslationManager.Translate(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string t(string key, params object?[] args)
        => Service.TranslationManager.Translate(key, args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeString tSe(string key, params SeString[] args)
        => Service.TranslationManager.TranslateSeString(key, args.Select(s => s.Payloads).ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetAddonText(uint id)
        => Service.StringManager.GetSheetText<AddonSheet>(id, "Text");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
        => Service.StringManager.GetSheetText<T>(rowId, columnName);

    public static string GetItemName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.Item, NameFormatterIdConverter.None, id) ?? $"[Item#{id}]";

    public static string GetBNpcName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.BNpcName, id) ?? $"[BNpcName#{id}]";

    public static string GetENpcResidentName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.ENpcResident, id) ?? $"[ENpcResident#{id}]";

    public static string GetTreasureName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.Treasure, id) ?? $"[Treasure#{id}]";

    public static string GetAetheryteName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.Aetheryte, id) ?? $"[Aetheryte#{id}]";

    public static string GetGatheringPointName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.GatheringPointName, id) ?? $"[GatheringPointName#{id}]";

    public static string GetEObjName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.EObjName, id) ?? $"[EObjName#{id}]";

    public static string GetCompanionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ObjStr, NameFormatterIdConverter.Companion, id) ?? $"[Companion#{id}]";

    public static string GetTraitName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.Trait, id) ?? $"[Trait#{id}]";

    public static string GetActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.Action, id) ?? $"[Action#{id}]";

    public static string GetEventActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.EventAction, id) ?? $"[EventAction#{id}]";

    public static string GetGeneralActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.GeneralAction, id) ?? $"[GeneralAction#{id}]";

    public static string GetBuddyActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.BuddyAction, id) ?? $"[BuddyAction#{id}]";

    public static string GetMainCommandName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.MainCommand, id) ?? $"[MainCommand#{id}]";

    public static string GetCraftActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.CraftAction, id) ?? $"[CraftAction#{id}]";

    public static string GetPetActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.PetAction, id) ?? $"[PetAction#{id}]";

    public static string GetCompanyActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.CompanyAction, id) ?? $"[CompanyAction#{id}]";

    public static string GetMountName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.Mount, id) ?? $"[Mount#{id}]";

    public static string GetBgcArmyActionName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.BgcArmyAction, id) ?? $"[BgcArmyAction#{id}]";

    public static string GetOrnamentName(uint id)
        => Service.StringManager.FormatName(NameFormatterPlaceholder.ActStr, NameFormatterIdConverter.Ornament, id) ?? $"[Ornament#{id}]";
}
