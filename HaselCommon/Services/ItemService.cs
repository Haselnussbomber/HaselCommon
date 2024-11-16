using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Cache.Internal;
using HaselCommon.Extensions;
using HaselCommon.Extensions.Sheets;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Services.SeStringEvaluation;
using HaselCommon.Sheets;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services;

public class ItemService(IClientState clientState, ExcelService excelService, SeStringEvaluatorService seStringEvaluatorService)
{
    private readonly GatheringItemGatheringPointsCache _gatheringItemGatheringPointsCache = new(excelService);
    private readonly SpearfishingItemGatheringPointsCache _spearfishingItemGatheringPointsCache = new(excelService);
    private readonly ItemRecipesCache _itemRecipesCache = new(excelService);
    private readonly ItemIconIdCache _itemIconIdCache = new(excelService);

    private static FrozenDictionary<short, (uint Min, uint Max)>? MaxLevelRanges = null;

    public uint GetBaseItemId(uint itemId)
    {
        if (IsEventItem(itemId)) return itemId; // uses EventItem sheet
        if (IsHighQuality(itemId)) return itemId - 1_000_000;
        if (IsCollectible(itemId)) return itemId - 500_000;
        return itemId;
    }

    public bool IsNormalItem(Item item) => IsNormalItem(item.RowId);
    public bool IsNormalItem(RowRef<Item> itemRef) => IsNormalItem(itemRef.RowId);
    public bool IsNormalItem(uint itemId) => itemId is < 500_000;

    public bool IsCollectible(Item item) => IsCollectible(item.RowId);
    public bool IsCollectible(RowRef<Item> itemRef) => IsCollectible(itemRef.RowId);
    public bool IsCollectible(uint itemId) => itemId is > 500_000 and < 1_000_000;

    public bool IsHighQuality(Item item) => IsHighQuality(item.RowId);
    public bool IsHighQuality(RowRef<Item> itemRef) => IsHighQuality(itemRef.RowId);
    public bool IsHighQuality(uint itemId) => itemId is > 1_000_000 and < 2_000_000;

    public bool IsEventItem(Item item) => IsEventItem(item.RowId);
    public bool IsEventItem(RowRef<Item> itemRef) => IsEventItem(itemRef.RowId);
    public bool IsEventItem(uint itemId) => itemId is > 2_000_000;

    public uint GetIconId(uint itemId) => _itemIconIdCache.GetValue(itemId);

    public string GetItemName(uint itemId, ClientLanguage? language = null)
    {
        if (IsEventItem(itemId))
            return excelService.TryGetRow<EventItem>(itemId, language, out var eventItem) ? eventItem.Name.ExtractText() : $"EventItem#{itemId}";

        return excelService.TryGetRow<Item>(GetBaseItemId(itemId), language, out var item) ? item.Name.ExtractText() : $"Item#{itemId}";
    }
    public string GetItemName(Item item) => GetItemName(item.RowId);
    public string GetItemName(RowRef<Item> itemRef) => GetItemName(itemRef.RowId);

    public IEnumerable<Recipe> GetRecipes(Item item) => GetRecipes(item.RowId);
    public IEnumerable<Recipe> GetRecipes(RowRef<Item> itemRef) => GetRecipes(itemRef.RowId);
    public IEnumerable<Recipe> GetRecipes(uint itemId) => _itemRecipesCache.GetValue(itemId) ?? [];

    public IEnumerable<GatheringItem> GetGatheringItems(Item item) => GetGatheringItems(item.RowId);
    public IEnumerable<GatheringItem> GetGatheringItems(RowRef<Item> itemRef) => GetGatheringItems(itemRef.RowId);
    public IEnumerable<GatheringItem> GetGatheringItems(uint itemId) => excelService.FindRows<GatheringItem>(row => row.Item.RowId == itemId);

