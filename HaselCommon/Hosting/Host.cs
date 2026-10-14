using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.Hosting;

// https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/IHost.cs

public sealed class Host(IServiceProvider services)
{
    public IServiceProvider Services => services;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            Services
                .GetServices<IHostedService>()
                .Select(service => service.StartAsync(cancellationToken)))
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            Services
                .GetServices<IHostedService>()
                .Select(service => service.StopAsync(cancellationToken)))
            .ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        switch (Services)
        {
            case IAsyncDisposable asyncDisposable:
                return asyncDisposable.DisposeAsync();
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }

        return default;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
