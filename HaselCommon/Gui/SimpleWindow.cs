using Dalamud.Interface.Windowing;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Misc;

namespace HaselCommon.Gui;

public abstract class SimpleWindow : Window, IDisposable
{
    private readonly WindowManager _windowManager;
    private readonly TextService _textService;
    private readonly LanguageProvider _languageProvider;
    private readonly Lazy<AddonObserver> _addonObserver = new(Service.Get<AddonObserver>);

    private string _windowNameKey = string.Empty;

    public string WindowNameKey
    {
        get { return _windowNameKey; }
        set
        {
            _windowNameKey = value;
            UpdateWindowName();
        }
    }

    protected SimpleWindow(WindowManager windowManager, TextService textService, LanguageProvider languageProvider) : base("SimpleWindow", ImGuiWindowFlags.None, false)
    {
        _windowManager = windowManager;
        _textService = textService;
        _languageProvider = languageProvider;

        var type = GetType();
        Namespace = type.Namespace;
        WindowNameKey = $"{type.Name}.Title";

        languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public virtual void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
        Close();
    }

    private void OnLanguageChanged(string langCode)
    {
        UpdateWindowName();
    }

    private void UpdateWindowName()
    {
        if (!string.IsNullOrEmpty(_windowNameKey))
        {
            WindowName = $"{_textService.Translate(_windowNameKey)}###{GetType().FullName}_{Crc32.Get(_windowNameKey):X}";
        }
    }

    public void Open()
    {
        _windowManager.AddWindow(this);
        IsOpen = true;
        Collapsed = false;
        BringToFront();
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

    public override void OnClose()
    {
        _windowManager.RemoveWindow(this);
    }

    public override bool DrawConditions()
    {
        return !_addonObserver.Value.IsAddonVisible("Filter");
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Collapsed != null)
            Collapsed = null;
    }

    public virtual void OnScaleChange(float scale) { }
}
