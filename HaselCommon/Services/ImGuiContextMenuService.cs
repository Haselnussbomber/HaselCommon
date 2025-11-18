using System.Threading.Tasks;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Exd;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ImGuiContextMenuService
{
    private readonly TextService _textService;
    private readonly MapService _mapService;
    private readonly ItemService _itemService;

    private readonly ObjectPool<List<IImGuiContextMenuEntry>> _objectPool = ObjectPool.Create(new PooledListPolicy<IImGuiContextMenuEntry>());

    public void Draw(string id, Action<ImGuiContextMenuBuilder> buildAction)
    {
        var shouldOpen = ImGui.IsMouseReleased(ImGuiMouseButton.Right) && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByPopup);
        var isOpen = shouldOpen || ImGui.IsPopupOpen(id);

        if (!isOpen)
            return;

        var entries = _objectPool.Get();

        try
        {
            var builder = new ImGuiContextMenuBuilder(_textService, _mapService, _itemService, entries);
            buildAction(builder);

            if (shouldOpen && entries.Any(entry => entry.Visible))
                ImGui.OpenPopup(id, ImGuiPopupFlags.MouseButtonRight);

            using var popup = ImRaii.Popup(id, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings);
            if (!popup)
                return;

            var i = 0;
            var visibleEntries = entries.Where(entry => entry.Visible);
            var count = visibleEntries.Count();
            foreach (var entry in visibleEntries)
                entry.Draw(new IterationArgs(i++, count));
        }
        finally
        {
            _objectPool.Return(entries);
        }
    }
}

public unsafe struct ImGuiContextMenuBuilder(TextService textService, MapService mapService, ItemService itemService, List<IImGuiContextMenuEntry> entries)
{
    public ImGuiContextMenuBuilder Add(IImGuiContextMenuEntry entry)
    {
        entries.Add(entry);
        return this;
    }

    public ImGuiContextMenuBuilder AddSeparator()
    {
        entries.Add(new ImGuiContextMenuSeparator());
        return this;
    }

