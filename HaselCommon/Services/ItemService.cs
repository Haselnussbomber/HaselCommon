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
using HaselCommon.Extensions.Sheets;
using HaselCommon.Extensions.Strings;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Services.SeStringEvaluation;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services;

public class ItemService(IClientState clientState, ExcelService excelService, TextService textService, SeStringEvaluatorService seStringEvaluatorService)
{
    private readonly Dictionary<(uint, ClientLanguage), string> _itemNameCache = [];
    private readonly Dictionary<uint, bool> _isCraftableCache = [];
    private readonly Dictionary<uint, bool> _isGatherableCache = [];
    private readonly Dictionary<uint, bool> _isFishCache = [];
    private readonly Dictionary<uint, bool> _isSpearfishCache = [];
    private readonly Dictionary<uint, GatheringPoint[]> _gatheringItemGatheringPointsCache = [];
    private readonly Dictionary<uint, GatheringPoint[]> _spearfishingItemGatheringPointsCache = [];
    private readonly Dictionary<uint, uint> _itemIconIdCache = [];
    private readonly Dictionary<uint, Recipe[]> _recipesCache = [];
    private readonly Dictionary<uint, ItemAmount[]> _ingredientsCache = [];
    private readonly Dictionary<uint, GatheringItem[]> _gatheringItemsCache = [];
    private readonly Dictionary<uint, GatheringPoint[]> _gatheringPointsCache = [];
    private readonly Dictionary<uint, FishingSpot[]> _fishingSpotsCache = [];

    private FrozenDictionary<short, (uint Min, uint Max)>? _maxLevelRanges = null;

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

    public uint GetIconId(uint itemId)
    {
        if (_itemIconIdCache.TryGetValue(itemId, out var iconId))
            return iconId;

        if (IsEventItem(itemId))
        {
            _itemIconIdCache.Add(itemId, iconId = excelService.TryGetRow<EventItem>(itemId, out var eventItem) ? eventItem.Icon : 0u);
            return iconId;
        }

        _itemIconIdCache.Add(itemId, iconId = excelService.TryGetRow<Item>(GetBaseItemId(itemId), out var item) ? item.Icon : 0u);
        return iconId;
    }

    public string GetItemName(Item item) => GetItemName(item.RowId);
    public string GetItemName(RowRef<Item> itemRef) => GetItemName(itemRef.RowId);
    public string GetItemName(uint itemId, ClientLanguage? language = null)
    {
        var effectiveLanguage = language ?? textService.ClientLanguage;
        var key = (itemId, effectiveLanguage);

        if (_itemNameCache.TryGetValue(key, out var name))
            return name;

        if (IsEventItem(itemId))
        {
            _itemNameCache.Add(key, name = excelService.TryGetRow<EventItem>(itemId, language, out var eventItem)
                ? eventItem.Name.ExtractText().StripSoftHypen()
                : $"EventItem#{itemId}");
            return name;
        }

        _itemNameCache.Add(key, name = excelService.TryGetRow<Item>(GetBaseItemId(itemId), language, out var item)
            ? item.Name.ExtractText().StripSoftHypen()
            : $"Item#{itemId}");
        return name;
    }

    public Recipe[] GetRecipes(Item item) => GetRecipes(item.RowId);
    public Recipe[] GetRecipes(RowRef<Item> itemRef) => GetRecipes(itemRef.RowId);
    public Recipe[] GetRecipes(uint itemId)
    {
        if (_recipesCache.TryGetValue(itemId, out var recipes))
            return recipes;

        if (!excelService.TryGetRow<RecipeLookup>(itemId, out var lookup))
        {
            _recipesCache.Add(itemId, []);
            return [];
        }

        var recipesList = new List<Recipe>();

        if (lookup.CRP.RowId != 0 && lookup.CRP.IsValid) recipesList.Add(lookup.CRP.Value);
        if (lookup.BSM.RowId != 0 && lookup.BSM.IsValid) recipesList.Add(lookup.BSM.Value);
        if (lookup.ARM.RowId != 0 && lookup.ARM.IsValid) recipesList.Add(lookup.ARM.Value);
        if (lookup.GSM.RowId != 0 && lookup.GSM.IsValid) recipesList.Add(lookup.GSM.Value);
        if (lookup.LTW.RowId != 0 && lookup.LTW.IsValid) recipesList.Add(lookup.LTW.Value);
        if (lookup.WVR.RowId != 0 && lookup.WVR.IsValid) recipesList.Add(lookup.WVR.Value);
        if (lookup.ALC.RowId != 0 && lookup.ALC.IsValid) recipesList.Add(lookup.ALC.Value);
        if (lookup.CUL.RowId != 0 && lookup.CUL.IsValid) recipesList.Add(lookup.CUL.Value);

        var recipesArray = recipesList.ToArray();
        _recipesCache.Add(itemId, recipesArray);
        return recipesArray;
    }

