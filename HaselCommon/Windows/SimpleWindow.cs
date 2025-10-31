using Dalamud.Interface.Windowing;
using Lumina.Misc;

namespace HaselCommon.Windows;

#pragma warning disable IDE0032 // Use auto property

public abstract partial class SimpleWindow : Window, IDisposable
{
    private readonly WindowManager _windowManager;
    private readonly TextService _textService;
    private readonly AddonObserver _addonObserver;
    private readonly ImRaii.Color _colors = new();
    private readonly ImRaii.Style _styles = new();

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

    public bool EnableColorReset { get; set; } = true;
    public bool EnableStyleReset { get; set; } = true;

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
        _colors.Dispose();
        _styles.Dispose();
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

    public override void PreDraw()
    {
        base.PreDraw();

        if (EnableColorReset)
        {
            // ImGui::StyleColorsDark + DalamudStandard
            // https://github.com/ocornut/imgui/blob/v1.88/imgui_draw.cpp#L190
            // https://github.com/goatcorp/Dalamud/blob/13.0.0.7/Dalamud/Interface/Style/StyleModelV1.cs#L60

            _colors
                .Push(ImGuiCol.Text, Vector4.One)
                .Push(ImGuiCol.TextDisabled, new Vector4(0.5f, 0.5f, 0.5f, 1.0f))
                .Push(ImGuiCol.WindowBg, new Vector4(0.06f, 0.06f, 0.06f, 0.93f))
                .Push(ImGuiCol.ChildBg, Vector4.Zero)
                .Push(ImGuiCol.PopupBg, new Vector4(0.08f, 0.08f, 0.08f, 0.94f))
                .Push(ImGuiCol.Border, new Vector4(0.43f, 0.43f, 0.50f, 0.50f))
                .Push(ImGuiCol.BorderShadow, Vector4.Zero)
                .Push(ImGuiCol.FrameBg, new Vector4(0.29f, 0.29f, 0.29f, 0.54f))
                .Push(ImGuiCol.FrameBgHovered, new Vector4(0.54f, 0.54f, 0.54f, 0.4f))
                .Push(ImGuiCol.FrameBgActive, new Vector4(0.64f, 0.64f, 0.64f, 0.67f))
                .Push(ImGuiCol.TitleBg, new Vector4(0.022624433f, 0.022624206f, 0.022624206f, 0.85067874f))
                .Push(ImGuiCol.TitleBgActive, new Vector4(0.38914025f, 0.10917056f, 0.10917056f, 0.8280543f))
                .Push(ImGuiCol.TitleBgCollapsed, new Vector4(0.00f, 0.00f, 0.00f, 0.51f))
                .Push(ImGuiCol.MenuBarBg, new Vector4(0.14f, 0.14f, 0.14f, 1.00f))
                .Push(ImGuiCol.ScrollbarBg, Vector4.Zero)
                .Push(ImGuiCol.ScrollbarGrab, new Vector4(0.31f, 0.31f, 0.31f, 1.00f))
                .Push(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.41f, 0.41f, 0.41f, 1.00f))
                .Push(ImGuiCol.ScrollbarGrabActive, new Vector4(0.51f, 0.51f, 0.51f, 1.00f))
                .Push(ImGuiCol.CheckMark, new Vector4(0.86f, 0.86f, 0.86f, 1.00f))
                .Push(ImGuiCol.SliderGrab, new Vector4(0.54f, 0.54f, 0.54f, 1.00f))
                .Push(ImGuiCol.SliderGrabActive, new Vector4(0.67f, 0.67f, 0.67f, 1.00f))
                .Push(ImGuiCol.Button, new Vector4(0.71f, 0.71f, 0.71f, 0.40f))
                .Push(ImGuiCol.ButtonHovered, new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f))
                .Push(ImGuiCol.ButtonActive, new Vector4(0.48416287f, 0.10077597f, 0.10077597f, 0.94509804f))
                .Push(ImGuiCol.Header, new Vector4(0.59f, 0.59f, 0.59f, 0.31f))
                .Push(ImGuiCol.HeaderHovered, new Vector4(0.5f, 0.5f, 0.5f, 0.8f))
                .Push(ImGuiCol.HeaderActive, new Vector4(0.6f, 0.6f, 0.6f, 1.00f))
                .Push(ImGuiCol.Separator, new Vector4(0.43f, 0.43f, 0.50f, 0.50f))
                .Push(ImGuiCol.SeparatorHovered, new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.78280544f))
                .Push(ImGuiCol.SeparatorActive, new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f))
                .Push(ImGuiCol.ResizeGrip, new Vector4(0.79f, 0.79f, 0.79f, 0.25f))
                .Push(ImGuiCol.ResizeGripHovered, new Vector4(0.78f, 0.78f, 0.78f, 0.67f))
                .Push(ImGuiCol.ResizeGripActive, new Vector4(0.3647059f, 0.078431375f, 0.078431375f, 0.94509804f))
                .Push(ImGuiCol.Tab, new Vector4(0.23f, 0.23f, 0.23f, 0.86f))
                .Push(ImGuiCol.TabHovered, new Vector4(0.58371043f, 0.30374074f, 0.30374074f, 0.7647059f))
                .Push(ImGuiCol.TabActive, new Vector4(0.47963798f, 0.15843244f, 0.15843244f, 0.7647059f))
                .Push(ImGuiCol.TabUnfocused, new Vector4(0.068f, 0.10199998f, 0.14800003f, 0.9724f))
                .Push(ImGuiCol.TabUnfocusedActive, new Vector4(0.13599998f, 0.26199996f, 0.424f, 1))
                .Push(ImGuiCol.DockingPreview, new Vector4(0.26f, 0.59f, 0.98f, 0.70f))
                .Push(ImGuiCol.DockingEmptyBg, new Vector4(0.20f, 0.20f, 0.20f, 1.00f))
                .Push(ImGuiCol.PlotLines, new Vector4(0.61f, 0.61f, 0.61f, 1.00f))
                .Push(ImGuiCol.PlotLinesHovered, new Vector4(1.00f, 0.43f, 0.35f, 1.00f))
                .Push(ImGuiCol.PlotHistogram, new Vector4(0.578199f, 0.16989735f, 0.16989735f, 0.78431374f))
                .Push(ImGuiCol.PlotHistogramHovered, new Vector4(0.7819905f, 0.12230185f, 0.12230185f, 0.78431374f))
                .Push(ImGuiCol.TableHeaderBg, new Vector4(0.19f, 0.19f, 0.20f, 1.00f))
                .Push(ImGuiCol.TableBorderStrong, new Vector4(0.31f, 0.31f, 0.35f, 1.00f))
                .Push(ImGuiCol.TableBorderLight, new Vector4(0.23f, 0.23f, 0.25f, 1.00f))
                .Push(ImGuiCol.TableRowBg, Vector4.Zero)
                .Push(ImGuiCol.TableRowBgAlt, new Vector4(1.00f, 1.00f, 1.00f, 0.06f))
                .Push(ImGuiCol.TextSelectedBg, new Vector4(0.26f, 0.59f, 0.98f, 0.35f))
                .Push(ImGuiCol.DragDropTarget, new Vector4(1.00f, 1.00f, 0.00f, 0.90f))
                .Push(ImGuiCol.NavHighlight, new Vector4(0.26f, 0.59f, 0.98f, 1.00f))
                .Push(ImGuiCol.NavWindowingHighlight, new Vector4(1.00f, 1.00f, 1.00f, 0.70f))
                .Push(ImGuiCol.NavWindowingDimBg, new Vector4(0.80f, 0.80f, 0.80f, 0.20f))
                .Push(ImGuiCol.ModalWindowDimBg, new Vector4(0.80f, 0.80f, 0.80f, 0.35f));
        }

        if (EnableStyleReset)
        {
            // ImGuiStyle::ImGuiStyle + DalamudStandard
            // https://github.com/ocornut/imgui/blob/v1.88/imgui.cpp#L1041
            // https://github.com/goatcorp/Dalamud/blob/13.0.0.7/Dalamud/Interface/Style/StyleModelV1.cs#L31

            _styles
                .Push(ImGuiStyleVar.Alpha, 1.0f)
                .Push(ImGuiStyleVar.DisabledAlpha, 0.60f)
                .Push(ImGuiStyleVar.WindowPadding, new Vector2(8, 8))
                .Push(ImGuiStyleVar.WindowRounding, 4f)
                .Push(ImGuiStyleVar.WindowBorderSize, 0f)
                .Push(ImGuiStyleVar.WindowMinSize, new Vector2(32, 32))
                .Push(ImGuiStyleVar.WindowTitleAlign, new Vector2(0, 0.5f))
                .Push(ImGuiStyleVar.ChildRounding, 0.0f)
                .Push(ImGuiStyleVar.ChildBorderSize, 1.0f)
                .Push(ImGuiStyleVar.PopupRounding, 0.0f)
                .Push(ImGuiStyleVar.PopupBorderSize, 0.0f)
                .Push(ImGuiStyleVar.FramePadding, new Vector2(4, 3))
                .Push(ImGuiStyleVar.FrameRounding, 4.0f)
                .Push(ImGuiStyleVar.FrameBorderSize, 0.0f)
                .Push(ImGuiStyleVar.ItemSpacing, new Vector2(8, 4))
                .Push(ImGuiStyleVar.ItemInnerSpacing, new Vector2(4, 4))
                .Push(ImGuiStyleVar.CellPadding, new Vector2(4, 2))
                .Push(ImGuiStyleVar.IndentSpacing, 21.0f)
                .Push(ImGuiStyleVar.ScrollbarSize, 16.0f)
                .Push(ImGuiStyleVar.ScrollbarRounding, 9.0f)
                .Push(ImGuiStyleVar.GrabMinSize, 10.0f)
                .Push(ImGuiStyleVar.GrabRounding, 3.0f)
                .Push(ImGuiStyleVar.TabRounding, 4.0f)
                .Push(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f))
                .Push(ImGuiStyleVar.SelectableTextAlign, Vector2.Zero);
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();

        if (Collapsed != null)
        {
            Collapsed = null;
        }

        _colors.Dispose();
        _styles.Dispose();
    }

    public virtual void OnLanguageChanged(string langCode)
    {
        UpdateWindowName();
    }

    public virtual void OnScaleChanged(float scale)
    {
    }
}
