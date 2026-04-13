namespace HaselCommon.Gui;

public static class ImStyle
{
    public static float Scale => ImGui.GetIO().FontGlobalScale;

    public static float Alpha => ImGui.GetStyle().Alpha;
    public static float DisabledAlpha => ImGui.GetStyle().DisabledAlpha;
    public static Vector2 WindowPadding => ImGui.GetStyle().WindowPadding;
    public static float WindowRounding => ImGui.GetStyle().WindowRounding;
    public static float WindowBorderSize => ImGui.GetStyle().WindowBorderSize;
    public static Vector2 WindowMinSize => ImGui.GetStyle().WindowMinSize;
    public static Vector2 WindowTitleAlign => ImGui.GetStyle().WindowTitleAlign;
    public static ImGuiDir WindowMenuButtonPosition => ImGui.GetStyle().WindowMenuButtonPosition;
    public static float ChildRounding => ImGui.GetStyle().ChildRounding;
    public static float ChildBorderSize => ImGui.GetStyle().ChildBorderSize;
    public static float PopupRounding => ImGui.GetStyle().PopupRounding;
    public static float PopupBorderSize => ImGui.GetStyle().PopupBorderSize;
    public static Vector2 FramePadding => ImGui.GetStyle().FramePadding;
    public static float FrameRounding => ImGui.GetStyle().FrameRounding;
    public static float FrameBorderSize => ImGui.GetStyle().FrameBorderSize;
    public static Vector2 ItemSpacing => ImGui.GetStyle().ItemSpacing;
    public static Vector2 ItemInnerSpacing => ImGui.GetStyle().ItemInnerSpacing;
    public static Vector2 CellPadding => ImGui.GetStyle().CellPadding;
    public static Vector2 TouchExtraPadding => ImGui.GetStyle().TouchExtraPadding;
    public static float IndentSpacing => ImGui.GetStyle().IndentSpacing;
    public static float ColumnsMinSpacing => ImGui.GetStyle().ColumnsMinSpacing;
    public static float ScrollbarSize => ImGui.GetStyle().ScrollbarSize;
    public static float ScrollbarRounding => ImGui.GetStyle().ScrollbarRounding;
    public static float GrabMinSize => ImGui.GetStyle().GrabMinSize;
    public static float GrabRounding => ImGui.GetStyle().GrabRounding;
    public static float LogSliderDeadzone => ImGui.GetStyle().LogSliderDeadzone;
    public static float TabRounding => ImGui.GetStyle().TabRounding;
    public static float TabBorderSize => ImGui.GetStyle().TabBorderSize;
    public static float TabMinWidthForCloseButton => ImGui.GetStyle().TabMinWidthForCloseButton;
    public static ImGuiDir ColorButtonPosition => ImGui.GetStyle().ColorButtonPosition;
    public static Vector2 ButtonTextAlign => ImGui.GetStyle().ButtonTextAlign;
    public static Vector2 SelectableTextAlign => ImGui.GetStyle().SelectableTextAlign;
    public static Vector2 DisplayWindowPadding => ImGui.GetStyle().DisplayWindowPadding;
    public static Vector2 DisplaySafeAreaPadding => ImGui.GetStyle().DisplaySafeAreaPadding;
    public static float MouseCursorScale => ImGui.GetStyle().MouseCursorScale;
    public static bool AntiAliasedLines => ImGui.GetStyle().AntiAliasedLines;
    public static bool AntiAliasedLinesUseTex => ImGui.GetStyle().AntiAliasedLinesUseTex;
    public static bool AntiAliasedFill => ImGui.GetStyle().AntiAliasedFill;
    public static float CurveTessellationTol => ImGui.GetStyle().CurveTessellationTol;
    public static float CircleTessellationMaxError => ImGui.GetStyle().CircleTessellationMaxError;
}
