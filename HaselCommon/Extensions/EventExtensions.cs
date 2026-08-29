namespace HaselCommon.Extensions;

public static class EventExtensions
{
    public static IDisposable Subscribe<TDelegate>(
        Action<TDelegate> addHandler,
        Action<TDelegate> removeHandler,
        TDelegate handler) where TDelegate : Delegate
    {
        addHandler(handler);
        return new DisposableAction(() => removeHandler(handler));
    }
}
