using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Extensions.Sheets;

public static class ItemExtensions
{
    public static bool CanTryOn(this Item item)
    {
        // see "E8 ?? ?? ?? ?? 85 C0 48 8B 03"
        return item.EquipSlotCategory.Row switch
        {
            2 when item.FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        };
    }

    public static bool CanSearchForItem(this Item item)
    {
        return item.RowId != 0 && !item.IsUntradable && !item.IsCollectable && IsAddonOpen(AgentId.ItemSearch);
    }
}
