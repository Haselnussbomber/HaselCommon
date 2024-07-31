using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using HaselCommon.Extensions;
using ImGuiNET;

namespace HaselCommon.Utils;

public static class ImGuiUtils
{
    public static void DrawPaddedSeparator()
    {
        var style = ImGui.GetStyle();

        PushCursorY(style.ItemSpacing.Y);
        ImGui.Separator();
        PushCursorY(style.ItemSpacing.Y - 1);
    }

    public static void DrawSection(string Label, bool PushDown = true, bool RespectUiTheme = false, Lumina.Excel.GeneratedSheets.UIColor? UIColor = null)
    {
        var style = ImGui.GetStyle();

        // push down a bit
        if (PushDown)
            PushCursorY(style.ItemSpacing.Y * 2);

        var color = Colors.Gold;
        if (RespectUiTheme && UIColor != null && Colors.IsLightTheme)
            color = UIColor.GetForegroundColor();

        TextUnformattedColored(color, Label);

        // pull up the separator
        PushCursorY(-style.ItemSpacing.Y + 3);
        ImGui.Separator();
        PushCursorY(style.ItemSpacing.Y * 2 - 1);
    }

    public static ImRaii.Indent ConfigIndent(bool enabled = true)
        => ImRaii.PushIndent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X / 2f, true, enabled);

    public static void DrawLink(string label, string title, string url)
    {
        ImGui.TextUnformatted(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                TextUnformattedColored(Colors.White, title);

                var pos = ImGui.GetCursorPos();
                ImGui.GetWindowDrawList().AddText(
                    UiBuilder.IconFont, 12,
                    ImGui.GetWindowPos() + pos + new Vector2(2),
                    Colors.Grey,
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0));
                TextUnformattedColored(Colors.Grey, url);
            }
        }

        if (ImGui.IsItemClicked())
        {
            Task.Run(() => Util.OpenLink(url));
        }
    }

    public static void VerticalSeparator(uint color)
    {
        ImGui.SameLine();

        var height = ImGui.GetFrameHeight();
        var pos = ImGui.GetWindowPos() + ImGui.GetCursorPos();

        ImGui.GetWindowDrawList().AddLine(
            pos + new Vector2(3f, 0f),
            pos + new Vector2(3f, height),
            color
        );

        ImGui.Dummy(new(7, height));
    }

    public static void VerticalSeparator()
        => VerticalSeparator(ImGui.GetColorU32(ImGuiCol.Separator));

    public static void SameLineSpace()
    {
        using var itemSpacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(ImGui.CalcTextSize(" ").X, 0));
        ImGui.SameLine();
    }

    public static bool IsInViewport(Vector2 size)
    {
        var distanceY = ImGui.GetCursorPosY() - ImGui.GetScrollY();
        return distanceY >= -size.Y && distanceY <= ImGui.GetWindowHeight();
    }

    public static void PushCursor(Vector2 vec)
        => ImGui.SetCursorPos(ImGui.GetCursorPos() + vec);

    public static void PushCursor(float x, float y)
        => PushCursor(new Vector2(x, y));

    public static void PushCursorX(float x)
        => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + x);

    public static void PushCursorY(float y)
        => ImGui.SetCursorPosY(ImGui.GetCursorPosY() + y);

    public static void TextUnformattedDisabled(string text)
    {
        using (ImRaii.Disabled())
            ImGui.TextUnformatted(text);
    }

    public static void TextUnformattedColored(uint col, string text)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, col))
            ImGui.TextUnformatted(text);
    }

    public static Vector2 GetIconSize(FontAwesomeIcon icon)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        return ImGui.CalcTextSize(icon.ToIconString());
    }

    public static void Icon(FontAwesomeIcon icon, uint? col = null)
    {
        using var color = col != null ? ImRaii.PushColor(ImGuiCol.Text, (uint)col) : null;
        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.TextUnformatted(icon.ToIconString());
    }

    public static Vector2 GetIconButtonSize(FontAwesomeIcon icon)
    {
        using var iconFont = ImRaii.PushFont(UiBuilder.IconFont);
        return ImGui.CalcTextSize(icon.ToIconString()) + ImGui.GetStyle().FramePadding * 2;
    }

    public static bool IconButton(string key, FontAwesomeIcon icon, string tooltip, Vector2 size = default, bool disabled = false, bool active = false)
    {
        using var iconFont = ImRaii.PushFont(UiBuilder.IconFont);
        if (!key.StartsWith("##")) key = "##" + key;

        var disposables = new List<IDisposable>();

        if (disabled)
        {
            disposables.Add(ImRaii.PushColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]));
            disposables.Add(ImRaii.PushColor(ImGuiCol.ButtonActive, ImGui.GetStyle().Colors[(int)ImGuiCol.Button]));
            disposables.Add(ImRaii.PushColor(ImGuiCol.ButtonHovered, ImGui.GetStyle().Colors[(int)ImGuiCol.Button]));
        }
        else if (active)
        {
            disposables.Add(ImRaii.PushColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]));
        }

        var pressed = ImGui.Button(icon.ToIconString() + key, size);

        foreach (var disposable in disposables)
            disposable.Dispose();

        iconFont?.Dispose();

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tooltip);
            ImGui.EndTooltip();
        }

        return pressed;
    }

    public struct EndUnconditionally(Action endAction, bool success) : ImRaii.IEndObject, IDisposable
    {
        private Action EndAction { get; } = endAction;

        public bool Success { get; } = success;

        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (!Disposed)
            {
                EndAction();
                Disposed = true;
            }
        }
    }

    public struct EndConditionally(Action endAction, bool success) : ImRaii.IEndObject, IDisposable
    {
        public bool Success { get; } = success;

        public bool Disposed { get; private set; } = false;

        private Action EndAction { get; } = endAction;

        public void Dispose()
        {
            if (!Disposed)
            {
                if (Success)
                {
                    EndAction();
                }

                Disposed = true;
            }
        }
    }
}
