using System.Threading.Tasks;
using Dalamud.Utility;
using UIColor = Lumina.Excel.Sheets.UIColor;

namespace HaselCommon.Gui;

public static class ImGuiUtils
{
    public static void DrawPaddedSeparator()
    {
        var itemSpacingHeight = ImGui.GetStyle().ItemSpacing.Y;

        PushCursorY(itemSpacingHeight);
        ImGui.Separator();
        PushCursorY(itemSpacingHeight - 1);
    }

    public static void DrawSection(string label, bool pushDown = true, bool respectUiTheme = false, RowRef<UIColor> uiColor = default)
    {
        var itemSpacingHeight = ImGui.GetStyle().ItemSpacing.Y;

        // push down a bit
        if (pushDown)
            PushCursorY(itemSpacingHeight * 2);

        var color = Color.Gold;
        if (respectUiTheme && Misc.IsLightTheme && uiColor.IsValid)
            color = Color.FromABGR(uiColor.Value.Dark);

        ImGui.TextColored(color, label);

        // pull up the separator
        PushCursorY(-itemSpacingHeight + 3);
        ImGui.Separator();
        PushCursorY(itemSpacingHeight * 2 - 1);
    }

    public static ImRaii.Indent ConfigIndent(bool condition = true)
        => ImRaii.PushIndent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X / 2f, true, condition);

    public static unsafe void DrawLink(string label, string title, string url)
    {
        ImGui.Text(label);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

            using var tooltip = ImRaii.Tooltip();
            if (tooltip.Success)
            {
                if (!string.IsNullOrEmpty(title))
                    ImGui.TextColored(Color.White, title);

                var pos = ImGui.GetCursorPos();
                ImGui.GetWindowDrawList().AddText(
                    UiBuilder.IconFont, 12,
                    ImGui.GetWindowPos() + pos + new Vector2(2),
                    Color.Grey.ToUInt(),
                    FontAwesomeIcon.ExternalLinkAlt.ToIconString()
                );
                ImGui.SetCursorPos(pos + new Vector2(20, 0));
                ImGui.TextColored(Color.Grey, url);
            }
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && ImGui.IsItemHovered())
            Task.Run(() => Util.OpenLink(url));
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
        => ImGui.SameLine(0, ImGui.CalcTextSize(" ").X);

    public static void PushCursor(Vector2 vec)
        => ImGui.SetCursorPos(ImGui.GetCursorPos() + vec);

    public static void PushCursor(float x, float y)
        => PushCursor(new Vector2(x, y));

    public static void PushCursorX(float x)
        => ImGui.SetCursorPosX(ImGui.GetCursorPosX() + x);

    public static void PushCursorY(float y)
        => ImGui.SetCursorPosY(ImGui.GetCursorPosY() + y);

    public static Vector2 GetIconSize(FontAwesomeIcon icon)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        return ImGui.CalcTextSize(icon.ToIconString());
    }

    public static void Icon(FontAwesomeIcon icon, uint? color = null)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, color ?? 0u, color != null))
        using (ImRaii.PushFont(UiBuilder.IconFont))
            ImGui.Text(icon.ToIconString());
    }

    public static Vector2 GetIconButtonSize(FontAwesomeIcon icon)
    {
        using var iconFont = ImRaii.PushFont(UiBuilder.IconFont);
        return ImGui.CalcTextSize(icon.ToIconString()) + ImGui.GetStyle().FramePadding * 2;
    }

    public static bool IconButton(string key, FontAwesomeIcon icon, string? tooltip = null, Vector2 size = default, bool disabled = false, bool active = false)
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

        if (tooltip != null && ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltip);
            ImGui.EndTooltip();
        }

        return pressed;
    }

    public static void DrawCopyableText(string displayText, CopyableTextOptions? options = null)
    {
        var opt = options ?? default;
        var textCopy = opt.CopyText ?? displayText;

        using var color = opt.TextColor?.Push(ImGuiCol.Text);

        if (opt.AsSelectable)
        {
            ImGui.Selectable(displayText);
        }
        else if (!string.IsNullOrEmpty(opt.HighlightedText) && displayText.IndexOf(opt.HighlightedText, StringComparison.InvariantCultureIgnoreCase) is { } pos && pos != -1)
        {
            ImGui.Text(displayText[..pos]);
            ImGui.SameLine(0, 0);

            using (Color.Yellow.Push(ImGuiCol.Text))
                ImGui.Text(displayText[pos..(pos + opt.HighlightedText.Length)]);

            ImGui.SameLine(0, 0);
            ImGui.Text(displayText[(pos + opt.HighlightedText.Length)..]);
        }
        else
        {
            ImGui.Text(displayText);
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (!opt.NoTooltip)
                ImGui.SetTooltip(opt.Tooltip ?? textCopy);
        }

        if (ImGui.IsItemClicked())
            ImGui.SetClipboardText(textCopy);
    }
}

public struct CopyableTextOptions
{
    public string? CopyText { get; set; }
    public string? Tooltip { get; set; }
    public bool AsSelectable { get; set; }
    public Color? TextColor { get; set; }
    public string? HighlightedText { get; set; }
    public bool NoTooltip { get; set; }
}
