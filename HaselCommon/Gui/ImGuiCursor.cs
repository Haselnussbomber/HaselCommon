namespace HaselCommon.Gui;

// Adapted from ImSharp
// https://github.com/Ottermandias/ImSharp/blob/gc1.88/ImSharp/ImSharp/Cursor.cs

public static class ImGuiCursor
{
    public static Vector2 Position
    {
        get => ImGui.GetCursorPos();
        set => ImGui.SetCursorPos(value);
    }

    public static float X
    {
        get => ImGui.GetCursorPosX();
        set => ImGui.SetCursorPosX(value);
    }

    public static float Y
    {
        get => ImGui.GetCursorPosY();
        set => ImGui.SetCursorPosY(value);
    }

    public static Vector2 StartPosition => ImGui.GetCursorStartPos();

    public static Vector2 ScreenPosition
    {
        get => ImGui.GetCursorScreenPos();
        set => ImGui.SetCursorScreenPos(value);
    }

    public static unsafe void SameLineSpace()
    {
        var font = ImGui.GetFont();
        var glyph = font.FindGlyph(' ');
        var advanceX = glyph != null ? glyph->AdvanceX : font.FallbackGlyph.AdvanceX;
        var scale = ImGui.GetFontSize() / font.FontSize;
        ImGui.SameLine(0, advanceX * scale);
    }
}
