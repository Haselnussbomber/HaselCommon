using MSServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

namespace HaselCommon.DependencyInjection;

public class ServiceCollection : MSServiceCollection
{
    public List<Func<IServiceProvider, IServiceResolver>> ResolverFactories { get; } = [];
    public List<IServiceProvider> FallbackProviders { get; } = [];

    public ServiceProvider BuildServiceProvider() => new(this);
}
