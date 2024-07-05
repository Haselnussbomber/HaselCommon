using Dalamud.Interface.Windowing;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Windowing;

public abstract class SimpleWindow : Window, IDisposable
{
    private readonly WindowManager WindowManager;

    protected SimpleWindow(WindowManager windowManager, string windowName)
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
