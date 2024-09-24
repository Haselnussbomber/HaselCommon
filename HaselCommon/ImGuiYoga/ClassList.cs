using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HaselCommon.ImGuiYoga;

// https://dom.spec.whatwg.org/#interface-domtokenlist
public partial class ClassList(Node OwnerNode) : ICollection<string>
{
    [GeneratedRegex(@"\s")]
    private static partial Regex RegexWhitespace();

    private List<string>? Cache { get; set; }

    private List<string> GetList()
    {
        return Cache ??= [.. OwnerNode.Attributes["class"].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)];
    }

    public override string ToString()
    {
        return Count == 0 ? string.Empty : ("." + string.Join(".", GetList()));
    }

    internal void ClearCache()
    {
        Cache = null;
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

        OwnerNode.Attributes["class"] += " " + item;
    }

    public void Clear()
    {
        OwnerNode.Attributes["class"] = string.Empty;
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
            OwnerNode.Attributes["class"] = string.Join(' ', list);
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
