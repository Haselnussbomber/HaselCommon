using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Game.Enums;

namespace HaselCommon.Utils;

public record struct ItemLocation
{
    public InventoryType Container { get; set; } = InventoryType.Invalid;
    public ushort Slot { get; set; } = 0;

    public ItemLocation()
    {

    }

    public ItemLocation(InventoryType container, ushort slot)
    {
        Container = container;
        Slot = slot;
    }

    public unsafe ItemLocation(InventoryItem* item)
    {
        SetInventoryItem(item);
    }

    public ItemLocation(EquipmentSlot slot)
    {
        SetEquipmentSlot(slot);
    }

    public void Clear()
    {
        Container = InventoryType.Invalid;
        Slot = 0;
    }

    public void SetContainerAndSlot(InventoryType container, ushort slot)
    {
        Clear();
        Container = container;
        Slot = slot;
    }

    public unsafe void SetInventoryItem(InventoryItem* item)
    {
        if (item == null)
        {
            Clear();
        }
        else
        {
            Container = item->GetInventoryType();
            Slot = item->GetSlot();
        }
    }

    public unsafe InventoryItem* GetInventoryItem()
    {
        if (Container == InventoryType.Invalid)
            return null;

        return InventoryManager.Instance()->GetInventorySlot(Container, Slot);
    }

    public void SetEquipmentSlot(ushort slot)
    {
        Container = InventoryType.EquippedItems;
        Slot = slot;
    }

    public void SetEquipmentSlot(EquipmentSlot slot)
    {
        Container = InventoryType.EquippedItems;
        Slot = (ushort)slot;
    }

    public unsafe bool IsEmpty
    {
        get
        {
            if (Container != InventoryType.Invalid)
                return false;

            var inventoryItem = GetInventoryItem();
            return inventoryItem == null || inventoryItem->IsEmpty();
        }
    }

    public ValueTuple<InventoryType, ushort> AsTuple()
    {
        return (Container, Slot);
    }

    public static unsafe implicit operator ItemLocation(InventoryItem* item) => new(item);

    public static implicit operator ItemLocation((InventoryType Container, ushort Slot) tuple) => new(tuple.Container, tuple.Slot);

    public static implicit operator ItemLocation(EquipmentSlot slot) => new(slot);

    public static implicit operator ValueTuple<InventoryType, ushort>(ItemLocation itemLoc) => itemLoc.AsTuple();
}
