using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HaselCommon.ImGuiYoga.Events;

namespace HaselCommon.ImGuiYoga;

// https://dom.spec.whatwg.org/#namednodemap
public class AttributeMap(Node OwnerNode) : IDictionary<string, string?>
{
    private Dictionary<string, string?> Map { get; set; } = [];

    public string? this[string attrName]
    {
        get => Map.TryGetValue(attrName, out var value) ? value : null;
        set
        {
            if (this[attrName] != value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    Map.Remove(attrName);
                }
                else
                {
                    Map[attrName] = value;
                }

                OwnerNode.DispatchEvent(new AttributeChangedEvent()
                {
                    Name = attrName,
                    Value = value
                });

                switch (attrName)
                {
                    case "class":
                        OwnerNode.ClassList.ClearCache();
                        break;

                    case "style":
                        var stylesheet = NodeParser.StylesheetParser.Parse(".s{" + value + "}");
                        var rule = stylesheet.StyleRules.First();
                        OwnerNode.Style.Clear();
                        foreach (var property in rule.Style.Declarations)
                            OwnerNode.Style[property.Name] = property.Value;
                        OwnerNode.ComputedStyle.ResetCache();
                        return; // don't invalidate style
                }

                OwnerNode.GetDocument()?.SetStyleDirty();
            }
        }
    }

    public ICollection<string> Keys => ((IDictionary<string, string>)Map).Keys;
    public ICollection<string?> Values => ((IDictionary<string, string?>)Map).Values;
    public int Count => Map.Count;
    public bool IsReadOnly => false;

    public void Add(string key, string? value)
    {
        Map.Add(key, value);

        if (key == "class")
            OwnerNode.ClassList.ClearCache();
    }

    public bool ContainsKey(string key)
    {
        return Map.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        var success = Map.Remove(key);

        if (success && key == "class")
            OwnerNode.ClassList.ClearCache();

        return success;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return Map.TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<string, string?> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        if (Map.ContainsKey("class"))
            OwnerNode.ClassList.ClearCache();

        Map.Clear();
    }

    public bool Contains(KeyValuePair<string, string?> item)
    {
        return Map.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex)
    {
        Map.ToArray().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, string?> item)
    {
        return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
    {
        return Map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Map.GetEnumerator();
    }

    public override string ToString()
    {
        return string.Join(" ", Map.Select(kv => $"{kv.Key}=\"{kv.Value ?? string.Empty}\"")); // TODO: escape strings
    }
}
