using System.Numerics;
using System.Text;
using ImGuiNET;

namespace HaselCommon.Utils;

public static unsafe class ImGuiNativeAdditions
{
    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern void* igGetCurrentWindow();

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern float ImGuiWindow_TitleBarHeight(void* window);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    public static extern float ImGuiWindow_MenuBarHeight(void* window);

    public static void AddText(ImDrawListPtr drawListPtr, ImFontPtr fontPtr, float fontSize, Vector2 pos, uint col, string text, float wrapWidth)
    {
        fixed (byte* text_begin = Encoding.UTF8.GetBytes(text + "\0"))
            ImGuiNative.ImDrawList_AddText_FontPtr(drawListPtr.NativePtr, fontPtr.NativePtr, fontSize, pos, col, text_begin, null, wrapWidth, null);
    }
}
