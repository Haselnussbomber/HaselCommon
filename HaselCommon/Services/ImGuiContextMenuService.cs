using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using Lumina.Text.ReadOnly;
using Action = System.Action;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

namespace HaselCommon.Services;

[RegisterSingleton]
public class ImGuiContextMenuService(TextService textService, MapService mapService, ItemService itemService)
{
    public void Draw(string id, Action<ImGuiContextMenuBuilder> buildAction)
    {
        var builder = new ImGuiContextMenuBuilder(id, textService, mapService, itemService);
        buildAction(builder);
        builder.Draw();
    }
}

public unsafe struct ImGuiContextMenuBuilder(string id, TextService textService, MapService mapService, ItemService itemService)
{
    private readonly List<IImGuiContextMenuEntry> _entries = [];

    internal void Draw()
    {
        var visibleEntries = _entries.Where(entry => entry.Visible);
        var count = visibleEntries.Count();
        if (count == 0)
            return;
        using var popup = ImRaii.ContextPopupItem(id);
        if (!popup)
            return;
        var i = 0;
        foreach (var entry in visibleEntries)
            entry.Draw(new IterationArgs(i++, count));
    }

    public ImGuiContextMenuBuilder Add(IImGuiContextMenuEntry entry)
    {
        _entries.Add(entry);
        return this;
    }

    public ImGuiContextMenuBuilder AddSeparator()
    {
        _entries.Add(new ImGuiContextMenuSeparator());
        return this;
    }

    public ImGuiContextMenuBuilder AddTryOn(ExcelRowId<Item> itemId, uint glamourItemId = 0, byte stain0Id = 0, byte stain1Id = 0)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = itemService.CanTryOn(itemId),
            Label = textService.GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                    AgentTryon.TryOn(0, itemId, stain0Id, stain1Id);
                else
                    AgentTryon.TryOn(0, itemId, stain0Id, stain1Id, glamourItemId);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemFinder(ExcelRowId<Item> itemId)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = AgentLobby.Instance()->IsLoggedIn,
            Label = textService.GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () => ItemFinderModule.Instance()->SearchForItem(itemId)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddCopyItemName(ExcelRowId<Item> itemId)
    {
        var itemName = textService.GetItemName(itemId);

        _entries.Add(new ImGuiContextMenuEntry()
        {
            Label = textService.GetAddonText(159), // "Copy Item Name"
            ClickCallback = () => ImGui.SetClipboardText(itemName)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemSearch(ExcelRowId<Item> itemId)
    {
        var _itemService = itemService;

        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = _itemService.CanSearchForItem(itemId),
            Label = textService.Translate("ItemContextMenu.SearchTheMarkets"),
            LoseFocusOnClick = true,
            ClickCallback = () => _itemService.Search(itemId)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetLinkGlamour(GearsetEntry* gearset)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink == 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = UIGlobals.CanApplyGlamourPlates(),
            Label = textService.GetAddonText(4394),
            LoseFocusOnClick = true,
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetChangeGlamour(GearsetEntry* gearset)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = UIGlobals.CanApplyGlamourPlates(),
            Label = textService.GetAddonText(4395),
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetUnlinkGlamour(GearsetEntry* gearset)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = textService.GetAddonText(4396),
            ClickCallback = () => RaptureGearsetModule.Instance()->LinkGlamourPlate(gearset->Id, 0)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetChangePortrait(GearsetEntry* gearset)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null,
            Label = textService.GetAddonText(4411),
            ClickCallback = () =>
            {
                AgentBannerEditor.Instance()->Hide();
                AgentGearSet.Instance()->OpenBannerEditorForGearset(gearset->Id);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchCraftingMethod(ExcelRowId<Item> itemId)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = itemService.IsCraftable(itemId),
            Label = textService.GetAddonText(1414), // "Search for Item by Crafting Method"
            LoseFocusOnClick = true,
            ClickCallback = () => AgentRecipeNote.Instance()->OpenRecipeByItemId(itemId)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForGatheringPoint(ExcelRowId<Item> itemId, TerritoryType territoryType, ReadOnlySeString? prefix = null)
    {
        var _mapService = mapService;
        var _itemService = itemService;

        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = territoryType.RowId != 0 && itemService.IsGatherable(itemId),
            Label = textService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (_itemService.GetGatheringPoints(itemId).TryGetFirst(point => point.TerritoryType.RowId == territoryType.RowId, out var point))
                    _mapService.OpenMap(point, itemId, prefix);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForFishingSpot(ExcelRowId<Item> itemId, ReadOnlySeString? prefix = null)
    {
        var _mapService = mapService;
        var _itemService = itemService;
        var isFish = itemService.IsFish(itemId);
        var isSpearfish = itemService.IsSpearfish(itemId);

        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = isFish || isSpearfish,
            Label = textService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (isSpearfish)
                {
                    if (_itemService.GetSpearfishingGatheringPoints(itemId).TryGetFirst(out var point))
                        _mapService.OpenMap(point, itemId, prefix);
                }
                else
                {
                    if (_itemService.GetFishingSpots(itemId).TryGetFirst(out var spot))
                        _mapService.OpenMap(spot, itemId, prefix);
                }
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchGatheringMethod(ExcelRowId<Item> itemId)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = itemService.IsGatherable(itemId),
            Label = textService.GetAddonText(1472), // "Search for Item by Gathering Method"
            LoseFocusOnClick = true,
            ClickCallback = () => AgentGatheringNote.Instance()->OpenGatherableByItemId((ushort)(uint)itemId)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenInFishGuide(ExcelRowId<Item> itemId)
    {
        var isFish = itemService.IsFish(itemId);
        var isSpearfish = itemService.IsSpearfish(itemId);

        _entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = isFish || isSpearfish,
            Label = textService.Translate("ItemContextMenu.OpenInFishGuide"),
            LoseFocusOnClick = true,
            ClickCallback = () => AgentFishGuide.Instance()->OpenForItemId(itemId, isSpearfish)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenOnGarlandTools(string type, uint id)
    {
        _entries.Add(new ImGuiContextMenuEntry()
        {
            Label = textService.Translate("ItemContextMenu.OpenOnGarlandTools"),
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
                    Color.Grey,
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0) * ImGuiHelpers.GlobalScale);
                ImGuiUtils.TextUnformattedColored(Color.Grey, $"https://www.garlandtools.org/db/#{type}/{id}");
            }
        });

        return this;
    }
}

public interface IImGuiContextMenuEntry
{
    public bool Visible { get; }
    public bool Enabled { get; }
    public string Label { get; }
    public bool LoseFocusOnClick { get; }
    public Action? ClickCallback { get; }
    public Action? HoverCallback { get; }
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
