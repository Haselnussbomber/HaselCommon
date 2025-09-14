using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using R3;

namespace HaselCommon.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class EventSystem : IHostedService
{
    private readonly ILogger<EventSystem> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ObservableSystem.RegisterServiceProvider(_serviceProvider);
        ObservableSystem.RegisterUnhandledExceptionHandler(OnUnhandledException);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnUnhandledException(Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred in an observable.");
    }
}
