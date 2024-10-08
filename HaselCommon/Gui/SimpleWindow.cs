using Dalamud.Interface.Windowing;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui;

public abstract class SimpleWindow : Window, IDisposable
{
    protected readonly WindowManager WindowManager;

    protected SimpleWindow(WindowManager windowManager, string windowName, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false)
        : base(windowName, flags, forceMainWindow)
    {
        WindowManager = windowManager;
        Namespace = GetType().Name;
    }

    public void Open()
    {
        WindowManager.AddWindow(this);
        IsOpen = true;
        Collapsed = false;
        BringToFront();
    }

    public void Close()
    {
        IsOpen = false;
    }

    public new void Toggle()
    {
        if (!IsOpen)
            Open();
        else
            Close();
    }

    public override void OnClose()
    {
        WindowManager.RemoveWindow(this);
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Collapsed != null)
            Collapsed = null;
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
