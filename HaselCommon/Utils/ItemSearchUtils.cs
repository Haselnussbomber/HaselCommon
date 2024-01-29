using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using HaselCommon.Sheets;
using HaselCommon.Structs.Internal;

namespace HaselCommon.Utils;

public static unsafe class ItemSearchUtils
{
    public static void Search(ExtendedItem item)
    {
        if (!item.CanSearchForItem)
            return;

        if (!TryGetAddon<AddonItemSearch>(AgentId.ItemSearch, out var addon))
            return;

        if (TryGetAddon<AtkUnitBase>("ItemSearchResult", out var itemSearchResult))
            itemSearchResult->Hide2();

        var itemName = GetItemName(item.RowId);
        if (itemName.Length > 40)
            itemName = itemName[..40];

        ((HAtkComponentTextInput*)addon->TextInput)->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }
}
