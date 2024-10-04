using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Globals;

// Source:
// https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Debugging/UIDebug.cs
// https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/UiHelper.cs

public static unsafe class Atk
{
    public static T* GetNode<T>(AtkUnitBase* addon, uint nodeId) where T : unmanaged
        => addon == null ? null : (T*)addon->UldManager.SearchNodeById(nodeId);

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

    public static void SetSize(AtkResNode* node, int? width, int? height)
    {
        if (width != null && width >= ushort.MinValue && width <= ushort.MaxValue)
            node->Width = (ushort)width.Value;

        if (height != null && height >= ushort.MinValue && height <= ushort.MaxValue)
            node->Height = (ushort)height.Value;

        node->DrawFlags |= 0x1;
    }

    public static void Scale(AtkResNode* node, float scale)
        => SetSize(node, (int)(node->Width * scale), (int)(node->Height * scale));

    public static void SetPosition(AtkResNode* node, float? x, float? y)
    {
        if (x != null)
            node->X = x.Value;

        if (y != null)
            node->Y = y.Value;

        node->DrawFlags |= 0x1;
    }

    public static void SetWindowSize(AtkComponentNode* windowNode, ushort? width, ushort? height)
    {
        if (((AtkUldComponentInfo*)windowNode->Component->UldManager.Objects)->ComponentType != ComponentType.Window)
            return;

        width ??= windowNode->AtkResNode.Width;
        height ??= windowNode->AtkResNode.Height;

        if (width < 64)
            width = 64;

        if (height < 16)
            height = 16;

        // Window
        SetSize((AtkResNode*)windowNode, width, height);

        var node = windowNode->Component->UldManager.RootNode;

        // Collision
        SetSize(node, width, height);

        // Header Collision
        node = node->PrevSiblingNode;
        SetSize(node, (ushort)(width - 14), null);

        // Background
        node = node->PrevSiblingNode;
        SetSize(node, width, height);

        // Focused Border
        node = node->PrevSiblingNode;
        SetSize(node, width, height);

        // Gradient
        node = node->PrevSiblingNode;
        if (RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType == 3)
            SetSize(node, width - 8, height - 16);
        else
            SetSize(node, width, height);

        // Header Node
        node = node->PrevSiblingNode;
        SetSize(node, (ushort)(width - 5), null);

        // Header Seperator
        node = node->ChildNode;
        SetSize(node, (ushort)(width - 20), null);

        // Close Button
        node = node->PrevSiblingNode;
        SetPosition(node, width - 33, 6);

        // Gear Button
        node = node->PrevSiblingNode;
        SetPosition(node, width - 47, 8);

        // Help Button
        node = node->PrevSiblingNode;
        SetPosition(node, width - 61, 8);

        windowNode->AtkResNode.DrawFlags |= 0x1;
    }
}
