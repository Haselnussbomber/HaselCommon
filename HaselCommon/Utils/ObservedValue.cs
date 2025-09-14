using R3;

namespace HaselCommon.Utils;

public class ObservedValue<T> : ReactiveProperty<T>
{
    private readonly Func<T> _getter;
    private readonly IDisposable _subscription;

    public ObservedValue(Func<T> getter)
    {
        _getter = getter;
        _subscription = Observable.EveryUpdate()
            .Subscribe(_ => Value = _getter());
    }

    public override void Dispose()
    {
        _subscription.Dispose();
        base.Dispose();
    }
}
