using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AgentItemSearchExtensions
{
    public static bool CanSearchForItem(ref this AgentItemSearch agent, ItemHandle item)
    {
        return agent.IsAgentActive()
            && !item.IsEventItem
            && item.TryGetItem(out var itemRow)
            && !itemRow.IsUntradable
            && !itemRow.IsCollectable
            && IsAddonOpen(AgentId.ItemSearch);
    }

    public static void SearchForItem(ref this AgentItemSearch agent, ItemHandle item)
    {
        if (!agent.CanSearchForItem(item))
            return;

        if (!ServiceLocator.TryGetService<IClientState>(out var clientState))
            return;

        if (!TryGetAddon<AddonItemSearch>((ushort)agent.GetAddonId(), out var addon))
            return;

        if (TryGetAddon<AtkUnitBase>("ItemSearchResult"u8, out var itemSearchResult))
            itemSearchResult->Hide2();

        var itemName = item.GetItemName(clientState.ClientLanguage).ToString();
        if (itemName.Length > 40)
            itemName = itemName[..40];

        addon->SearchTextInput->SetText(itemName);

        addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
        addon->RunSearch(false);
    }
}
