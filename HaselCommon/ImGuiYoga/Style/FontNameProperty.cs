namespace HaselCommon.ImGuiYoga.Style;

public class FontNameProperty(Node OwnerNode) : IInheritableProperty<string>
{
    protected string? _value;
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

    public void SetValue(string? value)
    {
        _value = value;
        _isInherited = false;
        SetStyleDirty();
    }

    public string Resolve()
    {
        if (_isInherited)
        {
            if (OwnerNode.Parent != null)
                return OwnerNode.Parent.Style.FontName.Resolve();

            return "axis";
        }

        return _value ?? "axis";
    }
}
