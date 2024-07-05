using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Caches.Internal;
using HaselCommon.Enums;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Services;

public class ItemService(ExcelService ExcelService, TextService TextService)
{
    private readonly GatheringItemGatheringPointsCache GatheringItemGatheringPointsCache = new(ExcelService);
    private readonly ItemFishingSpotsCache ItemFishingSpotsCache = new(ExcelService);
    private readonly ItemGatheringItemsCache ItemGatheringItemsCache = new(ExcelService);
    private readonly ItemRecipesCache ItemRecipesCache = new(ExcelService);
    private readonly ItemSpearfishingItemCache ItemSpearfishingItemCache = new(ExcelService);

    public Recipe[] GetRecipes(Item item) => GetRecipes(item.RowId);
    public Recipe[] GetRecipes(uint itemId) => ItemRecipesCache.GetValue(itemId) ?? [];

    public GatheringItem[] GetGatheringItems(Item item) => GetGatheringItems(item.RowId);
    public GatheringItem[] GetGatheringItems(uint itemId) => ItemGatheringItemsCache.GetValue(itemId) ?? [];

    public GatheringPoint[] GetGatheringPoints(Item item) => GetGatheringPoints(item.RowId);
    public GatheringPoint[] GetGatheringPoints(uint itemId) => GetGatheringItems(itemId).SelectMany(GetGatheringPoints).ToArray();

    public FishingSpot[] GetFishingSpots(Item item) => GetFishingSpots(item.RowId);
    public FishingSpot[] GetFishingSpots(uint itemId) => ItemFishingSpotsCache.GetValue(itemId) ?? [];

    public bool IsCraftable(Item item) => IsCraftable(item.RowId);
    public bool IsCraftable(uint itemId) => GetRecipes(itemId).Length != 0;

    public bool IsCrystal(Item item) => item.ItemUICategory.Row == 59;
    public bool IsCrystal(uint itemId)
    {
        var item = ExcelService.GetRow<Item>(itemId);
        return item != null && IsCrystal(item);
    }

    public bool IsGatherable(Item item) => IsGatherable(item.RowId);
    public bool IsGatherable(uint itemId) => GetGatheringPoints(itemId).Length != 0;

    public bool IsFish(Item item) => IsFish(item.RowId);
    public bool IsFish(uint itemId) => GetFishingSpots(itemId).Length != 0;

    public bool IsSpearfish(Item item) => IsSpearfish(item.RowId);
    public bool IsSpearfish(uint itemId) => ItemSpearfishingItemCache.TryGetValue(itemId, out var _);

    public bool IsUnlockable(Item item) => item.ItemAction.Row != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == item.ItemAction.Value!.Type);
    public bool IsUnlockable(uint itemId)
    {
        var item = ExcelService.GetRow<Item>(itemId);
        return item != null && IsUnlockable(item);
    }

    // TODO: check race/sex restrictions
    public bool CanTryOn(Item item)
    {
        return item.EquipSlotCategory.Row switch
        {
            0 => false,
            2 when item.FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        };
    }

    public bool CanTryOn(uint itemId)
    {
        var item = ExcelService.GetRow<Item>(itemId);
        return item != null && CanTryOn(item);
    }

    public unsafe bool IsUnlocked(Item item)
    {
        if (item.ItemAction.Row == 0)
            return false;

        var type = (ItemActionType)item.ItemAction.Value!.Type;

        // most of what "E8 ?? ?? ?? ?? 84 C0 75 A6 32 C0" does
        // just to avoid the ExdModule.GetItemRowById call...
        switch (type)
        {
            case ItemActionType.Companion:
                return UIState.Instance()->IsCompanionUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.BuddyEquip:
                return UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Mount:
                return PlayerState.Instance()->IsMountUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.SecretRecipeBook:
                return PlayerState.Instance()->IsSecretRecipeBookUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.UnlockLink:
                return UIState.Instance()->IsUnlockLinkUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.TripleTriadCard:
                return UIState.Instance()->IsTripleTriadCardUnlocked((ushort)item.AdditionalData);

            case ItemActionType.FolkloreTome:
                return PlayerState.Instance()->IsFolkloreBookUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.OrchestrionRoll:
                return PlayerState.Instance()->IsOrchestrionRollUnlocked(item.AdditionalData);

            case ItemActionType.FramersKit:
                return PlayerState.Instance()->IsFramersKitUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Ornament:
                return PlayerState.Instance()->IsOrnamentUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Glasses:
                return PlayerState.Instance()->IsGlassesUnlocked((ushort)item.AdditionalData);
        }

        var row = ExdModule.GetItemRowById(item.RowId);
        return row != null && UIState.Instance()->IsItemActionUnlocked(row) == 1;
    }

    public bool IsUnlocked(uint itemId)
    {
        var item = ExcelService.GetRow<Item>(itemId);
        return item != null && IsUnlocked(item);
    }

    public bool CanSearchForItem(Item item) => !item.IsUntradable && !item.IsCollectable && IsAddonOpen(AgentId.ItemSearch);

    public bool CanSearchForItem(uint itemId)
    {
        var item = ExcelService.GetRow<Item>(itemId);
        return item != null && CanSearchForItem(item);
    }

    public unsafe void Search(Item item)
    {
        if (!CanSearchForItem(item))
            return;

        if (!TryGetAddon<AddonItemSearch>(AgentId.ItemSearch, out var addon))
            return;

        if (TryGetAddon<AtkUnitBase>("ItemSearchResult", out var itemSearchResult))
            itemSearchResult->Hide2();

        var itemName = TextService.GetItemName(item.RowId);
        if (itemName.Length > 40)
            itemName = itemName[..40];

        addon->SearchTextInput->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }

    public GatheringPoint[] GetGatheringPoints(GatheringItem gatheringItem)
        => GatheringItemGatheringPointsCache.GetValue(gatheringItem.RowId) ?? [];
}
