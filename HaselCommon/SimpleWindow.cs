using Dalamud.Interface.Windowing;
using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon;

public abstract class SimpleWindow : Window, IDisposable
{
    public readonly WindowManager WindowManager;

    public SimpleWindow(WindowManager windowManager, string name) : base(name, ImGuiWindowFlags.None, false)
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
        OnClose();
        WindowManager.RemoveWindow(this);
        GC.SuppressFinalize(this);
    }
}
