using System.Collections.Generic;
using HaselCommon.Extensions;
using HaselCommon.Interfaces;

namespace HaselCommon.Services;

public class CacheManager()
{
    private readonly HashSet<ICache> Caches = [];

    public T Get<T>() where T : ICache, new()
    {
        foreach (var cache in Caches)
        {
            if (cache is T typedCache)
                return typedCache;
        }

        var newcache = new T();
        Caches.Add(newcache);
        return newcache;
    }

    public bool Add(ICache cache)
        => Caches.Add(cache);

    public bool Add<T>() where T : ICache, new()
        => Caches.Add(new T());

    public bool Remove(ICache cache)
        => Caches.Remove(cache);

    public bool Remove<T>() where T : ICache, new()
    {
        foreach (var cache in Caches)
        {
            if (cache is T typedCache)
                return Caches.Remove(typedCache);
        }

        return false;
    }

    public void Preload<T>() where T : IPreloadableCache, new()
        => Get<T>().Preload();

    internal void ClearLocalizedCaches()
    {
        Caches.ForEach(cache =>
        {
            if (cache is ILocalizedCache localizedCache)
                localizedCache.Clear();
        });
    }
}
