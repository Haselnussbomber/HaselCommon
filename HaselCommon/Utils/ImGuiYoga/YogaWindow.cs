using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace HaselCommon.Utils.ImGuiYoga;

public class YogaWindow : Window, IDisposable
{
    public Node RootNode { get; }

    public Vector2 WindowPosition { get; private set; }
    public Vector2 WindowSize { get; private set; }
    public float TitleBarHeight { get; private set; }
    public float MenuBarHeight { get; private set; }
    public Vector2 ViewportPosition { get; private set; }
    public Vector2 ViewportSize { get; private set; }

    public YogaWindow(string name, string ns) : base(name, ImGuiWindowFlags.None, false)
    {
        Namespace = ns;
        RootNode = new(this);
    }

    public void Dispose()
    {
        RootNode.DisposeRecursive();
    }

    public override unsafe void Draw()
    {
        WindowPosition = ImGui.GetWindowPos();
        WindowSize = ImGui.GetWindowSize();

        var windowPtr = ImGuiNativeAdditions.igGetCurrentWindow();
        TitleBarHeight = ImGuiNativeAdditions.ImGuiWindow_TitleBarHeight(windowPtr);
        MenuBarHeight = ImGuiNativeAdditions.ImGuiWindow_MenuBarHeight(windowPtr);

        ViewportPosition = WindowPosition + new Vector2(0, TitleBarHeight + MenuBarHeight) + ImGui.GetStyle().WindowPadding;
        ViewportSize = WindowPosition + WindowSize - ViewportPosition - ImGui.GetStyle().WindowPadding;

        RootNode.CalculateLayout(ViewportSize);
        RootNode.Draw();
    }
}
