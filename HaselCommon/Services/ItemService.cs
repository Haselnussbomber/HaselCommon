using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.Exd;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Lumina.Text.ReadOnly;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ItemService
{
    private readonly IClientState _clientState;
    private readonly ExcelService _excelService;
    private readonly SeStringEvaluator _seStringEvaluatorService;
    private readonly TextService _textService;

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
    private readonly Dictionary<(uint, byte, byte), uint> _hairStyleIconsCache = [];

    private FrozenDictionary<short, (uint Min, uint Max)>? _maxLevelRanges = null;

    public uint GetIconId(uint itemId)
    {
        if (_itemIconIdCache.TryGetValue(itemId, out var iconId))
            return iconId;

        if (ItemUtil.IsEventItem(itemId))
        {
            _itemIconIdCache.Add(itemId, iconId = _excelService.TryGetRow<EventItem>(itemId, out var eventItem) ? eventItem.Icon : 0u);
            return iconId;
        }

        _itemIconIdCache.Add(itemId, iconId = _excelService.TryGetRow<Item>(ItemUtil.GetBaseId(itemId).ItemId, out var item) ? item.Icon : 0u);
        return iconId;
    }

    public Recipe[] GetRecipes(uint itemId)
    {
        if (_recipesCache.TryGetValue(itemId, out var recipes))
            return recipes;

        if (!_excelService.TryGetRawRow("RecipeLookup", itemId, out var lookup))
        {
            _recipesCache.Add(itemId, []);
            return [];
        }

        var recipesList = new List<Recipe>();

        for (nuint i = 0; i < 8; i++)
        {
            var recipeId = lookup.ReadUInt16(i * 2);
            if (recipeId == 0)
                continue;

            if (_excelService.TryGetRow<Recipe>(recipeId, out var recipe))
                recipesList.Add(recipe);
        }

        var recipesArray = recipesList.ToArray();
        _recipesCache.Add(itemId, recipesArray);
        return recipesArray;
    }

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

    public GatheringItem[] GetGatheringItems(uint itemId)
    {
        if (_gatheringItemsCache.TryGetValue(itemId, out var gatheringItems))
            return gatheringItems;

        _gatheringItemsCache.Add(itemId, gatheringItems = _excelService.FindRows<GatheringItem>(row => row.Item.RowId == itemId));

        return gatheringItems;
    }

    public GatheringPoint[] GetGatheringPoints(uint itemId)
    {
        if (_gatheringPointsCache.TryGetValue(itemId, out var gatheringPoints))
            return gatheringPoints;

        _gatheringPointsCache.Add(itemId, gatheringPoints = [.. GetGatheringItems(itemId).SelectMany(GetGatheringPoints)]);

        return gatheringPoints;
    }

    public FishingSpot[] GetFishingSpots(uint itemId)
    {
        if (_fishingSpotsCache.TryGetValue(itemId, out var fishingSpots))
            return fishingSpots;

        _fishingSpotsCache.Add(itemId, fishingSpots = _excelService.FindRows<FishingSpot>(row => row.Item.Any(item => item.RowId == itemId)));

        return fishingSpots;
    }

    public IEnumerable<GatheringPoint> GetSpearfishingGatheringPoints(uint itemId)
    {
        if (_spearfishingItemGatheringPointsCache.TryGetValue(itemId, out var points))
            return points;

        if (!_excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out var spearfishingItem))
        {
            _spearfishingItemGatheringPointsCache.Add(itemId, points = []);
            return points;
        }

        var bases = _excelService.FindRows<GatheringPointBase>(row => row.GatheringType.RowId == 5 && row.Item.Any(item => item.RowId == spearfishingItem.RowId));
        points = _excelService.FindRows<GatheringPoint>(row => bases.Any(b => b.RowId == row.GatheringPointBase.RowId));
        _spearfishingItemGatheringPointsCache.Add(itemId, points);
        return points;
    }

    public bool IsCraftable(uint itemId)
    {
        if (_isCraftableCache.TryGetValue(itemId, out var result))
            return result;

        _isCraftableCache.Add(itemId, result = GetRecipes(itemId).Length != 0);

        return result;
    }

    public bool IsCrystal(uint itemId)
    {
        return _excelService.TryGetRow<Item>(itemId, out var item) && item.ItemUICategory.RowId == 59;
    }

    public bool IsGatherable(uint itemId)
    {
        if (_isGatherableCache.TryGetValue(itemId, out var result))
            return result;

        _isGatherableCache.Add(itemId, result = GetGatheringPoints(itemId).Length != 0);

        return result;
    }

    public bool IsFish(uint itemId)
    {
        if (_isFishCache.TryGetValue(itemId, out var result))
            return result;

        _isFishCache.Add(itemId, result = GetFishingSpots(itemId).Length != 0);

        return result;
    }

    public bool IsSpearfish(uint itemId)
    {
        if (_isSpearfishCache.TryGetValue(itemId, out var result))
            return result;

        _isSpearfishCache.Add(itemId, result = _excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out _));

        return result;
    }

    public bool IsUnlockable(uint itemId)
    {
        if (!_excelService.TryGetRow<Item>(itemId, out var item))
            return false;

        if (item.ItemAction.RowId == 0)
            return false;

        return (ItemActionType)item.ItemAction.Value.Type is
            ItemActionType.Companion or
            ItemActionType.BuddyEquip or
            ItemActionType.Mount or
            ItemActionType.SecretRecipeBook or
            ItemActionType.UnlockLink or
            ItemActionType.TripleTriadCard or
            ItemActionType.FolkloreTome or
            ItemActionType.OrchestrionRoll or
            ItemActionType.FramersKit or
            ItemActionType.Ornament or
            ItemActionType.Glasses;
    }

    public unsafe bool CanTryOn(uint itemId)
    {
        if (!_excelService.TryGetRow<Item>(itemId, out var item))
            return false;

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

        if (!_excelService.TryGetRow<EquipRaceCategory>(item.EquipRestriction, out var equipRaceCategoryRow))
            return false;

        if (!_excelService.TryGetRawRow("EquipRaceCategory", item.EquipRestriction, out var equipRaceCategoryRawRow))
            return false;

        var race = playerState->Race;
        if (race == 0 || !equipRaceCategoryRawRow.ReadBool(race - 1u))
            return false;

        var sex = playerState->Sex;
        if (sex == 1) // is female
            return equipRaceCategoryRow.Female;

        return equipRaceCategoryRow.Male;
    }

    public unsafe bool IsUnlocked(uint itemId)
    {
        if (!_excelService.TryGetRow<Item>(itemId, out var item))
            return false;

        if (item.ItemAction.RowId == 0)
            return false;

        // just to avoid the ExdModule.GetItemRowById call...
        switch ((ItemActionType)item.ItemAction.Value.Type)
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
                return PlayerState.Instance()->IsFramersKitUnlocked(item.AdditionalData.RowId);

            case ItemActionType.Ornament:
                return PlayerState.Instance()->IsOrnamentUnlocked(item.ItemAction.Value.Data[0]);

            case ItemActionType.Glasses:
                return PlayerState.Instance()->IsGlassesUnlocked((ushort)item.AdditionalData.RowId);

            case ItemActionType.CompanySealVouchers:
                return false;
        }

        var row = ExdModule.GetItemRowById(item.RowId);
        return row != null && UIState.Instance()->IsItemActionUnlocked(row) == 1;
    }

    public bool CanSearchForItem(uint itemId)
    {
        return _excelService.TryGetRow<Item>(itemId, out var item) && !item.IsUntradable && !item.IsCollectable && IsAddonOpen(AgentId.ItemSearch);
    }

    public unsafe void Search(uint item)
    {
        if (!CanSearchForItem(item))
            return;

        if (!TryGetAddon<AddonItemSearch>(AgentId.ItemSearch, out var addon))
            return;

        if (TryGetAddon<AtkUnitBase>("ItemSearchResult", out var itemSearchResult))
            itemSearchResult->Hide2();

        var itemName = _textService.GetItemName(item, _clientState.ClientLanguage);
        if (itemName.Length > 40)
            itemName = itemName[..40];

        addon->SearchTextInput->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }

    public FrozenDictionary<short, (uint Min, uint Max)> GetMaxLevelRanges()
    {
        if (_maxLevelRanges != null)
            return _maxLevelRanges;

        var dict = new Dictionary<short, (uint Min, uint Max)>();

        short level = 50;
        foreach (var exVersion in _excelService.GetSheet<ExVersion>())
        {
            var entry = (Min: uint.MaxValue, Max: 0u);

            foreach (var item in _excelService.GetSheet<Item>())
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

    public Color GetItemRarityColor(uint itemId, bool isEdgeColor = false)
    {
        if (ItemUtil.IsEventItem(itemId))
            return isEdgeColor ? Color.Black : Color.White;

        itemId = ItemUtil.GetBaseId(itemId).ItemId;

        if (!_excelService.GetSheet<Item>().HasRow(itemId))
            return isEdgeColor ? Color.Black : Color.White;

        if (!_excelService.TryGetRow<UIColor>(ItemUtil.GetItemRarityColorType(itemId, isEdgeColor), out var color))
            return Color.White;

        return Color.FromABGR(color.Dark);
    }

    public unsafe Color GetItemLevelColor(byte classJob, Item item, params Color[] colors)
    {
        if (colors.Length < 2)
            throw new ArgumentException("At least two colors are required for interpolation.");

        if (!_excelService.TryGetRow<ClassJob>(classJob, out var classJobRow))
            return Color.White;

        var expArrayIndex = classJobRow.ExpArrayIndex;
        if (expArrayIndex == -1)
            return Color.White;

        var level = PlayerState.Instance()->ClassJobLevels[expArrayIndex];
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
        return Color.FromVector4(Vector4.Lerp(colors[startIndex], colors[endIndex], t));
    }

    public GatheringPoint[] GetGatheringPoints(GatheringItem gatheringItem)
    {
        if (_gatheringItemGatheringPointsCache.TryGetValue(gatheringItem.RowId, out var points))
            return points;

        var pointBases = new HashSet<uint>();

        if (gatheringItem.IsHidden)
        {
            var gatheringItemPointSheet = _excelService.GetSubrowSheet<GatheringItemPoint>();

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

        var gatheringPointSheet = _excelService.GetSheet<GatheringPoint>();

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

        points = [.. pointBases
            .Select((baseId) => gatheringPointSheet.Where((row) => row.TerritoryType.RowId > 1 && row.GatheringPointBase.RowId == baseId))
            .SelectMany(e => e)
            .OfType<GatheringPoint>()];

        _gatheringItemGatheringPointsCache.Add(gatheringItem.RowId, points);

        return points;
    }

    public ReadOnlySeString GetItemLink(uint itemId, ClientLanguage? language = null)
    {
        var itemName = _textService.GetItemName(itemId, language);

        if (ItemUtil.IsHighQuality(itemId))
            itemName += " \uE03C";
        else if (ItemUtil.IsCollectible(itemId))
            itemName += " \uE03D";

        ReadOnlySeString itemLink;
        using (SeStringBuilderHelper.Rent(out var sb))
        {
            itemLink = sb
                .PushColorType(ItemUtil.GetItemRarityColorType(itemId, false))
                .PushEdgeColorType(ItemUtil.GetItemRarityColorType(itemId, true))
                .PushLinkItem(itemId, itemName)
                .Append(itemName)
                .PopLink()
                .PopEdgeColorType()
                .PopColorType()
                .ToReadOnlySeString();
        }

        return _seStringEvaluatorService.EvaluateFromAddon(371, [itemLink], language);
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

        var key = (id, (byte)tribeId, (byte)sexId);
        if (_hairStyleIconsCache.TryGetValue(key, out var icon))
            return icon;

        if (!_excelService.TryFindRow<HairMakeType>(t => t.Tribe.RowId == tribeId && t.Gender == sexId, out var hairMakeType))
        {
            _hairStyleIconsCache.Add(key, 0);
            return 0;
        }

        if (!hairMakeType.CharaMakeStruct[0].SubMenuParam
            .Select(rowId => _excelService.CreateRef<CharaMakeCustomize>(rowId))
            .Where(rowRef => rowRef.RowId != 0 && rowRef.IsValid)
            .TryGetFirst(h => h.Value.HintItem.RowId == id, out var item) || !item.IsValid)
        {
            _hairStyleIconsCache.Add(key, 0);
            return 0;
        }

        _hairStyleIconsCache.Add(key, item.Value.Icon);
        return item.Value.Icon;
    }
}
