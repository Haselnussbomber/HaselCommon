namespace HaselCommon.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AddressHookAttribute<T>(string addressName) : HookAttribute
{
    public string AddressName { get; } = addressName;
}
