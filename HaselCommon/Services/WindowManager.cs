using System.Linq;
using Dalamud.Interface.Windowing;
using HaselCommon.Extensions;

namespace HaselCommon.Services;

public class WindowManager : WindowSystem, IDisposable
{
    public WindowManager() : base(Service.PluginInterface.InternalName)
    {
        Service.PluginInterface.UiBuilder.Draw += Draw;
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= Draw;
        Windows.OfType<IDisposable>().ForEach(window => window.Dispose());
        RemoveAllWindows();
    }

    public T? GetWindow<T>() where T : Window
    {
        return Windows.OfType<T>().FirstOrDefault();
    }

    public bool IsWindowOpen<T>() where T : Window
    {
        return GetWindow<T>() is not null;
    }

    public T OpenWindow<T>() where T : Window, new()
    {
        var window = GetWindow<T>();
        if (window is null)
        {
            AddWindow(window = new T());
        }

        window.IsOpen = true;

        return window;
    }

    public void CloseWindow<T>() where T : Window
    {
        var window = GetWindow<T>();
        if (window is null)
            return;

        (window as IDisposable)?.Dispose();
        RemoveWindow(window);
    }

    public void ToggleWindow<T>() where T : Window, new()
    {
        if (GetWindow<T>() is null)
        {
            OpenWindow<T>();
        }
        else
        {
            CloseWindow<T>();
        }
    }
}