    public ItemAmount[] GetIngredients(Item item) => GetIngredients(item.RowId);
    public ItemAmount[] GetIngredients(RowRef<Item> itemRef) => GetIngredients(itemRef.RowId);
    public ItemAmount[] GetIngredients(uint itemId)
    {
        if (_ingredientsCache.TryGetValue(itemId, out var ingredients))
            return ingredients;

        var recipes = GetRecipes(itemId);
        if (recipes.Length == 0)
        {
            _ingredientsCache.Add(itemId, ingredients = []);
            return ingredients;
        }

        var list = new List<ItemAmount>();
        var recipe = recipes.First();

        for (var i = 0; i < recipe.Ingredient.Count; i++)
        {
            var ingredient = recipe.Ingredient[i];
            if (ingredient.RowId == 0 || !ingredient.IsValid)
                continue;

            var amount = recipe.AmountIngredient[i];
            if (amount == 0)
                continue;

            list.Add(new(ingredient.Value, amount));
        }

        _ingredientsCache.Add(itemId, ingredients = [.. list]);

        return ingredients;
    }

    public GatheringItem[] GetGatheringItems(Item item) => GetGatheringItems(item.RowId);
    public GatheringItem[] GetGatheringItems(RowRef<Item> itemRef) => GetGatheringItems(itemRef.RowId);
    public GatheringItem[] GetGatheringItems(uint itemId)
    {
        if (_gatheringItemsCache.TryGetValue(itemId, out var gatheringItems))
            return gatheringItems;

        _gatheringItemsCache.Add(itemId, gatheringItems = excelService.FindRows<GatheringItem>(row => row.Item.RowId == itemId));

        return gatheringItems;
    }

    public GatheringPoint[] GetGatheringPoints(Item item) => GetGatheringPoints(item.RowId);
    public GatheringPoint[] GetGatheringPoints(RowRef<Item> itemRef) => GetGatheringPoints(itemRef.RowId);
    public GatheringPoint[] GetGatheringPoints(uint itemId)
    {
        if (_gatheringPointsCache.TryGetValue(itemId, out var gatheringPoints))
            return gatheringPoints;

        _gatheringPointsCache.Add(itemId, gatheringPoints = GetGatheringItems(itemId).SelectMany(GetGatheringPoints).ToArray());

        return gatheringPoints;
    }

    public FishingSpot[] GetFishingSpots(Item item) => GetFishingSpots(item.RowId);
    public FishingSpot[] GetFishingSpots(RowRef<Item> itemRef) => GetFishingSpots(itemRef.RowId);
    public FishingSpot[] GetFishingSpots(uint itemId)
    {
        if (_fishingSpotsCache.TryGetValue(itemId, out var fishingSpots))
            return fishingSpots;

        _fishingSpotsCache.Add(itemId, fishingSpots = excelService.FindRows<FishingSpot>(row => row.Item.Any(item => item.RowId == itemId)));

        return fishingSpots;
    }

    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(Item item) => GetSpearfishingGatheringPoints(item.RowId);
    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(RowRef<Item> itemRef) => GetSpearfishingGatheringPoints(itemRef.RowId);
    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(uint itemId)
    {
        if (_spearfishingItemGatheringPointsCache.TryGetValue(itemId, out var points))
            return points;

        if (!excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out var spearfishingItem))
        {
            _spearfishingItemGatheringPointsCache.Add(itemId, points = []);
            return points;
        }

