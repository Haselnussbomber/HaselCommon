namespace HaselCommon.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFallbackProvider(this IServiceCollection services, IServiceProvider fallback)
    {
        if (services is ServiceCollection sc)
            sc.FallbackProviders.Add(fallback);

        return services;
    }

    public static IServiceCollection AddResolver(this IServiceCollection services, Func<IServiceProvider, IServiceResolver> factory)
    {
        if (services is ServiceCollection sc)
            sc.ResolverFactories.Add(factory);

        return services;
    }
}
