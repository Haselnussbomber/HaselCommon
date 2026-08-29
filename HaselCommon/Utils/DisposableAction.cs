namespace HaselCommon.Utils;

public readonly struct DisposableAction(Action action) : IDisposable
{
    public void Dispose()
    {
        action();
    }
}
