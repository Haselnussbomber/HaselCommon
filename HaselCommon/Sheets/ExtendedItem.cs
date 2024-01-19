using System.Collections.Generic;
using System.Linq;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using HaselCommon.Enums;
using HaselCommon.SheetLookup;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedItem : Item
{
    private string? _name { get; set; } = null;
    private Recipe[]? recipes { get; set; } = null;
    private bool? _isCraftable { get; set; } = null;
    private bool? _isFish { get; set; } = null;
    private bool? _isSpearfishing { get; set; } = null;
    private bool? _isUnlockable { get; set; } = null;
    private ExtendedGatheringItem[]? _gatheringItems { get; set; } = null;
    private ExtendedGatheringPoint[]? _gatheringPoints { get; set; } = null;
    private ExtendedFishingSpot[]? _fishingSpots { get; set; } = null;

    public new string Name
        => _name ??= base.Name.ToDalamudString().ToString();

    public Recipe[] Recipes
    {
        get
        {
            if (recipes != null)
                return recipes;

            var lookup = GetRow<RecipeLookup>(RowId);
            if (lookup == null)
                return recipes ??= Array.Empty<Recipe>();

            var _recipes = new List<Recipe>();
            if (lookup.CRP.Row != 0 && lookup.CRP.Value != null) _recipes.Add(lookup.CRP.Value);
            if (lookup.BSM.Row != 0 && lookup.BSM.Value != null) _recipes.Add(lookup.BSM.Value);
            if (lookup.ARM.Row != 0 && lookup.ARM.Value != null) _recipes.Add(lookup.ARM.Value);
            if (lookup.GSM.Row != 0 && lookup.GSM.Value != null) _recipes.Add(lookup.GSM.Value);
            if (lookup.LTW.Row != 0 && lookup.LTW.Value != null) _recipes.Add(lookup.LTW.Value);
            if (lookup.WVR.Row != 0 && lookup.WVR.Value != null) _recipes.Add(lookup.WVR.Value);
            if (lookup.ALC.Row != 0 && lookup.ALC.Value != null) _recipes.Add(lookup.ALC.Value);
            if (lookup.CUL.Row != 0 && lookup.CUL.Value != null) _recipes.Add(lookup.CUL.Value);
            return recipes ??= _recipes.ToArray();
        }
    }

    public ExtendedGatheringItem[] GatheringItems
        => _gatheringItems ??= ItemGatheringItemLookup.All(RowId);

    public ExtendedGatheringPoint[] GatheringPoints
        => _gatheringPoints ??= GatheringItems
            .SelectMany(row => row.GatheringPoints)
            .ToArray();

    public ExtendedFishingSpot[] FishingSpots
        => _fishingSpots ??= ItemFishingSpotLookup.All(RowId);

    public bool IsCraftable
        => _isCraftable ??= Recipes.Any();

    public bool IsCrystal
        => ItemUICategory.Row == 59;

    public bool IsGatherable
        => GatheringPoints.Any();

    public bool IsFish
        => _isFish ??= FishingSpots.Any();

    public bool IsSpearfishing
        => _isSpearfishing ??= GetSheet<SpearfishingItem>().Any(row => row.Item.Row == RowId);

    public bool IsUnlockable
        => _isUnlockable ??= ItemAction.Row != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == ItemAction.Value!.Type);

    public bool CanTryOn
        => EquipSlotCategory.Row switch
        {
            0 => false,
            2 when FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        };

    public bool CanSearchForItem
        => !IsUntradable && !IsCollectable && IsAddonOpen(AgentId.ItemSearch);

    public unsafe bool HasAcquired
    {
        get
        {
            if (ItemAction.Row == 0)
                return false;

            var type = (ItemActionType)ItemAction.Value!.Type;
            if (type == ItemActionType.Cards)
                return UIState.Instance()->IsTripleTriadCardUnlocked((ushort)AdditionalData);

            var row = ExdModule.GetItemRowById(RowId);
            return row != null && UIState.Instance()->IsItemActionUnlocked(row) == 1;
        }
    }
}
