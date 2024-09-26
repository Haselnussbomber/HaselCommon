using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YogaSharp;

namespace HaselCommon.ImGuiYoga;

// TODO: Clone(bool deep = false)

public unsafe partial class Node : IList<Node>
{
    private List<Node> Children { get; } = [];

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

    public void UpdateChildNodes()
    {
        foreach (var child in this)
        {
            child.Update();
        }
    }

    public void DrawChildNodes()
    {
        foreach (var child in this)
        {
            if (child.Display != YGDisplay.None)
            {
                child.Draw();
            }
        }
    }

    public Node this[int index]
    {
        get => Children[index];
        set => Children[index] = value;
    }

    public int Count => Children.Count;

    public bool IsReadOnly => false;

    public void Add(Node child)
    {
        ThrowIfDisposed();

        child.Parent = this;
        YGNode->InsertChild(child.YGNode, Count);
        Children.Add(child);
    }

    public void Clear()
    {
        ThrowIfDisposed();

        for (var i = Count - 1; i >= 0; i--)
        {
            Remove(this[i]);
        }
    }

    public bool Contains(Node item)
    {
        ThrowIfDisposed();

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
        ThrowIfDisposed();

        for (var i = 0; i < array.Length; i++)
        {
            array[i].Parent = this;
            YGNode->InsertChild(array[i].YGNode, arrayIndex + i);
        }

        Children.InsertRange(arrayIndex, array);
    }

    public IEnumerator<Node> GetEnumerator()
    {
        ThrowIfDisposed();

        return new YogaNodeEnumerator<Node>(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(Node child)
    {
        ThrowIfDisposed();

        for (var i = Count - 1; i >= 0; i--)
        {
            if (this[i] == child)
                return i;
        }

        return -1;
    }

    public void Insert(int index, Node child)
    {
        ThrowIfDisposed();

        child.Parent = this;
        YGNode->InsertChild(child.YGNode, index);
        Children.Insert(index, child);
    }

    public bool Remove(Node child)
    {
        ThrowIfDisposed();

        if (Contains(child))
        {
            child.Parent = null;
            YGNode->RemoveChild(child.YGNode);
            Children.Remove(child);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        ThrowIfDisposed();

        this[index].Parent = null;
        YGNode->RemoveChild(this[index].YGNode);
        Children.RemoveAt(index);
    }

    public class YogaNodeEnumerator<T>(Node node) : IEnumerator<T> where T : Node
    {
        private int Index = -1;

        public bool MoveNext()
        {
            node.ThrowIfDisposed();

            Index++;
            return Index < node.Count;
        }

        public void Reset()
        {
            Index = -1;
        }

        public void Dispose() { }

        object IEnumerator.Current => node[Index];
        T IEnumerator<T>.Current => (T)node[Index];
    }

    public Document? GetDocument()
    {
        if (this is Document doc)
            return doc;

        if (Parent == null)
            return null;

        return Parent.GetDocument();
    }
}
