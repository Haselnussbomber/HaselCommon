using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HaselCommon.ImGuiYoga;

// https://dom.spec.whatwg.org/#namednodemap
public class StyleMap(Node OwnerNode) : IDictionary<string, string>
{
    private Dictionary<string, string> Map { get; set; } = [];

    public string this[string propertyName]
    {
        get => Map.TryGetValue(propertyName, out var value) ? value : string.Empty;
        set
        {
            if (this[propertyName] != value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    Map.Remove(propertyName);
                }
                else
                {
                    Map[propertyName] = value;
                }

                OwnerNode.ComputedStyle.UpdateStyle(propertyName);
            }
        }
    }

    public ICollection<string> Keys => ((IDictionary<string, string>)Map).Keys;
    public ICollection<string> Values => ((IDictionary<string, string>)Map).Values;
    public int Count => Map.Count;
    public bool IsReadOnly => false;

    public void Add(string key, string value)
    {
        if (Map.TryAdd(key, value))
            OwnerNode.ComputedStyle.ResetCache();
    }

    public bool ContainsKey(string key)
    {
        return Map.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        var success = Map.Remove(key);

        if (success)
            OwnerNode.ComputedStyle.ResetCache();

        return success;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return Map.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<string, string> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Map.Clear();
        OwnerNode.ComputedStyle.ResetCache();
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
        return Map.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        Map.ToArray().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, string> item)
    {
        return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return Map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Map.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Join("\n", Map.Select(kv => $"{kv.Key}: {kv.Value};"));
    }
}
