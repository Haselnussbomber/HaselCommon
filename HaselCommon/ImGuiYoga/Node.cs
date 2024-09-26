using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ExCSS;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga;

public unsafe partial class Node : EventTarget
{
    public Guid Guid { get; } = Guid.NewGuid();
    public virtual string TagName => "div";
    public virtual string DisplayName => !string.IsNullOrEmpty(Id) ? $"#{Id}" : ClassList.Count > 0 ? ClassList.ToString() : Guid.ToString();
    public virtual string AsHtmlOpenTag => $"<{TagName}{(Attributes.Count > 0 ? $" {Attributes}" : string.Empty)}{(Count == 0 ? " /" : string.Empty)}>";

    private Type CachedType { get; }
    public string TypeName => CachedType.Name;
    public bool IsSetupComplete { get; private set; }

    public Node? Parent { get; internal set; }

    public AttributeMap Attributes { get; }

    internal List<(string, StyleDeclaration)> StyleDeclarations { get; } = [];
    public ClassList ClassList { get; }
    public StyleMap Style { get; }
    public ComputedStyle ComputedStyle { get; }

    public string Id
    {
        get => Attributes["id"] ?? string.Empty;
        set => Attributes["id"] = value;
    }

    public string ClassName
    {
        get => Attributes["class"] ?? string.Empty;
        set => Attributes["class"] = value;
    }

    private bool IsDisposed;
    private readonly ImRaii.Style ChildFrameStyle = new();
    private readonly ImRaii.Color ChildFrameColor = new();
    private GCHandle? MeasureFuncHandle;
    private bool UpdateRefsPending;
    internal bool UpdateFontHandlePending;

    public Node()
    {
        CachedType = GetType();
        ComputedStyle = new ComputedStyle(this);
        ClassList = new ClassList(this);
        Attributes = new AttributeMap(this);
        Style = new StyleMap(this);
    }

    ~Node()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (IsDisposed)
            return;

        ChildFrameStyle.Dispose();
        ChildFrameColor.Dispose();
        Clear(); // remove children, but does not dispose them
        MeasureFuncHandle?.Free();
        YGNode->Dispose();
        base.Dispose();
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void DisposeRecursive()
    {
        if (IsDisposed)
            return;

        foreach (var child in this)
        {
            child.DisposeRecursive();
        }

        Dispose();
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public virtual void Setup()
    {
    }

    public virtual void Update()
    {
        if (!IsSetupComplete)
        {
            Setup();
            IsSetupComplete = true;
        }

        if (UpdateRefsPending)
        {
            UpdateRefs();
            UpdateRefsPending = false;
        }

        if (UpdateFontHandlePending)
        {
            UpdateFontHandle();
            UpdateFontHandlePending = false;
        }

        UpdateChildNodes();
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Guid.ToString());
        PreDraw();
        DrawNode();
        DrawChildNodes();
        PostDraw();
    }

    protected virtual void DrawNode()
    {
        // for nodes to implement
    }

    private void PreDraw()
    {
        ImGui.SetCursorPos(CumulativePosition);
        //ImGui.SetCursorPos(new Vector2(ComputedLeft, ComputedTop));

        DrawBackground();

        var overflow = Style["overflow"];
        var isOverflowScroll = overflow == "scroll";
        var isOverflowHidden = overflow == "hidden";

        if (isOverflowScroll || isOverflowHidden)
        {
            ChildFrameStyle
                .Push(ImGuiStyleVar.FramePadding, Vector2.Zero)
                .Push(ImGuiStyleVar.WindowPadding, Vector2.Zero)
                .Push(ImGuiStyleVar.FrameBorderSize, 0)
                //.Push(ImGuiStyleVar.ChildRounding, ComputedStyle.BorderRadius)
                //.Push(ImGuiStyleVar.FrameRounding, ComputedStyle.BorderRadius)
                //.Push(ImGuiStyleVar.ScrollbarSize, 10)
                //.Push(ImGuiStyleVar.ScrollbarRounding, 0)
                .Push(ImGuiStyleVar.ChildBorderSize, 0);

            ChildFrameColor
                .Push(ImGuiCol.FrameBg, 0)
                /*
                .Push(ImGuiCol.ScrollbarBg, ComputedStyle.ScrollbarTrackColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrab, ComputedStyle.ScrollbarThumbColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrabActive, ComputedStyle.ScrollbarThumbActiveColor.ToUInt())
                .Push(ImGuiCol.ScrollbarGrabHovered, ComputedStyle.ScrollbarThumbHoverColor.ToUInt())*/;

            // HACK: abusing PaddingRight here for the scrollbar width. are there any better methods?
            Style["padding-right"] = $"{(isOverflowScroll && HadOverflow ? ImGui.GetStyle().ScrollbarSize : 0)}px";

            ImGui.BeginChildFrame(
                ImGui.GetID("##__ChildFrame"),
                ComputedSize,
                (isOverflowScroll ? ImGuiWindowFlags.HorizontalScrollbar : ImGuiWindowFlags.None) | ImGuiWindowFlags.NoSavedSettings
            );
        }

        HandleMouse();
    }

