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
            return framework.RunOnTick(() => host.StartAsync(cancellationToken), cancellationToken: cancellationToken);
        }

        public ValueTask StopOnFrameworkThread(IFramework framework)
        {
            Task stopTask;
            try
            {
                stopTask = framework.RunOnTick(() => host.StopAsync());
            }
            catch
            {
                host.Dispose();
                throw;
            }

            if (stopTask.IsCompleted) // fast path when IsFrameworkUnloading is true
            {
                host.Dispose();
                return new ValueTask(stopTask);
            }

            return new ValueTask(SlowPathAsync(stopTask, host));
            static async Task SlowPathAsync(Task task, IDisposable host)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                finally
                {
                    host.Dispose();
                }
            }
        }
    }
}
