using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    private readonly GlobalScaleObserver _globalScaleObserver;
    private readonly WindowSystem _windowSystem;

    public WindowManager(ILogger<WindowManager> logger, IDalamudPluginInterface pluginInterface, GlobalScaleObserver globalScaleObserver)
    {
        _logger = logger;
        _pluginInterface = pluginInterface;
        _globalScaleObserver = globalScaleObserver;

        _windowSystem = new(_pluginInterface.InternalName);

        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _globalScaleObserver.ScaleChange += OnScaleChange;
    }

    void IDisposable.Dispose()
    {
        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _globalScaleObserver.ScaleChange -= OnScaleChange;

        _windowSystem.Windows.OfType<IDisposable>().ForEach(window => window.Dispose());
        _windowSystem.RemoveAllWindows();
    }

    private void OnScaleChange(float scale)
    {
        foreach (var window in _windowSystem.Windows.OfType<SimpleWindow>())
        {
            try
            {
                window.OnScaleChange(scale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while propagating scale change");
            }
        }
    }

    public bool TryGetWindow<T>([NotNullWhen(returnValue: true)] out T? outWindow) where T : Window
    {
        outWindow = null;

        foreach (var window in _windowSystem.Windows)
        {
            if (window is not T typedWindow)
                continue;

            outWindow = typedWindow;
            return true;
        }

        return false;
    }

    public bool TryFindWindow<T>(Predicate<Window> predicate, [NotNullWhen(returnValue: true)] out T? outWindow) where T : Window
    {
        outWindow = null;

        foreach (var window in _windowSystem.Windows)
        {
            if (window is not T typedWindow)
                continue;

            if (!predicate(window))
                continue;

            outWindow = typedWindow;
            return true;
        }

        return false;
    }

    public bool TryGetWindow<T>(string windowName, [NotNullWhen(returnValue: true)] out T? outWindow) where T : Window
    {
        return TryFindWindow<T>(win => win.WindowName == windowName, out outWindow);
    }

    public T CreateOrOpen<T>() where T : SimpleWindow
    {
        return CreateOrOpen(Service.Get<T>);
    }

    public T CreateOrOpen<T>(Func<T> factory) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(out var window))
            _windowSystem.AddWindow(window = factory());

        window.Open();
        return window;
    }

    public T CreateOrOpen<T>(string windowName, Func<T> factory) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(windowName, out var window))
            _windowSystem.AddWindow(window = factory());

        window.Open();
        return window;
    }

    public T CreateOrToggle<T>() where T : SimpleWindow
    {
        return CreateOrToggle(Service.Get<T>);
    }

    public T CreateOrToggle<T>(Func<T> factory) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(out var window))
        {
            _windowSystem.AddWindow(window = factory());
            window.Open();
        }
        else
        {
            window.Toggle();
        }

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

    public bool Contains(Predicate<Window> predicate)
    {
        return _windowSystem.Windows.Any(win => predicate(win));
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
