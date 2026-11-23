using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.Hosting;

// https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/IHost.cs

public sealed class Host(IServiceProvider services)
{
    public IServiceProvider Services => services;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            Services
                .GetServices<IHostedService>()
                .Select(service => service.StartAsync(cancellationToken)));
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            Services
                .GetServices<IHostedService>()
                .Select(service => service.StopAsync(cancellationToken)));
    }

    public ValueTask DisposeAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
            return asyncDisposable.DisposeAsync();
        else if (Services is IDisposable disposable)
            disposable.Dispose();

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
