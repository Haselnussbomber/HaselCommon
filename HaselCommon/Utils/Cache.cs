using System.Collections.Concurrent;
using HaselCommon.Interfaces;

namespace HaselCommon.Utils;

public abstract class Cache<TKey, TValue> : ICache where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue?> internalCache = [];

    public void Clear()
        => internalCache.Clear();

    public TValue? Get(TKey key)
        => internalCache.GetOrAdd(key, CreateValue);

    protected abstract TValue? CreateValue(TKey key);
}
