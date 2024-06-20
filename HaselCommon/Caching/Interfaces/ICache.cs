namespace HaselCommon.Caching.Interfaces;

public interface ICache<TKey, TValue> : IDisposable where TKey : notnull, IEquatable<TKey>
{
    TValue? GetValue(TKey key);
    bool TryGetValue(TKey key, out TValue? value);
    TValue? CreateEntry(TKey key);
    bool Remove(TKey key);
    void Clear();
}
