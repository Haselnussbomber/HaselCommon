using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui.Enums;
using HaselCommon.Services;
using HaselCommon.Windowing;
using ImGuiNET;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Gui;

public partial class Window : SimpleWindow, IDisposable
{
    private readonly ILogger _logger;
    private readonly ImRaii.Style _windowStyle = new();
    public Node RootNode { get; init; }

    public Window(WindowManager wm, ILogger logger, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(wm, name, flags, forceMainWindow)
    {
        _logger = logger;
        RootNode = new Node();
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
        base.Dispose();
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
        DebugTimer.Restart();
        RootNode.Update();
        DebugTimer.Stop();
        DebugUpdateTime = DebugTimer.Elapsed.TotalMilliseconds;

        DebugTimer.Restart();
        RootNode.CalculateLayout(ImGui.GetContentRegionAvail());
        DebugTimer.Stop();
        DebugLayoutTime = DebugTimer.Elapsed.TotalMilliseconds;

        DebugTimer.Restart();
        RootNode.Draw();
        DebugTimer.Stop();
        DebugDrawTime = DebugTimer.Elapsed.TotalMilliseconds;
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
