using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui.Enums;
using HaselCommon.Services;
using HaselCommon.Windowing;
using ImGuiNET;

namespace HaselCommon.Gui;

public partial class Window : SimpleWindow, IDisposable
{
    private readonly ImRaii.Style _windowStyle = new();
    public Node RootNode { get; } = [];

    public Window(WindowManager wm, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(wm, name, flags, forceMainWindow)
    {
        Flags |= ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
    }

    public override void OnClose()
    {
        _windowStyle.Dispose();
        base.OnClose();
    }

    public override void Dispose()
    {
        _windowStyle.Dispose();
        RootNode.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void PreDraw()
    {
        base.PreDraw();
        //_windowStyle.Push(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override unsafe void Draw()
    {
        _windowStyle.Dispose();

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
        _windowStyle.Dispose();

        if (RootNode.EnableDebug)
        {
            DrawDebugWindow();
        }
    }
}
