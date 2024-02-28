using System.Runtime.InteropServices;

namespace HaselCommon.Yoga;

public static unsafe partial class Interop
{
    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetLeft(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetTop(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetRight(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetBottom(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetWidth(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetHeight(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern Direction YGNodeLayoutGetDirection(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool YGNodeLayoutGetHadOverflow(YGNode* node);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetMargin(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetBorder(YGNode* node, Edge edge);

    [DllImport(YogaDll, CallingConvention = CallingConvention.Cdecl)]
    public static extern float YGNodeLayoutGetPadding(YGNode* node, Edge edge);
}
