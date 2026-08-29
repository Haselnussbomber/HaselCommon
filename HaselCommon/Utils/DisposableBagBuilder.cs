namespace HaselCommon.Utils;

public class DisposableBagBuilder
{
    private readonly List<IDisposable> _disposables;

    public DisposableBagBuilder()
    {
        _disposables = [];
    }

    public DisposableBagBuilder(int capacity)
    {
        _disposables = new(capacity);
    }

    public DisposableBagBuilder Add(IDisposable disposable)
    {
        _disposables.Add(disposable);
        return this;
    }

    public DisposableBagBuilder Add(params Span<IDisposable> disposables)
    {
        _disposables.AddRange(disposables);
        return this;
    }

    public void Clear()
    {
        _disposables.Clear();
    }

    public IDisposable Build()
    {
        return _disposables.Count switch
        {
            0 => DisposableBag.Empty,
            1 => DisposableBag.Create(_disposables[0]),
            2 => DisposableBag.Create(_disposables[0], _disposables[1]),
            3 => DisposableBag.Create(_disposables[0], _disposables[1], _disposables[2]),
            4 => DisposableBag.Create(_disposables[0], _disposables[1], _disposables[2], _disposables[3]),
            5 => DisposableBag.Create(_disposables[0], _disposables[1], _disposables[2], _disposables[3], _disposables[4]),
            6 => DisposableBag.Create(_disposables[0], _disposables[1], _disposables[2], _disposables[3], _disposables[4], _disposables[5]),
            7 => DisposableBag.Create(_disposables[0], _disposables[1], _disposables[2], _disposables[3], _disposables[4], _disposables[5], _disposables[6]),
            _ => DisposableBag.Create([.. _disposables]),
        };
    }
}
