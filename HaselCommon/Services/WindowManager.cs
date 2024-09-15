using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using HaselCommon.Extensions;
using HaselCommon.Windowing;

namespace HaselCommon.Services;

public class WindowManager : IDisposable
{
    private readonly IDalamudPluginInterface PluginInterface;
    private readonly WindowSystem WindowSystem;

    public WindowManager(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;

        WindowSystem = new(PluginInterface.InternalName);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
    }

    void IDisposable.Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;

        WindowSystem.Windows.OfType<IDisposable>().ForEach(window => window.Dispose());
        WindowSystem.RemoveAllWindows();
    }

    public bool TryGetWindow<T>(string windowName, [NotNullWhen(returnValue: true)] out T? outWindow) where T : Window
    {
        outWindow = null;

        foreach (var window in WindowSystem.Windows)
        {
            if (window is not T typedWindow)
                continue;

            if (window.WindowName != windowName)
                continue;

            outWindow = typedWindow;
            return true;
        }

        return false;
    }

    public T CreateOrOpen<T>(string windowName, Func<T> factory) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(windowName, out var window))
            WindowSystem.AddWindow(window = factory());

        window.Open();
        return window;
    }

    public T Open<T>(T window) where T : SimpleWindow
    {
        WindowSystem.AddWindow(window);
        window.Open();
        return window;
    }

    public bool AddWindow(Window window)
    {
        if (WindowSystem.Windows.Contains(window))
            return false;

        WindowSystem.AddWindow(window);
        return true;
    }

    public bool Contains(string windowName)
    {
        return WindowSystem.Windows.Any(win => win.WindowName == windowName);
    }

    public bool Contains(Window window)
    {
        return WindowSystem.Windows.Contains(window);
    }

    public void RemoveWindow(string windowName)
    {
        if (TryGetWindow<SimpleWindow>(windowName, out var window))
            RemoveWindow(window);
    }

    public void Close<T>() where T : SimpleWindow
    {
        WindowSystem.Windows.OfType<T>().ForEach(window => window.Close());
    }

    public bool RemoveWindow(Window window)
    {
        if (!WindowSystem.Windows.Contains(window))
            return false;

        WindowSystem.RemoveWindow(window);
        return true;
    }
}
