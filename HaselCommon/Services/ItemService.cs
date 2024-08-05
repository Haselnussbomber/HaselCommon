using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Caches.Internal;
using HaselCommon.Enums;
using HaselCommon.Extensions;
using HaselCommon.Utils;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Services;

public class ItemService(IClientState ClientState, ExcelService ExcelService, TextService TextService)
{
    private readonly GatheringItemGatheringPointsCache GatheringItemGatheringPointsCache = new(ExcelService);
    private readonly ItemFishingSpotsCache ItemFishingSpotsCache = new(ExcelService);
    private readonly SpearfishingItemGatheringPointsCache SpearfishingItemGatheringPointsCache = new(ExcelService);
    private readonly ItemGatheringItemsCache ItemGatheringItemsCache = new(ExcelService);
    private readonly ItemRecipesCache ItemRecipesCache = new(ExcelService);
    private readonly ItemSpearfishingItemCache ItemSpearfishingItemCache = new(ExcelService);
    private readonly ItemIconIdCache ItemIconIdCache = new(ExcelService);

    private static FrozenDictionary<short, (uint Min, uint Max)>? MaxLevelRanges = null;

    public uint GetBaseItemId(uint itemId)
    {
        if (IsEventItem(itemId)) return itemId; // uses EventItem sheet
        if (IsHighQuality(itemId)) return itemId - 1_000_000;
        if (IsCollectible(itemId)) return itemId - 500_000;
        return itemId;
    }

    public bool IsNormalItem(Item item) => IsNormalItem(item.RowId);
    public bool IsNormalItem(uint itemId) => itemId is < 500_000;

    public bool IsCollectible(Item item) => IsCollectible(item.RowId);
    public bool IsCollectible(uint itemId) => itemId is > 500_000 and < 1_000_000;

    public bool IsHighQuality(Item item) => IsHighQuality(item.RowId);
    public bool IsHighQuality(uint itemId) => itemId is > 1_000_000 and < 2_000_000;

    public bool IsEventItem(Item item) => IsEventItem(item.RowId);
    public bool IsEventItem(uint itemId) => itemId is > 2_000_000;

    public uint GetIconId(uint itemId) => ItemIconIdCache.GetValue(itemId);

    public Recipe[] GetRecipes(Item item) => GetRecipes(item.RowId);
    public Recipe[] GetRecipes(uint itemId) => ItemRecipesCache.GetValue(itemId) ?? [];

    public GatheringItem[] GetGatheringItems(Item item) => GetGatheringItems(item.RowId);
    public GatheringItem[] GetGatheringItems(uint itemId) => ItemGatheringItemsCache.GetValue(itemId) ?? [];

    public GatheringPoint[] GetGatheringPoints(Item item) => GetGatheringPoints(item.RowId);
    public GatheringPoint[] GetGatheringPoints(uint itemId) => GetGatheringItems(itemId).SelectMany(GetGatheringPoints).ToArray();

    public FishingSpot[] GetFishingSpots(Item item) => GetFishingSpots(item.RowId);
    public FishingSpot[] GetFishingSpots(uint itemId) => ItemFishingSpotsCache.GetValue(itemId) ?? [];

    public GatheringPoint[] GetSpearfishingGatheringPoints(Item item) => GetSpearfishingGatheringPoints(item.RowId);
    public GatheringPoint[] GetSpearfishingGatheringPoints(uint itemId)
    {
        if (!ItemSpearfishingItemCache.TryGetValue(itemId, out var spearfishingItem))
            return [];
        return SpearfishingItemGatheringPointsCache.GetValue(spearfishingItem.RowId) ?? [];
    }

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

        var itemName = TextService.GetItemName(item.RowId, ClientState.ClientLanguage);
        if (itemName.Length > 40)
            itemName = itemName[..40];

        addon->SearchTextInput->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }

    public FrozenDictionary<short, (uint Min, uint Max)> GetMaxLevelRanges()
    {
        if (MaxLevelRanges != null)
            return MaxLevelRanges;

        var dict = new Dictionary<short, (uint Min, uint Max)>();

        short level = 50;
        foreach (var exVersion in ExcelService.GetSheet<ExVersion>())
        {
            var entry = (Min: uint.MaxValue, Max: 0u);

            foreach (var item in ExcelService.GetSheet<Item>())
            {
                if (item.LevelEquip != level || item.LevelItem.Row <= 1)
                    continue;

                if (entry.Min > item.LevelItem.Row)
                    entry.Min = item.LevelItem.Row;

                if (entry.Max < item.LevelItem.Row)
                    entry.Max = item.LevelItem.Row;
            }

            dict.Add(level, entry);
            level += 10;
        }

        return MaxLevelRanges = dict.ToFrozenDictionary();
    }

    public uint GetItemRarityColorType(Item item, bool isEdgeColor = false)
        => (isEdgeColor ? 548u : 547u) + item.Rarity * 2u;

    public uint GetItemRarityColorType(uint itemId, bool isEdgeColor = false)
    {
        if (IsEventItem(itemId))
            return GetItemRarityColorType(1, isEdgeColor);

        var item = ExcelService.GetRow<Item>(GetBaseItemId(itemId));
        if (item == null)
            return GetItemRarityColorType(1, isEdgeColor);

        return GetItemRarityColorType(item, isEdgeColor);
    }

    public HaselColor GetItemRarityColor(Item item, bool isEdgeColor = false)
        => ExcelService.GetRow<UIColor>(GetItemRarityColorType(item, isEdgeColor))?.GetForegroundColor() ?? Colors.White;

    public HaselColor GetItemRarityColor(uint itemId, bool isEdgeColor = false)
    {
        if (IsEventItem(itemId))
            return isEdgeColor ? HaselColor.FromABGR(0x000000FF) : Colors.White;

        var item = ExcelService.GetRow<Item>(GetBaseItemId(itemId));
        if (item == null)
            return isEdgeColor ? HaselColor.FromABGR(0x000000FF) : Colors.White;

        return GetItemRarityColor(item, isEdgeColor);
    }

    public unsafe HaselColor GetItemLevelColor(byte classJob, Item item, params Vector4[] colors)
    {
        if (colors.Length < 2)
            throw new ArgumentException("At least two colors are required for interpolation.");

        var expArrayIndex = ExcelService.GetRow<ClassJob>(classJob)?.ExpArrayIndex;
        if (expArrayIndex is null or -1)
            return Colors.White;

        var level = PlayerState.Instance()->ClassJobLevels[(short)expArrayIndex];
        if (level < 1 || !GetMaxLevelRanges().TryGetValue(level, out var range))
            return Colors.White;

        var itemLevel = item.LevelItem.Row;

        // special case for Fisher's Secondary Tool
        // which has only one item, Spearfishing Gig
        if (item.ItemUICategory.Row == 99)
            return itemLevel == 180 ? Colors.Green : Colors.Red;

        if (itemLevel < range.Min)
            return Colors.Red;

        var value = (itemLevel - range.Min) / (float)(range.Max - range.Min);

        var startIndex = (int)(value * (colors.Length - 1));
        var endIndex = Math.Min(startIndex + 1, colors.Length - 1);

        if (startIndex < 0 || startIndex >= colors.Length || endIndex < 0 || endIndex >= colors.Length)
            return Colors.White;

        var t = value * (colors.Length - 1) - startIndex;
        return new(Vector4.Lerp(colors[startIndex], colors[endIndex], t));
    }

    public GatheringPoint[] GetGatheringPoints(GatheringItem gatheringItem)
        => GatheringItemGatheringPointsCache.GetValue(gatheringItem.RowId) ?? [];
}
