using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HaselCommon.Gui.Enums;

namespace HaselCommon.Gui;

public partial class Node : IList<Node>
{
    private readonly List<Node> _children = [];

    public Node? Parent { get; internal set; }

    public List<Node> Children
    {
        get => _children;
        set
        {
            Clear();
            foreach (var child in value)
                Add(child);
        }
    }

    public Node? FirstChild => Count > 0 ? this[0] : null;
    public Node? LastChild => Count > 0 ? this[Count - 1] : null;

    public Node? PreviousSibling
    {
        get
        {
            if (Parent == null || Parent.Count == 1)
                return null;

            var index = Parent.IndexOf(this);
            if (index <= 0)
                return null;

            return Parent[index - 1];
        }
    }

    public IReadOnlyList<Node> PreviousSiblings
    {
        get
        {
            if (Parent == null || Parent.Count == 1)
                return [];

            var index = Parent.IndexOf(this);
            if (index <= 0)
                return [];

            return Parent.Take(index).ToArray();
        }
    }

    public Node? NextSibling
    {
        get
        {
            if (Parent == null || Parent.Count == 1)
                return null;

            var index = Parent.IndexOf(this);
            if (index == -1 || index + 1 >= Parent.Count)
                return null;

            return Parent[index + 1];
        }
    }

    public IReadOnlyList<Node> NextSiblings
    {
        get
        {
            if (Parent == null || Parent.Count == 1)
                return [];

            var index = Parent.IndexOf(this);
            if (index == -1 || index + 1 >= Parent.Count)
                return [];

            return Parent.Skip(index + 1).ToArray();
        }
    }

    public Node this[int index]
    {
        get => Children[index];
        set
        {
            RemoveAt(index);
            Insert(index, value);
        }
    }

    /// <summary>
    /// The number of child nodes.
    /// </summary>
    public int Count => Children.Count;

    public bool IsReadOnly => false;

    public void Add(Node child)
    {
        if (child.Parent != null)
            throw new Exception("Child already has a owner, it must be removed first.");

        if (HasMeasureFunc)
            throw new Exception("Cannot add child: Nodes with measure functions cannot have children.");

        Children.Add(child);
        child.Parent = this;
        MarkDirtyAndPropagate();
    }

    public void Add(params Node[] children)
    {
        foreach (var child in children)
            Add(child);
    }

    /// <summary>
    /// Removes all children nodes.
    /// </summary>
    public void Clear()
    {
        for (var index = Count - 1; index >= 0; index--)
        {
            Remove(this[index]);
        }
    }

    public bool Contains(Node item)
    {
        for (var i = 0; i < Count; i++)
        {
            if (this[i] == item)
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(Node[] array, int arrayIndex)
    {
        if (array.Length == 0)
            return;

        for (var i = 0; i < array.Length; i++)
            Insert(arrayIndex + i, array[i]);
    }

    public IEnumerator<Node> GetEnumerator() => Children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(Node child)
    {
        for (var i = Count - 1; i >= 0; i--)
        {
            if (this[i] == child)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Inserts a child node at the given index.
    /// </summary>
    public void Insert(int index, Node child)
    {
        if (child.Parent != null)
            throw new Exception("Child already has a owner, it must be removed first.");

        if (HasMeasureFunc)
            throw new Exception("Cannot add child: Nodes with measure functions cannot have children.");

        Children.Insert(index, child);
        child.Parent = this;
        MarkDirtyAndPropagate();
    }

    /// <summary>
    /// Removes the given child node.
    /// </summary>
    public bool Remove(Node child)
    {
        if (!Contains(child))
            return false;

        if (!Children.Remove(child))
            return false;

        // Children may be shared between parents, which is indicated by not having an
        // owner. We only want to reset the child completely if it is owned
        // exclusively by one node.
        if (child.Parent == this)
        {
            child._layout = new(); // layout is no longer valid
            child.Parent = null;
        }

        MarkDirtyAndPropagate();
        return true;
    }

    public void RemoveAt(int index)
    {
        if (index > -1 && index < Count)
            Remove(this[index]);
    }

    public Vector2 AbsolutePosition
    {
        get
        {
            var position = new Vector2(ComputedLeft, ComputedTop);

            if (Parent != null && Parent.Overflow != Overflow.Scroll)
                position += Parent.AbsolutePosition;

            return position;
        }
    }
}
