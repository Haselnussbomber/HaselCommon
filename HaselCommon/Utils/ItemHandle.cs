using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Game.Enums;

namespace HaselCommon.Utils;

public record struct ItemHandle
{
    // backing fields for properties
    private ItemLocation? _itemLocation;
    private uint? _itemId;

    // cache
    private Item? _itemRow;
    private EventItem? _eventItemRow;

    public ItemHandle()
    {

    }

    public ItemHandle(uint itemId)
    {
        _itemId = itemId;
    }

    public ItemHandle(ItemLocation itemLocation)
    {
        _itemLocation = itemLocation;
    }

    public unsafe uint ItemId
    {
        get
        {
            if (_itemId is { } itemId)
                return itemId;

            if (ItemLocation is { } location)
            {
                var inventoryItem = location.GetInventoryItem();
                if (inventoryItem != null)
                    return _itemId ??= inventoryItem->GetItemId();
            }

            return default;
        }

        set
        {
            Clear();
            _itemId = value;
        }
    }

    public ItemLocation? ItemLocation
    {
        get => _itemLocation;
        set
        {
            Clear();
            _itemLocation = value;
        }
    }

    public bool IsEmpty => (!_itemId.HasValue || _itemId.Value == 0) && (!_itemLocation.HasValue || _itemLocation.Value.IsEmpty);

    public uint BaseItemId => ItemUtil.GetBaseId(ItemId).ItemId;

    public ItemKind ItemKind => ItemUtil.GetBaseId(ItemId).Kind;

    public bool IsNormalItem => ItemUtil.IsNormalItem(ItemId);

    public bool IsCollectible => ItemUtil.IsCollectible(ItemId);

    public bool IsHighQuality => ItemUtil.IsHighQuality(ItemId);

    public bool IsEventItem => ItemUtil.IsEventItem(ItemId);

    public Item? ItemRow => TryGetItem(out var item) ? item : null;

    public EventItem? EventItemRow => TryGetEventItem(out var item) ? item : null;

    public uint Icon => ItemService.Instance?.GetItemIcon(this) ?? default;

    public ReadOnlySeString Name => GetItemName();

    public ReadOnlySeString Description => GetItemDescription();

    public ReadOnlySeString Link => GetItemLink();

    public uint RarityColorType => GetItemRarityColorType(false);

    public uint RarityEdgeColorType => GetItemRarityColorType(true);

    public Color RarityColor => GetItemRarityColor(false);

    public Color RarityEdgeColor => GetItemRarityColor(true);

    #region Item API

    public uint ItemLevel => ItemRow?.LevelItem.RowId ?? 0;

    public ItemActionType ItemActionType => (ItemActionType)(ItemRow?.ItemAction.Value.Type ?? 0);

    public ItemFilterGroup ItemFilterGroup => (ItemFilterGroup)(ItemRow?.FilterGroup ?? 0);

    public uint EquipSlotCategory => ItemRow?.EquipSlotCategory.RowId ?? 0;

    public uint EquipRestriction => ItemRow?.EquipRestriction ?? 0;

    public bool IsCraftable => ItemService.Instance?.IsCraftable(this) ?? default;

    public bool IsCrystal => ItemFilterGroup == ItemFilterGroup.Crystal;

    public bool IsCurrency => ItemFilterGroup == ItemFilterGroup.Currency;

    public bool IsFish => ItemService.Instance?.IsFish(this) ?? default;

    public bool IsGatherable => ItemService.Instance?.IsGatherable(this) ?? default;

    public bool IsSpearfish => ItemService.Instance?.IsSpearfish(this) ?? default;

    public bool IsUnlockable => ItemActionType is
        ItemActionType.Companion
        or ItemActionType.BuddyEquip
        or ItemActionType.Mount
        or ItemActionType.SecretRecipeBook
        or ItemActionType.UnlockLink
        or ItemActionType.TripleTriadCard
        or ItemActionType.FolkloreTome
        or ItemActionType.OrchestrionRoll
        or ItemActionType.FramersKit
        or ItemActionType.Ornament
        or ItemActionType.Glasses
        or ItemActionType.OccultRecords
        or ItemActionType.SoulShards;

