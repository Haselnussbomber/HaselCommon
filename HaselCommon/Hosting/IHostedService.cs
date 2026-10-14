using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.Hosting;

//! https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/IHostedService.cs

public interface IHostedService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
