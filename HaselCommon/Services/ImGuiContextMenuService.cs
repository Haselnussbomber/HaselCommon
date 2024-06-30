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
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Text.ReadOnly;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using TerritoryType = Lumina.Excel.GeneratedSheets.TerritoryType;

namespace HaselCommon.Services;

// TODO: convert to builder

public unsafe class ImGuiContextMenuService(TextService TextService)
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

    public ImGuiContextMenuEntry CreateTryOn(ExtendedItem item, uint glamourItemId = 0, byte stain1Id = 0, byte stain2Id = 0)
        => new()
        {
            Visible = item.CanTryOn,
            Label = TextService.GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                    AgentTryon.TryOn(0, item.RowId, stain1Id, stain2Id);
                else
                    AgentTryon.TryOn(0, item.RowId, stain1Id, stain2Id, glamourItemId);
            }
        };

    public ImGuiContextMenuEntry CreateItemFinder(uint itemId)
        => new()
        {
            Label = TextService.GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemFinderModule.Instance()->SearchForItem(itemId);
            }
        };

    public ImGuiContextMenuEntry CreateCopyItemName(uint itemId)
        => new()
        {
            Label = TextService.GetAddonText(159), // "Copy Item Name"
            ClickCallback = () =>
            {
                ImGui.SetClipboardText(TextService.GetItemName(itemId));
            }
        };

    public ImGuiContextMenuEntry CreateItemSearch(ExtendedItem item)
        => new()
        {
            Visible = item.CanSearchForItem,
            Label = TextService.Translate("ItemContextMenu.SearchTheMarkets"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemSearchUtils.Search(item);
            }
        };

    public ImGuiContextMenuEntry CreateGearsetLinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink == 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = TextService.GetAddonText(4394),
            LoseFocusOnClick = true,
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        };

    public ImGuiContextMenuEntry CreateGearsetChangeGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = TextService.GetAddonText(4395),
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        };

    public ImGuiContextMenuEntry CreateGearsetUnlinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = TextService.GetAddonText(4396),
            ClickCallback = () => RaptureGearsetModule.Instance()->LinkGlamourPlate(gearset->Id, 0)
        };

    public ImGuiContextMenuEntry CreateGearsetChangePortrait(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset != null,
            Label = TextService.GetAddonText(4411),
            ClickCallback = () =>
            {
                AgentBannerEditor.Instance()->AgentInterface.Hide();
                AgentBannerEditor.Instance()->OpenForGearset(gearset->Id);
            }
        };

    public ImGuiContextMenuEntry CreateSearchCraftingMethod(ExtendedItem item)
        => new()
        {
            Visible = item.IsCraftable,
            Label = TextService.GetAddonText(1414), // "Search for Item by Crafting Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentRecipeNote.Instance()->OpenRecipeByItemId(item.RowId);
            }
        };

    public ImGuiContextMenuEntry CreateOpenMapForGatheringPoint(ExtendedItem item, TerritoryType? territoryType, ReadOnlySeString? prefix = null)
        => new()
        {
            Visible = territoryType != null && item.IsGatherable,
            Label = TextService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var point = item.GatheringPoints.First(point => point.TerritoryType.Row == territoryType!.RowId);
                point.OpenMap(item, prefix);
            }
        };

    public ImGuiContextMenuEntry CreateOpenMapForFishingSpot(ExtendedItem item, ReadOnlySeString? prefix = null)
        => new()
        {
            Visible = item.IsFish,
            Label = TextService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                item.FishingSpots.First().OpenMap(item, prefix);
            }
        };

    public ImGuiContextMenuEntry CreateSearchGatheringMethod(ExtendedItem item)
        => new()
        {
            Visible = item.IsGatherable,
            Label = TextService.GetAddonText(1472), // "Search for Item by Gathering Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentGatheringNote.Instance()->OpenGatherableByItemId((ushort)item.RowId);
            }
        };

    public ImGuiContextMenuEntry CreateOpenInFishGuide(ExtendedItem item)
        => new()
        {
            Visible = item.IsFish,
            Label = TextService.Translate("ItemContextMenu.OpenInFishGuide"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var agent = (AgentFishGuide*)AgentModule.Instance()->GetAgentByInternalId(AgentId.FishGuide);
                agent->OpenForItemId(item.RowId, item!.IsSpearfishing);
            }
        };

    public ImGuiContextMenuEntry CreateOpenOnGarlandTools(string type, uint id)
        => new()
        {
            Label = TextService.Translate("ItemContextMenu.OpenOnGarlandTools"),
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

public interface IImGuiContextMenuEntry
{
    public bool Visible { get; set; }
    public bool Enabled { get; set; }
    public string Label { get; set; }
    public bool LoseFocusOnClick { get; set; }
    public Action? ClickCallback { get; set; }
    public Action? HoverCallback { get; set; }
    public void Draw(IterationArgs args);
}

public struct ImGuiContextMenuEntry : IImGuiContextMenuEntry
{
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool LoseFocusOnClick { get; set; } = false;
    public string Label { get; set; } = string.Empty;
    public Action? ClickCallback { get; set; } = null;
    public Action? HoverCallback { get; set; } = null;

    public ImGuiContextMenuEntry() { }

    public void Draw(IterationArgs args)
    {
        if (ImGui.MenuItem(Label, Enabled))
        {
            ClickCallback?.Invoke();

            if (LoseFocusOnClick)
                ImGui.SetWindowFocus(null);
        }
        if (ImGui.IsItemHovered())
            HoverCallback?.Invoke();
    }
}

public struct ImGuiContextMenuSeparator : IImGuiContextMenuEntry
{
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool LoseFocusOnClick { get; set; } = false;
    public string Label { get; set; } = string.Empty;
    public Action? ClickCallback { get; set; } = null;
    public Action? HoverCallback { get; set; } = null;

    public ImGuiContextMenuSeparator() { }

    public void Draw(IterationArgs args)
    {
        if (!args.IsFirst && !args.IsLast)
            ImGui.Separator();
    }
}
