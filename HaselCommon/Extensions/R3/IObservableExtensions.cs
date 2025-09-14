using HaselCommon.Utils.Internal.R3;
using R3;

namespace HaselCommon.Extensions;

public static class IObservableExtensions
{
    public static Observable<T> SubscribeOnFrameworkThread<T>(this Observable<T> source, IFramework framework)
    {
        return new SubscribeOnFrameworkThread<T>(source, framework);
    }

    public static ReadOnlyReactiveProperty<TValue> TrackProperty<TUnit, TValue>(this Observable<TUnit> observable, Func<TValue> getter)
    {
        return observable
            .Select(_ => getter())
            .DistinctUntilChanged()
            .ToReadOnlyReactiveProperty(getter());
    }
}
