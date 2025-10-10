using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public partial class ServiceLocator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    private static ServiceLocator? Instance { get; set; }

    /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
    public static T? GetService<T>() => Instance == null ? default : Instance._serviceProvider.GetService<T>();

    /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
    public static bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
    {
        if (Instance == null)
        {
            service = default;
            return false;
        }

        service = Instance._serviceProvider.GetService<T>();
        return service != null;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Instance = this;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Instance = null;
        return Task.CompletedTask;
    }
}
