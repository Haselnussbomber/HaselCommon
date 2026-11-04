using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.DependencyInjection;

public class ServiceProvider : IServiceProvider, IServiceProviderIsService, IDisposable, IAsyncDisposable
{
    private readonly IReadOnlyList<ServiceDescriptor> _descriptors;
    private readonly IReadOnlyList<IServiceResolver> _resolvers;
    private readonly IReadOnlyList<IServiceProvider> _fallbackProviders;
    private readonly ConcurrentDictionary<Type, object> _dynamicSingletons = new();
    private readonly List<object> _disposables = [];
    private readonly Lock _disposablesLock = new();

    public ServiceProvider(ServiceCollection services)
    {
        _descriptors = [.. services];
        _fallbackProviders = [.. services.FallbackProviders];
        _resolvers = services.ResolverFactories.ConvertAll(f => f(this));
    }

    private T TrackDisposable<T>(T instance)
    {
        if (instance is IDisposable or IAsyncDisposable)
        {
            using (_disposablesLock.EnterScope())
            {
                if (!_disposables.Contains(instance))
                    _disposables.Add(instance);
            }
        }

        return instance;
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
            var fallback = _fallbackProviders[i].GetService(serviceType);
            if (fallback != null)
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
            var match = resolver.Resolve(serviceType);
            if (match != null)
                return match;
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
            var fallback = provider.GetService(serviceType);
            if (fallback != null)
                return fallback;
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
        List<object> disposables;

        using (_disposablesLock.EnterScope())
            disposables = [.. _disposables];

        for (var i = disposables.Count - 1; i >= 0; i--)
        {
            if (disposables[i] is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            else if (disposables[i] is IDisposable disposable)
                disposable.Dispose();
        }

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
