using FFXIVClientStructs.FFXIV.Client.System.Framework;
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
            return agent.IsAgentActive()
                && !item.IsEventItem
                && item.TryGetItem(out var itemRow)
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

            if (TryGetAddon<AtkUnitBase>("ItemSearchResult"u8, out var itemSearchResult))
                itemSearchResult->Hide2();

            var clientLanguage = (ClientLanguage)Framework.Instance()->ClientLanguage;
            var itemName = item.GetItemName(clientLanguage).ToString();
            if (itemName.Length > 40)
                itemName = itemName[..40];

            addon->SearchTextInput->SetText(itemName);

            addon->SetModeFilter(AddonItemSearch.SearchMode.Normal, -1);
            addon->RunSearch(false);
        }
    }
}
