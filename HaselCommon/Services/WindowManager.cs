using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using HaselCommon.Extensions;

namespace HaselCommon.Services;

public class WindowManager : IDisposable
{
    private readonly DalamudPluginInterface PluginInterface;
    private readonly WindowSystem WindowSystem;

    public WindowManager(DalamudPluginInterface pluginInterface)
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

    public bool AddWindow(Window window)
    {
        if (WindowSystem.Windows.Contains(window))
            return false;

        WindowSystem.AddWindow(window);
        return true;
    }

    public bool RemoveWindow(Window window)
    {
        if (!WindowSystem.Windows.Contains(window))
            return false;

        WindowSystem.RemoveWindow(window);
        return true;
    }
}
