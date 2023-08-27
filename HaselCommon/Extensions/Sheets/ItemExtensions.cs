using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Enums;
using HaselCommon.Structs.Internal;
using HaselCommon.Utils;
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

    public static bool IsUnlockable(this Item item)
    {
        return item.ItemAction.Row != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == item.ItemAction.Value!.Type);
    }

    public static unsafe bool HasAcquired(this Item item)
    {
        var action = item.ItemAction.Value;
        if (action == null)
        {
            return false;
        }

        var type = (ItemActionType)action.Type;
        if (type == ItemActionType.Cards)
        {
            var cardId = item.AdditionalData;
            var card = Service.DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
            return card != null && UIState.Instance()->IsTripleTriadCardUnlocked((ushort)card.RowId);
        }

        using var data = new DisposableStruct<ItemActionUnlockedData>();
        data.Ptr->ItemActionId = action.RowId;

        if (type == ItemActionType.OrchestrionRolls || type == ItemActionType.FramersKits)
        {
            data.Ptr->ItemActionAdditionalData = item.AdditionalData;
        }

        return HasItemActionUnlocked.Value.Invoke(data);
    }

    internal unsafe delegate bool HasItemActionUnlockedDelegate(ItemActionUnlockedData* data, nint a2 = 0, nint a3 = 0);
    internal static readonly Lazy<HasItemActionUnlockedDelegate> HasItemActionUnlocked
        = new(() => MemoryUtils.GetDelegateForSignature<HasItemActionUnlockedDelegate>("E8 ?? ?? ?? ?? 84 C0 75 A6 32 C0"));
}
