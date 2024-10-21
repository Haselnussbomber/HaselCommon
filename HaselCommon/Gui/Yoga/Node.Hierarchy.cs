using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HaselCommon.Gui.Yoga.Events;
using YogaSharp;

namespace HaselCommon.Gui.Yoga;

public unsafe partial class Node : IList<Node>
{
    private readonly List<Node> _children = [];

    public Node? Parent { get; set; }

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
        set
        {
            Children[index] = value;
            DispatchEvent(new ChildrenChangedEvent());
        }
    }

    public int Count => Children.Count;

    public bool IsReadOnly => false;

    public void Add(Node child)
    {
        ThrowIfDisposed();

        if (child.Parent != null)
            throw new Exception("Child already has a owner, it must be removed first.");

        if (HasMeasureFunc)
            throw new Exception("Cannot add child: Nodes with measure functions cannot have children.");

        child.Parent = this;
        _yogaNode->InsertChild(child._yogaNode, Count);
        Children.Add(child);
        DispatchEvent(new ChildrenChangedEvent());
    }

    public void Add(params Node[] children)
    {
        foreach (var child in children)
            Add(child);
    }

    public void Clear() => Clear(false);

    public void Clear(bool dispose)
    {
        ThrowIfDisposed();

        for (var i = Count - 1; i >= 0; i--)
        {
            var node = this[i];

            Remove(node);

            if (dispose)
                node.Dispose();
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
        if (array.Length == 0)
            return;

        ThrowIfDisposed();

        for (var i = 0; i < array.Length; i++)
        {
            array[i].Parent = this;
            _yogaNode->InsertChild(array[i]._yogaNode, arrayIndex + i);
        }

        Children.InsertRange(arrayIndex, array);
        DispatchEvent(new ChildrenChangedEvent());
    }

    public void Traverse(Action<Node> callback)
    {
        foreach (var child in Children)
        {
            callback(child);
            child.Traverse(callback);
        }
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

        if (child.Parent != null)
            throw new Exception("Child already has a owner, it must be removed first.");

        if (HasMeasureFunc)
            throw new Exception("Cannot add child: Nodes with measure functions cannot have children.");

        child.Parent = this;
        _yogaNode->InsertChild(child._yogaNode, index);
        Children.Insert(index, child);
        DispatchEvent(new ChildrenChangedEvent());
    }

    public bool Remove(Node child)
    {
        ThrowIfDisposed();

        if (!Contains(child))
            return false;

        if (!Children.Remove(child))
            return false;

        child.Parent = null;
        _yogaNode->RemoveChild(child._yogaNode);
        Children.Remove(child);
        DispatchEvent(new ChildrenChangedEvent());
        return true;
    }

    public void RemoveAt(int index)
    {
        ThrowIfDisposed();

        this[index].Parent = null;
        _yogaNode->RemoveChild(this[index]._yogaNode);
        Children.RemoveAt(index);
        DispatchEvent(new ChildrenChangedEvent());
    }

    public class YogaNodeEnumerator<T>(Node node) : IEnumerator<T> where T : Node
    {
        private int _index = -1;

        public bool MoveNext()
        {
            node.ThrowIfDisposed();

            _index++;
            return _index < node.Count;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() { }

        object IEnumerator.Current => node[_index];
        T IEnumerator<T>.Current => (T)node[_index];
    }
}
