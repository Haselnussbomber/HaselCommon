using ImGuiNET;

namespace HaselCommon.ImGuiYoga.Style;

public class FontSizeProperty(Node OwnerNode) : IInheritableProperty<float>
{
    protected float? _value;
    protected bool _isInherited;

    private void SetStyleDirty()
    {
        OwnerNode.GetDocument()?.SetStyleDirty();
    }

    public void SetInherited()
    {
        _value = null;
        _isInherited = true;
        SetStyleDirty();
    }

    public void SetValue(float value)
    {
        _value = float.IsNaN(value) ? null : value;
        _isInherited = false;
        SetStyleDirty();
    }

    public float Resolve()
    {
        if (_isInherited)
        {
            if (OwnerNode.Parent != null)
                return OwnerNode.Parent.Style.FontSize.Resolve();

            return ImGui.GetFontSize();
        }

        return _value ?? ImGui.GetFontSize();
    }
}
