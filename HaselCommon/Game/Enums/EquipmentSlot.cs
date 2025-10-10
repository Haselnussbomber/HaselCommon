using FFXIVClientStructs.FFXIV.Client.Game;

namespace HaselCommon.Game.Enums;

/// <summary>
/// An enum representing the slots in <see cref="InventoryType.EquippedItems"/>.
/// </summary>
public enum EquipmentSlot : ushort
{
    MainHand,
    OffHand,
    Head,
    Body,
    Hands,
    Waist,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrists,
    RightRing,
    LeftRing,
    SoulCrystal,
}
