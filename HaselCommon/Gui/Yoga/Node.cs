using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using HaselCommon.Gui.Yoga.Attributes;
using ImGuiNET;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public partial class Node : IDisposable
{
    private bool _isDisposed;
    private bool _scrollbarPaddingApplied;

    [NodeProp("Node")]
    public Guid Guid { get; } = Guid.NewGuid();

    public unsafe Node()
    {
        NodeRegistry.TryAdd((nint)_yogaNode, this);
    }

    ~Node()
    {
        Dispose();
    }

    public virtual void Dispose()
    {
        if (_isDisposed)
            return;

        foreach (var child in this)
            child.Dispose();

        Clear();

        unsafe
        {
            if (HasBaselineFunc)
                EnableBaselineFunc = false;

            if (HasMeasureFunc)
                EnableMeasureFunc = false;

            NodeRegistry.TryRemove((nint)_yogaNode, out _);
            _yogaNode->FreeRecursive();
        }

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void Update()
    {
        if (HasNewLayout)
        {
            UpdateScrollbarPadding();
            ApplyLayout();
            HasNewLayout = false;
        }

        ProcessEvents();
        UpdateContent();

        foreach (var child in this)
        {
            child.Update();
        }
    }

    private void UpdateScrollbarPadding()
    {
        if (Overflow == YGOverflow.Scroll)
        {
            if (HadOverflow && !_scrollbarPaddingApplied)
            {
                if (PaddingRight.Unit is YGUnit.Point or YGUnit.Percent && PaddingRight.Value != 0)
                {
                    Service.Get<IPluginLog>().Warning("Scrollable node {guid} has PaddingRight set. PaddingRight will be overwritten for the scrollbar.", Guid.ToString());
                }
                else if (PaddingHorizontal.Unit is YGUnit.Point or YGUnit.Percent && Padding.Value != 0)
                {
                    Service.Get<IPluginLog>().Warning("Scrollable node {guid} has PaddingHorizontal set. PaddingRight for the scrollbar takes precedence.", Guid.ToString());
                }
                else if (Padding.Unit is YGUnit.Point or YGUnit.Percent && Padding.Value != 0)
                {
                    Service.Get<IPluginLog>().Warning("Scrollable node {guid} has Padding set. PaddingRight for the scrollbar will not be respected.", Guid.ToString());
                }

                PaddingRight = ImGui.GetStyle().ScrollbarSize + ImGui.GetStyle().DisplaySafeAreaPadding.X;
                _scrollbarPaddingApplied = true;
            }
            else if (!HadOverflow && _scrollbarPaddingApplied)
            {
                PaddingRight = YGValue.Undefined;
                _scrollbarPaddingApplied = false;
            }
        }
        else if (_scrollbarPaddingApplied)
        {
            PaddingRight = YGValue.Undefined;
            _scrollbarPaddingApplied = false;
        }
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Guid.ToString());

        var pos = AbsolutePosition + new Vector2(ComputedBorderLeft + ComputedPaddingLeft, ComputedBorderTop + ComputedPaddingTop);
        var size = ComputedSize - new Vector2(ComputedBorderLeft + ComputedBorderRight, ComputedBorderTop + ComputedBorderBottom);

        // to make sure ImGui knows about the size of this node
        ImGui.SetCursorPos(pos);
        ImGui.Dummy(size);

        ImGui.SetCursorPos(pos);

        using var scrollContainer = Overflow is YGOverflow.Scroll or YGOverflow.Hidden
            ? ImRaii.Child(
                Guid.ToString() + "_ScrollContainer",
                size,
                false,
                Overflow == YGOverflow.Hidden
                    ? ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse
                    : ImGuiWindowFlags.None)
            : null;

        if (ImGuiUtils.IsInViewport(size))
        {
            DrawContent();
        }

        foreach (var child in this)
        {
            if (child.Display != YGDisplay.None)
            {
                child.Draw();
            }
        }

        scrollContainer?.Dispose();

        DrawDebugHighlight();
    }

    /// <remarks>
    /// This function is called when the layout changed.
    /// </remarks>
    public virtual void ApplyLayout()
    {

    }

    /// <remarks>
    /// This function is called when the global font scale changed.
    /// </remarks>
    public virtual void ApplyGlobalScale(float globalFontScale)
    {

    }

    /// <summary>
    /// A custom function for determining the text baseline for use in baseline alignment.
    /// </summary>
    /// <remarks>
    /// To enable this, set <see cref="HasBaselineFunc"/> to <c>true</c>.
    /// </remarks>
    public virtual float Baseline(float width, float height)
    {
        throw new NotImplementedException("Baseline function was not implemented");
    }

    /// <summary>
    /// Allows providing custom measurements for a Yoga leaf node (usually for measuring text).<br/>
    /// <see cref="IsDirty"/> must be set if content effecting the measurements of the node changes.
    /// </summary>
    /// <remarks>
    /// To enable this, set <see cref="HasMeasureFunc"/> to <c>true</c>.
    /// </remarks>
    public virtual Vector2 Measure(float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode)
    {
        throw new NotImplementedException("Measure function was not implemented");
    }

    /// <remarks>
    /// This function is called before the layout is calculated.
    /// </remarks>
    public virtual void UpdateContent()
    {

    }

    /// <remarks>
    /// This function is called to draw the content on the screen.
    /// </remarks>
    public virtual void DrawContent()
    {

    }
}
