using HaselCommon.Enums;
using ImGuiNET;

namespace HaselCommon.ImGuiYoga.Style;

public class CursorProperty(Node OwnerNode) : IInheritableProperty<Cursor>
{
    protected Cursor? _value;
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

    public void SetValue(Cursor value)
    {
        _value = value;
        _isInherited = false;
        SetStyleDirty();
    }

    public Cursor Resolve()
    {
        if (_isInherited)
        {
            if (OwnerNode.Parent != null)
                return OwnerNode.Parent.Style.Cursor.Resolve();

            return Cursor.Pointer;
        }

        return _value ?? Cursor.Pointer;
    }

    public ImGuiMouseCursor ResolveImGuiMouseCursor()
    {
        return Resolve() switch
        {
            Cursor.Hand => ImGuiMouseCursor.Hand,
            Cursor.NotAllowed => ImGuiMouseCursor.NotAllowed,
            Cursor.Text => ImGuiMouseCursor.TextInput,
            Cursor.ResizeNS => ImGuiMouseCursor.ResizeNS,
            Cursor.ResizeEW => ImGuiMouseCursor.ResizeEW,
            _ => ImGuiMouseCursor.Arrow,
        };
    }
}