    public ImGuiContextMenuBuilder AddTryOn(ItemHandle item, uint glamourItemId = 0, byte stain0Id = 0, byte stain1Id = 0)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = item.CanTryOn,
            Label = textService.GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                AgentTryon.TryOn(
                    0,
                    item,
                    stain0Id: stain0Id,
                    stain1Id: stain1Id,
                    glamourItemId: ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift)
                        ? glamourItemId
                        : 0);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemFinder(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = AgentLobby.Instance()->IsLoggedIn && !item.IsCurrency,
            Label = textService.GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () => ItemFinderModule.Instance()->SearchForItem(item)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddLinkItem(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = AgentLobby.Instance()->IsLoggedIn && !item.IsCurrency && ExdModule.GetItemRowById(item) != null,
            Label = textService.GetAddonText(4697), // "Link"
            LoseFocusOnClick = true,
            ClickCallback = () => AgentChatLog.Instance()->LinkItem(item)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddCopyItemName(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Label = textService.GetAddonText(159), // "Copy Item Name"
            ClickCallback = () => ImGui.SetClipboardText(item.Name.ToString())
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddItemSearch(ItemHandle item)
    {
        var agent = AgentItemSearch.Instance();

        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = agent->CanSearchForItem(item),
            Label = textService.Translate("ItemContextMenu.SearchTheMarkets"),
            LoseFocusOnClick = true,
            ClickCallback = () => agent->SearchForItem(item)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchCraftingMethod(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = item.IsCraftable,
            Label = textService.GetAddonText(1414), // "Search for Item by Crafting Method"
            LoseFocusOnClick = true,
            ClickCallback = () => AgentRecipeNote.Instance()->SearchRecipeByItemId(item)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForGatheringPoint(ItemHandle item, TerritoryType territoryType, ReadOnlySeString? prefix = null)
    {
        var _mapService = mapService;
        var _itemService = itemService;

        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = territoryType.RowId != 0 && item.IsGatherable,
            Label = textService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (_itemService.GetGatheringPoints(item).TryGetFirst(point => point.TerritoryType.RowId == territoryType.RowId, out var point))
                    _mapService.OpenMap(point, item, prefix);
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenMapForFishingSpot(ItemHandle item, ReadOnlySeString? prefix = null)
    {
        var _mapService = mapService;
        var _itemService = itemService;

        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = item.IsFish || item.IsSpearfish,
            Label = textService.GetAddonText(8506), // "Open Map"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (item.IsSpearfish)
                {
                    if (_itemService.GetSpearfishingGatheringPoints(item).TryGetFirst(out var point))
                        _mapService.OpenMap(point, item, prefix);
                }
                else
                {
                    if (_itemService.GetFishingSpots(item).TryGetFirst(out var spot))
                        _mapService.OpenMap(spot, item, prefix);
                }
            }
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddSearchGatheringMethod(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = item.IsGatherable,
            Label = textService.GetAddonText(1472), // "Search for Item by Gathering Method"
            LoseFocusOnClick = true,
            ClickCallback = () => AgentGatheringNote.Instance()->OpenGatherableByItemId((ushort)item.ItemId)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddOpenInFishGuide(ItemHandle item)
    {
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = item.IsFish || item.IsSpearfish,
            Label = textService.Translate("ItemContextMenu.OpenInFishGuide"),
            LoseFocusOnClick = true,
            ClickCallback = () => AgentFishGuide.Instance()->OpenForItemId(item, item.IsSpearfish)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetLinkGlamour(GearsetEntry* gearset)
    {
        entries.Add(new ImGuiContextMenuEntry()
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
        entries.Add(new ImGuiContextMenuEntry()
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
        entries.Add(new ImGuiContextMenuEntry()
        {
            Visible = gearset != null && gearset->GlamourSetLink != 0 && UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = textService.GetAddonText(4396),
            ClickCallback = () => RaptureGearsetModule.Instance()->LinkGlamourPlate(gearset->Id, 0)
        });

        return this;
    }

    public ImGuiContextMenuBuilder AddGearsetChangePortrait(GearsetEntry* gearset)
    {
        entries.Add(new ImGuiContextMenuEntry()
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

    public ImGuiContextMenuBuilder AddOpenOnGarlandTools(string type, uint id)
    {
        entries.Add(new ImGuiContextMenuEntry()
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
                    Color.Grey.ToUInt(),
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0) * ImGuiHelpers.GlobalScale);
                ImGui.TextColored(Color.Grey, $"https://www.garlandtools.org/db/#{type}/{id}");
            }
        });

        return this;
    }
}

public interface IImGuiContextMenuEntry
{
    bool Visible { get; }
    bool Enabled { get; }
    bool Selected { get; }
    string Label { get; }
    bool LoseFocusOnClick { get; }
    Action? ClickCallback { get; }
    Action? HoverCallback { get; }
    void Draw(IterationArgs args);
}

public struct ImGuiContextMenuEntry : IImGuiContextMenuEntry
{
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool Selected { get; set; } = false;
    public bool LoseFocusOnClick { get; set; } = false;
    public string Label { get; set; } = string.Empty;
    public Action? ClickCallback { get; set; } = null;
    public Action? HoverCallback { get; set; } = null;

    public ImGuiContextMenuEntry() { }

    public void Draw(IterationArgs args)
    {
        if (ImGui.MenuItem(Label, Selected, Enabled))
        {
            ClickCallback?.Invoke();

            if (LoseFocusOnClick)
                ImGui.ClearWindowFocus();
        }
        if (ImGui.IsItemHovered())
            HoverCallback?.Invoke();
    }
}

public struct ImGuiContextMenuSeparator : IImGuiContextMenuEntry
{
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool Selected { get; set; } = false;
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
