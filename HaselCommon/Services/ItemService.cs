using System.Collections.Concurrent;
using System.Collections.Frozen;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using HaselCommon.Game.Enums;
using HaselCommon.Sheets;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ItemService
{
    private readonly ExcelService _excelService;
    private readonly ISeStringEvaluator _seStringEvaluatorService;
    private readonly LanguageProvider _languageProvider;

    private readonly ConcurrentDictionary<uint, ItemCacheEntry> _itemCache = [];
    private readonly Dictionary<uint, GatheringPoint[]> _gatheringItemGatheringPointsCache = [];
    private FrozenDictionary<short, (uint Min, uint Max)>? _maxLevelRanges = null;

    public static ItemService? Instance => ServiceLocator.GetService<ItemService>();

    public uint GetItemIcon(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.IconId ??= item.IsEventItem
            ? item.TryGetEventItem(out var eventItem) ? eventItem.Icon : 0u
            : item.TryGetItem(out var itemRow) ? itemRow.Icon : 0u;
    }

    public ReadOnlySeString GetItemName(ItemHandle item, bool includeIcon, ClientLanguage? language = null)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var lang = language ?? _languageProvider.ClientLanguage;
        return entry.ItemNames.GetOrAdd((lang, includeIcon), _ => ItemUtil.GetItemName(item.ItemId, includeIcon, lang));
    }

    public ReadOnlySeString GetItemDescription(ItemHandle item, ClientLanguage? language = null)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var lang = language ?? _languageProvider.ClientLanguage;
        return entry.ItemDescriptions.GetOrAdd(lang, _ =>
        {
            if (item.IsEventItem && _excelService.TryGetRow<EventItemHelp>(item, lang, out var eventItemHelp))
            {
                return eventItemHelp.Description;
            }
            else if (item.TryGetItem(lang, out var itemRow))
            {
                return itemRow.Description;
            }

            return default;
        });
    }

    public ReadOnlySeString GetItemLink(ItemHandle item, ClientLanguage? language = null)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.ItemLinks.GetOrAdd(language ?? _languageProvider.ClientLanguage, clientLanguage =>
        {
            using var rssb = new RentedSeStringBuilder();

            var itemName = item.GetItemName(true, clientLanguage);
            var itemLink = rssb.Builder
                .PushColorType(item.RarityColorType)
                .PushEdgeColorType(item.RarityEdgeColorType)
                .PushLinkItem(item.ItemId, itemName.ToString())
                .Append(itemName)
                .PopLink()
                .PopEdgeColorType()
                .PopColorType()
                .ToReadOnlySeString();

            return _seStringEvaluatorService.EvaluateFromAddon(371, [itemLink], language);
        });
    }

    public bool IsCraftable(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.IsCraftable ??= GetRecipes(item).Count != 0;
    }

    public bool IsGatherable(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.IsGatherable ??= GetGatheringPoints(item).Count != 0;
    }

    public bool IsFish(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.IsFish ??= GetFishingSpots(item).Count != 0;
    }

    public bool IsSpearfish(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var itemId = item.ItemId;
        return entry.IsSpearfish ??= _excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out _);
    }

    public IReadOnlyList<Recipe> GetRecipes(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());

        if (entry.Recipes is { } recipes)
            return recipes;

        if (!_excelService.TryGetRawRow("RecipeLookup", item.ItemId, out var lookup))
            return entry.Recipes = [];

        var recipesList = new List<Recipe>();

        for (nuint i = 0; i < 8; i++)
        {
            var recipeId = lookup.ReadUInt16(i * 2);
            if (recipeId == 0)
                continue;

            if (_excelService.TryGetRow<Recipe>(recipeId, out var recipe))
                recipesList.Add(recipe);
        }

        return entry.Recipes = [.. recipesList];
    }

    public IReadOnlyList<ItemAmount> GetIngredients(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());

        if (entry.Ingredients is { } ingredients)
            return ingredients;

        var recipes = GetRecipes(item);
        if (recipes.Count == 0)
            return entry.Ingredients = [];

        var list = new List<ItemAmount>();
        var recipe = recipes[0];

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

        return entry.Ingredients = [.. list];
    }

    public IReadOnlyList<GatheringItem> GetGatheringItems(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var itemId = item.ItemId;
        return entry.GatheringItems ??= _excelService.FindRows<GatheringItem>(row => row.Item.RowId == itemId);
    }

    public IReadOnlyList<GatheringPoint> GetGatheringPoints(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        return entry.GatheringPoints ??= [.. GetGatheringItems(item).SelectMany(GetGatheringPoints)];
    }

    public IReadOnlyList<FishingSpot> GetFishingSpots(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var itemId = item.ItemId;
        return entry.FishingSpots ??= _excelService.FindRows<FishingSpot>(row => row.Item.Any(item => item.RowId == itemId));
    }

    public IReadOnlyList<GatheringPoint> GetSpearfishingGatheringPoints(ItemHandle item)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());

        if (entry.SpearfishingPoints is { } spearfishingPoints)
            return spearfishingPoints;

        var itemId = item.ItemId;
        if (!_excelService.TryFindRow<SpearfishingItem>(row => row.Item.RowId == itemId, out var spearfishingItem))
            return entry.SpearfishingPoints = [];

        var bases = _excelService.FindRows<GatheringPointBase>(row => row.GatheringType.RowId == 5 && row.Item.Any(item => item.RowId == spearfishingItem.RowId));
        return entry.SpearfishingPoints = _excelService.FindRows<GatheringPoint>(row => bases.Any(b => b.RowId == row.GatheringPointBase.RowId));
    }

    public IReadOnlyList<GatheringPoint> GetGatheringPoints(GatheringItem gatheringItem)
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

    public unsafe uint GetHairstyleIconId(ItemHandle item, byte? tribeId = null, byte? sexId = null)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());

        if (tribeId is not { } tribe)
        {
            var character = Control.GetLocalPlayer();
            tribe = character != null ? character->DrawData.CustomizeData.Tribe : (byte)1;
        }

        if (sexId is not { } sex)
        {
            var character = Control.GetLocalPlayer();
            sex = character != null ? character->DrawData.CustomizeData.Sex : (byte)1;
        }

        return entry.HairStyleIcons.GetOrAdd((tribe, sex), _ =>
        {
            if (!_excelService.TryFindRow<HairMakeType>(t => t.Tribe.RowId == tribe && t.Gender == sex, out var hairMakeType))
                return 0;

            if (!hairMakeType.CharaMakeStruct[0].SubMenuParam
                .Select(rowId => _excelService.CreateRowRef<CharaMakeCustomize>(rowId))
                .Where(rowRef => rowRef.RowId != 0 && rowRef.IsValid)
                .TryGetFirst(h => h.Value.HintItem.RowId == item.ItemId, out var itemRow))
            {
                return 0;
            }

            return itemRow.IsValid ? itemRow.Value.Icon : 0;
        });
    }

    public unsafe bool IsUnlocked(ItemHandle item)
    {
        if (!item.TryGetItem(out var itemRow))
            return false;

        // just to avoid the ExdModule.GetItemRowById call...
        switch (item.ItemActionType)
        {
            case ItemActionType.None:
                return false;

            case ItemActionType.Companion:
                return UIState.Instance()->IsCompanionUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.BuddyEquip:
                return UIState.Instance()->Buddy.CompanionInfo.IsBuddyEquipUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.Mount:
                return PlayerState.Instance()->IsMountUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.SecretRecipeBook:
                return PlayerState.Instance()->IsSecretRecipeBookUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.UnlockLink:
            case ItemActionType.OccultRecords:
                return UIState.Instance()->IsUnlockLinkUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.TripleTriadCard when itemRow.AdditionalData.Is<TripleTriadCard>():
                return UIState.Instance()->IsTripleTriadCardUnlocked((ushort)itemRow.AdditionalData.RowId);

            case ItemActionType.FolkloreTome:
                return PlayerState.Instance()->IsFolkloreBookUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.OrchestrionRoll when itemRow.AdditionalData.Is<Orchestrion>():
                return PlayerState.Instance()->IsOrchestrionRollUnlocked(itemRow.AdditionalData.RowId);

            case ItemActionType.FramersKit:
                return PlayerState.Instance()->IsFramersKitUnlocked(itemRow.AdditionalData.RowId);

            case ItemActionType.Ornament:
                return PlayerState.Instance()->IsOrnamentUnlocked(itemRow.ItemAction.Value.Data[0]);

            case ItemActionType.Glasses:
                return PlayerState.Instance()->IsGlassesUnlocked((ushort)itemRow.AdditionalData.RowId);

            case ItemActionType.SoulShards when PublicContentOccultCrescent.GetState() is var occultCrescentState && occultCrescentState != null:
                var supportJobId = (byte)itemRow.ItemAction.Value.Data[0];
                return supportJobId < occultCrescentState->SupportJobLevels.Length && occultCrescentState->SupportJobLevels[supportJobId] != 0;

            case ItemActionType.CompanySealVouchers:
                return false;
        }

        var row = ExdModule.GetItemRowById(itemRow.RowId);
        return row != null && UIState.Instance()->IsItemActionUnlocked(row) == 1;
    }

    public unsafe bool CanTryOn(ItemHandle item)
    {
        // not equippable, Waist or SoulCrystal => false
        if (item.EquipSlotCategory is 0 or 6 or 17)
            return false;

        // any OffHand that's not a Shield => false
        if (item.EquipSlotCategory is 2 && item.ItemFilterGroup != ItemFilterGroup.Shield)
            return false;

        var playerState = PlayerState.Instance();
        if (!playerState->IsLoaded)
            return false;

        var race = playerState->Race;
        if (race == 0)
            return false;

        if (ExcelService.Instance is not { } excelService || !excelService.TryGetRow<CustomEquipRaceCategory>(item.EquipRestriction, out var equipRaceCategoryRow))
            return false;

        if (!equipRaceCategoryRow.Races[race - 1])
            return false;

        return playerState->Sex switch
        {
            1 => equipRaceCategoryRow.Female,
            _ => equipRaceCategoryRow.Male,
        };
    }

    public unsafe bool CanEquip(ItemHandle item, out uint errorLogMessage)
    {
        var playerState = PlayerState.Instance();
        if (!playerState->IsLoaded)
        {
            errorLogMessage = 0;
            return false;
        }

        if (playerState->Race == 0)
        {
            errorLogMessage = 704; // "Only equippable by certain races."
            return false;
        }

        return CanEquip(
            item,
            playerState->Race,
            playerState->Sex,
            playerState->CurrentClassJobId,
            playerState->CurrentLevel,
            playerState->GrandCompany,
            playerState->GrandCompany switch
            {
                1 => playerState->GCRankMaelstrom,
                2 => playerState->GCRankTwinAdders,
                3 => playerState->GCRankImmortalFlames,
                _ => 0
            }, out errorLogMessage);
    }

    // E8 ?? ?? ?? ?? 85 C0 75 ?? 80 7E
    public bool CanEquip(ItemHandle item, byte race, byte sex, byte classJob, short level, byte grandCompany, byte pvpRank, out uint errorLogMessage)
    {
        var entry = _itemCache.GetOrAdd(item, _ => new ItemCacheEntry());
        var key = (race, sex, classJob, level, grandCompany, pvpRank);
        if (entry.CanEquipCache.TryGetValue(key, out var cachedEntry))
        {
            errorLogMessage = cachedEntry.Item2;
            return cachedEntry.Item1;
        }

        if (race == 0)
        {
            errorLogMessage = 704; // "Only equippable by certain races."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (!item.TryGetItem(out var itemRow) || !_excelService.TryGetRow<CustomEquipRaceCategory>(itemRow.EquipRestriction, out var equipRaceCategoryRow))
        {
            errorLogMessage = 716; // "Unable to move item. Please try again. (Reading data...)" ????
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (itemRow.EquipSlotCategory.RowId == 0)
        {
            errorLogMessage = 713; // "Unable to equip <ennoun(Item,2,lnum1,1,1)>."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (!equipRaceCategoryRow.Races[race - 1])
        {
            errorLogMessage = 704; // "Only equippable by certain races."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (sex == 0 && !equipRaceCategoryRow.Male)
        {
            errorLogMessage = 706; // "Only equippable by females."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (sex == 1 && !equipRaceCategoryRow.Female)
        {
            errorLogMessage = 705; // "Only equippable by males."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (itemRow.GrandCompany.RowId > 0 && itemRow.GrandCompany.RowId != grandCompany)
        {
            errorLogMessage = 752; // "Unable to equip this Grand Company's gear."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (!_excelService.TryGetRow<CustomClassJobCategory>(itemRow.ClassJobCategory.RowId, out var classJobCategory) || !classJobCategory.ClassesJobs[classJob])
        {
            errorLogMessage = 703; // "Cannot equip as current class."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (level < itemRow.LevelEquip)
        {
            errorLogMessage = 707; // "Not high enough level to equip."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        if (pvpRank < itemRow.RequiredPvpRank)
        {
            errorLogMessage = 662; // "Unable to equip at current PvP rank."
            entry.CanEquipCache[key] = (false, errorLogMessage);
            return false;
        }

        errorLogMessage = 0;
        entry.CanEquipCache[key] = (true, errorLogMessage);
        return true;
    }

    public unsafe Color GetItemLevelColor(ItemHandle item, byte classJob, params Color[] colors)
    {
        if (colors.Length < 2)
            throw new ArgumentException("At least two colors are required for interpolation.");

        if (!item.TryGetItem(out var itemRow))
            return Color.White;

        if (!_excelService.TryGetRow<ClassJob>(classJob, out var classJobRow))
            return Color.White;

        var expArrayIndex = classJobRow.ExpArrayIndex;
        if (expArrayIndex == -1)
            return Color.White;

        var level = PlayerState.Instance()->ClassJobLevels[expArrayIndex];
        if (level < 1 || !GetMaxLevelRanges().TryGetValue(level, out var range))
            return Color.White;

        var itemLevel = item.ItemLevel;

        // special case for Fisher's Secondary Tool
        // which has only one item, Spearfishing Gig
        if (itemRow.ItemUICategory.RowId == 99)
            return itemLevel == 180 ? Color.Green : Color.Red;

        if (itemLevel < range.Min)
            return Color.Red;

        var value = (itemLevel - range.Min) / (float)(range.Max - range.Min);

        var startIndex = (int)(value * (colors.Length - 1));
        var endIndex = Math.Min(startIndex + 1, colors.Length - 1);

        if (startIndex < 0 || startIndex >= colors.Length || endIndex < 0 || endIndex >= colors.Length)
            return Color.White;

        var t = value * (colors.Length - 1) - startIndex;
        return Color.FromVector4(Vector4.Lerp(colors[startIndex], colors[endIndex], t));
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

    private class ItemCacheEntry
    {
        public bool? IsCraftable;
        public bool? IsGatherable;
        public bool? IsFish;
        public bool? IsSpearfish;

        public uint? IconId;

        public ConcurrentDictionary<ValueTuple<ClientLanguage, bool>, ReadOnlySeString> ItemNames = []; // Key: (Language, IncludeIcon)
        public ConcurrentDictionary<ClientLanguage, ReadOnlySeString> ItemDescriptions = []; // Key: (Language, IncludeIcon)
        public ConcurrentDictionary<ClientLanguage, ReadOnlySeString> ItemLinks = [];
        public ConcurrentDictionary<ValueTuple<byte, byte>, uint> HairStyleIcons = []; // Key: (Tribe, Sex)
        public ConcurrentDictionary<ValueTuple<byte, byte, byte, short, byte, byte>, (bool, uint)> CanEquipCache = []; // Key: (Race, Sex, ClassJob, Level, GrandCompany, PvpRank), Value: (CanEquip, ErrorLogMessage)

        public IReadOnlyList<GatheringPoint>? GatheringPoints;
        public IReadOnlyList<GatheringPoint>? SpearfishingPoints;
        public IReadOnlyList<Recipe>? Recipes;
        public IReadOnlyList<ItemAmount>? Ingredients;
        public IReadOnlyList<GatheringItem>? GatheringItems;
        public IReadOnlyList<FishingSpot>? FishingSpots;
    }
}
