using System.Threading;

namespace HaselCommon.Globals;

public static class Disposable
{
    public static void DisposeAndNull<T>(ref T? disposable) where T : class, IDisposable
    {
        if (disposable != null)
            Interlocked.Exchange(ref disposable, null).Dispose();
    }

    public static void DisposeAndNull<T>(ref T? disposable) where T : struct, IDisposable
    {
        if (disposable.HasValue)
            Interlocked.Exchange(ref disposable, null).Value.Dispose();
    }
}
