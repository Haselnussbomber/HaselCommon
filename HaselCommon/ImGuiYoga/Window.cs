using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Services;
using HaselCommon.Windowing;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

public partial class Window(WindowManager wm, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : SimpleWindow(wm, name, flags, forceMainWindow), IDisposable
{
    private ImRaii.Style WindowStyle { get; } = new();
    public Document Document { get; init; } = new();

    public override void OnClose()
    {
        WindowStyle.Dispose();
        base.OnClose();
    }

    public override void Dispose()
    {
        WindowStyle.Dispose();
        Document.DisposeRecursive();
        base.Dispose();
    }

    public override void Update()
    {
        try
        {
            Document.Update();
        }
        catch (Exception ex)
        {
            Document.Logger?.LogError(ex, "Unhandled exception in Update");
        }
    }

    public override void PreDraw()
    {
        base.PreDraw();
        WindowStyle.Push(ImGuiStyleVar.WindowPadding, Vector2.Zero);
    }

    public override unsafe void Draw()
    {
        WindowStyle.Dispose();

        Document.Style.PositionType = YGPositionType.Absolute;
        Document.Style.PositionTop = ImGui.GetCursorPosY();
        Document.Style.PositionLeft = ImGui.GetCursorPosX();

        try
        {
#if DEBUG
            DebugTimer.Restart();
            Document.CalculateLayout(ImGui.GetContentRegionAvail());
            DebugTimer.Stop();
            DebugLayoutTime = DebugTimer.Elapsed.TotalMilliseconds;

            DebugTimer.Restart();
            Document.Draw();
            DebugTimer.Stop();
            DebugDrawTime = DebugTimer.Elapsed.TotalMilliseconds;
#else
        Document.CalculateLayout(ImGui.GetContentRegionAvail());
        Document.Draw();
#endif
        }
        catch { }
    }

#if DEBUG
    public override void PostDraw()
    {
        base.PostDraw();
        WindowStyle.Dispose();
        DrawDebugWindow();
    }
#endif
}
