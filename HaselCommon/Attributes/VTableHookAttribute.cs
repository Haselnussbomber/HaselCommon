namespace HaselCommon.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class VTableHookAttribute<T>(int index) : HookAttribute
{
    public int Index { get; } = index;
}
