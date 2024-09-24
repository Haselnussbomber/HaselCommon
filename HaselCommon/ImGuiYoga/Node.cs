using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.ImGuiYoga.Attributes;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

// TODO: dirty flags for style and font
public unsafe partial class Node : EventTarget
{
    public Guid Guid { get; } = Guid.NewGuid();
    public virtual string TagName => "div";
    public virtual string DisplayName => !string.IsNullOrEmpty(Id) ? $"#{Id}" : ClassList.Count > 0 ? ClassList.ToString() : string.Empty;
    public virtual string AsHtmlOpenTag => $"<{TagName}{(Attributes.Count > 0 ? $" {Attributes}" : string.Empty)}{(Count == 0 ? " /" : string.Empty)}>";

    private Type CachedType { get; }
    public string TypeName => CachedType.Name;
    public bool IsSetupComplete { get; private set; }

    public Node? Parent { get; internal set; }

    public NodeStyle Style { get; }
    public ClassList ClassList { get; }
    public AttributeMap Attributes { get; }

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

    public Node()
    {
        CachedType = GetType();
        Style = new NodeStyle(this);
        ClassList = new ClassList(this);
        Attributes = new AttributeMap(this);
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

        UpdateChildNodes();
        UpdateRefs();
    }

    protected void UpdateRefs()
    {
        if (!IsDirty) return;

        GetDocument()?.Logger?.LogDebug("Updating refs of {node}", Guid.ToString());

        foreach (var propInfo in CachedType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (propInfo.GetCustomAttribute<NodeRefAttribute>() is NodeRefAttribute refAttr)
            {
                propInfo.SetValue(this, QuerySelector(refAttr.Selector));
            }
        }

        foreach (var fieldInfo in CachedType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (fieldInfo.GetCustomAttribute<NodeRefAttribute>() is NodeRefAttribute refAttr)
            {
                fieldInfo.SetValue(this, QuerySelector(refAttr.Selector));
            }
        }
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

        if (Style.Overflow is YGOverflow.Scroll or YGOverflow.Hidden)
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
            Style.PaddingRight = Style.Overflow == YGOverflow.Scroll && HadOverflow
                ? ImGui.GetStyle().ScrollbarSize
                : 0;

            ImGui.BeginChildFrame(
                ImGui.GetID("##__ChildFrame"),
                ComputedSize,
                (Style.Overflow == YGOverflow.Scroll ? ImGuiWindowFlags.HorizontalScrollbar : ImGuiWindowFlags.None) | ImGuiWindowFlags.NoSavedSettings
            );
        }

        HandleMouse();
    }

    private void PostDraw()
    {
        if (Style.Overflow is YGOverflow.Scroll or YGOverflow.Hidden)
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
}
