namespace HaselCommon.DependencyInjection;

public interface IServiceResolver : IServiceProviderIsService
{
    object? Resolve(Type serviceType);
    IEnumerable<object> ResolveAll(Type serviceType);
}
