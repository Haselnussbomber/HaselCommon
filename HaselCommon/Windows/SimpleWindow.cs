using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Misc;

namespace HaselCommon.Windows;

public abstract partial class SimpleWindow : Window, IDisposable
{
    private readonly WindowManager _windowManager;
    private readonly TextService _textService;

    public readonly ImRaii.Style WindowStyle = new();
    public readonly ImRaii.Color WindowColor = new();

    public string WindowNameKey
    {
        get;
        set
        {
            field = value;
            UpdateWindowName();
        }
    } = string.Empty;

    protected SimpleWindow(WindowManager windowManager, TextService textService) : base("SimpleWindow", ImGuiWindowFlags.NoFocusOnAppearing, false)
    {
        _windowManager = windowManager;
        _textService = textService;

        WindowNameKey = $"{GetType().Name}.Title";

        _windowManager.AddWindow(this);
    }

    public virtual void Dispose()
    {
        WindowStyle.Dispose();
        WindowColor.Dispose();
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

    public override void OnClose()
    {
        WindowStyle.Dispose();
        WindowColor.Dispose();
        base.OnClose();
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

    public override unsafe bool DrawConditions()
    {
        return AtkStage.Instance()->Filter.NumActiveFilters == 0;
    }

    public override void PostDraw()
    {
        WindowStyle.Dispose();
        WindowColor.Dispose();

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
