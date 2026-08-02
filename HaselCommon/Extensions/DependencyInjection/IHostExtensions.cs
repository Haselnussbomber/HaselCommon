using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HaselCommon.Extensions;

public static class IHostExtensions
{
    extension(IHost host)
    {
        public Task StartOnFrameworkThread(IFramework framework, CancellationToken cancellationToken = default)
        {
            return framework.Run(() => host.StartAsync(cancellationToken), cancellationToken: cancellationToken);
        }

        public ValueTask StopOnFrameworkThread(IFramework framework)
        {
            return new(framework.Run(() => host.StopAsync()));
        }
    }
}
