using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Utils.Globals;

public static unsafe class Atk
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* GetNode<T>(AtkUnitBase* addon, uint nodeId) where T : unmanaged
        => addon == null ? null : (T*)addon->UldManager.SearchNodeById(nodeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* GetNode<T>(AtkComponentBase* component, uint nodeId) where T : unmanaged
        => component == null ? null : (T*)component->UldManager.SearchNodeById(nodeId);

    public static Vector2 GetNodeScale(AtkResNode* node)
    {
        if (node == null)
            return Vector2.One;

        var scale = new Vector2(node->ScaleX, node->ScaleY);

        while (node->ParentNode != null)
        {
            node = node->ParentNode;
            scale *= new Vector2(node->ScaleX, node->ScaleY);
        }

        return scale;
    }
}
