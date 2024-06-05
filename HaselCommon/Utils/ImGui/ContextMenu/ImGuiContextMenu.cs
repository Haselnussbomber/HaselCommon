using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Records;
using HaselCommon.Sheets;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using TerritoryType = Lumina.Excel.GeneratedSheets.TerritoryType;

namespace HaselCommon.Utils;

public static unsafe class ImGuiContextMenu
{
    public static void Draw(string key, IEnumerable<IImGuiContextMenuEntry> entries)
    {
        using var popup = ImRaii.ContextPopupItem(key);
        if (!popup.Success)
            return;

        var visibleEntries = entries.Where(entry => entry.Visible);
        var count = visibleEntries.Count();
        var i = 0;
        foreach (var entry in visibleEntries)
            entry.Draw(new IterationArgs(i++, count));
    }

    public static ImGuiContextMenuSeparator CreateSeparator()
        => new();

    public static ImGuiContextMenuEntry CreateTryOn(ExtendedItem item, uint glamourItemId = 0, byte stainId = 0)
        => new()
        {
            Visible = item.CanTryOn,
            Label = GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                    AgentTryon.TryOn(0, item.RowId, stainId, 0, 0);
                else
                    AgentTryon.TryOn(0, item.RowId, stainId, glamourItemId, stainId);
            }
        };

    public static ImGuiContextMenuEntry CreateItemFinder(uint itemId)
        => new()
        {
            Label = GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemFinderModule.Instance()->SearchForItem(itemId);
            }
        };

    public static ImGuiContextMenuEntry CreateCopyItemName(uint itemId)
        => new()
        {
            Label = GetAddonText(159), // "Copy Item Name"
            ClickCallback = () =>
            {
                ImGui.SetClipboardText(GetItemName(itemId));
            }
        };

    public static ImGuiContextMenuEntry CreateItemSearch(ExtendedItem item)
        => new()
        {
            Visible = item.CanSearchForItem,
            Label = t("ItemContextMenu.SearchTheMarkets"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemSearchUtils.Search(item);
            }
        };

    public static ImGuiContextMenuEntry CreateGearsetLinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink == 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = GetAddonText(4394),
            LoseFocusOnClick = true,
            ClickCallback = () => GetAgent<AgentMiragePrismMiragePlate>()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        };

    public static ImGuiContextMenuEntry CreateGearsetChangeGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = GetAddonText(4395),
            ClickCallback = () => GetAgent<AgentMiragePrismMiragePlate>()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        };

    public static ImGuiContextMenuEntry CreateGearsetUnlinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = GetAddonText(4396),
            ClickCallback = () => RaptureGearsetModule.Instance()->LinkGlamourPlate(gearset->Id, 0)
        };

    public static ImGuiContextMenuEntry CreateGearsetChangePortrait(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null,
            Label = GetAddonText(4411),
            ClickCallback = () =>
            {
                GetAgent<AgentBannerEditor>()->AgentInterface.Hide();
                GetAgent<AgentBannerEditor>()->OpenForGearset(gearset->Id);
            }
        };

    public static ImGuiContextMenuEntry CreateSearchCraftingMethod(ExtendedItem item)
        => new()
        {
            Visible = item.IsCraftable,
            Label = GetAddonText(1414), // "Search for Item by Crafting Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentRecipeNote.Instance()->OpenRecipeByItemId(item.RowId);
            }
        };

    public static ImGuiContextMenuEntry CreateOpenMapForGatheringPoint(ExtendedItem item, TerritoryType? territoryType, ReadOnlySeString? prefix = null)
        => new()
        {
            Visible = territoryType != null && item.IsGatherable,
            Label = GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var point = item.GatheringPoints.First(point => point.TerritoryType.Row == territoryType!.RowId);
                point.OpenMap(item, prefix);
            }
        };

    public static ImGuiContextMenuEntry CreateOpenMapForFishingSpot(ExtendedItem item, ReadOnlySeString? prefix = null)
        => new()
        {
            Visible = item.IsFish,
            Label = GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                item.FishingSpots.First().OpenMap(item, prefix);
            }
        };

    public static ImGuiContextMenuEntry CreateSearchGatheringMethod(ExtendedItem item)
        => new()
        {
            Visible = item.IsGatherable,
            Label = GetAddonText(1472), // "Search for Item by Gathering Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentGatheringNote.Instance()->OpenGatherableByItemId((ushort)item.RowId);
            }
        };

    public static ImGuiContextMenuEntry CreateOpenInFishGuide(ExtendedItem item)
        => new()
        {
            Visible = item.IsFish,
            Label = t("ItemContextMenu.OpenInFishGuide"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var agent = (AgentFishGuide*)AgentModule.Instance()->GetAgentByInternalId(AgentId.FishGuide);
                agent->OpenForItemId(item.RowId, item!.IsSpearfishing);
            }
        };

    public static ImGuiContextMenuEntry CreateOpenOnGarlandTools(string type, uint id)
        => new()
        {
            Label = t("ItemContextMenu.OpenOnGarlandTools"),
            ClickCallback = () =>
            {
                Task.Run(() => Util.OpenLink($"https://www.garlandtools.org/db/#{type}/{id}"));
            },
            HoverCallback = () =>
            {
                using var tooltip = ImRaii.Tooltip();

                var pos = ImGui.GetCursorPos();
                ImGui.GetWindowDrawList().AddText(
                    UiBuilder.IconFont, 12 * ImGuiHelpers.GlobalScale,
                    ImGui.GetWindowPos() + pos + new Vector2(2),
                    Colors.Grey,
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0) * ImGuiHelpers.GlobalScale);
                ImGuiUtils.TextUnformattedColored(Colors.Grey, $"https://www.garlandtools.org/db/#{type}/{id}");
            }
        };
}
