using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HaselCommon.DependencyInjection;

public class ServiceProvider : IServiceProvider, IServiceProviderIsService, IDisposable, IAsyncDisposable
{
    private static ServiceProvider? Instance;

    private readonly IReadOnlyList<ServiceDescriptor> _descriptors;
    private readonly IReadOnlyList<IServiceResolver> _resolvers;
    private readonly IReadOnlyList<IServiceProvider> _fallbackProviders;
    private readonly ConcurrentDictionary<Type, object> _dynamicSingletons = new();
    private readonly DisposableBag _disposables = [];

    public ServiceProvider(ServiceCollection services)
    {
        Instance = this;

        _descriptors = [.. services];
        _fallbackProviders = [.. services.FallbackProviders];
        _resolvers = services.ResolverFactories.ConvertAll(f => f(this));
    }

    public static T? GetService<T>() where T : notnull
    {
        return Instance != null ? Instance.GetService<T>() : default;
    }

    public static T GetRequiredService<T>() where T : notnull
    {
        return Instance!.GetRequiredService<T>();
    }

    public static bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
    {
        service = default;
        return Instance != null && Instance.TryGetService(out service);
    }

    public bool IsService(Type serviceType)
    {
        if (serviceType == typeof(IServiceProvider) || serviceType == typeof(IServiceProviderIsService))
            return true;

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return IsService(serviceType.GenericTypeArguments[0]);

        if (_resolvers.Any(r => r.IsService(serviceType)))
            return true;

        if (_descriptors.Any(d => d.ServiceType == serviceType))
            return true;

        if (serviceType.IsGenericType)
        {
            var genericDef = serviceType.GetGenericTypeDefinition();
            if (_descriptors.Any(d => d.ServiceType == genericDef))
                return true;
        }

        for (var i = 0; i < _fallbackProviders.Count; i++)
        {
            var instance = _fallbackProviders[i].GetService(serviceType);
            if (instance != null)
                return true;
        }

        return false;
    }

    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IServiceProvider))
            return this;

        if (serviceType == typeof(IServiceProviderIsService))
            return this;

        if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var itemType = serviceType.GenericTypeArguments[0];
            var list = new List<object>();

            foreach (var resolver in _resolvers)
            {
                list.AddRange(resolver.ResolveAll(itemType));
            }

            foreach (var descriptor in _descriptors)
            {
                if (descriptor.ServiceType == itemType)
                {
                    var inst = ResolveDescriptor(descriptor);
                    if (inst != null)
                        list.Add(inst);
                }
            }

            var array = Array.CreateInstance(itemType, list.Count);

            for (var i = 0; i < list.Count; i++)
                array.SetValue(list[i], i);

            return array;
        }

        foreach (var resolver in _resolvers)
        {
            var instance = resolver.Resolve(serviceType);
            if (instance != null)
                return instance;
        }

        var serviceDescriptor = _descriptors.LastOrDefault(d => d.ServiceType == serviceType);
        if (serviceDescriptor != null)
            return ResolveDescriptor(serviceDescriptor);

        // Open generics (e.g. ILogger<> -> Logger<>)
        if (serviceType.IsGenericType)
        {
            var genericDef = serviceType.GetGenericTypeDefinition();
            var genericDesc = _descriptors.LastOrDefault(d => d.ServiceType == genericDef);
            if (genericDesc?.ImplementationType != null)
            {
                var closedType = genericDesc.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
                var instance = ActivatorUtilities.CreateInstance(this, closedType);

                if (genericDesc.Lifetime == ServiceLifetime.Singleton)
                {
                    instance = TrackDisposable(_dynamicSingletons.GetOrAdd(serviceType, _ => instance));
                }

                return instance;
            }
        }

        foreach (var provider in _fallbackProviders)
        {
            var instance = provider.GetService(serviceType);
            if (instance != null)
                return instance;
        }

        return null;
    }

    private object? ResolveDescriptor(ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationFactory != null)
        {
            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                return _dynamicSingletons.GetOrAdd(descriptor.ServiceType,
                    _ => TrackDisposable(descriptor.ImplementationFactory(this)));
            }

            return descriptor.ImplementationFactory(this);
        }

        if (descriptor.ImplementationType != null)
        {
            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                return _dynamicSingletons.GetOrAdd(descriptor.ServiceType,
                    _ => TrackDisposable(ActivatorUtilities.CreateInstance(this, descriptor.ImplementationType)));
            }

            return ActivatorUtilities.CreateInstance(this, descriptor.ImplementationType);
        }

        return null;
    }

    public async ValueTask DisposeAsync()
    {
        await _disposables.DisposeAsync().ConfigureAwait(false);

        for (var i = _resolvers.Count - 1; i >= 0; i--)
        {
            if (_resolvers[i] is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else if (_resolvers[i] is IDisposable disposable)
                disposable.Dispose();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
