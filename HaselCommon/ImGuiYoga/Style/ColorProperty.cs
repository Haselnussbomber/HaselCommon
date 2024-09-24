using HaselCommon.Utils;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga.Style;

public class ColorProperty(Node OwnerNode) : IInheritableProperty<HaselColor>
{
    protected HaselColor? _value;
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

    public void SetValue(HaselColor value)
    {
        _value = value;
        _isInherited = false;
        SetStyleDirty();
    }

    public HaselColor Resolve()
    {
        if (_isInherited)
        {
            if (OwnerNode.Parent != null)
                return OwnerNode.Parent.Style.Color.Resolve();

            return HaselColor.From(ImGuiCol.Text);
        }

        return _value ?? HaselColor.From(ImGuiCol.Text);
    }
}