    public IEnumerable<GatheringPoint> GetGatheringPoints(Item item) => GetGatheringPoints(item.RowId);
    public IEnumerable<GatheringPoint> GetGatheringPoints(RowRef<Item> itemRef) => GetGatheringPoints(itemRef.RowId);
    public IEnumerable<GatheringPoint> GetGatheringPoints(uint itemId) => GetGatheringItems(itemId).SelectMany(GetGatheringPoints).ToArray();

    public IEnumerable<FishingSpot> GetFishingSpots(Item item) => GetFishingSpots(item.RowId);
    public IEnumerable<FishingSpot> GetFishingSpots(RowRef<Item> itemRef) => GetFishingSpots(itemRef.RowId);
    public IEnumerable<FishingSpot> GetFishingSpots(uint itemId) => excelService.FindRows<FishingSpot>(row => row.Item.Any(item => item.RowId == itemId));

    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(Item item) => GetSpearfishingGatheringPoints(item.RowId);
    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(RowRef<Item> itemRef) => GetSpearfishingGatheringPoints(itemRef.RowId);
    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(uint itemId)
    {
        if (!excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out var spearfishingItem))
            return [];

        return _spearfishingItemGatheringPointsCache.GetValue(spearfishingItem.RowId) ?? [];
    }

    public bool IsCraftable(Item item) => IsCraftable(item.RowId);
    public bool IsCraftable(RowRef<Item> itemRef) => IsCraftable(itemRef.RowId);
    public bool IsCraftable(uint itemId) => GetRecipes(itemId).Any();

    public bool IsCrystal(Item item) => item.ItemUICategory.RowId == 59;
    public bool IsCrystal(RowRef<Item> itemRef) => itemRef.IsValid && IsCrystal(itemRef.Value);
    public bool IsCrystal(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && IsCrystal(item);

    public bool IsGatherable(Item item) => IsGatherable(item.RowId);
    public bool IsGatherable(RowRef<Item> itemRef) => IsGatherable(itemRef.RowId);
    public bool IsGatherable(uint itemId) => GetGatheringPoints(itemId).Any();

    public bool IsFish(Item item) => IsFish(item.RowId);
    public bool IsFish(RowRef<Item> itemRef) => IsFish(itemRef.RowId);
    public bool IsFish(uint itemId) => GetFishingSpots(itemId).Any();

    public bool IsSpearfish(Item item) => IsSpearfish(item.RowId);
    public bool IsSpearfish(RowRef<Item> itemRef) => IsSpearfish(itemRef.RowId);
    public bool IsSpearfish(uint itemId) => excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out _);

    public bool IsUnlockable(Item item) => item.ItemAction.RowId != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == item.ItemAction.Value!.Type);
    public bool IsUnlockable(RowRef<Item> itemRef) => itemRef.IsValid && IsUnlockable(itemRef.Value);
    public bool IsUnlockable(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && IsUnlockable(item);

    // TODO: check race/sex restrictions
    public bool CanTryOn(Item item)
    {
        return item.EquipSlotCategory.RowId switch
        {
            0 => false,
            2 when item.FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        };
    }
    public bool CanTryOn(RowRef<Item> itemRef) => itemRef.IsValid && CanTryOn(itemRef.Value);
    public bool CanTryOn(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && CanTryOn(item);

    public unsafe bool IsUnlocked(Item item)
    {
        if (item.ItemAction.RowId == 0)
            return false;

        var type = (ItemActionType)item.ItemAction.Value.Type;

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

            case ItemActionType.TripleTriadCard when item.AdditionalData.Is<TripleTriadCard>():
                return UIState.Instance()->IsTripleTriadCardUnlocked((ushort)item.AdditionalData.RowId);

            case ItemActionType.FolkloreTome:
                return PlayerState.Instance()->IsFolkloreBookUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.OrchestrionRoll when item.AdditionalData.Is<Orchestrion>():
                return PlayerState.Instance()->IsOrchestrionRollUnlocked(item.AdditionalData.RowId);

            case ItemActionType.FramersKit:
                return PlayerState.Instance()->IsFramersKitUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Ornament:
                return PlayerState.Instance()->IsOrnamentUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Glasses:
                return PlayerState.Instance()->IsGlassesUnlocked((ushort)item.AdditionalData.RowId);
        }

        var row = ExdModule.GetItemRowById(item.RowId);
        return row != null && UIState.Instance()->IsItemActionUnlocked(row) == 1;
    }
    public bool IsUnlocked(RowRef<Item> itemRef) => itemRef.IsValid && IsUnlocked(itemRef.Value);
    public bool IsUnlocked(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && IsUnlocked(item);

    public bool CanSearchForItem(Item item) => !item.IsUntradable && !item.IsCollectable && IsAddonOpen(AgentId.ItemSearch);
    public bool CanSearchForItem(RowRef<Item> itemRef) => itemRef.IsValid && CanSearchForItem(itemRef.Value);
    public bool CanSearchForItem(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && CanSearchForItem(item);

    public unsafe void Search(Item item)
    {
        if (!CanSearchForItem(item))
            return;

        if (!TryGetAddon<AddonItemSearch>(AgentId.ItemSearch, out var addon))
            return;

        if (TryGetAddon<AtkUnitBase>("ItemSearchResult", out var itemSearchResult))
            itemSearchResult->Hide2();

        var itemName = GetItemName(item.RowId, clientState.ClientLanguage);
        if (itemName.Length > 40)
            itemName = itemName[..40];

        addon->SearchTextInput->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }
    public void Search(RowRef<Item> itemRef)
    {
        if (itemRef.IsValid)
            Search(itemRef.Value);
    }

    public FrozenDictionary<short, (uint Min, uint Max)> GetMaxLevelRanges()
    {
        if (MaxLevelRanges != null)
            return MaxLevelRanges;

        var dict = new Dictionary<short, (uint Min, uint Max)>();

        short level = 50;
        foreach (var exVersion in excelService.GetSheet<ExVersion>())
        {
            var entry = (Min: uint.MaxValue, Max: 0u);

            foreach (var item in excelService.GetSheet<Item>())
            {
                if (item.LevelEquip != level || item.LevelItem.RowId <= 1)
                    continue;

                if (entry.Min > item.LevelItem.RowId)
                    entry.Min = item.LevelItem.RowId;

                if (entry.Max < item.LevelItem.RowId)
                    entry.Max = item.LevelItem.RowId;
            }

            dict.Add(level, entry);
            level += 10;
        }

        return MaxLevelRanges = dict.ToFrozenDictionary();
    }

    public uint GetItemRarityColorType(Item item, bool isEdgeColor = false)
        => (isEdgeColor ? 548u : 547u) + item.Rarity * 2u;

    public uint GetItemRarityColorType(RowRef<Item> itemRef, bool isEdgeColor = false)
        => GetItemRarityColorType(itemRef.RowId, isEdgeColor);

    public uint GetItemRarityColorType(uint itemId, bool isEdgeColor = false)
    {
        if (IsEventItem(itemId))
            return GetItemRarityColorType(1, isEdgeColor);

        if (!excelService.TryGetRow<Item>(GetBaseItemId(itemId), out var item))
            return GetItemRarityColorType(1, isEdgeColor);

        return GetItemRarityColorType(item, isEdgeColor);
    }

    public Color GetItemRarityColor(Item item, bool isEdgeColor = false)
        => excelService.TryGetRow<UIColor>(GetItemRarityColorType(item, isEdgeColor), out var color) ? color.GetForegroundColor() : Color.White;

    public Color GetItemRarityColor(RowRef<Item> itemRef, bool isEdgeColor = false)
        => itemRef.IsValid ? GetItemRarityColor(itemRef) : Color.White;

    public Color GetItemRarityColor(uint itemId, bool isEdgeColor = false)
    {
        if (IsEventItem(itemId))
            return isEdgeColor ? Color.FromABGR(0x000000FF) : Color.White;

        if (!excelService.TryGetRow<Item>(GetBaseItemId(itemId), out var item))
            return isEdgeColor ? Color.FromABGR(0x000000FF) : Color.White;

        return GetItemRarityColor(item, isEdgeColor);
    }

    public unsafe Color GetItemLevelColor(byte classJob, Item item, params Vector4[] colors)
    {
        if (colors.Length < 2)
            throw new ArgumentException("At least two colors are required for interpolation.");

        if (!excelService.TryGetRow<ClassJob>(classJob, out var classJobRow))
            return Color.White;

        var expArrayIndex = classJobRow.ExpArrayIndex;
        if (expArrayIndex == -1)
            return Color.White;

        var level = PlayerState.Instance()->ClassJobLevels[(short)expArrayIndex];
        if (level < 1 || !GetMaxLevelRanges().TryGetValue(level, out var range))
            return Color.White;

        var itemLevel = item.LevelItem.RowId;

        // special case for Fisher's Secondary Tool
        // which has only one item, Spearfishing Gig
        if (item.ItemUICategory.RowId == 99)
            return itemLevel == 180 ? Color.Green : Color.Red;

        if (itemLevel < range.Min)
            return Color.Red;

        var value = (itemLevel - range.Min) / (float)(range.Max - range.Min);

        var startIndex = (int)(value * (colors.Length - 1));
        var endIndex = System.Math.Min(startIndex + 1, colors.Length - 1);

        if (startIndex < 0 || startIndex >= colors.Length || endIndex < 0 || endIndex >= colors.Length)
            return Color.White;

        var t = value * (colors.Length - 1) - startIndex;
        return new(Vector4.Lerp(colors[startIndex], colors[endIndex], t));
    }

    public GatheringPoint[] GetGatheringPoints(GatheringItem gatheringItem)
        => _gatheringItemGatheringPointsCache.GetValue(gatheringItem.RowId) ?? [];

    public ReadOnlySeString GetItemLink(uint id, ClientLanguage? language = null)
    {
        var itemName = GetItemName(id, language);

        if (IsHighQuality(id))
            itemName += " \uE03C";
        else if (IsCollectible(id))
            itemName += " \uE03D";

        var sb = SeStringBuilder.SharedPool.Get();
        var itemLink = sb
            .PushColorType(GetItemRarityColorType(id, false))
            .PushEdgeColorType(GetItemRarityColorType(id, true))
            .PushLinkItem(id, itemName)
            .Append(itemName)
            .PopLink()
            .PopEdgeColorType()
            .PopColorType()
            .ToReadOnlySeString();
        SeStringBuilder.SharedPool.Return(sb);

        return seStringEvaluatorService.EvaluateFromAddon(371, new SeStringContext()
        {
            Language = language,
            LocalParameters = [itemLink]
        });
    }

    public unsafe uint GetHairstyleIconId(uint id, byte? tribeId = null, byte? sexId = null)
    {
        if (tribeId == null)
        {
            tribeId = 1;

            var character = Control.GetLocalPlayer();
            if (character != null)
                tribeId = character->DrawData.CustomizeData.Tribe;
        }

        if (sexId == null)
        {
            sexId = 1;

            var character = Control.GetLocalPlayer();
            if (character != null)
                sexId = character->DrawData.CustomizeData.Sex;
        }

        if (!excelService.TryFindRow<HairMakeTypeCustom>(t => t.HairMakeType.Tribe.RowId == tribeId && t.HairMakeType.Gender == sexId, out var hairMakeType))
            return 0;

        if (!hairMakeType.HairStyles.TryGetFirst(h => h.Value.HintItem.RowId == id, out var item) || item.IsValid)
            return 0;

        return item.Value.Icon;
    }
}
