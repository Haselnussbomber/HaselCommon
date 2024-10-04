using HaselCommon.Gui.Yoga.Enums;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

public partial class YogaWindow : SimpleWindow, IDisposable
{
    public Node RootNode { get; } = [];

    public YogaWindow(WindowManager wm, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(wm, name, flags, forceMainWindow)
    {
        Flags |= ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    }

    public override void Dispose()
    {
        RootNode.Dispose();
        GC.SuppressFinalize(this);
    }

    public override unsafe void Draw()
    {
        RootNode.PositionType = PositionType.Absolute;
        RootNode.PositionTop = ImGui.GetCursorPosY();
        RootNode.PositionLeft = ImGui.GetCursorPosX();

#if DEBUG
        _debugTimer.Restart();
        RootNode.Update();
        _debugTimer.Stop();
        _debugUpdateTime = _debugTimer.Elapsed.TotalMilliseconds;

        _debugTimer.Restart();
        RootNode.CalculateLayout(ImGui.GetContentRegionAvail());
        _debugTimer.Stop();
        _debugLayoutTime = _debugTimer.Elapsed.TotalMilliseconds;

        _debugTimer.Restart();
        RootNode.Draw();
        _debugTimer.Stop();
        _debugDrawTime = _debugTimer.Elapsed.TotalMilliseconds;
#else
        RootNode.Update();
        RootNode.CalculateLayout(ImGui.GetContentRegionAvail());
        RootNode.Draw();
#endif
    }

    public override void PostDraw()
    {
        base.PostDraw();

        if (RootNode.EnableDebug)
        {
            DrawDebugWindow();
        }
    }
}
