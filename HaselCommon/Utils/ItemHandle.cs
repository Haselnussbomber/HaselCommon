using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Game.Enums;

namespace HaselCommon.Utils;

public readonly record struct ItemHandle
{
    public ItemHandle(uint itemId)
    {
        ItemId = itemId;
    }

    public ItemHandle(ItemLocation itemLocation)
    {
        ItemLocation = itemLocation;
        unsafe
        {
            var inventoryItem = itemLocation.GetInventoryItem();
            ItemId = inventoryItem != null ? inventoryItem->GetItemId() : 0;
        }
    }

    public uint ItemId { get; }

    public ItemLocation? ItemLocation { get; }

    public bool IsEmpty => ItemId == 0;

    public uint BaseItemId => ItemUtil.GetBaseId(ItemId).ItemId;

    public ItemKind ItemKind => ItemUtil.GetBaseId(ItemId).Kind;

    public bool IsNormalItem => ItemUtil.IsNormalItem(ItemId);

    public bool IsCollectible => ItemUtil.IsCollectible(ItemId);

    public bool IsHighQuality => ItemUtil.IsHighQuality(ItemId);

    public bool IsEventItem => ItemUtil.IsEventItem(ItemId);

    public uint Icon => ServiceLocator.GetService<ItemService>()?.GetItemIcon(this) ?? 0;

    public ReadOnlySeString Name => GetItemName();

    public ReadOnlySeString Description => GetItemDescription();

    public ReadOnlySeString Link => GetItemLink();

    public uint RarityColorType => GetItemRarityColorType(false);

    public uint RarityEdgeColorType => GetItemRarityColorType(true);

    public Color RarityColor => GetItemRarityColor(false);

    public Color RarityEdgeColor => GetItemRarityColor(true);

    #region Item API

    public uint ItemLevel => TryGetItem(out var itemRow) ? itemRow.LevelItem.RowId : 0;

    public ItemActionType ItemActionType => (ItemActionType)(TryGetItem(out var itemRow) ? itemRow.ItemAction.Value.Action.RowId : 0);

    public ItemFilterGroup ItemFilterGroup => (ItemFilterGroup)(TryGetItem(out var itemRow) ? itemRow.FilterGroup : 0);

    public uint EquipSlotCategory => TryGetItem(out var itemRow) ? itemRow.EquipSlotCategory.RowId : 0;

    public uint EquipRestriction => TryGetItem(out var itemRow) ? itemRow.EquipRestriction : 0u;

    public bool IsCraftable => ServiceLocator.GetService<ItemService>()?.IsCraftable(this) ?? false;

    public bool IsCrystal => ItemFilterGroup == ItemFilterGroup.Crystal;

    public bool IsCurrency => ItemFilterGroup == ItemFilterGroup.Currency;

    public bool IsFish => ServiceLocator.GetService<ItemService>()?.IsFish(this) ?? false;

    public bool IsGatherable => ServiceLocator.GetService<ItemService>()?.IsGatherable(this) ?? false;

    public bool IsSpearfish => ServiceLocator.GetService<ItemService>()?.IsSpearfish(this) ?? false;

    public bool IsUnlockable => ItemActionType.IsUnlockable;

    public bool IsUnlocked => TryGetItem(out var itemRow) && (ServiceLocator.GetService<IUnlockState>()?.IsItemUnlocked(itemRow) ?? false);

    public bool CanTryOn => ServiceLocator.GetService<ItemService>()?.CanTryOn(this) ?? false;

    public bool CanEquip(out uint errorLogMessage)
    {
        if (!ServiceLocator.TryGetService<ItemService>(out var itemService))
        {
            errorLogMessage = 0;
            return false;
        }

        return itemService.CanEquip(this, out errorLogMessage);
    }

    public bool CanEquip(byte race, byte sex, byte classJob, short level, byte grandCompany, byte pvpRank, out uint errorLogMessage)
    {
        if (!ServiceLocator.TryGetService<ItemService>(out var itemService))
        {
            errorLogMessage = 0;
            return false;
        }

        return itemService.CanEquip(this, race, sex, classJob, level, grandCompany, pvpRank, out errorLogMessage);
    }

    #endregion

    public bool TryGetItem(out Item item, ClientLanguage? language = null)
    {
        if (IsEmpty || IsEventItem)
        {
            item = default;
            return false;
        }

        if (!ServiceLocator.TryGetService<ExcelService>(out var excelService))
        {
            item = default;
            return false;
        }

        return excelService.TryGetRow(BaseItemId, language, out item);
    }

    public bool TryGetEventItem(out EventItem eventItem, ClientLanguage? language = null)
    {
        if (IsEmpty || !IsEventItem)
        {
            eventItem = default;
            return false;
        }

        if (!ServiceLocator.TryGetService<ExcelService>(out var excelService))
        {
            eventItem = default;
            return false;
        }

        return excelService.TryGetRow(ItemId, language, out eventItem);
    }

    public ReadOnlySeString GetItemName(ClientLanguage? language = null, bool includeIcon = false)
        => ServiceLocator.GetService<ItemService>()?.GetItemName(this, includeIcon, language) ?? default;

    public ReadOnlySeString GetItemDescription(ClientLanguage? language = null)
        => ServiceLocator.GetService<ItemService>()?.GetItemDescription(this, language) ?? default;

    public ReadOnlySeString GetItemLink(ClientLanguage? language = null)
        => ServiceLocator.GetService<ItemService>()?.GetItemLink(this, language) ?? default;

    public uint GetItemRarityColorType(bool isEdgeColor = false)
        => ItemUtil.GetItemRarityColorType(ItemId, isEdgeColor);

    public Color GetItemRarityColor(bool isEdgeColor = false)
    {
        if (!ServiceLocator.TryGetService<ExcelService>(out var excelService))
            return Color.White;

        if (!excelService.TryGetRow<UIColor>(GetItemRarityColorType(isEdgeColor), out var uiColor))
            return Color.White;

        return Color.FromABGR(uiColor.Dark);
    }

    public override string ToString()
    {
        if (IsEmpty)
            return $"{nameof(ItemHandle)}#Empty";

        var name = GetItemName(null, true);
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
