using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text.SeStringHandling;
using HaselCommon.Structs.Internal;
using Lumina.Excel;
using AddonSheet = Lumina.Excel.GeneratedSheets.Addon;

namespace HaselCommon.Utils.Globals;

public static unsafe class Strings
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string t(string key)
        => HaselCommonBase.TranslationManager.Translate(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string t(string key, params object?[] args)
        => HaselCommonBase.TranslationManager.Translate(key, args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SeString tSe(string key, params SeString[] args)
        => HaselCommonBase.TranslationManager.TranslateSeString(key, args.Select(s => s.Payloads).ToArray());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetAddonText(uint id)
        => HaselCommonBase.StringManager.GetSheetText<AddonSheet>(id, "Text");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetSheetText<T>(uint rowId, string columnName) where T : ExcelRow
        => HaselCommonBase.StringManager.GetSheetText<T>(rowId, columnName);

    public static string GetItemName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.Item, NameFormatter.IdConverter.Item, id) ?? $"[Item#{id}]";

    public static string GetBNpcName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_BNpcName, id) ?? $"[BNpcName#{id}]";

    public static string GetENpcResidentName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_ENpcResident, id) ?? $"[ENpcResident#{id}]";

    public static string GetTreasureName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_Treasure, id) ?? $"[Treasure#{id}]";

    public static string GetAetheryteName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_Aetheryte, id) ?? $"[Aetheryte#{id}]";

    public static string GetGatheringPointName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_GatheringPointName, id) ?? $"[GatheringPointName#{id}]";

    public static string GetEObjName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_EObjName, id) ?? $"[EObjName#{id}]";

    public static string GetCompanionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ObjStr, NameFormatter.IdConverter.ObjStr_Companion, id) ?? $"[Companion#{id}]";

    public static string GetTraitName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_Trait, id) ?? $"[Trait#{id}]";

    public static string GetActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_Action, id) ?? $"[Action#{id}]";

    public static string GetEventActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_EventAction, id) ?? $"[EventAction#{id}]";

    public static string GetGeneralActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_GeneralAction, id) ?? $"[GeneralAction#{id}]";

    public static string GetBuddyActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_BuddyAction, id) ?? $"[BuddyAction#{id}]";

    public static string GetMainCommandName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_MainCommand, id) ?? $"[MainCommand#{id}]";

    public static string GetCraftActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_CraftAction, id) ?? $"[CraftAction#{id}]";

    public static string GetPetActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_PetAction, id) ?? $"[PetAction#{id}]";

    public static string GetCompanyActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_CompanyAction, id) ?? $"[CompanyAction#{id}]";

    public static string GetMountName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_Mount, id) ?? $"[Mount#{id}]";

    public static string GetBgcArmyActionName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_BgcArmyAction, id) ?? $"[BgcArmyAction#{id}]";

    public static string GetOrnamentName(uint id)
        => HaselCommonBase.StringManager.FormatName(NameFormatter.Placeholder.ActStr, NameFormatter.IdConverter.ActStr_Ornament, id) ?? $"[Ornament#{id}]";
}
