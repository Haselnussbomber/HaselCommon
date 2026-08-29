namespace HaselCommon.Utils;

public static class DisposableBag
{
    public static IDisposable Empty => EmptyDisposable.Instance;

    public static void AddTo(this IDisposable disposable, DisposableBagBuilder disposableBag)
    {
        disposableBag.Add(disposable);
    }

    internal class EmptyDisposable : IDisposable
    {
        internal static readonly IDisposable Instance = new EmptyDisposable();
        public void Dispose() { }
    }

    public static IDisposable Create(IDisposable disposable1)
    {
        return new Disposable1(disposable1);
    }

    private sealed class Disposable1(IDisposable disposable1) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2)
    {
        return new Disposable2(disposable1, disposable2);
    }

    private sealed class Disposable2(IDisposable disposable1, IDisposable disposable2) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
    {
        return new Disposable3(disposable1, disposable2, disposable3);
    }

    private sealed class Disposable3(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
    {
        return new Disposable4(disposable1, disposable2, disposable3, disposable4);
    }

    private sealed class Disposable4(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
                disposable4.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5)
    {
        return new Disposable5(disposable1, disposable2, disposable3, disposable4, disposable5);
    }

    private sealed class Disposable5(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
                disposable4.Dispose();
                disposable5.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5, IDisposable disposable6)
    {
        return new Disposable6(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
    }

    private sealed class Disposable6(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
                disposable4.Dispose();
                disposable5.Dispose();
                disposable6.Dispose();
            }
        }
    }

    public static IDisposable Create(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7)
    {
        return new Disposable7(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7);
    }

    private sealed class Disposable7(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
                disposable4.Dispose();
                disposable5.Dispose();
                disposable6.Dispose();
                disposable7.Dispose();
            }
        }
    }

    public static IDisposable Create(params IDisposable[] disposables)
    {
        return new DisposablesArray(disposables);
    }

    private sealed class DisposablesArray(IDisposable[] disposables) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                foreach (var item in disposables)
                    item.Dispose();
            }
        }
    }
}
