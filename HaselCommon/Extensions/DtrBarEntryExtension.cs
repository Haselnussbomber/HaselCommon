using Dalamud.Game.Gui.Dtr;

namespace HaselCommon.Extensions;

public static class DtrBarEntryExtension
{
    public static void SetText(this DtrBarEntry entry, string text)
    {
        if (entry.Text == null || entry.Text.TextValue != text)
            entry.Text = text;
    }

    public static void SetVisibility(this DtrBarEntry entry, bool visible)
    {
        if (entry.Shown != visible)
            entry.Shown = visible;
    }
}
