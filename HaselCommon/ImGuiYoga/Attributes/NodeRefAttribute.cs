namespace HaselCommon.ImGuiYoga.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class NodeRefAttribute(string selector) : Attribute
{
    public string Selector { get; } = selector;
}
