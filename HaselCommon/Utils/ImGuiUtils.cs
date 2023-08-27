using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Raii;
using Dalamud.Utility;
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

    public static void DrawSection(string Label, bool PushDown = true, bool RespectUiTheme = false, uint UIColor = 1)
    {
        var style = ImGui.GetStyle();

        // push down a bit
        if (PushDown)
            PushCursorY(style.ItemSpacing.Y * 2);

        var color = Colors.Gold;
        if (RespectUiTheme && Colors.IsLightTheme)
            color = Colors.GetUIColor(UIColor);

        TextUnformattedColored(color, Label);

        // pull up the separator
        PushCursorY(-style.ItemSpacing.Y + 3);
        ImGui.Separator();
        PushCursorY(style.ItemSpacing.Y * 2 - 1);
    }

    public static ImRaii.Indent ConfigIndent()
        => ImRaii.PushIndent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X / 2f);

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

    public static void Icon(FontAwesomeIcon icon)
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.TextDisabled(icon.ToIconString());
    }
}
