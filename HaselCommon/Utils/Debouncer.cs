using System.Threading;

namespace HaselCommon.Utils;

public class Debouncer(IFramework framework, TimeSpan delay, Action action) : IDisposable
{
    private CancellationTokenSource? _cts;

    public void Debounce()
    {
        var oldCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());

        oldCts?.Cancel();
        oldCts?.Dispose();

        framework.RunOnTick(action, delay, cancellationToken: _cts.Token);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
