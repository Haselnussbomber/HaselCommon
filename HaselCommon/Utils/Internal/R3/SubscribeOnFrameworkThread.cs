using System.Threading;
using R3;

namespace HaselCommon.Utils.Internal.R3;

internal sealed class SubscribeOnFrameworkThread<T>(Observable<T> source, IFramework framework) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> observer)
    {
        return new _SubscribeOn(observer, source, framework).Run();
    }

    private sealed class _SubscribeOn(Observer<T> observer, Observable<T> source, IFramework framework) : Observer<T>, IThreadPoolWorkItem
    {
        private SingleAssignmentDisposableCore _disposable;

        public IDisposable Run()
        {
            ThreadPool.UnsafeQueueUserWorkItem(this, preferLocal: false);
            return this;
        }

        public void Execute()
        {
            try
            {
                _disposable.Disposable = source.Subscribe(this);
            }
            catch (Exception ex)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
                Dispose();
            }
        }

        protected override void OnNextCore(T value)
        {
            framework.RunOnFrameworkThread(() => observer.OnNext(value));
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            framework.RunOnFrameworkThread(() => observer.OnErrorResume(error));
        }

        protected override void OnCompletedCore(Result result)
        {
            framework.RunOnFrameworkThread(() => observer.OnCompleted(result));
        }

        protected override void DisposeCore()
        {
            _disposable.Dispose();
        }
    }
}
