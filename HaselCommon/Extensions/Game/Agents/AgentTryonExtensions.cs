using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Game.Enums;

namespace HaselCommon.Extensions;

public static unsafe class AgentTryonExtensions
{
    public static bool CanTryOn(ref this AgentTryon agent, ItemHandle item)
    {
        if (!item.TryGetItem(out var itemRow))
            return false;

        if (!(itemRow.EquipSlotCategory.RowId switch
        {
            0 => false, // not equippable
            2 when (ItemFilterGroup)itemRow.FilterGroup != ItemFilterGroup.Shield => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        }))
        {
            return false;
        }

        var playerState = PlayerState.Instance();
        if (!playerState->IsLoaded)
            return false;

        var race = playerState->Race;
        if (race == 0)
            return false;

        if (!ServiceLocator.TryGetService<ExcelService>(out var excelService))
            return false;

        if (!excelService.TryGetRawRow("EquipRaceCategory", itemRow.EquipRestriction, out var equipRaceCategoryRawRow))
            return false;

        if (!equipRaceCategoryRawRow.ReadBool(race - 1u))
            return false;

        if (!excelService.TryGetRow<EquipRaceCategory>(itemRow.EquipRestriction, out var equipRaceCategoryRow))
            return false;

        return playerState->Sex switch
        {
            1 => equipRaceCategoryRow.Female,
            _ => equipRaceCategoryRow.Male,
        };
    }

    public static bool TryOn(ref this AgentTryon agent, ItemHandle item, uint openerAddonId = 0, byte stain0Id = 0, byte stain1Id = 0, uint glamourItemId = 0, bool applyCompanyCrest = false)
    {
        return agent.CanTryOn(item) && AgentTryon.TryOn(openerAddonId, item.BaseItemId, stain0Id, stain1Id, glamourItemId, applyCompanyCrest);
    }
}