    private void PostDraw()
    {
        var overflow = Style["overflow"];
        var isOverflowScroll = overflow == "scroll";
        var isOverflowHidden = overflow == "hidden";

        if (isOverflowScroll || isOverflowHidden)
        {
            ImGui.EndChildFrame();

            ChildFrameStyle.Dispose();
            ChildFrameColor.Dispose();
        }

        DrawBorder();

        var paddingBottom = ComputedPaddingBottom;
        if (paddingBottom > 0)
            ImGui.Dummy(new Vector2(0, paddingBottom));
    }

    internal string ResolveStyleValue(string propertyName)
    {
        // inline styles
        if (Style.TryGetValue(propertyName, out var value))
            return value;

        // class styles
        if (StyleDeclarations.Count > 0)
        {
            foreach (var (_, declaration) in StyleDeclarations)
            {
                if (!string.IsNullOrEmpty(declaration[propertyName]))
                {
                    value = declaration[propertyName];
                }
            }
        }

        if (!string.IsNullOrEmpty(value) && value != "inherit")
            return value;

        // properties that can be inherited
        if (propertyName is
            "accent-color" or
            "border-collapse" or
            "caption-side" or
            "caret-color" or
            "clip-rule" or
            "color" or
            "color-interpolation" or
            "color-interpolation-filters" or
            "color-rendering" or
            "color-scheme" or
            "cursor" or
            "direction" or
            "dominant-baseline" or
            "dynamic-range-limit" or
            "empty-cells" or
            "fill" or
            "fill-opacity" or
            "fill-rule" or
            "font-family" or
            "font-feature-settings" or
            "font-kerning" or
            "font-optical-sizing" or
            "font-palette" or
            "font-size" or
            "font-size-adjust" or
            "font-stretch" or
            "font-style" or
            "font-synthesis-small-caps" or
            "font-synthesis-style" or
            "font-synthesis-weight" or
            "font-variant-alternates" or
            "font-variant-caps" or
            "font-variant-east-asian" or
            "font-variant-emoji" or
            "font-variant-ligatures" or
            "font-variant-numeric" or
            "font-variant-position" or
            "font-variation-settings" or
            "font-weight" or
            "forced-color-adjust" or
            "hyphenate-character" or
            "hyphenate-limit-chars" or
            "hyphens" or
            "image-orientation" or
            "image-rendering" or
            "interpolate-size" or
            "letter-spacing" or
            "line-break" or
            "line-height" or
            "list-style-image" or
            "list-style-position" or
            "list-style-type" or
            "marker-end" or
            "marker-mid" or
            "marker-start" or
            "math-depth" or
            "math-shift" or
            "math-style" or
            "orphans" or
            "overflow-wrap" or
            "paint-order" or
            "pointer-events" or
            "quotes" or
            "ruby-align" or
            "ruby-position" or
            "scrollbar-color" or
            "shape-rendering" or
            "speak" or
            "stroke" or
            "stroke-dasharray" or
            "stroke-dashoffset" or
            "stroke-linecap" or
            "stroke-linejoin" or
            "stroke-miterlimit" or
            "stroke-opacity" or
            "stroke-width" or
            "tab-size" or
            "text-align" or
            "text-align-last" or
            "text-anchor" or
            "text-autospace" or
            "text-box-edge" or
            "text-combine-upright" or
            "text-decoration-skip-ink" or
            "text-emphasis-color" or
            "text-emphasis-position" or
            "text-emphasis-style" or
            "text-indent" or
            "text-orientation" or
            "text-rendering" or
            "text-shadow" or
            "text-size-adjust" or
            "text-spacing-trim" or
            "text-transform" or
            "text-underline-offset" or
            "text-underline-position" or
            "text-wrap-mode" or
            "text-wrap-style" or
            "user-select" or
            "visibility" or
            "white-space-collapse" or
            "widows" or
            "word-break" or
            "word-spacing" or
            "writing-mode")
        {
            return Parent?.ResolveStyleValue(propertyName) ?? "initial";
        }

        return "initial";
    }
}
