using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class AgentItemSearchExtensions
{
    extension(ref AgentItemSearch agent)
    {
        public bool CanSearchForItem(ItemHandle item)
        {
            var itemService = ServiceProvider.GetRequiredService<ItemService>();

            return agent.IsAgentActive()
                && !item.IsEventItem
                && itemService.TryGetItem(item, out var itemRow)
                && !itemRow.IsUntradable
                && !itemRow.IsCollectable
                && IsAddonOpen(AgentId.ItemSearch);
        }

        public void SearchForItem(ItemHandle item)
        {
            if (!agent.CanSearchForItem(item))
                return;

            if (!TryGetAddon<AddonItemSearch>((ushort)agent.GetAddonId(), out var addon))
                return;

            var clientState = ServiceProvider.GetRequiredService<IClientState>();
            var itemService = ServiceProvider.GetRequiredService<ItemService>();

            if (TryGetAddon<AtkUnitBase>("ItemSearchResult"u8, out var itemSearchResult))
                itemSearchResult->Hide2();

            var itemName = itemService.GetItemName(item, false, clientState.ClientLanguage).ToString();
            if (itemName.Length > 40)
                itemName = itemName[..40];

            addon->SearchTextInput->SetText(itemName);

            addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
            addon->RunSearch(false);
        }
    }
}
