namespace HaselCommon.DependencyInjection;

public enum DuplicateStrategy
{
    Skip,
    Replace,
    Append,
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RegisterSingletonAttribute : Attribute
{
    public Type? ServiceType { get; set; }
    public string? Factory { get; set; }
    public DuplicateStrategy Duplicate { get; set; } = DuplicateStrategy.Skip;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RegisterSingletonAttribute<TService> : RegisterSingletonAttribute
{
    public RegisterSingletonAttribute()
    {
        ServiceType = typeof(TService);
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RegisterTransientAttribute : Attribute
{
    public Type? ServiceType { get; set; }
    public DuplicateStrategy Duplicate { get; set; } = DuplicateStrategy.Skip;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class RegisterTransientAttribute<TService> : RegisterTransientAttribute
{
    public RegisterTransientAttribute()
    {
        ServiceType = typeof(TService);
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RegisterServicesAttribute : Attribute
{
}
