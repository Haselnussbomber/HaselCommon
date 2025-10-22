using Dalamud.Interface.Windowing;
using Lumina.Misc;

namespace HaselCommon.Windows;

#pragma warning disable IDE0032 // Use auto property

public abstract partial class SimpleWindow : Window, IDisposable
{
    private readonly WindowManager _windowManager;
    private readonly TextService _textService;
    private readonly AddonObserver _addonObserver;

    private string _windowNameKey = string.Empty;

    public string WindowNameKey
    {
        get => _windowNameKey;
        set
        {
            _windowNameKey = value;
            UpdateWindowName();
        }
    }

    protected SimpleWindow(WindowManager windowManager, TextService textService, AddonObserver addonObserver) : base("SimpleWindow", ImGuiWindowFlags.NoFocusOnAppearing, false)
    {
        _windowManager = windowManager;
        _textService = textService;
        _addonObserver = addonObserver;

        WindowNameKey = $"{GetType().Name}.Title";

        _windowManager.AddWindow(this);
    }

    public virtual void Dispose()
    {
        Close();
        _windowManager.RemoveWindow(this);
    }

    protected void UpdateWindowName()
    {
        if (!string.IsNullOrEmpty(WindowNameKey))
        {
            WindowName = $"{_textService.Translate(WindowNameKey)}###{GetType().FullName}_{Crc32.Get(WindowNameKey):X}";
        }
    }

    public void Open(bool focus = true)
    {
        IsOpen = true;
        Collapsed = false;

        if (focus)
        {
            BringToFront();
        }
    }

    public void Close()
    {
        IsOpen = false;
    }

    public new void Toggle()
    {
        Toggle(true);
    }

    public void Toggle(bool focus = true)
    {
        if (!IsOpen)
        {
            Open(focus);
        }
        else
        {
            Close();
        }
    }

    public override bool DrawConditions()
    {
        return !_addonObserver.IsAddonVisible("Filter");
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Collapsed != null)
        {
            Collapsed = null;
        }
    }

    public virtual void OnLanguageChanged(string langCode)
    {
        UpdateWindowName();
    }

    public virtual void OnScaleChanged(float scale)
    {
    }
}
