using System.Diagnostics.CodeAnalysis;
using Dalamud.Interface.Windowing;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class WindowManager : IDisposable
{
    private readonly ILogger<WindowManager> _logger;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly LanguageProvider _languageProvider;
    private readonly GlobalScaleObserver _globalScaleObserver;

    private WindowSystem _windowSystem;
    private bool _isDisposing;

    [AutoPostConstruct]
    private void Initialize()
    {
        _windowSystem = new(_pluginInterface.InternalName);

        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;

        _languageProvider.LanguageChanged += OnLanguageChanged;
        _globalScaleObserver.ScaleChanged += OnScaleChanged;
    }

    void IDisposable.Dispose()
    {
        _isDisposing = true;

        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;

        _languageProvider.LanguageChanged -= OnLanguageChanged;
        _globalScaleObserver.ScaleChanged -= OnScaleChanged;

        lock (_windowSystem)
        {
            _windowSystem.Windows.OfType<IDisposable>().ForEach(window => window.Dispose());
            _windowSystem.RemoveAllWindows();
        }
    }

    private void OnLanguageChanged(string langCode)
    {
        foreach (var window in _windowSystem.Windows.OfType<SimpleWindow>())
        {
            try
            {
                window.OnLanguageChanged(langCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while propagating language change");
            }
        }
    }

    private void OnScaleChanged(float scale)
    {
        foreach (var window in _windowSystem.Windows.OfType<SimpleWindow>())
        {
            try
            {
                window.OnScaleChanged(scale);
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

    public T CreateOrOpen<T>(bool focus = true) where T : SimpleWindow
    {
        return CreateOrOpen(Service.Get<T>, focus);
    }

    public T CreateOrOpen<T>(Func<T> factory, bool focus = true) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(out var window))
            _windowSystem.AddWindow(window = factory());

        window.Open(focus);
        return window;
    }

    public T CreateOrOpen<T>(string windowName, Func<T> factory, bool focus = true) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(windowName, out var window))
            _windowSystem.AddWindow(window = factory());

        window.Open(focus);
        return window;
    }

    public T CreateOrToggle<T>(bool focus = true) where T : SimpleWindow
    {
        return CreateOrToggle(Service.Get<T>, focus);
    }

    public T CreateOrToggle<T>(Func<T> factory, bool focus = true) where T : SimpleWindow
    {
        if (!TryGetWindow<T>(out var window))
        {
            _windowSystem.AddWindow(window = factory());
            window.Open(focus);
        }
        else
        {
            window.Toggle(focus);
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
        if (_isDisposing)
            return;

        if (TryGetWindow<SimpleWindow>(windowName, out var window))
            RemoveWindow(window);
    }

    public void Close<T>() where T : SimpleWindow
    {
        if (_isDisposing)
            return;

        _windowSystem.Windows.OfType<T>().ForEach(window => window.Close());
    }

    public bool RemoveWindow(Window window)
    {
        if (_isDisposing)
            return false;

        if (!_windowSystem.Windows.Contains(window))
            return false;

        _windowSystem.RemoveWindow(window);
        return true;
    }
}
