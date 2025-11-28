using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HaselCommon.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class ServiceLocator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    private static ServiceLocator? Instance { get; set; }

    public static T? GetService<T>() => TryGetService<T>(out var service) ? service : default;

    public static bool TryGetService<T>([NotNullWhen(returnValue: true)] out T? service)
    {
        if (Instance is not { } instance)
        {
            service = default;
            return false;
        }

        return instance._serviceProvider.TryGetService(out service);
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
