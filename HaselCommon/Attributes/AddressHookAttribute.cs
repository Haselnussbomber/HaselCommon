namespace HaselCommon.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AddressHookAttribute(string AddressName) : Attribute
{
    public string AddressName { get; } = AddressName;
}
