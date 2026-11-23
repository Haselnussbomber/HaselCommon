using System.Buffers;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace HaselCommon.Utils;

public sealed class DisposableBag : IEnumerable, IDisposable, IAsyncDisposable
{
    private readonly List<object> _disposables = [];
    private readonly Lock _lock = new();

    public bool IsDisposed { get; private set; }

    public IEnumerator GetEnumerator()
    {
        object[] rented;
        int count;

        using (_lock.EnterScope())
        {
            count = _disposables.Count;
            if (count == 0)
                return Array.Empty<object>().GetEnumerator();

            rented = ArrayPool<object>.Shared.Rent(count);
            _disposables.CopyTo(rented, 0);
        }

        return EnumerateRented(rented, count);

        static IEnumerator EnumerateRented(object[] buffer, int length)
        {
            try
            {
                for (var i = 0; i < length; i++)
                    yield return buffer[i];
            }
            finally
            {
                ArrayPool<object>.Shared.Return(buffer, clearArray: true);
            }
        }
    }

    public void Reset()
    {
        using (_lock.EnterScope())
        {
            DisposeCoreSync();
            IsDisposed = false;
        }
    }

    public void Add(IDisposable? disposable)
    {
        AddCore(disposable);
    }

    public void Add(IAsyncDisposable? disposable)
    {
        AddCore(disposable);
    }

    public void AddRange(params ReadOnlySpan<IDisposable?> disposables)
    {
        foreach (var item in disposables)
            AddCore(item);
    }

    public void AddRange(params ReadOnlySpan<IAsyncDisposable?> disposables)
    {
        foreach (var item in disposables)
            AddCore(item);
    }

    public void AddRange(params ReadOnlySpan<object?> disposables)
    {
        foreach (var item in disposables)
            AddCore(item);
    }

    private void AddCore(object? item)
    {
        if (item is not (IDisposable or IAsyncDisposable))
            return;

        using (_lock.EnterScope())
        {
            if (IsDisposed)
                return;

            if (!_disposables.Contains(item))
                _disposables.Add(item);
        }
    }

    public async ValueTask DisposeAsync()
    {
        object[]? rented = null;
        var count = 0;

        using (_lock.EnterScope())
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            count = _disposables.Count;

            if (count > 0)
            {
                rented = ArrayPool<object>.Shared.Rent(count);
                _disposables.CopyTo(rented, 0);
                _disposables.Clear();
            }
        }

        if (rented is null)
            return;

        try
        {
            for (var i = count - 1; i >= 0; i--)
            {
                if (rented[i] is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                else if (rented[i] is IDisposable disposable)
                    disposable.Dispose();
            }
        }
        finally
        {
            ArrayPool<object>.Shared.Return(rented, clearArray: true);
        }
    }

    public void Dispose()
    {
        using (_lock.EnterScope())
        {
            if (IsDisposed)
                return;

            DisposeCoreSync();
            IsDisposed = true;
        }
    }

    private void DisposeCoreSync()
    {
        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            if (_disposables[i] is IDisposable disposable)
                disposable.Dispose();
            else if (_disposables[i] is IAsyncDisposable asyncDisposable)
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        _disposables.Clear();
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
