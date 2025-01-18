using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using HaselCommon.Extensions.Collections;
using HaselCommon.Gui;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterSingleton]
public class WindowManager : IDisposable
{
    private readonly ILogger<WindowManager> _logger;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly WindowSystem _windowSystem;
    private float _globalScale;

    public WindowManager(ILogger<WindowManager> logger, IDalamudPluginInterface pluginInterface)
    {
        _logger = logger;
        _pluginInterface = pluginInterface;

        _windowSystem = new(_pluginInterface.InternalName);

        _pluginInterface.UiBuilder.Draw += Draw;
    }

    private void Draw()
    {
        if (_globalScale != ImGuiHelpers.GlobalScaleSafe)
        {
            OnScaleChange();
            _globalScale = ImGuiHelpers.GlobalScaleSafe;
        }

        _windowSystem.Draw();
    }

    void IDisposable.Dispose()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;

        _windowSystem.Windows.OfType<IDisposable>().ForEach(window => window.Dispose());
        _windowSystem.RemoveAllWindows();

        GC.SuppressFinalize(this);
    }

    private void OnScaleChange()
    {
        foreach (var window in _windowSystem.Windows.OfType<SimpleWindow>())
        {
            try
            {
                window.OnScaleChange(_globalScale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while propagating scale change");
            }
        }
    }

    public bool TryGetWindow<T>(string windowName, [NotNullWhen(returnValue: true)] out T? outWindow) where T : Window
    {
        outWindow = null;

        foreach (var window in _windowSystem.Windows)
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
            _windowSystem.AddWindow(window = factory());

        window.Open();
        return window;
    }

    public T Open<T>(T window) where T : SimpleWindow
    {
        _windowSystem.AddWindow(window);
        window.Open();
        return window;
    }

    public bool AddWindow(Window window)
    {
        if (_windowSystem.Windows.Contains(window))
            return false;

        _windowSystem.AddWindow(window);
        return true;
    }

    public bool Contains(string windowName)
    {
        return _windowSystem.Windows.Any(win => win.WindowName == windowName);
    }

    public bool Contains(Window window)
    {
        return _windowSystem.Windows.Contains(window);
    }

    public void RemoveWindow(string windowName)
    {
        if (TryGetWindow<SimpleWindow>(windowName, out var window))
            RemoveWindow(window);
    }

    public void Close<T>() where T : SimpleWindow
    {
        _windowSystem.Windows.OfType<T>().ForEach(window => window.Close());
    }

    public bool RemoveWindow(Window window)
    {
        if (!_windowSystem.Windows.Contains(window))
            return false;

        _windowSystem.RemoveWindow(window);
        return true;
    }
}
