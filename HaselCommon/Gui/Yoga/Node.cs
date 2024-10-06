using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Gui.Yoga.Attributes;
using HaselCommon.Gui.Yoga.Enums;
using ImGuiNET;

namespace HaselCommon.Gui.Yoga;

public partial class Node : IDisposable
{
    private bool _isDisposed;

    [NodeProp("Node")]
    public Guid Guid { get; } = Guid.NewGuid();

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

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void Update()
    {
        ProcessEvents();
        UpdateContent();

        foreach (var child in this)
        {
            child.Update();
        }
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Guid.ToString());

        ImGui.SetCursorPos(AbsolutePosition + new Vector2(
            ComputedBorderLeft + ComputedPaddingLeft,
            ComputedBorderTop + ComputedPaddingTop));

        DrawDebugBefore();
        DrawContent();
        DrawDebugAfter();

        foreach (var child in this)
        {
            if (child.Display != Display.None)
            {
                child.Draw();
            }
        }
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
    public virtual Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode)
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
