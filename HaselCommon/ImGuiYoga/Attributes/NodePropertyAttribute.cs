namespace HaselCommon.ImGuiYoga.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NodePropertyAttribute(string? attrName = null, bool useChildrenInnerText = false) : Attribute
{
    public string? AttrName { get; } = attrName;
    public bool UseChildrenInnerText { get; } = useChildrenInnerText;
}
