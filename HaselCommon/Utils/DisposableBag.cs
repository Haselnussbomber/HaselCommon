using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.Utils;

public class DisposableBag : IEnumerable, IDisposable, IAsyncDisposable
{
    private readonly List<object> _disposables = [];
    private readonly Lock _lock = new();

    public bool IsDisposed { get; private set; }

    public IEnumerator GetEnumerator()
    {
        try
        {
            _lock.Enter();

            foreach (var disposable in _disposables)
            {
                yield return disposable;
            }
        }
        finally
        {
            _lock.Exit();
        }
    }

    public void Reset()
    {
        using (_lock.EnterScope())
        {
            Dispose();
            IsDisposed = false;
        }
    }

    public void Add(IDisposable disposable)
    {
        using (_lock.EnterScope())
        {
            if (!_disposables.Contains(disposable))
            {
                _disposables.Add(disposable);
            }
        }
    }

    public void Add(IAsyncDisposable disposable)
    {
        using (_lock.EnterScope())
        {
            if (!_disposables.Contains(disposable))
            {
                _disposables.Add(disposable);
            }
        }
    }

    public void AddRange(params Span<IDisposable> disposables)
    {
        using (_lock.EnterScope())
        {
            foreach (var disposable in disposables)
            {
                if (!_disposables.Contains(disposable))
                {
                    _disposables.Add(disposable);
                }
            }
        }
    }

    public void AddRange(params Span<IAsyncDisposable> disposables)
    {
        using (_lock.EnterScope())
        {
            foreach (var disposable in disposables)
            {
                if (!_disposables.Contains(disposable))
                {
                    _disposables.Add(disposable);
                }
            }
        }
    }

    public void AddRange(params Span<object> disposables)
    {
        using (_lock.EnterScope())
        {
            foreach (var disposable in disposables)
            {
                if (disposable is IDisposable or IAsyncDisposable && !_disposables.Contains(disposable))
                {
                    _disposables.Add(disposable);
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _lock.Enter();

            if (IsDisposed)
                return;

            for (var i = _disposables.Count - 1; i >= 0; i--)
            {
                if (_disposables[i] is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (_disposables[i] is IDisposable disposable)
                    disposable.Dispose();
            }

            IsDisposed = true;
            _disposables.Clear();
        }
        finally
        {
            _lock.Exit();
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}

public static class DisposableExtensions
{
    extension(IDisposable disposable)
    {
        public void AddTo(DisposableBag bag)
        {
            bag.Add(disposable);
        }
    }

    extension(IAsyncDisposable disposable)
    {
        public void AddTo(DisposableBag bag)
        {
            bag.Add(disposable);
        }
    }
}