    public bool IsUnlocked => TryGetItem(out var itemRow) && ServiceLocator.TryGetService<IUnlockState>(out var unlockState) && unlockState.IsItemUnlocked(itemRow);

    public bool CanTryOn => ItemService.Instance?.CanTryOn(this) ?? default;

    public bool CanEquip(out uint errorLogMessage)
    {
        errorLogMessage = 0;
        return ItemService.Instance?.CanEquip(this, out errorLogMessage) ?? default;
    }

    public bool CanEquip(byte race, byte sex, byte classJob, short level, byte grandCompany, byte pvpRank, out uint errorLogMessage)
    {
        errorLogMessage = 0;
        return ItemService.Instance?.CanEquip(this, race, sex, classJob, level, grandCompany, pvpRank, out errorLogMessage) ?? default;
    }

    #endregion

    public bool TryGetItem(out Item item)
    {
        return TryGetItem(null, out item);
    }

    public bool TryGetItem(ClientLanguage? language, out Item item)
    {
        if (_itemRow is { } itemRow)
        {
            item = itemRow;
            return true;
        }

        if (IsEmpty || IsEventItem || ExcelService.Instance is not { } excelService || !excelService.TryGetRow(BaseItemId, language, out item))
        {
            item = default;
            return false;
        }

        _itemRow = item;
        return true;
    }

    public bool TryGetEventItem(out EventItem eventItem)
    {
        return TryGetEventItem(null, out eventItem);
    }

    public bool TryGetEventItem(ClientLanguage? language, out EventItem eventItem)
    {
        if (_eventItemRow is { } eventItemRow)
        {
            eventItem = eventItemRow;
            return true;
        }

        if (IsEmpty || !IsEventItem || ExcelService.Instance is not { } excelService || !excelService.TryGetRow(ItemId, language, out eventItem))
        {
            eventItem = default;
            return false;
        }

        _eventItemRow = eventItem;
        return true;
    }

    public ReadOnlySeString GetItemName(ClientLanguage? language = null)
    {
        return GetItemName(false, language);
    }

    public ReadOnlySeString GetItemName(bool includeIcon, ClientLanguage? language = null)
    {
        return ItemService.Instance?.GetItemName(this, includeIcon, language) ?? default;
    }

    public ReadOnlySeString GetItemDescription(ClientLanguage? language = null)
    {
        return ItemService.Instance?.GetItemDescription(this, language) ?? default;
    }

    public ReadOnlySeString GetItemLink(ClientLanguage? language = null)
    {
        return ItemService.Instance?.GetItemLink(this, language) ?? default;
    }

    public uint GetItemRarityColorType(bool isEdgeColor = false)
    {
        return ItemUtil.GetItemRarityColorType(ItemId, isEdgeColor);
    }

    public Color GetItemRarityColor(bool isEdgeColor = false)
    {
        var colorType = GetItemRarityColorType(isEdgeColor);
        return ExcelService.Instance?.TryGetRow<UIColor>(colorType, out var uiColor) == true ? Color.FromABGR(uiColor.Dark) : Color.White;
    }

    public void Clear()
    {
        _itemLocation = null;
        _itemId = null;
    }

    public override string ToString()
    {
        var name = GetItemName(true);
        return name.IsEmpty ? $"{nameof(ItemHandle)}#{ItemId}" : name.ToString();
    }

    public static unsafe implicit operator ItemHandle(InventoryItem* item) => new(item);

    public static implicit operator ItemHandle(Item item) => new(item.RowId);

    public static implicit operator ItemHandle(RowRef<Item> rowRef) => new(rowRef.RowId);

    public static implicit operator ItemHandle(EventItem eventItem) => new(eventItem.RowId);

    public static implicit operator ItemHandle(RowRef<EventItem> rowRef) => new(rowRef.RowId);

    public static implicit operator ItemHandle(ItemLocation itemLocation) => new(itemLocation);

    public static implicit operator ItemHandle(uint itemId) => new(itemId);

    public static implicit operator uint(ItemHandle itemInfo) => itemInfo.ItemId;
}