        var bases = excelService.FindRows<GatheringPointBase>(row => row.GatheringType.RowId == 5 && row.Item.Any(item => item.RowId == itemId));
        points = excelService.FindRows<GatheringPoint>(row => bases.Any(b => b.RowId == row.GatheringPointBase.RowId));
        _spearfishingItemGatheringPointsCache.Add(itemId, points);
        return points;
    }

    public bool IsCraftable(Item item) => IsCraftable(item.RowId);
    public bool IsCraftable(RowRef<Item> itemRef) => IsCraftable(itemRef.RowId);
    public bool IsCraftable(uint itemId)
    {
        if (_isCraftableCache.TryGetValue(itemId, out var result))
            return result;

        _isCraftableCache.Add(itemId, result = GetRecipes(itemId).Length != 0);

        return result;
    }

    public bool IsCrystal(Item item) => item.ItemUICategory.RowId == 59;
    public bool IsCrystal(RowRef<Item> itemRef) => itemRef.IsValid && IsCrystal(itemRef.Value);
    public bool IsCrystal(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && IsCrystal(item);

    public bool IsGatherable(Item item) => IsGatherable(item.RowId);
    public bool IsGatherable(RowRef<Item> itemRef) => IsGatherable(itemRef.RowId);
    public bool IsGatherable(uint itemId)
    {
        if (_isGatherableCache.TryGetValue(itemId, out var result))
            return result;

        _isGatherableCache.Add(itemId, result = GetGatheringPoints(itemId).Length != 0);

        return result;
    }

    public bool IsFish(Item item) => IsFish(item.RowId);
    public bool IsFish(RowRef<Item> itemRef) => IsFish(itemRef.RowId);
    public bool IsFish(uint itemId)
    {
        if (_isFishCache.TryGetValue(itemId, out var result))
            return result;

        _isFishCache.Add(itemId, result = GetFishingSpots(itemId).Length != 0);

        return result;
    }

    public bool IsSpearfish(Item item) => IsSpearfish(item.RowId);
    public bool IsSpearfish(RowRef<Item> itemRef) => IsSpearfish(itemRef.RowId);
    public bool IsSpearfish(uint itemId)
    {
        if (_isSpearfishCache.TryGetValue(itemId, out var result))
            return result;

        _isSpearfishCache.Add(itemId, result = excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out _));

        return result;
    }

    public bool IsUnlockable(Item item) => item.ItemAction.RowId != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == item.ItemAction.Value!.Type);
    public bool IsUnlockable(RowRef<Item> itemRef) => itemRef.IsValid && IsUnlockable(itemRef.Value);
    public bool IsUnlockable(uint itemId) => excelService.TryGetRow<Item>(itemId, out var item) && IsUnlockable(item);

    public unsafe bool CanTryOn(Item item)
    {
        if (!(item.EquipSlotCategory.RowId switch
        {
            0 => false, // not equippable
            2 when item.FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        }))
        {
            return false;
        }

        var playerState = PlayerState.Instance();
        if (playerState->IsLoaded == 0)
            return false;

        if (!excelService.TryGetRow<EquipRaceCategory>(item.EquipRestriction, out var equipRaceCategoryRow))
            return false;

        if (!excelService.GetSheet<RawRow>(null, "EquipRaceCategory").TryGetRow(item.EquipRestriction, out var equipRaceCategoryRawRow))
            return false;

        var race = playerState->Race;
        if (race == 0 || !equipRaceCategoryRawRow.ReadBool(race - 1u))
            return false;

        var sex = playerState->Sex;
        if (sex == 1) // is female
            return equipRaceCategoryRow.Female;

        return equipRaceCategoryRow.Male;
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
        if (_maxLevelRanges != null)
            return _maxLevelRanges;

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

        return _maxLevelRanges = dict.ToFrozenDictionary();
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
    {
        if (_gatheringItemGatheringPointsCache.TryGetValue(gatheringItem.RowId, out var points))
            return points;

        var pointBases = new HashSet<uint>();

        if (gatheringItem.IsHidden)
        {
            var gatheringItemPointSheet = excelService.GetSubrowSheet<GatheringItemPoint>();

            foreach (var row in gatheringItemPointSheet)
            {
                if (row.RowId < gatheringItem.RowId) continue;
                if (row.RowId > gatheringItem.RowId) break;

                foreach (var subrow in row)
                {
                    if (subrow.RowId != 0 && subrow.GatheringPoint.IsValid)
                        pointBases.Add(subrow.GatheringPoint.Value.GatheringPointBase.RowId);
                }
            }
        }

        var gatheringPointSheet = excelService.GetSheet<GatheringPoint>();

        foreach (var point in gatheringPointSheet)
        {
            if (point.TerritoryType.RowId <= 1)
                continue;

            if (!point.GatheringPointBase.IsValid)
                continue;

            var gatheringPointBase = point.GatheringPointBase.Value;

            // only accept Mining, Quarrying, Logging and Harvesting
            if (gatheringPointBase.GatheringType.RowId >= 5)
                continue;

            foreach (var id in gatheringPointBase.Item)
            {
                if (id.RowId == gatheringItem.RowId)
                    pointBases.Add(gatheringPointBase.RowId);
            }
        }

        points = pointBases
            .Select((baseId) => gatheringPointSheet.Where((row) => row.TerritoryType.RowId > 1 && row.GatheringPointBase.RowId == baseId))
            .SelectMany(e => e)
            .OfType<GatheringPoint>()
            .ToArray();

        _gatheringItemGatheringPointsCache.Add(gatheringItem.RowId, points);

        return points;
    }

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
