using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Raii;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Extensions.Sheets;
using HaselCommon.Records;
using ImGuiNET;
using static HaselCommon.Utils.ImGuiContextMenu;
using GearsetEntry = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureGearsetModule.GearsetEntry;
using Item = Lumina.Excel.GeneratedSheets.Item;

namespace HaselCommon.Utils;

public class ImGuiContextMenu : List<IContextMenuEntry>
{
    private readonly string key;

    public ImGuiContextMenu(string key)
    {
        this.key = key;
    }

    public void Draw()
    {
        using var popup = ImRaii.ContextPopupItem(key);
        if (!popup.Success)
            return;

        var visibleEntries = this.Where(entry => entry.Visible);
        var count = visibleEntries.Count();
        var i = 0;
        foreach (var entry in visibleEntries)
        {
            entry.Draw(new IterationArgs(i++, count));
        }
    }

    private enum ContextMenuGlamourCallbackAction
    {
        Link = 20,
        ChangeLink = 21,
        Unlink = 22,
    }

    private unsafe delegate void ContextMenuGlamourCallbackDelegate(nint agentGearset, uint gearsetId, ContextMenuGlamourCallbackAction action);
    private static readonly Lazy<ContextMenuGlamourCallbackDelegate> ContextMenuGlamourCallback
        = new(() => MemoryUtils.GetDelegateForSignature<ContextMenuGlamourCallbackDelegate>("40 53 48 83 EC 20 8B DA 41 83 F8 14"));

    public interface IContextMenuEntry
    {
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public string Label { get; set; }
        public bool LoseFocusOnClick { get; set; }
        public Action? ClickCallback { get; set; }
        public Action? HoverCallback { get; set; }
        public void Draw(IterationArgs args);
    }

    public record ContextMenuSeparator : IContextMenuEntry
    {
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool LoseFocusOnClick { get; set; } = false;
        public string Label { get; set; } = string.Empty;
        public Action? ClickCallback { get; set; } = null;
        public Action? HoverCallback { get; set; } = null;

        public void Draw(IterationArgs args)
        {
            if (!args.IsFirst && !args.IsLast)
                ImGui.Separator();
        }
    }

    public record ContextMenuEntry : IContextMenuEntry
    {
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool LoseFocusOnClick { get; set; } = false;
        public string Label { get; set; } = string.Empty;
        public Action? ClickCallback { get; set; } = null;
        public Action? HoverCallback { get; set; } = null;

        public void Draw(IterationArgs args)
        {
            if (ImGui.MenuItem(Label, Enabled))
            {
                ClickCallback?.Invoke();

                if (LoseFocusOnClick)
                {
                    ImGui.SetWindowFocus(null);
                }
            }
            if (ImGui.IsItemHovered())
            {
                HoverCallback?.Invoke();
            }
        }
    }

    public static unsafe ContextMenuEntry CreateTryOn(uint ItemId, uint GlamourItemId = 0, byte StainId = 0)
        => new()
        {
            Visible = GetRow<Item>(ItemId)?.CanTryOn() ?? false,
            Label = GetAddonText(2426), // "Try On"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                if (ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift))
                    AgentTryon.TryOn(0, ItemId, StainId, 0, 0);
                else
                    AgentTryon.TryOn(0, ItemId, StainId, GlamourItemId, StainId);
            }
        };

    public static unsafe ContextMenuEntry CreateItemFinder(uint ItemId)
        => new()
        {
            Label = GetAddonText(4379), // "Search for Item"
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                ItemFinderModule.Instance()->SearchForItem(ItemId);
            }
        };

    public static ContextMenuEntry CreateGarlandTools(uint ItemId)
        => new()
        {
            Label = t("ItemContextMenu.OpenOnGarlandTools"),
            ClickCallback = () =>
            {
                Task.Run(() => Util.OpenLink($"https://www.garlandtools.org/db/#item/{ItemId}"));
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
                ImGuiUtils.TextUnformattedColored(Colors.Grey, $"https://www.garlandtools.org/db/#item/{ItemId}");
            }
        };

    public static ContextMenuEntry CreateCopyItemName(uint ItemId)
        => new()
        {
            Label = GetAddonText(159), // "Copy Item Name"
            ClickCallback = () =>
            {
                ImGui.SetClipboardText(GetRow<Item>(ItemId)!.Singular.ToDalamudString().ToString());
            }
        };

    public static unsafe ContextMenuEntry CreateGearsetLinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset->GlamourSetLink == 0,
            Enabled = UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = GetAddonText(4394),
            LoseFocusOnClick = true,
            ClickCallback = () =>
            {
                var agentGearset = AgentModule.Instance()->GetAgentByInternalId(AgentId.Gearset);
                ContextMenuGlamourCallback.Value?.Invoke((nint)agentGearset, gearset->ID, ContextMenuGlamourCallbackAction.Link);
            }
        };

    public static unsafe ContextMenuEntry CreateGearsetUnlinkGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset->GlamourSetLink != 0,
            Enabled = UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = GetAddonText(4396),
            ClickCallback = () =>
            {
                var agentGearset = AgentModule.Instance()->GetAgentByInternalId(AgentId.Gearset);
                ContextMenuGlamourCallback.Value?.Invoke((nint)agentGearset, gearset->ID, ContextMenuGlamourCallbackAction.Unlink);
            }
        };

    public static unsafe ContextMenuEntry CreateGearsetChangeGlamour(GearsetEntry* gearset)
        => new()
        {
            Visible = gearset->GlamourSetLink != 0,
            Enabled = UIState.Instance()->IsUnlockLinkUnlocked(15),
            Label = GetAddonText(4395),
            ClickCallback = () =>
            {
                var agentGearset = AgentModule.Instance()->GetAgentByInternalId(AgentId.Gearset);
                ContextMenuGlamourCallback.Value?.Invoke((nint)agentGearset, gearset->ID, ContextMenuGlamourCallbackAction.ChangeLink);
            }
        };

    public static unsafe ContextMenuEntry CreateGearsetChangePortrait(GearsetEntry* gearset)
        => new()
        {
            Label = GetAddonText(4411),
            ClickCallback = () =>
            {
                GetAgent<AgentBannerEditor>()->AgentInterface.Hide();
                GetAgent<AgentBannerEditor>()->OpenForGearset(gearset->ID);
            }
        };
}
