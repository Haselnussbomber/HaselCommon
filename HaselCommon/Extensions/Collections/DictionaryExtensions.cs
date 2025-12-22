namespace HaselCommon.Extensions;

public static class DictionaryExtensions
{
    extension<K,V>(IDictionary<K, V> dict)
    {
        public V GetOrCreate(K key, Func<V> createFn)
        {
            if (!dict.TryGetValue(key, out var val))
                dict.TryAdd(key, val = createFn());

            return val;
        }

        //! https://www.codeproject.com/Tips/494499/Implementing-Dictionary-RemoveAll
        public bool RemoveAll(Func<K, V, bool> match, bool dispose = false)
        {
            var anyRemoved = false;

            foreach (var key in dict.Keys.ToArray())
            {
                if (!dict.TryGetValue(key, out var value) || !match(key, value))
                    continue;

                if (dispose && value is IDisposable disposable)
                    disposable.Dispose();

                anyRemoved |= dict.Remove(key);
            }

            return anyRemoved;
        }

        public void Dispose()
        {
            dict.Values.OfType<IDisposable>().ForEach(disposable => disposable.Dispose());
            dict.Clear();
        }
    }
}
