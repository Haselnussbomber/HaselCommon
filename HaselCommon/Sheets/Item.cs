using System.Linq;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Enums;
using HaselCommon.Structs.Internal;
using HaselCommon.Utils;
using Lumina.Excel.GeneratedSheets;
using Recipe = Lumina.Excel.GeneratedSheets.Recipe;
using SpearfishingItem = Lumina.Excel.GeneratedSheets.SpearfishingItem;

namespace HaselCommon.Sheets;

public class Item : Lumina.Excel.GeneratedSheets.Item
{
    private string? _name { get; set; } = null;
    private Recipe? _recipe { get; set; } = null;
    private bool? _isCraftable { get; set; } = null;
    private bool? _isFish { get; set; } = null;
    private bool? _isSpearfishing { get; set; } = null;
    private bool? _isUnlockable { get; set; } = null;
    private GatheringItem[]? _gatheringItems { get; set; } = null;
    private FishingSpot[]? _fishingSpots { get; set; } = null;

    public new string Name
        => _name ??= base.Name.ToDalamudString().ToString();

    public Recipe? Recipe
        => _recipe ??= FindRow<Recipe>(recipe => recipe?.ItemResult.Row == RowId);

    public GatheringItem[] GatheringItems
        => _gatheringItems ??= GetSheet<GatheringItem>().Where((row) => row.Item == RowId).ToArray();

    public GatheringPoint[] GatheringPoints
        => GatheringItems
            .SelectMany(row => row.GatheringPoints)
            .ToArray();

    public FishingSpot[] FishingSpots
        => _fishingSpots ??= FishingSpot.FindByItemId(RowId);

    public bool IsCraftable
        => _isCraftable ??= Recipe != null;

    public bool IsCrystal
        => ItemUICategory.Row == 59;

    public bool IsGatherable
        => GatheringPoints.Any();

    public bool IsFish
        => _isFish ??= FishingSpots.Any();

    public bool IsSpearfishing
        => _isSpearfishing ??= GetSheet<SpearfishingItem>().Any(row => row.Item.Row == RowId);

    public bool IsUnlockable
        => _isUnlockable ??= ItemAction.Row != 0 && Enum.GetValues<ItemActionType>().Any(type => (ushort)type == ItemAction.Value!.Type);

    public bool CanTryOn
        => EquipSlotCategory.Row switch
        {
            2 when FilterGroup != 3 => false, // any OffHand that's not a Shield
            6 => false, // Waist
            17 => false, // SoulCrystal
            _ => true
        };

    public bool CanSearchForItem
        => !IsUntradable && !IsCollectable && IsAddonOpen(AgentId.ItemSearch);

    public unsafe bool HasAcquired
    {
        get
        {
            var action = ItemAction.Value;
            if (action == null)
            {
                return false;
            }

            var type = (ItemActionType)action.Type;
            if (type == ItemActionType.Cards)
            {
                var cardId = AdditionalData;
                var card = Service.DataManager.GetExcelSheet<TripleTriadCard>()!.GetRow(cardId);
                return card != null && UIState.Instance()->IsTripleTriadCardUnlocked((ushort)card.RowId);
            }

            using var data = new DisposableStruct<ItemActionUnlockedData>();
            data.Ptr->ItemActionId = action.RowId;

            if (type == ItemActionType.OrchestrionRolls || type == ItemActionType.FramersKits)
            {
                data.Ptr->ItemActionAdditionalData = AdditionalData;
            }

            return HasItemActionUnlocked.Value.Invoke(data);
        }
    }

    internal unsafe delegate bool HasItemActionUnlockedDelegate(ItemActionUnlockedData* data, nint a2 = 0, nint a3 = 0);
    internal static readonly Lazy<HasItemActionUnlockedDelegate> HasItemActionUnlocked
        = new(() => MemoryUtils.GetDelegateForSignature<HasItemActionUnlockedDelegate>("E8 ?? ?? ?? ?? 84 C0 75 A6 32 C0"));
}
