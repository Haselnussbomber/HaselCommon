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
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text.ReadOnly;
using Action = System.Action;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using TerritoryType = Lumina.Excel.GeneratedSheets.TerritoryType;

namespace HaselCommon.Services;

public class ImGuiContextMenuService(TextService TextService, MapService MapService, ItemService ItemService)
{
    public void Draw(string id, Action<ImGuiContextMenuBuilder> buildAction)
    {
        using var popup = ImRaii.ContextPopupItem(id);
        if (!popup)
            return;

        var builder = new ImGuiContextMenuBuilder(TextService, MapService, ItemService);
        buildAction(builder);
        builder.Draw();
    }
}

public unsafe struct ImGuiContextMenuBuilder(TextService TextService, MapService MapService, ItemService ItemService)
{
    private readonly List<IImGuiContextMenuEntry> Entries = [];

    internal void Draw()
    {
        var visibleEntries = Entries.Where(entry => entry.Visible);
        var count = visibleEntries.Count();
        var i = 0;
        foreach (var entry in visibleEntries)
            entry.Draw(new IterationArgs(i++, count));
    }

    public ImGuiContextMenuBuilder Add(IImGuiContextMenuEntry entry)
    {
        Entries.Add(entry);
        return this;
    }

    public ImGuiContextMenuBuilder AddSeparator()
    {
        Entries.Add(new ImGuiContextMenuSeparator());
        return this;
    }

    public ImGuiContextMenuBuilder AddTryOn(Item item, uint glamourItemId = 0, byte stain1Id = 0, byte stain2Id = 0)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = ItemService.CanTryOn(item),
            Label = TextService.GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                    AgentTryon.TryOn(0, item.RowId, stain1Id, stain2Id);
                else
                    AgentTryon.TryOn(0, item.RowId, stain1Id, stain2Id, glamourItemId);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemFinder(uint itemId)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Label = TextService.GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemFinderModule.Instance()->SearchForItem(itemId);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddCopyItemName(uint itemId)
    {
        var itemName = TextService.GetItemName(itemId);

        Entries.Add(new ImGuiContextMenuEntry()
        {
            Label = TextService.GetAddonText(159), // "Copy Item Name"
            ClickCallback = () =>
            {
                ImGui.SetClipboardText(itemName);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemSearch(Item item)
    {
        var itemService = ItemService;

        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = ItemService.CanSearchForItem(item),
            Label = TextService.Translate("ItemContextMenu.SearchTheMarkets"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                itemService.Search(item);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetLinkGlamour(GearsetEntry* gearset)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink == 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = TextService.GetAddonText(4394),
            LoseFocusOnClick = true,
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetChangeGlamour(GearsetEntry* gearset)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Enabled = GameMain.IsInSanctuary(),
            Label = TextService.GetAddonText(4395),
            ClickCallback = () => AgentMiragePrismMiragePlate.Instance()->OpenForGearset(gearset->Id, gearset->GlamourSetLink)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetUnlinkGlamour(GearsetEntry* gearset)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = TextService.GetAddonText(4396),
            ClickCallback = () => RaptureGearsetModule.Instance()->LinkGlamourPlate(gearset->Id, 0)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetChangePortrait(GearsetEntry* gearset)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null,
            Label = TextService.GetAddonText(4411),
            ClickCallback = () =>
            {
                AgentBannerEditor.Instance()->Hide();
                AgentGearSet.Instance()->OpenBannerEditorForGearset(gearset->Id);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchCraftingMethod(Item item)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = ItemService.IsCraftable(item),
            Label = TextService.GetAddonText(1414), // "Search for Item by Crafting Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentRecipeNote.Instance()->OpenRecipeByItemId(item.RowId);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForGatheringPoint(Item item, TerritoryType? territoryType, ReadOnlySeString? prefix = null)
    {
        var mapService = MapService;
        var itemService = ItemService;

        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = territoryType != null && ItemService.IsGatherable(item),
            Label = TextService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var point = itemService.GetGatheringPoints(item).First(point => point.TerritoryType.Row == territoryType!.RowId);
                mapService.OpenMap(point, item, prefix);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForFishingSpot(Item item, ReadOnlySeString? prefix = null)
    {
        var mapService = MapService;
        var itemService = ItemService;
        var isFish = itemService.IsFish(item);
        var isSpearfish = itemService.IsSpearfish(item);

        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = isFish || isSpearfish,
            Label = TextService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (isSpearfish)
                    mapService.OpenMap(itemService.GetSpearfishingGatheringPoints(item).First(), item, prefix);
                else
                    mapService.OpenMap(itemService.GetFishingSpots(item).First(), item, prefix);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchGatheringMethod(Item item)
    {
        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = ItemService.IsGatherable(item),
            Label = TextService.GetAddonText(1472), // "Search for Item by Gathering Method"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentGatheringNote.Instance()->OpenGatherableByItemId((ushort)item.RowId);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenInFishGuide(Item item)
    {
        var itemService = ItemService;
        var isFish = itemService.IsFish(item);
        var isSpearfish = itemService.IsSpearfish(item);

        Entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = isFish || isSpearfish,
            Label = TextService.Translate("ItemContextMenu.OpenInFishGuide"),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var agent = (AgentFishGuide*)AgentModule.Instance()->GetAgentByInternalId(AgentId.FishGuide);
                agent->OpenForItemId(item.RowId, isSpearfish);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenOnGarlandTools(string type, uint id)
    {
        Entries.Add(new ImGuiContextMenuEntry()
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
        });

        return this;
    }
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
