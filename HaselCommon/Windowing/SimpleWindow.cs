using Dalamud.Interface.Windowing;
using HaselCommon.Windowing.Interfaces;
using ImGuiNET;

namespace HaselCommon.Windowing;

public abstract class SimpleWindow : Window, ISimpleWindow, IDisposable
{
    private readonly IWindowManager WindowManager;

    protected SimpleWindow(IWindowManager windowManager, string windowName)
        : base(windowName, ImGuiWindowFlags.None, false)
    {
        WindowManager = windowManager;
        Namespace = GetType().Name;
    }

    public void Open()
    {
        WindowManager.AddWindow(this);
        IsOpen = true;
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

    public void Dispose()
    {
        Close();
        OnClose();
        WindowManager.RemoveWindow(this);
        GC.SuppressFinalize(this);
    }
}
