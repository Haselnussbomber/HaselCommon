using System.Threading;

namespace HaselCommon.Utils;

public class Debouncer(IFramework framework, TimeSpan delay, Action action) : IDisposable
{
    private CancellationTokenSource? _cts;

    public void Debounce()
    {
        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _cts, newCts);

        oldCts?.Cancel();
        oldCts?.Dispose();

        framework.RunOnTick(action, delay, cancellationToken: newCts.Token);
    }

    public void Dispose()
    {
        var oldCts = Interlocked.Exchange(ref _cts, null);

        oldCts?.Cancel();
        oldCts?.Dispose();
    }
}
