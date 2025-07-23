using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HaselCommon.Services;

[RegisterSingleton<IHostLifetime>(Duplicate = DuplicateStrategy.Replace)]
public class NoopLifetime : IHostLifetime
{
    public Task WaitForStartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
