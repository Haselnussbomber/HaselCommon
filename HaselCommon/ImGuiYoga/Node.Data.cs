namespace HaselCommon.ImGuiYoga;

/*
public unsafe partial class Node
{
    public Dictionary<string, object?> Data { get; set; } = [];

    public Node? FindNodeByData<TValue>(string key, TValue value) where TValue : IEquatable<TValue>
        => FindNodeByData<Node, TValue>(key, value);

    public TNode? FindNodeByData<TNode, TValue>(string key, TValue value)
        where TNode : Node
        where TValue : IEquatable<TValue>
    {
        if (Data.TryGetValue(key, out var dataValue) && dataValue is TValue typedDataValue && typedDataValue.Equals(value) && typeof(TNode) == CachedType)
            return (TNode)this;

        foreach (var child in this)
        {
            if (child.Data.TryGetValue(key, out dataValue) && dataValue is TValue _typedDataValue && _typedDataValue.Equals(value) && typeof(TNode) == child.CachedType)
                return (TNode)child;

            var childNode = child.GetNodeById<TNode>(key);
            if (childNode != null)
                return childNode;
        }

        return null;
    }
}
*/
