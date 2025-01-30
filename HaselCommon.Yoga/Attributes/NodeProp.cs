namespace HaselCommon.Yoga.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class NodeProp(string category, bool editable = false) : Attribute
{
    public string Category { get; } = category;
    public bool Editable { get; } = editable;
}
