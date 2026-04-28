namespace HaselCommon.Extensions;

public static class ImRaiiColorExtensions
{
    extension(ImRaii.ColorDisposable col)
    {
        public ImRaii.ColorDisposable Push(ImGuiCol idx, ImGuiCol color, bool condition = true)
        {
            return col.Push(idx, ImGui.GetColorU32(color), condition);
        }
    }
}
