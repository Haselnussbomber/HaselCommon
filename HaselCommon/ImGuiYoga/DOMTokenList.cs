using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HaselCommon.ImGuiYoga;

// https://dom.spec.whatwg.org/#interface-domtokenlist
public partial class DOMTokenList(Node OwnerNode, string AttributeName) : ICollection<string>
{
    [GeneratedRegex(@"\s")]
    private static partial Regex RegexWhitespace();

    private List<string>? Cache { get; set; }

    private List<string> GetList()
        => Cache ??= [.. OwnerNode.Attributes[AttributeName].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];

    internal void ClearCache()
    {
        Cache = null;
        OwnerNode.GetDocument()?.SetStyleDirty();
    }

    public int IndexOf(string item)
    {
        return GetList().IndexOf(item);
    }

    #region ICollection

    public int Count => GetList().Count;
    public bool IsReadOnly => false;

    public void Add(string item)
    {
        if (string.IsNullOrEmpty(item))
            throw new ArgumentNullException(nameof(item), "Item is null or empty.");

        if (RegexWhitespace().IsMatch(item))
            throw new ArgumentException("Item may not contain whitespaces.", nameof(item));

        OwnerNode.Attributes[AttributeName] += " " + item;
        ClearCache();
    }

    public void Clear()
    {
        OwnerNode.Attributes[AttributeName] = string.Empty;
        ClearCache();
    }

    public bool Contains(string item)
    {
        return GetList().Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<string> GetEnumerator()
    {
        return GetList().GetEnumerator();
    }

    public bool Remove(string item)
    {
        var list = GetList();
        if (list.Remove(item))
        {
            OwnerNode.Attributes[AttributeName] = string.Join(' ', list);
            ClearCache();
            return true;
        }
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetList().GetEnumerator();
    }

    public void AddRange(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    #endregion
}
